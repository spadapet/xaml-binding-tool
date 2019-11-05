using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Columns
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(ColumnNames.Target)]
    internal sealed class ColumnTarget : TableColumnDefinitionBase
    {
        public override string Name => ColumnNames.Target;
        public override double DefaultWidth => 150;
        public override string DisplayName => Resource.Header_Target;
    }
}
