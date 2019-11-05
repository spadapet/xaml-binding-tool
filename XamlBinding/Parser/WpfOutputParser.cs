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
        private readonly Dictionary<int, Regex> codeToRegex;

        private const string CaptureCode = "code";
        private const string CaptureText = "text";

        public WpfOutputParser(StringCache stringCache)
        {
            this.stringCache = stringCache;

            const RegexOptions overallRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Multiline;
            const RegexOptions lineRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;

            this.processTextRegex = new Regex($@"^System.Windows.Data Error: (?<{WpfOutputParser.CaptureCode}>\d+) : (?<{WpfOutputParser.CaptureText}>.+?)$", overallRegexOptions);

            // 1
            Regex regexCannotCreateDefaultValueConverter = new Regex($@"Cannot create default converter to perform '(one-way|two-way)' conversions between types '(?<{BindingEntry.SourceFullType}>.+?)' and '(?<{BindingEntry.TargetFullType}>.+?)'. Consider using Converter property of Binding. {WpfOutputParser.CaptureBindingExpression()}", lineRegexOptions);
            // 5
            Regex regexBadValueAtTransfer = new Regex($@"Value produced by BindingExpression is not valid for target property.; Value='(?<DataValue>.+?)' {WpfOutputParser.CaptureBindingExpression()}", lineRegexOptions);
            // 40
            Regex regexClrReplaceItem = new Regex($@"BindingExpression path error: '(?<{nameof(BindingEntry.SourceProperty)}>.+?)' property not found on '(object|current item of collection)' '{WpfOutputParser.CaptureItem(nameof(BindingEntry.SourcePropertyType), nameof(BindingEntry.SourcePropertyName))}'. {WpfOutputParser.CaptureBindingExpression()}", lineRegexOptions);

            this.codeToRegex = new Dictionary<int, Regex>()
            {
                { ErrorCodes.CannotCreateDefaultValueConverter, regexCannotCreateDefaultValueConverter},
                { ErrorCodes.BadValueAtTransfer, regexBadValueAtTransfer },
                { ErrorCodes.ClrReplaceItem, regexClrReplaceItem },
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
                string errorCodeString = match.Groups[WpfOutputParser.CaptureCode].Value;

                ITableEntry entry;
                if (int.TryParse(errorCodeString, out int errorCode) && this.codeToRegex.TryGetValue(errorCode, out Regex lineRegex))
                {
                    entry = this.ProcessKnownError(errorCode, lineRegex, match.Groups[WpfOutputParser.CaptureText].Value);
                }
                else
                {
                    entry = this.ProcessUnknownError(errorCode, match.Groups[WpfOutputParser.CaptureText].Value);
                }

                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        private ITableEntry ProcessKnownError(int errorCode, Regex lineRegex, string lineText)
        {
            Match match = lineRegex.Match(lineText);
            if (!match.Success)
            {
                Debug.Fail($"Failed to parse error code {errorCode}: {lineText}");
                return this.ProcessUnknownError(errorCode, lineText);
            }

            return new BindingEntry(errorCode, match, this.stringCache);
        }

        private ITableEntry ProcessUnknownError(int errorCode, string lineText)
        {
            return new BindingEntry(errorCode, lineText, this.stringCache);
        }

        private static string CaptureItem(string groupType, string groupName)
        {
            // From TraceData.DescribeSourceObject in Microsoft.DotNet.Wpf\src\PresentationFramework\MS\Internal\TraceData.cs
            return $@"((?<{groupType}>null)|'(?<{groupType}>.+?)' \(HashCode=.+?\)|'(?<{groupType}>.+?)' \(Name='(?<{groupName}>.*?)'\))";
        }

        private static string CaptureBindingPath()
        {
            // From TraceData.Describe in Microsoft.DotNet.Wpf\src\PresentationFramework\MS\Internal\TraceData.cs
            return $@"(((Path|XPath)=(?<{nameof(BindingEntry.BindingPath)}>.+?))|\(no path\))";
        }

        private static string CaptureBindingExpression()
        {
            return $@"BindingExpression:{WpfOutputParser.CaptureBindingPath()}; DataItem={WpfOutputParser.CaptureItem(nameof(BindingEntry.DataItemType), nameof(BindingEntry.DataItemName))}; target element is {WpfOutputParser.CaptureItem(nameof(BindingEntry.TargetElementType), nameof(BindingEntry.TargetElementName))}; target property is '(?<{nameof(BindingEntry.TargetProperty)}>.+?)' \(type '(?<{nameof(BindingEntry.TargetPropertyType)}>.+?)'\)";
        }
    }
}
