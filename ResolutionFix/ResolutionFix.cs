using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace ResolutionFix
{
    [BepInPlugin("resolution_fix", "Resolution Fix", "1.0.0")]
    public class ResolutionFixPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("resolution_fix");
            harmony.PatchAll();
            Logger.LogInfo("Plugin resolution_fix is loaded!");
        }
    }

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
}
