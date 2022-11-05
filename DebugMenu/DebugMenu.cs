using System;
using BepInEx;
using HarmonyLib;
using App.Klonoa2;
using UnityEngine;
using BepInEx.Configuration;

namespace DebugMenu
{
    [BepInPlugin("debug_menu", "Debug Menu", "2.0.0")]
    [BepInProcess("Klonoa.exe")]
    public class DebugMenuPlugin : BaseUnityPlugin
    {
        public static bool configEnableLog;
        public static bool configFixDLCMenu;

        private void Awake()
        {
            configEnableLog = Config.Bind<bool>("General", "EnableDebugLog", false, "Enables debug messages from the game. Logging.UnityLogListening must be enabled in the main BepInEx config.").Value;
            configFixDLCMenu = Config.Bind<bool>("General", "FixDLCMenu", true, "Fixes the DLC menu in the debug menu so that it properly enables/disables DLC costumes.").Value;

            var harmony = new Harmony("debug_menu");
            harmony.PatchAll();
            Logger.LogInfo("Plugin debug_menu is loaded!");
        }
    }

    // Enables debug log output
    [HarmonyPatch(typeof(Log), "Init")]
    public class Log__Init
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Debug.unityLogger.logEnabled = DebugMenuPlugin.configEnableLog;
        }
    }

    #region DTP
    // Enables debug menu in DTP
    [HarmonyPatch(typeof(nsPFW.MainController), "Awake")]
    public class nsPFW__MainController__Awake
    {
        [HarmonyPostfix]
        public static void Postfix(ref nsPFW.MainController __instance)
        {
            Traverse objDebug = Traverse.Create(__instance).Field("objDebug"); // Get private objDebug field
            GameObject debugMenuObj = UnityEngine.Object.Instantiate<GameObject>(__instance.prefabDebug); // Instantiate debug menu prefab
            debugMenuObj.transform.SetParent(__instance.objUIFront.transform, false); // Add debug menu to canvas
            objDebug.SetValue(debugMenuObj); // Set objDebug to debug menu object
            debugMenuObj.SetActive(false); // Disable debug menu by default
        }
    }

    // Log menu option
    // Remove this when GUI is working
    [HarmonyPatch(typeof(nsPFW.DebugMenuController), "Update")]
    public class nsPFW__DebugMenuController__Update
    {
        [HarmonyPostfix]
        public static void Postfix(ref nsPFW.DebugMenuController __instance)
        {
            Traverse traverse = Traverse.Create(__instance).Field("selectMenu");
            // Debug.Log(traverse.GetValue());
        }
    }
    #endregion

    #region LV
    // Enables debug menu in LV
    [HarmonyPatch(typeof(DebugSetting), "Awake")]
    public class DebugSetting__Awake
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            DebugSetting.instance.useDebugAction = true;
        }
    }

    // Fixes DLC menu in LV
    [HarmonyPatch(typeof(Game), "DLCCheck")]
    public class Game__DLCCheck
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __runOriginal)
        {
            // Only run if a value was changed in the DLC menu
            if (DebugMenuPlugin.configFixDLCMenu && Array.Exists(Game.debugDLC, element => element))
            {
                __runOriginal = false;
                for (int i = 0; i < Game.EnableDLC.Length; i++)
                {
                    Game.EnableDLC[i] = Game.debugDLC[i];
                }
            }
        }
    }

    // Log puppet/cutscene messages in LV
    [HarmonyPatch(typeof(Game.HR_PREAD), "pt_log")]
    public class Game__HR_PREAD__pt_log
    {
        [HarmonyPostfix]
        public static void Prefix(Game.HR_CALL ca, string text)
        {
            Debug.Log("PT: " + text);
        }
    }
    #endregion
}
