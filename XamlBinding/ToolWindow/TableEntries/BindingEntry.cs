using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using XamlBinding.Parser;
using XamlBinding.Resources;
using XamlBinding.ToolWindow.Table;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow.TableEntries
{
    /// <summary>
    /// One entry in the failure list
    /// </summary>
    internal sealed class BindingEntry : ObservableObject, IEquatable<BindingEntry>, ICountedTableEntry, IWpfTableEntry
    {
        public int Code { get; }
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

        public const string SourceFullType = nameof(BindingEntry.SourceFullType);
        public const string TargetFullType = nameof(BindingEntry.TargetFullType);

        private readonly StringCache stringCache;
        private int hashCode;

        object ITableEntry.Identity => this;

        public BindingEntry(int code, Match match, StringCache stringCache)
        {
            this.stringCache = stringCache;
            this.Code = code;
            this.Count = 1;

            if (Constants.IsXamlDesigner)
            {
                this.SourceProperty = nameof(this.SourceProperty);
                this.SourcePropertyType = nameof(this.SourcePropertyType);
                this.SourcePropertyName = nameof(this.SourcePropertyName);
                this.BindingPath = nameof(this.BindingPath);
                this.DataItemType = nameof(this.DataItemType);
                this.DataItemName = nameof(this.DataItemName);
                this.DataValue = nameof(this.DataValue);
                this.TargetElementType = nameof(this.TargetElementType);
                this.TargetElementName = nameof(this.TargetElementName);
                this.TargetProperty = nameof(this.TargetProperty);
                this.TargetPropertyType = nameof(this.TargetPropertyType);
                this.Description = nameof(this.Description);
            }
            else
            {
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
        }

        public BindingEntry(int code, string description, StringCache stringCache)
        {
            this.stringCache = stringCache;
            this.Code = code;
            this.Count = 1;

            this.Description = stringCache.Get(description);
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
        }

        private string CreateDescription(Match match)
        {
            string text;

            switch (this.Code)
            {
                case ErrorCodes.CannotCreateDefaultValueConverter:
                    text = string.Format(CultureInfo.CurrentCulture, Resource.Description_CannotCreateDefaultValueConverter,
                        match.Groups[BindingEntry.SourceFullType].Value,
                        match.Groups[BindingEntry.TargetFullType].Value);
                    break;

                case ErrorCodes.BadValueAtTransfer:
                    text = string.Format(CultureInfo.CurrentCulture, Resource.Description_BadValueAtTransfer, this.DataValue, this.TargetText);
                    break;

                case ErrorCodes.ClrReplaceItem:
                    text = string.Format(CultureInfo.CurrentCulture, Resource.Description_ClrReplaceItem, this.SourceProperty, this.SourcePropertyType);
                    break;

                default:
                    text = match.Value;
                    break;
            }

            return text;
        }

        private string TargetText => !string.IsNullOrEmpty(this.TargetProperty)
            ? this.stringCache.Get(string.Format(CultureInfo.CurrentCulture, Resource.BindingEntry_TargetText, this.TargetElementType, this.TargetProperty, this.TargetPropertyType))
            : string.Empty;

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

                this.hashCode = this.Code.GetHashCode() ^ sb.ToString().GetHashCode();
            }

            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is BindingEntry other && this.Equals(other);
        }

        public bool Equals(BindingEntry other)
        {
            return this.Code == other.Code &&
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
                this.TargetPropertyType == other.TargetPropertyType;
        }

        bool ITableEntry.TryGetValue(string keyName, out object content)
        {
            switch (keyName)
            {
                case StandardTableKeyNames.ErrorSeverity:
                    content = __VSERRORCATEGORY.EC_ERROR;
                    break;

                case ColumnNames.Code:
                    content = this.Code;
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
                    content = this.stringCache.Get(this.Code);
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
