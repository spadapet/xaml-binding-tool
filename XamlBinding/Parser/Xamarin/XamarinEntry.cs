using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using XamlBinding.ToolWindow.Columns;
using XamlBinding.ToolWindow.Entries;
using XamlBinding.Utility;

namespace XamlBinding.Parser.Xamarin
{
    /// <summary>
    /// One entry in the failure list
    /// </summary>
    internal sealed class XamarinEntry : EntryBase, IEquatable<XamarinEntry>
    {
        public XamarinTraceCode Code { get; }
        public string BindingPath { get; }
        public string DataItemType { get; }
        public string TargetElementType { get; }
        public string TargetProperty { get; }
        public string TargetPropertyType { get; }
        public string Description { get; }

        private int hashCode;

        public XamarinEntry(XamarinTraceCode code, Match match, StringCache stringCache)
            : base(stringCache)
        {
            this.Code = code;

            this.BindingPath = stringCache.Get(match.Groups[nameof(this.BindingPath)].Value);
            this.DataItemType = stringCache.Get(match.Groups[nameof(this.DataItemType)].Value);
            this.TargetElementType = stringCache.Get(match.Groups[nameof(this.TargetElementType)].Value);
            this.TargetProperty = stringCache.Get(match.Groups[nameof(this.TargetProperty)].Value);
            this.TargetPropertyType = stringCache.Get(match.Groups[nameof(this.TargetPropertyType)].Value);
            this.Description = stringCache.Get(this.CreateDescription(match));
        }

        public XamarinEntry(XamarinTraceCode code, string description, StringCache stringCache)
            : base(stringCache)
        {
            this.Code = code;

            this.BindingPath = string.Empty;
            this.DataItemType = string.Empty;
            this.TargetElementType = string.Empty;
            this.TargetProperty = string.Empty;
            this.TargetPropertyType = string.Empty;
            this.Description = stringCache.Get(description);
        }

        private string CreateDescription(Match descriptionMatch)
        {
            string text = null;

            switch (this.Code)
            {
                case XamarinTraceCode.PropertyNotFound:
                case XamarinTraceCode.BadType:
                case XamarinTraceCode.BadIndex:
                    // TODO: Come up with localized descriptions
                    break;

                default:
                    Debug.Fail($"No description for: XamarinTraceCode.{this.Code}");
                    break;
            }

            return !string.IsNullOrEmpty(text) ? text : descriptionMatch.Value;
        }

        private string TargetText => !string.IsNullOrEmpty(this.TargetProperty) ? $"{this.TargetElementType}.{this.TargetProperty}" : string.Empty;

        public override int GetHashCode()
        {
            if (this.hashCode == 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(this.BindingPath);
                sb.AppendLine(this.DataItemType);
                sb.AppendLine(this.TargetElementType);
                sb.AppendLine(this.TargetProperty);
                sb.AppendLine(this.TargetPropertyType);
                sb.AppendLine(this.Description);

                this.hashCode = this.Code.GetHashCode() ^ sb.ToString().GetHashCode();
            }

            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is XamarinEntry other && this.Equals(other);
        }

        public bool Equals(XamarinEntry other)
        {
            return this.Code == other.Code &&
                this.BindingPath == other.BindingPath &&
                this.DataItemType == other.DataItemType &&
                this.TargetElementType == other.TargetElementType &&
                this.TargetProperty == other.TargetProperty &&
                this.TargetPropertyType == other.TargetPropertyType &&
                this.Description == other.Description;
        }

        public override bool TryGetValue(string keyName, out object content)
        {
            switch (keyName)
            {
                case StandardTableKeyNames.ErrorSeverity:
                    content = __VSERRORCATEGORY.EC_ERROR;
                    break;

                case ColumnNames.Code:
                    content = this.Code;
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
                    content = this.StringCache.Get((int)this.Code);
                    break;

                default:
                    return base.TryCreateStringContent(columnName, truncatedText, singleColumnView, out content);
            }

            return true;
        }
    }
}
