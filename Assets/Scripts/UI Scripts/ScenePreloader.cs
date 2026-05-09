using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePreloader : MonoBehaviour
{
    private const string TargetSceneKey = "PreloaderTargetScene";

    public static void GoTo(string sceneName)
    {
        PlayerPrefs.SetString(TargetSceneKey, sceneName);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Preloader");
    }

    private void Start()
    {
        string targetScene = PlayerPrefs.GetString(TargetSceneKey, "");

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("[ScenePreloader] No target scene set. " +
                           "Use ScenePreloader.GoTo('SceneName') to load scenes.");
            return;
        }

        // Clear the saved key
        PlayerPrefs.DeleteKey(TargetSceneKey);

        // Now load the actual target scene
        // By this point shaders from the previous attempt are cached
        LevelManager.Instance.LoadScene("Level 2", "polskrin mo");
    }
}
