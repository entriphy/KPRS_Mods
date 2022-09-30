using System;
using System.IO;
using HarmonyLib;
using UnityEngine;
using nsPFW;

namespace AssetLoader.Loaders.DTP
{
    // Load Beautify profile if it exists
    [HarmonyPatch(typeof(nsPFW.Global), "SetMainCameraObject")]
    public class Global__SetMainCameraObject
    {
        public static string CurrentScene;

        public static void LoadBeautifyProfile()
        {
            Global global = SingletonMonoBehaviour<Global>.Instance;
            string customPath = Path.Combine(AssetLoaderPlugin.DTPDataPath, $"beautify/{CurrentScene}.json").ToLower();
            if (File.Exists(customPath) && global.beautify != null)
            {
                BeautifyEffect.BeautifyProfile profile = BeautifyUtility.LoadJson(customPath);
                profile.Load(global.beautify);
                global.SetBeautifyQuality();
                Console.WriteLine("Loaded Beautify profile: " + customPath);
            }
        }

        [HarmonyPostfix]
        public static void Postfix(UnityEngine.Camera cam)
        {
            CurrentScene = cam.gameObject.scene.name;
            LoadBeautifyProfile();
        }
    }

    // Save Beautify profile when player presses F4, reload profile when player presses F5
    [HarmonyPatch(typeof(nsPFW.Global), "Update")]
    public class Global__Update
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Global global = SingletonMonoBehaviour<Global>.Instance;
                if (global != null && global.beautify != null && Global__SetMainCameraObject.CurrentScene != null)
                {
                    Console.WriteLine(Global__SetMainCameraObject.CurrentScene);
                    BeautifyEffect.BeautifyProfile profile = ScriptableObject.CreateInstance<BeautifyEffect.BeautifyProfile>();
                    profile.Save(global.beautify);
                    string customPath = Path.Combine(AssetLoaderPlugin.DTPDataPath, $"beautify/{Global__SetMainCameraObject.CurrentScene}.json").ToLower();
                    BeautifyUtility.SaveJson(profile, customPath);
                    Console.WriteLine("Wrote Beautify profile: " + customPath);
                }
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Global__SetMainCameraObject.LoadBeautifyProfile();
            }
        }
    }
}