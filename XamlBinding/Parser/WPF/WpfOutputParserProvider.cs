using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace XamlBinding.Parser.WPF
{
    [Export(typeof(IOutputParserProvider))]
    [Name(nameof(WpfOutputParserProvider))]
    internal sealed class WpfOutputParserProvider : IOutputParserProvider
    {
        private readonly Lazy<WpfOutputParser> outputParser = new Lazy<WpfOutputParser>();

        IOutputParser IOutputParserProvider.GetOutputParser() => this.outputParser.Value;
    }
}
