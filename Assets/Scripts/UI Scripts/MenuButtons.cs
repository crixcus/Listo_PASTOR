using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
   
    private string playSceneName = "GameScene";

    public GameObject epMenu;
    public GameObject ep1Title;
    public GameObject ep2Title;
    public GameObject ep3Title;
    public GameObject ep1Bg;
    public GameObject ep2Bg;
    public GameObject ep3Bg;
    public GameObject playButton;
    public GameObject mainMenu;

    public void Start()
    {

        epMenu.SetActive(false);
        ep1Title.SetActive(false);
        ep2Title.SetActive(false);
        ep3Title.SetActive(false);
        ep1Bg.SetActive(false);
        ep2Bg.SetActive(false);
        ep3Bg.SetActive(false);
        playButton.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void PlayButton()
    {
        epMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void BackButton()
    {
        ep1Title.SetActive(false);
        ep2Title.SetActive(false);
        ep3Title.SetActive(false);
        ep1Bg.SetActive(false);
        ep2Bg.SetActive(false);
        ep3Bg.SetActive(false);
        playButton.SetActive(false);
        epMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void Ep1Button()
    {
        ep1Title.SetActive(true);
        ep1Bg.SetActive(true);
        playButton.SetActive(true);
        playSceneName = "Level 1";

        ep2Title.SetActive(false);
        ep2Bg.SetActive(false);
        ep3Title.SetActive(false);
        ep3Bg.SetActive(false);
    }
    public void Ep2Button()
    {
        ep2Title.SetActive(true);
        ep2Bg.SetActive(true);
        playButton.SetActive(true);
        playSceneName = "Level 2";

        ep1Title.SetActive(false);
        ep1Bg.SetActive(false);
        ep3Title.SetActive(false);
        ep3Bg.SetActive(false);
    }
    public void Ep3Button()
    {
        ep3Title.SetActive(true);
        ep3Bg.SetActive(true);
        playButton.SetActive(true);
        playSceneName = "Level 3";

        ep2Title.SetActive(false);
        ep2Bg.SetActive(false);
        ep1Title.SetActive(false);
        ep1Bg.SetActive(false);
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
