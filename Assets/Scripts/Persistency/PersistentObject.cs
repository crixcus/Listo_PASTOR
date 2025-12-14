using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    // The Awake method is called when the script instance is being loaded,
    // which is the ideal time to make the object persistent.
    void Awake()
    {
        // 1. Check if an instance of this object already exists
        // This is crucial to prevent multiple copies when returning to the scene
        // where the object was first created.
        int objectCount = FindObjectsOfType<PersistentObject>().Length;

        if (objectCount > 1)
        {
            // 2. If a duplicate is found, destroy this new one immediately
            Destroy(gameObject);
        }
        else
        {
            // 3. Otherwise, if it's the first instance, make it persistent
            // This is the core function call that prevents destruction on scene load.
            DontDestroyOnLoad(gameObject);
        }
    }
}