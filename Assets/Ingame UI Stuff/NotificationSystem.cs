using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Defines the types of notifications available in the game.
/// Add new entries here and register them in the Inspector's
/// notificationMessages array on the NotificationSystem component.
/// </summary>
public enum NotificationType
{
    ItemCollected,
    FloodRising,
    CheckPhone,
    UseFlashlight,
    TraumaRising,
    ObjectiveComplete
}

/// <summary>
/// Maps a NotificationType to its display string.
/// Configured via the Inspector on the NotificationSystem component.
/// </summary>
[System.Serializable]
public class NotificationMessage
{
    public NotificationType type;
    [TextArea] public string message;
}

/// <summary>
/// Manages the in-game notification UI. Displays queued messages one at a time
/// using Animator-driven popup and disappear animations.
///
/// Usage:
///   - Place this component on a persistent UI GameObject (e.g. a Canvas child).
///   - Assign the Animator and TextMeshProUGUI references in the Inspector.
///   - Call NotificationSystem.Instance.ShowNotification(...) or any static Trigger* helper.
///
/// The Animator must have two states:
///   "Notif_Popup"     — plays when a notification appears.
///   "Notif_Disappear" — plays when a notification leaves.
/// </summary>
public class NotificationSystem : MonoBehaviour
{
    /// <summary>Global singleton. Assigned on Awake; null if no instance exists in the scene.</summary>
    public static NotificationSystem Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Animator controlling popup and disappear animations.")]
    public Animator animator;

    [Tooltip("Text component that displays the notification message.")]
    public TextMeshProUGUI notifText;

    [Header("Timing")]
    [Tooltip("Seconds the notification stays fully visible before dismissing.")]
    public float displayTime = 5f;

    [Header("Queue Settings")]
    [Tooltip("Maximum number of pending notifications. Older messages are dropped when full.")]
    public int maxQueueSize = 5;

    [Header("Notification Messages")]
    [Tooltip("Maps each NotificationType to a display message. Editable in the Inspector.")]
    public NotificationMessage[] notificationMessages = new NotificationMessage[]
    {
        new NotificationMessage { type = NotificationType.ItemCollected,    message = "Item collected!" },
        new NotificationMessage { type = NotificationType.FloodRising,      message = "Flood is Rising!" },
        new NotificationMessage { type = NotificationType.CheckPhone,       message = "Check your phone for important updates." },
        new NotificationMessage { type = NotificationType.UseFlashlight,    message = "Use your Flashlight!" },
        new NotificationMessage { type = NotificationType.TraumaRising,     message = "Your Trauma is Rising!" },
        new NotificationMessage { type = NotificationType.ObjectiveComplete, message = "Objective complete!" }
    };

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private readonly Dictionary<NotificationType, string> _messageLookup = new();
    private readonly Queue<string> _queue = new();
    private bool _isShowing;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    /// <summary>
    /// Initialises the singleton and builds the internal message lookup table.
    /// Disables the component and logs an error if required references are missing.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!ValidateReferences()) return;

        foreach (var entry in notificationMessages)
        {
            _messageLookup.TryAdd(entry.type, entry.message);
        }
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Enqueues a notification using a predefined <see cref="NotificationType"/>.
    /// The message text is resolved from the Inspector configuration.
    /// </summary>
    /// <param name="type">The notification type to display.</param>
    public void ShowNotification(NotificationType type)
    {
        if (_messageLookup.TryGetValue(type, out string message))
        {
            Enqueue(message);
        }
        else
        {
            Debug.LogWarning($"[NotificationSystem] No message registered for type '{type}'. " +
                             "Add it to the notificationMessages array in the Inspector.");
        }
    }

    /// <summary>
    /// Enqueues a notification with an arbitrary string message.
    /// Use this for dynamic content such as item names or objective titles.
    /// </summary>
    /// <param name="message">The text to display in the notification.</param>
    public void ShowNotification(string message)
    {
        Enqueue(message);
    }

    // ------------------------------------------------------------------
    // Static trigger helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Triggers a "Flood Rising" notification.
    /// Call when the flood level increases or crosses a threshold.
    /// </summary>
    public static void TriggerFloodRising() =>
        Instance?.ShowNotification(NotificationType.FloodRising);

    /// <summary>
    /// Triggers a "Check Phone" notification.
    /// Call when a new in-game message arrives or the player enters a relevant area.
    /// </summary>
    public static void TriggerCheckPhone() =>
        Instance?.ShowNotification(NotificationType.CheckPhone);

    /// <summary>
    /// Triggers a "Use Flashlight" notification.
    /// Call when the player enters a dark area that requires the flashlight.
    /// </summary>
    public static void TriggerUseFlashlight() =>
        Instance?.ShowNotification(NotificationType.UseFlashlight);

    /// <summary>
    /// Triggers a "Trauma Rising" notification.
    /// Call when the player's trauma stat crosses a warning threshold.
    /// </summary>
    public static void TriggerTraumaRising() =>
        Instance?.ShowNotification(NotificationType.TraumaRising);

    /// <summary>
    /// Triggers an "Objective Complete" notification.
    /// If <paramref name="objectiveName"/> is provided, it is included in the message text.
    /// </summary>
    /// <param name="objectiveName">Optional name of the completed objective.</param>
    public static void TriggerObjectiveComplete(string objectiveName = "")
    {
        if (Instance == null) return;

        string message = string.IsNullOrEmpty(objectiveName)
            ? null
            : $"Objective '{objectiveName}' complete!";

        if (message != null)
            Instance.ShowNotification(message);
        else
            Instance.ShowNotification(NotificationType.ObjectiveComplete);
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Adds a message to the notification queue and starts processing if idle.
    /// Drops the message silently if the queue has reached <see cref="maxQueueSize"/>.
    /// </summary>
    /// <param name="message">The message string to enqueue.</param>
    private void Enqueue(string message)
    {
        if (_queue.Count >= maxQueueSize)
        {
            Debug.LogWarning("[NotificationSystem] Queue full — notification dropped.");
            return;
        }

        _queue.Enqueue(message);

        if (!_isShowing)
            StartCoroutine(ProcessQueue());
    }

    /// <summary>
    /// Validates that required component references are assigned.
    /// Disables the component and logs an error if any reference is missing.
    /// </summary>
    /// <returns><c>true</c> if all references are valid; otherwise <c>false</c>.</returns>
    private bool ValidateReferences()
    {
        if (animator != null && notifText != null) return true;

        Debug.LogError("[NotificationSystem] Missing required references — " +
                       "assign Animator and TextMeshProUGUI in the Inspector.");
        enabled = false;
        return false;
    }

    /// <summary>
    /// Coroutine that dequeues and displays notifications one at a time.
    /// Waits one frame after calling <c>Animator.Play</c> before reading the
    /// clip length, ensuring the animator has transitioned to the new state.
    /// </summary>
    private IEnumerator ProcessQueue()
    {
        _isShowing = true;

        while (_queue.Count > 0)
        {
            notifText.text = _queue.Dequeue();

            // Play popup animation — wait one frame so the animator transitions
            animator.Play("Notif_Popup", 0, 0f);
            yield return null;
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            // Hold on screen
            yield return new WaitForSeconds(displayTime);

            // Play disappear animation — same one-frame wait
            animator.Play("Notif_Disappear", 0, 0f);
            yield return null;
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        _isShowing = false;
    }
}