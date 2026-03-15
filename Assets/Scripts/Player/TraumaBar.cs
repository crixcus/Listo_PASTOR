using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's trauma level (0–1) in Level 3.
/// Trauma rises passively over time and can be reduced by cleaning and restoring objects.
/// Drives TraumaEffects for visuals and fires notifications at warning thresholds.
///
/// Setup:
///   - Assign traumaBar (Slider), gameOver (GameOverPanel), and traumaEffects in Inspector.
///   - Call AddTrauma(amount) from other scripts to increase trauma.
///   - Call ReduceTrauma(amount) when player makes progress (cleaning, placing objects).
/// </summary>
public class TraumaBar : MonoBehaviour
{
    public static TraumaBar Instance { get; private set; }

    [Header("References")]
    public Slider traumaBar;
    public GameOverPanel gameOver;
    public TraumaEffects traumaEffects;

    [Header("Passive Trauma")]
    [Tooltip("How much trauma increases per second passively.")]
    public float passiveRiseRate = 0.005f;

    [Tooltip("Pause passive rise for this many seconds after the player makes progress.")]
    public float progressGracePeriod = 5f;

    [Header("Notification Thresholds")]
    public float warningThreshold1 = 0.3f;
    public float warningThreshold2 = 0.6f;
    public float warningThreshold3 = 0.8f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private float _trauma;
    private bool _gameOverTriggered;
    private bool _hasWarned30;
    private bool _hasWarned60;
    private bool _hasWarned80;
    private float _lastProgressTime;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        _trauma = 0f;
        traumaBar.value = 0f;
        traumaBar.maxValue = 1f;
        traumaEffects?.SetTrauma(0f);
    }

    private void Update()
    {
        if (_gameOverTriggered) return;

        // Passive trauma rise — paused briefly after player makes progress
        if (Time.time > _lastProgressTime + progressGracePeriod)
        {
            ModifyTrauma(passiveRiseRate * Time.deltaTime);
        }

        // Game over — only trigger once
        if (_trauma >= 1f)
        {
            _gameOverTriggered = true;
            gameOver?.TriggerGameOver();
        }
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Increases trauma by the given amount (0–1 scale).
    /// Call this from any script that should cause trauma (e.g. flood contact).
    /// </summary>
    public void AddTrauma(float amount)
    {
        ModifyTrauma(amount);
    }

    /// <summary>
    /// Reduces trauma by the given amount (0–1 scale).
    /// Call this when the player makes progress — cleaning, placing objects, etc.
    /// Also resets the passive rise grace period so trauma pauses briefly.
    /// </summary>
    public void ReduceTrauma(float amount)
    {
        _lastProgressTime = Time.time;
        ModifyTrauma(-amount);
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    /// <summary>
    /// Applies a delta to trauma, clamps it, updates the slider, drives effects,
    /// and checks warning thresholds.
    /// </summary>
    private void ModifyTrauma(float delta)
    {
        _trauma = Mathf.Clamp01(_trauma + delta);
        traumaBar.value = _trauma;
        traumaEffects?.SetTrauma(_trauma);
        CheckThresholds();
    }

    /// <summary>
    /// Fires one-time notifications at 30%, 60%, and 80% trauma thresholds.
    /// Uses correct percentage values in the message.
    /// </summary>
    private void CheckThresholds()
    {
        if (_trauma >= warningThreshold3 && !_hasWarned80)
        {
            _hasWarned80 = true;
            NotificationSystem.Instance?.ShowNotification("Your sanity is critical! Keep going!");
        }
        else if (_trauma >= warningThreshold2 && !_hasWarned60)
        {
            _hasWarned60 = true;
            NotificationSystem.Instance?.ShowNotification("Your sanity is deteriorating...");
        }
        else if (_trauma >= warningThreshold1 && !_hasWarned30)
        {
            _hasWarned30 = true;
            NotificationSystem.Instance?.ShowNotification("You're feeling uneasy. Keep cleaning.");
        }
    }
}