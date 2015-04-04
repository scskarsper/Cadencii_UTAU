using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cadencii.vsq;

namespace cadencii
{
    class UtauPitchBendGenerator
    {
        static char encode(int i)
        {
            if (i == 62) return '+';
            if (i == 63) return '/';
            if (i >= 0 && i <= 25) return (char)(i + (int)'A');
            if (i >= 26 && i <= 51) return (char)(i + 71);
            if (i >= 52 && i <= 61) return (char)(i - 4);
            return ' ';
        }
        public static string base64encoderForUtau(int i)
        {
            if (i < 0) i += 4096;
            int low6 = i & 0x3F;
            int high6 = (i & 0xFC0) >> 6;
            return encode(high6)+"" + encode(low6);
        }

        public static string FlagGener(VsqTrack track,VsqEvent item)
        {
            string SpecialI = item.UstEvent.Flags;
            int g = (track.MetaText.GEN.getValue(item.Clock + (item.ID.getLength() > 10 ? 10 : (item.ID.getLength() / 2))) - 64);
            int b = (track.MetaText.BRE.getValue(item.Clock + (item.ID.getLength() > 10 ? 10 : (item.ID.getLength() / 2))) - 64);
            if (SpecialI.IndexOf("g") < 0)
            {
                if(g!=0)
                {
                    SpecialI = SpecialI + "g" + g.ToString();
                }
            }
            if (SpecialI.IndexOf("BRE") < 0)
            {
                if (b != 0)
                {
                    SpecialI = SpecialI + "BRE" + b.ToString();
                }
            }
            return SpecialI;// SpecialI.Replace("p", "popop") + " p0";
        }
    }
}
