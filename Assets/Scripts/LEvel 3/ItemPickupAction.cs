using UnityEngine;
public class ItemPickupAction : MonoBehaviour
{
    public string pickupMessage = "Picked Up Item";

    // CALLED by Interactable
    public void Pickup()
    {
        Destroy(gameObject);
    }
}