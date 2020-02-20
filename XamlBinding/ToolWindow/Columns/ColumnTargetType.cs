using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Columns
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(ColumnNames.TargetType)]
    internal sealed class ColumnTargetType : TableColumnDefinitionBase
    {
        public override string Name => ColumnNames.TargetType;
        public override double DefaultWidth => 100;
        public override string DisplayName => Resource.Header_TargetType;
    }
}
