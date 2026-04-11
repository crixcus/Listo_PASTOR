using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Slider progressBar;
    public GameObject transitionsContainer;

    private SceneTransition[] transitions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
    }

    public void LoadScene(string sceneName, string transitionName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        SceneTransition transition = transitions.First(t => t.name == transitionName);

        // Start transition in first
        yield return transition.AnimateTransitionIn();

        // Then start loading scene
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        progressBar.gameObject.SetActive(true);

        // Update progress bar until 0.9
        while (scene.progress < 0.9f)
        {
            progressBar.value = scene.progress;
            yield return null;
        }

        // Smoothly interpolate bar from 0.9 to 1.0 over 1 second
        float elapsed = 0f;
        float duration = 1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            progressBar.value = Mathf.Lerp(0.9f, 1f, elapsed / duration);
            yield return null;
        }

        // Now allow scene activation
        scene.allowSceneActivation = true;

        progressBar.gameObject.SetActive(false);

        // Keep bar visible until transition out finishes
        yield return transition.AnimateTransitionOut();
    }
}