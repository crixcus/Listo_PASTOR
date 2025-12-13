using UnityEngine;


[DisallowMultipleComponent]
public class HouseObject : MonoBehaviour
{
    [Tooltip("Permanent unique identifier. Assign once in editor (use context menu to auto-assign).")]
    public string uniqueID;


    [Tooltip("Name of prefab used to spawn this object if missing on load (optional for static scene objects).")]
    public string prefabKey;


    [Tooltip("If true, object spawns at saved state even if it didn't exist in scene initially.")]
    public bool isRuntimeSpawnable = false;


    // Called to capture state from this object
    public HouseObjectState CaptureState()
    {
        return new HouseObjectState
        {
            id = uniqueID,
            prefabKey = prefabKey,
            position = new float[] { transform.position.x, transform.position.y, transform.position.z },
            rotation = new float[] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z },
            active = gameObject.activeSelf
        };
    }


    // Apply a saved state to this object
    public void ApplyState(HouseObjectState s)
    {
        if (s.position == null || s.position.Length != 3)
        {
            Debug.LogError($"Invalid position array for {gameObject.name}. Expected 3 elements, got {(s.position?.Length ?? 0)}.");
            return;
        }
        if (s.rotation == null || s.rotation.Length != 3)
        {
            Debug.LogError($"Invalid rotation array for {gameObject.name}. Expected 3 elements, got {(s.rotation?.Length ?? 0)}.");
            return;
        }
        transform.position = new Vector3(s.position[0], s.position[1], s.position[2]);
        transform.eulerAngles = new Vector3(s.rotation[0], s.rotation[1], s.rotation[2]);
        gameObject.SetActive(s.active);
    }
}