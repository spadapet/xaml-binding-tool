using System.Collections.Concurrent;
using System.Globalization;

namespace XamlBinding.Utility
{
    /// <summary>
    /// Binding failure columns are going to contain a lot of duplicate strings.
    /// This class makes sure only one copy of each string stays in memory
    /// </summary>
    internal class StringCache
    {
        private readonly ConcurrentDictionary<string, string> stringCache;
        private readonly ConcurrentDictionary<int, string> intCache;

        public StringCache()
        {
            this.stringCache = new ConcurrentDictionary<string, string>();
            this.intCache = new ConcurrentDictionary<int, string>();
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

            return this.stringCache.GetOrAdd(value, value);
        }

        public string Get(int value)
        {
            return this.intCache.GetOrAdd(value, v => v.ToString(CultureInfo.InvariantCulture));
        }

        public void Clear()
        {
            this.stringCache.Clear();
            this.intCache.Clear();
        }
    }
}
