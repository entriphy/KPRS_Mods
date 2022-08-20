using BepInEx;
using HarmonyLib;
using UnityEngine;
using App.Klonoa2;

namespace SettingsFix
{
    [BepInPlugin("settings_fix", "settings_fix", "1.1.0")]
    public class SettingsFixPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("settings_fix");
            harmony.PatchAll();
            Logger.LogInfo("Plugin settings_fix is loaded!");
        }
    }

    #region Anti-aliasing patches

    [HarmonyPatch(typeof(StageManager), "SetAntiAliasing")]
    public class StageManager__SetAntiAliasing
    {
        [HarmonyPostfix]
        public static void Postfix(Game.AntiAliasing antiAliasing)
        {
            switch (antiAliasing)
            {
                case Game.AntiAliasing.Disabled:
                    QualitySettings.antiAliasing = 0;
                    break;
                case Game.AntiAliasing.Lv1:
                    QualitySettings.antiAliasing = 2;
                    break;
                case Game.AntiAliasing.Lv2:
                    QualitySettings.antiAliasing = 4;
                    break;
                case Game.AntiAliasing.Lv3:
                    QualitySettings.antiAliasing = 8;
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(nsPFW.Global), "Update")]
    public class Global__Update
    {
        [HarmonyPrefix]
        public static void Prefix(ref nsPFW.Global __instance)
        {
            bool isRequestCheckQuality = Traverse.Create(__instance).Field("isRequestCheckQuality").GetValue<bool>();
            if (isRequestCheckQuality) {
                nsPFW.CSaveDataLauncher.tSystemData system = nsPFW.Global.mainController.saveDataLauncher.m_pCurrentData.m_System;
                switch (system.m_AntiAlias)
                {
                    case 0:
                        QualitySettings.antiAliasing = 0;
                        break;
                    case 1:
                        QualitySettings.antiAliasing = 2;
                        break;
                    case 2:
                        QualitySettings.antiAliasing = 4;
                        break;
                    case 3:
                        QualitySettings.antiAliasing = 8;
                        break;
                }
            }
            
        }
    }

    #endregion

    #region Resolution patches

    [HarmonyPatch(typeof(nsPFWLauncher.MainController), "IsEnableResolution")]
    public class nsPFWLauncher__MainController__IsEnableResolution
    {
        [HarmonyPrefix]
        public static void Prefix(int no, ref nsPFWLauncher.MainController __instance, ref bool __result, ref bool __runOriginal)
        {
            __runOriginal = false;
            __result = false;
            Vector2[] resolutionSize = Traverse.Create(__instance).Field("resolutionSize").GetValue<Vector2[]>();
            Vector2 vector = resolutionSize[no];
            foreach (Resolution resolution in Screen.resolutions)
            {
                if (resolution.width <= vector.x && resolution.height <= vector.y)
                {
                    __result = true;
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(nsPFW.Global), "IsEnableResolution")]
    public class nsPFW__Global__IsEnableResolution
    {
        [HarmonyPrefix]
        public static void Prefix(int no, ref nsPFW.Global __instance, ref bool __result, ref bool __runOriginal)
        {
            __runOriginal = false;
            __result = false;
            Vector2 vector = __instance.resolutionSize[no];
            foreach (Resolution resolution in Screen.resolutions)
            {
                if (resolution.width <= vector.x && resolution.height <= vector.y)
                {
                    __result = true;
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(App.Klonoa2.Game), "IsValidUnityScreenResolution")]
    public class Game__IsValidUnityScreenResolution
    {
        [HarmonyPrefix]
        public static void Prefix(int width, int height, ref bool __result, ref bool __runOriginal)
        {
            __runOriginal = false;
            __result = false;
            for (int index = 0; index < Screen.resolutions.Length; ++index)
            {
                Resolution resolution = Screen.resolutions[index];
                if (resolution.width <= width && resolution.height <= height)
                {
                    __result = true;
                    break;
                }
            }
        }
    }

    #endregion
}
