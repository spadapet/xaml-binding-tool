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

#if false
        From os/onecoreuap/windows/dxaml/xcp/win/agcore.debug/CommonErrors.rc

        TEXT_BINDINGTRACE_CONVERT_FAILED                "Error: Converter failed to convert value of type '%s' to type '%s'; %s. " // %s is the type of the object being converted, %s is the type to which it failed to convert, %s is the binding trace string
        TEXT_BINDINGTRACE_INT_INDEXER_FAILED            "Error: Cannot get index [%u] value (type '%s') from type '%s'. %s." // %u is the index for which we failed, %s is the type of the indexer, %s is the type that implements the indexer, %s is the binding trace string
        TEXT_BINDINGTRACE_INT_INDEXER_CONNECTION_FAILED "Error: Failed to connect to index '%u' in object '%s'. %s" // %u is the index to which we failed to connect, %s is the type of the source object, %s is the binding trace string
        TEXT_BINDINGTRACE_GETTER_FAILED                 "Error: Cannot get '%s' value (type '%s') from type '%s'. %s." // %s is the name of the property for which the getter failed, %s is the type of the property, %s is the type that owns the property, %s is the binding trace string
        TEXT_BINDINGTRACE_PROPERTY_CONNECTION_FAILED    "Error: BindingExpression path error: '%s' property not found on '%s'. %s" // %s is the property that was not found, %s is the type that should own the property, %s is the trace
        TEXT_BINDINGTRACE_SETTER_FAILED                 "Error: Cannot save value from target back to source. %s." // %s is a trace string with the binding expression definition

        TEXT_BINDINGTRACE_BINDINGEXPRESSION_TRACE "BindingExpression: Path='%s' DataItem='%s'; target element is '%s' (Name='%s'); target property is '%s' (type '%s')" // %s is the property path, %s is the type of the data item, %s is the type of the target element, %s is the name of the target element, %s is the target property, %s is the type of the target property
#endif

        public UwpOutputParser()
        {
            this.stringCache = new StringCache();

            this.processTextRegex = new Regex($@"^Error: (?<{UwpOutputParser.CaptureDescription}>.+?)(;|\.) BindingExpression: (?<{UwpOutputParser.CaptureBindingExpression}>.+?)\r?$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            this.bindingExpressionRegex = new Regex($@"^Path='(?<{nameof(UwpEntry.BindingPath)}>.+?)' DataItem='(?<{nameof(UwpEntry.DataItemType)}>.+?)'; target element is '(?<{nameof(UwpEntry.TargetElementType)}>.+?)' \(Name='(?<{nameof(UwpEntry.TargetElementName)}>.+?)'\); target property is '(?<{nameof(UwpEntry.TargetProperty)}>.+?)' \(type '(?<{nameof(UwpEntry.TargetPropertyType)}>.+?)'\)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

            this.codeToRegex = new Dictionary<UwpTraceCode, Lazy<Regex>>(Enum.GetNames(typeof(UwpTraceCode)).Length);

            this.AddRegex(UwpTraceCode.PropertyConnectionFailed,
                $@"BindingExpression path error: '(.+?)' property not found on '(.+?)'");
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
