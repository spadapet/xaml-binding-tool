using Microsoft.Internal.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XamlBinding.ToolWindow.Table;

namespace XamlBinding.ToolWindow
{
    internal partial class BindingPaneControl : UserControl, IDisposable
    {
        public BindingPaneViewModel ViewModel { get; }
        private readonly ITableManager tableManager;
        private readonly IWpfTableControlProvider tableControlProvider;
        private TableDataSource tableDataSource;
        private IWpfTableControl4 tableControl;

        public BindingPaneControl(BindingPaneViewModel viewModel, ITableManager tableManager, IWpfTableControlProvider tableControlProvider)
        {
            this.ViewModel = viewModel;
            this.tableManager = tableManager;
            this.tableControlProvider = tableControlProvider;
            this.tableDataSource = new TableDataSource(this.ViewModel.Entries);
            this.tableManager.AddSource(this.tableDataSource, TableColumn.AllColumnNames.ToArray());

            this.InitializeComponent();

            this.tableControl = (IWpfTableControl4)this.tableControlProvider.CreateControl(this.tableManager, true,
                TableColumn.AllColumnNames.Select(n => new ColumnState2(n, true, 0)),
                TableColumn.AllColumnNames.ToArray());

            this.tableHolder.Child = this.tableControl.Control;
        }

        public void Dispose()
        {
            this.tableHolder.Child = null;

            this.tableControl.Dispose();
            this.tableControl = null;

            this.tableManager.RemoveSource(this.tableDataSource);
            this.tableDataSource.Dispose();
            this.tableDataSource = null;
        }

        private void OnKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            this.ViewModel.Telemetry.TrackEvent(Constants.EventFocusChanged, new Dictionary<string, object>()
            {
                { Constants.PropertyFocused, this.IsKeyboardFocusWithin },
            });
        }

        public void ClearSearch()
        {
            if (this.tableControl != null)
            {
                TableSearchTask.ClearSearch(this.tableControl);
            }
        }

        public IVsSearchTask CreateSearch(uint cookie, IVsSearchQuery searchQuery, IVsSearchCallback searchCallback)
        {
            return this.tableControl != null
                ? new TableSearchTask(cookie, searchQuery, searchCallback, this.tableControl)
                : null;
        }
    }
}
