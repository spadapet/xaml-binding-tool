using Microsoft.Internal.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.ComponentModelHost;
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
    internal sealed partial class BindingPaneControl : UserControl, IDisposable
    {
        public BindingPaneViewModel ViewModel { get; }
        private readonly IServiceProvider serviceProvider;
        private readonly TableDataSource tableDataSource;
        private ITableManager tableManager;
        private IWpfTableControl4 tableControl;

        public BindingPaneControl(IServiceProvider serviceProvider, BindingPaneViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.serviceProvider = serviceProvider;
            this.tableDataSource = new TableDataSource(this.ViewModel.Entries);

            this.InitializeComponent();
        }

        public void Dispose()
        {
            this.tableHolder.Child = null;

            this.tableControl?.Dispose();
            this.tableManager?.RemoveSource(this.tableDataSource);
            this.tableDataSource.Dispose();
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

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (this.tableControl == null && this.serviceProvider.GetService(typeof(SComponentModel)) is IComponentModel componentModel)
            {
                ITableManagerProvider tableManagerProvider = componentModel.GetService<ITableManagerProvider>();
                IWpfTableControlProvider tableControlProvider = componentModel.GetService<IWpfTableControlProvider>();

                this.tableManager = tableManagerProvider.GetTableManager(Constants.TableManagerString);
                this.tableManager.AddSource(this.tableDataSource, ColumnNames.DefaultSet.ToArray());

                this.tableControl = (IWpfTableControl4)tableControlProvider.CreateControl(this.tableManager, true,
                    ColumnNames.DefaultSet.Select(n => new ColumnState2(n, isVisible: true, width: 0)),
                    ColumnNames.DefaultSet.ToArray());

                this.tableHolder.Child = this.tableControl.Control;

            }
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
