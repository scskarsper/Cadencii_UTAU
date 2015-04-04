/*
 * SelectedEventEntry.cs
 * Copyright © 2008-2011 kbinani
 *
 * This file is part of cadencii.
 *
 * cadencii is free software; you can redistribute it and/or
 * modify it under the terms of the GPLv3 License.
 *
 * cadencii is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
using System;
using System.ComponentModel;
using cadencii.java.util;
using cadencii.vsq;
using cadencii.utau;
using System.Collections.Generic;

namespace cadencii
{

    struct PositionSpec
    {
        public int measure;
        public int beat;
        public int gate;
        public Timesig timesig;
    }

    /// <summary>
    /// 選択されたアイテムを管理します。
    /// また、プロパティグリッドの登録アイテムとして編集されているオブジェクトと、
    /// VsqFileExに登録されているオブジェクトとの間を取り持つ処理を担います。
    /// </summary>
#if ENABLE_PROPERTY
    [TypeConverter(typeof(SelectedEventEntryTypeConverter))]
    public class SelectedEventEntry
#else
    public class SelectedEventEntry
#endif
    {
        /// <summary>
        /// 選択されたアイテムが存在しているトラック番号。
        /// </summary>
        public int track;
        /// <summary>
        /// 選択されたアイテム。
        /// </summary>
        public VsqEvent original;
        /// <summary>
        /// 選択されたアイテムの、編集後の値。
        /// </summary>
        public VsqEvent editing;
#if ENABLE_PROPERTY
        private static int lastVibratoLength = 66;
        private string m_clock;
        private BooleanEnum m_symbol_protected;
        private string m_length;
        private NoteNumberProperty m_note;
        private BooleanEnum m_portamento_up;
        private BooleanEnum m_portamento_down;
        private AttackVariation m_attack;
        private VibratoVariation m_vibrato;
        private string m_measure;
        private string m_beat;
        private string m_tick;
#if DEBUG
        private DEBUG_GatetimeProperty m_debug_clock = new DEBUG_GatetimeProperty();
#endif
#endif

        /// <summary>
        /// 指定されたパラメータを用いて、選択アイテムを表す新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="track_"></param>
        /// <param name="original_"></param>
        /// <param name="editing_"></param>
        public SelectedEventEntry(int track_, VsqEvent original_, VsqEvent editing_)
        {
            track = track_;
            original = original_;
            editing = editing_;

#if ENABLE_PROPERTY
            captureValuesFromEditing();
#endif
        }

#if ENABLE_PROPERTY
        /// <summary>
        /// このオブジェクトのeditingフィールドの値から、プロパティの各値のオブジェクトを構築します
        /// </summary>
        public void captureValuesFromEditing()
        {
            // clock
            m_clock = editing.Clock + "";

            // measure, beat, gate
            PositionSpec ret = getPosition();
            m_measure = ret.measure + "";
            m_beat = ret.beat + "";
            m_tick = ret.gate + "";

            // symbol_protected
            m_symbol_protected = BooleanEnum.Off;
            if (editing.ID.LyricHandle != null && editing.ID.LyricHandle.L0 != null) {
                m_symbol_protected = editing.ID.LyricHandle.L0.PhoneticSymbolProtected ? BooleanEnum.On : BooleanEnum.Off;
            }

            // length
            m_length = editing.ID.getLength() + "";

            // note
            m_note = new NoteNumberProperty();
            m_note.noteNumber = editing.ID.Note;

            // portamento
            m_portamento_up = BooleanEnum.Off;
            m_portamento_down = BooleanEnum.Off;
            if (editing.ID.PMbPortamentoUse >= 2) {
                m_portamento_down = BooleanEnum.On;
            }
            if (editing.ID.PMbPortamentoUse == 1 || editing.ID.PMbPortamentoUse == 3) {
                m_portamento_up = BooleanEnum.On;
            }

            // attack, vibrato
            VsqFileEx vsq = AppManager.getVsqFile();
            if (vsq != null) {
                SynthesizerType type = SynthesizerType.VOCALOID2;

                if (type == SynthesizerType.VOCALOID1) {
                    if (editing.ID.NoteHeadHandle != null) {
                        m_attack = new AttackVariation(editing.ID.NoteHeadHandle.getDisplayString());
                    }
                }
                if (editing.ID.VibratoHandle != null) {
                    m_vibrato = new VibratoVariation(editing.ID.VibratoHandle.getDisplayString());
                }
            }
            if (m_attack == null) {
                m_attack = new AttackVariation();
            }
            if (m_vibrato == null) {
                m_vibrato = new VibratoVariation(VibratoVariation.empty.description);
            }
        }
#endif

#if ENABLE_PROPERTY
        /// <summary>
        /// 小節数、拍数、ゲート数から、クロック値を計算します
        /// </summary>
        /// <param name="measure"></param>
        /// <param name="beat"></param>
        /// <param name="gate"></param>
        /// <returns></returns>
        private int calculateClock(int measure, int beat, int gate)
        {
            VsqFileEx vsq = AppManager.getVsqFile();
            if (vsq == null) {
                int premeasure = 2;
                return ((measure + premeasure - 1) * 4 + (beat - 1)) * 480 + gate;
            } else {
                int premeasure = vsq.getPreMeasure();
                int bartopclock = vsq.getClockFromBarCount(measure + premeasure - 1);
                Timesig timesig = vsq.getTimesigAt(bartopclock);
                return bartopclock + (beat - 1) * 480 * 4 / timesig.denominator + gate;
            }
        }

        /// <summary>
        /// 現在のクロック値(m_clock)から、小節数、拍数、ゲート数(?)を計算します
        /// </summary>
        /// <returns></returns>
        private PositionSpec getPosition()
        {
            PositionSpec ret = new PositionSpec();
            VsqFileEx vsq = AppManager.getVsqFile();
            int clock = editing.Clock;
            if (vsq == null) {
                // 4/4拍子, プリメジャー2と仮定
                int i = clock / (480 * 4);
                int tpremeasure = 2;
                ret.measure = i - tpremeasure + 1;
                int tdif = clock - i * 480 * 4;
                ret.beat = tdif / 480 + 1;
                ret.gate = tdif - (ret.beat - 1) * 480;
                ret.timesig = new Timesig(4, 4);
                return ret;
            }

            int premeasure = vsq.getPreMeasure();
            ret.measure = vsq.getBarCountFromClock(clock) - premeasure + 1;
            int clock_bartop = vsq.getClockFromBarCount(ret.measure + premeasure - 1);
            Timesig timesig = vsq.getTimesigAt(clock);
            int den = timesig.denominator;
            int dif = clock - clock_bartop;
            int step = 480 * 4 / den;
            ret.beat = dif / step + 1;
            ret.gate = dif - (ret.beat - 1) * step;
            ret.timesig = timesig;
            return ret;
        }

        /// <summary>
        /// プロパティに入力された文字列と、編集前の値を元に、入力された文字列を解釈することによって編集後の値がどうなるかを調べます
        /// </summary>
        /// <param name="old_value"></param>
        /// <param name="received_string"></param>
        /// <returns></returns>
        public static int evalReceivedString(int old_value, string received_string)
        {
            int draft = old_value;
            if (received_string.StartsWith("+") || received_string.StartsWith("-") || received_string.StartsWith("*") || received_string.StartsWith("/")) {
                try {
                    string eq = "x" + received_string;

                    // 「+ 480)*1.1」みたいな書式を許容したいので。「+ 480)*1.1」=>「(x+ 480)*1.1」
                    int num_bla = 0; // "("の個数
                    int num_cket = 0; // ")"の個数
                    for (int i = 0; i < eq.Length; i++) {
                        char c = eq[i];
                        if (c == '(') {
                            num_bla++;
                        } else if (c == ')') {
                            num_cket++;
                        }
                    }
                    int diff = num_cket - num_bla;
                    for (int i = 0; i < diff; i++) {
                        eq = "(" + eq;
                    }
                    draft = (int)Utility.eval(draft, eq);
                } catch (Exception ex) {
                    draft = old_value;
                }
            } else {
                try {
                    draft = (int)Utility.eval(old_value, received_string);
                } catch (Exception ex) {
                    draft = old_value;
                }
            }
            return draft;
        }

        private UstEvent getEditingUstEvent()
        {
            if (editing.UstEvent == null) {
                editing.UstEvent = new UstEvent();
            }
            return editing.UstEvent;
        }

        #region Lyric
        [Category("Lyric")]
        public string Phrase
        {
            get
            {
                return getPhrase();
            }
            set
            {
                setPhrase(value);
            }
        }

        public void updatePhone(string pharse)
        {
            try
            {   VsqFileEx vsq = AppManager.getVsqFile();
                if (vsq == null)
                {
                    return;
                }
                int selected = AppManager.getSelected();
                VsqTrack vsq_track = vsq.Track[selected];
                int selectedid = editing.InternalID;
                int selectedcount = 2;
                List<VsqEvent> trackevents = new List<VsqEvent>();
                bool isStart = false;
                int c = 0;
                foreach (var item in vsq_track.getNoteEventIterator())
                {
                    if (item.InternalID + 1 >= selectedid)
                    {
                        isStart = true;
                    }
                    if (isStart)
                    {
                        trackevents.Add(item);
                        c++;
                    }
                    if (c >= selectedcount)
                    {
                        break;
                    }
                }
                for (int k = 0; k < trackevents.Count; k++)
                {
                    if (trackevents[k].InternalID == editing.InternalID)
                    {
                        VsqID id = trackevents[k].ID;
                        VsqID preid = null;
                        VsqID nxtid = null;
                        int CurEndClock = trackevents[k].Clock + id.getLength();
                        int NxtStartClock = 0;
                        if (id.LyricHandle.L0.PhoneticSymbolProtected)
                        {
                            continue;
                        }

                        if (k > 0)
                        {
                            preid = trackevents[k - 1].ID;
                        }
                        int o = 0;
                        while (true)
                        {
                            o++;
                            if (k + o < trackevents.Count)
                            {
                                nxtid = trackevents[k + o].ID;
                                NxtStartClock = trackevents[k + o].Clock;
                                try
                                {
                                    string pp = nxtid.LyricHandle.L0.Phrase;
                                    if (pp == "-" || pp == "—")
                                    {
                                        CurEndClock = NxtStartClock + nxtid.getLength();
                                        nxtid = null; NxtStartClock = 0; continue;
                                    }
                                }
                                catch { nxtid = null; NxtStartClock = 0; }
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (AppManager.UtauSplitDictionary.getDictionaryByName(vsq_track.SplitDictionaryName).getMinSplitNoteLength() >= (CurEndClock - trackevents[k].Clock))
                        {
                            nxtid = null;
                        }
                        string symbolOld = id.LyricHandle.L0.getPhoneticSymbol();
                        string symbolResult = symbolOld;
                        string[] phrases = new string[3] { "", "", "" };
                        if (preid != null) { phrases[0] = preid.LyricHandle.L0.Phrase; };
                        if (nxtid != null)
                        {
                            if (NxtStartClock != 0)
                            {
                                if (NxtStartClock - CurEndClock < 120)
                                {
                                    phrases[2] = nxtid.LyricHandle.L0.Phrase;
                                }
                            }
                        };
                        phrases[1] = pharse;
                        SymbolTableEntry entry = UtauSymbolTable.UtauSymbolTable.attatch(phrases, 1, vsq_track.SplitDictionaryName);//SymbolTable.attatch(phrases,1);
                        if (entry == null)
                        {
                            continue;
                        }
                        symbolResult = entry.getSymbol();
                        if (symbolResult.Equals(symbolOld))
                        {
                            continue;
                        }
//                        VsqID idNew = (VsqID)id.clone();
                        editing.ID.LyricHandle.L0.setPhoneticSymbol(symbolResult);
                    }
                }
            }
            catch { }
        }

        public void setPhrase(string value)
        {
            if (editing.ID.LyricHandle == null) {
                return;
            }
            if (editing.ID.LyricHandle.L0 == null) {
                return;
            }
            string old = editing.ID.LyricHandle.L0.Phrase;
            if (!old.Equals(value)) {
                // 歌詞
                string phrase = value;
                if (AppManager.editorConfig.SelfDeRomanization) {
                    phrase = KanaDeRomanization.Attach(value);
                }
                editing.ID.LyricHandle.L0.Phrase = phrase;
                /*
                // 発音記号
                string phonetic_symbol = "";
                SymbolTableEntry entry = SymbolTable.attatch(phrase);
                if (entry == null) {
                    phonetic_symbol = "a";
                } else {
                    phonetic_symbol = entry.getSymbol();
                }
                editing.ID.LyricHandle.L0.setPhoneticSymbol(phonetic_symbol);

                // consonant adjustment
                string[] spl = PortUtil.splitString(phonetic_symbol, new char[] { ' ', ',' }, true);
                string consonant_adjustment = "";
                for (int i = 0; i < spl.Length; i++) {
                    consonant_adjustment += (i == 0 ? "" : " ") + (VsqPhoneticSymbol.isConsonant(spl[i]) ? 64 : 0);
                }
                editing.ID.LyricHandle.L0.setConsonantAdjustment(consonant_adjustment);
                */
                string OP=editing.ID.LyricHandle.L0.getPhoneticSymbol();
                updatePhone(phrase);
                string NP = editing.ID.LyricHandle.L0.getPhoneticSymbol();
                if (OP != NP)
                {
                    string pharseA = NP;
                    if (NP.IndexOf(",") > 0)
                    {
                        pharseA = NP.Split(',')[0];
                    }
                    // overlap, preUtterancec
                    VsqFileEx vsq = AppManager.getVsqFile();
                    if (vsq != null)
                    {
                        int selected = AppManager.getSelected();
                        VsqTrack vsq_track = vsq.Track[selected];
                        VsqEvent singer = vsq_track.getSingerEventAt(editing.Clock);
                        SingerConfig sc = AppManager.getSingerInfoUtau(singer.ID.IconHandle.Language, singer.ID.IconHandle.Program);
                        if (sc != null && AppManager.mUtauVoiceDB.ContainsKey(sc.VOICEIDSTR))
                        {
                            UtauVoiceDB db = AppManager.mUtauVoiceDB[sc.VOICEIDSTR];
                            int CheckNote = editing.ID.Note;
                            if (editing.UstEvent != null)
                            {
                                CheckNote = editing.UstEvent.ReplaceNoteID > 0 ? editing.UstEvent.ReplaceNoteID : editing.ID.Note;
                            }
                            OtoArgs oa = db.attachFileNameFromLyric(pharseA, CheckNote);
                            if (editing.UstEvent == null)
                            {
                                editing.UstEvent = new UstEvent();
                            }
                            editing.UstEvent.setVoiceOverlap(oa.msOverlap);
                            editing.UstEvent.setPreUtterance(oa.msPreUtterance);
                        }
                    }
                }
            }
        }

        public string getPhrase()
        {
            if (editing.ID.LyricHandle != null && editing.ID.LyricHandle.L0 != null) {
                return editing.ID.LyricHandle.L0.Phrase;
            }
            return "";
        }

        [Category("Lyric")]
        public string PhoneticSymbol
        {
            get
            {
                return getPhoneticSymbol();
            }
            set
            {
                setPhoneticSymbol(value);
            }
        }

        [Category("Actions")]
        public bool ReloadOtoArgs
        {
            get
            {
                return false;
            }
            set
            {
                resetUTAU();
            }
        }

        public void setPhoneticSymbol(string value)
        {
            if (editing.ID.LyricHandle == null) {
                return;
            }
            if (editing.ID.LyricHandle.L0 == null) {
                return;
            }
            string OP = editing.ID.LyricHandle.L0.getPhoneticSymbol();
            editing.ID.LyricHandle.L0.setPhoneticSymbol(value);
            string NP = editing.ID.LyricHandle.L0.getPhoneticSymbol();
            if (OP != NP)
            {
                string pharseA = NP;
                if (NP.IndexOf(",") > 0)
                {
                    pharseA = NP.Split(',')[0];
                }
                VsqFileEx vsq = AppManager.getVsqFile();
                if (vsq != null)
                {
                    int selected = AppManager.getSelected();
                    VsqTrack vsq_track = vsq.Track[selected];
                    VsqEvent singer = vsq_track.getSingerEventAt(editing.Clock);
                    SingerConfig sc = AppManager.getSingerInfoUtau(singer.ID.IconHandle.Language, singer.ID.IconHandle.Program);
                    if (sc != null && AppManager.mUtauVoiceDB.ContainsKey(sc.VOICEIDSTR))
                    {
                        UtauVoiceDB db = AppManager.mUtauVoiceDB[sc.VOICEIDSTR];
                        int CheckNote = editing.ID.Note;
                        if (editing.UstEvent != null)
                        {
                            CheckNote = editing.UstEvent.ReplaceNoteID > 0 ? editing.UstEvent.ReplaceNoteID : editing.ID.Note;
                        }
                        OtoArgs oa = db.attachFileNameFromLyric(pharseA, CheckNote);
                        if (editing.UstEvent == null)
                        {
                            editing.UstEvent = new UstEvent();
                        }
                        editing.UstEvent.setVoiceOverlap(oa.msOverlap);
                        editing.UstEvent.setPreUtterance(oa.msPreUtterance);
                    }
                }
            }
        }

        public void resetUTAU()
        {
            if (editing.ID.LyricHandle == null)
            {
                return;
            }
            if (editing.ID.LyricHandle.L0 == null)
            {
                return;
            }
                string pharseA = editing.ID.LyricHandle.L0.getPhoneticSymbol();
                if (pharseA.IndexOf(",") > 0)
                {
                    pharseA = pharseA.Split(',')[0];
                }
                VsqFileEx vsq = AppManager.getVsqFile();
                if (vsq != null)
                {
                    int selected = AppManager.getSelected();
                    VsqTrack vsq_track = vsq.Track[selected];
                    VsqEvent singer = vsq_track.getSingerEventAt(editing.Clock);
                    SingerConfig sc = AppManager.getSingerInfoUtau(singer.ID.IconHandle.Language, singer.ID.IconHandle.Program);
                    if (sc != null && AppManager.mUtauVoiceDB.ContainsKey(sc.VOICEIDSTR))
                    {
                        UtauVoiceDB db = AppManager.mUtauVoiceDB[sc.VOICEIDSTR];
                        int CheckNote = editing.ID.Note;
                        if (editing.UstEvent != null)
                        {
                            CheckNote = editing.UstEvent.ReplaceNoteID > 0 ? editing.UstEvent.ReplaceNoteID : editing.ID.Note;
                        }
                        OtoArgs oa = db.attachFileNameFromLyric(pharseA, CheckNote);
                        if (editing.UstEvent == null)
                        {
                            editing.UstEvent = new UstEvent();
                        }
                        editing.UstEvent.setVoiceOverlap(oa.msOverlap);
                        editing.UstEvent.setPreUtterance(oa.msPreUtterance);
                    }
                }
        }


        public string getPhoneticSymbol()
        {
            if (editing.ID.LyricHandle != null && editing.ID.LyricHandle.L0 != null) {
                return editing.ID.LyricHandle.L0.getPhoneticSymbol();
            }
            return "";
        }

        [Category("Lyric")]
        public string CosonantAdjustment
        {
            get
            {
                return getCosonantAdjustment();
            }
            set
            {
                setCosonantAdjustment(value);
            }
        }

        public void setCosonantAdjustment(string value)
        {
            if (editing.ID.LyricHandle == null) {
                return;
            }
            if (editing.ID.LyricHandle.L0 == null) {
                return;
            }
            string[] symbol = PortUtil.splitString(editing.ID.LyricHandle.L0.getPhoneticSymbol(), new char[] { ' ' }, true);
            string[] adjustment = PortUtil.splitString(value, new char[] { ' ', ',' }, true);
            if (adjustment.Length < symbol.Length) {
                Array.Resize(ref adjustment, symbol.Length);
            }
            int[] iadj = new int[symbol.Length];
            for (int i = 0; i < iadj.Length; i++) {
                if (VsqPhoneticSymbol.isConsonant(symbol[i])) {
                    int v = 64;
                    try {
                        v = int.Parse(adjustment[i]);
                    } catch (Exception ex) {
                    }
                    if (v < 0) {
                        v = 0;
                    } else if (127 < v) {
                        v = 127;
                    }
                    iadj[i] = v;
                } else {
                    iadj[i] = 0;
                }
            }
            string consonant_adjustment = "";
            for (int i = 0; i < iadj.Length; i++) {
                consonant_adjustment += (i == 0 ? "" : " ") + iadj[i];
            }
            editing.ID.LyricHandle.L0.setConsonantAdjustment(consonant_adjustment);
        }

        public string getCosonantAdjustment()
        {
            if (editing.ID.LyricHandle != null && editing.ID.LyricHandle.L0 != null) {
                return editing.ID.LyricHandle.L0.getConsonantAdjustment();
            }
            return "";
        }

        [Category("Lyric")]
        public BooleanEnum Protect
        {
            get
            {
                return getProtect();
            }
            set
            {
                setProtect(value);
            }
        }

        public void setProtect(BooleanEnum value)
        {
            m_symbol_protected = value;
            if (editing.ID.LyricHandle == null) {
                return;
            }
            if (editing.ID.LyricHandle.L0 == null) {
                return;
            }
            editing.ID.LyricHandle.L0.PhoneticSymbolProtected = (value == BooleanEnum.On) ? true : false;
        }

        public BooleanEnum getProtect()
        {
            return m_symbol_protected;
        }
        #endregion

        #region Note Location
        [Category("Note Location")]
        public string Clock
        {
            get
            {
                return getClock();
            }
            set
            {
                setClock(value);
            }
        }

        public void setClock(string value)
        {
            int oldvalue = editing.Clock;
            int draft = evalReceivedString(oldvalue, value);
            editing.Clock = draft;
            m_clock = draft + "";
        }

        public string getClock()
        {
            return m_clock;
        }

        [Category("Note Location")]
        public string Measure
        {
            get
            {
                return getMeasure();
            }
            set
            {
                setMeasure(value);
            }
        }

        public void setMeasure(string value)
        {
            PositionSpec ret = getPosition();
            int draft = evalReceivedString(ret.measure, value);
            int clock = calculateClock(draft, ret.beat, ret.gate);
            editing.Clock = clock;
            m_clock = clock + "";
        }

        public string getMeasure()
        {
            return m_measure;
        }

        [Category("Note Location")]
        public string Beat
        {
            get
            {
                return getBeat();
            }
            set
            {
                setBeat(value);
            }
        }

        public void setBeat(string value)
        {
            PositionSpec ret = getPosition();
            int draft = evalReceivedString(ret.beat, value);
            int clock = calculateClock(ret.measure, draft, ret.gate);
            editing.Clock = clock;
            m_clock = clock + "";
        }

        public string getBeat()
        {
            return m_beat;
        }

        [Category("Note Location")]
        public string Tick
        {
            get
            {
                return getTick();
            }
            set
            {
                setTick(value);
            }
        }

        public void setTick(string value)
        {
            PositionSpec ret = getPosition();
            int draft = evalReceivedString(ret.gate, value);
            int clock = calculateClock(ret.measure, ret.beat, draft);
            editing.Clock = clock;
            m_clock = clock + "";
        }

        public string getTick()
        {
            return m_tick;
        }
        #endregion

        #region Note
        [Category("Note")]
        public string Length
        {
            get
            {
                return getLength();
            }
            set
            {
                setLength(value);
            }
        }

        public void setLength(string value)
        {
            int oldvalue = editing.ID.getLength();
            int draft = evalReceivedString(oldvalue, value);
            if (draft < 0) {
                draft = 0;
            } else {
                VsqFileEx vsq = AppManager.getVsqFile();
                if (vsq != null) {
                    int maxlength = vsq.getMaximumNoteLengthAt(editing.Clock);
                    if (maxlength < draft) {
                        draft = maxlength;
                    }
                }
            }

            // ビブラートの長さを調節
            Utility.editLengthOfVsqEvent(editing, draft, AppManager.vibratoLengthEditingRule);
        }

        public string getLength()
        {
            return m_length;
        }

        [Category("Note")]
        public NoteNumberProperty Note
        {
            get
            {
                return getNote();
            }
            set
            {
                setNote(value);
            }
        }

        public void setNote(NoteNumberProperty value)
        {
            if (value.noteNumber < 0) {
                m_note.noteNumber = 0;
            } else if (127 < value.noteNumber) {
                m_note.noteNumber = 127;
            } else {
                m_note = value;
            }
            editing.ID.Note = m_note.noteNumber;
        }

        public NoteNumberProperty getNote()
        {
            return m_note;
        }
        
        [Category("Note")]
        public NoteNumberProperty CalcPreMapAsNote
        {
            get
            {
                return editing.UstEvent.ReplaceNoteID>0?getPreMapNote():Note;
            }
            set
            {
                if (value.noteNumber==60)
                {
                    editing.UstEvent.ReplaceNoteID = 0;
                    return;
                }
                if (value == getNote())
                {
                    editing.UstEvent.ReplaceNoteID = 0;
                }
                else
                {
                    setPreMapNote(value);
                }
            }
        }

        NoteNumberProperty p_note = new NoteNumberProperty();
        public void setPreMapNote(NoteNumberProperty value)
        {
            if (value.noteNumber < 0)
            {
                p_note.noteNumber = 0;
            }
            else if (127 < value.noteNumber)
            {
                p_note.noteNumber = 127;
            }
            else
            {
                p_note = value;
            }
            editing.UstEvent.ReplaceNoteID = p_note.noteNumber;
        }

        public NoteNumberProperty getPreMapNote()
        {
            p_note.noteNumber = editing.UstEvent.ReplaceNoteID;
            return p_note;
        }

        [Category("Note")]
        public float NotePreOverlap
        {
            get
            {
                return editing.UstEvent.NotePreOverlap;
            }
            set
            {
                editing.UstEvent.NotePreOverlap = value;
            }
        }

        #endregion

        #region Split
        [Category("SplitPhonetic")]
        public string CurrentSplitDictionary
        {
            get
            {
                try
                {
                    VsqFileEx vsq = AppManager.getVsqFile();
                    if (vsq != null)
                    {
                        int selected = AppManager.getSelected();
                        VsqTrack vsq_track = vsq.Track[selected];
                        return vsq_track.SplitDictionaryName;
                    }
                    else
                    {
                        return "Default";
                    }
                }
                catch { return "Default"; }
            }
        }
        [Category("SplitPhonetic")]
        public double SplitLength
        {
            get
            {
                try
                {
                    if (editing.ID.LyricHandle.L0.getPhoneticSymbol().IndexOf(",") >= 0)
                    {
                        return editing.UstEvent.SplitLength;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch { return 0; }
            }
            set
            {
                try
                {
                    if (editing.ID.LyricHandle.L0.getPhoneticSymbol().IndexOf(",") >= 0)
                    {
                        editing.UstEvent.SplitLength = value;
                    }
                }
                catch { ;}
            }
        }
        [Category("SplitPhonetic")]
        public bool SplitAsPercent
        {
            get
            {
                return editing.UstEvent.SplitisPercent;
            }
            set
            {
                editing.UstEvent.SplitisPercent = value;
            }
        }
       
        [Category("SplitPhonetic")]
        public float SplitVoiceOverlap
        {
            get
            {
                try
                {
                    if (editing.ID.LyricHandle.L0.getPhoneticSymbol().IndexOf(",") >= 0)
                    {
                        return editing.UstEvent.SplitVoiceOverlap; ;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch { return 0;}
            }
            set
            {
                try
                {
                    if (editing.ID.LyricHandle.L0.getPhoneticSymbol().IndexOf(",") >= 0)
                    {
                        editing.UstEvent.SplitVoiceOverlap = value;
                    }
                    else
                    {
                    }
                }
                catch { ;}
            }
        }

        [Category("SplitPhonetic")]
        public int VolumeDecreaseInHead
        {
            get
            {
                return editing.UstEvent.MoreOver;
            }
            set
            {
                editing.UstEvent.MoreOver = value;
            }
        }

        [Category("SplitPhonetic")]
        public float SplitSTP
        {
            get
            {
                return editing.UstEvent.SplitSTP;
            }
            set
            {
                editing.UstEvent.SplitSTP = value;
            }
        }
        #endregion


        #region Envlope
        [Category("Envlope")]
        public int Env_P1
        {
            get
            {
                UstEnvelope UE = editing.UstEvent.Envelope;
                if (UE == null) UE = new UstEnvelope();
                return UE.p1;
            }
            set
            {
                UstEnvelope UE = editing.UstEvent.Envelope;
                if (UE == null) UE = new UstEnvelope();
                UE.p1 = value;
                editing.UstEvent.Envelope = UE;
            }
        }
        [Category("Envlope")]
        public int Env_P2
        {
            get
            {
                UstEnvelope UE = editing.UstEvent.Envelope;
                if (UE == null) UE = new UstEnvelope();
                return UE.p2;
            }
            set
            {
                UstEnvelope UE = editing.UstEvent.Envelope;
                if (UE == null) UE = new UstEnvelope();
                UE.p2 = value;
                editing.UstEvent.Envelope = UE;
            }
        }
        [Category("Envlope")]
        public int Env_P3
        {
            get
            {
                UstEnvelope UE = editing.UstEvent.Envelope;
                if (UE == null) UE = new UstEnvelope();
                return UE.p3;
            }
            set
            {
                UstEnvelope UE = editing.UstEvent.Envelope;
                if (UE == null) UE = new UstEnvelope();
                UE.p3 = value;
                editing.UstEvent.Envelope = UE;
            }
        }
        [Category("Envlope")]
        public int Env_P4
        {
            get
            {
                UstEnvelope UE = editing.UstEvent.Envelope;
                if (UE == null) UE = new UstEnvelope();
                return UE.p4;
            }
            set
            {
                UstEnvelope UE = editing.UstEvent.Envelope;
                if (UE == null) UE = new UstEnvelope();
                UE.p4 = value;
                editing.UstEvent.Envelope = UE;
            }
        }
        #endregion

        #region UTAU
        //[Category("Readonly")]
        /*
        private int PreOverLength
        {
            get
            {
                int preTtr = 0;
                VsqFileEx vsq = AppManager.getVsqFile();
                if (vsq != null)
                {
                    int selected = AppManager.getSelected();
                    VsqTrack vsq_track = vsq.Track[selected];
                    int OVEMIN = AppManager.UtauSplitDictionary.getDictionaryByName(vsq_track.SplitDictionaryName).getMinOverlapLen();
                    int PTRS = AppManager.UtauSplitDictionary.getDictionaryByName(vsq_track.SplitDictionaryName).getPreOverlap();
                    int Len = editing.ID.getLength();
                    string PreEventPhy = "";
                    for (int i = editing.InternalID; i <= int.MaxValue; i++)
                    {
                        try
                        {
                            VsqEvent EventNxt = vsq_track.getEvent(i);
                            string NXR = EventNxt.ID.LyricHandle.L0.getPhoneticSymbol();
                            if (NXR != "-" && NXR != "—")
                            {
                                break;
                            }
                            Len = Len + EventNxt.ID.getLength();
                        }
                        catch { break; }
                    }
                    bool foundme = false;
                    for (int i = editing.InternalID-1; i >0 ; i--)
                    {
                        if (i < 1) break;
                        try
                        {
                            VsqEvent EventPre= vsq_track.getEvent(i);
                            string NPre = EventPre.ID.LyricHandle.L0.getPhoneticSymbol();
                            if (NPre != "-" && NPre != "—")
                            {
                                if (foundme)
                                {
                                    PreEventPhy = NPre;
                                    break;
                                }
                                else
                                {
                                    foundme = true;
                                }
                            }
                        }
                        catch { break; }
                    }

                    if (PTRS > 0 && OVEMIN < Len)
                    {
                            if (PreEventPhy.IndexOf(",") > 0)
                            {
                                VsqEvent singer = vsq_track.getSingerEventAt(editing.Clock);
                                SingerConfig sc = AppManager.getSingerInfoUtau(singer.ID.IconHandle.Language, singer.ID.IconHandle.Program);
                                if (sc != null && AppManager.mUtauVoiceDB.ContainsKey(sc.VOICEIDSTR))
                                {
                                    UtauVoiceDB db = AppManager.mUtauVoiceDB[sc.VOICEIDSTR];
                                    OtoArgs oa = db.attachFileNameFromLyric(PreEventPhy.Split(',')[1], editing.ID.Note);
                                    if (editing.UstEvent != null)
                                    {
                                        preTtr = PTRS;
                                    }
                                }
                            }
                    }
                }
                return preTtr;
            }
        }
        */

        [Category("UTAU")]
        public float PreUtterance
        {
            get
            {
                return getPreUtterance();// +PreOverLength;
            }
            set
            {
                setPreUtterance(value);//-PreOverLength);
            }
        }

        public void setPreUtterance(float value)
        {
            if (editing.UstEvent == null) {
                editing.UstEvent = new UstEvent();
            }
            editing.UstEvent.setPreUtterance(value);
        }

        public float getPreUtterance()
        {
            if (editing.UstEvent == null) {
                return 0;
            }
            return editing.UstEvent.getPreUtterance();
        }

        [Category("UTAU")]
        public float Overlap
        {
            get
            {
                return getOverlap();
            }
            set
            {
                setOverlap(value);
            }
        }

        public void setOverlap(float value)
        {
            if (editing.UstEvent == null) {
                editing.UstEvent = new UstEvent();
            }
            editing.UstEvent.setVoiceOverlap(value);
        }

        public float getOverlap()
        {
            if (editing.UstEvent == null) {
                return 0;
            }
            return editing.UstEvent.getVoiceOverlap();
        }

        [Category("UTAU")]
        public int Moduration
        {
            get
            {
                return getModuration();
            }
            set
            {
                setModuration(value);
            }
        }

        public void setModuration(int value)
        {
            if (editing.UstEvent == null) {
                editing.UstEvent = new UstEvent();
            }
            editing.UstEvent.setModuration(value);
        }

        public int getModuration()
        {
            if (editing.UstEvent == null) {
                editing.UstEvent = new UstEvent();
            }
            return editing.UstEvent.getModuration();
        }

        [Category("UTAU")]
        public string Flags
        {
            get
            {
                return getFlags();
            }
            set
            {
                setFlags(value);
            }
        }

        public void setFlags(string value)
        {
            if (editing.UstEvent == null) {
                editing.UstEvent = new UstEvent();
            }
            editing.UstEvent.Flags = value;
        }

        public string getFlags()
        {
            if (editing.UstEvent == null) {
                return "";
            }
            return editing.UstEvent.Flags;
        }

        [Category("UTAU")]
        public float StartPoint
        {
            get
            {
                return getStartPoint();
            }
            set
            {
                setStartPoint(value);
            }
        }

        public float getStartPoint()
        {
            return getEditingUstEvent().getStartPoint();
        }

        public void setStartPoint(float value)
        {
            getEditingUstEvent().setStartPoint(value);
        }

        [Category("UTAU")]
        public int Intensity
        {
            get
            {
                return getIntensity();
            }
            set
            {
                setIntensity(value);
            }
        }

        public int getIntensity()
        {
            return getEditingUstEvent().getIntensity();
        }

        public void setIntensity(int value)
        {
            getEditingUstEvent().setIntensity(value);
        }


        [Category("UTAU")]
        public int SampleLeftBorder
        {
            get
            {
                return editing.UstEvent.LeftLimit;
            }
            set
            {
                editing.UstEvent.LeftLimit = value;
            }
        }
        #endregion

        #region Vibrato
        [Category("Vibrato")]
        public VibratoVariation Vibrato
        {
            get
            {
                return getVibrato();
            }
            set
            {
                setVibrato(value);
            }
        }

        public void setVibrato(VibratoVariation value)
        {
            if (value.description.Equals(VibratoVariation.empty.description)) {
                editing.ID.VibratoHandle = null;
            } else {
                int last_length = 0;
                if (editing.ID.VibratoHandle != null) {
                    last_length = editing.ID.VibratoHandle.getLength();
                }

                if (m_vibrato != null && value != null && !m_vibrato.equals(value)) {
                    string description = value.description;
                    if (AppManager.editorConfig.UseUserDefinedAutoVibratoType) {
                        int size = AppManager.editorConfig.AutoVibratoCustom.Count;
                        for (int i = 0; i < size; i++) {
                            VibratoHandle handle = AppManager.editorConfig.AutoVibratoCustom[i];
                            string display_string = handle.getDisplayString();
                            if (description == display_string) {
                                editing.ID.VibratoHandle = (VibratoHandle)handle.clone();
                                break;
                            }
                        }
                    } else {
                        VsqFileEx vsq = AppManager.getVsqFile();
                        if (vsq != null) {
                            SynthesizerType type = SynthesizerType.VOCALOID2;
                            foreach (var vconfig in VocaloSysUtil.vibratoConfigIterator(type)) {
                                if (description.Equals(vconfig.getDisplayString())) {
                                    editing.ID.VibratoHandle = (VibratoHandle)vconfig.clone();
                                    break;
                                }
                            }
                        }
                    }
                }
                if (editing.ID.VibratoHandle != null) {
                    if (last_length <= 0) {
                        last_length = lastVibratoLength;
                        if (last_length <= 0) {
                            last_length = 66;
                        }
                    }
                    editing.ID.VibratoHandle.setLength(last_length);
                }
            }
            m_vibrato = value;
        }

        public VibratoVariation getVibrato()
        {
            return m_vibrato;
        }

        [Category("Vibrato")]
        public int VibratoLength
        {
            get
            {
                return getVibratoLength();
            }
            set
            {
                setVibratoLength(value);
            }
        }

        public void setVibratoLength(int value)
        {
#if DEBUG
            sout.println("VsqEventItemProxy#set_VibratoLength; value=" + value);
#endif
            if (value <= 0) {
                m_vibrato = new VibratoVariation(VibratoVariation.empty.description);
                editing.ID.VibratoHandle = null;
                editing.ID.VibratoDelay = editing.ID.getLength();
            } else {
                int draft = value;
                if (100 < draft) {
                    draft = 100;
                }
                if (editing.ID.VibratoHandle == null) {
                    VsqFileEx vsq = AppManager.getVsqFile();
                    if (vsq != null) {
                        string iconid = AppManager.editorConfig.AutoVibratoType2;
                        SynthesizerType type = SynthesizerType.VOCALOID2;
                        editing.ID.VibratoHandle = AppManager.editorConfig.createAutoVibrato(type, 480); // 480はダミー
                    }
                    if (editing.ID.VibratoHandle == null) {
                        editing.ID.VibratoHandle = new VibratoHandle();
                    }
                }
                editing.ID.VibratoHandle.setLength(editing.ID.getLength() * draft / 100);
                editing.ID.VibratoDelay = editing.ID.getLength() - editing.ID.VibratoHandle.getLength();
                lastVibratoLength = editing.ID.VibratoHandle.getLength() * 100 / editing.ID.getLength();
            }
        }

        public int getVibratoLength()
        {
            if (editing.ID.VibratoHandle == null) {
                return 0;
            }
            return editing.ID.VibratoHandle.getLength() * 100 / editing.ID.getLength();
        }
        #endregion


#endif
    }

#if DEBUG
    public class DEBUG_GatetimePropertyConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        // String -> DEBUG_GatetimeProperty
        public override Object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, Object value)
        {
            if (value is string) {
                string s = (string)value;
                string[] spl = s.Split(',');
                if (spl.Length >= 3) {
                    try {
                        int measure = int.Parse(spl[0].Trim());
                        int beat = int.Parse(spl[1].Trim());
                        int gate = int.Parse(spl[2].Trim());
                        DEBUG_GatetimeProperty ret = new DEBUG_GatetimeProperty();
                        ret.Measure = measure + "";
                        ret.Beat = beat + "";
                        ret.Gate = gate + "";
                        return ret;
                    } catch {
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DEBUG_GatetimeProperty)) {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        // DEBUG_GatetimeProperty -> String
        public override Object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, Object value, Type destinationType)
        {
            if (value is DEBUG_GatetimeProperty && destinationType == typeof(string)) {
                DEBUG_GatetimeProperty gp = (DEBUG_GatetimeProperty)value;
                return gp.Measure + ", " + gp.Beat + ", " + gp.Gate;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [TypeConverter(typeof(DEBUG_GatetimePropertyConverter))]
    public class DEBUG_GatetimeProperty
    {
        string m = "1";
        string b = "2";
        string g = "3";

        [NotifyParentProperty(true), RefreshProperties(RefreshProperties.All)]
        public string Measure
        {
            get
            {
                return m;
            }
            set
            {
                m = value;
            }
        }

        [NotifyParentProperty(true), RefreshProperties(RefreshProperties.All)]
        public string Beat
        {
            get
            {
                return b;
            }
            set
            {
                b = value;
            }
        }

        [NotifyParentProperty(true), RefreshProperties(RefreshProperties.All)]
        public string Gate
        {
            get
            {
                return g;
            }
            set
            {
                g = value;
            }
        }
    }
#endif

}
