using System.Collections.Concurrent;

namespace XamlBinding.Utility
{
    /// <summary>
    /// Binding error columns are going to contain a lot of duplicate strings.
    /// This class makes sure only one copy of each string stays in memory
    /// </summary>
    internal class StringCache
    {
        private ConcurrentDictionary<string, string> cache;

        public StringCache()
        {
            this.cache = new ConcurrentDictionary<string, string>();
        }

        public string Get(string value, bool trim = true)
        {
            if (value == null)
            {
                return null;
            }

            if (trim)
            {
                value = value.Trim();
            }

            if (value.Length == 0)
            {
                return string.Empty;
            }

            return this.cache.GetOrAdd(value, value);
        }

        public void Clear()
        {
            this.cache.Clear();
        }
    }
}
