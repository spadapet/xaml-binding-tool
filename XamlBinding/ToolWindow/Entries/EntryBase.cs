using System.Windows;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using XamlBinding.ToolWindow.Columns;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow.Entries
{
    /// <summary>
    /// Base class for an entry in the failure list
    /// </summary>
    internal abstract class EntryBase : ObservableObject, ICountedTableEntry, IWpfTableEntry
    {
        public int Count { get; private set; }
        protected StringCache StringCache { get; }

        public EntryBase(StringCache stringCache)
        {
            this.Count = 1;
            this.StringCache = stringCache;
        }

        void ICountedTableEntry.AddCount(int count)
        {
            this.Count += count;
            this.NotifyPropertyChanged(nameof(this.Count));
        }

        object ITableEntry.Identity => this;

        // ITableEntry
        public virtual bool TryGetValue(string keyName, out object content)
        {
            switch (keyName)
            {
                case ColumnNames.Count:
                    content = this.Count;
                    break;

                default:
                    content = null;
                    return false;
            }

            return true;
        }

        // ITableEntry
        public virtual bool TrySetValue(string keyName, object content)
        {
            return false;
        }

        // ITableEntry
        public virtual bool CanSetValue(string keyName)
        {
            return false;
        }

        // IWpfTableEntry
        public virtual bool TryCreateImageContent(string columnName, bool singleColumnView, out ImageMoniker content)
        {
            content = default;
            return false;
        }

        // IWpfTableEntry
        public virtual bool TryCreateStringContent(string columnName, bool truncatedText, bool singleColumnView, out string content)
        {
            switch (columnName)
            {
                case ColumnNames.Count:
                    content = this.StringCache.Get(this.Count);
                    break;

                default:
                    content = null;
                    return false;
            }

            return true;
        }

        // IWpfTableEntry
        public virtual bool TryCreateColumnContent(string columnName, bool singleColumnView, out FrameworkElement content)
        {
            content = null;
            return false;
        }

        // IWpfTableEntry
        public virtual bool CanCreateDetailsContent()
        {
            return false;
        }

        // IWpfTableEntry
        public virtual bool TryCreateDetailsContent(out FrameworkElement expandedContent)
        {
            expandedContent = null;
            return false;
        }

        // IWpfTableEntry
        public virtual bool TryCreateDetailsStringContent(out string content)
        {
            content = null;
            return false;
        }

        // IWpfTableEntry
        public virtual bool TryCreateToolTip(string columnName, out object toolTip)
        {
            switch (columnName)
            {
                case ColumnNames.BindingPath:
                case ColumnNames.DataContextType:
                case ColumnNames.Description:
                case ColumnNames.Target:
                case ColumnNames.TargetType:
                    if (TableEntryExtensions.TryGetValue(this, columnName, out string stringContent) && !string.IsNullOrEmpty(stringContent))
                    {
                        toolTip = stringContent;
                        return true;
                    }
                    break;
            }

            toolTip = null;
            return false;
        }
    }
}
