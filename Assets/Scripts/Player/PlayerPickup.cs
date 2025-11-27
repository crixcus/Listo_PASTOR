using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public LayerMask pickupLayer;

    [Header("Inventory (Displayed in Inspector Only)")]
    public string[] itemNames;      // names of items collected
    public int[] itemCounts;        // how many you have

    // Dictionary for runtime item counting (not visible in inspector)
    private System.Collections.Generic.Dictionary<string, int> inventoryDict =
        new System.Collections.Generic.Dictionary<string, int>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupItem();
        }
    }

    void TryPickupItem()
    {
        // Check for nearby pickup items
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);

        if (hits.Length > 0)
        {
            PickupItem item = hits[0].GetComponent<PickupItem>();

            if (item != null)
            {
                AddToInventory(item.itemName);
                Destroy(item.gameObject);
                Debug.Log("Picked up: " + item.itemName);
            }
        }
    }

    void AddToInventory(string itemName)
    {
        // Add to dictionary
        if (inventoryDict.ContainsKey(itemName))
        {
            inventoryDict[itemName]++;
        }
        else
        {
            inventoryDict[itemName] = 1;
        }

        // Update Inspector arrays (just for display)
        itemNames = new string[inventoryDict.Count];
        itemCounts = new int[inventoryDict.Count];

        int index = 0;
        foreach (var pair in inventoryDict)
        {
            itemNames[index] = pair.Key;
            itemCounts[index] = pair.Value;
            index++;
        }
    }
}
