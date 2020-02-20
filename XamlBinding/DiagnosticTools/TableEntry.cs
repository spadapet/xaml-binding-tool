using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using XamlBinding.ToolWindow.Columns;
using XamlBinding.ToolWindow.Entries;
using XamlBinding.Utility;

namespace XamlBinding.DiagnosticTools
{
    /// <summary>
    /// One entry in the failure list
    /// </summary>
    internal sealed class TableEntry : EntryBase, IEquatable<TableEntry>
    {
        public string Code { get; }
        public string BindingPath { get; }
        public string SourceType { get; }
        public string TargetElementType { get; }
        public string TargetElementName { get; }
        public string TargetProperty { get; }
        public string TargetPropertyType { get; }
        public string Description { get; }

        private int hashCode;

        public TableEntry(IEnumerable<KeyValuePair<string, string>> properties, StringCache stringCache)
            : base(stringCache)
        {
            foreach (KeyValuePair<string, string> property in properties)
            {
                switch (property.Key)
                {
                    case nameof(this.Code):
                        this.Code = stringCache.Get(property.Value);
                        break;

                    case "Path":
                        this.BindingPath = stringCache.Get(property.Value);
                        break;

                    case nameof(this.SourceType):
                        this.SourceType = stringCache.Get(property.Value);
                        break;

                    case nameof(this.TargetProperty):
                        this.TargetProperty = stringCache.Get(property.Value);
                        break;

                    case "TargetType":
                        this.TargetPropertyType = stringCache.Get(property.Value);
                        break;

                    case nameof(this.Description):
                        this.Description = stringCache.Get(property.Value);
                        break;
                }
            }
        }

        public override int GetHashCode()
        {
            if (this.hashCode == 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(this.Code);
                sb.AppendLine(this.BindingPath);
                sb.AppendLine(this.SourceType);
                sb.AppendLine(this.TargetElementType);
                sb.AppendLine(this.TargetElementName);
                sb.AppendLine(this.TargetProperty);
                sb.AppendLine(this.TargetPropertyType);
                sb.AppendLine(this.Description);

                this.hashCode = sb.ToString().GetHashCode();
            }

            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is TableEntry other && this.Equals(other);
        }

        public bool Equals(TableEntry other)
        {
            return this.Code == other.Code &&
                this.BindingPath == other.BindingPath &&
                this.SourceType == other.SourceType &&
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
                    content = __VSERRORCATEGORY.EC_ERROR;
                    break;

                case ColumnNames.Code:
                    content = this.Code;
                    break;

                case ColumnNames.BindingPath:
                    content = this.BindingPath;
                    break;

                case ColumnNames.DataContextType:
                    content = this.SourceType;
                    break;

                case ColumnNames.Description:
                    content = this.Description;
                    break;

                case ColumnNames.Target:
                    content = this.TargetProperty;
                    break;

                case ColumnNames.TargetType:
                    content = this.TargetPropertyType;
                    break;

                default:
                    return base.TryGetValue(keyName, out content);
            }

            return true;
        }
    }
}
