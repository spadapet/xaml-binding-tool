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
    [ProvideToolWindow(typeof(BindingPane), Window = "DocumentWell", Orientation = ToolWindowOrientation.Top, Style = VsDockStyle.Tabbed)]
    [ProvideToolWindowVisibility(typeof(BindingPane), Constants.BindingShowToolWindowString)]
    [Guid(Constants.PackageString)]
    internal sealed class BindingPackage : AsyncPackage
    {
        private SolutionOptions options;
        private Telemetry telemetry;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            this.options = new SolutionOptions();
            this.AddOptionKey(Constants.SolutionOptionKey);

            this.telemetry = new Telemetry(this.options);
            this.telemetry.TrackEvent(Constants.EventInitializePackage);

            // Add a command handler for showing the tool window
            CommandID menuCommandID = new CommandID(Constants.GuidCommandSet, Constants.BindingToolWindowCommandId);
            MenuCommand menuItem = new MenuCommand((s, a) => this.ShowBindingPane(), menuCommandID);
            OleMenuCommandService commandService = await this.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            commandService?.AddCommand(menuItem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.telemetry.Dispose();
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
                return Task.FromResult<object>(this.telemetry);
            }

            return base.InitializeToolWindowAsync(toolWindowType, id, cancellationToken);
        }

        protected override void OnLoadOptions(string key, Stream stream)
        {
            base.OnLoadOptions(key, stream);

            if (key == Constants.SolutionOptionKey && stream != null)
            {
                this.options.Load(stream);
            }
        }

        protected override void OnSaveOptions(string key, Stream stream)
        {
            if (key == Constants.SolutionOptionKey && stream != null)
            {
                this.options.Save(stream);
            }

            base.OnSaveOptions(key, stream);
        }

        private void ShowBindingPane()
        {
            this.JoinableTaskFactory.RunAsync(async delegate
            {
                await this.ShowToolWindowAsync(typeof(BindingPane), 0, true, this.DisposalToken);
            });
        }
    }
}
