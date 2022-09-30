using System;
using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace AssetLoader.Loaders.DTP
{
    // BGM log + load
    [HarmonyPatch()]
    public class AssetBundleSimulator__LoadAsset
    {
        public static MethodBase TargetMethod()
        {
            return typeof(nsPFW.AssetBundleSimulator).GetMethod("LoadAsset").MakeGenericMethod(typeof(UnityEngine.Object));
        }

        [HarmonyPrefix]
        public static void Prefix(string assetBundleName, string assetName, ref UnityEngine.Object __result, ref bool __runOriginal)
        {
            if (assetBundleName.StartsWith("sound/bgm"))
            {
                Console.WriteLine($"bgm {assetName}");
                string customPath = Path.Combine(AssetLoaderPlugin.DTPDataPath, $"sound/bgm/{assetName}.wav").ToLower();
                if (File.Exists(customPath))
                {
                    __result = WavUtility.ToAudioClip(customPath);
                    __runOriginal = false;
                }
            }
        }
    }

    // SE log
    [HarmonyPatch(typeof(nsPFW.CommonSound), "SeStart")]
    public class CommonSound__SeStart
    {
        [HarmonyPrefix]
        public static void Prefix(string index)
        {
            Console.WriteLine($"se {index}");
        }
    }

    // Voice log
    [HarmonyPatch(typeof(nsPFW.CommonSound), "VoiceStart")]
    public class CommonSound__VoiceStart
    {
        [HarmonyPrefix]
        public static void Prefix(string index, ref Dictionary<string, string> ___dicSoundResource)
        {
            string i = index;
            string str = "";
            if (___dicSoundResource.TryGetValue(index, out str) && str != null)
                i = str;
            Console.WriteLine($"voice {i}");
        }
    }

    // Add SE/Voice files to sound dictionary
    [HarmonyPatch(typeof(nsPFW.CommonSound), "Initialize")]
    public class CommonSound__Initialize
    {
        [HarmonyPostfix]
        public static void Postfix(ref Dictionary<string, AudioClip> ___clipsSE, ref Dictionary<string, AudioClip> ___clipsVoice)
        {
            foreach (String file in Directory.GetFiles(Path.Combine(AssetLoaderPlugin.DTPDataPath, "sound/se")))
            {
                if (file.EndsWith(".wav"))
                    ___clipsSE[Path.GetFileNameWithoutExtension(file)] = WavUtility.ToAudioClip(file);
            }

            foreach (String file in Directory.GetFiles(Path.Combine(AssetLoaderPlugin.DTPDataPath, "sound/voice")))
            {
                if (file.EndsWith(".wav"))
                    ___clipsVoice[Path.GetFileNameWithoutExtension(file)] = WavUtility.ToAudioClip(file);
            }
        }
    }
}