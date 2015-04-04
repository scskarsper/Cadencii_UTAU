using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cadencii.vsq;
using cadencii;
using cadencii.utau;
using System.IO;

namespace cadencii.UtauSymbolTable
{
        /// <summary>
        /// 歌詞から発音記号列を引き当てるための辞書を表現するクラス
        /// </summary>
    public class UtauSymbolTable
    {
        public static bool CheckSymbolExists(string symbol, cadencii.utau.UtauVoiceDB db, int note)
        {
            if (symbol == "-" || symbol == "—")
            {
                return true;
            }
            List<string> OtoNames = new List<string>();
            if (symbol.IndexOf(',') > 0)
            {
                OtoNames.AddRange(symbol.Split(','));
            }
            else
            {
                OtoNames.Add(symbol);
            }
            bool Ret = true;
            foreach (string lyric in OtoNames)
            {
                OtoArgs oa = db.attachFileNameFromLyric(lyric, note);
                if (oa.fileName == null ||
                    (oa.fileName != null && oa.fileName == ""))
                {
                    Ret = false;
                    break;
                }
                else
                {
                    Ret = Ret && System.IO.File.Exists(Path.Combine(db.getVOICEIDSTR(),oa.fileName));
                }
            }
            return Ret;
        }
            /// <summary>
            /// 指定した歌詞から、発音記号列を引き当てます
            /// </summary>
            /// <param name="phrase"></param>
            /// <returns></returns>
        public static SymbolTableEntry attatch(string[] phrase, int index, string DictionaryName)
        {
                if (phrase[index] == "-" || phrase[index] == "—")
                {
                    return new SymbolTableEntry("-", "-");
                }
                else
                {
                    string cur = phrase[index];
                    string nxt = "";
                    string pre = "";

                    int i = 1;
                    while (index+i<phrase.Length)
                    {
                        nxt = phrase[index+i];
                        i++;
                        if (nxt != "-" && nxt != "—")
                        {
                            break;
                        }
                    }
                    i = 1;
                    while (index - i >=0)
                    {
                        pre = phrase[index - i];
                        i++;
                        if (pre != "-" && pre != "—")
                        {
                            break;
                        }
                    }
                    string r = AppManager.UtauSplitDictionary.getDictionaryByName(DictionaryName).getSymbolString(pre, cur, nxt);
                    if (r == "") return null;
                    return new SymbolTableEntry(cur,r);
                }
            }

            [Obsolete]
            public static bool attatch(string[] phrase,int index,string DictionaryName, out string result)
            {
                var entry = attatch(phrase,index,DictionaryName);
                if (entry == null)
                {
                    result = "a";
                    return false;
                }
                else
                {
                    result = entry.getSymbol();
                    return true;
                }
            }

    }
}
