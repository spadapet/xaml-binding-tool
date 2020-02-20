using System;
using System.Collections.Generic;
using Microsoft.DiagnosticsHub;
using Microsoft.DiagnosticsHub.Collectors;
using Microsoft.DiagnosticsHub.Views;
using Microsoft.VisualStudio.Shell;
using XamlBinding.Package;
using XamlBinding.Resources;
using XamlBinding.Utility;

namespace XamlBinding.DiagnosticTools
{
    internal sealed class BindingView : IDetailsView, IMessageListener, IDisposable
    {
        private ICollectorTransportService transportService;
        private Lazy<BindingViewModel> viewModel;
        private Lazy<BindingViewControl> content;
        private StringCache stringCache;

        private BindingViewModel ViewModel => this.viewModel.Value;

        public BindingView(IHubServiceProvider serviceprovider, IDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IServiceProvider globalServiceProvider = serviceprovider.GetService<IServiceProvider>();
            BindingPackage package = BindingPackage.Get(globalServiceProvider);
            Telemetry telemetry = package.Telemetry;

            this.viewModel = new Lazy<BindingViewModel>(() => new BindingViewModel(document, telemetry));
            this.content = new Lazy<BindingViewControl>(() => new BindingViewControl(globalServiceProvider, this.ViewModel));
            this.stringCache = new StringCache();

            ICollectorTransportServiceController transportServiceController = serviceprovider.GetService<ICollectorTransportServiceController>(document);
            this.transportService = transportServiceController.GetCollectorTransportService(Guids.StandardCollectorComponentId);
            this.transportService.AddMessageListener(Constants.BindingToolGuid, this);
        }

        void IDisposable.Dispose()
        {
        }

        ControlType IView.ControlType => ControlType.WpfControl;
        string IDetailsView.GetDescription() => Resource.BindingDetailsView_Description;
        Guid IDetailsView.GetId() => Constants.BindingDetailsViewGuid;
        string IDetailsView.GetTitle() => Resource.BindingDetailsView_Title;
        object IView.GetViewContent() => this.content.Value;

        void IMessageListener.OnByteMessageReceived(byte[] message)
        {
        }

        void IMessageListener.OnFileMessageReceived(FileMessage message)
        {
        }

        void IMessageListener.OnStringMessageReceived(string message)
        {
            string[] rows = message.Split('\n');
            Dictionary<string, string> properties = new Dictionary<string, string>(rows.Length);

            foreach (string row in rows)
            {
                int equal = row.IndexOf('=');
                if (equal != -1)
                {
                    string name = row.Substring(0, equal);
                    string value = row.Substring(equal + 1);

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                    {
                        properties[name] = value;
                    }
                }
            }

            if (properties.TryGetValue("Event", out string eventName) && eventName == "BindingFailed")
            {
                TableEntry entry = new TableEntry(properties, this.stringCache);
                this.ViewModel.AddEntry(entry);
            }
        }
    }
}
