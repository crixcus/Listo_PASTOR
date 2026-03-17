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
        public bool isMop;
    }
    public List<EquipSlot> equipSlots = new List<EquipSlot>();
    private EquipSlot currentlyEquipped;
    private PlayerCleaning playerCleaning;

    void Start()
    {
        playerCleaning = GetComponent<PlayerCleaning>();
        foreach (var slot in equipSlots)
            slot.visual.SetActive(false);
    }

    void Update()
    {
        if (PlacementSystem.Instance != null && PlacementSystem.Instance.IsCarryingObject)
            return;

        foreach (var slot in equipSlots)
        {
            if (Input.GetKeyDown(slot.key))
                TryToggleEquip(slot);
        }
    }

    void TryToggleEquip(EquipSlot slot)
    {
        if (!Inventory.instance.items.Contains(slot.item))
            return;

        // Same key = unequip
        if (currentlyEquipped == slot)
        {
            UnequipCurrent();
            return;
        }

        // Switch item
        if (currentlyEquipped != null)
            UnequipCurrent();

        // Equip new
        slot.visual.SetActive(true);
        currentlyEquipped = slot;

        if (slot.isMop && playerCleaning != null)
        {
            MopTool mop = slot.visual.GetComponent<MopTool>();
            playerCleaning.EquipMop(mop);
        }
    }

    void UnequipCurrent()
    {
        if (currentlyEquipped == null) return;

        currentlyEquipped.visual.SetActive(false);

        if (currentlyEquipped.isMop && playerCleaning != null)
            playerCleaning.EquipMop(null);

        currentlyEquipped = null;
    }

    public void ForceUnequip()
    {
        UnequipCurrent();
    }
}