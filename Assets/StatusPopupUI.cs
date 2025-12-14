using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusPopupUI : MonoBehaviour
{
    CanvasGroup canvasGroup;
    // Store the running coroutine reference
    private Coroutine currentFadeCoroutine = null; // <-- ADDED FIELD

    [SerializeField] GameObject popUpFineFull;
    [SerializeField] GameObject popUpFineDamaged;
    [SerializeField] GameObject popUpCaution;
    [SerializeField] GameObject popUpDanger;

    [Header("Fade Options")]
    [SerializeField] float timeBeforeFadeOutBegins = 2;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // Ensure the CanvasGroup is initially invisible
        canvasGroup.alpha = 0;
        DisableAllPopUps();
    }

    public void DisplayHealthPopUp(int playerHealth)
    {
        // 1. Stop any current fade sequence
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // 2. Reset the alpha and disable ALL popups before setting the new one
        canvasGroup.alpha = 0;
        DisableAllPopUps();

        // 3. Set the appropriate popup active based on the 'health equivalent' value
        if (playerHealth >= 100)
        {
            popUpFineFull.SetActive(true);
        }
        else if (playerHealth >= 66 && playerHealth <= 99)
        {
            popUpFineDamaged.SetActive(true);
        }
        else if (playerHealth >= 30 && playerHealth <= 65)
        {
            popUpCaution.SetActive(true);
        }
        else if (playerHealth >= 1 && playerHealth <= 29)
        {
            popUpDanger.SetActive(true);
        }
        else // Handle 0 health/100% trauma case
        {
            // Optionally handle death/full trauma state here if needed
        }

        // 4. Start the new fade sequence and store the reference
        currentFadeCoroutine = StartCoroutine(FadeInPopUp());
    }

    private void DisableAllPopUps()
    {
        popUpFineFull.SetActive(false);
        popUpFineDamaged.SetActive(false);
        popUpCaution.SetActive(false);
        popUpDanger.SetActive(false);
    }

    IEnumerator FadeInPopUp()
    {
        // Smoothly fade in the CanvasGroup alpha
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime / 0.3f; // Faster fade in
            yield return null;
        }
        canvasGroup.alpha = 1f; // Ensure it ends at 1

        Debug.Log("PopUp fully faded in. Waiting...");

        // After fully fading in, immediately start the fade out sequence
        currentFadeCoroutine = StartCoroutine(FadeOutPopUp());
    }

    IEnumerator FadeOutPopUp()
    {
        // Wait for the specified time
        yield return new WaitForSeconds(timeBeforeFadeOutBegins);

        // Smoothly fade out the CanvasGroup alpha
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime / 1f; // Slower fade out
            yield return null;
        }

        // Ensure it ends at 0 and disable the UI elements
        canvasGroup.alpha = 0f;
        DisableAllPopUps();

        Debug.Log("Fading Out PopUp complete.");
        currentFadeCoroutine = null; // Mark coroutine as finished
    }
}