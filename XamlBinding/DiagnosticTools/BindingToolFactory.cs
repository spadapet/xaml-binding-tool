using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.DiagnosticsHub;
using Microsoft.DiagnosticsHub.Tools;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using XamlBinding.Resources;

namespace XamlBinding.DiagnosticTools
{
    [Export(typeof(IToolFactory))]
    [ExportMetadata(ToolFactoryMetadataConstants.Id, Constants.BindingToolString)]
    [ExportMetadata(ToolFactoryMetadataConstants.SupportsDebugger, true)]
    internal sealed class BindingToolFactory : IToolFactory
    {
        private IHubServiceProvider serviceProvider;
        private Lazy<string> agentPath;
        private bool AgentExists => this.agentPath.Value != null;

        [ImportingConstructor]
        public BindingToolFactory(IHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.agentPath = new Lazy<string>(BindingToolFactory.GetAgentPath);
        }

        /// <summary>
        /// The agent DLL might not be installed (it has to be part of the VS install) so don't enable this tool if there is no agent DLL
        /// </summary>
        private static string GetAgentPath()
        {
            string agentPath = null;

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: false);

                IVsShell shell = ServiceProvider.GlobalProvider.GetService<SVsShell, IVsShell>();
                if (ErrorHandler.Succeeded(shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out object dirObject)) && dirObject is string dirString)
                {
                    agentPath = Path.GetFullPath(Path.Combine(dirString, "..", "..", "Team Tools", "DiagnosticsHub", "Collector", "Agents", "XamlTraceAgent.dll"));
                    if (!File.Exists(agentPath))
                    {
                        agentPath = null;
                    }
                }
            });

            return agentPath;
        }

        ITool IToolFactory.CreateTool(IDocument document) => this.AgentExists ? new BindingTool(this.agentPath.Value, this.serviceProvider, document) : null;
        string IToolFactory.GetDescription() => Resource.BindingToolFactory_Description;
        bool IToolFactory.GetExclusivity() => false;
        Guid IHubComponent.GetId() => Constants.BindingToolGuid;
        string IToolFactory.GetName() => Resource.BindingToolFactory_Title;
        SupportedScenario IToolFactory.IsSupported(PerformanceSessionConfiguration sessionConfiguration) => this.AgentExists ? SupportedScenario.Supported : SupportedScenario.Unsupported;
        void IToolFactory.LaunchStandaloneScenario(PerformanceSessionConfiguration sessionConfiguration) => throw new NotSupportedException();
    }
}
