using System.Globalization;

namespace XamlBinding.Parser
{
    internal enum WpfTraceCode
    {
        None = 0,
        CannotCreateDefaultValueConverter = 1,
        BadValueAtTransfer = 5,
        ClrReplaceItem = 40,
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
