using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using XamlBinding.Resources;
using XamlBinding.ToolWindow;
using XamlBinding.Utility;
using Task = System.Threading.Tasks.Task;

namespace XamlBinding.Package
{
    /// <summary>
    /// The entry point for running within Visual Studio
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(BindingPane), Window = Constants.CallStackWindowString, Orientation = ToolWindowOrientation.Bottom, Style = VsDockStyle.Tabbed)]
    [ProvideToolWindowVisibility(typeof(BindingPane), Constants.ShowBindingPaneContextString)]
    [ProvideOptionPage(typeof(BindingOptions), "Debugger", "XamlBinding", 0, 100, true)]
    [Guid(Constants.BindingPackageString)]
    internal sealed class BindingPackage : AsyncPackage
    {
        public Telemetry Telemetry { get; private set; }
        public IOptions Options { get; private set; }

        private SolutionOptions solutionOptions;

        public static BindingPackage Get(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsShell shell = serviceProvider.GetService<SVsShell, IVsShell>();
            Guid packageGuid = typeof(BindingPackage).GUID;

            if (ErrorHandler.Succeeded(shell.LoadPackage(ref packageGuid, out IVsPackage package)))
            {
                return package as BindingPackage;
            }

            return null;
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            this.solutionOptions = new SolutionOptions();
            this.AddOptionKey(Constants.SolutionOptionKey);

            this.Options = (IOptions)this.GetDialogPage(typeof(BindingOptions));
            this.Telemetry = new Telemetry(this.Options, this.solutionOptions);
            this.Telemetry.TrackEvent(Constants.EventInitializePackage);

            await this.InitMenuCommandsAsync();
        }

        private async Task InitMenuCommandsAsync()
        {
            CommandID menuCommandID = new CommandID(Constants.GuidPackageCommandSet, Constants.BindingPaneCommandId);
            MenuCommand menuItem = new MenuCommand((s, a) => this.ShowBindingPane(), menuCommandID);
            IMenuCommandService commandService = await this.GetServiceAsync<IMenuCommandService, IMenuCommandService>();
            commandService.AddCommand(menuItem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Telemetry?.Dispose();
            }

            base.Dispose(disposing);
        }

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (toolWindowType == typeof(BindingPane).GUID)
            {
                return this;
            }

            return base.GetAsyncToolWindowFactory(toolWindowType);
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            if (toolWindowType == typeof(BindingPane))
            {
                return Resource.ToolWindow_TitleLoading;
            }

            return base.GetToolWindowTitle(toolWindowType, id);
        }

        protected override Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            if (toolWindowType == typeof(BindingPane))
            {
                // Return value passed to BindingPane constructor
                return Task.FromResult<object>(this);
            }

            return base.InitializeToolWindowAsync(toolWindowType, id, cancellationToken);
        }

        protected override void OnLoadOptions(string key, Stream stream)
        {
            base.OnLoadOptions(key, stream);

            if (key == Constants.SolutionOptionKey && stream != null)
            {
                this.solutionOptions.Load(stream);
            }
        }

        protected override void OnSaveOptions(string key, Stream stream)
        {
            if (key == Constants.SolutionOptionKey && stream != null)
            {
                this.solutionOptions.Save(stream);
            }

            base.OnSaveOptions(key, stream);
        }

        public void ShowBindingPane(bool create = true)
        {
            this.JoinableTaskFactory.RunAsync(async delegate
            {
                await this.ShowToolWindowAsync(typeof(BindingPane), 0, create, this.DisposalToken);
            }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(this.ShowBindingPane));
        }
    }
}
