using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables; // Required for interacting with PlayableDirector

public class SceneChanger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The name of the scene to load after the current one.")]
    public string nextSceneName = "Level_02";

    [Tooltip("Optional: Drag your Playable Director component here if you want to automatically trigger the load when it stops.")]
    public PlayableDirector playableDirector;

    private void Start()
    {
        // Check if a PlayableDirector is assigned and automatically subscribe to its 'stopped' event
        if (playableDirector != null)
        {
            // Subscribe the LoadNextScene method to the director's stopped event
            // This is the cleanest way to handle the end of a Timeline
            playableDirector.stopped += OnTimelineFinished;

            Debug.Log($"SceneChanger subscribed to {playableDirector.name}'s stopped event.");
        }
    }

    /// <summary>
    /// Public method to load the next scene. 
    /// This can be called by a Timeline Signal, a button, or another script.
    /// </summary>
    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Next scene name is empty. Please set the 'Next Scene Name' in the Inspector.");
            return;
        }

        // Make sure the scene is added to your Build Settings!
        Debug.Log($"Loading scene: {nextSceneName}...");
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Event handler for when the Playable Director stops naturally (not paused or stopped by a script).
    /// </summary>
    private void OnTimelineFinished(PlayableDirector director)
    {
        // We ensure the event is coming from the director we are tracking, though not strictly necessary if only one is assigned.
        if (director == playableDirector)
        {
            LoadNextScene();
        }
    }

    // Best practice to unsubscribe from events when the object is destroyed
    private void OnDestroy()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineFinished;
        }
    }
}