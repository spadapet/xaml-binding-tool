using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using XamlBinding.Parser;
using XamlBinding.Resources;
using XamlBinding.ToolWindow.Columns;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow.Entries
{
    /// <summary>
    /// One entry in the failure list
    /// </summary>
    internal sealed class WpfEntry : ObservableObject, IEquatable<WpfEntry>, ICountedTableEntry, IWpfTableEntry
    {
        public WpfTraceInfo Info { get; }
        public int Count { get; private set; }
        public string SourceProperty { get; }
        public string SourcePropertyType { get; }
        public string SourcePropertyName { get; }
        public string BindingPath { get; }
        public string DataItemType { get; }
        public string DataItemName { get; }
        public string DataValue { get; }
        public string TargetElementType { get; }
        public string TargetElementName { get; }
        public string TargetProperty { get; }
        public string TargetPropertyType { get; }
        public string Description { get; }

        public const string SourceFullType = nameof(WpfEntry.SourceFullType);
        public const string TargetFullType = nameof(WpfEntry.TargetFullType);

        private readonly StringCache stringCache;
        private int hashCode;

        object ITableEntry.Identity => this;

        public WpfEntry(WpfTraceInfo info, Match match, StringCache stringCache)
        {
            this.stringCache = stringCache;
            this.Info = info;
            this.Count = 1;

            this.SourceProperty = stringCache.Get(match.Groups[nameof(this.SourceProperty)].Value);
            this.SourcePropertyType = stringCache.Get(match.Groups[nameof(this.SourcePropertyType)].Value);
            this.SourcePropertyName = stringCache.Get(match.Groups[nameof(this.SourcePropertyName)].Value);
            this.BindingPath = stringCache.Get(match.Groups[nameof(this.BindingPath)].Value);
            this.DataItemType = stringCache.Get(match.Groups[nameof(this.DataItemType)].Value);
            this.DataItemName = stringCache.Get(match.Groups[nameof(this.DataItemName)].Value);
            this.DataValue = stringCache.Get(match.Groups[nameof(this.DataValue)].Value);
            this.TargetElementType = stringCache.Get(match.Groups[nameof(this.TargetElementType)].Value);
            this.TargetElementName = stringCache.Get(match.Groups[nameof(this.TargetElementName)].Value);
            this.TargetProperty = stringCache.Get(match.Groups[nameof(this.TargetProperty)].Value);
            this.TargetPropertyType = stringCache.Get(match.Groups[nameof(this.TargetPropertyType)].Value);
            this.Description = stringCache.Get(this.CreateDescription(match));
        }

        public WpfEntry(WpfTraceInfo info, string description, StringCache stringCache)
        {
            this.stringCache = stringCache;
            this.Info = info;
            this.Count = 1;

            this.SourceProperty = string.Empty;
            this.SourcePropertyType = string.Empty;
            this.SourcePropertyName = string.Empty;
            this.BindingPath = string.Empty;
            this.DataItemType = string.Empty;
            this.DataItemName = string.Empty;
            this.DataValue = string.Empty;
            this.TargetElementType = string.Empty;
            this.TargetElementName = string.Empty;
            this.TargetProperty = string.Empty;
            this.TargetPropertyType = string.Empty;
            this.Description = stringCache.Get(description);
        }

        private string CreateDescription(Match match)
        {
            string text = null;

            switch (this.Info.Category)
            {
                case WpfTraceCategory.Data:
                    switch (this.Info.Code)
                    {
                        case WpfTraceCode.BadValueAtTransfer:
                            text = string.Format(CultureInfo.CurrentCulture, Resource.Description_BadValueAtTransfer, this.DataValue, this.TargetText, this.TargetPropertyType);
                            break;

                        case WpfTraceCode.CannotCreateDefaultValueConverter:
                            text = string.Format(CultureInfo.CurrentCulture,
                                Resource.Description_CannotCreateDefaultValueConverter,
                                match.Groups[WpfEntry.SourceFullType].Value,
                                match.Groups[WpfEntry.TargetFullType].Value);
                            break;

                        case WpfTraceCode.ClrReplaceItem:
                            text = string.Format(CultureInfo.CurrentCulture, Resource.Description_ClrReplaceItem, this.SourceProperty, this.SourcePropertyType);
                            break;
                    }
                    break;

                case WpfTraceCategory.ResourceDictionary:
                    break;
            }

            return !string.IsNullOrEmpty(text) ? text : match.Value;
        }

        private string TargetText => !string.IsNullOrEmpty(this.TargetProperty) ? $"{this.TargetElementType}.{this.TargetProperty}" : string.Empty;

        public void AddCount(int count = 1)
        {
            this.Count += count;
            this.NotifyPropertyChanged(nameof(this.Count));
        }

        public override int GetHashCode()
        {
            if (this.hashCode == 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(this.SourceProperty);
                sb.AppendLine(this.SourcePropertyType);
                sb.AppendLine(this.SourcePropertyName);
                sb.AppendLine(this.BindingPath);
                sb.AppendLine(this.DataItemType);
                sb.AppendLine(this.DataItemName);
                sb.AppendLine(this.DataValue);
                sb.AppendLine(this.TargetElementType);
                sb.AppendLine(this.TargetElementName);
                sb.AppendLine(this.TargetProperty);
                sb.AppendLine(this.TargetPropertyType);
                sb.AppendLine(this.Description);

                this.hashCode = this.Info.GetHashCode() ^ sb.ToString().GetHashCode();
            }

            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is WpfEntry other && this.Equals(other);
        }

        public bool Equals(WpfEntry other)
        {
            return this.Info == other.Info &&
                this.SourceProperty == other.SourceProperty &&
                this.SourcePropertyType == other.SourcePropertyType &&
                this.SourcePropertyName == other.SourcePropertyName &&
                this.BindingPath == other.BindingPath &&
                this.DataItemType == other.DataItemType &&
                this.DataItemName == other.DataItemName &&
                this.DataValue == other.DataValue &&
                this.TargetElementType == other.TargetElementType &&
                this.TargetElementName == other.TargetElementName &&
                this.TargetProperty == other.TargetProperty &&
                this.TargetPropertyType == other.TargetPropertyType &&
                this.Description == other.Description;
        }

        bool ITableEntry.TryGetValue(string keyName, out object content)
        {
            switch (keyName)
            {
                case StandardTableKeyNames.ErrorSeverity:
                    content = this.Info.Severity.ToVsErrorCategory();
                    break;

                case ColumnNames.Code:
                    content = this.Info;
                    break;

                case ColumnNames.Count:
                    content = this.Count;
                    break;

                case ColumnNames.BindingPath:
                    content = this.BindingPath;
                    break;

                case ColumnNames.DataContextType:
                    content = this.DataItemType;
                    break;

                case ColumnNames.Description:
                    content = this.Description;
                    break;

                case ColumnNames.Target:
                    content = this.TargetText;
                    break;

                case ColumnNames.TargetType:
                    content = this.TargetPropertyType;
                    break;

                default:
                    content = null;
                    return false;
            }

            return true;
        }

        bool ITableEntry.TrySetValue(string keyName, object content)
        {
            return false;
        }

        bool ITableEntry.CanSetValue(string keyName)
        {
            return false;
        }

        bool IWpfTableEntry.TryCreateImageContent(string columnName, bool singleColumnView, out ImageMoniker content)
        {
            content = default;
            return false;
        }

        bool IWpfTableEntry.TryCreateStringContent(string columnName, bool truncatedText, bool singleColumnView, out string content)
        {
            switch (columnName)
            {
                case ColumnNames.Code:
                    content = this.stringCache.Get(this.Info.ToString());
                    break;

                case ColumnNames.Count:
                    content = this.stringCache.Get(this.Count);
                    break;

                default:
                    content = null;
                    return false;
            }

            return true;
        }

        bool IWpfTableEntry.TryCreateColumnContent(string columnName, bool singleColumnView, out FrameworkElement content)
        {
            content = null;
            return false;
        }

        bool IWpfTableEntry.CanCreateDetailsContent()
        {
            return false;
        }

        bool IWpfTableEntry.TryCreateDetailsContent(out FrameworkElement expandedContent)
        {
            expandedContent = null;
            return false;
        }

        bool IWpfTableEntry.TryCreateDetailsStringContent(out string content)
        {
            content = null;
            return false;
        }

        bool IWpfTableEntry.TryCreateToolTip(string columnName, out object toolTip)
        {
            switch (columnName)
            {
                case ColumnNames.BindingPath:
                case ColumnNames.DataContextType:
                case ColumnNames.Description:
                case ColumnNames.Target:
                case ColumnNames.TargetType:
                    if (this.TryGetValue(columnName, out string stringContent) && !string.IsNullOrEmpty(stringContent))
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
