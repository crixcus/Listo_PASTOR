using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<Item> items = new List<Item>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            items.Clear(); // Make sure inventory starts empty
            Debug.Log("Inventory instance set. Inventory cleared on start.");
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Add item to inventory
    public void AddItem(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("Trying to add a null item!");
            return;
        }

        items.Add(item);
        Debug.Log("Added item to inventory: " + item.itemName);
        Debug.Log("Current inventory count: " + items.Count);
    }

    // Remove item (optional)
    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log("Removed item from inventory: " + item.itemName);
            Debug.Log("Current inventory count: " + items.Count);
        }
        else
        {
            Debug.LogWarning("Item not found in inventory: " + item.itemName);
        }
    }
}
