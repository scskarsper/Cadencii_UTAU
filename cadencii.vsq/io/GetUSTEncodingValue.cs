using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cadencii.utau
{
    class GetUSTEncodingValue
    {
        public static string getUstEncoding(string FilePath)
        {
            string ENC = "";
            string FL = "";
            try
            {
                for (int i = 0; i <= 10; i++)
                {
                    FL = System.IO.File.ReadAllLines(FilePath)[i];
                    if (FL.IndexOf("[#0") >= 0)
                    {
                        break;
                    }
                    else if (FL != "")
                    {
                        if (FL.Substring(0, 8) == "Charset=")
                        {
                            ENC = FL.Substring(8).Replace("utf", "UTF").Replace("UTF-", "UTF").Replace("UTF", "UTF-");
                            if (Encoding.GetEncoding(ENC) == null)
                            {
                                ENC = "";
                            }
                            break;
                        }
                    }
                }
            }
            catch { ENC = ""; }
            return ENC;
        }
    }
}
