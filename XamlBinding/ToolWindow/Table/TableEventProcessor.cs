using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using System;
using System.Windows;
using System.Windows.Input;
using IServiceProvider = System.IServiceProvider;

namespace XamlBinding.ToolWindow.Table
{
    internal sealed class TableEventProcessor : ITableControlEventProcessor
    {
        private readonly IServiceProvider services;
        private readonly IWpfTableControl control;

        public TableEventProcessor(IServiceProvider services, IWpfTableControl control)
        {
            this.services = services;
            this.control = control;
        }

        private void ShowContextMenu(bool mousePosition)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true);

                FrameworkElement table = this.control.Control;
                IOleCommandTarget commandTarget = table.Tag as IOleCommandTarget;
                Point point = mousePosition
                    ? table.PointToScreen(Mouse.GetPosition(table))
                    : table.PointToScreen(new Point(0, table.RenderSize.Height)); // same as error list, but should use focus location
                POINTS[] locationPoints = new[] { new POINTS() { x = (short)point.X, y = (short)point.Y } };

                IVsUIShell shell = this.services.GetService<SVsUIShell, IVsUIShell>();
                Guid commandSet = Constants.GuidBindingPaneCommandSet;

                shell.ShowContextMenu(0, ref commandSet, Constants.BindingPaneContextMenuId, locationPoints, commandTarget);
            }).FileAndForget(Constants.VsBindingPaneFeaturePrefix + nameof(ITableControlEventProcessor.PostprocessMouseRightButtonUp));
        }

        void ITableControlEventProcessor.KeyDown(KeyEventArgs args)
        {
        }

        void ITableControlEventProcessor.KeyUp(KeyEventArgs args)
        {
            if (!args.Handled && args.Key == Key.Apps)
            {
                args.Handled = true;
                this.ShowContextMenu(mousePosition: false);
            }
        }

        void ITableControlEventProcessor.PostprocessDragEnter(ITableEntryHandle entry, DragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessDragLeave(ITableEntryHandle entry, DragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessDragOver(ITableEntryHandle entry, DragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessDrop(ITableEntryHandle entry, DragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessGiveFeedback(ITableEntryHandle entry, GiveFeedbackEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseDown(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseEnter(ITableEntryHandle entry, MouseEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseLeave(ITableEntryHandle entry, MouseEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseLeftButtonDown(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseLeftButtonUp(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseMove(ITableEntryHandle entry, MouseEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseRightButtonDown(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseRightButtonUp(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
            this.ShowContextMenu(mousePosition: true);
        }

        void ITableControlEventProcessor.PostprocessMouseUp(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessMouseWheel(ITableEntryHandle entry, MouseWheelEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessNavigate(ITableEntryHandle entry, TableEntryNavigateEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessNavigateToHelp(ITableEntryHandle entry, TableEntryEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessQueryContinueDrag(ITableEntryHandle entry, QueryContinueDragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PostprocessSelectionChanged(TableSelectionChangedEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessDragEnter(ITableEntryHandle entry, DragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessDragLeave(ITableEntryHandle entry, DragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessDragOver(ITableEntryHandle entry, DragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessDrop(ITableEntryHandle entry, DragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessGiveFeedback(ITableEntryHandle entry, GiveFeedbackEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseDown(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseEnter(ITableEntryHandle entry, MouseEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseLeave(ITableEntryHandle entry, MouseEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseLeftButtonDown(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseLeftButtonUp(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseMove(ITableEntryHandle entry, MouseEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseRightButtonDown(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseRightButtonUp(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseUp(ITableEntryHandle entry, MouseButtonEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessMouseWheel(ITableEntryHandle entry, MouseWheelEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessNavigate(ITableEntryHandle entry, TableEntryNavigateEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessNavigateToHelp(ITableEntryHandle entry, TableEntryEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessQueryContinueDrag(ITableEntryHandle entry, QueryContinueDragEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreprocessSelectionChanged(TableSelectionChangedEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreviewKeyDown(KeyEventArgs args)
        {
        }

        void ITableControlEventProcessor.PreviewKeyUp(KeyEventArgs args)
        {
        }
    }
}
