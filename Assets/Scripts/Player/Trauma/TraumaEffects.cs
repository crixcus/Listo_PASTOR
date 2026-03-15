using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TraumaEffects : MonoBehaviour
{
    public static TraumaEffects Instance { get; private set; }

    [Header("References")]
    [Tooltip("The Global Volume containing post-process overrides.")]
    public Volume volume;

    [Tooltip("The player camera transform, used for screen shake.")]
    public Transform playerCamera;

    [Header("Vignette")]
    [Tooltip("Vignette intensity at 0% trauma.")]
    public float vignetteMin = 0.15f;

    [Tooltip("Vignette intensity at 100% trauma.")]
    public float vignetteMax = 0.85f;

    [Tooltip("Vignette color at low trauma (normal dark edges).")]
    public Color vignetteLowColor = Color.black;

    [Tooltip("Vignette color at high trauma (red pulse).")]
    public Color vignetteHighColor = new Color(0.8f, 0f, 0f, 1f);

    [Header("Color Grading")]
    [Tooltip("Saturation at 0% trauma (full color).")]
    public float saturationNormal = 0f;

    [Tooltip("Saturation at 100% trauma (fully desaturated).")]
    public float saturationMin = -80f;

    [Tooltip("How dark the screen gets at max trauma (-1 to 0).")]
    public float maxDarken = -0.4f;

    [Header("Lens Distortion")]
    [Tooltip("Max lens distortion intensity at high trauma.")]
    public float maxDistortion = -0.25f;

    [Header("Screen Shake")]
    [Tooltip("Max shake magnitude at 100% trauma.")]
    public float maxShakeMagnitude = 0.04f;

    [Tooltip("How fast the shake oscillates.")]
    public float shakeSpeed = 12f;

    [Header("Red Flash (Critical)")]
    [Tooltip("Trauma level at which the red flash begins pulsing.")]
    public float redFlashThreshold = 0.8f;

    [Tooltip("How fast the red vignette pulses at critical trauma.")]
    public float redPulseSpeed = 2f;

    [Header("Red Overlay")]
    [Tooltip("Full screen red UI Image, set alpha to 0 in Inspector.")]
    public Image redOverlay;

    [Tooltip("Max alpha of the red overlay at 100% trauma.")]
    public float maxOverlayAlpha = 0.4f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private Vignette _vignette;
    private ColorAdjustments _colorAdjustments;
    private LensDistortion _lensDistortion;

    private float _currentTrauma;
    private Vector3 _cameraOriginLocal;
    private bool _initialized;

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
        if (volume == null)
        {
            Debug.LogError("[TraumaEffects] No Volume assigned. Assign a Global Volume in the Inspector.");
            enabled = false;
            return;
        }

        volume.profile.TryGet(out _vignette);
        volume.profile.TryGet(out _colorAdjustments);
        volume.profile.TryGet(out _lensDistortion);

        if (_vignette == null)
            Debug.LogWarning("[TraumaEffects] No Vignette override found in Volume Profile. Add one.");
        if (_colorAdjustments == null)
            Debug.LogWarning("[TraumaEffects] No Color Adjustments override found in Volume Profile. Add one.");
        if (_lensDistortion == null)
            Debug.LogWarning("[TraumaEffects] No Lens Distortion override found in Volume Profile. Add one.");

        if (playerCamera != null)
            _cameraOriginLocal = playerCamera.localPosition;

        // Make sure overlay starts invisible
        if (redOverlay != null)
        {
            Color c = redOverlay.color;
            c.a = 0f;
            redOverlay.color = c;
        }

        _initialized = true;
        ApplyEffects(0f);
    }

    private void Update()
    {
        if (!_initialized) return;

        // Screen shake
        if (playerCamera != null && _currentTrauma > 0.1f)
        {
            float shakePower = Mathf.InverseLerp(0.1f, 1f, _currentTrauma) * maxShakeMagnitude;
            playerCamera.localPosition = _cameraOriginLocal + new Vector3(
                Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) * 2f - 1f,
                Mathf.PerlinNoise(0f, Time.time * shakeSpeed) * 2f - 1f,
                0f
            ) * shakePower;
        }
        else if (playerCamera != null)
        {
            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition,
                _cameraOriginLocal,
                Time.deltaTime * 8f
            );
        }

        // Red pulse at critical trauma
        if (_currentTrauma >= redFlashThreshold && _vignette != null)
        {
            float pulse = (Mathf.Sin(Time.time * redPulseSpeed * Mathf.PI) + 1f) * 0.5f;
            float t = Mathf.InverseLerp(redFlashThreshold, 1f, _currentTrauma);

            _vignette.color.Override(Color.Lerp(vignetteHighColor, Color.red, pulse * t));

            float baseIntensity = Mathf.Lerp(vignetteMin, vignetteMax, _currentTrauma);
            _vignette.intensity.Override(baseIntensity + pulse * 0.1f * t);
        }
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    public void SetTrauma(float trauma)
    {
        _currentTrauma = Mathf.Clamp01(trauma);
        ApplyEffects(_currentTrauma);
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    private void ApplyEffects(float t)
    {
        // --- Vignette ---
        if (_vignette != null)
        {
            _vignette.intensity.Override(Mathf.Lerp(vignetteMin, vignetteMax, t));
            _vignette.color.Override(Color.Lerp(vignetteLowColor, vignetteHighColor, t));
        }

        // --- Color Adjustments ---
        if (_colorAdjustments != null)
        {
            _colorAdjustments.saturation.Override(Mathf.Lerp(saturationNormal, saturationMin, t));
            _colorAdjustments.postExposure.Override(Mathf.Lerp(0f, maxDarken, t));
        }

        // --- Lens Distortion ---
        if (_lensDistortion != null)
            _lensDistortion.intensity.Override(Mathf.Lerp(0f, maxDistortion, t));

        // --- Red Overlay ---
        if (redOverlay != null)
        {
            Color c = redOverlay.color;
            c.a = Mathf.Lerp(0f, maxOverlayAlpha, t);
            redOverlay.color = c;
        }
    }
}