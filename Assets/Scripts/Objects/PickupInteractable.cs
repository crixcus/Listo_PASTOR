using UnityEngine;
using UnityEngine.Events;
public class PickupInteractable : MonoBehaviour
{
    private Outline outline;
    public string message;
    public string pickupMessage = "Picked Up Canned Goods";
    public UnityEvent onInteraction;

    private void Start()
    {
        outline = GetComponent<Outline>();
        DisableOutline();
    }

    public void Interact()
    {
        if (onInteraction != null)
        {
            Debug.Log($"Picked up: {gameObject.name}");
            onInteraction.Invoke();
            NotificationSystem.Instance.ShowNotification(pickupMessage);
        }
    }

    public void EnableOutline()
    {
        if (outline != null)
            outline.enabled = true;
    }

    public void DisableOutline()
    {
        if (outline != null)
            outline.enabled = false;
    }
}