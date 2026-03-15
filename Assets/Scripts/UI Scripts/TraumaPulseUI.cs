using UnityEngine;

/// <summary>
/// Renders a heartbeat-style sine wave via LineRenderer.
/// Amplitude and frequency intensify as trauma rises.
/// The trauma value is set externally by TraumaEffects.SetTrauma().
///
/// Setup:
///   - Assign a LineRenderer component to 'line'.
///   - Place this on a Screen Space canvas UI element.
/// </summary>
public class TraumaPulseUI : MonoBehaviour
{
    [Header("Pulse Shape")]
    public LineRenderer line;
    public int resolution = 540;

    [Header("Wave Settings")]
    [Tooltip("Base amplitude when trauma is 0 (flat line).")]
    public float baseAmplitude = 2f;

    [Tooltip("Max amplitude at 100% trauma.")]
    public float maxAmplitude = 30f;

    [Tooltip("Base frequency at 0% trauma.")]
    public float baseFrequency = 1f;

    [Tooltip("Max frequency at 100% trauma (faster heartbeat).")]
    public float maxFrequency = 4f;

    [Tooltip("Base scroll speed at 0% trauma.")]
    public float baseScrollSpeed = 5f;

    [Tooltip("Max scroll speed at 100% trauma.")]
    public float maxScrollSpeed = 25f;

    /// <summary>
    /// Current trauma level (0–1). Set by TraumaEffects every frame.
    /// </summary>
    [Range(0f, 1f)]
    public float trauma = 0f;

    private float _timeOffset;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        if (!line)
        {
            Debug.LogError("[TraumaPulseUI] LineRenderer not assigned.");
            enabled = false;
            return;
        }

        line.useWorldSpace = false;
        line.positionCount = resolution;
    }

    private void Update()
    {
        float scrollSpeed = Mathf.Lerp(baseScrollSpeed, maxScrollSpeed, trauma);
        _timeOffset += Time.deltaTime * scrollSpeed;
        DrawPulse();
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    /// <summary>
    /// Redraws the pulse line each frame with amplitude and frequency
    /// scaled by the current trauma value.
    /// </summary>
    private void DrawPulse()
    {
        float amplitude = Mathf.Lerp(baseAmplitude, maxAmplitude, trauma);
        float frequency = Mathf.Lerp(baseFrequency, maxFrequency, trauma);

        for (int i = 0; i < resolution; i++)
        {
            float x = i;
            float y = Mathf.Sin((i * frequency * 0.01f) + _timeOffset) * amplitude;

            // Add a spike effect at high trauma — sharp heartbeat peak
            if (trauma > 0.6f)
            {
                float spike = Mathf.Exp(-Mathf.Pow((i % 120 - 60) * 0.05f, 2f))
                              * amplitude * (trauma - 0.6f) * 3f;
                y += spike;
            }

            line.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}