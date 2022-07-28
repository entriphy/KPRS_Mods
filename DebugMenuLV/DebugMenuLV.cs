using System;
using BepInEx;
using HarmonyLib;
using App.Klonoa2;
using UnityEngine;
using BepInEx.Configuration;

namespace DebugMenuLV
{
    [BepInPlugin("debug_menu_lv", "Klonoa 2 Debug Menu", "1.0.0")]
    [BepInProcess("Klonoa.exe")]
    public class DebugMenuPlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableLog;
        private void Awake()
        {
            configEnableLog = Config.Bind<bool>("General", "EnableDebugLog", false, "Enables debug messages from the game. Logging.UnityLogListening must be enabled in the main BepInEx config.");
            Log__Init.enableLog = configEnableLog.Value;
            var harmony = new Harmony("debug_menu_lv");
            harmony.PatchAll();
            Logger.LogInfo("Plugin debug_menu_lv is loaded!");
        }
    }

    [HarmonyPatch(typeof(DebugSetting), "Awake")]
    public class DebugSetting__Awake
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            DebugSetting.instance.useDebugAction = true;
        }
    }

    [HarmonyPatch(typeof(Log), "Init")]
    public class Log__Init
    {
        public static bool enableLog = false;

        [HarmonyPostfix]
        public static void Postfix()
        {
            Debug.unityLogger.logEnabled = enableLog;
        }
    }
        }
    }
}
