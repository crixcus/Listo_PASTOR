using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickableObject : MonoBehaviour
{
    public string objectName;          // name to match placeholder
    public GameObject prefabReference; // assign the prefab version of this object
    
    [SerializeField] private bool isPlaced = false; // Track if this object has been placed

    private void OnMouseDown()
    {
        if (PlayerCarrySystem.Instance == null) return;
        
        // Don't allow pickup if this object has been placed
        if (isPlaced) return;

        // only pick up if not carrying anything
        if (!PlayerCarrySystem.Instance.IsCarryingSomething)
        {
            PlayerCarrySystem.Instance.SetCarriedObject(objectName, prefabReference);
            Destroy(gameObject); // remove from scene
        }
    }
    
    // Method to mark this object as placed (called by PlaceholderPlacer)
    public void MarkAsPlaced()
    {
        isPlaced = true;
    }
}
