using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using XamlBinding.Resources;
using XamlBinding.ToolWindow.Entries;

namespace XamlBinding.Parser
{
    /// <summary>
    /// Converts UWP's debug output into a list of table entries
    /// </summary>
    internal sealed class UwpOutputParser : OutputParserBase<UwpTraceCode>
    {
        private readonly Regex bindingExpressionRegex;

        private const string CaptureDescription = "description";
        private const string CaptureBindingExpression = "bindingExpression";
        private static readonly string ProcessTextPattern = $@"^Error: (?<{UwpOutputParser.CaptureDescription}>.+?)(;|\.) BindingExpression: (?<{UwpOutputParser.CaptureBindingExpression}>.+?)\r?$";

        public UwpOutputParser()
            : base(UwpOutputParser.ProcessTextPattern)
        {
            this.bindingExpressionRegex = new Regex($@"^Path='(?<{nameof(UwpEntry.BindingPath)}>.+?)' DataItem='(?<{nameof(UwpEntry.DataItemType)}>.+?)'; target element is '(?<{nameof(UwpEntry.TargetElementType)}>.+?)' \(Name='(?<{nameof(UwpEntry.TargetElementName)}>.+?)'\); target property is '(?<{nameof(UwpEntry.TargetProperty)}>.+?)' \(type '(?<{nameof(UwpEntry.TargetPropertyType)}>.+?)'\)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

            this.AddRegex(UwpTraceCode.ConvertFailed,
                $@"Converter failed to convert value of type '(.+?)' to type '(.+?)'");

            this.AddRegex(UwpTraceCode.IntIndexerFailed,
                $@"Cannot get index \[(.+?)\] value \(type '(.+?)'\) from type '(.+?)'");

            this.AddRegex(UwpTraceCode.IntIndexerConnectionFailed,
                $@"Failed to connect to index '(.+?)' in object '(.+?)'");

            this.AddRegex(UwpTraceCode.GetterFailed,
                $@"Cannot get '(.+?)' value \(type '(.+?)'\) from type '(.+?)'");

            this.AddRegex(UwpTraceCode.PropertyConnectionFailed,
                $@"BindingExpression path error: '(.+?)' property not found on '(.+?)'");

            this.AddRegex(UwpTraceCode.SetterFailed,
                $@"Cannot save value from target back to source");
        }

        protected override ITableEntry ProcessLine(Match match)
        {
            string description = match.Groups[UwpOutputParser.CaptureDescription].Value;
            string bindingExpression = match.Groups[UwpOutputParser.CaptureBindingExpression].Value.TrimEnd('.', ' ');

            Match bindingExpressionMatch = this.bindingExpressionRegex.Match(bindingExpression);
            if (bindingExpressionMatch.Success)
            {
                foreach (KeyValuePair<UwpTraceCode, Lazy<Regex>> kvp in this.CodeToRegex)
                {
                    Match descriptionMatch = kvp.Value.Value.Match(description);
                    if (descriptionMatch.Success)
                    {
                        return new UwpEntry(kvp.Key, descriptionMatch, bindingExpressionMatch, this.StringCache);
                    }
                }
            }

            Debug.Fail($"Failed to match UWP binding failure text: {description}");
            return new UwpEntry(UwpTraceCode.None, string.Format(CultureInfo.CurrentCulture, Resource.Uwp_Description_None, description, bindingExpression), this.StringCache);
        }
    }
}
