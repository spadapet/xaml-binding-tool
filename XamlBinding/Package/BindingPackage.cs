using Microsoft.Internal.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using XamlBinding.Resources;
using XamlBinding.ToolWindow;
using XamlBinding.ToolWindow.Table;
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
    [Guid(Constants.BindingPackageString)]
    internal sealed class BindingPackage : AsyncPackage
    {
        public IComponentModel ComponentModel { get; private set; }
        public IWpfTableControlProvider TableControlProvider { get; private set; }
        public ITableManager TableManager { get; private set; }
        public Telemetry Telemetry { get; private set; }

        private SolutionOptions options;
        private IDisposable registeredTableColumns;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            this.ComponentModel = await this.GetServiceAsync<SComponentModel, IComponentModel>();

            this.options = new SolutionOptions();
            this.AddOptionKey(Constants.SolutionOptionKey);

            this.Telemetry = new Telemetry(this.options);
            this.Telemetry.TrackEvent(Constants.EventInitializePackage);

            await this.InitializeTableAsync();
            await this.InitializeMenuCommandsAsync();
        }

        private async Task InitializeTableAsync()
        {
            ITableManagerProvider tableManagerProvider = this.ComponentModel.GetService<ITableManagerProvider>();
            this.TableControlProvider = this.ComponentModel.GetService<IWpfTableControlProvider>();

            await Task.Run(() =>
            {
                this.TableManager = tableManagerProvider.GetTableManager(Constants.TableManagerString);
                this.registeredTableColumns = TableColumn.RegisterColumnDefinitions(this.TableControlProvider);
            });
        }

        private async Task InitializeMenuCommandsAsync()
        {
            // Add a command handler for showing the tool window
            CommandID menuCommandID = new CommandID(Constants.GuidPackageCommandSet, Constants.BindingPaneCommandId);
            MenuCommand menuItem = new MenuCommand((s, a) => this.ShowBindingPane(), menuCommandID);
            OleMenuCommandService commandService = await this.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            commandService?.AddCommand(menuItem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Telemetry?.Dispose();
                this.registeredTableColumns?.Dispose();
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
            }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(this.ShowBindingPane));
        }
    }
}
