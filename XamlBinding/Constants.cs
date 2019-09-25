using System;
using System.ComponentModel;
using System.Windows;

namespace XamlBinding
{
    internal static class Constants
    {
        // Helpers
        public readonly static bool IsXamlDesigner = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        // Commands from VS
        public const string CommandSet97String = "5efc7975-14bc-11cf-9b2b-00aa00573819";
        public static readonly Guid GuidCommandSet97 = new Guid(Constants.CommandSet97String);
        public const string ToolsOptionsDebugOutputPageString = "82c13516-664f-4f52-8eee-3f90e80cea9a";
        public const int ToolsOptionsCommandId = 264;

        // Commands in this package
        public const string CommandSetString = "418AB65F-8C3A-470A-8BEF-7DFA5F82364A";
        public static readonly Guid GuidCommandSet = new Guid(Constants.CommandSetString);
        public const int BindingToolWindowCommandId = 0x0100;

        // Per-solution options
        public const string SolutionOptionKey = @"XamlBindingPackage.Options";
        public const string OptionSolutionId = @"SolutionId";

        // Telemetry
        public const string ApplicationInsightsInstrumentationKeyString = "CA03211C-F990-4E39-A24E-FF5CDC946D9B";

        // Telemetry events
        public const string EventActivatePane = "ActivatePane";
        public const string EventClearPane = "ClearPane";
        public const string EventClosePane = "ClosePane";
        public const string EventDebugEnd = "DebugEnd";
        public const string EventDebugOutputConnected = "DebugOutputConnected";
        public const string EventDebugStart = "DebugStart";
        public const string EventHidePane = "HidePane";
        public const string EventInitializePackage = "InitializePackage";
        public const string EventInitializePane = "InitializePane";
        public const string EventListViewFocusChanged = "ListViewFocusChanged";
        public const string EventShowPane = "ShowPane";

        // Telemetry properties
        public const string PropertyEntryCount = "EntryCount";
        public const string PropertyFocused = "Focused";
        public const string PropertyExpandedEntryCount = "ExpandedEntryCount";

        // GUIDs

        public const string PackageString = "08F93EBA-7555-4CCB-9CEA-82925FCBE8FF";
        public static readonly Guid GuidPackage = new Guid(Constants.PackageString);

        public const string BindingToolWindowString = "DCCA5C53-7A37-4F9B-BE15-E1063D061497";
        public static readonly Guid GuidBindingToolWindow = new Guid(Constants.BindingToolWindowString);

        public const string BindingShowToolWindowString = "C8272E21-A17A-429F-8BEF-325C6D1E8462";
        public static readonly Guid GuidBindingShowToolWindow = new Guid(Constants.BindingShowToolWindowString);

        public const string ImageSetString = "2E47D056-94F1-4C81-920F-42B390524FE5";
        public static readonly Guid GuidImageSet = new Guid(Constants.ImageSetString);
    }
}
