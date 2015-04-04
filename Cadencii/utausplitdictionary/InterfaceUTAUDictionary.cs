using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cadencii.utausplitdictionary
{
    public interface InterfaceUTAUDictionary
    {
        string getSymbolString(string pre, string cur, string next);
        int getMinSplitNoteLength();
    }
}
