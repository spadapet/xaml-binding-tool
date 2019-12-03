using System.Globalization;

namespace XamlBinding.Parser
{
    internal enum WpfTraceCode
    {
        // "No columns" means that the text doesn't contain anything that can be extracted into columns.
        // "Verbose message" means that PresentationTraceSources.TraceLevel=High is set on the Binding and extra trace message are output that don't contain anything to parse into columns.

        // Coming next: 11, 12, 

        None = 0,
        CannotCreateDefaultValueConverter = 1,
        NoMentor = 2,
        NoSource = 4,
        BadValueAtTransfer = 5,
        BadConverterForTransfer = 6,
        NoValueToTransfer = 10,
        CannotGetClrRawValue = 17,
        MissingInfo = 20,
        NullDataItem = 21,
        // No columns: RefPreviousNotInContext = 36,
        // No columns: RefAncestorTypeNotSpecified = 38,
        ClrReplaceItem = 40,
        NullItem = 41,
        // Verbose message: 56
        // Verbose message: 58
        // Verbose message: 61
        // Verbose message: 62
        // Verbose message: 67
        // Verbose message: 70
        // Verbose message: 78
        // Verbose message: 80
        // Verbose message: 89
        // Verbose message: 101
        // Verbose message: 104
        // Verbose message: 107
        // Verbose message: 108
    }

    internal static class WpfTraceCodeUtility
    {
        public static WpfTraceCode Parse(string text)
        {
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int textInt)
                ? (WpfTraceCode)textInt
                : WpfTraceCode.None;
        }
    }
}
