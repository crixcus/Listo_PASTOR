using System.Collections;
using TMPro;
using UnityEngine;

public class FloodWarning : MonoBehaviour
{
    public TMP_Text warningText; // Assign your TMP Text in inspector
    public float displayTime = 3f; // Total time to show the message
    public float fadeSpeed = 1f; // Speed of fade in/out

    void Start()
    {
        StartCoroutine(ShowStartMessage());
    }

    IEnumerator ShowStartMessage()
    {
        yield return StartCoroutine(FadeBlinkText("Warning: Flood waters are rising!", displayTime));
    }

    // Call this whenever the flood is rising
    public void ShowRisingFloodWarning()
    {
        StartCoroutine(FadeBlinkText("Flood is rising! Move to higher ground!", 3f));
    }

    IEnumerator FadeBlinkText(string message, float duration)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Fade in
            for (float t = 0; t < 1f; t += Time.deltaTime * fadeSpeed)
            {
                warningText.alpha = t;
                yield return null;
            }

            // Fade out
            for (float t = 1f; t > 0f; t -= Time.deltaTime * fadeSpeed)
            {
                warningText.alpha = t;
                yield return null;
            }

            elapsed += 2f / fadeSpeed; // Rough estimate of time spent fading in and out
        }

        warningText.alpha = 0f;
        warningText.gameObject.SetActive(false);
    }
}
