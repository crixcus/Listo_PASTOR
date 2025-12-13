using System.IO;
using UnityEngine;


public static class SaveFileHandler
{
    static string path => Path.Combine(Application.persistentDataPath, "scene_save.json");


    public static void SaveToFile(SceneSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved to {path}");
    }


    public static SceneSaveData LoadFromFile()
    {
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SceneSaveData>(json);
    }
}