using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using XamlBinding.ToolWindow.Entries;
using XamlBinding.Utility;

namespace XamlBinding.Parser
{
    /// <summary>
    /// Converts UWP's debug output into a list of table entries
    /// </summary>
    internal sealed class UwpOutputParser : IOutputParser
    {
        private readonly StringCache stringCache;
        private readonly Regex processTextRegex;
        private readonly Regex bindingExpressionRegex;
        private readonly Dictionary<UwpTraceCode, Lazy<Regex>> codeToRegex;

        private const string CaptureDescription = "description";
        private const string CaptureBindingExpression = "bindingExpression";

        public UwpOutputParser()
        {
            this.stringCache = new StringCache();

            this.processTextRegex = new Regex($@"^Error: (?<{UwpOutputParser.CaptureDescription}>.+?)(;|\.) BindingExpression: (?<{UwpOutputParser.CaptureBindingExpression}>.+?)\r?$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            this.bindingExpressionRegex = new Regex($@"^Path='(?<{nameof(UwpEntry.BindingPath)}>.+?)' DataItem='(?<{nameof(UwpEntry.DataItemType)}>.+?)'; target element is '(?<{nameof(UwpEntry.TargetElementType)}>.+?)' \(Name='(?<{nameof(UwpEntry.TargetElementName)}>.+?)'\); target property is '(?<{nameof(UwpEntry.TargetProperty)}>.+?)' \(type '(?<{nameof(UwpEntry.TargetPropertyType)}>.+?)'\)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

            this.codeToRegex = new Dictionary<UwpTraceCode, Lazy<Regex>>(Enum.GetNames(typeof(UwpTraceCode)).Length);

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

        IReadOnlyList<ITableEntry> IOutputParser.ParseOutput(string text)
        {
            MatchCollection matches = this.processTextRegex.Matches(text);
            if (matches.Count == 0)
            {
                return Array.Empty<ITableEntry>();
            }

            List<ITableEntry> entries = new List<ITableEntry>(matches.Count);

            foreach (Match match in matches)
            {
                if (this.ProcessLine(match) is ITableEntry entry)
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        void IOutputParser.EntriesCleared()
        {
            this.stringCache.Clear();
        }

        private void AddRegex(UwpTraceCode code, string text)
        {
            this.codeToRegex.Add(code, new Lazy<Regex>(() =>
            {
                return new Regex(text, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
            }));
        }

        private ITableEntry ProcessLine(Match match)
        {
            string description = match.Groups[UwpOutputParser.CaptureDescription].Value;
            string bindingExpression = match.Groups[UwpOutputParser.CaptureBindingExpression].Value.TrimEnd('.', ' ');

            Match bindingExpressionMatch = this.bindingExpressionRegex.Match(bindingExpression);
            if (bindingExpressionMatch.Success)
            {
                foreach (KeyValuePair<UwpTraceCode, Lazy<Regex>> kvp in this.codeToRegex)
                {
                    Match descriptionMatch = kvp.Value.Value.Match(description);
                    if (descriptionMatch.Success)
                    {
                        return new UwpEntry(kvp.Key, descriptionMatch, bindingExpressionMatch, this.stringCache);
                    }
                }
            }

            return null;
        }
    }
}
