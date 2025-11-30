using UnityEngine;
using TMPro;

public class LookDownTutorial : MonoBehaviour
{
    public Transform playerCamera; // Assign your camera here
    public TMP_Text tutorialText;  // Assign your TMP Text here
    public float lookDownThreshold = 0.7f; // How much the player needs to look down (0-1)
    public float fadeSpeed = 2f; // Speed of fade in/out

    private float targetAlpha = 0f; // Target alpha value

    void Update()
    {
        // Check if player is looking down
        float dot = Vector3.Dot(playerCamera.forward, Vector3.down);

        if (dot > lookDownThreshold)
        {
            targetAlpha = 1f; // Fade in
        }
        else
        {
            targetAlpha = 0f; // Fade out
        }

        // Smoothly fade the alpha
        Color color = tutorialText.color;
        color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
        tutorialText.color = color;
    }
}
