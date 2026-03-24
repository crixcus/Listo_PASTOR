using UnityEngine;

public class DirtCleaner : MonoBehaviour
{
    public Camera cam;
    public float cleanSpeed = 1.5f;

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
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
                        }
                    }
                }
            }
        }
    }
}