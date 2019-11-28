using Microsoft.VisualStudio.Shell.TableControl;

namespace XamlBinding.ToolWindow.Columns
{
    internal static class ColumnNames
    {
        public const string BindingPath = ColumnNames.Prefix + nameof(ColumnNames.BindingPath);
        public const string Code = ColumnNames.Prefix + nameof(ColumnNames.Code);
        public const string Count = ColumnNames.Prefix + nameof(ColumnNames.Count);
        public const string DataContextType = ColumnNames.Prefix + nameof(ColumnNames.DataContextType);
        public const string Description = ColumnNames.Prefix + nameof(ColumnNames.Description);
        public const string Target = ColumnNames.Prefix + nameof(ColumnNames.Target);
        public const string TargetType = ColumnNames.Prefix + nameof(ColumnNames.TargetType);

        private const string Prefix = "XamlBinding_";

        public static string[] DefaultSet
        {
            get
            {
                return new string[]
                {
                    StandardTableColumnDefinitions.ErrorSeverity,
                    ColumnNames.Code,
                    ColumnNames.Count,
                    ColumnNames.DataContextType,
                    ColumnNames.BindingPath,
                    ColumnNames.Target,
                    ColumnNames.TargetType,
                    ColumnNames.Description,
                };
            }
        }
    }
}
