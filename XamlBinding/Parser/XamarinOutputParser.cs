using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using XamlBinding.ToolWindow.Entries;

namespace XamlBinding.Parser
{
    /// <summary>
    /// Converts UWP's debug output into a list of table entries
    /// </summary>
    internal sealed class XamarinOutputParser : OutputParserBase<XamarinTraceCode>
    {
        private const string CaptureDescription = "description";
        private static readonly string ProcessTextPattern = $@"^\[.+?\] Binding: (?<{XamarinOutputParser.CaptureDescription}>.+?)\r?$";

        public XamarinOutputParser()
            : base(XamarinOutputParser.ProcessTextPattern)
        {
            this.AddRegex(XamarinTraceCode.PropertyNotFound,
                $@"'(?<{nameof(XamarinEntry.BindingPath)}>.+?)' property not found on '(?<{nameof(XamarinEntry.DataItemType)}>.+?)', target property: '(?<{nameof(XamarinEntry.TargetElementType)}>.+)\.(?<{nameof(XamarinEntry.TargetProperty)}>.+?)'");

            this.AddRegex(XamarinTraceCode.BadType,
                $@"XAMARIN TODO ADD REGEX");

            this.AddRegex(XamarinTraceCode.BadIndex,
                $@"XAMARIN TODO ADD REGEX");
        }

        protected override ITableEntry ProcessLine(Match match)
        {
            string description = match.Groups[XamarinOutputParser.CaptureDescription].Value;

            foreach (KeyValuePair<XamarinTraceCode, Lazy<Regex>> kvp in this.CodeToRegex)
            {
                Match descriptionMatch = kvp.Value.Value.Match(description);
                if (descriptionMatch.Success)
                {
                    return new XamarinEntry(kvp.Key, descriptionMatch, this.StringCache);
                }
            }

            Debug.Fail($"Failed to match Xamarin binding failure text: {description}");
            return new XamarinEntry(XamarinTraceCode.None, description, this.StringCache);
        }
    }
}
