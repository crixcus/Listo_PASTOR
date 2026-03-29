using UnityEngine;

public class WaterRiser : MonoBehaviour
{
    [Header("Flood Settings")]
    public float floodDuration = 60f;
    public float maxHeight = 10f;

    [Header("Notification Settings")]
    public float notificationInterval = 10f;
    public float warningHeightThreshold = 2f;

    [Header("Pause Settings")]
    public float pauseInterval = 30f;        // how often a pause occurs (seconds of rising time)
    public float pauseMinDuration = 5f;      // minimum pause length
    public float pauseMaxDuration = 10f;     // maximum pause length

    public static event System.Action OnFloodComplete;

    private float _startY;
    private float _riseSpeed;
    private float _lastNotificationTime;
    private bool _hasStartedRising;
    private bool _hasCompleted;

    // Pause state
    private bool _isPaused;
    private float _pauseTimer;
    private float _pauseDuration;
    private float _risingTimeAccumulator;    // tracks elapsed rising time (excludes pauses)

    private void Start()
    {
        _startY = transform.position.y;
        _riseSpeed = (maxHeight - _startY) / floodDuration;
    }

    private void Update()
    {
        if (_hasCompleted) return;

        // --- Handle active pause ---
        if (_isPaused)
        {
            _pauseTimer += Time.deltaTime;
            if (_pauseTimer >= _pauseDuration)
            {
                _isPaused = false;
                _pauseTimer = 0f;
                Debug.Log("Flood resumes rising.");
            }
            return; // skip rising while paused
        }

        if (transform.position.y < maxHeight)
        {
            // Accumulate rising time and check if it's time to pause
            _risingTimeAccumulator += Time.deltaTime;
            if (_risingTimeAccumulator >= pauseInterval)
            {
                _risingTimeAccumulator = 0f;
                _isPaused = true;
                _pauseDuration = Random.Range(pauseMinDuration, pauseMaxDuration);
                Debug.Log($"Flood pausing for {_pauseDuration:F1} seconds.");
                return;
            }

            // Rise as normal
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