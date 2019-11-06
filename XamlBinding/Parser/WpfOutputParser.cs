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
    /// Converts WPF's debug output into a list of table entries
    /// </summary>
    internal sealed class WpfOutputParser : IOutputParser
    {
        private readonly StringCache stringCache;
        private readonly Regex processTextRegex;
        private readonly Dictionary<WpfTraceCode, Regex> codeToRegex;

        private const string CaptureCategory = "category";
        private const string CaptureSeverity = "severity";
        private const string CaptureCode = "code";
        private const string CaptureText = "text";

        public WpfOutputParser(StringCache stringCache)
        {
            this.stringCache = stringCache;

            const RegexOptions overallRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Multiline;
            const RegexOptions lineRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;

            this.processTextRegex = new Regex($@"^System.Windows.(?<{WpfOutputParser.CaptureCategory}>Data|ResourceDictionary) (?<{WpfOutputParser.CaptureSeverity}>.+?): (?<{WpfOutputParser.CaptureCode}>\d+) : (?<{WpfOutputParser.CaptureText}>.+?)$", overallRegexOptions);

            // 1
            Regex regexCannotCreateDefaultValueConverter = new Regex($@"Cannot create default converter to perform '(one-way|two-way)' conversions between types '(?<{WpfEntry.SourceFullType}>.+?)' and '(?<{WpfEntry.TargetFullType}>.+?)'. Consider using Converter property of Binding. {WpfOutputParser.CaptureBindingExpression()}", lineRegexOptions);
            // 5
            Regex regexBadValueAtTransfer = new Regex($@"Value produced by BindingExpression is not valid for target property.; Value='(?<DataValue>.+?)' {WpfOutputParser.CaptureBindingExpression()}", lineRegexOptions);
            // 40
            Regex regexClrReplaceItem = new Regex($@"BindingExpression path error: '(?<{nameof(WpfEntry.SourceProperty)}>.+?)' property not found on '(object|current item of collection)' '{WpfOutputParser.CaptureItem(nameof(WpfEntry.SourcePropertyType), nameof(WpfEntry.SourcePropertyName))}'. {WpfOutputParser.CaptureBindingExpression()}", lineRegexOptions);

            this.codeToRegex = new Dictionary<WpfTraceCode, Regex>()
            {
                { WpfTraceCode.CannotCreateDefaultValueConverter, regexCannotCreateDefaultValueConverter},
                { WpfTraceCode.BadValueAtTransfer, regexBadValueAtTransfer },
                { WpfTraceCode.ClrReplaceItem, regexClrReplaceItem },
            };
        }

        public IReadOnlyList<ITableEntry> ParseOutput(string text)
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

        private ITableEntry ProcessLine(Match match)
        {
            string categoryString = match.Groups[WpfOutputParser.CaptureCategory].Value;
            string severityString = match.Groups[WpfOutputParser.CaptureSeverity].Value;
            string codeString = match.Groups[WpfOutputParser.CaptureCode].Value;
            string text = match.Groups[WpfOutputParser.CaptureText].Value;

            WpfTraceInfo info = new WpfTraceInfo(
                WpfTraceCategoryUtility.Parse(categoryString),
                WpfTraceSeverityUtility.Parse(severityString),
                WpfTraceCodeUtility.Parse(codeString));

            return this.codeToRegex.TryGetValue(info.Code, out Regex regex)
                ? this.ProcessKnownError(info, regex, text)
                : this.ProcessUnknownError(info, text);
        }

        private ITableEntry ProcessKnownError(WpfTraceInfo info, Regex lineRegex, string text)
        {
            Match match = lineRegex.Match(text);
            if (!match.Success)
            {
                Debug.Fail($"Failed to parse error code {(int)info.Code}: {text}");
                return this.ProcessUnknownError(info, text);
            }

            return new WpfEntry(info, match, this.stringCache);
        }

        private ITableEntry ProcessUnknownError(WpfTraceInfo info, string lineText)
        {
            return new WpfEntry(info, lineText, this.stringCache);
        }

        private static string CaptureBindingExpression()
        {
            return $@"((BindingExpression:{WpfOutputParser.CaptureBindingPath()}; {WpfOutputParser.CaptureDataItem()}; )|(MultiBindingExpression:)){WpfOutputParser.CaptureTargetElement()}; {WpfOutputParser.CaptureTargetProperty()}";
        }

        private static string CaptureBindingPath()
        {
            // From TraceData.Describe in Microsoft.DotNet.Wpf\src\PresentationFramework\MS\Internal\TraceData.cs
            return $@"(((Path|XPath)=(?<{nameof(WpfEntry.BindingPath)}>.+?))|\(no path\))";
        }

        private static string CaptureDataItem()
        {
            return $@"DataItem={WpfOutputParser.CaptureItem(nameof(WpfEntry.DataItemType), nameof(WpfEntry.DataItemName))}";
        }

        private static string CaptureTargetElement()
        {
            return $@"target element is {WpfOutputParser.CaptureItem(nameof(WpfEntry.TargetElementType), nameof(WpfEntry.TargetElementName))}";
        }

        private static string CaptureTargetProperty()
        {
            return $@"target property is '(?<{nameof(WpfEntry.TargetProperty)}>.+?)' \(type '(?<{nameof(WpfEntry.TargetPropertyType)}>.+?)'\)";
        }

        private static string CaptureItem(string groupType, string groupName)
        {
            // From TraceData.DescribeSourceObject in Microsoft.DotNet.Wpf\src\PresentationFramework\MS\Internal\TraceData.cs
            return $@"((?<{groupType}>null)|'(?<{groupType}>.+?)' \(HashCode=.+?\)|'(?<{groupType}>.+?)' \(Name='(?<{groupName}>.*?)'\))";
        }
    }
}
