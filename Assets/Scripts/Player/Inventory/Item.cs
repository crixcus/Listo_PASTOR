using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;   // Name of the item
    public Sprite icon;       // Icon to show in inventory
    public string description; // Optional description
}
