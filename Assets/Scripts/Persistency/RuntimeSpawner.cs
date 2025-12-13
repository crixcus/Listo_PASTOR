using UnityEngine;


public class RuntimeSpawner : MonoBehaviour
{
    // Example helper: spawn prefab by key and register in SaveManager
    public static GameObject Spawn(string prefabKey, Vector3 pos, Quaternion rot)
    {
        if (PrefabDatabase.Instance == null)
        {
            Debug.LogError("PrefabDatabase.Instance is null. Cannot spawn prefab.");
            return null;
        }
        var prefab = PrefabDatabase.Instance.GetPrefabByKey(prefabKey);
        if (prefab == null)
        {
            Debug.LogError($"Prefab key {prefabKey} not found.");
            return null;
        }


        var go = GameObject.Instantiate(prefab, pos, rot);
        var ho = go.GetComponent<HouseObject>();
        if (ho == null)
        {
            ho = go.AddComponent<HouseObject>();
            ho.uniqueID = System.Guid.NewGuid().ToString();
            ho.prefabKey = prefabKey;
            ho.isRuntimeSpawnable = true;
        }


        // Optionally mark DontDestroy if you want it to persist physically between scenes
        // DontDestroyOnLoad(go);


        return go;
    }
}