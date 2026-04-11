using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject loadScreen;
    public ProgressBar bar;

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
            return;
        }

        SceneManager.LoadSceneAsync((int)SceneIndexes.MainMenu, LoadSceneMode.Additive);
    }

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    public void LoadGame(string targetScene)
    {
        loadScreen.SetActive(true);
        scenesLoading.Clear();

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "LoadingScene")
        {
            scenesLoading.Add(SceneManager.UnloadSceneAsync(currentScene.name));
        }

        scenesLoading.Add(SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive));

        StartCoroutine(GetSceneLoadProgress(targetScene));
    }

    float totalSceneProgress;
    public IEnumerator GetSceneLoadProgress(string targetScene)
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;

                foreach (AsyncOperation operation in scenesLoading)
                {
                    totalSceneProgress += operation.progress;
                }

                totalSceneProgress = (totalSceneProgress / scenesLoading.Count) * 100f;

                bar.current =  Mathf.RoundToInt(totalSceneProgress);

                yield return null;
            }
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));

        loadScreen.SetActive(false);
    }
}
