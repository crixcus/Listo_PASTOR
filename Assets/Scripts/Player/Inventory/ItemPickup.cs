using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // i-assign sa inspector kung ano ini nga item

    // i-call ini pag na-interact sa object
    public void PickUp()
    {
        if (item != null)
        {
            Inventory.instance.AddItem(item); // add sa inventory
            Debug.Log("Picked up: " + item.itemName);
        }

        // disable or destroy object
        gameObject.SetActive(false);
    }
}
