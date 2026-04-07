using UnityEngine;

public class WaterRiser : MonoBehaviour
{
    [Header("Flood Settings")]
    public float floodDuration = 60f;
    public float maxHeight = 10f;

    [Header("Second Rise (after stacking complete)")]
    public float secondMaxHeight = 16f;

    [Header("Notification Settings")]
    public float notificationInterval = 10f;
    public float warningHeightThreshold = 2f;

    [Header("Pause Settings")]
    public float pauseInterval = 30f;
    public float pauseMinDuration = 5f;
    public float pauseMaxDuration = 10f;

    public static event System.Action OnFloodComplete;

    private float _startY;
    private float _riseSpeed;
    private float _lastNotificationTime;
    private bool _hasStartedRising;
    private bool _hasCompleted;

    private bool _isPaused;
    private float _pauseTimer;
    private float _pauseDuration;
    private float _risingTimeAccumulator;

    // Second rise state
    private bool _stackingDone = false;
    private bool _secondRiseComplete = false;

    private void Start()
    {
        _startY = transform.position.y;
        _riseSpeed = (maxHeight - _startY) / floodDuration;

        StackProgressUI.OnStackingComplete += OnStackingComplete;
    }

    private void OnDestroy()
    {
        StackProgressUI.OnStackingComplete -= OnStackingComplete;
    }

    private void OnStackingComplete()
    {
        _stackingDone = true;
        Debug.Log("Stacking complete! Water will now rise to second max height.");
    }

    private void Update()
    {
        // --- First rise: up to maxHeight ---
        if (!_hasCompleted)
        {
            if (_isPaused)
            {
                _pauseTimer += Time.deltaTime;
                if (_pauseTimer >= _pauseDuration)
                {
                    _isPaused = false;
                    _pauseTimer = 0f;
                    Debug.Log("Flood resumes rising.");
                }
                return;
            }

            if (transform.position.y < maxHeight)
            {
                _risingTimeAccumulator += Time.deltaTime;
                if (_risingTimeAccumulator >= pauseInterval)
                {
                    _risingTimeAccumulator = 0f;
                    _isPaused = true;
                    _pauseDuration = Random.Range(pauseMinDuration, pauseMaxDuration);
                    Debug.Log($"Flood pausing for {_pauseDuration:F1} seconds.");
                    return;
                }

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
                // Reached first ceiling — wait here for stacking
                _hasCompleted = true;
                OnFloodComplete?.Invoke();
                Debug.Log("Water reached max height. Waiting for stacking...");
            }
        }

        // --- Second rise: only after stacking is done ---
        if (_hasCompleted && _stackingDone && !_secondRiseComplete)
        {
            if (transform.position.y < secondMaxHeight)
            {
                transform.position += Vector3.up * _riseSpeed * Time.deltaTime;
            }
            else
            {
                _secondRiseComplete = true;
                Debug.Log("Water reached second max height. Done.");
            }
        }
    }
}