using UnityEngine;

/// <summary>
/// Manages the player's stamina in Level 3.
/// Stamina depletes while the player is actively mopping and recovers when idle.
/// At low stamina both movement speed and cleaning speed are reduced.
/// Food items restore large amounts of stamina instantly.
///
/// CONNECTIONS:
///   - MopTool.IsCleaning  → read every frame to know if player is mopping
///   - BasicMovements      → SetStaminaMultiplier() to slow movement at low stamina
///   - StaminaUI           → UpdateUI() called whenever stamina changes
///
/// SETUP:
///   1. Attach to PlayerHolder
///   2. Assign mopTool, playerMovement, and staminaUI in Inspector
/// </summary>
public class StaminaSystem : MonoBehaviour
{
    public static StaminaSystem Instance { get; private set; }

    [Header("References")]
    [Tooltip("The MopTool currently equipped. Assign at runtime or via Inspector.")]
    public MopTool mopTool;

    [Tooltip("BasicMovements on PlayerHolder for speed reduction.")]
    public BasicMovements playerMovement;

    [Header("Stamina Settings")]
    [Tooltip("How fast stamina depletes per second while mopping.")]
    public float depletionRate = 0.08f;

    [Tooltip("How fast stamina recovers per second when not mopping.")]
    public float recoveryRate = 0.03f;

    [Tooltip("Stamina threshold below which movement and cleaning slow down (0-1).")]
    public float exhaustionThreshold = 0.25f;

    [Tooltip("Movement speed multiplier when fully exhausted.")]
    [Range(0.1f, 1f)]
    public float exhaustedSpeedMultiplier = 0.4f;

    [Tooltip("Cleaning speed multiplier when fully exhausted.")]
    [Range(0.1f, 1f)]
    public float exhaustedCleanMultiplier = 0.3f;

    [Tooltip("How smoothly speed transitions when exhausted/recovered.")]
    public float transitionSpeed = 3f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private float _stamina = 1f;  // 0-1
    private bool _isExhausted;
    private float _currentCleanMultiplier = 1f;
    private float _targetCleanMultiplier = 1f;

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
        {
            // Deplete while mopping
            ModifyStamina(-depletionRate * Time.deltaTime);
        }
        else
        {
            // Recover when not mopping
            ModifyStamina(recoveryRate * Time.deltaTime);
        }

        // Determine exhaustion state
        bool shouldBeExhausted = _stamina <= exhaustionThreshold;
        if (shouldBeExhausted != _isExhausted)
        {
            _isExhausted = shouldBeExhausted;
            OnExhaustionChanged(_isExhausted);
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

    /// <summary>
    /// Restores stamina by the given amount. Called by FoodItem when eaten.
    /// Clamped to max 1.
    /// </summary>
    public void RestoreStamina(float amount)
    {
        ModifyStamina(amount);

        // If restored above exhaustion threshold, recover immediately
        if (_stamina > exhaustionThreshold && _isExhausted)
        {
            _isExhausted = false;
            OnExhaustionChanged(false);
        }

        NotificationSystem.Instance?.ShowNotification("You feel more energized.");
    }

    /// <summary>
    /// Returns the current cleaning speed multiplier.
    /// MopTool reads this to scale CleanTarget.
    /// </summary>
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
    /// Called when exhaustion state changes.
    /// Applies or removes speed and cleaning penalties.
    /// </summary>
    private void OnExhaustionChanged(bool exhausted)
    {
        // Movement speed
        if (playerMovement != null)
            playerMovement.SetStaminaMultiplier(exhausted ? exhaustedSpeedMultiplier : 1f);

        // Cleaning speed
        _targetCleanMultiplier = exhausted ? exhaustedCleanMultiplier : 1f;

        // Notify player
        if (exhausted)
            NotificationSystem.Instance?.ShowNotification("You're exhausted. Take a break or eat something.");
    }
}