using System;
using BepInEx;
using HarmonyLib;
using App.Klonoa2;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RestoreBShapeAnimations
{
    [BepInPlugin("restore_bshape_animations", "Klonoa 2 - Restore Blend Shape Animations", "1.0.0")]
    [BepInProcess("Klonoa.exe")]
    public class RestoreBShapeAnimationsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogMessage("Reading animation data...");
            byte[] animationsBytes = RestoreBShapeAnimations.Properties.Resources.animations;
            string animationsJson = System.Text.Encoding.UTF8.GetString(animationsBytes);
            Game__SetSyncMime.Animations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, AnimationData[,]>>>(animationsJson);
            Logger.LogInfo("Plugin restore_bshape_animations is loaded!");
            var harmony = new Harmony("restore_bshape_animations");
            harmony.PatchAll();
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
            __runOriginal = false;
            if (pObj.chrInfo == null || pObj.chrInfo.actStateInfos == null || pObj.actNum == 0) return;
            ActStateInfo actStateInfo = pObj.chrInfo.actStateInfos.Search(pObj.actNum);
            foreach (var info in pObj.chrInfo.actStateInfos.infos)
            {
                info.stop = 1;
            }
            if (actStateInfo == null) return;
            SyncMimeInfo[] syncMimeInfos = actStateInfo.syncMimeInfos;
            if (syncMimeInfos == null) return;
            float num1 = 0.0f;
            float actEndCnt = Game.GetActEndCnt(pObj);
            float actCnt = Game.GetActCnt(pObj, false);
            if (actCnt > 0.0f && actEndCnt > 0.0f)
                num1 = actCnt / actEndCnt;
            if (Animations.ContainsKey(pObj.chrInfo.id) && Animations[pObj.chrInfo.id].ContainsKey(actStateInfo.name))
            {
                AnimationData[,] channels = Animations[pObj.chrInfo.id][actStateInfo.name];
                int index = (int)actCnt % (int)actEndCnt;
                if (index >= channels.GetLength(1)) index = channels.GetLength(1) - 1; // Prevent out of range errors

                // TODO: The game doesn't reset blend shape weights when switching animations - find a way to fix it
                for (int i = 0; i < channels.GetLength(0); i++)
                {
                    AnimationData data = channels[i, index];
                    Game.MimeSet(pObj, data.Start, data.End, data.Weight);
                }
            } else if (syncMimeInfos.Length != 0)
            {
                for (int index = 0; index < syncMimeInfos.Length; ++index)
                {
                    SyncMimeInfo syncMimeInfo = syncMimeInfos[index];
                    if (syncMimeInfo != null)
                    {
                        float Weight = num1;
                        switch (syncMimeInfo.mode)
                        {
                            case SyncMimeInfo.Mode.Clamp:
                                Weight = Mathf.Clamp(Weight, 0.0f, 1f);
                                break;
                            case SyncMimeInfo.Mode.Repeat:
                                Weight %= 1f;
                                break;
                            case SyncMimeInfo.Mode.Mirror:
                                int num2 = ((uint) (int) Weight & 1U) > 0U ? 1 : 0;
                                Weight %= 1f;
                                if (num2 != 0)
                                {
                                    Weight = 1f - Weight;
                                    break;
                                }
                                break;
                            case SyncMimeInfo.Mode.Zero:
                                Weight = 0.0f;
                                break;
                            case SyncMimeInfo.Mode.One:
                                Weight = 1f;
                                break;
                            case SyncMimeInfo.Mode.MirrorX2:
                                float num3 = Weight % 1f;
                                Weight = 0.5 <= (double) num3 ? (float) (1.0 - ((double) num3 - 0.5) * 2.0) : num3 * 2f;
                                break;
                            case SyncMimeInfo.Mode.ReverseMirrorX2:
                                float num4 = Weight % 1f;
                                Weight = 1f - (0.5 <= (double) num4 ? (float) (1.0 - ((double) num4 - 0.5) * 2.0) : num4 * 2f);
                                break;
                        }
                        Game.MimeSet(pObj, syncMimeInfo.startNum, syncMimeInfo.endNum, Weight);
                    }
                }
            }
        }
    }
}
