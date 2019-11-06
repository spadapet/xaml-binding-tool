using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using XamlBinding.ToolWindow.Columns;
using XamlBinding.ToolWindow.Entries;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// Keeps the state of the tool window
    /// </summary>
    internal sealed class BindingPaneViewModel : ObservableObject, IDisposable
    {
        public Telemetry Telemetry { get; }
        public IReadOnlyList<ITableEntry> Entries => this.entries;
        public bool CanClearEntries => this.entries.Count > 0;

        private readonly StringCache stringCache;
        private readonly ObservableCollection<ITableEntry> entries;
        private readonly HashSet<ICountedTableEntry> countedEntries;
        private string traceLevel;
        private bool isDebugging;

        public BindingPaneViewModel(Telemetry telemetry, StringCache stringCache)
        {
            this.Telemetry = telemetry;
            this.stringCache = stringCache;
            this.countedEntries = new HashSet<ICountedTableEntry>(new CountedTableEntryComparer());
            this.entries = new ObservableCollection<ITableEntry>();
            this.entries.CollectionChanged += this.OnEntryCollectionChanged;
            this.traceLevel = nameof(Parser.WpfTraceLevel.Error);
        }

        public void Dispose()
        {
            this.entries.CollectionChanged -= this.OnEntryCollectionChanged;
        }

        private void OnEntryCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (this.entries.Count < 2)
            {
                this.NotifyPropertyChanged(nameof(this.CanClearEntries));
            }
        }

        private int ExpandedEntryCount
        {
            get
            {
                int count = this.entries.Count;

                foreach (ITableEntry entry in this.entries)
                {
                    if (entry is ICountedTableEntry countedEntry)
                    {
                        count += countedEntry.Count - 1;
                    }
                }

                return count;
            }
        }

        public Dictionary<string, object> GetEntryTelemetryProperties(bool includeErrorCodes = false)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { Constants.PropertyEntryCount, this.entries.Count },
                { Constants.PropertyExpandedEntryCount, this.ExpandedEntryCount },
                { Constants.PropertyTraceLevel, this.traceLevel },
            };

            if (includeErrorCodes)
            {
                HashSet<string> codes = new HashSet<string>(this.entries.Count);

                foreach (IWpfTableEntry entry in this.entries.OfType<IWpfTableEntry>())
                {
                    if (entry.TryCreateStringContent(ColumnNames.Code, false, false, out string code))
                    {
                        codes.Add(code);
                    }
                }

                List<string> codeList = codes.ToList();
                codeList.Sort();

                string codeString = string.Join(",", codeList);
                properties[Constants.PropertyErrorCodes] = codeString;
            }

            return properties;
        }

        public void ClearEntries()
        {
            this.countedEntries.Clear();
            this.entries.Clear();
            this.stringCache.Clear();
        }

        public void AddEntry(ITableEntry entry)
        {
            ICountedTableEntry countedEntry = entry as ICountedTableEntry;

            if (countedEntry != null && this.countedEntries.TryGetValue(countedEntry, out ICountedTableEntry existingCountedEntry))
            {
                existingCountedEntry.AddCount();

                int i = this.entries.IndexOf(entry);
                Debug.Assert(i != -1, $"{nameof(this.countedEntries)} has extra entries in it.");

                if (i != -1)
                {
                    // Tell the table that this entry has been updated
                    this.entries[i] = existingCountedEntry;
                }
            }
            else
            {
                if (countedEntry != null)
                {
                    this.countedEntries.Add(countedEntry);
                }

                this.entries.Add(entry);
            }
        }

        public void AddEntries(IEnumerable<ITableEntry> entries)
        {
            foreach (ITableEntry entry in entries)
            {
                this.AddEntry(entry);
            }
        }

        public string TraceLevel
        {
            get => this.traceLevel;
            set => this.SetProperty(ref this.traceLevel, value ?? nameof(Parser.WpfTraceLevel.Error));
        }

        public bool IsDebugging
        {
            get => this.isDebugging;
            set => this.SetProperty(ref this.isDebugging, value);
        }
    }
}
