using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using XamlBinding.Parser;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Columns
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(ColumnNames.Code)]
    internal sealed class ColumnCode : TableColumnDefinitionBase
    {
        public override string Name => ColumnNames.Code;
        public override double DefaultWidth => 60;
        public override string DisplayName => Resource.Header_Code;

        public override int CompareContent(ITableEntryHandle leftEntry, ITableEntryHandle rightEntry)
        {
            if (leftEntry.TryGetValue(this.Name, out object leftValue) && leftValue is WpfTraceInfo leftInfo &&
                rightEntry.TryGetValue(this.Name, out object rightValue) && rightValue is WpfTraceInfo rightInfo)
            {
                return leftInfo.CompareTo(rightInfo);
            }

            return base.CompareContent(leftEntry, rightEntry);
        }
    }
}
