using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.TableManager;

namespace XamlBinding.Parser
{
    internal interface IOutputParser
    {
        IReadOnlyList<ITableEntry> ParseOutput(string text);
        void EntriesCleared();
    }
}
