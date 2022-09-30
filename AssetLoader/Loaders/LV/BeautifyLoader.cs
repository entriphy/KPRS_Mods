using System;
using System.IO;
using HarmonyLib;
using UnityEngine;
using App.Klonoa2;

namespace AssetLoader.Loaders.LV
{
    // Load Beautify profile if it exists
    [HarmonyPatch(typeof(StageManager), "InitBeautify")]
    public class StageManager__InitBeautify
    {
        public static void LoadBeautifyProfile(ref StageManager __instance)
        {
            string customPath = Path.Combine(AssetLoaderPlugin.LVDataPath, $"beautify/{__instance.sceneName}.json").ToLower();
            if (File.Exists(customPath) && __instance.beautify != null)
            {
                BeautifyEffect.BeautifyProfile profile = BeautifyUtility.LoadJson(customPath);
                profile.Load(__instance.beautify);
                __instance.stagePostEffectSettings.Init(__instance.beautify);
                __instance.ReflectDisplaySettings();
                Console.WriteLine("Loaded Beautify profile: " + customPath);
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ref StageManager __instance)
        {
            LoadBeautifyProfile(ref __instance);
        }
    }

    // Save Beautify profile when player presses F4, reload profile when player presses F5
    [HarmonyPatch(typeof(StageManager), "Update")]
    public class StageManager__Update
    {
        [HarmonyPostfix]
        public static void Postfix(ref StageManager __instance)
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                if (__instance.beautify != null)
                {
                    BeautifyEffect.BeautifyProfile profile = ScriptableObject.CreateInstance<BeautifyEffect.BeautifyProfile>();
                    profile.Save(__instance.beautify);
                    string customPath = Path.Combine(AssetLoaderPlugin.LVDataPath, $"beautify/{__instance.sceneName}.json").ToLower();
                    BeautifyUtility.SaveJson(profile, customPath);
                    Console.WriteLine("Wrote Beautify profile: " + customPath);
                }
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                StageManager__InitBeautify.LoadBeautifyProfile(ref __instance);
            }
        }
    }
}