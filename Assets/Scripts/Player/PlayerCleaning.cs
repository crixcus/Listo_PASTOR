using UnityEngine;

public class PlayerCleaning : MonoBehaviour
{
    public Camera playerCamera;
    public float cleanRange = 3f;

    [HideInInspector] public MopTool equippedMop;
    [HideInInspector] public RagTool equippedRag;

    void Update()
    {
        bool mopActive = equippedMop != null && equippedMop.gameObject.activeSelf;
        bool ragActive = equippedRag != null && equippedRag.gameObject.activeSelf;

        if (!mopActive && !ragActive) return;

        if (Input.GetMouseButton(0))
        {
            if (mopActive) equippedMop.SetCleaning(true);
            if (ragActive) equippedRag.SetCleaning(true);
            if (ragActive) TryClean();
        }

        if (Input.GetMouseButtonUp(0))
        {
            equippedMop?.SetCleaning(false);
            equippedRag?.SetCleaning(false);
        }
    }

    void TryClean()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, cleanRange))
        {
            CleanableObject cleanable = hit.collider.GetComponent<CleanableObject>();
            if (cleanable != null && !cleanable.IsClean)
            {
                equippedRag.CleanTarget(cleanable);
            }
        }
    }

    public void EquipMop(MopTool mop)
    {
        equippedMop = mop;
    }

    public void EquipRag(RagTool rag)
    {
        equippedRag = rag;
    }
}