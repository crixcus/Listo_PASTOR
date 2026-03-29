using UnityEngine;

/// <summary>
/// Candle system with water height detection using a referenced sea object.
/// </summary>
public class Candle : MonoBehaviour
{
    [Header("State")]
    public bool startLit = false;

    [Header("Visuals")]
    public ParticleSystem flameParticle;
    public Light candleLight;

    [Header("Trauma Reduction")]
    public float traumaReductionRadius = 5f;
    public float traumaReductionRate = 0.008f;

    [Header("Blow Out")]
    public float blowOutTime = 60f;
    public float blowOutWarningTime = 10f;

    [Header("Light Flicker")]
    public float flickerAmount = 0.2f;
    public float flickerSpeed = 8f;

    [Header("Interaction")]
    public float noLighterNotifCooldown = 3f;

    [Header("Water Detection (Sea Reference)")]
    [Tooltip("Drag your sea / water object here.")]
    public Transform seaTransform;

    [Tooltip("Offset to adjust detection (use if water surface is slightly above/below pivot).")]
    public float waterOffset = 0f;

    // ------------------------------------------------------------------

    public bool IsLit { get; private set; }

    private float _baseIntensity;
    private float _litTimer;
    private bool _hasWarnedBlowOut;
    private Transform _player;
    private float _lastNoLighterNotifTime = -99f;

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
            Extinguish(true);
    }

    private void Update()
    {
        // --- WATER CHECK FIRST ---
        if (IsLit && seaTransform != null)
        {
            float waterHeight = seaTransform.position.y + waterOffset;

            if (transform.position.y <= waterHeight)
            {
                Extinguish(false);
                return;
            }
        }

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
            Extinguish(false);
            return;
        }

        // --- Light flicker ---
        if (candleLight != null)
        {
            float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            candleLight.intensity = _baseIntensity + (flicker * 2f - 1f) * flickerAmount;
        }

        // --- Trauma reduction ---
        if (_player != null)
        {
            float distance = Vector3.Distance(transform.position, _player.position);
            if (distance <= traumaReductionRadius)
                TraumaBar.Instance?.ReduceTrauma(traumaReductionRate * Time.deltaTime);
        }
    }

    // ------------------------------------------------------------------

    public void Light()
    {
        if (!Lighter.PlayerHasLighter)
        {
            if (Time.time - _lastNoLighterNotifTime >= noLighterNotifCooldown)
            {
                _lastNoLighterNotifTime = Time.time;
                NotificationSystem.Instance?.ShowNotification("You need a lighter to light this.");
            }
            return;
        }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, traumaReductionRadius);
        Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, traumaReductionRadius);
    }
}