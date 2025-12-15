using UnityEngine;

public class PlayerCleaning : MonoBehaviour
{
    public Camera playerCamera;
    public float cleanRange = 3f;
    public MopTool equippedMop;

    void Update()
    {
        if (equippedMop == null)
        {
            Debug.Log("There is no mop");
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            TryClean();
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
