using UnityEngine;

public class TraumaECG : MonoBehaviour
{
    public LineRenderer lineRenderer;

    [Header("ECG Settings")]
    public int points = 200; // resolution of the line
    public float width = 2f;
    public float baseSpeed = 2f;
    public float baseAmplitude = 1f;

    [Header("Trauma Control (0 = calm, 1 = dying)")]
    [Range(0f, 1f)]
    public float trauma = 0f;

    private float timeOffset;

    void Start()
    {
        lineRenderer.positionCount = points;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    void Update()
    {
        DrawECG();
    }

    void DrawECG()
    {
        timeOffset += Time.deltaTime * Mathf.Lerp(baseSpeed, baseSpeed * 4f, trauma);

        for (int i = 0; i < points; i++)
        {
            float x = i * 0.02f;
            float t = timeOffset + x;

            float amplitude = Mathf.Lerp(baseAmplitude, baseAmplitude * 3f, trauma);

            // core ECG shape
            float y = ECGWave(t) * amplitude;

            // add chaos when trauma is high
            if (trauma > 0.7f)
            {
                float chaos = (trauma - 0.7f) * 8f;
                y += Mathf.PerlinNoise(t * 20f, 0f) * chaos;
            }

            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }

        // color shifting
        Color calm = new Color(0.5f, 1f, 1f); // cyan
        Color danger = new Color(1f, 0.2f, 0.2f); // red
        lineRenderer.material.color = Color.Lerp(calm, danger, trauma);
    }

    float ECGWave(float t)
    {
        // ECG = mix of sin + sharp spike function
        float wave = Mathf.Sin(t * 6f) * 0.2f;

        // sharp heartbeat spike
        wave += Mathf.Max(0, Mathf.Exp(-Mathf.Pow((t % 1f) * 12f - 5.5f, 2f)) * 2.5f);

        return wave;
    }
}
