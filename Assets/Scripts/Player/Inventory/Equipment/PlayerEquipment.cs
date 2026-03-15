using UnityEngine;
using System.Collections.Generic;
public class PlayerEquipment : MonoBehaviour
{
    [System.Serializable]
    public class EquipSlot
    {
        public KeyCode key;
        public Item item;
        public GameObject visual;
        public bool isMop; // check this in Inspector for the mop slot
    }
    public List<EquipSlot> equipSlots = new List<EquipSlot>();
    private EquipSlot currentlyEquipped;
    private PlayerCleaning playerCleaning;
    void Start()
    {
        playerCleaning = GetComponent<PlayerCleaning>();
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
        if (!Inventory.instance.items.Contains(slot.item))
            return;
        // Unequip if same item pressed again
        if (currentlyEquipped == slot)
        {
            slot.visual.SetActive(false);
            currentlyEquipped = null;
            // If it was the mop, unequip it
            if (slot.isMop && playerCleaning != null)
                playerCleaning.EquipMop(null);
            return;
        }
        // Unequip current item
        if (currentlyEquipped != null)
        {
            currentlyEquipped.visual.SetActive(false);
            // Unequip mop if switching away from it
            if (currentlyEquipped.isMop && playerCleaning != null)
                playerCleaning.EquipMop(null);
        }
        // Equip new item
        slot.visual.SetActive(true);
        currentlyEquipped = slot;
        // If it's the mop, equip it in PlayerCleaning
        if (slot.isMop && playerCleaning != null)
        {
            MopTool mop = slot.visual.GetComponent<MopTool>();
            playerCleaning.EquipMop(mop);
        }
    }
}