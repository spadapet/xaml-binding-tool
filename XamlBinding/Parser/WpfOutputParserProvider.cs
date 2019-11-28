using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace XamlBinding.Parser
{
    [Export(typeof(IOutputParserProvider))]
    [Name(nameof(WpfOutputParserProvider))]
    internal sealed class WpfOutputParserProvider : IOutputParserProvider
    {
        private Lazy<WpfOutputParser> outputParser = new Lazy<WpfOutputParser>();

        IOutputParser IOutputParserProvider.GetOutputParser() => this.outputParser.Value;
    }
}
