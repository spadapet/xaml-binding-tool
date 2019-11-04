using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace XamlBinding.ToolWindow.Table
{
    /// <summary>
    /// Filters the table based on a search
    /// </summary>
    internal sealed class TableSearchFilter : IEntryFilter
    {
        private readonly List<string> tokens;
        private readonly List<ITableColumnDefinition> columns;

        [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "ParsedTokenText shouldn't need UI thread")]
        public TableSearchFilter(IVsSearchQuery searchQuery, IWpfTableControl control)
        {
            this.tokens = new List<string>();
            this.columns = new List<ITableColumnDefinition>(control.ColumnStates.Count);

            foreach (IVsSearchToken token in SearchUtilities.ExtractSearchTokens(searchQuery) ?? Array.Empty<IVsSearchToken>())
            {
                if (!string.IsNullOrEmpty(token.ParsedTokenText))
                {
                    this.tokens.Add(token.ParsedTokenText);
                }
            }

            foreach (ColumnState2 columnState in control.ColumnStates.OfType<ColumnState2>())
            {
                if (columnState.IsVisible || columnState.GroupingPriority > 0)
                {
                    ITableColumnDefinition definition = control.ColumnDefinitionManager.GetColumnDefinition(columnState.Name);
                    if (definition != null)
                    {
                        this.columns.Add(definition);
                    }
                }
            }
        }

        bool IEntryFilter.Match(ITableEntryHandle entry)
        {
            foreach (string token in this.tokens)
            {
                foreach (ITableColumnDefinition column in this.columns)
                {
                    if (entry.TryCreateStringContent(column, false, false, out string content) &&
                        content != null && content.IndexOf(token, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
