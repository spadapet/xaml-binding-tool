using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Internal.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using XamlBinding.ToolWindow.Columns;
using XamlBinding.ToolWindow.Table;
using IServiceProvider = System.IServiceProvider;

namespace XamlBinding.DiagnosticTools
{
    internal sealed partial class BindingViewControl : UserControl, IDisposable
    {
        public BindingViewModel ViewModel { get; }
        public IWpfTableControl4 TableControl { get; }
        private readonly TableDataSource tableDataSource;
        private readonly ITableManager tableManager;

        public BindingViewControl(IServiceProvider serviceProvider, BindingViewModel viewModel)
        {
            IComponentModel componentModel = serviceProvider.GetService<SComponentModel, IComponentModel>();
            ITableManagerProvider tableManagerProvider = componentModel.GetService<ITableManagerProvider>();
            IWpfTableControlProvider tableControlProvider = componentModel.GetService<IWpfTableControlProvider>();

            this.ViewModel = viewModel;
            this.tableDataSource = new TableDataSource(this.ViewModel.Entries);
            this.tableManager = tableManagerProvider.GetTableManager(Constants.TableManagerString);
            this.tableManager.AddSource(this.tableDataSource, ColumnNames.DefaultSet.ToArray());
            this.TableControl = (IWpfTableControl4)tableControlProvider.CreateControl(this.tableManager, true,
                ColumnNames.DefaultSet.Select(n => new ColumnState2(n, isVisible: true, width: 0)),
                ColumnNames.DefaultSet.ToArray());

            this.InitializeComponent();

            this.tableHolder.Child = this.TableControl.Control;
        }

        public void Dispose()
        {
            this.TableControl.Dispose();
            this.tableManager.RemoveSource(this.tableDataSource);
            this.tableDataSource.Dispose();
        }

        public void ClearSearch()
        {
            TableSearchTask.ClearSearch(this.TableControl);
        }

        public IVsSearchTask CreateSearch(uint cookie, IVsSearchQuery searchQuery, IVsSearchCallback searchCallback)
        {
            return new TableSearchTask(cookie, searchQuery, searchCallback, this.TableControl);
        }

        private void OnKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            this.ViewModel.Telemetry.TrackEvent(Constants.EventFocusChanged, new Dictionary<string, object>()
            {
                { Constants.PropertyFocused, this.IsKeyboardFocusWithin },
            });
        }
    }
}
