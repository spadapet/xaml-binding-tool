using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
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
    }
}
