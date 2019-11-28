using Microsoft.VisualStudio.Shell.TableManager;
using System.Collections.Generic;

namespace XamlBinding.Parser
{
    internal interface IOutputParser
    {
        IReadOnlyList<ITableEntry> ParseOutput(string text);
        void EntriesCleared();
    }
}
