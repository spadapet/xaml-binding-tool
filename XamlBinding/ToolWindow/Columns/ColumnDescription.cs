using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Columns
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(ColumnNames.Description)]
    internal sealed class ColumnDescription : TableColumnDefinitionBase
    {
        public override string Name => ColumnNames.Description;
        public override double DefaultWidth => 600;
        public override string DisplayName => Resource.Header_Description;
        public override TextWrapping TextWrapping => TextWrapping.Wrap;
    }
}
