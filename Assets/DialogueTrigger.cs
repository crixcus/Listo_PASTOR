using UnityEngine;
using TMPro;

public class AutoDialogueTriggerOnce : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea] public string[] dialogueLines; // Dialogue lines
    public float interactRadius = 3f;         // Radius to trigger dialogue
    public float displayDuration = 3f;        // Time the dialogue stays on screen

    [Header("UI Settings")]
    public TextMeshProUGUI dialogueText;      // TMP Text UI
    public GameObject dialoguePanel;          // Panel to show/hide

    private Transform player;
    private bool hasTriggered = false;

    void Start()
    {
        // Automatically find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (player == null || hasTriggered) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= interactRadius)
        {
            // Trigger dialogue once
            hasTriggered = true;

            if (dialoguePanel != null && dialogueText != null && dialogueLines.Length > 0)
            {
                dialogueText.text = string.Join("\n", dialogueLines); // Show all lines at once
                dialoguePanel.SetActive(true);
                Invoke(nameof(HideDialogue), displayDuration); // Hide after 3 seconds
            }

            Debug.Log("Dialogue triggered!");
        }
    }

    void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Optional: remove the script after dialogue ends
        Destroy(this);
    }

    // Draw the radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}