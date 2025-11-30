using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    public Transform holdParent;
    public float holdDistance = 1.5f;
    public float scrollSensitivity = 0.5f;
    public float minHoldDistance = 0.5f;
    public float maxHoldDistance = 3f;
    public float pickupRange = 3f;
    public Transform rayOrigin; // Typically the camera transform
    public float holdSmoothing = 15f;
    private Pickup heldObject;
    
    public Pickup HeldObject => heldObject;

    void Awake()
    {
        if (rayOrigin == null && Camera.main != null)
        {
            rayOrigin = Camera.main.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickupObject();
            else
                DropObject();
        }

        if (heldObject != null)
        {
            ScrollToAdjustHoldDistance();
            UpdateHeldObjectPosition();
        }
    }

    void TryPickupObject()
    {
        RaycastHit hit;
        Vector3 origin = rayOrigin != null ? rayOrigin.position : transform.position;
        Vector3 direction = rayOrigin != null ? rayOrigin.forward : transform.forward;
        if (Physics.Raycast(origin, direction, out hit, pickupRange))
        {
            Pickup pickable = hit.collider.GetComponent<Pickup>();
            if (pickable != null)
            {
                heldObject = pickable;
                heldObject.OnPickup(holdParent);
                PlayerCarrySystem.Instance.SetCarriedObject(hit.collider.name, hit.collider.gameObject);
            }
        }
    }

    void DropObject()
    {
        PlayerCarrySystem.Instance.ClearCarriedObject();
        if (heldObject != null)
        {
            heldObject.OnDrop();
            heldObject = null;
        }
    }

    void ScrollToAdjustHoldDistance()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            holdDistance += scrollInput * scrollSensitivity;
            holdDistance = Mathf.Clamp(holdDistance, minHoldDistance, maxHoldDistance);
        }
    }

    void UpdateHeldObjectPosition()
    {
        if (heldObject == null)
            return;

        // Since the object is parented to holdParent, smoothly move along local Z
        Vector3 targetLocal = new Vector3(0f, 0f, holdDistance);
        Transform t = heldObject.transform;
        t.localPosition = Vector3.Lerp(t.localPosition, targetLocal, Time.deltaTime * holdSmoothing);
        t.localRotation = Quaternion.Lerp(t.localRotation, Quaternion.identity, Time.deltaTime * holdSmoothing);
    }

    void OnDisable()
    {
        DropObject();
    }

    // Use this when an external system destroys the held object (e.g., placement)
    public void ForceClearHeldReference()
    {
        heldObject = null;
    }

}

