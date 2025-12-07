using UnityEngine;

public class ScenarioHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            ItemData item = other.GetComponent<ItemData>();

            if (item == null)
            {
                Debug.LogError("Object tagged Interactable has no ItemData component!");
                return;
            }

            NotificationSystem.Instance.ShowNotification(
                $"You have Collected: {item.itemName}!"
            );
        }
    }
}
