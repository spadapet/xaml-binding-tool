using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace XamlBinding.ToolWindow
{
    internal partial class BindingPaneControl : UserControl
    {
        public BindingPaneViewModel ViewModel { get; }

        public BindingPaneControl(BindingPaneViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.InitializeComponent();
        }

        private void OnListViewKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            this.ViewModel.Telemetry.TrackEvent(Constants.EventListViewFocusChanged, new Dictionary<string, object>()
            {
                { Constants.PropertyFocused, this.listView.IsKeyboardFocusWithin },
            });
        }

        private void OnClickHyperlink(object sender, RequestNavigateEventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                string uriString = args.Uri.ToString();

                if (uriString == @"tools://options/debug/output")
                {
                    Guid cmdGroup = Constants.GuidCommandSet97;
                    object debugOutputPageId = Constants.ToolsOptionsDebugOutputPageString;
                    const int cmdidToolsOptions = Constants.ToolsOptionsCommandId;

                    if (ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) is IVsUIShell shell)
                    {
                        shell.PostExecCommand(ref cmdGroup, cmdidToolsOptions, 0, ref debugOutputPageId);
                    }
                }
                else
                {
                    Process.Start(new ProcessStartInfo(uriString)
                    {
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                this.ViewModel.Telemetry.TrackException(ex);
            }
        }
    }
}
