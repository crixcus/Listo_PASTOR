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
    [SerializeField] Animator PopupAnimator;

    [Header("Fade Options")]
    [SerializeField] float timeBeforeFadeOutBegins = 4;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // Ensure the CanvasGroup is initially invisible
        canvasGroup.alpha = 0;
        DisableAllPopUps();
    }

    public void DisplayHealthPopUp(int playerHealth, int currentStatusCategory)
    {

        DisableAllPopUps();

 
        if (currentStatusCategory == 1) // Fine Damaged / Fine Full
        {
            // Decide whether to show FineFull (100) or FineDamaged (66-99)
            if (playerHealth >= 100)
                popUpFineFull.SetActive(true);
            else
                popUpFineDamaged.SetActive(true);
        }
        else if (currentStatusCategory == 2) // Caution
        {
            popUpCaution.SetActive(true);
        }
        else if (currentStatusCategory == 3) // Danger
        {
            popUpDanger.SetActive(true);
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