using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using XamlBinding.Parser;
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

        public BindingPaneViewModel()
            : this(new Telemetry(), new StringCache())
        {
            Debug.Assert(Constants.IsXamlDesigner);
        }

        public BindingPaneViewModel(Telemetry telemetry, StringCache stringCache)
        {
            this.Telemetry = telemetry;
            this.stringCache = stringCache;
            this.countedEntries = new HashSet<ICountedTableEntry>(new CountedTableEntryComparer());
            this.entries = new ObservableCollection<ITableEntry>();
            this.entries.CollectionChanged += this.OnEntryCollectionChanged;
            this.traceLevel = nameof(TraceLevels.Error);

            if (Constants.IsXamlDesigner)
            {
                this.entries.Add(new BindingEntry(ErrorCodes.ClrReplaceItem, Match.Empty, stringCache));
                this.entries.Add(new BindingEntry(ErrorCodes.ClrReplaceItem, Match.Empty, stringCache));
                this.entries.Add(new BindingEntry(ErrorCodes.ClrReplaceItem, Match.Empty, stringCache));
                this.entries.Add(new BindingEntry(ErrorCodes.Unknown, Match.Empty, stringCache));
                this.entries.Add(new BindingEntry(ErrorCodes.Unknown, Match.Empty, stringCache));
            }
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
                HashSet<int> codes = new HashSet<int>(this.entries.Count);

                foreach (ITableEntry entry in this.entries)
                {
                    if (entry.TryGetValue(ColumnNames.Code, out int code))
                    {
                        codes.Add(code);
                    }
                }

                List<int> codeList = codes.ToList();
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
            set => this.SetProperty(ref this.traceLevel, value ?? nameof(TraceLevels.Error));
        }

        public bool IsDebugging
        {
            get => this.isDebugging;
            set => this.SetProperty(ref this.isDebugging, value);
        }
    }
}
