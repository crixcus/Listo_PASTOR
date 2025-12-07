using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum NotificationType
{
    ItemCollected,
    FloodRising,
    CheckPhone,
    UseFlashlight,
    TraumaRising,
    ObjectiveComplete,
    Custom // For custom messages
}

[System.Serializable]
public class NotificationMessage
{
    public NotificationType type;
    public string message;
}

public class NotificationSystem : MonoBehaviour
{
    // Singleton instance for easy access
    public static NotificationSystem Instance { get; private set; }

    [Header("UI")]
    public Animator animator;
    public TextMeshProUGUI notifText;

    [Header("Timing")]
    public float displayTime = 5f;

    [Header("Message Container")]
    [Tooltip("Define your notification messages here. Each situation type maps to a message.")]
    public NotificationMessage[] notificationMessages = new NotificationMessage[]
    {
        new NotificationMessage { type = NotificationType.ItemCollected, message = "Item collected!" },
        new NotificationMessage { type = NotificationType.FloodRising, message = "Flood is Rising!" },
        new NotificationMessage { type = NotificationType.CheckPhone, message = "Check your phone for important stuff" },
        new NotificationMessage { type = NotificationType.UseFlashlight, message = "Use your Flashlight!" },
        new NotificationMessage { type = NotificationType.TraumaRising, message = "Your Trauma is Rising!" },
        new NotificationMessage { type = NotificationType.ObjectiveComplete, message = "Objective complete!" }
    };

    private Dictionary<NotificationType, string> messageDictionary = new Dictionary<NotificationType, string>();
    private Queue<string> notifQueue = new Queue<string>();
    private bool isShowing = false;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Build the message dictionary from the inspector array
        foreach (var notifMsg in notificationMessages)
        {
            if (!messageDictionary.ContainsKey(notifMsg.type))
            {
                messageDictionary[notifMsg.type] = notifMsg.message;
            }
        }
    }

    // ------------------------------
    // PUBLIC CALLS
    // ------------------------------
    
    /// <summary>
    /// Show a notification by situation type (uses predefined messages)
    /// </summary>
    public void ShowNotification(NotificationType notificationType)
    {
        if (messageDictionary.ContainsKey(notificationType))
        {
            ShowNotification(messageDictionary[notificationType]);
        }
        else
        {
            Debug.LogWarning($"Notification type '{notificationType}' not found in message container. Add it in the inspector.");
        }
    }

    /// <summary>
    /// Show a custom notification message (original method for flexibility)
    /// </summary>
    public void ShowNotification(string message)
    {
        notifQueue.Enqueue(message);

        if (!isShowing)
            StartCoroutine(ProcessQueue());
    }

    // ------------------------------
    // STATIC HELPER METHODS FOR TRIGGERS
    // ------------------------------
    
    /// <summary>
    /// Trigger flood rising notification. Call this when flood starts rising or reaches certain heights.
    /// </summary>
    public static void TriggerFloodRising()
    {
        if (Instance != null)
        {
            Instance.ShowNotification(NotificationType.FloodRising);
        }
    }

    /// <summary>
    /// Trigger phone check notification. Call this when player should check their phone (e.g., new message, dark area, etc.)
    /// </summary>
    public static void TriggerCheckPhone()
    {
        if (Instance != null)
        {
            Instance.ShowNotification(NotificationType.CheckPhone);
        }
    }

    /// <summary>
    /// Trigger flashlight notification. Call this when player enters dark areas or should use flashlight.
    /// </summary>
    public static void TriggerUseFlashlight()
    {
        if (Instance != null)
        {
            Instance.ShowNotification(NotificationType.UseFlashlight);
        }
    }

    /// <summary>
    /// Trigger trauma rising notification. Call this when trauma reaches warning thresholds.
    /// </summary>
    public static void TriggerTraumaRising()
    {
        if (Instance != null)
        {
            Instance.ShowNotification(NotificationType.TraumaRising);
        }
    }

    /// <summary>
    /// Trigger objective complete notification. Call this when an objective is completed.
    /// </summary>
    public static void TriggerObjectiveComplete(string objectiveName = "")
    {
        if (Instance != null)
        {
            if (string.IsNullOrEmpty(objectiveName))
            {
                Instance.ShowNotification(NotificationType.ObjectiveComplete);
            }
            else
            {
                Instance.ShowNotification($"Objective '{objectiveName}' complete!");
            }
        }
    }

    // ------------------------------
    // QUEUE PROCESSOR
    // ------------------------------
    private IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (notifQueue.Count > 0)
        {
            string msg = notifQueue.Dequeue();
            notifText.text = msg;

            animator.Play("Notif_Popup", 0, 0f);

            // Wait for popup animation to finish
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            // Hold on screen
            yield return new WaitForSeconds(displayTime);

            // Play leave animation
            animator.Play("Notif_Disappear", 0, 0f);

            // Wait for disappear animation
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        isShowing = false;
    }
}
