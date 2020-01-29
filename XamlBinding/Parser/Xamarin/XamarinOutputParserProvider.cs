using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace XamlBinding.Parser.Xamarin
{
    [Export(typeof(IOutputParserProvider))]
    [Name(nameof(XamarinOutputParserProvider))]
    internal sealed class XamarinOutputParserProvider : IOutputParserProvider
    {
        private readonly Lazy<XamarinOutputParser> outputParser = new Lazy<XamarinOutputParser>();

        IOutputParser IOutputParserProvider.GetOutputParser() => this.outputParser.Value;
    }
}
