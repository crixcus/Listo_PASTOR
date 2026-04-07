using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PausePanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject tips;
    public GameObject PhoneAlert;

    [Header("Player Reference")]
    [Tooltip("Assign the PlayerHolder GameObject here.")]
    public GameObject playerHolder;

    private bool isPaused = false;

    // Components to disable when paused
    private BasicMovements _basicMovements;
    private PlayerInteraction _playerInteraction;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Grab player components if playerHolder is assigned
        if (playerHolder != null)
        {
            _basicMovements = playerHolder.GetComponent<BasicMovements>();
            _playerInteraction = playerHolder.GetComponent<PlayerInteraction>();
        }

        // Make sure game starts in correct state
        LockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
            if (isPaused)
            {
                if (settingsPanel != null)
                    settingsPanel.SetActive(false);
                if (tips != null)
                    tips.SetActive(false);
                if (PhoneAlert != null)
                    PhoneAlert.SetActive(false);
                //LockCursor();
                //ResumeGame();
            }

            else
            {
                PauseGame();
            }
        }
    }

    // ------------------------------------------------------------------
    // Pause / Resume
    // ------------------------------------------------------------------

    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        //Time.timeScale = 0f;
        isPaused = true;

        // Disable player look and interaction
        if (_basicMovements != null) _basicMovements.enabled = false;
        if (_playerInteraction != null) _playerInteraction.enabled = false;

        UnlockCursor();
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        // Re-enable player look and interaction
        if (_basicMovements != null) _basicMovements.enabled = true;
        if (_playerInteraction != null) _playerInteraction.enabled = true;

        LockCursor();
    }

    public void SettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void SettingsClose()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        UnlockCursor();
        pausePanel.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void TipsPanel()
    {
        tips.SetActive(true);
    }

    // ------------------------------------------------------------------
    // Cursor helpers
    // ------------------------------------------------------------------

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}