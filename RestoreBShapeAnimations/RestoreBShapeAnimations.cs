using BepInEx;
using HarmonyLib;
using App.Klonoa2;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RestoreBShapeAnimations
{
    [BepInPlugin("restore_bshape_animations", "Restore Blend Shape Animations (LV)", "1.1.1")]
    [BepInProcess("Klonoa.exe")]
    public class RestoreBShapeAnimationsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogMessage("Reading animation data...");
            byte[] animationsBytes = RestoreBShapeAnimations.Properties.Resources.animations;
            string animationsJson = System.Text.Encoding.UTF8.GetString(animationsBytes);
            Game__SetSyncMime.Animations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, AnimationData[,]>>>(animationsJson);
            var harmony = new Harmony("restore_bshape_animations");
            harmony.PatchAll();
            Logger.LogInfo("Plugin restore_bshape_animations is loaded!");
        }
    }

    public class AnimationData
    {
        public int Start;
        public int End;
        public float Weight;
    }

    [HarmonyPatch(typeof(Game), "SetSyncMime")]
    public class Game__SetSyncMime
    {
        public static Dictionary<string, Dictionary<string, AnimationData[,]>> Animations;

        [HarmonyPrefix]
        public static void Prefix(Game.PSFXOBJ pObj, ref bool __runOriginal)
        {
            if (pObj.chrInfo == null || pObj.chrInfo.actStateInfos == null || pObj.actNum == 0) return;
            ActStateInfo actStateInfo = pObj.chrInfo.actStateInfos.Search(pObj.actNum);
            if (actStateInfo == null) return;
            SyncMimeInfo[] syncMimeInfos = actStateInfo.syncMimeInfos;
            if (syncMimeInfos == null) return;
            string modelName = pObj.chrInfo.id;
            if (modelName.StartsWith("CHR_KL"))
                modelName = "CHR_KL"; // Fixes animations for DLC models
            if (Animations.ContainsKey(modelName) && Animations[modelName].ContainsKey(actStateInfo.name) && syncMimeInfos.Length == 0)
            {
                __runOriginal = false;

                float num1 = 0.0f;
                float actEndCnt = Game.GetActEndCnt(pObj);
                float actCnt = Game.GetActCnt(pObj, false);
                if (actCnt > 0.0f && actEndCnt > 0.0f)
                num1 = actCnt / actEndCnt;
                AnimationData[,] channels = Animations[modelName][actStateInfo.name];
                int index = (int)actCnt % (int)actEndCnt;
                if (index >= channels.GetLength(1)) index = channels.GetLength(1) - 1; // Prevent out of range errors

                // TODO: The game doesn't reset blend shape weights when switching animations - find a way to fix it
                for (int i = 0; i < channels.GetLength(0); i++)
                {
                    AnimationData data = channels[i, index];
                    float weight = data.Weight;
                    if (data.Start == data.End && weight == 0.0f) weight = 1.0f;
                    Game.MimeSet(pObj, data.Start, data.End, weight);
                }
            }
        }
    }
}
