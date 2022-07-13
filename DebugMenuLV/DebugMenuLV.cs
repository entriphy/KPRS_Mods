using System;
using BepInEx;
using HarmonyLib;

namespace DebugMenu
{
    [BepInPlugin("debug_menu_lv", "Klonoa 2 Debug Menu", "1.0.0")]
    [BepInProcess("Klonoa.exe")]
    public class DebugMenuPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Plugin debug_menu_lv is loaded!");
            var harmony = new Harmony("debug_menu_lv");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(App.Klonoa2.DebugSetting), "Awake")]
    public class SoundManager_PlayVoice
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            App.Klonoa2.DebugSetting.instance.useDebugAction = true;
        }
    }
}
