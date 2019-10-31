using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using XamlBinding.Package;
using XamlBinding.Utility;
using Task = System.Threading.Tasks.Task;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// This hooks up the tool window's user interface with Visual Studio
    /// </summary>
    [Guid(Constants.BindingToolWindowString)]
    internal class BindingPane
        : ToolWindowPane
        , IVsWindowFrameNotify
        , IVsWindowFrameNotify2
        , IVsDebuggerEvents
    {
        private readonly Telemetry telemetry;
        private ITextBuffer debugTextBuffer;
        private ITrackingPoint lastTextPoint;
        private RegistryKey dataBindingOutputLevelKey;
        private IVsWindowFrame2 frameForCookie;
        private IVsDebugger debuggerForCookie;
        private uint frameCookie;
        private uint debugCookie;
        private __FRAMESHOW lastFrameShow;
        private readonly BindingEntryParser bindingParser;
        private readonly BindingPaneViewModel viewModel;
        private readonly CancellationTokenSource cancellationTokenSource;

        private BindingPackage BindingPackage => (BindingPackage)this.Package;
        private JoinableTaskFactory JoinableTaskFactory => this.BindingPackage.JoinableTaskFactory;
        private const string DataBindingTraceKey = @"Debugger\Tracing\WPF.DataBinding";

        public BindingPane(Telemetry telemetry)
            : base(null)
        {
            StringCache stringCache = new StringCache();

            this.telemetry = telemetry;
            this.bindingParser = new BindingEntryParser(stringCache);
            this.viewModel = new BindingPaneViewModel(telemetry, stringCache);
            this.cancellationTokenSource = new CancellationTokenSource();

            // Create the WPF user interface
            BindingPaneControl control = new BindingPaneControl(this.viewModel);
            this.Caption = AutomationProperties.GetName(control);
            this.Content = control;
        }

        /// <summary>
        /// Called after Visual Studio services are available, before the frame is created (this.Frame can be null)
        /// </summary>
        protected override void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.telemetry.TrackEvent(Constants.EventInitializePane);

            this.BindingPackage.DebugOutputTextViewCreated += this.OnTextViewCreated;

            // Look up the value of WPF trace settings in the registry
            using (RegistryKey rootKey = VSRegistry.RegistryRoot(this, __VsLocalRegistryType.RegType_UserSettings, writable: true))
            {
                this.dataBindingOutputLevelKey = rootKey.CreateSubKey(BindingPane.DataBindingTraceKey, writable: true);
            }

            // Need to know when debugging starts/stops
            if (this.GetService(typeof(IVsDebugger)) is IVsDebugger debugger)
            {
                this.debuggerForCookie = debugger;
                this.debuggerForCookie.AdviseDebuggerEvents(this, out this.debugCookie);

                DBGMODE[] dbgMode = new DBGMODE[1];
                if (ErrorHandler.Succeeded(debugger.GetMode(dbgMode)))
                {
                    this.viewModel.IsDebugging = dbgMode[0].HasFlag(DBGMODE.DBGMODE_Run) || dbgMode[0].HasFlag(DBGMODE.DBGMODE_Break);
                }
            }

            this.AttachToDebugOutput();
            this.FetchDataBindingTraceSetting();
            this.HookDataBindingTraceSettingChange();

            base.Initialize();
        }

        /// <summary>
        /// Called after this.Frame gets set
        /// </summary>
        public override void OnToolWindowCreated()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (this.Frame is IVsWindowFrame2 frame)
            {
                this.frameForCookie = frame;
                this.frameForCookie.Advise(this, out this.frameCookie);
            }

            base.OnToolWindowCreated();
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.BindingPackage.DebugOutputTextViewCreated -= this.OnTextViewCreated;

            if (this.debugCookie != 0)
            {
                this.debuggerForCookie.UnadviseDebuggerEvents(this.debugCookie);
                this.debuggerForCookie = null;
                this.debugCookie = 0;
            }

            if (this.frameCookie != 0)
            {
                this.frameForCookie.Unadvise(this.frameCookie);
                this.frameForCookie = null;
                this.frameCookie = 0;
            }

            if (this.debugTextBuffer != null)
            {
                this.debugTextBuffer.Changed -= this.OnTextBufferChanged;
            }

            this.dataBindingOutputLevelKey.Dispose();

            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource.Dispose();

            base.Dispose(disposing);
        }

        private void HookDataBindingTraceSettingChange()
        {
            CancellationToken cancelToken = this.cancellationTokenSource.Token;

            try
            {
                this.JoinableTaskFactory.RunAsync(async delegate
                {
                    if (this.dataBindingOutputLevelKey != null)
                    {
                        while (!cancelToken.IsCancellationRequested)
                        {
                            await this.dataBindingOutputLevelKey.WaitForChangeAsync(cancellationToken: cancelToken);
                            this.FetchDataBindingTraceSetting();
                        }
                    }
                });
            }
            catch (Exception ex) when (!(ex is TaskCanceledException))
            {
                this.telemetry.TrackException(ex);
            }
        }

        private void FetchDataBindingTraceSetting()
        {
            string value;
            try
            {
                value = this.dataBindingOutputLevelKey?.GetValue("Level") as string;
            }
            catch
            {
                value = null;
            }

            this.viewModel.TraceLevel = value;
        }

        /// <summary>
        /// Get the text buffer in the debug output window (if it exists)
        /// </summary>
        private void AttachToDebugOutput()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (this.debugTextBuffer == null)
            {
                IVsOutputWindow outputWindow = this.GetService<SVsOutputWindow, IVsOutputWindow>();
                Guid debugPaneId = VSConstants.GUID_OutWindowDebugPane;
                Guid viewId = DefGuidList.guidIWpfTextViewHost;

                if (ErrorHandler.Succeeded(outputWindow.GetPane(ref debugPaneId, out IVsOutputWindowPane pane)) && pane is IVsUserData userData &&
                    ErrorHandler.Succeeded(userData.GetData(ref viewId, out object viewHostObject)) && viewHostObject is IWpfTextViewHost viewHost)
                {
                    this.telemetry.TrackEvent(Constants.EventDebugOutputConnected);

                    this.debugTextBuffer = viewHost.TextView.TextBuffer;
                    this.debugTextBuffer.Changed += this.OnTextBufferChanged;
                    this.BeginProcessNewOutput();
                }
            }
        }

        private void OnTextViewCreated(ITextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Maybe the debug output text view was just created
            this.AttachToDebugOutput();
        }

        /// <summary>
        /// Called for all new debug output
        /// </summary>
        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.BeginProcessNewOutput();
        }

        private void BeginProcessNewOutput()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (this.debugTextBuffer?.CurrentSnapshot is ITextSnapshot snapshot)
            {
                ITrackingPoint startPoint = this.lastTextPoint ?? snapshot.CreateTrackingPoint(0, PointTrackingMode.Negative);
                ITrackingPoint endPoint = snapshot.CreateTrackingPoint(snapshot.Length, PointTrackingMode.Negative);
                this.lastTextPoint = endPoint;

                this.JoinableTaskFactory.RunAsync(async delegate
                {
                    await Task.Run(() => this.ProcessOutput(snapshot, startPoint, endPoint));
                });
            }
        }

        private void ProcessOutput(ITextSnapshot snapshot, ITrackingPoint startPoint, ITrackingPoint endPoint)
        {
            int textStart = startPoint.GetPosition(snapshot);
            int textLength = endPoint.GetPoint(snapshot) - startPoint.GetPoint(snapshot);
            string text = snapshot.GetText(textStart, textLength);
            BindingEntry[] entries = this.bindingParser.ParseOutput(text);

            if (entries.Length > 0)
            {
                this.JoinableTaskFactory.RunAsync(async delegate
                {
                    await this.JoinableTaskFactory.SwitchToMainThreadAsync();
                    this.viewModel.AddEntries(entries);
                });
            }
        }

        int IVsWindowFrameNotify.OnShow(int fShow)
        {
            __FRAMESHOW frameShow = (__FRAMESHOW)fShow;

            if (frameShow != this.lastFrameShow)
            {
                switch (frameShow)
                {
                    case __FRAMESHOW.FRAMESHOW_WinShown:
                        this.lastFrameShow = frameShow;
                        this.telemetry.TrackEvent(Constants.EventShowPane, this.viewModel.GetEntryTelemetryProperties());
                        break;

                    case __FRAMESHOW.FRAMESHOW_WinHidden:
                        this.lastFrameShow = frameShow;
                        this.telemetry.TrackEvent(Constants.EventHidePane, this.viewModel.GetEntryTelemetryProperties());
                        break;
                }
            }

            return 0;
        }

        int IVsWindowFrameNotify.OnMove() => 0;
        int IVsWindowFrameNotify.OnSize() => 0;
        int IVsWindowFrameNotify.OnDockableChange(int fDockable) => 0;

        int IVsWindowFrameNotify2.OnClose(ref uint pgrfSaveOptions)
        {
            this.telemetry.TrackEvent(Constants.EventClosePane, this.viewModel.GetEntryTelemetryProperties());
            return 0;
        }

        int IVsDebuggerEvents.OnModeChange(DBGMODE dbgmodeNew)
        {
            if (dbgmodeNew.HasFlag(DBGMODE.DBGMODE_Run))
            {
                this.telemetry.TrackEvent(Constants.EventDebugStart, this.viewModel.GetEntryTelemetryProperties());

                this.viewModel.IsDebugging = true;
                this.viewModel.ClearEntries();
            }
            else if (dbgmodeNew.HasFlag(DBGMODE.DBGMODE_Break))
            {
                this.viewModel.IsDebugging = true;
            }
            else
            {
                this.telemetry.TrackEvent(Constants.EventDebugEnd, this.viewModel.GetEntryTelemetryProperties());
                this.viewModel.IsDebugging = false;
            }

            return 0;
        }
    }
}
