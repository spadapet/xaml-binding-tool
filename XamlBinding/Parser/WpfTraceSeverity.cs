using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace XamlBinding.Parser
{
    internal enum WpfTraceSeverity
    {
        Error = __VSERRORCATEGORY.EC_ERROR,
        Warning = __VSERRORCATEGORY.EC_WARNING,
        Message = __VSERRORCATEGORY.EC_MESSAGE,
    }

    internal static class WpfTraceSeverityUtility
    {
        public static WpfTraceSeverity Parse(string text)
        {
            WpfTraceSeverity severity = WpfTraceSeverity.Message;

            switch (text ?? string.Empty)
            {
                case nameof(SourceLevels.Warning):
                    severity = WpfTraceSeverity.Warning;
                    break;

                case nameof(SourceLevels.Critical):
                case nameof(SourceLevels.Error):
                    severity = WpfTraceSeverity.Error;
                    break;
            }

            return severity;
        }

        public static __VSERRORCATEGORY ToVsErrorCategory(this WpfTraceSeverity severity)
        {
            return (__VSERRORCATEGORY)severity;
        }
    }
}
