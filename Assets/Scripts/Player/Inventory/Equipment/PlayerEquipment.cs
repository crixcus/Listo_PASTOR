using UnityEngine;
using System.Collections.Generic;

public class PlayerEquipment : MonoBehaviour
{
    [System.Serializable]
    public class EquipSlot
    {
        public KeyCode key;          // 1,2,3,4
        public Item item;            // Item ScriptableObject
        public GameObject visual;    // GameObject on player
    }

    public List<EquipSlot> equipSlots = new List<EquipSlot>();

    private EquipSlot currentlyEquipped;

    void Start()
    {
        // Make sure nothing is equipped at start
        foreach (var slot in equipSlots)
        {
            slot.visual.SetActive(false);
        }
    }

    void Update()
    {
        foreach (var slot in equipSlots)
        {
            if (Input.GetKeyDown(slot.key))
            {
                TryToggleEquip(slot);
            }
        }
    }

    void TryToggleEquip(EquipSlot slot)
    {
        // Player must own the item
        if (!Inventory.instance.items.Contains(slot.item))
            return;

        // Unequip if same item pressed again
        if (currentlyEquipped == slot)
        {
            slot.visual.SetActive(false);
            currentlyEquipped = null;
            return;
        }

        // Unequip current item
        if (currentlyEquipped != null)
        {
            currentlyEquipped.visual.SetActive(false);
        }

        // Equip new item
        slot.visual.SetActive(true);
        currentlyEquipped = slot;
    }
}
