using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;

namespace XamlBinding.ToolWindow.Table
{
    /// <summary>
    /// Sets the appropriate filter on the table when a search is run or cleared
    /// </summary>
    internal class TableSearchTask : VsSearchTask
    {
        private readonly IWpfTableControl4 control;

        public TableSearchTask(uint cookie, IVsSearchQuery searchQuery, IVsSearchCallback searchCallback, IWpfTableControl4 control)
            : base(cookie, searchQuery, searchCallback)
        {
            this.control = control;
        }

        public static void ClearSearch(IWpfTableControl4 control)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                control.SetFilter(nameof(TableSearchTask), null);
            }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(TableSearchTask.ClearSearch));
        }

        protected override void OnStartSearch()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                this.control.SetFilter(nameof(TableSearchTask), new TableSearchFilter(this.SearchQuery, this.control));
            }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(this.OnStartSearch));

            base.OnStartSearch();
        }

        protected override void OnStopSearch()
        {
            TableSearchTask.ClearSearch(this.control);
        }
    }
}
