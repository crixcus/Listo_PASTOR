using UnityEngine;
using UnityEngine.Events;
public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float playerReach = 3f;
    Interactable currentInteractable;
    InteractableItems currentInteractableItem;
    void Update()
    {
        CheckInteraction();
        // F key only for interacting
        if (Input.GetKeyDown(KeyCode.F) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
        if (Input.GetKeyDown(KeyCode.F) && currentInteractableItem != null)
        {
            currentInteractableItem.InteractItem();
        }
    }
    void CheckInteraction()
    {
        RaycastHit hit;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out hit, playerReach))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                Interactable newInteractable = hit.collider.GetComponent<Interactable>();
                if (currentInteractable && newInteractable != currentInteractable)
                {
                    currentInteractable.DisableOutline();
                }
                if (newInteractable != null && newInteractable.enabled)
                {
                    SetNewCurrentInteractable(newInteractable);
                    return;
                }
            }
            if (hit.collider.CompareTag("Interactable Item"))
            {
                InteractableItems newInteractable = hit.collider.GetComponent<InteractableItems>();
                if (currentInteractable && newInteractable != currentInteractable)
                {
                    currentInteractable.DisableOutline();
                }
                if (newInteractable != null && newInteractable.enabled)
                {
                    SetNewCurrentInteractableItem(newInteractable);
                    return;
                }
            }
        }
        DisableCurrentInteractable();
    }
    void SetNewCurrentInteractable(Interactable newInteractable)
    {
        currentInteractable = newInteractable;
        currentInteractable.EnableOutline();
        HUDController.instance.EnableInteractionText(currentInteractable.message);
    }
    void SetNewCurrentInteractableItem(InteractableItems newInteractable)
    {
        currentInteractableItem = newInteractable;
        currentInteractableItem.EnableOutline();
        HUDController.instance.EnableInteractionText(currentInteractableItem.message);
    }
    void DisableCurrentInteractable()
    {
        HUDController.instance.DisableInteractionText();
        if (currentInteractable)
        {
            currentInteractable.DisableOutline();
            currentInteractable = null;
        }
    }
    void OnDrawGizmos()
    {
        if (playerCamera == null) return;
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, playerReach))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, hit.point);
            Gizmos.DrawSphere(hit.point, 0.05f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + direction * playerReach);
        }
    }
}