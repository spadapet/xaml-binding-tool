using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using XamlBinding.ToolWindow.TableEntries;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow.Parser
{
    /// <summary>
    /// Converts debug output into a list of table entries
    /// </summary>
    internal class OutputParser
    {
        private readonly StringCache stringCache;
        private readonly Regex processTextRegex;
        private readonly Regex pathErrorRegex;

        private const string CaptureCode = "code";
        private const string CaptureText = "text";

        public OutputParser(StringCache stringCache)
        {
            this.stringCache = stringCache;

            this.processTextRegex = new Regex($@"^System.Windows.Data Error: (?<{OutputParser.CaptureCode}>\d+) : (?<{OutputParser.CaptureText}>.+?)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Multiline);

            this.pathErrorRegex = new Regex($@"BindingExpression path error: '(?<{nameof(BindingEntry.SourceProperty)}>.+?)' property not found on '(object|current item of collection)' '{OutputParser.CaptureItem(nameof(BindingEntry.SourcePropertyType), nameof(BindingEntry.SourcePropertyName))}'. BindingExpression:Path=(?<{nameof(BindingEntry.BindingPath)}>.+?); DataItem={OutputParser.CaptureItem(nameof(BindingEntry.DataItemType), nameof(BindingEntry.DataItemName))}; target element is {OutputParser.CaptureItem(nameof(BindingEntry.TargetElementType), nameof(BindingEntry.TargetElementName))}; target property is '(?<{nameof(BindingEntry.TargetProperty)}>.+?)' \(type '(?<{nameof(BindingEntry.TargetPropertyType)}>.+?)'\)",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
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
                ITableEntry entry = null;
                string errorCodeString = match.Groups[OutputParser.CaptureCode].Value;

                if (int.TryParse(errorCodeString, out int errorCode))
                {
                    switch (errorCode)
                    {
                        case ErrorCodes.PathError:
                            entry = this.ProcessPathError(match);
                            break;

                        default:
                            entry = this.ProcessUnknownError(errorCode, match);
                            break;
                    }
                }

                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        private BindingEntry ProcessPathError(Match match)
        {
            string text = match.Groups[OutputParser.CaptureText].Value;
            Match textMatch = this.pathErrorRegex.Match(text);

            if (!textMatch.Success)
            {
                Debug.Fail($"Failed to parse path error: {text}");
                return null;
            }

            return new BindingEntry(ErrorCodes.PathError, textMatch, this.stringCache);
        }

        private BindingEntry ProcessUnknownError(int errorCode, Match match)
        {
            return new BindingEntry(errorCode, match.Groups[OutputParser.CaptureText].Value, this.stringCache);
        }

        private static string CaptureItem(string groupType, string groupName)
        {
            return $@"((?<{groupType}>null)|'(?<{groupType}>.+?)' \(HashCode=.+?\)|'(?<{groupType}>.+?)' \(Name='(?<{groupName}>.*?)'\))";
        }
    }
}
