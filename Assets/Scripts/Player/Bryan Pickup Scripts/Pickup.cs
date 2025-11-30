using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickup : MonoBehaviour
{
    private Rigidbody rb;
    private Transform originalParent;
    private Vector3 originalLocalScale;

    [HideInInspector]
    public GameObject OriginalPrefab; // link back to the prefab type

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // If spawned directly in scene, OriginalPrefab can remain null.
        // You can assign it manually from a spawner if needed.
    }

    public void OnPickup(Transform holdParent)
    {
        rb.useGravity = false;
        rb.isKinematic = true;

        // Save original hierarchy and scale
        originalParent = transform.parent;
        originalLocalScale = transform.localScale;

        // Maintain scale when reparented
        Vector3 worldScaleBefore = transform.lossyScale;
        transform.SetParent(holdParent, false);
        Vector3 parentWorldScale = holdParent != null ? holdParent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            parentWorldScale.x != 0f ? worldScaleBefore.x / parentWorldScale.x : originalLocalScale.x,
            parentWorldScale.y != 0f ? worldScaleBefore.y / parentWorldScale.y : originalLocalScale.y,
            parentWorldScale.z != 0f ? worldScaleBefore.z / parentWorldScale.z : originalLocalScale.z
        );

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDrop()
    {
        rb.useGravity = true;
        rb.isKinematic = false;

        transform.SetParent(originalParent, true);
        transform.localScale = originalLocalScale;
    }
}
