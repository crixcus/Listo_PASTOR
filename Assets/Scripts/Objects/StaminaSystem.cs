using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    public static StaminaSystem Instance { get; private set; }

    [Header("References")]
    public MopTool mopTool;
    public GameOverPanel gameOver;
    public BasicMovements playerMovement;

    [Header("Stamina Settings")]
    public float depletionRate = 0.08f;
    public float recoveryRate = 0.03f;
    public float exhaustionThreshold = 0.25f;

    [Range(0.1f, 1f)]
    public float exhaustedSpeedMultiplier = 0.4f;

    [Range(0.1f, 1f)]
    public float exhaustedCleanMultiplier = 0.3f;

    public float transitionSpeed = 3f;
    public float gameOverTimer = 30f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private float _stamina = 1f;
    private bool _isExhausted;
    private float _currentCleanMultiplier = 1f;
    private float _targetCleanMultiplier = 1f;
    private bool _exhaustionNotified = false;

    public float Stamina => _stamina;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        StaminaUI.Instance?.UpdateUI(_stamina);
    }

    private void Update()
    {
        bool isMopping = mopTool != null && mopTool.IsCleaning;

        if (isMopping)
            ModifyStamina(-depletionRate * Time.deltaTime);
        else
            ModifyStamina(recoveryRate * Time.deltaTime);

        // Determine exhaustion state change
        bool shouldBeExhausted = _stamina <= exhaustionThreshold;
        if (shouldBeExhausted != _isExhausted)
        {
            _isExhausted = shouldBeExhausted;
            OnExhaustionChanged(_isExhausted);
        }

        // ── TIMER — runs every frame in Update, not inside OnExhaustionChanged ──
        if (_isExhausted)
        {
            gameOverTimer -= Time.deltaTime;
            Debug.Log($"Exhaustion timer: {gameOverTimer:F1}");

            if (gameOverTimer <= 0f)
            {
                gameOverTimer = 0f;
                Debug.Log("Game Over — exhaustion timer ran out.");
                gameOver?.TriggerGameOver(); // or however you trigger game over
            }
        }
        else
        {
            // Reset timer every frame player is NOT exhausted
            gameOverTimer = 30f;
        }

        // Smoothly transition clean multiplier
        _currentCleanMultiplier = Mathf.Lerp(
            _currentCleanMultiplier,
            _targetCleanMultiplier,
            Time.deltaTime * transitionSpeed
        );
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    public void RestoreStamina(float amount)
    {
        ModifyStamina(amount);

        if (_stamina > exhaustionThreshold && _isExhausted)
        {
            _isExhausted = false;
            OnExhaustionChanged(false);
        }

        NotificationSystem.Instance?.ShowNotification("You feel more energized.");
    }

    public float GetCleanMultiplier() => _currentCleanMultiplier;

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    private void ModifyStamina(float delta)
    {
        _stamina = Mathf.Clamp01(_stamina + delta);
        StaminaUI.Instance?.UpdateUI(_stamina);
    }

    /// <summary>
    /// Called ONLY when exhaustion state changes (not every frame).
    /// Timer logic has been moved to Update() so it runs every frame.
    /// </summary>
    private void OnExhaustionChanged(bool exhausted)
    {
        // Movement speed
        if (playerMovement != null)
            playerMovement.SetStaminaMultiplier(exhausted ? exhaustedSpeedMultiplier : 1f);

        // Cleaning speed
        _targetCleanMultiplier = exhausted ? exhaustedCleanMultiplier : 1f;

        // Notification — only on the moment exhaustion begins
        if (exhausted)
        {
            if (!_exhaustionNotified)
            {
                _exhaustionNotified = true;
                NotificationSystem.Instance?.ShowNotification("You're exhausted. Take a break or eat something.");
            }
        }
        else
        {
            _exhaustionNotified = false;
        }
    }
}