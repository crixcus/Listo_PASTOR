using UnityEngine;
public class ItemPickupAction : MonoBehaviour
{
    public string pickupMessage = "Picked Up Item";
    public Item item; // drag Item ScriptableObject here

    public void Pickup()
    {
        // Add to inventory
        if (Inventory.instance != null)
            Inventory.instance.items.Add(item);

        if (NotificationSystem.Instance != null)
            NotificationSystem.Instance.ShowDebounced("pickup", pickupMessage, 1f);

        Destroy(gameObject);
    }
}