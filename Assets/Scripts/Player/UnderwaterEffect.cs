using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Manages all post-processing visual effects when the player's head is submerged
/// in flood water. Creates a murky, disorienting underwater feel using URP Volume
/// overrides and a full-screen color overlay.
///
/// EFFECTS APPLIED WHEN SUBMERGED:
///   - Depth Of Field (Gaussian blur) — limits visibility, simulates murky water
///   - Lens Distortion — warps the screen edges for a disorienting underwater feel
///   - Color Adjustments — desaturates and darkens the scene
///   - Murk Overlay — a full-screen UI image tinted brown to match flood water color
///
/// All effects smoothly transition in when submerging and out when surfacing.
///
/// HOW IT CONNECTS:
///   WaterTraumaTrigger detects when the player's head goes below water
///   and calls UnderwaterEffects.Instance.SetSubmerged(true/false)
///
/// </summary>
public class UnderwaterEffects : MonoBehaviour
{
    /// <summary>
    /// Global singleton. Accessed by WaterTraumaTrigger to trigger effects.
    /// Automatically set on Awake and cleared on Destroy.
    /// </summary>
    public static UnderwaterEffects Instance { get; private set; }

    // ------------------------------------------------------------------
    // Inspector Fields
    // ------------------------------------------------------------------

    [Header("References")]

    /// <summary>
    /// The URP Global Volume containing post-process overrides.
    /// Drag the Global Volume GameObject here.
    /// </summary>
    public Volume volume;

    /// <summary>
    /// Full-screen UI Image used as a color overlay for the murky water tint.
    /// Set its default color to dark brown (R:60 G:40 B:10) with Alpha 0.
    /// Must be on a Canvas set to Screen Space — Overlay.
    /// Also make sure Raycast Target is unchecked on the Image component.
    /// </summary>
    public Image murkOverlay;

    [Header("Transition")]

    /// <summary>
    /// How fast effects fade in when submerging and out when surfacing.
    /// Lower values = slower, more gradual transition.
    /// Recommended range: 0.5 (slow/cinematic) to 3.0 (fast/snappy)
    /// </summary>
    public float transitionSpeed = 1.5f;

    [Header("Depth Of Field / Blur")]

    /// <summary>
    /// Distance (in units) at which blur starts when submerged.
    /// 0 = blur begins immediately in front of the camera.
    /// </summary>
    public float underwaterBlurStart = 0f;

    /// <summary>
    /// Distance (in units) at which blur reaches maximum when submerged.
    /// Lower values = shorter visibility range underwater.
    /// Recommended: 2-5 units for murky flood water.
    /// </summary>
    public float underwaterBlurEnd = 3f;

    /// <summary>
    /// Maximum blur radius when fully submerged.
    /// Higher values = heavier, more blurred vision.
    /// Recommended: 10-20 for strong murky effect.
    /// </summary>
    public float maxBlurIntensity = 15f;

    [Header("Lens Distortion")]

    /// <summary>
    /// Lens distortion intensity when fully submerged.
    /// Negative values create barrel distortion (edges bend outward).
    /// Recommended: -0.3 to -0.6 for subtle underwater warp.
    /// </summary>
    public float underwaterDistortion = -0.4f;

    [Header("Murky Color")]

    /// <summary>
    /// The full-screen tint color applied when submerged.
    /// Match this to the color of your flood water in the scene.
    /// Default is dark muddy brown (R:0.24 G:0.16 B:0.04).
    /// Alpha controls max opacity of the overlay at full submersion.
    /// </summary>
    public Color murkColor = new Color(0.24f, 0.16f, 0.04f, 0.65f);

    /// <summary>
    /// How much color is drained from the scene underwater.
    /// Range: 0 (no change) to -100 (fully grayscale).
    /// Recommended: -40 to -70 for dirty flood water feel.
    /// </summary>
    public float underwaterSaturation = -60f;

    /// <summary>
    /// How much the scene darkens when submerged.
    /// Negative values darken, positive values brighten.
    /// Recommended: -0.3 to -0.6 for limited underwater visibility.
    /// </summary>
    public float underwaterExposure = -0.5f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    // URP post-process override references (grabbed from Volume Profile at Start)
    private DepthOfField _dof;
    private LensDistortion _lensDistortion;
    private ColorAdjustments _colorAdjustments;

    // Current blend value (0 = fully above water, 1 = fully submerged)
    private float _currentBlend = 0f;

    // Tracks whether player is currently submerged to prevent redundant calls
    private bool _isSubmerged = false;

    // Reference to the running transition coroutine so it can be interrupted
    private Coroutine _transitionCoroutine;

    // Flag to prevent effects being applied before Start() completes
    private bool _initialized = false;

    // Original Volume Profile values saved at Start — used as the "dry" baseline
    // when lerping effects in and out
    private float _originalDofStart;
    private float _originalDofEnd;
    private float _originalDofRadius;
    private float _originalDistortion;
    private float _originalSaturation;
    private float _originalExposure;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        // Singleton setup — only one instance allowed
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        // Clear singleton reference so other scripts don't access a destroyed object
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Grabs Volume Profile override references and saves their original values
    /// as the "above water" baseline for smooth lerping.
    /// Also ensures the murk overlay starts fully transparent.
    /// </summary>
    private void Start()
    {
        if (volume == null)
        {
            Debug.LogError("[UnderwaterEffects] No Volume assigned. Drag your Global Volume into the Volume field.");
            enabled = false;
            return;
        }

        // Get override references from the Volume Profile
        volume.profile.TryGet(out _dof);
        volume.profile.TryGet(out _lensDistortion);
        volume.profile.TryGet(out _colorAdjustments);

        // Warn if any overrides are missing
        if (_dof == null)
            Debug.LogWarning("[UnderwaterEffects] Depth Of Field override missing from Volume Profile. Add it and set Mode to Gaussian.");
        if (_lensDistortion == null)
            Debug.LogWarning("[UnderwaterEffects] Lens Distortion override missing from Volume Profile.");
        if (_colorAdjustments == null)
            Debug.LogWarning("[UnderwaterEffects] Color Adjustments override missing from Volume Profile.");

        // Save original values so we can lerp back to them when surfacing
        if (_dof != null)
        {
            _originalDofStart = _dof.gaussianStart.value;
            _originalDofEnd = _dof.gaussianEnd.value;
            _originalDofRadius = _dof.gaussianMaxRadius.value;
            _dof.active = false; // DOF disabled above water for performance
        }

        if (_lensDistortion != null)
            _originalDistortion = _lensDistortion.intensity.value;

        if (_colorAdjustments != null)
        {
            _originalSaturation = _colorAdjustments.saturation.value;
            _originalExposure = _colorAdjustments.postExposure.value;
        }

        // Ensure overlay starts fully transparent
        if (murkOverlay != null)
        {
            Color c = murkColor;
            c.a = 0f;
            murkOverlay.color = c;
        }

        _initialized = true;
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Triggers the underwater effect transition.
    /// Called by WaterTraumaTrigger when the player's HEAD crosses the water surface.
    ///
    /// Passing true starts fading all effects IN (player submerging).
    /// Passing false starts fading all effects OUT (player surfacing).
    ///
    /// If called while a transition is already running, the current transition
    /// is interrupted and a new one starts immediately from the current blend value.
    /// </summary>
    /// <param name="submerged">True when head goes below water, false when head comes back up.</param>
    public void SetSubmerged(bool submerged)
    {
        if (_isSubmerged == submerged) return; // Already in this state, do nothing
        _isSubmerged = submerged;

        // Cancel any running transition and start a new one
        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        _transitionCoroutine = StartCoroutine(TransitionTo(submerged ? 1f : 0f));
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    /// <summary>
    /// Coroutine that smoothly moves _currentBlend toward the target value each frame.
    /// Calls ApplyEffects() every frame to update all post-process overrides.
    /// Disables Depth Of Field when fully above water to save performance.
    /// </summary>
    /// <param name="target">Target blend value: 1 = fully submerged, 0 = fully above water.</param>
    private IEnumerator TransitionTo(float target)
    {
        if (!_initialized) yield break;

        // Enable DOF at the start of any transition
        if (_dof != null) _dof.active = true;

        while (!Mathf.Approximately(_currentBlend, target))
        {
            _currentBlend = Mathf.MoveTowards(
                _currentBlend, target, Time.deltaTime / transitionSpeed);

            ApplyEffects(_currentBlend);
            yield return null;
        }

        _currentBlend = target;
        ApplyEffects(_currentBlend);

        // Disable DOF once fully surfaced — saves GPU cost above water
        if (_dof != null && _currentBlend <= 0f)
            _dof.active = false;

        _transitionCoroutine = null;
    }

    /// <summary>
    /// Applies all underwater post-process overrides proportional to the blend value.
    /// blend = 0 restores original values (above water).
    /// blend = 1 applies full underwater effect values.
    ///
    /// Called every frame during a transition by TransitionTo().
    /// </summary>
    /// <param name="blend">0 = dry/above water, 1 = fully submerged.</param>
    private void ApplyEffects(float blend)
    {
        // Blur — limits visibility distance underwater
        if (_dof != null)
        {
            _dof.gaussianStart.Override(Mathf.Lerp(_originalDofStart, underwaterBlurStart, blend));
            _dof.gaussianEnd.Override(Mathf.Lerp(_originalDofEnd, underwaterBlurEnd, blend));
            _dof.gaussianMaxRadius.Override(Mathf.Lerp(_originalDofRadius, maxBlurIntensity, blend));
        }

        // Lens warp — disorienting barrel distortion
        if (_lensDistortion != null)
            _lensDistortion.intensity.Override(Mathf.Lerp(_originalDistortion, underwaterDistortion, blend));

        // Color grading — desaturate and darken for murky look
        if (_colorAdjustments != null)
        {
            _colorAdjustments.saturation.Override(Mathf.Lerp(_originalSaturation, underwaterSaturation, blend));
            _colorAdjustments.postExposure.Override(Mathf.Lerp(_originalExposure, underwaterExposure, blend));
        }

        // Murky color overlay — brown tint matching flood water color
        if (murkOverlay != null)
        {
            Color c = murkColor;
            c.a = Mathf.Lerp(0f, murkColor.a, blend);
            murkOverlay.color = c;
        }
    }
}