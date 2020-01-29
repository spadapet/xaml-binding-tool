using System;
using System.Globalization;

namespace XamlBinding.Parser.WPF
{
    /// <summary>
    /// Just combines category, severity, and code into one struct that allows comparison
    /// </summary>
    internal struct WpfTraceInfo : IEquatable<WpfTraceInfo>, IComparable<WpfTraceInfo>, IComparable
    {
        public WpfTraceCategory Category { get; }
        public WpfTraceSeverity Severity { get; }
        public WpfTraceCode Code { get; }

        public WpfTraceInfo(WpfTraceCategory category, WpfTraceSeverity severity, WpfTraceCode code)
        {
            this.Category = category;
            this.Severity = severity;
            this.Code = code;
        }

        public override string ToString()
        {
            string text = ((int)this.Code).ToString(CultureInfo.InvariantCulture);

            switch (this.Category)
            {
                case WpfTraceCategory.ResourceDictionary:
                    text = "RD" + text;
                    break;
            }

            return text;
        }

        public override int GetHashCode()
        {
            return ((int)this.Category << 24) | ((int)this.Severity << 16) | (int)this.Code;
        }

        public override bool Equals(object obj)
        {
            return obj is WpfTraceInfo other && this.Equals(other);
        }

        public bool Equals(WpfTraceInfo other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public int CompareTo(WpfTraceInfo other)
        {
            return this.GetHashCode().CompareTo(other.GetHashCode());
        }

        public int CompareTo(object other)
        {
            return this.GetHashCode().CompareTo(other.GetHashCode());
        }

        public static bool operator ==(WpfTraceInfo left, WpfTraceInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WpfTraceInfo left, WpfTraceInfo right)
        {
            return !left.Equals(right);
        }
    }
}
