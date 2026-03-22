using UnityEngine;

public class WaterRiser : MonoBehaviour
{
    [Header("Flood Settings")]
    public float floodDuration = 60f;  // seconds to reach max height
    public float maxHeight = 10f;

    [Header("Notification Settings")]
    public float notificationInterval = 10f;
    public float warningHeightThreshold = 2f;

    public static event System.Action OnFloodComplete;

    private float _startY;
    private float _riseSpeed;
    private float _lastNotificationTime;
    private bool _hasStartedRising;
    private bool _hasCompleted;

    private void Start()
    {
        _startY = transform.position.y;
        // Derive units-per-second from total distance and desired duration
        _riseSpeed = (maxHeight - _startY) / floodDuration;
    }

    private void Update()
    {
        if (_hasCompleted) return;

        if (transform.position.y < maxHeight)
        {
            transform.position += Vector3.up * _riseSpeed * Time.deltaTime;

            if (!_hasStartedRising)
            {
                _hasStartedRising = true;
                NotificationSystem.TriggerFloodRising();
            }

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
            _hasCompleted = true;
            OnFloodComplete?.Invoke();
        }
    }
}