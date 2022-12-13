using System;
using System.IO;
using HarmonyLib;
using UnityEngine;
using nsPFW;

namespace AssetLoader.Loaders.DTP
{
    // Load field bsb file if it exists
    [HarmonyPatch(typeof(nsPFW.CDVDLoader), "pImage")]
    public class CDVDLoader__pImage
    {
        [HarmonyPrefix]
        public static void Prefix(ref nsPFW.CDVDLoader __instance, ref bool __runOriginal, ref object __result)
        {
            string filename = __instance.m_File.nameResource;
            Console.Write("AssetLoader: Game is loading file " + filename);
            if (filename.StartsWith("bs/field/") && filename.EndsWith(".bsb"))
            {
                string customPath = Path.Combine(AssetLoaderPlugin.DTPDataPath, filename).ToLower();
                if (File.Exists(customPath))
                {
                    __result = File.ReadAllBytes(customPath);
                    __runOriginal = false;
                    Console.WriteLine("AssetLoader: Loaded file " + filename);
                }
            }
        }
    }

    // Load action/demo bsb file if it exists
    [HarmonyPatch(typeof(nsPFW.CCachedLoader), "Image")]
    public class CCachedLoader__Image
    {
        [HarmonyPrefix]
        public static void Prefix(uint filename, ref bool __runOriginal, ref object __result)
        {
            string fileName = CFileName.pName(filename).ToLower();
            Console.Write("AssetLoader: Game is loading file " + fileName);
            if (fileName.StartsWith("bs/") && fileName.EndsWith(".bsb"))
            {
                string customPath = Path.Combine(AssetLoaderPlugin.DTPDataPath, fileName).ToLower();
                if (File.Exists(customPath))
                {
                    __result = File.ReadAllBytes(customPath);
                    __runOriginal = false;
                    Console.WriteLine("AssetLoader: Loaded file " + fileName);
                }
            }
        }
    }
}