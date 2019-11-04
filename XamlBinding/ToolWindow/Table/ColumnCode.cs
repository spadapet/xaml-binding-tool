using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Table
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
