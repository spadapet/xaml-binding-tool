using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Columns
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(ColumnNames.DataContextType)]
    internal sealed class ColumnDataContextType : TableColumnDefinitionBase
    {
        public override string Name => ColumnNames.DataContextType;
        public override double DefaultWidth => 150;
        public override string DisplayName => Resource.Header_DataContextType;
    }
}
