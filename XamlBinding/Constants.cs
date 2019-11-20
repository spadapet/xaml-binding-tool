using System;
using System.ComponentModel;
using System.Windows;

namespace XamlBinding
{
    internal static class Constants
    {
        // Helpers
        public readonly static bool IsXamlDesigner = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        // Registry
        public const string UserSettingsKey = @"XamlBinding\Settings";
        public const string DataBindingTraceKey = @"Debugger\Tracing\WPF.DataBinding";
        public const string DataBindingTraceLevel = "Level";
        public const string UserIdValue = @"UserId";

        // COM
        public const int S_OK = 0;
        public const int OLECMDERR_E_NOTSUPPORTED = unchecked((int)0x80040100);
        public const int OLECMDF_SUPPORTED = 1;
        public const int OLECMDF_ENABLED = 2;
        public const int OLECMDF_SUPPORTED_AND_ENABLED = 3;

        // Commands from VS
        public const string CommandSet97String = "5efc7975-14bc-11cf-9b2b-00aa00573819";
        public static readonly Guid GuidCommandSet97 = new Guid(Constants.CommandSet97String);
        public const int ToolsOptionsCommandId = 264;

        // Commands in this package
        public const string PackageCommandSetString = "418AB65F-8C3A-470A-8BEF-7DFA5F82364A";
        public static readonly Guid GuidPackageCommandSet = new Guid(Constants.PackageCommandSetString);
        public const int BindingPaneCommandId = 0x0100;

        public const string BindingPaneCommandSetString = "86940B37-818D-4B50-86E6-87A7C3EA77B9";
        public static readonly Guid GuidBindingPaneCommandSet = new Guid(Constants.BindingPaneCommandSetString);
        public const int BindingPaneToolbarId = 0x1000;
        public const int BindingPaneContextMenuId = 0x1001;
        public const int ClearCommandId = 0x0100;
        public const int TraceLevelDropDownId = 0x0101;
        public const int TraceLevelDropDownListId = 0x0102;
        public const int TraceLevelOptionsId = 0x0103;
        public const int ProvideFeedbackId = 0x0104;
        public const int CopyDataContextId = 0x0105;
        public const int CopyBindingPathId = 0x0106;
        public const int CopyTargetId = 0x0107;

        // Per-solution options
        public const string SolutionOptionKey = @"XamlBindingPackage.Options";
        public const string OptionSolutionId = @"SolutionId";

        // Telemetry
        public const string VsBindingPaneFeaturePrefix = @"vs/XamlBindingPanePrototype/";

        // Telemetry events
        public const string EventClearPane = "ClearPane";
        public const string EventClosePane = "ClosePane";
        public const string EventCopyColumnValue = "CopyColumnValue";
        public const string EventDebugEnd = "DebugEnd";
        public const string EventDebugOutputConnected = "DebugOutputConnected";
        public const string EventDebugStart = "DebugStart";
        public const string EventHidePane = "HidePane";
        public const string EventInitializePackage = "InitializePackage";
        public const string EventInitializePane = "InitializePane";
        public const string EventNewEntry = "NewEntry";
        public const string EventFocusChanged = "FocusChanged";
        public const string EventProvideFeedback = "ProvideFeedback";
        public const string EventShowContextMenu = "ShowContextMenu";
        public const string EventShowPane = "ShowPane";
        public const string EventShowTraceOptions = "ShowTraceOptions";
        public const string EventSetTraceLevel = "SetTraceLevel";

        // Telemetry properties
        public const string PropertyColumnValue = "ColumnValue";
        public const string PropertyEntryCount = "EntryCount";
        public const string PropertyErrorCode = "ErrorCode";
        public const string PropertyErrorCodes = "ErrorCodes";
        public const string PropertyExpandedEntryCount = "ExpandedEntryCount";
        public const string PropertyFocused = "Focused";
        public const string PropertySolutionId = "SolutionId";
        public const string PropertyTraceLevel = "TraceLevel";

        // GUIDs
        public const string ApplicationInsightsInstrumentationKeyString = "CA03211C-F990-4E39-A24E-FF5CDC946D9B";
        public const string BindingPackageString = "08F93EBA-7555-4CCB-9CEA-82925FCBE8FF";
        public const string BindingOptionsString = "C828BD8A-8C73-48CE-B9DF-63CB8AF909C4";
        public const string BindingPaneString = "DCCA5C53-7A37-4F9B-BE15-E1063D061497";
        public const string ShowBindingPaneContextString = "C8272E21-A17A-429F-8BEF-325C6D1E8462";
        public const string CallStackWindowString = "34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3";
        public const string TableManagerString = "520311B2-6778-4AD7-A923-BD93F0E19892";
        public const string ToolsOptionsDebugOutputPageString = "82C13516-664F-4F52-8EEE-3F90E80CEA9A";
    }
}
