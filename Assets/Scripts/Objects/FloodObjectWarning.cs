using UnityEngine;
using TMPro;
using System.Collections;

public class FloodObjectWarning : MonoBehaviour
{
    public TMP_Text warningText;      // Assign your TMP Text in the inspector
    public float fadeSpeed = 2f;      // How fast it fades in/out
    public float displayDuration = 5f; // How long the message stays fully visible
    public string startMessage = "Pick up the forgotten objects on the 1st floor and move them to the 2nd floor!";

    private float targetAlpha = 0f;

    void Start()
    {
        if (warningText == null)
        {
            Debug.LogWarning("WarningText is not assigned!");
            return;
        }

        // Start fully transparent
        Color color = warningText.color;
        color.a = 0f;
        warningText.color = color;
        warningText.gameObject.SetActive(true);

        // Start the automatic display coroutine
        StartCoroutine(DisplayOnStart());
    }

    void Update()
    {
        if (warningText == null) return;

        // Smoothly fade the alpha toward the target
        Color color = warningText.color;
        color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
        warningText.color = color;
    }

    private IEnumerator DisplayOnStart()
    {
        // Set message and fade in
        warningText.text = startMessage;
        targetAlpha = 1f;

        // Wait for the display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        targetAlpha = 0f;
    }
}
