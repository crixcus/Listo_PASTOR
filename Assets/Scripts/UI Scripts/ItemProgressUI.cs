using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Circular progress ring UI that tracks collected items via
/// InteractableItems.GlobalCounter — no trigger zone needed.
///
/// Mirrors the structure of StackProgressUI but polls GlobalCounter
/// every frame so it updates the moment any InteractableItems is interacted with.
///
/// SETUP:
///   1. Create a Canvas (Screen Space Overlay) — name it "ItemProgressUI"
///   2. Create a Panel child — add CanvasGroup + ItemProgressUI to it
///   3. Inside the Panel add:
///      - ringFill      : Image → Image Type: Filled, Fill Method: Radial 360, Fill Origin: Top
///      - countText     : TMP Text (large center number — current collected count)
///      - totalText     : TMP Text (small "of X")
///      - hintText      : TMP Text ("X more to go")
///      - dotsContainer : Empty GameObject with Horizontal Layout Group
///   4. Assign all references in the Inspector
///   5. Assign dotPrefab → a simple small Image prefab (12×12 square)
///   6. Set totalItems to match the collection goal (default 11)
///      — dots rebuild automatically if you change it at runtime
/// </summary>
public class ItemProgressUI : MonoBehaviour
{
    public static ItemProgressUI Instance { get; private set; }
    public HintsScript hints1;
    public sceneTrigger lvl1Trigger;

    [Header("Ring")]
    [Tooltip("Filled Image for the progress ring. " +
             "Set Image Type: Filled, Fill Method: Radial 360, Fill Origin: Top.")]
    public Image ringFill;

    [Tooltip("Ring color at low progress (0 items collected).")]
    public Color ringColorLow = new Color(0.89f, 0.60f, 0.13f, 1f);   // warm amber

    [Tooltip("Ring color at full progress (all items collected).")]
    public Color ringColorHigh = new Color(0.11f, 0.75f, 0.40f, 1f);  // fresh green

    [Header("Text")]
    [Tooltip("Large center number showing how many items have been collected.")]
    public TMP_Text countText;

    [Tooltip("Small text showing total e.g. 'of 11'.")]
    public TMP_Text totalText;

    [Tooltip("Hint text e.g. '5 more to go' / 'All items collected!'.")]
    public TMP_Text hintText;

    [Header("Dots")]
    [Tooltip("Parent container with Horizontal Layout Group. Dots are spawned here.")]
    public RectTransform dotsContainer;

    [Tooltip("Prefab for each dot — a small Image (12×12). " +
             "Create a UI Image, set size to 12×12, save to Project as a prefab.")]
    public GameObject dotPrefab;

    [Tooltip("Color of a collected item dot.")]
    public Color dotCollectedColor = new Color(0.11f, 0.75f, 0.40f, 1f);

    [Tooltip("Color of an uncollected item dot.")]
    public Color dotEmptyColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    [Header("Goal")]
    [Tooltip("Total items to collect this level. " +
             "Keep in sync with the threshold inside InteractableItems.InteractItem().")]
    public int totalItems = 11;

    [Header("Visibility")]
    [Tooltip("If true, the UI is always visible. " +
             "If false, call SetVisible(true/false) manually (e.g. from SafeZone).")]
    public bool alwaysVisible = true;

    [Tooltip("How fast the UI fades in and out.")]
    public float fadeSpeed = 2f;

    // ─── Private state ─────────────────────────────────────────────────────────

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;
    private List<Image> _dots = new List<Image>();
    private int _currentTotal = 0;
    private int _lastCount = -1;   // cache — avoids redundant UI refreshes

    // ─── Unity lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Start invisible — will be shown in Start or via SetVisible()
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        
        // Build dots and push initial zeroed state
        BuildDots(totalItems);
        RefreshUI(0, totalItems);

        StartCoroutine(HideUI());
        _lastCount = 0;
    }

    private void Update()
    {
        int current = InteractableItems.GlobalCounter;

        // Only refresh when the counter actually changed
        if (current == _lastCount) return;

        _lastCount = current;
        RefreshUI(current, totalItems);
    }

    // ─── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Force-refresh the UI with explicit values.
    /// Normally called automatically from Update(), but you can call this
    /// manually if you need an immediate repaint (e.g. after ResetCounter).
    /// </summary>
    public void UpdateUI(int collected, int total)
    {
        if (total != _currentTotal)
            BuildDots(total);

        RefreshUI(collected, total);
    }

    IEnumerator HideUI()
    {
        yield return new WaitForSeconds(10f);
        
        if (alwaysVisible)
            SetVisible(true);
    }

    /// <summary>
    /// Fades the entire UI panel in or out smoothly.
    /// Only needed when alwaysVisible is false.
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeTo(visible ? 1f : 0f));
    }

    // ─── Private helpers ────────────────────────────────────────────────────────

    private void RefreshUI(int collected, int total)
    {
        if (total <= 0) return;

        // Rebuild dot row if total count changed
        if (total != _currentTotal)
            BuildDots(total);

        float progress = (float)collected / total;

        // Ring fill + color lerp
        if (ringFill != null)
        {
            ringFill.fillAmount = progress;
            ringFill.color = Color.Lerp(ringColorLow, ringColorHigh, progress);
        }

        // Center count
        if (countText != null)
            countText.text = collected.ToString();

        // "of X" label
        if (totalText != null)
            totalText.text = $"of {total}";

        // Hint line
        if (hintText != null)
        {
            int remaining = total - collected;
            hintText.text = remaining <= 0
                ? "All items collected!"
                : $"{remaining} more to go";
            if (remaining <= 0)
            {
                StartCoroutine(LastTask());
            }
        }

        // Dots — fill left to right as items are collected
        for (int i = 0; i < _dots.Count; i++)
            _dots[i].color = i < collected ? dotCollectedColor : dotEmptyColor;
    }

    /// <summary>
    /// Clears and rebuilds the dot row to match the given total.
    /// Runs automatically whenever totalItems changes.
    /// </summary>
    private void BuildDots(int total)
    {
        foreach (var dot in _dots)
            if (dot != null) Destroy(dot.gameObject);

        _dots.Clear();

        if (dotsContainer == null || dotPrefab == null)
        {
            Debug.LogWarning("[ItemProgressUI] dotsContainer or dotPrefab not assigned.");
            _currentTotal = total;
            return;
        }

        for (int i = 0; i < total; i++)
        {
            GameObject go = Instantiate(dotPrefab, dotsContainer);
            Image img = go.GetComponent<Image>();

            if (img != null)
            {
                img.color = dotEmptyColor;
                _dots.Add(img);
            }
        }

        _currentTotal = total;
    }

    private IEnumerator FadeTo(float target)
    {
        while (!Mathf.Approximately(_canvasGroup.alpha, target))
        {
            _canvasGroup.alpha = Mathf.MoveTowards(
                _canvasGroup.alpha, target, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }

        _canvasGroup.alpha = target;
        _fadeCoroutine = null;
    }

    IEnumerator LastTask()
    {
        Debug.Log("Waiting");
        yield return new WaitForSeconds(2f);
        hints1.Hint1Show();
        lvl1Trigger.StartEarly();
    }
}