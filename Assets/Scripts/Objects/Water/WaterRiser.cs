using UnityEngine;

public class WaterRiser : MonoBehaviour
{
    [Header("Flood Settings")]
    public float riseSpeed = 0.5f;   // How fast the water rises (units per second)
    public float maxHeight = 10f;    // Maximum height the water can reach
    
    [Header("Notification Settings")]
    public float notificationInterval = 10f; // How often to show flood warning (seconds)
    public float warningHeightThreshold = 2f; // Show warning when water reaches this height
    
    private float startY;
    private float lastNotificationTime = 0f;
    private bool hasStartedRising = false;

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
            
            // Trigger initial flood rising notification
            if (!hasStartedRising)
            {
                hasStartedRising = true;
                NotificationSystem.TriggerFloodRising();
            }
            
            // Show periodic warnings if water is above threshold
            if (transform.position.y >= startY + warningHeightThreshold)
            {
                if (Time.time - lastNotificationTime >= notificationInterval)
                {
                    NotificationSystem.TriggerFloodRising();
                    lastNotificationTime = Time.time;
                }
            }
        }
    }
}

