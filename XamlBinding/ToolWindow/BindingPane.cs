using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using XamlBinding.Package;
using XamlBinding.Parser;
using XamlBinding.Resources;
using XamlBinding.Utility;
using Task = System.Threading.Tasks.Task;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// This hooks up the tool window's user interface with Visual Studio
    /// </summary>
    [Guid(Constants.BindingPaneString)]
    internal sealed class BindingPane
        : ToolWindowPane
        , IVsDebuggerEvents
        , IVsWindowFrameNotify
        , IVsWindowFrameNotify2
    {
        private readonly BindingPackage package;
        private readonly List<IOutputParser> outputParsers;
        private readonly BindingPaneViewModel viewModel;
        private readonly BindingPaneControl control;
        private readonly BindingPaneController controller;
        private readonly CancellationTokenSource cancellationTokenSource;

        private ITextBuffer debugTextBuffer;
        private ITrackingPoint lastTextPoint;
        private RegistryKey dataBindingOutputLevelKey;
        private __FRAMESHOW lastFrameShow;
        private IVsWindowFrame2 frameForCookie;
        private IVsDebugger debuggerForCookie;
        private uint frameCookie;
        private uint debugCookie;
        private DateTime lastTimeSoundPlayed;

        public BindingPane(BindingPackage package)
            : base(null)
        {
            this.package = package;
            this.outputParsers = new List<IOutputParser>();
            this.viewModel = new BindingPaneViewModel(package.Telemetry, this.outputParsers);
            this.control = new BindingPaneControl(this.package, this.viewModel);
            this.controller = new BindingPaneController(package, this.viewModel, this.control.TableControl);
            this.cancellationTokenSource = new CancellationTokenSource();

            this.Caption = Resource.ToolWindow_Title;
            this.Content = this.control;
            this.ToolBarCommandTarget = this.controller;
            this.ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
            this.ToolBar = new CommandID(Constants.GuidBindingPaneCommandSet, Constants.BindingPaneToolbarId);
        }

        /// <summary>
        /// Called after Visual Studio services are available, before the frame is created (this.Frame can be null)
        /// </summary>
        protected override void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.package.Telemetry.TrackEvent(Constants.EventInitializePane);

            // Look up the value of WPF trace settings in the registry
            using (RegistryKey rootKey = VSRegistry.RegistryRoot(this, __VsLocalRegistryType.RegType_UserSettings, writable: true))
            {
                this.dataBindingOutputLevelKey = rootKey.CreateSubKey(Constants.DataBindingTraceKey, writable: true);
            }

            this.GetOutputParsers();
            this.HookDataBindingTraceLevel();
            this.HookDebugEvents();
            this.WaitForDebugOutputTextBuffer();

            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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

            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource.Dispose();
            this.dataBindingOutputLevelKey.Dispose();
            this.viewModel.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Need to know when debugging starts/stops
        /// </summary>
        private void HookDebugEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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
        }

        /// <summary>
        /// The debug output text buffer may not be created yet, it will get created as needed while debugging.
        /// So, while debugging, this method will loop and keep checking until the text buffer is created.
        /// </summary>
        private void WaitForDebugOutputTextBuffer()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!this.AttachToDebugOutput())
            {
                CancellationToken cancelToken = this.cancellationTokenSource.Token;

                this.package.JoinableTaskFactory.RunAsync(async delegate
                {
                    while (!this.AttachToDebugOutput() && this.viewModel.IsDebugging && !cancelToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1.0), cancelToken);
                    }
                }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(this.WaitForDebugOutputTextBuffer));
            }
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

        /// <summary>
        /// Should be done on demand rather than during init
        /// </summary>
        private void GetOutputParsers()
        {
            IComponentModel componentModel = this.GetService<SComponentModel, IComponentModel>();
            foreach (IOutputParserProvider provider in componentModel.GetExtensions<IOutputParserProvider>())
            {
                try
                {
                    IOutputParser outputParser = provider.GetOutputParser();
                    if (outputParser != null)
                    {
                        this.outputParsers.Add(outputParser);
                    }
                }
                catch (Exception ex)
                {
                    this.package.Telemetry.TrackException(ex);
                }
            }
        }

        /// <summary>
        /// Wait "forever" for the trace registry value to change and update my cache when it changes
        /// </summary>
        private void HookDataBindingTraceLevel()
        {
            CancellationToken cancelToken = this.cancellationTokenSource.Token;

            this.FetchDataBindingTraceLevel();

            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                if (this.dataBindingOutputLevelKey != null)
                {
                    while (!cancelToken.IsCancellationRequested)
                    {
                        await this.dataBindingOutputLevelKey.WaitForChangeAsync(cancellationToken: cancelToken);
                        this.FetchDataBindingTraceLevel();
                    }
                }
            }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(this.HookDataBindingTraceLevel));
        }

        /// <summary>
        /// Update my cache of the trace level
        /// </summary>
        private void FetchDataBindingTraceLevel()
        {
            string value;
            try
            {
                value = this.dataBindingOutputLevelKey?.GetValue(Constants.DataBindingTraceLevel) as string;
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
        private bool AttachToDebugOutput()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (this.debugTextBuffer == null)
            {
                IVsOutputWindow outputWindow = this.GetService<SVsOutputWindow, IVsOutputWindow>();
                Guid debugPaneId = VSConstants.GUID_OutWindowDebugPane;
                Guid viewId = Microsoft.VisualStudio.Editor.DefGuidList.guidIWpfTextViewHost;

                if (ErrorHandler.Succeeded(outputWindow.GetPane(ref debugPaneId, out IVsOutputWindowPane pane)) && pane is IVsUserData userData &&
                    ErrorHandler.Succeeded(userData.GetData(ref viewId, out object viewHostObject)) && viewHostObject is IWpfTextViewHost viewHost)
                {
                    this.package.Telemetry.TrackEvent(Constants.EventDebugOutputConnected);

                    this.debugTextBuffer = viewHost.TextView.TextBuffer;
                    this.debugTextBuffer.Changed += this.OnTextBufferChanged;
                    this.BeginProcessOutput();
                }
            }

            return this.debugTextBuffer != null;
        }

        /// <summary>
        /// Called for all new debug output
        /// </summary>
        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.BeginProcessOutput();
        }

        /// <summary>
        /// Kicks off a background task to parse the new debug output
        /// </summary>
        private void BeginProcessOutput()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (this.debugTextBuffer?.CurrentSnapshot is ITextSnapshot snapshot)
            {
                ITrackingPoint startPoint = this.lastTextPoint ?? snapshot.CreateTrackingPoint(0, PointTrackingMode.Negative);
                ITrackingPoint endPoint = snapshot.CreateTrackingPoint(snapshot.Length, PointTrackingMode.Negative);
                this.lastTextPoint = endPoint;

                this.package.JoinableTaskFactory.RunAsync(async delegate
                {
                    await Task.Run(() => this.ProcessOutput(snapshot, startPoint, endPoint));
                }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(this.BeginProcessOutput));
            }
        }

        private void ProcessOutput(ITextSnapshot snapshot, ITrackingPoint startPoint, ITrackingPoint endPoint)
        {
            int textStart = startPoint.GetPosition(snapshot);
            int textLength = endPoint.GetPoint(snapshot) - startPoint.GetPoint(snapshot);
            string text = snapshot.GetText(textStart, textLength);
            List<IReadOnlyList<ITableEntry>> allEntries = new List<IReadOnlyList<ITableEntry>>(this.outputParsers.Count);

            foreach (IOutputParser outputParser in this.outputParsers)
            {
                IReadOnlyList<ITableEntry> entries = outputParser.ParseOutput(text);
                if (entries.Count > 0)
                {
                    allEntries.Add(entries);
                }
            }

            if (allEntries.Count > 0)
            {
                this.package.JoinableTaskFactory.RunAsync(async delegate
                {
                    await this.package.JoinableTaskFactory.SwitchToMainThreadAsync();

                    bool added = false;

                    foreach (IReadOnlyList<ITableEntry> entries in allEntries)
                    {
                        if (this.viewModel.AddEntries(entries))
                        {
                            added = true;
                        }
                    }

                    if (added)
                    {
                        this.NotifyUserAboutNewEntries();
                    }
                }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(this.ProcessOutput));
            }
        }

        private void NotifyUserAboutNewEntries()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DateTime now = DateTime.UtcNow;
            if (this.lastTimeSoundPlayed == default || (now - this.lastTimeSoundPlayed) >= TimeSpan.FromSeconds(2))
            {
                this.lastTimeSoundPlayed = now;

                if (this.package.Options.FlashWindowOnError)
                {
                    IVsUIShell shell = this.package.GetService<SVsUIShell, IVsUIShell>();
                    if (ErrorHandler.Succeeded(shell.GetDialogOwnerHwnd(out IntPtr hwnd)) && hwnd != IntPtr.Zero)
                    {
                        NativeMethods.FlashWindow(hwnd, true, true);
                    }
                }

                if (this.package.Options.ShowPaneOnError)
                {
                    this.package.ShowBindingPane();
                }

                if (this.package.Options.PlaySoundOnError)
                {
                    SystemSounds.Beep.Play();
                }
            }
        }

        public override bool SearchEnabled => true;

        public override void ClearSearch()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.control?.ClearSearch();
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return this.control?.CreateSearch(dwCookie, pSearchQuery, pSearchCallback);
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
                        this.package.Telemetry.TrackEvent(Constants.EventShowPane, this.viewModel.GetEntryTelemetryProperties());
                        break;

                    case __FRAMESHOW.FRAMESHOW_WinHidden:
                        this.lastFrameShow = frameShow;
                        this.package.Telemetry.TrackEvent(Constants.EventHidePane, this.viewModel.GetEntryTelemetryProperties());
                        break;
                }
            }

            return Constants.S_OK;
        }

        int IVsWindowFrameNotify.OnMove() => Constants.S_OK;
        int IVsWindowFrameNotify.OnSize() => Constants.S_OK;
        int IVsWindowFrameNotify.OnDockableChange(int fDockable) => Constants.S_OK;

        int IVsWindowFrameNotify2.OnClose(ref uint pgrfSaveOptions)
        {
            this.package.Telemetry.TrackEvent(Constants.EventClosePane, this.viewModel.GetEntryTelemetryProperties());
            return Constants.S_OK;
        }

        int IVsDebuggerEvents.OnModeChange(DBGMODE dbgmodeNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (dbgmodeNew.HasFlag(DBGMODE.DBGMODE_Run))
            {
                this.package.Telemetry.TrackEvent(Constants.EventDebugStart, this.viewModel.GetEntryTelemetryProperties());
                this.viewModel.IsDebugging = true;

                this.WaitForDebugOutputTextBuffer();
            }
            else if (dbgmodeNew.HasFlag(DBGMODE.DBGMODE_Break))
            {
                this.viewModel.IsDebugging = true;
            }
            else
            {
                this.package.Telemetry.TrackEvent(Constants.EventDebugEnd, this.viewModel.GetEntryTelemetryProperties(includeErrorCodes: true));
                this.viewModel.IsDebugging = false;
            }

            return Constants.S_OK;
        }
    }
}
