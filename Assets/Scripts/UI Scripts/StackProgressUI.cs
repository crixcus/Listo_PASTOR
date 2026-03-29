using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Circular progress ring UI for the safe zone stacking mechanic.
/// Automatically generates item dots based on totalItems — no manual setup needed.
/// Fades in when player enters the safe zone, fades out when they leave.
///
/// SETUP:
///   1. Create a Canvas (Screen Space Overlay) — name it "SafeZoneUI"
///   2. Create a Panel child — add CanvasGroup + StackProgressUI to it
///   3. Inside Panel add:
///      - ringFill    : Image → Image Type: Filled, Fill Method: Radial 360, Fill Origin: Top
///      - countText   : TMP Text (large center number)
///      - totalText   : TMP Text (small "of X")
///      - hintText    : TMP Text ("X more to safety")
///      - dotsContainer: Empty GameObject with Horizontal Layout Group
///   4. Assign references in Inspector
///   5. Assign dotPrefab → a simple small Image prefab (12x12 square)
///   6. Set totalItems in SafeZone — dots are generated automatically on Start
/// </summary>
public class StackProgressUI : MonoBehaviour
{
    public static StackProgressUI Instance { get; private set; }

    [Header("Ring")]
    [Tooltip("Filled Image for the progress ring. " +
             "Set Image Type: Filled, Fill Method: Radial 360, Fill Origin: Top.")]
    public Image ringFill;

    [Tooltip("Ring color at low progress.")]
    public Color ringColorLow = new Color(0.11f, 0.62f, 0.46f, 1f);

    [Tooltip("Ring color at full progress.")]
    public Color ringColorHigh = new Color(0.11f, 0.62f, 0.46f, 1f);

    [Header("Text")]
    [Tooltip("Large center number showing saved count.")]
    public TMP_Text countText;

    [Tooltip("Small text showing total e.g. 'of 5'.")]
    public TMP_Text totalText;

    [Tooltip("Hint text e.g. '2 more to safety'.")]
    public TMP_Text hintText;

    [Header("Dots")]
    [Tooltip("Parent container with Horizontal Layout Group. Dots are spawned here.")]
    public RectTransform dotsContainer;

    [Tooltip("Prefab for each dot. Should be a small Image (12x12). " +
             "Create a UI Image, set size to 12x12, drag to Project window to make prefab.")]
    public GameObject dotPrefab;

    [Tooltip("Color of a saved item dot.")]
    public Color dotSavedColor = new Color(0.11f, 0.62f, 0.46f, 1f);

    [Tooltip("Color of an unsaved item dot.")]
    public Color dotEmptyColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    [Header("Fade")]
    [Tooltip("How fast the UI fades in and out.")]
    public float fadeSpeed = 2f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    // Generated dot images — rebuilt whenever total changes
    private List<Image> _dots = new List<Image>();
    private int _currentTotal = 0;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Start invisible
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Updates all UI elements to reflect current save progress.
    /// If total has changed, regenerates the dot row automatically.
    /// Called by SafeZone whenever an item enters or exits the zone.
    /// </summary>
    public void UpdateUI(int saved, int total)
    {
        if (total <= 0) return;

        // Regenerate dots if total count changed
        if (total != _currentTotal)
            BuildDots(total);

        float progress = (float)saved / total;

        // Ring
        if (ringFill != null)
        {
            ringFill.fillAmount = progress;
            ringFill.color = Color.Lerp(ringColorLow, ringColorHigh, progress);
        }

        // Count
        if (countText != null)
            countText.text = saved.ToString();

        // Total
        if (totalText != null)
            totalText.text = $"of {total}";

        // Hint
        if (hintText != null)
        {
            int remaining = total - saved;
            hintText.text = remaining <= 0
                ? "All items safe!"
                : $"{remaining} more to safety";
        }

        // Dots — fill left to right as items are saved
        for (int i = 0; i < _dots.Count; i++)
            _dots[i].color = i < saved ? dotSavedColor : dotEmptyColor;
    }

    /// <summary>
    /// Fades the entire UI panel in or out smoothly.
    /// Called by SafeZone when the player enters or exits the trigger zone.
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeTo(visible ? 1f : 0f));
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    /// <summary>
    /// Destroys existing dots and spawns a new row of dots matching the total.
    /// Called automatically when totalItems changes.
    /// Each dot is instantiated from dotPrefab inside dotsContainer.
    /// </summary>
    private void BuildDots(int total)
    {
        // Clear existing dots
        foreach (var dot in _dots)
        {
            if (dot != null)
                Destroy(dot.gameObject);
        }
        _dots.Clear();

        if (dotsContainer == null || dotPrefab == null)
        {
            Debug.LogWarning("[StackProgressUI] dotsContainer or dotPrefab not assigned.");
            _currentTotal = total;
            return;
        }

        // Spawn one dot per item
        for (int i = 0; i < total; i++)
        {
            GameObject dotObj = Instantiate(dotPrefab, dotsContainer);
            Image dotImage = dotObj.GetComponent<Image>();

            if (dotImage != null)
            {
                dotImage.color = dotEmptyColor;
                _dots.Add(dotImage);
            }
        }

        _currentTotal = total;
    }

    private IEnumerator FadeTo(float target)
    {
        while (!Mathf.Approximately(_canvasGroup.alpha, target))
        {
            _canvasGroup.alpha = Mathf.MoveTowards(
                _canvasGroup.alpha, target, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        _canvasGroup.alpha = target;
        _fadeCoroutine = null;
    }
}