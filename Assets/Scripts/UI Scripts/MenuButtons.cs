using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
   
    public string playSceneName = "GameScene";

  
    public void PlayGame()
    {
        // Mag-load kan next scene (main game)
        SceneManager.LoadScene(playSceneName);
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
}
