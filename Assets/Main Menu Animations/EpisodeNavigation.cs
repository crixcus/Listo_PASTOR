using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EpisodeNavigation : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button prevButton;
    public Button nextButton;


    [Header("Panel Animators")]
    public Animator panel1;
    public Animator panel2;
    public Animator panel3;

    private int currentEpisode = 1;
    private int maxEpisode = 3;

    void Start()
    {
        UpdateButtons();
        StartCoroutine(PlayInitialIntro());
    }

    private IEnumerator PlayInitialIntro()
    {
        yield return null; // wait 1 frame
        panel1.Play("Panel1_Intro");
    }

    public void NextEpisode()
    {
        if (currentEpisode >= maxEpisode) return;

        PlayTransition(currentEpisode, currentEpisode + 1);

        currentEpisode++;
        UpdateButtons();
    }

    public void PreviousEpisode()
    {
        if (currentEpisode <= 1) return;

        PlayTransition(currentEpisode, currentEpisode - 1);

        currentEpisode--;
        UpdateButtons();
    }

    private void PlayTransition(int from, int to)
    {
        // Play outro of "from"
        GetPanelAnimator(from).Play($"Panel{from}_Outro", 0, 0f);

        // Play intro of "to"
        GetPanelAnimator(to).Play($"Panel{to}_Intro", 0, 0f);
    }

    private Animator GetPanelAnimator(int episode)
    {
        switch (episode)
        {
            case 1: return panel1;
            case 2: return panel2;
            case 3: return panel3;
            default: return null;
        }
    }

    private void UpdateButtons()
    {
        prevButton.interactable = currentEpisode > 1;
        nextButton.interactable = currentEpisode < maxEpisode;
    }
}
