using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using App.Klonoa2;

namespace AssetLoader.Loaders.LV
{
    // Log + load PPT and BGM files
    [HarmonyPatch(typeof(SoundManager), "AsyncLoadSound")]
    public class SoundManager__AsyncLoadSound
    {
        [HarmonyPrefix]
        public static void Prefix(string fileName, SoundManager.SOUND_TYPE type, int channel, ref bool __runOriginal, ref AudioSource ____sourceVoice, ref AudioSource ____sourceBgm, ref SoundManager __instance, ref IEnumerator __result)
        {
            if (type != SoundManager.SOUND_TYPE.VOICE && type != SoundManager.SOUND_TYPE.BGM)
                return;
            Console.WriteLine($"{type} {fileName}");
            string customPath = Path.Combine(AssetLoaderPlugin.LVDataPath, $"sound/{(type == SoundManager.SOUND_TYPE.VOICE ? "ppt" : "bgm")}/{fileName}.wav").ToLower();
            if (File.Exists(customPath))
            {
                AudioClip audioClip = WavUtility.ToAudioClip(customPath);
                if (type == SoundManager.SOUND_TYPE.VOICE) {
                    ____sourceVoice.clip = audioClip;
                    __instance.LoadVoiceName = fileName;
                } else {
                    ____sourceBgm.clip = audioClip;
                    __instance.LoadBgmName = fileName;
                }
                __runOriginal = false;
                __result = new List<object>().GetEnumerator(); // Prevents from returning null
            }
        }
    }

    // Load SE files into pool
    [HarmonyPatch(typeof(SoundManager), "SyncLoadSound")]
    public class SoundManager__SyncLoadSound
    {
        [HarmonyPrefix]
        public static void Prefix(string fileName, SoundManager.SOUND_TYPE type, int channel, ref Dictionary<string, AudioClip> ____poolSe, ref bool __runOriginal)
        {
            if (type != SoundManager.SOUND_TYPE.SE)
                return;
            string customPath = Path.Combine(AssetLoaderPlugin.LVDataPath, $"sound/{type}/{fileName}.wav").ToLower();
            if (channel == SoundManager.AUTO_CHANNEL && File.Exists(customPath))
            {
                ____poolSe[fileName] = WavUtility.ToAudioClip(customPath);
                __runOriginal = false;
            }
        }
    }

    // SE log
    [HarmonyPatch(typeof(SoundManager), "PlaySE")]
    public class SoundManager__PlaySE
    {
        [HarmonyPrefix]
        public static void Prefix(string name)
        {
            Console.WriteLine($"SE {name}");
        }
    }
}
 