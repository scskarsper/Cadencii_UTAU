using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cadencii.UtauSymbolTable
{
    public class SymbolFull:UtauSplitTable
    {
        public string getSplitedSymbol(string prev,string cur, string next)
        {
            return cur;
        }
        public List<string> getOtoNameListFromSymbol(string Symbols)
        {
            List<string> Ret = new List<string>();
            Ret.Add(Symbols);
            return Ret;
        }
        public Dictionary<string, double> getSplitedTable(string Symbols,double fullLength)
        {
            return null;
        }
        
    }
}
