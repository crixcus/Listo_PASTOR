using UnityEngine;

public class DirtCleaner : MonoBehaviour
{
    public Camera cam;
    public float cleanSpeed = 1.5f;
    public MopPickupAction mopAction;
    public MaskPainter maskPainter;
    public float dirtToPixelRatio = 50f;
    public bool ragActive = false;

    [Header("Cleanable Dirt Objects")]
    public Renderer[] dirtObjects;

    public static bool IsCleaning { get; private set; }

    private float accumulatedDirt = 0f;

    void Update()
    {
         ragActive = mopAction != null &&
                         mopAction.HeldRag != null &&
                         mopAction.HeldRag.gameObject.activeSelf;

        if (ragActive && Input.GetMouseButton(0) && !DirtAccumulate.IsRagMaxDirty)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Renderer hitRend = hit.collider.GetComponent<Renderer>();
                if (hitRend != null && IsInDirtList(hitRend))
                {
                    foreach (Material mat in hitRend.materials)
                    {
                        if (mat.HasProperty("_DirtStrength"))
                        {
                            float current = mat.GetFloat("_DirtStrength");
                            if (current > 0f)
                            {
                                float reduction = cleanSpeed * Time.deltaTime;
                                current -= reduction;
                                mat.SetFloat("_DirtStrength", Mathf.Clamp01(current));

                                accumulatedDirt += reduction;
                                if (maskPainter != null)
                                {
                                    maskPainter.AddExternalProgress(accumulatedDirt * dirtToPixelRatio);
                                    accumulatedDirt = 0f;
                                }

                                IsCleaning = true;
                                return;
                            }
                        }
                    }
                }
            }
        }

        IsCleaning = false;
    }

    bool IsInDirtList(Renderer rend)
    {
        foreach (Renderer r in dirtObjects)
            if (r == rend) return true;
        return false;
    }
}