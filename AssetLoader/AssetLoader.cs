using BepInEx;
using HarmonyLib;
using System.IO;

namespace AssetLoader
{
    [BepInPlugin("asset_loader", "Asset Loader", "1.0.0")]
    [BepInProcess("Klonoa.exe")]
    public class AssetLoaderPlugin : BaseUnityPlugin
    {
        public static string LVDataPath;

        private void Awake()
        {
            LVDataPath = Path.Combine(Paths.PluginPath, "AssetLoader", "lv");
            Directory.CreateDirectory(Path.Combine(LVDataPath, "sound/ppt"));
            Directory.CreateDirectory(Path.Combine(LVDataPath, "sound/bgm"));
            Directory.CreateDirectory(Path.Combine(LVDataPath, "chr_infos"));
            var harmony = new Harmony("asset_loader");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin asset_loader is loaded!");
        }
    }
}
