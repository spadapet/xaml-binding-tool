using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Table
{
    /// <summary>
    /// Provides column data to the table from a list of binding entries
    /// </summary>
    internal sealed class TableDataSource : ITableDataSource, IDisposable
    {
        string ITableDataSource.SourceTypeIdentifier => Constants.TableManagerString;
        string ITableDataSource.Identifier => Constants.TableManagerString;
        string ITableDataSource.DisplayName => Resource.ToolWindow_Title;
        private readonly ConcurrentDictionary<Subscription, bool> subscriptions;
        private readonly IReadOnlyList<ITableEntry> entryList;

        public TableDataSource(IReadOnlyList<ITableEntry> entryList)
        {
            this.subscriptions = new ConcurrentDictionary<Subscription, bool>();
            this.entryList = entryList;

            if (this.entryList is ObservableCollection<ITableEntry> observableList)
            {
                observableList.CollectionChanged += this.OnCollectionChanged;
            }
        }

        public void Dispose()
        {
            if (this.entryList is ObservableCollection<ITableEntry> observableList)
            {
                observableList.CollectionChanged -= this.OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            ITableEntry[] oldItems = args.OldItems?.OfType<ITableEntry>().ToArray() ?? Array.Empty<ITableEntry>();
            ITableEntry[] newItems = args.NewItems?.OfType<ITableEntry>().ToArray() ?? Array.Empty<ITableEntry>();

            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (Subscription subscription in this.subscriptions.Keys.ToList())
                {
                    subscription.Sink.RemoveAllEntries();
                }
            }
            else if (oldItems.Length > 0 || newItems.Length > 0)
            {
                foreach (Subscription subscription in this.subscriptions.Keys.ToList())
                {
                    subscription.Sink.ReplaceEntries(oldItems, newItems);
                }
            }
        }

        IDisposable ITableDataSource.Subscribe(ITableDataSink sink)
        {
            Subscription subscription = new Subscription(this, sink);
            this.subscriptions.TryAdd(subscription, true);

            sink.AddEntries(this.entryList, removeAllEntries: true);

            return subscription;
        }

        private void Unsubscribe(Subscription subscription)
        {
            this.subscriptions.TryRemove(subscription, out _);
        }

        private class Subscription : IDisposable
        {
            public ITableDataSink Sink { get; }
            private readonly TableDataSource source;

            public Subscription(TableDataSource source, ITableDataSink sink)
            {
                this.Sink = sink;
                this.source = source;
            }

            void IDisposable.Dispose()
            {
                this.source.Unsubscribe(this);
            }
        }
    }
}
