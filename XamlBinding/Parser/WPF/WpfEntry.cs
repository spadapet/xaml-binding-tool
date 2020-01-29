using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using XamlBinding.Resources;
using XamlBinding.ToolWindow.Columns;
using XamlBinding.ToolWindow.Entries;
using XamlBinding.Utility;

namespace XamlBinding.Parser.WPF
{
    /// <summary>
    /// One entry in the failure list
    /// </summary>
    internal sealed class WpfEntry : EntryBase, IEquatable<WpfEntry>
    {
        public WpfTraceInfo Info { get; }
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
        public const string ExtraInfo = nameof(WpfEntry.ExtraInfo);
        public const string ExtraInfo2 = nameof(WpfEntry.ExtraInfo2);

        private int hashCode;

        public WpfEntry(WpfTraceInfo info, Match match, StringCache stringCache)
            : base(stringCache)
        {
            this.Info = info;

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
            : base(stringCache)
        {
            this.Info = info;

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
                        case WpfTraceCode.CannotCreateDefaultValueConverter:
                            text = string.Format(CultureInfo.CurrentCulture,
                                Resource.Description_CannotCreateDefaultValueConverter,
                                match.Groups[WpfEntry.SourceFullType].Value,
                                match.Groups[WpfEntry.TargetFullType].Value);
                            break;

                        case WpfTraceCode.NoMentor:
                            text = Resource.Description_NoMentor;
                            break;

                        case WpfTraceCode.NoSource:
                            text = string.Format(CultureInfo.CurrentCulture, Resource.Description_NoSource, match.Groups[WpfEntry.ExtraInfo].Value);
                            break;

                        case WpfTraceCode.BadValueAtTransfer:
                            text = string.Format(CultureInfo.CurrentCulture, Resource.Description_BadValueAtTransfer, this.DataValue, this.TargetText, this.TargetPropertyType);
                            break;

                        case WpfTraceCode.BadConverterForTransfer:
                            text = string.Format(CultureInfo.CurrentCulture, Resource.Description_BadConverterAtTransfer, this.DataValue, match.Groups[WpfEntry.ExtraInfo].Value);
                            break;

                        case WpfTraceCode.NoValueToTransfer:
                            text = Resource.Description_NoValueToTransfer;
                            break;

                        case WpfTraceCode.CannotGetClrRawValue:
                            text = string.Format(CultureInfo.CurrentCulture, Resource.Description_CannotGetClrRawValue, match.Groups[WpfEntry.ExtraInfo].Value);
                            break;

                        case WpfTraceCode.MissingInfo:
                            text = Resource.Description_MissingInfo;
                            break;

                        case WpfTraceCode.NullDataItem:
                            text = Resource.Description_NullDataItem;
                            break;

                        case WpfTraceCode.ClrReplaceItem:
                            text = string.Format(CultureInfo.CurrentCulture, Resource.Description_ClrReplaceItem, this.SourceProperty, this.SourcePropertyType);
                            break;

                        case WpfTraceCode.NullItem:
                            text = string.Format(CultureInfo.CurrentCulture,
                                Resource.Description_NullItem,
                                match.Groups[WpfEntry.ExtraInfo].Value,
                                match.Groups[WpfEntry.ExtraInfo2].Value);
                            break;

                        default:
                            Debug.Fail($"No description for: WpfTraceCode.{this.Info.Code}");
                            break;
                    }
                    break;

                case WpfTraceCategory.ResourceDictionary:
                    break;
            }

            return !string.IsNullOrEmpty(text) ? text : match.Value;
        }

        private string TargetText => !string.IsNullOrEmpty(this.TargetProperty) ? $"{this.TargetElementType}.{this.TargetProperty}" : string.Empty;

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

        public override bool TryGetValue(string keyName, out object content)
        {
            switch (keyName)
            {
                case StandardTableKeyNames.ErrorSeverity:
                    content = this.Info.Severity.ToVsErrorCategory();
                    break;

                case ColumnNames.Code:
                    content = this.Info;
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
                    return base.TryGetValue(keyName, out content);
            }

            return true;
        }

        public override bool TryCreateStringContent(string columnName, bool truncatedText, bool singleColumnView, out string content)
        {
            switch (columnName)
            {
                case ColumnNames.Code:
                    content = this.StringCache.Get(this.Info.ToString());
                    break;

                default:
                    return base.TryCreateStringContent(columnName, truncatedText, singleColumnView, out content);
            }

            return true;
        }
    }
}
