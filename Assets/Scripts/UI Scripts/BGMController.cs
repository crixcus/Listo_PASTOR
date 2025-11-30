using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))] 

public class BGMController : MonoBehaviour
{
    private AudioSource audioSource;

    [Tooltip("The audio clip (music track) to play in this scene.")]
    public AudioClip backgroundMusic;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true; 
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // --- Event Handlers ---

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log($"BGM started for scene: {scene.name}");
        }
    }

    // Called automatically before a scene is unloaded
    private void OnSceneUnloaded(Scene scene)
    {
        // Stop the music right before the scene is unloaded
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log($"BGM stopped before leaving scene: {scene.name}");
        }
    }
}