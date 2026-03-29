using UnityEngine;

public class DirtAccumulate : MonoBehaviour
{
    public float accumulateSpeed = 1.5f;
    public float maxDirtStrength = 1f;
    public float cleanWithWaterSpeed = 2f;
    public float waterRange = 3f;

    private MopTool _mopTool;
    private Renderer _renderer;

    public static bool IsMaxDirty { get; set; }

    void Start()
    {
        _mopTool = GetComponentInParent<MopTool>();
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (_mopTool == null || _renderer == null) return;

        float current = GetDirtStrength();

        // Check if player is aiming at a water object
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out RaycastHit hit, waterRange))
        {
            if (hit.collider.tag.ToLower().Contains("water"))
            {
                ReduceDirt();
                IsMaxDirty = false;
                return;
            }
        }

        // Accumulate only when painting MaskPainter
        if (!MaskPainter.IsPainting && !DirtCleaner.IsCleaning) return;

        if (current < maxDirtStrength)
        {
            SetDirtStrength(current + accumulateSpeed * Time.deltaTime);
            IsMaxDirty = false;
        }
        else
        {
            IsMaxDirty = true;
        }
    }

    void ReduceDirt()
    {
        float current = GetDirtStrength();
        SetDirtStrength(current - cleanWithWaterSpeed * Time.deltaTime);
    }

    float GetDirtStrength()
    {
        foreach (Material mat in _renderer.materials)
            if (mat.HasProperty("_DirtStrength"))
                return mat.GetFloat("_DirtStrength");
        return 0f;
    }

    void SetDirtStrength(float value)
    {
        value = Mathf.Clamp(value, 0f, maxDirtStrength);
        foreach (Material mat in _renderer.materials)
            if (mat.HasProperty("_DirtStrength"))
                mat.SetFloat("_DirtStrength", value);
    }
}