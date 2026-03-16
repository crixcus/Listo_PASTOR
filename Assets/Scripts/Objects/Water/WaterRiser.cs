using UnityEngine;

/// <summary>
/// Raises the water level over time.
/// Fires OnFloodComplete once the water reaches max height —
/// JumpScare tripwires listen for this to activate themselves.
/// </summary>
public class WaterRiser : MonoBehaviour
{
    [Header("Flood Settings")]
    public float riseSpeed = 0.5f;
    public float maxHeight = 10f;

    [Header("Notification Settings")]
    public float notificationInterval = 10f;
    public float warningHeightThreshold = 2f;

    /// <summary>
    /// Fired once when the water finishes rising and reaches max height.
    /// JumpScare tripwires subscribe to this to activate.
    /// </summary>
    public static event System.Action OnFloodComplete;

    private float _startY;
    private float _lastNotificationTime;
    private bool _hasStartedRising;
    private bool _hasCompleted;

    private void Start()
    {
        _startY = transform.position.y;
    }

    private void Update()
    {
        if (_hasCompleted) return;

        if (transform.position.y < maxHeight)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // First frame of rising — notify player
            if (!_hasStartedRising)
            {
                _hasStartedRising = true;
                NotificationSystem.TriggerFloodRising();
            }

            // Periodic warnings while rising
            if (transform.position.y >= _startY + warningHeightThreshold)
            {
                if (Time.time - _lastNotificationTime >= notificationInterval)
                {
                    NotificationSystem.TriggerFloodRising();
                    _lastNotificationTime = Time.time;
                }
            }
        }
        else
        {
            // Water has reached max height — activate tripwires
            _hasCompleted = true;
            OnFloodComplete?.Invoke();
        }
    }
}