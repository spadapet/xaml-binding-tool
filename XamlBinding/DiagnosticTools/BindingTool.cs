using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.DiagnosticsHub;
using Microsoft.DiagnosticsHub.Collectors;
using Microsoft.DiagnosticsHub.DataWarehouse;
using Microsoft.DiagnosticsHub.Tools;
using Microsoft.DiagnosticsHub.Views;

namespace XamlBinding.DiagnosticTools
{
    internal sealed class BindingTool : ITool, IToolSummary
    {
        private string agentPath;
        private Lazy<BindingView> view;
        private Lazy<BindingSummaryItem> summaryItem;

        public BindingTool(string agentPath, IHubServiceProvider serviceprovider, IDocument document)
        {
            Debug.Assert(File.Exists(agentPath));

            this.agentPath = agentPath;
            this.view = new Lazy<BindingView>(() => new BindingView(serviceprovider, document));
            this.summaryItem = new Lazy<BindingSummaryItem>();
        }

        /// <summary>
        /// Tells the collector service which agent DLL needs to be loaded
        /// </summary>
        IEnumerable<ICollectorConfiguration> ITool.GetCollectorConfigurations()
        {
            EtwProviderConfiguration xamlEtwProvider = EtwProviderConfiguration.Create(Constants.Microsoft_VisualStudio_DesignTools_XamlTrace_Guid);
            xamlEtwProvider.FilterByProcessId = true;
            xamlEtwProvider.Level = EtwProviderLevel.Verbose;

            yield return new StandardCollectorConfiguration(
                new EtwProviderConfiguration[]
                {
                    xamlEtwProvider
                },
                new StandardCollectorAgentConfiguration[]
                {
                    new StandardCollectorAgentConfiguration()
                    {
                        Clsid = Constants.StandardCollectorAgentGuid,
                        LocalDllPath = Path.GetFileName(this.agentPath),
                    }
                });
        }

        /// <summary>
        /// ETW data is sent directly from the agent and not analyzed in a data warehouse
        /// </summary>
        IDataWarehouseConfiguration ITool.GetDataWarehouseConfiguration() => null;

        IEnumerable<ISwimLaneConfiguration> ITool.GetSwimLanes(ViewDestinations viewDestination) => Enumerable.Empty<ISwimLaneConfiguration>();

        IEnumerable<IDetailsView> ITool.GetDetailsViews(ViewDestinations viewDestination)
        {
            if (viewDestination.HasFlag(ViewDestinations.PerformanceDebugging))
            {
                yield return this.view.Value;
            }
        }

        IEnumerable<ISummaryItem> IToolSummary.GetSummaryItems(ViewDestinations viewDestination)
        {
            if (viewDestination.HasFlag(ViewDestinations.PerformanceDebugging))
            {
                yield return this.summaryItem.Value;
            }
        }
    }
}
