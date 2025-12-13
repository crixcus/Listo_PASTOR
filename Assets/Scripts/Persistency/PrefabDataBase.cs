using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PrefabDatabase", menuName = "Persistence/PrefabDatabase")]
public class PrefabDatabase : ScriptableObject
{
    public List<PrefabEntry> entries = new List<PrefabEntry>();


    public static PrefabDatabase Instance { get; private set; }


    void OnEnable()
    {
        // cache instance for quick access in editor/runtime
        // Note: If multiple instances exist, the last one loaded will be used
        Instance = this;
    }


    void OnDisable()
    {
        // Clear instance when disabled to prevent stale references
        if (Instance == this)
        {
            Instance = null;
        }
    }


    public GameObject GetPrefabByKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("GetPrefabByKey called with null or empty key.");
            return null;
        }
        var e = entries.Find(x => x.key == key);
        return e != null ? e.prefab : null;
    }
}


[System.Serializable]
public class PrefabEntry
{
    public string key;
    public GameObject prefab;
}