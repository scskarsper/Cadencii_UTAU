using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cadencii.utau
{
    class GetFileEncodingValue
    {
        public static string getFileEncoding(string FilePath)
        {
            string ENC = "";
            string FL = "";
            try
            {
                FL = System.IO.File.ReadAllLines(FilePath)[0];
                if (FL != "")
                {
                    if (FL.Substring(0, 9) == "#Charset:")
                    {
                        ENC = FL.Substring(9).Replace("utf", "UTF").Replace("UTF-", "UTF").Replace("UTF", "UTF-");
                        if (Encoding.GetEncoding(ENC) == null)
                        {
                            ENC = "";
                        }
                    }
                }
            }
            catch { ENC = ""; }
            return ENC;
        }
    }
}
