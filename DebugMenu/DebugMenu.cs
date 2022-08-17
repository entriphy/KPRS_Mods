﻿using System;
using BepInEx;
using HarmonyLib;
using App.Klonoa2;
using UnityEngine;
using BepInEx.Configuration;

namespace DebugMenuLV
{
    [BepInPlugin("debug_menu", "Debug Menu", "1.1.0")]
    [BepInProcess("Klonoa.exe")]
    public class DebugMenuPlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableLog;
        private ConfigEntry<bool> configFixDLCMenu;

        private void Awake()
        {
            configEnableLog = Config.Bind<bool>("General", "EnableDebugLog", false, "Enables debug messages from the game. Logging.UnityLogListening must be enabled in the main BepInEx config.");
            configFixDLCMenu = Config.Bind<bool>("General", "FixDLCMenu", true, "Fixes the DLC menu in the debug menu so that it properly enables/disables DLC costumes.");
            Log__Init.enableLog = configEnableLog.Value;
            Game__DLCCheck.fixDLCMenu = configFixDLCMenu.Value;

            var harmony = new Harmony("debug_menu");
            harmony.PatchAll();
            Logger.LogInfo("Plugin debug_menu is loaded!");
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

    [HarmonyPatch(typeof(Game), "DLCCheck")]
    public class Game__DLCCheck
    {
        public static bool fixDLCMenu = false;

        [HarmonyPostfix]
        public static void Prefix(ref bool __runOriginal)
        {
            // Only run if a value was changed in the DLC menu
            if (fixDLCMenu && Array.Exists(Game.debugDLC, element => element))
            {
                __runOriginal = false;
                for (int i = 0; i < Game.EnableDLC.Length; i++)
                {
                    Game.EnableDLC[i] = Game.debugDLC[i];
                }
            }
        }
    }

    [HarmonyPatch(typeof(Game.HR_PREAD), "pt_log")]
    public class Game__HR_PREAD__pt_log
    {
        [HarmonyPostfix]
        public static void Prefix(Game.HR_CALL ca, string text)
        {
            Debug.Log("PT: " + text);
        }
    }

    [HarmonyPatch(typeof(nsPFW.MainController), "Awake")]
    public class nsPFW__MainController__Awake
    {
        [HarmonyPostfix]
        public static void Postfix(ref nsPFW.MainController __instance)
        {
            Traverse objDebug = Traverse.Create(__instance).Field("objDebug");
            GameObject obj = UnityEngine.Object.Instantiate<GameObject>(__instance.prefabDebug);
            objDebug.SetValue(obj);
        }
    }

    [HarmonyPatch(typeof(nsPFW.DebugMenuController), "Update")]
    public class nsPFW__DebugMenuController__Update
    {
        [HarmonyPostfix]
        public static void Postfix(ref nsPFW.DebugMenuController __instance)
        {
            Traverse traverse = Traverse.Create(__instance).Field("selectMenu");
            Debug.Log(traverse.GetValue());
        }
    }
}
