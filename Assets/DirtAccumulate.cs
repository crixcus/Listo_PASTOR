using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DirtAccumulate : MonoBehaviour
{
    public float accumulateSpeed = 1.5f;
    public float maxDirtStrength = 1f;

    private MopTool _mopTool;
    private Renderer _renderer;

    void Start()
    {
        // MopTool is on the parent "mop ni dimagiba"
        _mopTool = GetComponentInParent<MopTool>();
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (_mopTool == null || _renderer == null) return;
        if (!MaskPainter.IsPainting) return;

        foreach (Material mat in _renderer.materials)
        {
            if (!mat.HasProperty("_DirtStrength")) continue;

            float current = mat.GetFloat("_DirtStrength");
            if (current < maxDirtStrength)
            {
                current += accumulateSpeed * Time.deltaTime;
                mat.SetFloat("_DirtStrength", Mathf.Min(current, maxDirtStrength));
            }
        }
    }
}