using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace cadencii.utausplitdictionary
{
    public class FullPhonDictionary : InterfaceUTAUDictionary
    {
        public string getSymbolString(string pre, string cur, string next)
        {
            return cur;
        }
        public int getPreOverlap()
        {
            return 0;
        }
        public int getMinOverlapLen()
        {
            return 65536;
        }
        public int getMinSplitNoteLength()
        {
            return 120;
        }
    }
    public class TxtPhonDictionary : InterfaceUTAUDictionary
    {
        Dictionary<string, string> DataList = new Dictionary<string, string>();
        public void AddData(string Pre,string Cur, string Next, string splitA, string splitB)
        {
            if (Pre == "")
            {
                Pre = "*";
            }
            if (Next == "")
            {
                Next = "*";
            }
            if (Next == "R")
            {
                Next = "";
            }
            string K = Pre + "{||SPLIT||}" + Cur + "{||SPLIT||}" + Next;
            if (!DataList.ContainsKey(K))
            {
                if (splitA != "")
                {
                    if (splitB != "")
                    {
                        DataList.Add(K, splitA + "," + splitB);
                    }
                    else
                    {
                        DataList.Add(K, splitA);
                    }
                }
            }
        }
        int pr = 0;
        int minO = 240;
        int minN = 120;
        public void setPreOverlap(int value)
        {
            pr = value;
        }

        public int getPreOverlap()
        {
            return pr;
        }
        public void setMinOverlapLen(int value)
        {
            minO = value;
        }

        public int getMinOverlapLen()
        {
            return minO;
        }

        public void setMinSplitNoteLength(int value)
        {
            minN = value;
        }

        public int getMinSplitNoteLength()
        {
            return minN;
        }

        public string getSymbolString(string pre, string cur, string next)
        {
            string K = pre + "{||SPLIT||}" + cur + "{||SPLIT||}" + next;
            if (DataList.ContainsKey(K))
            {
                return DataList[K];
            }
            K = "*{||SPLIT||}" + cur + "{||SPLIT||}" + next;
            if (DataList.ContainsKey(K))
            {
                return DataList[K];
            }
            K = pre + "{||SPLIT||}" + cur + "{||SPLIT||}*";
            if (DataList.ContainsKey(K))
            {
                return DataList[K];
            }
            K = "*{||SPLIT||}" + cur + "{||SPLIT||}*";
            if (DataList.ContainsKey(K))
            {
                return DataList[K];
            }
            return cur;
        }
    }
    public class DictionaryManager
    {
        public Dictionary<string, InterfaceUTAUDictionary> DictionaryList = new Dictionary<string, InterfaceUTAUDictionary>()
            {
                {"Default",new FullPhonDictionary()}
            };
        public InterfaceUTAUDictionary getDictionaryByName(string DicName)
        {
            if(DictionaryList.ContainsKey(DicName))
            {
                return DictionaryList[DicName];
            }else
            {
                return DictionaryList["Default"];
            }
        }
        public List<string> getDictionaryNames()
        {
            List<string> Ret = new List<string>();
            Ret.AddRange(DictionaryList.Keys.ToArray<string>());
            return Ret;
        }
        public void InitDictionaryLibrary()
        {
#if DEBUG
            sout.println("INIT DICTIONARYS");
#endif
            string DictionaryRoot = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\SplitDictionarys";
            if (!System.IO.Directory.Exists(DictionaryRoot))
            {
                System.IO.Directory.CreateDirectory(DictionaryRoot);
            }
            DirectoryInfo DList = new DirectoryInfo(DictionaryRoot);
            foreach (FileInfo F in DList.GetFiles("*.txt"))
            {
                string[] InitLoad=System.IO.File.ReadAllLines(F.FullName);
                string Charst = "";
                foreach (string s in InitLoad)
                {
                    if ((Charst == ""))
                    {
                        if (s.Substring(0, 9).ToLower() == "#charset:")
                        {
                            Charst = s.Substring(9).Replace("utf", "UTF").Replace("UTF-", "UTF").Replace("UTF", "UTF-");
                            if (Encoding.GetEncoding(Charst) == null)
                            {
                                Charst = "";
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                string Name = F.Name.Substring(0, F.Name.Length - 4);
                string NameF = F.Name.Substring(0, F.Name.Length - 4);
                string[] InitData = InitLoad;
                if (Charst != "")
                {
                    InitData=System.IO.File.ReadAllLines(F.FullName, Encoding.GetEncoding(Charst));
                }
                TxtPhonDictionary tDic = new TxtPhonDictionary();
                foreach (string s in InitData)
                {
                    if (s.Length>6 && s.Substring(0, 6).ToLower() == "#name:")
                    {
                        Name = s.Substring(6);
                    }
                        /*
                    else if (s.Length>=12 && s.Substring(0, 12).ToLower() == "#preoverlap:")
                    {
                        string A = s.Substring(12);
                        int ss = 0;
                        int.TryParse(A, out ss);
                        tDic.setPreOverlap(ss);
                    }
                    else if (s.Length >= 17 && s.Substring(0, 17).ToLower() == "#minoverlinknote:")
                    {
                        string A = s.Substring(17);
                        int ss = 240;
                        int.TryParse(A, out ss);
                        tDic.setMinOverlapLen(ss);
                    }*/
                    else if (s.Length >= 20&& s.Substring(0, 20).ToLower() == "#minsplitnotelength:")
                    {
                        string A = s.Substring(20);
                        int ss = 120;
                        int.TryParse(A, out ss);
                        tDic.setMinOverlapLen(ss);
                    }
                    else if (s.Length>1 && s.Substring(0, 1) == "#")
                    {
                    }
                    else if (s.IndexOf('=')>0)
                    {
                        try
                        {
                            string[] str = s.Split('=');
                            string k = str[0];
                            if (k.IndexOf(",") < 0)
                            {
                                k = "*,"+k + ",*";
                            }
                            string v = str[1];
                            if (v.IndexOf(",") < 0)
                            {
                                v = k + ",,";
                            }
                            string[] kx = k.Split(',');
                            string[] vx = v.Split(',');
                            if (kx.Length == 2)
                            {
                                tDic.AddData("*", kx[0], kx[1], vx[0], vx[1]);
                            }
                            else if(kx.Length==3)
                            {
                                tDic.AddData(kx[0], kx[1] , kx[2], vx[0], vx[1]);
                            }
                        }
                        catch { ;}
                    }
                }
                if (DictionaryList.ContainsKey(Name))
                {
                    Name = NameF;
                }
                if (DictionaryList.ContainsKey(Name))
                {
                    int X = 1;
                    while (DictionaryList.ContainsKey(Name))
                    {
                        Name = Name + "#" + X.ToString();
                        X++;
                    }
                }
                DictionaryList.Add(Name, tDic);
            }
        }
    }
}
