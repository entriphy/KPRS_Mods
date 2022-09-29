using System;
using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using App.Klonoa2;

namespace AssetLoader.Loaders
{

    [HarmonyPatch(typeof(App.Klonoa2.Game.PSFXOBJ), "LinkUnity")]
    public class PSFXOBJ__LinkUnity
    {
        [HarmonyPostfix]
        public static void Postfix(ref Game.PSFXOBJ __instance, ref GameObject ____go)
        {
            if (____go != null)
            {
                string modelName = ____go.name.Replace("(Clone)", "");
                string customPath = Path.Combine(AssetLoaderPlugin.LVDataPath, $"chr_infos/{modelName}").ToLower();
                if (Directory.Exists(customPath))
                {
                    foreach (var transform in ____go.GetComponentsInChildren<Transform>())
                    {
                        string modelPath = Path.Combine(AssetLoaderPlugin.LVDataPath, $"chr_infos/{modelName}/{transform.name}.gltf").ToLower();
                        if (File.Exists(modelPath))
                        {
                            GameObject go = transform.gameObject;
                            foreach (var smr in go.GetComponents<SkinnedMeshRenderer>())
                            {
                                var model = SharpGLTF.Schema2.ModelRoot.Load(modelPath);
                                var newMesh = new Mesh();
                                newMesh.Clear();
                                var newVertices = new List<UnityEngine.Vector3>();
                                var newNormals = new List<UnityEngine.Vector3>();
                                var newUVs = new List<UnityEngine.Vector2>();
                                var newIndices = new List<int>();
                                foreach (var v in model.LogicalMeshes[0].Primitives[0].GetVertices("POSITION").AsVector3Array())
                                {
                                    newVertices.Add(new Vector3(v.X * 0.05f, -v.Z * 0.05f, v.Y * 0.05f));
                                }
                                foreach (var v in model.LogicalMeshes[0].Primitives[0].GetVertices("NORMAL").AsVector3Array())
                                {
                                    newNormals.Add(new Vector3(v.X, -v.Z, v.Y));
                                }
                                foreach (var v in model.LogicalMeshes[0].Primitives[0].GetVertices("TEXCOORD_0").AsVector2Array())
                                {
                                    newUVs.Add(new Vector2(v.X, 1.0f - v.Y));
                                }
                                foreach (var i in model.LogicalMeshes[0].Primitives[0].GetTriangleIndices())
                                {
                                    newIndices.Add(i.A);
                                    newIndices.Add(i.B);
                                    newIndices.Add(i.C);
                                }
                                newMesh.SetVertices(newVertices);
                                newMesh.SetNormals(newNormals);
                                newMesh.SetTriangles(newIndices, 0);
                                newMesh.SetUVs(0, newUVs);
                                
                                byte[] fileData;
                                using (var memoryStream = new MemoryStream())
                                {
                                    model.LogicalImages[0].Content.Open().CopyTo(memoryStream);
                                    fileData = memoryStream.ToArray();
                                }
                                
                                Texture2D tex = new Texture2D(128, 128);
                                tex.LoadImage(fileData);
                                newMesh.bindposes = smr.sharedMesh.bindposes;
                                // newMesh.boneWeights = new BoneWeight[newMesh.vertices.Length];
                                // for (int i = 0; i < newMesh.boneWeights.Length; i++)
                                // {
                                //     newMesh.boneWeights[i].boneIndex0 = 0;
                                //     newMesh.boneWeights[i].weight0 = 1.0f;
                                // }
                                // foreach (var weight in smr.sharedMesh.boneWeights)
                                // {
                                //     Console.WriteLine(weight.boneIndex0);
                                //     Console.WriteLine(weight.weight0);
                                // }
                                smr.material.mainTexture = tex;
                                smr.sharedMesh = newMesh;
                                
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}