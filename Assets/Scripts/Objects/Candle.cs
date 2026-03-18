using UnityEngine;

/// <summary>
/// Attach to each candle 3D model in Level 2.
/// Handles lit/unlit state, flame visuals, point light,
/// trauma reduction radius, and blowing out after a duration.
///
/// Setup:
///   1. Attach to the candle GameObject
///   2. Keep existing Interactable component — wire OnInteraction() to Candle.Light()
///   3. Assign flameParticle, candleLight in Inspector
///   4. Set traumaReductionRadius and blowOutTime in Inspector
/// </summary>
public class Candle : MonoBehaviour
{
    [Header("State")]
    [Tooltip("Start the candle already lit (e.g. for tutorial).")]
    public bool startLit = false;

    [Header("Visuals")]
    [Tooltip("Particle system for the candle flame.")]
    public ParticleSystem flameParticle;

    [Tooltip("Point light on the candle.")]
    public Light candleLight;

    [Header("Trauma Reduction")]
    [Tooltip("Radius within which the player receives trauma reduction.")]
    public float traumaReductionRadius = 5f;

    [Tooltip("How much trauma reduces per second while player is in range.")]
    public float traumaReductionRate = 0.008f;

    [Header("Blow Out")]
    [Tooltip("How long the candle stays lit before blowing out (seconds).")]
    public float blowOutTime = 60f;

    [Tooltip("Seconds before blowing out to warn the player.")]
    public float blowOutWarningTime = 10f;

    [Header("Light Flicker")]
    [Tooltip("How much the light intensity fluctuates.")]
    public float flickerAmount = 0.2f;

    [Tooltip("Speed of the light flicker.")]
    public float flickerSpeed = 8f;

    [Header("Interaction")]
    [Tooltip("Seconds before the 'need a lighter' notification can show again.")]
    public float noLighterNotifCooldown = 3f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    public bool IsLit { get; private set; }

    private float _baseIntensity;
    private float _litTimer;
    private bool _hasWarnedBlowOut;
    private Transform _player;
    private float _lastNoLighterNotifTime = -99f;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Start()
    {
        if (candleLight != null)
            _baseIntensity = candleLight.intensity;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        if (startLit)
            Light();
        else
            Extinguish(silent: true);
    }

    private void Update()
    {
        if (!IsLit) return;

        // --- Blow out timer ---
        _litTimer += Time.deltaTime;

        if (!_hasWarnedBlowOut && _litTimer >= blowOutTime - blowOutWarningTime)
        {
            _hasWarnedBlowOut = true;
            NotificationSystem.Instance?.ShowNotification("A candle is about to blow out...");
        }

        if (_litTimer >= blowOutTime)
        {
            Extinguish(silent: false);
            return;
        }

        // --- Light flicker ---
        if (candleLight != null)
        {
            float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            candleLight.intensity = _baseIntensity + (flicker * 2f - 1f) * flickerAmount;
        }

        // --- Trauma reduction if player is in range ---
        if (_player != null)
        {
            float distance = Vector3.Distance(transform.position, _player.position);
            if (distance <= traumaReductionRadius)
                TraumaBar.Instance?.ReduceTrauma(traumaReductionRate * Time.deltaTime);
        }
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Called via Interactable.OnInteraction() UnityEvent when player presses F.
    /// Checks if player has the lighter before lighting.
    /// Shows "need a lighter" notification with a cooldown to prevent spam.
    /// </summary>
    public void Light()
    {
        // Can't light without a lighter
        if (!Lighter.PlayerHasLighter)
        {
            // Only show the notification if cooldown has passed
            if (Time.time - _lastNoLighterNotifTime >= noLighterNotifCooldown)
            {
                _lastNoLighterNotifTime = Time.time;
                NotificationSystem.Instance?.ShowNotification("You need a lighter to light this.");
            }
            return;
        }

        // Already lit — do nothing
        if (IsLit) return;

        IsLit = true;
        _litTimer = 0f;
        _hasWarnedBlowOut = false;

        if (flameParticle != null)
            flameParticle.Play();

        if (candleLight != null)
            candleLight.enabled = true;

        NotificationSystem.Instance?.ShowNotification("Candle lit. You feel calmer.");
    }

    /// <summary>
    /// Extinguishes the candle. Called internally when blow out timer expires.
    /// </summary>
    public void Extinguish(bool silent = false)
    {
        IsLit = false;
        _litTimer = 0f;
        _hasWarnedBlowOut = false;

        if (flameParticle != null)
            flameParticle.Stop();

        if (candleLight != null)
            candleLight.enabled = false;

        if (!silent)
            NotificationSystem.Instance?.ShowNotification("A candle blew out.");
    }

    // ------------------------------------------------------------------
    // Gizmo
    // ------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, traumaReductionRadius);
        Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, traumaReductionRadius);
    }
}