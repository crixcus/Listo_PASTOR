using UnityEngine;

public class SimpleStackingSystem : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform holdPoint;
    public float range = 3f;
    public LayerMask stackableLayer;

    private SimpleStackable carried;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (carried == null)
                TryPickup();
            else
                TryDropOrStack();
        }

        if (carried != null)
            MoveHeld();
    }

    void TryPickup()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, stackableLayer))
        {
            var s = hit.collider.GetComponentInParent<SimpleStackable>();
            if (s == null) return;

            carried = s;
            s.Pickup(holdPoint);
        }
    }

    void TryDropOrStack()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, stackableLayer))
        {
            var target = hit.collider.GetComponentInParent<SimpleStackable>();

            if (target != null && target != carried)
            {
                carried.StackOn(target);
                carried = null;
                return;
            }
        }

        carried.Drop();
        carried = null;
    }

    void MoveHeld()
    {
        carried.transform.position = Vector3.Lerp(
            carried.transform.position,
            holdPoint.position,
            Time.deltaTime * 15f
        );

        carried.transform.rotation = Quaternion.Lerp(
            carried.transform.rotation,
            holdPoint.rotation,
            Time.deltaTime * 15f
        );
    }
}