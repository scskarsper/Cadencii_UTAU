/*
 * UtauVoiceDB.cs
 * Copyright © 2009-2011 kbinani
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
using System.IO;
using System.Collections.Generic;
using cadencii.vsq;

namespace cadencii.utau
{
    /// <summary>
    /// UTAUの原音設定を表すクラス
    /// </summary>
    public class UtauVoiceDB
    {
        private string name_ = "Unknown";
        private Oto root_;
        private PrefixMap prefixmap_;
        private List<Oto> sub_ = new List<Oto>();
        private string singconfig = "";

        public string getVOICEIDSTR()
        {
            return singconfig;
        }
        /// <summary>
        /// コンストラクタ．
        /// </summary>
        /// <param name="singer_config"></param>
        public UtauVoiceDB(SingerConfig singer_config)
        {
            name_ = singer_config.VOICENAME;
            singconfig = singer_config.VOICEIDSTR;
            string oto_ini = Path.Combine(singer_config.VOICEIDSTR, "oto.ini");
            string oto_Cache= Path.Combine(singer_config.VOICEIDSTR, "oto.cache");
            if (System.IO.File.Exists(oto_Cache))
            {
                string S = System.IO.File.ReadAllText(oto_Cache, System.Text.Encoding.UTF8);
                root_=XmlSerialize.DeserializeXML<Oto>(S);
            }
            else
            {
                root_ = new Oto(oto_ini, singer_config.VOICEIDSTR);
                string O=XmlSerialize.SerializeXML<Oto>(root_);
                System.IO.File.WriteAllText(oto_Cache,O, System.Text.Encoding.UTF8);
            }

            var prefixmap = Path.Combine(singer_config.VOICEIDSTR, "prefix.map");
            if (File.Exists(prefixmap)) {
                prefixmap_ = new PrefixMap(prefixmap);
            }

            foreach (var directory in Directory.EnumerateDirectories(singer_config.VOICEIDSTR)) {
                var maybe_oto_file = Path.Combine(directory, "oto.ini");
                var maybe_oto_cache = Path.Combine(directory, "oto.cache");
                if (File.Exists(maybe_oto_file))
                {
                    Oto oto=null;
                    if (System.IO.File.Exists(maybe_oto_cache))
                    {
                        string S = System.IO.File.ReadAllText(maybe_oto_cache, System.Text.Encoding.UTF8);
                        oto = XmlSerialize.DeserializeXML<Oto>(S);
                    }
                    else
                    {
                        oto = new Oto(maybe_oto_file, singer_config.VOICEIDSTR);
                        string O = XmlSerialize.SerializeXML<Oto>(oto);
                        System.IO.File.WriteAllText(maybe_oto_cache, O, System.Text.Encoding.UTF8);
                    }
                    sub_.Add(oto);
                }
            }
        }

        /// <summary>
        /// 指定した歌詞に合致する、エイリアスを考慮した原音設定を取得します
        /// </summary>
        /// <param name="lyric"></param>
        /// <returns></returns>
        public OtoArgs attachFileNameFromLyric(string lyric, int note_number)
        {
            Func<string, OtoArgs> get_oto_arg = (l) => {
                var result = root_.attachFileNameFromLyric(l);
                if (!result.isEmpty()) {
                    return result;
                }
                foreach (var oto in sub_) {
                    var args = oto.attachFileNameFromLyric(l);
                    if (!args.isEmpty()) {
                        return args;
                    }
                }
                return new OtoArgs();
            };

            // first, try finding mapped lyric.
            if (prefixmap_ != null) {
                var mapped_lyric = prefixmap_.getMappedLyric(lyric, note_number);
                var args = get_oto_arg(mapped_lyric);
                if (!args.isEmpty()) {
                    return args;
                }
            }

            return get_oto_arg(lyric);
        }

        /// <summary>
        /// この原音の名称を取得します．
        /// </summary>
        /// <returns>この原音の名称</returns>
        public string getName()
        {
            return name_;
        }
    }
}
