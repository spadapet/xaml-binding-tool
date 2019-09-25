using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using XamlBinding.Resources;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// Keeps the state of the tool window
    /// </summary>
    internal class BindingPaneViewModel : PropertyNotifier
    {
        public Telemetry Telemetry { get; }
        public IReadOnlyList<BindingEntry> Entries => this.entryList;
        public ICommand ClearCommand => this.clearCommand;
        public const string DefaultTraceLevel = "Error";

        private readonly StringCache stringCache;
        private readonly HashSet<BindingEntry> entrySet;
        private readonly ObservableCollection<BindingEntry> entryList;
        private readonly DelegateCommand clearCommand;
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
            this.entrySet = new HashSet<BindingEntry>();
            this.entryList = new ObservableCollection<BindingEntry>();
            this.entryList.CollectionChanged += this.OnEntryCollectionChanged;
            this.clearCommand = new DelegateCommand(this.UserClearEntries, this.CanClearEntries);
            this.traceLevel = BindingPaneViewModel.DefaultTraceLevel;

            if (Constants.IsXamlDesigner)
            {
                this.entryList.Add(new BindingEntry(BindingErrorCodes.PathError, Match.Empty, stringCache));
                this.entryList.Add(new BindingEntry(BindingErrorCodes.PathError, Match.Empty, stringCache));
                this.entryList.Add(new BindingEntry(BindingErrorCodes.PathError, Match.Empty, stringCache));
                this.entryList.Add(new BindingEntry(BindingErrorCodes.Unknown, Match.Empty, stringCache));
                this.entryList.Add(new BindingEntry(BindingErrorCodes.Unknown, Match.Empty, stringCache));
            }
        }

        private void OnEntryCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (this.entrySet.Count < 2)
            {
                this.clearCommand.UpdateCanExecute();
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
            this.entryList.Clear();
            this.stringCache.Clear();
        }

        private void UserClearEntries()
        {
            this.Telemetry.TrackEvent(Constants.EventClearPane, this.GetEntryTelemetryProperties());
            this.ClearEntries();
        }

        private bool CanClearEntries()
        {
            return this.entrySet.Count > 0;
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
                this.entryList.Add(entry);
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

            set
            {
                if (this.SetPropertyValue(ref this.traceLevel, value ?? BindingPaneViewModel.DefaultTraceLevel))
                {
                    this.OnPropertyChanged(nameof(this.TraceLevelText));
                }
            }
        }

        public string TraceLevelText => string.Format(CultureInfo.CurrentCulture, Resource.ToolWindow_TraceLevel, this.TraceLevel);

        public bool IsDebugging
        {
            get => this.isDebugging;
            set => this.SetPropertyValue(ref this.isDebugging, value);
        }
    }
}
