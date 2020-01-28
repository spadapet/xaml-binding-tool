using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XamlBinding.Utility;

namespace XamlBinding.Parser
{
    internal abstract class OutputParserBase<TEnumCode> : IOutputParser where TEnumCode : System.Enum
    {
        protected StringCache StringCache { get; }
        protected IReadOnlyDictionary<TEnumCode, Lazy<Regex>> CodeToRegex => this.codeToRegex;
        private readonly Regex processTextRegex;
        private readonly Dictionary<TEnumCode, Lazy<Regex>> codeToRegex;

        public OutputParserBase(string processTextPattern)
        {
            this.StringCache = new StringCache();
            this.processTextRegex = new Regex(processTextPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
            this.codeToRegex = new Dictionary<TEnumCode, Lazy<Regex>>(Enum.GetNames(typeof(TEnumCode)).Length);
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
            this.StringCache.Clear();
        }

        protected void AddRegex(TEnumCode code, string text)
        {
            this.codeToRegex.Add(code, new Lazy<Regex>(() =>
            {
                return new Regex(text, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
            }));
        }

        protected abstract ITableEntry ProcessLine(Match match);
    }
}
