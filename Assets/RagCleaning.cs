using UnityEngine;

public class RagCleaning : MonoBehaviour
{
    public Camera playerCamera;
    public float cleanRange = 3f;
    public MopPickupAction mopAction;

    void Update()
    {
        if (mopAction == null) return;

        bool ragActive = mopAction.HeldRag != null && mopAction.HeldRag.gameObject.activeSelf;

        if (!ragActive) return;

        if (Input.GetMouseButton(0))
        {
            mopAction.HeldRag.SetCleaning(true);
            TryClean();
        }

        if (Input.GetMouseButtonUp(0))
        {
            mopAction.HeldRag.SetCleaning(false);
        }
    }

    void TryClean()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, cleanRange))
        {
            CleanableObject cleanable = hit.collider.GetComponent<CleanableObject>();
            if (cleanable != null && !cleanable.IsClean)
            {
                mopAction.HeldRag.CleanTarget(cleanable);
            }
        }
    }
}