using BepInEx;
using HarmonyLib;
using System.IO;

namespace AssetLoader
{
    [BepInPlugin("asset_loader", "Asset Loader", "1.0.2")]
    [BepInProcess("Klonoa.exe")]
    public class AssetLoaderPlugin : BaseUnityPlugin
    {
        public static string DTPDataPath;
        public static string LVDataPath;

        private void Awake()
        {
            // DTP
            DTPDataPath = Path.Combine(Paths.PluginPath, "AssetLoader", "dtp");
            Directory.CreateDirectory(Path.Combine(DTPDataPath, "beautify"));
            Directory.CreateDirectory(Path.Combine(DTPDataPath, "sound/bgm"));
            Directory.CreateDirectory(Path.Combine(DTPDataPath, "sound/se"));
            Directory.CreateDirectory(Path.Combine(DTPDataPath, "sound/voice"));

            // LV
            LVDataPath = Path.Combine(Paths.PluginPath, "AssetLoader", "lv");
            Directory.CreateDirectory(Path.Combine(LVDataPath, "beautify"));
            Directory.CreateDirectory(Path.Combine(LVDataPath, "chr_infos"));
            Directory.CreateDirectory(Path.Combine(LVDataPath, "sound/bgm"));
            Directory.CreateDirectory(Path.Combine(LVDataPath, "sound/ppt"));
            Directory.CreateDirectory(Path.Combine(LVDataPath, "sound/se"));

            var harmony = new Harmony("asset_loader");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin asset_loader is loaded!");
        }
    }
}
