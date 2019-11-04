using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using XamlBinding.Resources;
using XamlBinding.ToolWindow.Parser;
using XamlBinding.ToolWindow.Table;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow.TableEntries
{
    /// <summary>
    /// One entry in the failure list
    /// </summary>
    internal class BindingEntry : ObservableObject, IEquatable<BindingEntry>, ICountedTableEntry, IWpfTableEntry
    {
        public int Code { get; }
        public int Count { get; private set; }
        public string Description { get; }
        public string SourceProperty { get; }
        public string SourcePropertyType { get; }
        public string SourcePropertyName { get; }
        public string BindingPath { get; }
        public string DataItemType { get; }
        public string DataItemName { get; }
        public string TargetElementType { get; }
        public string TargetElementName { get; }
        public string TargetProperty { get; }
        public string TargetPropertyType { get; }

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
                this.Description = nameof(this.Description);
                this.SourceProperty = nameof(this.SourceProperty);
                this.SourcePropertyType = nameof(this.SourcePropertyType);
                this.SourcePropertyName = nameof(this.SourcePropertyName);
                this.BindingPath = nameof(this.BindingPath);
                this.DataItemType = nameof(this.DataItemType);
                this.DataItemName = nameof(this.DataItemName);
                this.TargetElementType = nameof(this.TargetElementType);
                this.TargetElementName = nameof(this.TargetElementName);
                this.TargetProperty = nameof(this.TargetProperty);
                this.TargetPropertyType = nameof(this.TargetPropertyType);
            }
            else
            {
                this.Description = stringCache.Get(match.Value);
                this.SourceProperty = stringCache.Get(match.Groups[nameof(this.SourceProperty)].Value);
                this.SourcePropertyType = stringCache.Get(match.Groups[nameof(this.SourcePropertyType)].Value);
                this.SourcePropertyName = stringCache.Get(match.Groups[nameof(this.SourcePropertyName)].Value);
                this.BindingPath = stringCache.Get(match.Groups[nameof(this.BindingPath)].Value);
                this.DataItemType = stringCache.Get(match.Groups[nameof(this.DataItemType)].Value);
                this.DataItemName = stringCache.Get(match.Groups[nameof(this.DataItemName)].Value);
                this.TargetElementType = stringCache.Get(match.Groups[nameof(this.TargetElementType)].Value);
                this.TargetElementName = stringCache.Get(match.Groups[nameof(this.TargetElementName)].Value);
                this.TargetProperty = stringCache.Get(match.Groups[nameof(this.TargetProperty)].Value);
                this.TargetPropertyType = stringCache.Get(match.Groups[nameof(this.TargetPropertyType)].Value);
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
            this.TargetElementType = string.Empty;
            this.TargetElementName = string.Empty;
            this.TargetProperty = string.Empty;
            this.TargetPropertyType = string.Empty;
        }

        public string DescriptionText
        {
            get
            {
                string text;

                switch (this.Code)
                {
                    case ErrorCodes.PathError:
                        text = string.Format(CultureInfo.CurrentCulture, Resource.Description_PathError, this.SourceProperty, this.SourcePropertyType);
                        break;

                    default:
                        text = this.Description;
                        break;
                }

                return this.stringCache.Get(text);
            }
        }

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
                this.TargetElementType == other.TargetElementType &&
                this.TargetElementName == other.TargetElementName &&
                this.TargetProperty == other.TargetProperty &&
                this.TargetPropertyType == other.TargetPropertyType;
        }

        bool ITableEntry.TryGetValue(string keyName, out object content)
        {
            switch (keyName)
            {
                case StandardTableKeyNames.ErrorSeverityImage:
                    content = KnownMonikers.StatusError;
                    break;

                case TableColumn.ColumnBindingPath:
                    content = this.BindingPath;
                    break;

                case TableColumn.ColumnCode:
                    content = this.stringCache.Get(this.Code);
                    break;

                case TableColumn.ColumnCount:
                    content = this.stringCache.Get(this.Count);
                    break;

                case TableColumn.ColumnDataContextType:
                    content = this.DataItemType;
                    break;

                case TableColumn.ColumnDescription:
                    content = this.DescriptionText;
                    break;

                case TableColumn.ColumnTarget:
                    content = !string.IsNullOrEmpty(this.TargetProperty)
                        ? this.stringCache.Get(string.Format(CultureInfo.CurrentCulture, Resource.TargetText, this.TargetElementType, this.TargetProperty, this.TargetPropertyType))
                        : string.Empty;
                    break;

                default:
                    content = string.Empty;
                    break;
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
            switch (columnName)
            {
                case StandardTableColumnDefinitions.ErrorSeverity:
                    content = KnownMonikers.StatusError;
                    break;

                default:
                    content = default;
                    return false;
            }

            return true;
        }

        bool IWpfTableEntry.TryCreateStringContent(string columnName, bool truncatedText, bool singleColumnView, out string content)
        {
            content = null;
            return false;
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
            toolTip = null;
            return false;
        }
    }
}
