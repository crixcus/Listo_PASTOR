using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's trauma level (0–1) in Level 3.
///
/// Trauma behaviour:
///   - Rises passively over time (constant pressure)
///   - Reduces slowly when the player stands still (calm = recovery)
///   - Reduces when the player cleans or restores objects (progress = relief)
///   - Jumpscare triggers add a large instant spike
///   - At 100% trauma, game over triggers
///
/// Setup:
///   - Assign traumaBar (Slider), gameOver (GameOverPanel), traumaEffects, and playerBody.
///   - playerBody: the CharacterController or Rigidbody Transform to track movement.
/// </summary>
public class TraumaBar : MonoBehaviour
{
    public static TraumaBar Instance { get; private set; }

    [Header("References")]
    public Slider traumaBar;
    public GameOverPanel gameOver;
    public TraumaEffects traumaEffects;

    [Tooltip("The player's Transform used to detect movement. Assign PlayerHolder.")]
    public Transform playerBody;

    [Header("Passive Trauma Rise")]
    [Tooltip("How much trauma increases per second passively.")]
    public float passiveRiseRate = 0.005f;

    [Tooltip("Pause passive rise for this many seconds after player makes progress.")]
    public float progressGracePeriod = 5f;

    [Header("Stillness Recovery")]
    [Tooltip("How many seconds the player must be still before trauma starts reducing.")]
    public float stillnessDelay = 3f;

    [Tooltip("How much trauma reduces per second while the player is still.")]
    public float stillnessRecoveryRate = 0.008f;

    [Tooltip("Minimum movement distance per frame to be considered moving.")]
    public float movementThreshold = 0.01f;

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

    // Stillness tracking
    private Vector3 _lastPosition;
    private float _stillTimer;
    private bool _isStill;

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

        if (playerBody != null)
            _lastPosition = playerBody.position;
    }

    private void Update()
    {
        if (_gameOverTriggered) return;

        TrackStillness();

        if (_isStill)
        {
            // Player is still — recover trauma slowly
            ModifyTrauma(-stillnessRecoveryRate * Time.deltaTime);
        }
        else if (Time.time > _lastProgressTime + progressGracePeriod)
        {
            // Player is moving and no recent progress — trauma rises passively
            ModifyTrauma(passiveRiseRate * Time.deltaTime);
        }

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
    /// Increases trauma instantly by the given amount (0–1).
    /// Use for jumpscares, flood contact, or other traumatic events.
    /// </summary>
    public void AddTrauma(float amount)
    {
        ModifyTrauma(amount);
    }

    /// <summary>
    /// Reduces trauma by the given amount (0–1).
    /// Also resets the passive rise grace period.
    /// Use for cleaning progress, object restoration, etc.
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
    /// Tracks whether the player has been still long enough to trigger recovery.
    /// Uses position delta each frame against a small movement threshold.
    /// </summary>
    private void TrackStillness()
    {
        if (playerBody == null) return;

        float moved = Vector3.Distance(playerBody.position, _lastPosition);
        _lastPosition = playerBody.position;

        if (moved > movementThreshold)
        {
            // Player moved — reset still timer
            _stillTimer = 0f;
            _isStill = false;
        }
        else
        {
            // Player is standing still — count up
            _stillTimer += Time.deltaTime;
            _isStill = _stillTimer >= stillnessDelay;
        }
    }

    /// <summary>
    /// Applies a delta to trauma, clamps, updates UI and effects,
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
    /// Fires one-time notifications at 30%, 60%, and 80% trauma.
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