using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cadencii.vsq;
using cadencii.utau;

namespace cadencii.dsp
{
    class UtauSymbolSplitor
    {
        VsqEvent oriEvent = null;
        Dictionary<int,double> oriPitchBrust = null;
        List<VsqEvent> oriVibstrBrustEvents = null;
        Dictionary<VsqEvent, List<VsqEvent>> dstVibstrBrustEvents = new Dictionary<VsqEvent,List<VsqEvent>>();
        Dictionary<VsqEvent, Dictionary<int, double>> dstPitchBrust = new Dictionary<VsqEvent, Dictionary<int, double>>();
        Dictionary<VsqEvent, VsqEvent> dstParentTable = new Dictionary<VsqEvent, VsqEvent>();
        cadencii.utau.UtauVoiceDB vdb = null;
        VsqFileEx mVsq = null;

        public UtauSymbolSplitor(VsqFileEx mVsq,VsqEvent curEvent, Dictionary<int, double> PitchBrust, List<VsqEvent> VibstrBrustEvents, cadencii.utau.UtauVoiceDB db)
        {
            oriPitchBrust = PitchBrust;
            oriVibstrBrustEvents = VibstrBrustEvents;
            oriEvent = curEvent;
            dstVibstrBrustEvents.Add(oriEvent, oriVibstrBrustEvents);
            dstPitchBrust.Add(oriEvent, oriPitchBrust);
            dstParentTable.Add(oriEvent, oriEvent);
            vdb = db;
            this.mVsq = mVsq;
        }
        private int pre5(float X)
        {
            int p5 = ((int)(X / 5) * 5);
            if(X%5>0)
            {
                p5=p5+5;
            }
            return p5;
        }
        private int Cell5(float X)
        {
            int p5 = ((int)(X / 5) * 5);
            return p5;
        }
        /*private int round5(float X)
        {
            return (int)(Math.Round(X / 5, 0) * 5);
        }*/
        public void SplitIt()
        {
            dstVibstrBrustEvents.Clear();
            dstPitchBrust.Clear();
            dstParentTable.Clear();
            string PRO=oriEvent.ID.LyricHandle.L0.getPhoneticSymbol();
            if (PRO.IndexOf(',') > 0)
            {
                if (oriEvent.UstEvent.SplitLength > 0)
                {
                    int Len1 = 0;
                    int Len2 = 0;
                    int SplitTime = 0;
                    Len2 = (int)oriEvent.UstEvent.SplitLength;
                    Len1 = oriEvent.ID.getLength() - Len2;

                    /*if (oriEvent.UstEvent.SplitisPercent)
                    {
                        Len2 = (int)(oriEvent.ID.getLength() * (oriEvent.UstEvent.SplitLength / 100.0));
                        Len1 = oriEvent.ID.getLength() - Len2;
                    }
                    else
                    {
                        Len2 = (int)oriEvent.UstEvent.SplitLength;
                        Len1 = oriEvent.ID.getLength() - Len2;
                    }
                    if (Len1 <= 0)
                    {
                        Len1 = oriEvent.ID.getLength() / 2;
                        Len2 = oriEvent.ID.getLength() - Len1;
                    }*/

                    /*int PitchCountStartPoint=(int)Math.Round(oriEvent.Clock-oriEvent.UstEvent.getPreUtterance(),0);
                    Len1 = Cell5(PitchCountStartPoint + Len1) - PitchCountStartPoint;
                    Len2 = oriEvent.ID.getLength() - Len1;
                    SplitTime=(Len1 + oriEvent.Clock);
                    */

                    OtoArgs oa = new OtoArgs();
                    if (vdb!=null)
                    {
                        int CheckNote = oriEvent.ID.Note;
                        if (oriEvent.UstEvent != null)
                        {
                            CheckNote = oriEvent.UstEvent.ReplaceNoteID > 0 ? oriEvent.UstEvent.ReplaceNoteID : oriEvent.ID.Note;
                        }
                        oa = vdb.attachFileNameFromLyric(PRO.Split(',')[1], CheckNote);
                    }
                    if (oa.fileName == null ||
                   (oa.fileName != null && oa.fileName == ""))
                    {
                        oriEvent.ID.LyricHandle.L0.setPhoneticSymbol(PRO.Split(',')[0]);
                        //oriEvent.UstEvent.setVoiceOverlap(oriEvent.UstEvent.getVoiceOverlap() + preover);
                        dstVibstrBrustEvents.Add(oriEvent, oriVibstrBrustEvents);
                        dstPitchBrust.Add(oriEvent, oriPitchBrust);
                        dstParentTable.Add(oriEvent, oriEvent);
                        return;
                    }
                    int SplitLen = (int)oriEvent.UstEvent.SplitLength;
                    if (oriEvent.UstEvent.SplitisPercent)
                    {
                        SplitLen = (int)(oriEvent.ID.getLength() * SplitLen / 100.0);
                    }

                    float V2PreUttr = oa.msPreUtterance + oriEvent.UstEvent.SplitSTP;
                    float V1PreUttr = oriEvent.UstEvent.getPreUtterance()+oriEvent.UstEvent.getStartPoint();
                    int RealV1Start = (int)mVsq.getClockFromSec(mVsq.getSecFromClock(oriEvent.Clock)-V1PreUttr/1000.0);
                    int RealV2Start = (int)mVsq.getClockFromSec(mVsq.getSecFromClock(oriEvent.Clock+Len1)-V2PreUttr/1000.0);
                    int R = Cell5(RealV2Start - RealV1Start);
                    int Dert = (RealV2Start - RealV1Start) - R;
                    if (Dert > 3)
                    {
                        Len1 = Len1 + ((5 - Dert) > 0 ? (5 - Dert) : 0);
                    }
                    else
                    {
                        Len1 = Len1 - Dert;
                    }
                    Len2 = oriEvent.ID.getLength() - Len1;
                    SplitTime = (Len1 + oriEvent.Clock);
                    int CrossLen = (int)((oa.msOverlap) + oriEvent.UstEvent.SplitVoiceOverlap);


                    VsqEvent V1 = (VsqEvent)oriEvent.clone();
                    V1.ID.LyricHandle.L0.setPhoneticSymbol(PRO.Split(',')[0]);
                    V1.Clock = oriEvent.Clock;
                    V1.ID.setLength(Len1);
                    V1.UstEvent.setVoiceOverlap(V1.UstEvent.getVoiceOverlap()+V1.UstEvent.NotePreOverlap);
                    UstEnvelope env = oriEvent.UstEvent.getEnvelope();
                    if (env == null) env = new UstEnvelope();
                    env.p3 = CrossLen;
                    env.p4 = 0;
                    env.v4 = 0;
                    V1.UstEvent.setEnvelope(env);

                    VsqEvent V2 = (VsqEvent)oriEvent.clone();
                    V2.ID.LyricHandle.L0.setPhoneticSymbol(PRO.Split(',')[1]);
                    V2.Clock = SplitTime;
                    V2.ID.setLength(Len2);
                    V2.UstEvent.setVoiceOverlap(CrossLen);
                    V2.UstEvent.setPreUtterance(oa.msPreUtterance);
                    V2.UstEvent.setStartPoint(V2.UstEvent.SplitSTP);
                    V2.UstEvent.LeftLimit = 0;
                    env = oriEvent.UstEvent.getEnvelope();
                    if (env == null) env = new UstEnvelope();
                    env.p2 = CrossLen;
                    env.v2 = env.v2 - V1.UstEvent.MoreOver;
                    if (env.v2 < 0) env.v2 = 0;
                    env.p1 = 0;
                    env.v1 = 0;
                    if (env.p5 > 0)
                    {
                        env.p5 = env.p5 - Len1;
                    }
                    V2.UstEvent.setEnvelope(env);


                    dstVibstrBrustEvents.Add(V1, oriVibstrBrustEvents);
                    dstPitchBrust.Add(V1, oriPitchBrust);
                    dstParentTable.Add(V1, oriEvent);
                    dstVibstrBrustEvents.Add(V2, oriVibstrBrustEvents);
                    dstPitchBrust.Add(V2, oriPitchBrust);
                    dstParentTable.Add(V2, oriEvent);
                }
                else
                {
                    if(oriEvent.ID.LyricHandle.L0.getPhoneticSymbol().IndexOf(",")>0)
                    {   
                        oriEvent.ID.LyricHandle.L0.setPhoneticSymbol(PRO.Split(',')[0]);
                    }
                    dstVibstrBrustEvents.Add(oriEvent, oriVibstrBrustEvents);
                    oriEvent.UstEvent.setVoiceOverlap(oriEvent.UstEvent.getVoiceOverlap() + oriEvent.UstEvent.NotePreOverlap);
                    dstPitchBrust.Add(oriEvent, oriPitchBrust);
                    dstParentTable.Add(oriEvent, oriEvent);
                }
            }
            else
            {
                dstVibstrBrustEvents.Add(oriEvent, oriVibstrBrustEvents);
                oriEvent.UstEvent.setVoiceOverlap(oriEvent.UstEvent.getVoiceOverlap() + oriEvent.UstEvent.NotePreOverlap);
                dstPitchBrust.Add(oriEvent, oriPitchBrust);
                dstParentTable.Add(oriEvent, oriEvent);
            }

        }
        public Dictionary<VsqEvent,List<VsqEvent>> getChildNotes()
        {
            return dstVibstrBrustEvents;
        }
        public Dictionary<VsqEvent, Dictionary<int, double>> getPitchBrusts()
        {
            return dstPitchBrust;
        }
        public Dictionary<VsqEvent, VsqEvent> getParentNote()
        {
            return dstParentTable;
        }
    }
}
