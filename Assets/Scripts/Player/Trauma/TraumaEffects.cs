using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Drives all trauma-based visual effects using URP Post Processing.
/// Attach to the same GameObject as your Global Volume, or any active GameObject.
///
/// Setup:
///   1. Create a Global Volume in your scene (GameObject > Volume > Global Volume)
///   2. Create a Volume Profile and add these overrides:
///      - Vignette
///      - Color Adjustments
///      - Lens Distortion
///   3. Assign the Volume to the 'volume' field in this script.
///   4. Assign the camera to 'playerCamera' for screen shake.
///   5. Assign TraumaPulseUI reference.
///
/// TraumaBar calls SetTrauma() every time trauma changes.
/// </summary>
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

        // Grab post-process overrides
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

        _initialized = true;
        ApplyEffects(0f);
    }

    private void Update()
    {
        if (!_initialized) return;

        // Screen shake — runs every frame based on current trauma
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
            // Smoothly return to origin when trauma is low
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

            // Boost intensity on the pulse
            float baseIntensity = Mathf.Lerp(vignetteMin, vignetteMax, _currentTrauma);
            _vignette.intensity.Override(baseIntensity + pulse * 0.1f * t);
        }
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Called by TraumaBar whenever the trauma value changes.
    /// Drives all visual effects proportional to the trauma level (0–1).
    /// </summary>
    public void SetTrauma(float trauma)
    {
        _currentTrauma = Mathf.Clamp01(trauma);
        ApplyEffects(_currentTrauma);
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    /// <summary>
    /// Applies all post-process overrides proportional to the trauma value.
    /// </summary>
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
    }
}