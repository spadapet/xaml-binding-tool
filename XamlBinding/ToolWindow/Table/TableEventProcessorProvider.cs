using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace XamlBinding.ToolWindow.Table
{
    [Export(typeof(ITableControlEventProcessorProvider))]
    [DataSourceType(Constants.TableManagerString)]
    [DataSource(Constants.TableManagerString)]
    [Name("XamlBinding_TableEventProcessorProvider")]
    [Order(Before = Priority.Default)]
    internal sealed class TableEventProcessorProvider : ITableControlEventProcessorProvider
    {
        [Import(typeof(SVsServiceProvider))]
        public IServiceProvider ServiceProvider { get; set; }

        ITableControlEventProcessor ITableControlEventProcessorProvider.GetAssociatedEventProcessor(IWpfTableControl tableControl)
        {
            return new TableEventProcessor(this.ServiceProvider, tableControl);
        }
    }
}
