﻿using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace XamlBinding.Parser.UWP
{
    [Export(typeof(IOutputParserProvider))]
    [Name(nameof(UwpOutputParserProvider))]
    internal sealed class UwpOutputParserProvider : IOutputParserProvider
    {
        private readonly Lazy<UwpOutputParser> outputParser = new Lazy<UwpOutputParser>();

        IOutputParser IOutputParserProvider.GetOutputParser() => this.outputParser.Value;
    }
}
