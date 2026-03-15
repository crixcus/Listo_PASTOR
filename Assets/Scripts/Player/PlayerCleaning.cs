using UnityEngine;
public class PlayerCleaning : MonoBehaviour
{
    public Camera playerCamera;
    public float cleanRange = 3f;
    [HideInInspector]
    public MopTool equippedMop;

    void Update()
    {
        if (equippedMop == null) return;

        if (Input.GetKey(KeyCode.E))
        {
            equippedMop.SetCleaning(true);
            TryClean();
        }

        // Stop animation when E is released
        if (Input.GetKeyUp(KeyCode.E))
        {
            equippedMop.SetCleaning(false);
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
                equippedMop.CleanTarget(cleanable);
            }
        }
    }

    public void EquipMop(MopTool mop)
    {
        equippedMop = mop;
    }
}