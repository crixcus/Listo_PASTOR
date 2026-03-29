using UnityEngine;

public class DirtCleaner : MonoBehaviour
{
    public Camera cam;
    public float cleanSpeed = 1.5f;

    public static bool IsCleaning { get; private set; }

    void Update()
    {
        MopPickupAction mopAction = FindObjectOfType<MopPickupAction>(true);

        if (mopAction == null || !mopAction.heldMop.gameObject.activeSelf)
        {
            IsCleaning = false;
            return;
        }

        if (Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend != null)
                {
                    Material mat = rend.material;
                    if (mat.HasProperty("_DirtStrength"))
                    {
                        float current = mat.GetFloat("_DirtStrength");
                        if (current > 0f)
                        {
                            current -= cleanSpeed * Time.deltaTime;
                            mat.SetFloat("_DirtStrength", Mathf.Clamp01(current));
                            IsCleaning = true;
                            return;
                        }
                    }
                }
            }
        }

        IsCleaning = false;
    }
}