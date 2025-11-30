using UnityEngine;

public class WaterRiser : MonoBehaviour
{
    [Header("Flood Settings")]
    public float riseSpeed = 0.5f;   // How fast the water rises (units per second)
    public float maxHeight = 10f;    // Maximum height the water can reach
    private float startY;

    private void Start()
    {
        startY = transform.position.y;
    }

    private void Update()
    {
        // Raise water until it reaches max height
        if (transform.position.y < maxHeight)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        }
    }
}

