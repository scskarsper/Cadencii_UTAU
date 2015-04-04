/*
 * VSTiDllManager.cs
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
using System.Threading;
using System.IO;
using System.Collections.Generic;
using cadencii;
using cadencii.java.util;
using cadencii.media;
using cadencii.vsq;
using cadencii.java.io;



namespace cadencii
{

    /// <summary>
    /// VSTiのDLLを管理するクラス
    /// </summary>
    public static class VSTiDllManager
    {
        //public static int SAMPLE_RATE = 44100;
        const float a0 = -17317.563f;
        const float a1 = 86.7312112f;
        const float a2 = -0.237323499f;
        /// <summary>
        /// 使用するボカロの最大バージョン．2までリリースされているので今は2
        /// </summary>
        const int MAX_VOCALO_VERSION = 2;

        /// <summary>
        /// 指定した合成器の種類に合致する合成器の新しいインスタンスを取得します
        /// </summary>
        /// <param name="kind">合成器の種類</param>
        /// <returns>指定した種類の合成器の新しいインスタンス</returns>
        public static WaveGenerator getWaveGenerator(RendererKind kind)
        {
            return new UtauWaveGenerator();
            /*
            if (kind == RendererKind.AQUES_TONE) {
#if ENABLE_AQUESTONE
                return new AquesToneWaveGenerator(getAquesToneDriver());
            } else if (kind == RendererKind.AQUES_TONE2) {
                return new AquesTone2WaveGenerator(getAquesTone2Driver());
#endif
            } else if (kind == RendererKind.VCNT) {
                return new VConnectWaveGenerator();
            } else if (kind == RendererKind.UTAU) {
                return new UtauWaveGenerator();
            } else if (kind == RendererKind.VOCALOID1 ||
                        kind == RendererKind.VOCALOID2) {
#if ENABLE_VOCALOID
                return new VocaloidWaveGenerator();
#endif
            }
            return new EmptyWaveGenerator();*/
        }

        public static void init()
        {
        }


        public static bool isRendererAvailable(RendererKind renderer, string wine_prefix, string wine_top)
        {
            if (renderer == RendererKind.UTAU) {
                // ここでは，resamplerの内どれかひとつでも使用可能であればOKの判定にする
                bool resampler_exists = false;
                int size = AppManager.editorConfig.getResamplerCount();
                for (int i = 0; i < size; i++) {
                    string path = AppManager.editorConfig.getResamplerAt(i);
                    if (System.IO.File.Exists(path)) {
                        resampler_exists = true;
                        break;
                    }
                }
                if (resampler_exists &&
                     !AppManager.editorConfig.PathWavtool.Equals("") && System.IO.File.Exists(AppManager.editorConfig.PathWavtool)) {
                    if (AppManager.editorConfig.UtauSingers.Count > 0) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void terminate()
        {

        }

        public static int getErrorSamples(float tempo)
        {
            if (tempo <= 240) {
                return 4666;
            } else {
                float x = tempo - 240;
                return (int)((a2 * x + a1) * x + a0);
            }
        }

        public static float getPlayTime()
        {
            double pos = PlaySound.getPosition();
            return (float)pos;
        }
    }

}
