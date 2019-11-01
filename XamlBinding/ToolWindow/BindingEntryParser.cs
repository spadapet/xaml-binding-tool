using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// Converts debug output into a list of binding entries
    /// </summary>
    internal class BindingEntryParser
    {
        private readonly StringCache stringCache;
        private readonly Regex processTextRegex;
        private readonly Regex pathErrorRegex;

        public BindingEntryParser(StringCache stringCache)
        {
            this.stringCache = stringCache;

            this.processTextRegex = new Regex(@"^System.Windows.Data Error: (?<code>\d+) : (?<text>.+?)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Multiline);

            // BindingEntryType.PathError
            this.pathErrorRegex = new Regex($@"BindingExpression path error: '(?<{nameof(BindingEntry.SourceProperty)}>.+?)' property not found on '(object|current item of collection)' '{BindingEntryParser.CaptureItem(nameof(BindingEntry.SourcePropertyType), nameof(BindingEntry.SourcePropertyName))}'. BindingExpression:Path=(?<{nameof(BindingEntry.BindingPath)}>.+?); DataItem={BindingEntryParser.CaptureItem(nameof(BindingEntry.DataItemType), nameof(BindingEntry.DataItemName))}; target element is {BindingEntryParser.CaptureItem(nameof(BindingEntry.TargetElementType), nameof(BindingEntry.TargetElementName))}; target property is '(?<{nameof(BindingEntry.TargetProperty)}>.+?)' \(type '(?<{nameof(BindingEntry.TargetPropertyType)}>.+?)'\)",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        }

        public BindingEntry[] ParseOutput(string text)
        {
            MatchCollection matches = this.processTextRegex.Matches(text);
            if (matches.Count == 0)
            {
                return Array.Empty<BindingEntry>();
            }

            List<BindingEntry> entries = new List<BindingEntry>(matches.Count);

            foreach (Match match in matches)
            {
                BindingEntry entry = null;
                string errorCodeString = match.Groups["code"].Value;

                if (int.TryParse(errorCodeString, out int errorCode))
                {
                    switch (errorCode)
                    {
                        case BindingCodes.PathError:
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

            return entries.ToArray();
        }

        private BindingEntry ProcessPathError(Match match)
        {
            string text = match.Groups["text"].Value;
            Match textMatch = this.pathErrorRegex.Match(text);

            if (!textMatch.Success)
            {
                Debug.Fail($"Failed to parse path error: {text}");
                return null;
            }

            return new BindingEntry(BindingCodes.PathError, textMatch, this.stringCache);
        }

        private BindingEntry ProcessUnknownError(int errorCode, Match match)
        {
            return new BindingEntry(errorCode, match.Groups["text"].Value, this.stringCache);
        }

        private static string CaptureItem(string groupType, string groupName)
        {
            return $@"((?<{groupType}>null)|'(?<{groupType}>.+?)' \(HashCode=.+?\)|'(?<{groupType}>.+?)' \(Name='(?<{groupName}>.*?)'\))";
        }
    }
}
