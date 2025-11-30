using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverPanel : MonoBehaviour
{
    [Header("Game Over Settings")]
    public GameObject gameOverUI;       // "Game Over" panel
    public Image backgroundOverlay;     // Fade overlay
    public float restartDelay = 3f;
    public float fadeDuration = 1.5f;

    private bool isGameOver = false;
    private CanvasGroup uiGroup;

    void Start()
    {
        if (gameOverUI != null)
        {
            uiGroup = gameOverUI.GetComponent<CanvasGroup>();
            if (uiGroup == null)
                uiGroup = gameOverUI.AddComponent<CanvasGroup>();

            uiGroup.alpha = 0f;
            gameOverUI.SetActive(false);
        }

        if (backgroundOverlay != null)
        {
            Color c = backgroundOverlay.color;
            c.a = 0f;
            backgroundOverlay.color = c;
            backgroundOverlay.gameObject.SetActive(false);
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (backgroundOverlay != null)
            backgroundOverlay.gameObject.SetActive(true);

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            StartCoroutine(FadeInEffect());
        }

        Time.timeScale = 0f;
        StartCoroutine(RestartSceneAfterDelay());
    }

    IEnumerator FadeInEffect()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            if (uiGroup != null)
                uiGroup.alpha = alpha;

            if (backgroundOverlay != null)
            {
                Color c = backgroundOverlay.color;
                c.a = Mathf.Lerp(0f, 0.6f, alpha);
                backgroundOverlay.color = c;
            }

            yield return null;
        }
    }

    IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
