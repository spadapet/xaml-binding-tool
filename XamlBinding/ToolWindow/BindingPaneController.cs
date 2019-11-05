using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using XamlBinding.Parser;
using XamlBinding.Resources;
using XamlBinding.ToolWindow.Columns;
using IServiceProvider = System.IServiceProvider;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// Executes commands in the toolbar
    /// </summary>
    internal sealed class BindingPaneController : IOleCommandTarget
    {
        private readonly IServiceProvider serviceProvider;
        private readonly BindingPaneViewModel viewModel;
        private readonly IWpfTableControl table;
        private readonly IVsUIShell shell;
        private readonly string[] traceLevelDisplayNames;
        private readonly string[] traceLevels;

        public BindingPaneController(IServiceProvider serviceProvider, BindingPaneViewModel viewModel, IWpfTableControl table)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.serviceProvider = serviceProvider;
            this.viewModel = viewModel;
            this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;
            this.table = table;
            this.table.Control.Tag = this;

            this.traceLevelDisplayNames = Resource.TraceLevels.Split(',');
            this.traceLevels = new string[]
            {
                nameof(TraceLevels.Off),
                nameof(TraceLevels.Critical),
                nameof(TraceLevels.Error),
                nameof(TraceLevels.Warning),
                nameof(TraceLevels.Information),
                nameof(TraceLevels.Verbose),
                nameof(TraceLevels.Activity),
                nameof(TraceLevels.All),
            };

            if (!Constants.IsXamlDesigner)
            {
                this.shell = this.serviceProvider.GetService<SVsUIShell, IVsUIShell>();
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            bool all = string.IsNullOrEmpty(args.PropertyName);

            if (all ||
                args.PropertyName == nameof(this.viewModel.CanClearEntries) ||
                args.PropertyName == nameof(this.viewModel.TraceLevel))
            {
                this.UpdateCommandUI();
            }
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == Constants.GuidBindingPaneCommandSet && cCmds == 1)
            {
                switch (prgCmds[0].cmdID)
                {
                    case Constants.ClearCommandId:
                        prgCmds[0].cmdf = this.viewModel.CanClearEntries
                            ? (uint)Constants.OLECMDF_SUPPORTED_AND_ENABLED
                            : (uint)Constants.OLECMDF_SUPPORTED;
                        break;

                    case Constants.TraceLevelDropDownId:
                    case Constants.TraceLevelDropDownListId:
                    case Constants.TraceLevelOptionsId:
                    case Constants.ProvideFeedbackId:
                    case Constants.CopyDataContextId:
                    case Constants.CopyBindingPathId:
                    case Constants.CopyTargetId:
                        prgCmds[0].cmdf = Constants.OLECMDF_SUPPORTED_AND_ENABLED;
                        break;

                    default:
                        Debug.Fail($"Unexpected toolbar command: {prgCmds[0].cmdID}");
                        return Constants.OLECMDERR_E_NOTSUPPORTED;
                }

                return Constants.S_OK;
            }

            return (this.table is IOleCommandTarget nextController)
                ? nextController.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText)
                : Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == Constants.GuidBindingPaneCommandSet)
            {
                switch (nCmdID)
                {
                    case Constants.ClearCommandId:
                        this.viewModel.Telemetry.TrackEvent(Constants.EventClearPane, this.viewModel.GetEntryTelemetryProperties(includeErrorCodes: true));
                        this.viewModel.ClearEntries();
                        break;

                    case Constants.TraceLevelDropDownId:
                        this.OnTraceLevelCommand(pvaIn, pvaOut);
                        break;

                    case Constants.TraceLevelDropDownListId:
                        this.OnTraceLevelCommandList(pvaOut);
                        break;

                    case Constants.TraceLevelOptionsId:
                        this.OnTraceLevelOptions();
                        break;

                    case Constants.ProvideFeedbackId:
                        this.OnProvideFeedback();
                        break;

                    case Constants.CopyDataContextId:
                        this.OnCopyColumnValue(ColumnNames.DataContextType);
                        break;

                    case Constants.CopyBindingPathId:
                        this.OnCopyColumnValue(ColumnNames.BindingPath);
                        break;

                    case Constants.CopyTargetId:
                        this.OnCopyColumnValue(ColumnNames.Target);
                        break;

                    default:
                        Debug.Fail($"Unexpected toolbar command: {nCmdID}");
                        return Constants.OLECMDERR_E_NOTSUPPORTED;
                }
            }

            return (this.table is IOleCommandTarget nextController)
                ? nextController.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut)
                : Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        private void OnCopyColumnValue(string columnName)
        {
            this.viewModel.Telemetry.TrackEvent(Constants.EventCopyColumnValue, new Dictionary<string, object>()
            {
                { Constants.PropertyColumnValue, columnName },
            });

            StringBuilder sb = new StringBuilder();

            if (this.table.ColumnDefinitionManager.GetColumnDefinition(columnName) is ITableColumnDefinition columnDefinition)
            {
                foreach (ITableEntryHandle handle in this.table.SelectedEntries)
                {
                    if (handle.TryCreateStringContent(columnDefinition, false, false, out string content) && !string.IsNullOrWhiteSpace(content))
                    {
                        sb.AppendLine(content);
                    }
                }
            }

            string copyValue = sb.ToString().Trim();

            if (!string.IsNullOrEmpty(copyValue))
            {
                Clipboard.SetText(copyValue);
            }
        }

        private void OnProvideFeedback()
        {
            this.viewModel.Telemetry.TrackEvent(Constants.EventShowTraceOptions);

            try
            {
                Process.Start(new ProcessStartInfo(@"https://github.com/spadapet/xaml-binding-tool/issues")
                {
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                this.viewModel.Telemetry.TrackException(ex);
            }
        }

        private void OnTraceLevelCommand(IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pvaOut != IntPtr.Zero)
            {
                Marshal.GetNativeVariantForObject(this.viewModel.TraceLevel, pvaOut);
            }
            else if (pvaIn != IntPtr.Zero && Marshal.GetObjectForNativeVariant(pvaIn) is string newValue)
            {
                int i = Array.IndexOf(this.traceLevelDisplayNames, newValue);
                if (i >= this.traceLevels.Length)
                {
                    this.OnTraceLevelOptions();
                }
                else if (i >= 0)
                {
                    this.viewModel.Telemetry.TrackEvent(Constants.EventSetTraceLevel, this.viewModel.GetEntryTelemetryProperties());

                    using (RegistryKey rootKey = VSRegistry.RegistryRoot(this.serviceProvider, __VsLocalRegistryType.RegType_UserSettings, writable: true))
                    using (RegistryKey dataBindingOutputLevelKey = rootKey.CreateSubKey(Constants.DataBindingTraceKey, writable: true))
                    {
                        dataBindingOutputLevelKey?.SetValue(Constants.DataBindingTraceLevel, this.traceLevels[i], RegistryValueKind.String);
                    }
                }
            }
        }

        private void OnTraceLevelCommandList(IntPtr pvaOut)
        {
            if (pvaOut != IntPtr.Zero)
            {
                Marshal.GetNativeVariantForObject(this.traceLevelDisplayNames, pvaOut);
            }
        }

        private void OnTraceLevelOptions()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.viewModel.Telemetry.TrackEvent(Constants.EventShowTraceOptions);

            Guid cmdGroup = Constants.GuidCommandSet97;
            object debugOutputPageId = Constants.ToolsOptionsDebugOutputPageString;
            const int cmdidToolsOptions = Constants.ToolsOptionsCommandId;

            this.shell?.PostExecCommand(ref cmdGroup, cmdidToolsOptions, 0, ref debugOutputPageId);
        }

        private void UpdateCommandUI()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.shell?.UpdateCommandUI(0);
        }
    }
}
