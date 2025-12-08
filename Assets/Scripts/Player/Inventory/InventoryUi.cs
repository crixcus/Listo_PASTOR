using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUi : MonoBehaviour
{
    public GameObject inventoryPanel;   // The panel to show/hide
    public GameObject slotPrefab;       // Prefab for item slots
    public Transform slotsParent;       // Parent object for instantiated slots

    private List<GameObject> currentSlots = new List<GameObject>();

    private void Start()
    {
        // Hide inventory at start
        inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        // Toggle inventory with Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            if (inventoryPanel.activeSelf)
                RefreshUI();
        }
    }

    // Call this whenever you want to update the inventory display
    public void RefreshUI()
    {
        // Clear existing slots
        foreach (var slot in currentSlots)
            Destroy(slot);
        currentSlots.Clear();

        // Create a slot for each item in Inventory
        foreach (var item in Inventory.instance.items)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotsParent);
            Image icon = newSlot.GetComponentInChildren<Image>();
            if (icon != null && item.icon != null)
                icon.sprite = item.icon;

            currentSlots.Add(newSlot);
        }
    }
}
