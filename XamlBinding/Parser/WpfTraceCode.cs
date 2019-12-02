using System.Globalization;

namespace XamlBinding.Parser
{
    internal enum WpfTraceCode
    {
        // Coming next: 2, 17, 101, 6, 104, 80, 89, 107

        None = 0,
        CannotCreateDefaultValueConverter = 1,
        NoSource = 4,
        BadValueAtTransfer = 5,
        NoValueToTransfer = 10,
        MissingInfo = 20,
        NullDataItem = 21,
        // No columns: RefPreviousNotInContext = 36,
        // No columns: RefAncestorTypeNotSpecified = 38,
        ClrReplaceItem = 40,
        NullItem = 41,
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
