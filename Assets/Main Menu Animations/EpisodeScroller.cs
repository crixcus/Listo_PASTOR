using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedEpisodeScroller : MonoBehaviour
{
    private int currentEpisodeIndex = 1;
    private const int MIN_EPISODE = 1;

    [Header("UI References")]
    public Button leftButton;
    public Button rightButton;

    [Header("Episode Panel Containers")]
    // Assign the main GameObjects/Panels for each episode here.
    public GameObject episode1Container;
    public GameObject episode2Container;
    public GameObject episode3Container;

    [Header("Animation States")]
    [Tooltip("State that moves the current panel off-screen to the left.")]
    [SerializeField] private string slideOutLeftState = "Panel_SlideLeft";
    [Tooltip("State that brings the next panel in from the right.")]
    [SerializeField] private string slideInFromRightState = "Panel_SlideRight";
    [Tooltip("State that moves the current panel off-screen to the right.")]
    [SerializeField] private string slideOutRightState = "Panel_SlideRight";
    [Tooltip("State that brings the previous panel in from the left.")]
    [SerializeField] private string slideInFromLeftState = "Panel_SlideLeft";

    [Tooltip("Seconds to wait before disabling the outgoing panel.")]
    [SerializeField] private float transitionDuration = 0.4f;

    private Animator currentAnimator;
    private GameObject[] episodeContainers;
    private Animator[] episodeAnimators;
    private int episodeCount;

    void Awake()
    {
        // Organize containers into an array for easier access (index 0 is Episode 1)
        episodeContainers = new GameObject[] { episode1Container, episode2Container, episode3Container };
        episodeCount = episodeContainers.Length;

        if (episodeCount == 0)
        {
            Debug.LogError("AnimatedEpisodeScroller: No episode containers assigned.");
            enabled = false;
            return;
        }

        // Get Animators from all containers
        episodeAnimators = new Animator[episodeCount];
        for (int i = 0; i < episodeCount; i++)
        {
            if (episodeContainers[i] == null)
            {
                Debug.LogError($"AnimatedEpisodeScroller: Episode container at index {i} is not assigned.");
                enabled = false;
                return;
            }

            episodeAnimators[i] = episodeContainers[i].GetComponent<Animator>();
            if (episodeAnimators[i] == null)
            {
                Debug.LogError($"AnimatedEpisodeScroller: Missing Animator on container {episodeContainers[i].name}.");
                enabled = false;
                return;
            }
        }

        // Clamp starting index in case it drifts out of bounds
        currentEpisodeIndex = Mathf.Clamp(currentEpisodeIndex, MIN_EPISODE, episodeCount);
    }

    void Start()
    {
        // Set up the initial state: Only Episode 1 is visible
        for (int i = 0; i < episodeCount; i++)
        {
            // Only Episode 1 (index 0) is active initially
            episodeContainers[i].SetActive(i == 0);
        }

        currentAnimator = episodeAnimators[currentEpisodeIndex - 1];
        CheckButtonVisibility();
    }

    // Called by the RIGHT button's OnClick event
    public void ScrollRight()
    {
        if (currentEpisodeIndex < episodeCount)
        {
            int oldIndex = currentEpisodeIndex;
            currentEpisodeIndex++;

            // Trigger the animation sequence
            AnimatePanelTransition(true, oldIndex, currentEpisodeIndex);

            CheckButtonVisibility();
        }
    }

    // Called by the LEFT button's OnClick event
    public void ScrollLeft()
    {
        if (currentEpisodeIndex > MIN_EPISODE)
        {
            int oldIndex = currentEpisodeIndex;
            currentEpisodeIndex--;

            // Trigger the animation sequence
            AnimatePanelTransition(false, oldIndex, currentEpisodeIndex);

            CheckButtonVisibility();
        }
    }

    // --- Core Animation Logic ---
    private void AnimatePanelTransition(bool movingRight, int oldIndex, int newIndex)
    {
        // 1. Get Animators for the outgoing and incoming panels
        Animator outgoingAnimator = episodeAnimators[oldIndex - 1];
        Animator incomingAnimator = episodeAnimators[newIndex - 1];
        GameObject outgoingContainer = episodeContainers[oldIndex - 1];
        GameObject incomingContainer = episodeContainers[newIndex - 1];

        string outgoingState = movingRight ? slideOutLeftState : slideOutRightState;
        string incomingState = movingRight ? slideInFromRightState : slideInFromLeftState;

        // 2. Play the slide-out animation on the OLD panel.
        // We play the OPPOSITE of the movement direction (e.g., if we scrolled Right, the old panel slides Left)
        outgoingAnimator.Play(outgoingState, 0, 0f);

        // 3. Activate the new panel and play the slide-in animation.
        // We assume the SlideLeft animation moves the panel out of view, and SlideRight moves it into view.
        // If your animations are defined differently, you may need to adjust the animation names here.
        incomingContainer.SetActive(true);
        incomingAnimator.Play(incomingState, 0, 0f);

        // Hide the old panel after the animation finishes
        StopAllCoroutines(); // ensure previous hides don't conflict
        StartCoroutine(DisableAfterDelay(outgoingContainer, transitionDuration));
    }

    // Disables buttons when episode limits are reached (Same as before)
    private void CheckButtonVisibility()
    {
        if (leftButton != null)
        {
            // Disable Left button if at Episode 1
            leftButton.interactable = (currentEpisodeIndex > MIN_EPISODE);
        }
        if (rightButton != null)
        {
            // Disable Right button if at Episode 3
            rightButton.interactable = (currentEpisodeIndex < episodeCount);
        }
    }

    private IEnumerator DisableAfterDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null)
        {
            target.SetActive(false);
        }
    }
}