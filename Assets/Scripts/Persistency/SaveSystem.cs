using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public static class SaveSystem
{
    // Capture all HouseObject instances into SaveManager.Instance.CurrentSave
    public static void CaptureScene()
    {
        if (SaveManager.Instance == null) return;


        SaveManager.Instance.ClearSave();


        var objs = GameObject.FindObjectsByType<HouseObject>(FindObjectsSortMode.None);


        var usedIDs = new HashSet<string>();


        foreach (var o in objs)
        {
            if (string.IsNullOrEmpty(o.uniqueID))
            {
                Debug.LogWarning($"HouseObject on {o.name} has no uniqueID. Skipping.");
                continue;
            }


            if (usedIDs.Contains(o.uniqueID))
            {
                Debug.LogError($"Duplicate uniqueID detected: {o.uniqueID} on {o.name}. Skipping this instance.");
                continue;
            }


            usedIDs.Add(o.uniqueID);


            var s = o.CaptureState();
            SaveManager.Instance.CurrentSave.objects.Add(s);
        }
    }


    // Restore saved states. Spawn prefabs for missing runtime objects if necessary.
    public static void RestoreScene()
    {
        if (SaveManager.Instance == null) return;


        var save = SaveManager.Instance.CurrentSave;
        if (save == null) return;


        // First: apply all saved states to matching objects
        var objs = GameObject.FindObjectsByType<HouseObject>(FindObjectsSortMode.None);
        var objDict = new Dictionary<string, HouseObject>();
        foreach (var o in objs)
        {
            if (!string.IsNullOrEmpty(o.uniqueID))
            {
                if (objDict.ContainsKey(o.uniqueID))
                {
                    Debug.LogError($"Duplicate uniqueID {o.uniqueID} found in scene. Using first occurrence.");
                }
                else
                {
                    objDict[o.uniqueID] = o;
                }
            }
        }


        // Apply saved states
        foreach (var s in save.objects)
        {
            if (string.IsNullOrEmpty(s.id)) continue;


            if (objDict.TryGetValue(s.id, out var target))
            {
                target.ApplyState(s);
            }
            else
            {
                // Not present: maybe it's a runtime spawnable object
                if (!string.IsNullOrEmpty(s.prefabKey))
                {
                    if (PrefabDatabase.Instance == null)
                    {
                        Debug.LogError("PrefabDatabase.Instance is null. Cannot spawn runtime objects.");
                        continue;
                    }
                    var prefab = PrefabDatabase.Instance.GetPrefabByKey(s.prefabKey);
                    if (prefab != null)
                    {
                        var go = GameObject.Instantiate(prefab);
                        var ho = go.GetComponent<HouseObject>();
                        if (ho == null)
                        {
                            ho = go.AddComponent<HouseObject>();
                            ho.uniqueID = s.id;
                            ho.prefabKey = s.prefabKey;
                        }
                        ho.uniqueID = s.id; // ensure ID
                        go.transform.position = new Vector3(s.position[0], s.position[1], s.position[2]);
                        go.transform.eulerAngles = new Vector3(s.rotation[0], s.rotation[1], s.rotation[2]);
                        go.SetActive(s.active);
                    }
                    else
                    {
                        Debug.LogWarning($"Prefab key '{s.prefabKey}' not found in PrefabDatabase. Cannot spawn {s.id}.");
                    }
                }
                else
                {
                    Debug.Log($"Saved object {s.id} not found in scene and has no prefabKey. Skipping.");
                }
            }
        }
    }
}