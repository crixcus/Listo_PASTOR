using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    [Header("Wave Settings")]
    public float amplitude = 1f;  // height of the wave
    public float length = 2f;     // wavelength
    public float speed = 1f;      // wave speed
    private float offset = 0f;     // moving wave offset

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Update()
    {
        offset += Time.deltaTime * speed;
    }

    /// <summary>
    /// Get wave height at a specific X position
    /// </summary>
    public float GetWaveHeight(float x)
    {
        return amplitude * Mathf.Sin(x / length + offset);
    }

    /// <summary>
    /// Optional: Get wave height at a full world position (for easier mesh updates)
    /// </summary>
    public float GetWaveHeight(Vector3 worldPos)
    {
        return GetWaveHeight(worldPos.x);
    }
}
