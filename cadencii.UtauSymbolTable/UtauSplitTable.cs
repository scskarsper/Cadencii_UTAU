using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cadencii.UtauSymbolTable
{
    public interface UtauSplitTable
    {
        string getSplitedSymbol(string prev, string currentNote, string nextNote);
        List<string> getOtoNameListFromSymbol(string Symbols);
        Dictionary<string, double> getSplitedTable(string Symbols, double fullLength);
    }
}
