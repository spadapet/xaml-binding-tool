using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Windows;
using XamlBinding.Resources;

namespace XamlBinding.ToolWindow.Table
{
    /// <summary>
    /// Represents one column in the table (default width, image, etc).
    /// Also there are helpers for getting all columns.
    /// </summary>
    internal class TableColumn : TableColumnDefinitionBase
    {
        public const string ColumnBindingPath = TableColumn.ColumnPrefix + nameof(TableColumn.ColumnBindingPath);
        public const string ColumnCode = TableColumn.ColumnPrefix + nameof(TableColumn.ColumnCode);
        public const string ColumnCount = TableColumn.ColumnPrefix + nameof(TableColumn.ColumnCount);
        public const string ColumnDataContextType = TableColumn.ColumnPrefix + nameof(TableColumn.ColumnDataContextType);
        public const string ColumnDescription = TableColumn.ColumnPrefix + nameof(TableColumn.ColumnDescription);
        public const string ColumnTarget = TableColumn.ColumnPrefix + nameof(TableColumn.ColumnTarget);
        private const string ColumnPrefix = "XamlBinding.Table.";

        public override string Name { get; }
        public override bool DefaultVisible => this.DefaultVisibleOverride ?? base.DefaultVisible;
        public override bool IsCopyable => this.IsCopyableOverride ?? base.IsCopyable;
        public override bool IsFilterable => this.IsFilterableOverride ?? base.IsFilterable;
        public override bool IsHideable => this.IsHideableOverride ?? base.IsHideable;
        public override bool IsMovable => this.IsMovableOverride ?? base.IsMovable;
        public override bool IsResizable => this.IsResizableOverride ?? base.IsResizable;
        public override bool IsSortable => this.IsSortableOverride ?? base.IsSortable;
        public override double DefaultWidth => this.DefaultWidthOverride ?? base.DefaultWidth;
        public override double MaxWidth => this.MaxWidthOverride ?? base.MaxWidth;
        public override double MinWidth => this.MinWidthOverride ?? base.MinWidth;
        public override ImageMoniker DisplayImage => this.DisplayImageOverride ?? base.DisplayImage;
        public override string DisplayName => this.DisplayNameOverride ?? base.DisplayName;
        public override StringComparer Comparer => this.ComparerOverride ?? base.Comparer;
        public override TextWrapping TextWrapping => this.TextWrappingOverride ?? base.TextWrapping;

        private bool? DefaultVisibleOverride { get; set; }
        private bool? IsCopyableOverride { get; set; }
        private bool? IsFilterableOverride { get; set; }
        private bool? IsHideableOverride { get; set; }
        private bool? IsMovableOverride { get; set; }
        private bool? IsResizableOverride { get; set; }
        private bool? IsSortableOverride { get; set; }
        private double? DefaultWidthOverride { get; set; }
        private double? MaxWidthOverride { get; set; }
        private double? MinWidthOverride { get; set; }
        private ImageMoniker? DisplayImageOverride { get; set; }
        private string DisplayNameOverride { get; set; }
        private StringComparer ComparerOverride { get; set; }
        private TextWrapping? TextWrappingOverride { get; set; }

        private TableColumn(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// These are all the columns that show up in the table
        /// </summary>
        public static IEnumerable<string> AllColumnNames
        {
            get
            {
                yield return StandardTableColumnDefinitions.ErrorSeverity;
                yield return TableColumn.ColumnCode;
                yield return TableColumn.ColumnCount;
                yield return TableColumn.ColumnDataContextType;
                yield return TableColumn.ColumnBindingPath;
                yield return TableColumn.ColumnTarget;
                yield return TableColumn.ColumnDescription;
            }
        }

        /// <summary>
        /// These are all columns that are defined in this module
        /// </summary>
        private static IEnumerable<TableColumn> ColumnDefinitions
        {
            get
            {
                yield return new TableColumn(TableColumn.ColumnCode)
                {
                    DisplayNameOverride = Resource.Header_Code,
                    DefaultWidthOverride = 60,
                };

                yield return new TableColumn(TableColumn.ColumnCount)
                {
                    DisplayNameOverride = Resource.Header_Count,
                    DefaultWidthOverride = 60,
                };

                yield return new TableColumn(TableColumn.ColumnDataContextType)
                {
                    DisplayNameOverride = Resource.Header_DataContextType,
                    DefaultWidthOverride = 150,
                };

                yield return new TableColumn(TableColumn.ColumnBindingPath)
                {
                    DisplayNameOverride = Resource.Header_BindingPath,
                    DefaultWidthOverride = 150,
                };

                yield return new TableColumn(TableColumn.ColumnTarget)
                {
                    DisplayNameOverride = Resource.Header_Target,
                    DefaultWidthOverride = 150,
                };

                yield return new TableColumn(TableColumn.ColumnDescription)
                {
                    DisplayNameOverride = Resource.Header_Description,
                    TextWrappingOverride = TextWrapping.Wrap,
                    DefaultWidthOverride = 600,
                };
            }
        }

        public static IDisposable RegisterColumnDefinitions(ITableColumnDefinitionManager tableControlProvider)
        {
            List<string> names = new List<string>();

            foreach (TableColumn columnDefinition in TableColumn.ColumnDefinitions)
            {
                names.Add(columnDefinition.Name);
                tableControlProvider.AddColumnDefinition(columnDefinition);
            }

            return new OnDisposeActionDisposable(() =>
            {
                foreach (string name in names)
                {
                    tableControlProvider.RemoveColumnDefinition(name);
                }
            });
        }
    }
}
