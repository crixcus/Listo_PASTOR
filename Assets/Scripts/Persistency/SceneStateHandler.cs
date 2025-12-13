using UnityEngine;
using UnityEngine.SceneManagement;


[DefaultExecutionOrder(-100)]
public class SceneStateHandler : MonoBehaviour
{
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


    void OnSceneUnloaded(Scene scene)
    {
        SaveSystem.CaptureScene();
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveSystem.RestoreScene();
    }
}