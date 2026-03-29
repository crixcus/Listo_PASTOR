using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
   
    private string playSceneName = "GameScene";

    public GameObject epMenu;
    public GameObject mainMenu;
    public GameObject settingsButton;
    public GameObject credsButton;

    public void Start()
    {
        epMenu.SetActive(false);
        mainMenu.SetActive(true);
        settingsButton.SetActive(false);
        credsButton.SetActive(false);
    }

    public void PlayButton()
    {
        epMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void BackButton()
    {
        epMenu.SetActive(false);
        mainMenu.SetActive(true);
        settingsButton.SetActive(false);
        credsButton.SetActive(false);
    }

    public void Ep1Button()
    {
        playSceneName = "Level 1";
        PlayEpisode();

    }
    public void Ep2Button()
    {;
        playSceneName = "Level 2";
        PlayEpisode();

    }
    public void Ep3Button()
    {
        playSceneName = "Level 3 (Final)";
        PlayEpisode();

    }

    public void Settings()
    {
        settingsButton.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void Credits()
    {
        credsButton.SetActive(true);
        mainMenu.SetActive(false);
    }

    // --- EXIT BUTTON ---
    public void ExitGame()
    {
        // Para ma-stop an game kung naka-build
        Application.Quit();

        // Para makita mo sa editor na nag-call siya
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void PlayEpisode()
    {
        // Mag-load kan next scene (main game)
        SceneManager.LoadScene(playSceneName);
    }
}
