using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DirtAccumulate : MonoBehaviour
{
    public float accumulateSpeed = 1.5f;
    public float maxDirtStrength = 1f;
    public float cleanWithWaterSpeed = 2f;
    public float waterRange = 3f;
    private float current = 1f;

    private MopTool _mopTool;
    private RagTool _ragTool;
    private Renderer _renderer;

    public static bool IsMopMaxDirty { get; set; }
    public static bool IsRagMaxDirty { get; set; }

    // Keep this for any existing code that references IsMaxDirty
    public static bool IsMaxDirty => IsMopMaxDirty || IsRagMaxDirty;

    void Start()
    {
        IsMopMaxDirty = false;
        IsRagMaxDirty = false;
        _mopTool = GetComponentInParent<MopTool>();
        _ragTool = GetComponentInParent<RagTool>();
        _renderer = GetComponent<Renderer>();
        foreach (Material mat in _renderer.materials)
            if (mat.HasProperty("_DirtStrength"))
                mat.SetFloat("_DirtStrength", current);
    }

    void Update()
    {
        if (_renderer == null) return;
        if (_mopTool == null && _ragTool == null) return;

        current = GetDirtStrength();

        // Check if player is aiming at a water object
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out RaycastHit hit, waterRange))
        {
            if (hit.collider.tag.ToLower().Contains("water"))
            {
                ReduceDirt();
                if (_mopTool != null) IsMopMaxDirty = false;
                if (_ragTool != null) IsRagMaxDirty = false;
                return;
            }
        }

        // Mop dirt accumulation
        if (_mopTool != null && MaskPainter.IsPainting)
        {
            if (current < maxDirtStrength)
            {
                SetDirtStrength(current + accumulateSpeed * Time.deltaTime);
                IsMopMaxDirty = false;
            }
            else
            {
                IsMopMaxDirty = true;
            }
        }

        // Rag dirt accumulation
        if (_ragTool != null && DirtCleaner.IsCleaning)
        {
            if (current < maxDirtStrength)
            {
                SetDirtStrength(current + accumulateSpeed * Time.deltaTime);
                IsRagMaxDirty = false;
            }
            else
            {
                IsRagMaxDirty = true;
            }
        }
    }

    void ReduceDirt()
    {
         current = GetDirtStrength();
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