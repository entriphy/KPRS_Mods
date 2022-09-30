using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BeautifyUtility
{
    public static void SaveJson(BeautifyEffect.BeautifyProfile profile, string location)
    {
        string json = JValue.Parse(JsonUtility.ToJson(profile)).ToString(Formatting.Indented);
        using (StreamWriter writer = new StreamWriter(location))
        {
            writer.WriteLine(json);
        }
    }

    public static BeautifyEffect.BeautifyProfile LoadJson(string location)
    {
        BeautifyEffect.BeautifyProfile profile = ScriptableObject.CreateInstance<BeautifyEffect.BeautifyProfile>();
        string json = File.ReadAllText(location);
        JsonUtility.FromJsonOverwrite(json, profile);
        return profile;
    }
}