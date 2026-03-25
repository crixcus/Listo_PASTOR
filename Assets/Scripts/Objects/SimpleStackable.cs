using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleStackable : MonoBehaviour
{
    private Rigidbody rb;

    public float heightOffset = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Pickup(Transform holdPoint)
    {
        rb.isKinematic = true;
        transform.SetParent(holdPoint);
    }

    public void Drop()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
    }

    public void StackOn(SimpleStackable target)
    {
        transform.SetParent(null);

        Vector3 topPos = target.transform.position + Vector3.up * heightOffset;

        transform.position = topPos;
        transform.rotation = target.transform.rotation;

        rb.isKinematic = true;
    }
}