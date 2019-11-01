﻿using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// Executes commands in the toolbar
    /// </summary>
    internal class BindingPaneController : IOleCommandTarget
    {
        private readonly BindingPaneViewModel viewModel;
        private readonly IVsUIShell shell;
        private readonly string[] traceLevelDisplayNames;
        private readonly string[] traceLevels;

        public BindingPaneController(BindingPaneViewModel viewModel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.viewModel = viewModel;
            this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;

            this.traceLevelDisplayNames = Resource.TraceLevels.Split(',');
            this.traceLevels = new string[]
            {
                nameof(BindingTraceLevels.Off),
                nameof(BindingTraceLevels.Critical),
                nameof(BindingTraceLevels.Error),
                nameof(BindingTraceLevels.Warning),
                nameof(BindingTraceLevels.Information),
                nameof(BindingTraceLevels.Verbose),
                nameof(BindingTraceLevels.Activity),
                nameof(BindingTraceLevels.All),
            };

            if (!Constants.IsXamlDesigner)
            {
                this.shell = ServiceProvider.GlobalProvider.GetService<SVsUIShell, IVsUIShell>();
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
                        prgCmds[0].cmdf = Constants.OLECMDF_SUPPORTED_AND_ENABLED;
                        break;

                    default:
                        Debug.Fail($"Unexpected toolbar command: {prgCmds[0].cmdID}");
                        return Constants.OLECMDERR_E_NOTSUPPORTED;
                }

                return Constants.S_OK;
            }

            return Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == Constants.GuidBindingPaneCommandSet)
            {
                switch (nCmdID)
                {
                    case Constants.ClearCommandId:
                        this.viewModel.Telemetry.TrackEvent(Constants.EventClearPane, this.viewModel.GetEntryTelemetryProperties());
                        this.viewModel.ClearEntries();
                        break;

                    case Constants.TraceLevelDropDownId:
                        this.OnTraceLevelCommand(pvaIn, pvaOut);
                        break;

                    case Constants.TraceLevelDropDownListId:
                        this.OnTraceLevelCommandList(pvaOut);
                        break;

                    case Constants.TraceLevelOptionsId:
                        this.ShowTraceLevelOptions();
                        break;

                    default:
                        Debug.Fail($"Unexpected toolbar command: {nCmdID}");
                        return Constants.OLECMDERR_E_NOTSUPPORTED;
                }
            }

            return Constants.OLECMDERR_E_NOTSUPPORTED;
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
                    this.ShowTraceLevelOptions();
                }
                else if (i >= 0)
                {
                    this.viewModel.Telemetry.TrackEvent(Constants.EventSetTraceLevel, new Dictionary<string, object>()
                    {
                        { Constants.PropertyTraceLevel, (BindingTraceLevels)i },
                    });

                    using (RegistryKey rootKey = VSRegistry.RegistryRoot(ServiceProvider.GlobalProvider, __VsLocalRegistryType.RegType_UserSettings, writable: true))
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

        private void ShowTraceLevelOptions()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.viewModel.Telemetry.TrackEvent(Constants.EventShowTraceOptions);

            Guid cmdGroup = Constants.GuidCommandSet97;
            object debugOutputPageId = Constants.ToolsOptionsDebugOutputPageString;
            const int cmdidToolsOptions = Constants.ToolsOptionsCommandId;

            this.shell.PostExecCommand(ref cmdGroup, cmdidToolsOptions, 0, ref debugOutputPageId);
        }

        private void UpdateCommandUI()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.shell?.UpdateCommandUI(0);
        }
    }
}