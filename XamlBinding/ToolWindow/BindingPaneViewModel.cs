using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// Keeps the state of the tool window
    /// </summary>
    internal class BindingPaneViewModel : ObservableObject
    {
        public Telemetry Telemetry { get; }
        public BindingPaneController Controller { get; }
        public ObservableCollection<BindingEntry> Entries { get; }
        public bool CanClearEntries => this.entrySet.Count > 0;

        private readonly StringCache stringCache;
        private readonly HashSet<BindingEntry> entrySet;
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
            this.Controller = new BindingPaneController(this);

            this.stringCache = stringCache;
            this.entrySet = new HashSet<BindingEntry>();
            this.Entries = new ObservableCollection<BindingEntry>();
            this.Entries.CollectionChanged += this.OnEntryCollectionChanged;
            this.traceLevel = nameof(BindingTraceLevels.Error);

            if (Constants.IsXamlDesigner)
            {
                this.Entries.Add(new BindingEntry(BindingCodes.PathError, Match.Empty, stringCache));
                this.Entries.Add(new BindingEntry(BindingCodes.PathError, Match.Empty, stringCache));
                this.Entries.Add(new BindingEntry(BindingCodes.PathError, Match.Empty, stringCache));
                this.Entries.Add(new BindingEntry(BindingCodes.Unknown, Match.Empty, stringCache));
                this.Entries.Add(new BindingEntry(BindingCodes.Unknown, Match.Empty, stringCache));
            }
        }

        private void OnEntryCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (this.entrySet.Count < 2)
            {
                this.NotifyPropertyChanged(nameof(this.CanClearEntries));
            }
        }

        public Dictionary<string, object> GetEntryTelemetryProperties()
        {
            return new Dictionary<string, object>()
            {
                {  Constants.PropertyEntryCount, this.entrySet.Count },
                {  Constants.PropertyExpandedEntryCount, this.entrySet.Sum(e => e.Count) },
            };
        }

        public void ClearEntries()
        {
            this.entrySet.Clear();
            this.Entries.Clear();
            this.stringCache.Clear();
        }

        public void AddEntry(BindingEntry entry)
        {
            if (this.entrySet.TryGetValue(entry, out BindingEntry existingEntry))
            {
                existingEntry.AddCount();
            }
            else
            {
                this.entrySet.Add(entry);
                this.Entries.Add(entry);
            }
        }

        public void AddEntries(IEnumerable<BindingEntry> entries)
        {
            foreach (BindingEntry entry in entries)
            {
                this.AddEntry(entry);
            }
        }

        public string TraceLevel
        {
            get => this.traceLevel;
            set => this.SetProperty(ref this.traceLevel, value ?? nameof(BindingTraceLevels.Error));
        }

        public bool IsDebugging
        {
            get => this.isDebugging;
            set => this.SetProperty(ref this.isDebugging, value);
        }
    }
}
