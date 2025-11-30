using UnityEngine;

public class PlayerCarrySystem : MonoBehaviour
{
    public static PlayerCarrySystem Instance;

    public ObjectPickup PickupHandler; // assign via inspector
    public string CurrentObjectName { get; private set; }
    public GameObject CarriedPrefab { get; private set; }


    void Awake()
    {
        Instance = this;
    }

    public void SetCarriedObject(string name, GameObject prefab)
    {
        CurrentObjectName = name;
        CarriedPrefab = prefab;
    }

    public void ClearCarriedObject()
    {
        CurrentObjectName = null;
        CarriedPrefab = null;
    }

   // use this to check if player is currently holding something
    public bool IsCarryingSomething
    {
        get { return !string.IsNullOrEmpty(CurrentObjectName) && CarriedPrefab != null; }
    }
}
