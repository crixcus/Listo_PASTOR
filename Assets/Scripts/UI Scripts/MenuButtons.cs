using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuButtons : MonoBehaviour
{
    [Header("Panels")]
    public CanvasGroup mainMenuCG;
    public RectTransform mainMenuRT;

    public CanvasGroup episodesPanelCG;
    public RectTransform episodesPanelRT;

    public CanvasGroup settingsPanelCG;
    public RectTransform settingsPanelRT;

    public CanvasGroup creditsPanelCG;
    public RectTransform creditsPanelRT;

    [Header("Main Menu Buttons (top to bottom)")]
    public RectTransform[] menuButtons;

    [Header("Episode Cards")]
    public RectTransform episodeCardContainer;
    public CanvasGroup episodeCardCG;

    [Header("Transition")]
    public float panelDuration = 0.35f;
    public float slideDistance = 60f;
    public float staggerDelay = 0.08f;

    private int _currentEp = 0;
    private string[] _epScenes = { "Level 1", "Level 2", "Level 3 (Final)" };
    private bool _isTransitioning = false;

    private Vector2 _mainMenuRestPos;
    private Vector2 _episodesRestPos;
    private Vector2 _settingsRestPos;
    private Vector2 _creditsRestPos;

    private Vector2 _episodeCardRestPos;

    void Start()
    {
        Debug.Log("MenuButtons Start() called");

        _mainMenuRestPos = mainMenuRT.anchoredPosition;
        _episodesRestPos = episodesPanelRT.anchoredPosition;
        _settingsRestPos = settingsPanelRT.anchoredPosition;
        _creditsRestPos = creditsPanelRT.anchoredPosition;

        // Null checks
        if (mainMenuCG == null) { Debug.LogError("mainMenuCG is NULL"); return; }
        if (mainMenuRT == null) { Debug.LogError("mainMenuRT is NULL"); return; }
        if (episodesPanelCG == null) { Debug.LogError("episodesPanelCG is NULL"); return; }
        if (settingsPanelCG == null) { Debug.LogError("settingsPanelCG is NULL"); return; }
        if (creditsPanelCG == null) { Debug.LogError("creditsPanelCG is NULL"); return; }

        Debug.Log("All panel refs OK");

        if (menuButtons == null || menuButtons.Length == 0)
        { Debug.LogError("menuButtons array is empty!"); return; }

        Debug.Log("menuButtons count: " + menuButtons.Length);

        // Check each button
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null)
            {
                Debug.LogError("menuButtons[" + i + "] is NULL");
                return;
            }
            var cg = menuButtons[i].GetComponent<CanvasGroup>();
            if (cg == null)
                Debug.LogWarning("menuButtons[" + i + "] has no CanvasGroup — will add one");
        }

        Debug.Log("All button refs OK — starting setup");

        SetPanelState(episodesPanelCG, episodesPanelRT, false, true);
        SetPanelState(settingsPanelCG, settingsPanelRT, false, true);
        SetPanelState(creditsPanelCG, creditsPanelRT, false, true);

        Debug.Log("Panels hidden — starting stagger");

        StaggerButtonsIn();

        Debug.Log("StaggerButtonsIn called");
    }

    public void OnChooseEpisodePressed()
    {
        SlideOut(mainMenuCG, mainMenuRT, -1);
        SlideIn(episodesPanelCG, episodesPanelRT);
        InitCarousel();
    }
    public void OnSettingsPressed()
    {
        SlideOut(mainMenuCG, mainMenuRT, -1);
        SlideIn(settingsPanelCG, settingsPanelRT);
    }

    public void OnCreditsPressed()
    {
        SlideOut(mainMenuCG, mainMenuRT, -1);
        SlideIn(creditsPanelCG, creditsPanelRT);
    }

    public void OnCloseCredits()
    {
        SlideOut(creditsPanelCG, creditsPanelRT, 1);
        SlideIn(mainMenuCG, mainMenuRT);
    }

    public void OnCloseSettings()
    {
        SlideOut(settingsPanelCG, settingsPanelRT, 1);
        SlideIn(mainMenuCG, mainMenuRT);
    }


    public void OnBackPressed()
    {
        if (episodesPanelCG.gameObject.activeSelf)
            SlideOut(episodesPanelCG, episodesPanelRT, 1);
        if (settingsPanelCG.gameObject.activeSelf)
            SlideOut(settingsPanelCG, settingsPanelRT, 1);
        if (creditsPanelCG.gameObject.activeSelf)
            SlideOut(creditsPanelCG, creditsPanelRT, 1);

        SlideIn(mainMenuCG, mainMenuRT);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnNextEpisode() => SwipeEpisode(1);
    public void OnPrevEpisode() => SwipeEpisode(-1);

    void SwipeEpisode(int dir)
    {
        if (_isTransitioning) return;

        int next = (_currentEp + dir + episodeCardContainer.childCount) % episodeCardContainer.childCount;

        Debug.Log("Current: " + _currentEp + " Dir: " + dir + " Next: " + next + " ChildCount: " + episodeCardContainer.childCount);

        if (next == _currentEp) return;

        var currentCard = episodeCardContainer.GetChild(_currentEp).GetComponent<RectTransform>();
        var currentCG = currentCard.GetComponent<CanvasGroup>();
        if (currentCG == null) currentCG = currentCard.gameObject.AddComponent<CanvasGroup>();

        // Slide and fade out current
        currentCard.DOAnchorPosX(_episodeCardRestPos.x + (-dir * slideDistance * 2f),panelDuration).SetEase(Ease.InCubic);
        currentCG.DOFade(0f, panelDuration * 0.7f)
                 .OnComplete(() =>
                 {
                     currentCard.gameObject.SetActive(false);
                     _currentEp = next;
                     ShowEpisodeCard(_currentEp, animate: true, fromDir: dir);
                     _isTransitioning = false;
                 });
    }


    void ShowEpisodeCard(int index, bool animate, int fromDir = 1)
    {
        for (int i = 0; i < episodeCardContainer.childCount; i++)
            episodeCardContainer.GetChild(i).gameObject.SetActive(i == index);

        var rt = episodeCardContainer.GetChild(index).GetComponent<RectTransform>();
        var cg = rt.GetComponent<CanvasGroup>();
        if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();

        if (animate)
        {
            rt.anchoredPosition = new Vector2(
                _episodeCardRestPos.x + fromDir * slideDistance * 2f,
                _episodeCardRestPos.y
            );
            cg.alpha = 0f;
            rt.DOAnchorPos(_episodeCardRestPos, panelDuration).SetEase(Ease.OutCubic);
            cg.DOFade(1f, panelDuration * 0.8f);
        }
        else
        {
            rt.anchoredPosition = _episodeCardRestPos;
            cg.alpha = 1f;
        }
    }

    public void OnPlayCurrentEpisode()
    {
        LevelManager.Instance.LoadScene(_epScenes[_currentEp], "polskrin mo");
    }

    void SlideIn(CanvasGroup cg, RectTransform rt)
    {
        Vector2 restPos = GetRestPos(rt);

        cg.gameObject.SetActive(true);
        cg.alpha = 0f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        rt.anchoredPosition = restPos + Vector2.down * slideDistance;
        rt.DOAnchorPos(restPos, panelDuration).SetEase(Ease.OutCubic);
        cg.DOFade(1f, panelDuration * 0.8f);
    }

    void SlideOut(CanvasGroup cg, RectTransform rt, int dir)
    {
        Vector2 restPos = GetRestPos(rt);

        cg.blocksRaycasts = false;
        cg.interactable = false;

        Vector2 target = restPos + Vector2.up * slideDistance * dir;
        rt.DOAnchorPos(target, panelDuration).SetEase(Ease.InCubic);
        cg.DOFade(0f, panelDuration * 0.8f)
          .OnComplete(() => cg.gameObject.SetActive(false));
    }

    Vector2 GetRestPos(RectTransform rt)
    {
        if (rt == mainMenuRT) return _mainMenuRestPos;
        if (rt == episodesPanelRT) return _episodesRestPos;
        if (rt == settingsPanelRT) return _settingsRestPos;
        if (rt == creditsPanelRT) return _creditsRestPos;
        return rt.anchoredPosition;
    }

    void SetPanelState(CanvasGroup cg, RectTransform rt, bool visible, bool instant)
    {
        cg.alpha = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
        cg.interactable = visible;
        cg.gameObject.SetActive(visible);
    }

    void StaggerButtonsIn()
    {
        foreach (var btn in menuButtons)
        {
            var cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            btn.anchoredPosition += Vector2.down * 30f;
            Debug.Log("Hiding button: " + btn.gameObject.name);
        }

        for (int i = 0; i < menuButtons.Length; i++)
        {
            var btn = menuButtons[i];
            var cg = btn.GetComponent<CanvasGroup>();
            float delay = 0.25f + i * staggerDelay;

            Debug.Log("Tweening button: " + btn.gameObject.name + " delay: " + delay);

            btn.DOAnchorPosY(btn.anchoredPosition.y + 30f, 0.4f)
               .SetEase(Ease.OutBack)
               .SetDelay(delay);

            cg.DOFade(1f, 0.35f).SetDelay(delay);
        }
    }

    void InitCarousel()
    {
        _currentEp = 0;
        _isTransitioning = false;

        // Cache rest position from first card
        _episodeCardRestPos = episodeCardContainer.GetChild(0)
                               .GetComponent<RectTransform>().anchoredPosition;

        // Hide all cards first
        for (int i = 0; i < episodeCardContainer.childCount; i++)
        {
            var child = episodeCardContainer.GetChild(i).GetComponent<RectTransform>();
            var cg = child.GetComponent<CanvasGroup>();
            if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();

            child.gameObject.SetActive(false);
            cg.alpha = 0f;
        }

        // Show only first card
        ShowEpisodeCard(0, animate: false);
    }
}