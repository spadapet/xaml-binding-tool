namespace XamlBinding.Parser
{
    internal enum WpfTraceCategory
    {
        None,
        Data, // System.Windows.Data for Bindings
        ResourceDictionary, // System.Windows.ResourceDictionary
    }

    internal static class WpfTraceCategoryUtility
    {
        public static WpfTraceCategory Parse(string categoryText)
        {
            WpfTraceCategory category = WpfTraceCategory.None;

            switch (categoryText ?? string.Empty)
            {
                case nameof(WpfTraceCategory.Data):
                    category = WpfTraceCategory.Data;
                    break;

                case nameof(WpfTraceCategory.ResourceDictionary):
                    category = WpfTraceCategory.ResourceDictionary;
                    break;
            }

            return category;
        }
    }
}
