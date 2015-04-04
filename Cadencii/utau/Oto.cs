/*
 * Oto.cs
 * Copyright © 2013 kbinani
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Serialization;

namespace cadencii.utau
{
    /// <summary>
    /// Represents oto.ini information.
    /// </summary>
    [Serializable]  
    public class Oto
    {
        private List<OtoArgs> configs_ = new List<OtoArgs>();
        [XmlElement]
        public SerializableDictionary<string, OtoArgs> filename2configs = new SerializableDictionary<string, OtoArgs>();
        [XmlElement]
        public SerializableDictionary<string, OtoArgs> Alias2configs = new SerializableDictionary<string, OtoArgs>();
        [XmlElement]  
        public OtoArgs[] configList
        {
            get
            {
                return configs_.ToArray();
            }
            set
            {
                if (value != null)
                {
                    configs_.Clear();
                    configs_.AddRange(value);
                }
            }
        }

        public Oto()
        {
        }
        /// <summary>
        /// コンストラクタ．
        /// </summary>
        /// <param name="oto_ini_file_path"></param>
        public Oto(string oto_ini_file_path, string db_root_directory)
        {
            if (File.Exists(oto_ini_file_path))
            {
                string ENC = cadencii.utau.GetFileEncodingValue.getFileEncoding(oto_ini_file_path);
                if (ENC == "")
                {
                    ENC = cadencii.utau.GetFileEncodingValue.getFileEncoding(db_root_directory + "\\oto.ini");
                }
                if (ENC == "")
                {
                    foreach (var encoding_name in AppManager.TEXT_ENCODINGS_IN_UTAU)
                    {
                        readOtoIni(oto_ini_file_path, encoding_name, db_root_directory);
                    }
                }
                else
                {
                    readOtoIni(oto_ini_file_path, ENC, db_root_directory);
                }
            }
        }

        /// <summary>
        /// 原音設定ファイルを読み込みます．
        /// </summary>
        /// <param name="oto_ini">原音設定のパス</param>
        private void readOtoIni(string oto_ini, string encoding_name, string db_root_directory)
        {
            // oto.ini読込み
            string dir = Path.GetDirectoryName(oto_ini);
            var encoding = Encoding.GetEncoding(encoding_name);
            using (var sr = new StreamReader(oto_ini, encoding)) {
                while (!sr.EndOfStream) {
                    var line = sr.ReadLine();
                    string[] spl = line.Split('=');
                    if (spl.Length < 2) {
                        continue;
                    }
                    string file_name = spl[0]; // あ.wav
                    string a2 = spl[1]; // ,0,36,64,0,0
                    string a1 = Path.GetFileNameWithoutExtension(file_name);
                    spl = a2.Split(',');
                    if (spl.Length < 6) {
                        continue;
                    }

                    // ファイルがちゃんとあるかどうか？
                    string fullpath = Path.Combine(dir, file_name);
                    if (!File.Exists(fullpath)) {
                        continue;
                    }

                    OtoArgs oa = new OtoArgs();
                    var full_file_name = Path.GetFullPath(Path.Combine(dir, file_name));
                    var full_root_directory = Path.GetFullPath(db_root_directory) + @"\";
                    oa.fileName = full_file_name.StartsWith(full_root_directory) ? full_file_name.Substring(full_root_directory.Length) : file_name;
                    oa.Alias = spl[0];

                    oa.msOffset = parseOrDefault(spl[1]);
                    oa.msConsonant = parseOrDefault(spl[2]);
                    oa.msBlank = parseOrDefault(spl[3]);
                    oa.msPreUtterance = parseOrDefault(spl[4]);
                    oa.msOverlap = parseOrDefault(spl[5]);

                    // 重複登録が無いかチェック
                    if (configs_.All((item) => !item.equals(oa))) {
                        try
                        {
                            configs_.Add(oa);
                            if (!Alias2configs.ContainsKey(oa.Alias))
                            {
                                Alias2configs.Add(oa.Alias, oa);
                            }
                            var ext = Path.GetExtension(oa.fileName);
                            var file_name_without_ext = oa.fileName.EndsWith(ext) ? oa.fileName.Substring(0, oa.fileName.Length - ext.Length) : oa.fileName;
                            if (!filename2configs.ContainsKey(file_name_without_ext))
                            {
                                filename2configs.Add(file_name_without_ext, oa);
                            }
                        }
                        catch { ;}
                    }
                }
            }
        }

        /// <summary>
        /// 指定した歌詞に合致する、エイリアスを考慮した原音設定を取得します
        /// </summary>
        /// <param name="lyric"></param>
        /// <returns></returns>
        public OtoArgs attachFileNameFromLyric(string lyric)
        {
            /*
            foreach (var item in configs_) {
                var ext = Path.GetExtension(item.fileName);
                var file_name_without_ext = item.fileName.EndsWith(ext) ? item.fileName.Substring(0, item.fileName.Length - ext.Length) : item.fileName;
                if (file_name_without_ext == lyric) {
                    return item;
                }
                if (item.Alias == lyric) {
                    return item;
                }
            }*/
            if (Alias2configs.ContainsKey(lyric))
            {
                return Alias2configs[lyric];
            }
            else if (filename2configs.ContainsKey(lyric))
            {
                return filename2configs[lyric];
            }
            return new OtoArgs();
        }

        private static float parseOrDefault(string @string)
        {
            float result;
            if (float.TryParse(@string, out result)) {
                return result;
            } else {
                return default(float);
            }
        }
    }
}
