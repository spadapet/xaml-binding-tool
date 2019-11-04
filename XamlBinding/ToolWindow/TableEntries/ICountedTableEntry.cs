using Microsoft.VisualStudio.Shell.TableManager;

namespace XamlBinding.ToolWindow.TableEntries
{
    /// <summary>
    /// Any table entry can implement this if it wants to combine multiple duplicate entries into a single entry.
    /// </summary>
    internal interface ICountedTableEntry : ITableEntry
    {
        int Count { get; }
        void AddCount(int count = 1);
    }
}
