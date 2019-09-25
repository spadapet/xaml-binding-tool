using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using XamlBinding.Resources;
using XamlBinding.Utility;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// One entry in the error list
    /// </summary>
    internal class BindingEntry : PropertyNotifier, IEquatable<BindingEntry>
    {
        public int ErrorCode { get; }
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

        private StringCache stringCache;
        private int hashCode;

        public BindingEntry(int errorCode, Match match, StringCache stringCache)
        {
            this.stringCache = stringCache;
            this.ErrorCode = errorCode;
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

        public BindingEntry(int errorCode, string description, StringCache stringCache)
        {
            this.stringCache = stringCache;
            this.ErrorCode = errorCode;
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

        public string SourceText => !string.IsNullOrEmpty(this.SourceProperty)
            ? this.stringCache.Get(string.Format(CultureInfo.CurrentCulture, Resource.SourceText, this.SourceProperty, this.SourcePropertyType))
            : string.Empty;

        public string TargetText => !string.IsNullOrEmpty(this.TargetProperty)
            ? this.stringCache.Get(string.Format(CultureInfo.CurrentCulture, Resource.TargetText, this.TargetElementType, this.TargetProperty, this.TargetPropertyType))
            : string.Empty;

        public string EntryTypeText => this.stringCache.Get("BIND" + this.ErrorCode.ToString("0000", CultureInfo.InvariantCulture));

        public string DescriptionText
        {
            get
            {
                string text;

                switch (this.ErrorCode)
                {
                    case BindingErrorCodes.PathError:
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
            this.OnPropertyChanged(nameof(this.Count));
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

                this.hashCode = this.ErrorCode.GetHashCode() ^ sb.ToString().GetHashCode();
            }

            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is BindingEntry other && this.Equals(other);
        }

        public bool Equals(BindingEntry other)
        {
            return this.ErrorCode == other.ErrorCode &&
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
    }
}
