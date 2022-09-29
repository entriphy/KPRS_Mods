using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using App.Klonoa2;

namespace AssetLoader
{
    [BepInPlugin("asset_loader", "Asset Loader", "1.0.0")]
    [BepInProcess("Klonoa.exe")]
    public class AssetLoaderPlugin : BaseUnityPlugin
    {
        public static string LVDataPath;

        private void Awake()
        {
            LVDataPath = Path.Combine(Paths.PluginPath, "AssetLoader", "lv");
            Directory.CreateDirectory(Path.Combine(LVDataPath, "sound/ppt"));
            Directory.CreateDirectory(Path.Combine(LVDataPath, "sound/bgm"));
            Directory.CreateDirectory(Path.Combine(LVDataPath, "chr_infos"));
            var harmony = new Harmony("asset_loader");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin asset_loader is loaded!");
        }
    }

    [HarmonyPatch(typeof(App.Klonoa2.SoundManager), "AsyncLoadSound")]
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
}
