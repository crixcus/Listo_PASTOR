using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;      // Name of the item
    public Texture icon;         // Texture for RawImage
    public string description;   // Optional
}
