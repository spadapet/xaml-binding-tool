using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Table
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(ColumnNames.BindingPath)]
    internal sealed class ColumnBindingPath : TableColumnDefinitionBase
    {
        public override string Name => ColumnNames.BindingPath;
        public override double DefaultWidth => 150;
        public override string DisplayName => Resource.Header_BindingPath;
    }
}
