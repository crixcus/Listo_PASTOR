using UnityEngine;

public class TraumaPulseUI : MonoBehaviour
{
    [Header("Pulse Shape")]
    public LineRenderer line;
    public int resolution = 540;

    [Header("Wave Settings")]
    public float baseAmplitude = 8f;
    public float baseFrequency = 1.5f;
    public float scrollSpeed = 10f;

    [Range(0f, 1f)]
    public float trauma = 0f;

    float timeOffset;

    void Awake()
    {
        if (!line)
        {
            Debug.LogError("LineRenderer not assigned.");
            enabled = false;
            return;
        }

        line.useWorldSpace = false;
        line.positionCount = resolution;
    }

    void Update()
    {
        timeOffset += Time.deltaTime * scrollSpeed;
        DrawPulse();
    }

    void DrawPulse()
    {
        float amplitude = baseAmplitude * trauma;
        float frequency = baseFrequency;

        for (int i = 0; i < resolution; i++)
        {
            float x = i;
            float y = Mathf.Sin((i * frequency * 0.01f) + timeOffset) * amplitude;

            line.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}
