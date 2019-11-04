using System.Collections.Generic;

namespace XamlBinding.ToolWindow.TableEntries
{
    internal class CountedTableEntryComparer : IEqualityComparer<ICountedTableEntry>
    {
        bool IEqualityComparer<ICountedTableEntry>.Equals(ICountedTableEntry x, ICountedTableEntry y)
        {
            return (x?.Identity is object value1 && y?.Identity is object value2) ? value1.Equals(value2) : false;
        }

        int IEqualityComparer<ICountedTableEntry>.GetHashCode(ICountedTableEntry obj)
        {
            return (obj?.Identity is object value) ? value.GetHashCode() : 0;
        }
    }
}
