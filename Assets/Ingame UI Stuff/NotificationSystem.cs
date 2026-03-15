using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum NotificationType
{
    ItemCollected,
    FloodRising,
    CheckPhone,
    UseFlashlight,
    TraumaRising,
    ObjectiveComplete
}

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
/// Two modes:
///   ShowNotification()  — standard queue for important one-time events
///   ShowDebounced()     — for rapid updates like cleaning/placement progress.
///                         Rapid calls on the same channel replace each other
///                         instead of stacking, then show after a short delay.
/// </summary>
public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }

    [Header("UI References")]
    public Animator animator;
    public TextMeshProUGUI notifText;

    [Header("Timing")]
    [Tooltip("Seconds the notification stays fully visible before dismissing.")]
    public float displayTime = 3f;

    [Header("Queue Settings")]
    [Tooltip("Maximum pending notifications before dropping new ones.")]
    public int maxQueueSize = 5;

    [Header("Notification Messages")]
    public NotificationMessage[] notificationMessages = new NotificationMessage[]
    {
        new NotificationMessage { type = NotificationType.ItemCollected,     message = "Item collected!" },
        new NotificationMessage { type = NotificationType.FloodRising,       message = "Flood is Rising!" },
        new NotificationMessage { type = NotificationType.CheckPhone,        message = "Check your phone for important updates." },
        new NotificationMessage { type = NotificationType.UseFlashlight,     message = "Use your Flashlight!" },
        new NotificationMessage { type = NotificationType.TraumaRising,      message = "Your Trauma is Rising!" },
        new NotificationMessage { type = NotificationType.ObjectiveComplete, message = "Objective complete!" }
    };

    private readonly Dictionary<NotificationType, string> _messageLookup = new();
    private readonly Queue<string> _queue = new();
    private readonly Dictionary<string, string> _debouncePending = new();
    private readonly Dictionary<string, Coroutine> _debounceCoroutines = new();
    private bool _isShowing;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!ValidateReferences()) return;

        foreach (var entry in notificationMessages)
            _messageLookup.TryAdd(entry.type, entry.message);
    }

    // ------------------------------------------------------------------
    // Public API — standard queue
    // ------------------------------------------------------------------

    /// <summary>
    /// Enqueues a notification by type. Use for important one-time events.
    /// </summary>
    public void ShowNotification(NotificationType type)
    {
        if (_messageLookup.TryGetValue(type, out string message))
            Enqueue(message);
        else
            Debug.LogWarning($"[NotificationSystem] No message for type '{type}'.");
    }

    /// <summary>
    /// Enqueues a notification with a custom string. Use for one-time events.
    /// </summary>
    public void ShowNotification(string message) => Enqueue(message);

    // ------------------------------------------------------------------
    // Public API — debounced (for rapid updates)
    // ------------------------------------------------------------------

    /// <summary>
    /// Shows a debounced notification. Rapid calls on the same channelKey
    /// update the pending message rather than stacking in the queue.
    /// The notification only fires after <paramref name="delay"/> seconds of silence.
    ///
    /// Use this for cleaning progress, item placement, any frequently updating value.
    ///
    /// Example:
    ///   NotificationSystem.Instance.ShowDebounced("cleaning", $"Cleaning: {pct:0}%", 1f);
    ///   NotificationSystem.Instance.ShowDebounced("placement", $"Restored {n}/{total}", 0.5f);
    /// </summary>
    /// <param name="channelKey">Unique string identifying this notification channel.</param>
    /// <param name="message">Message to display when the debounce settles.</param>
    /// <param name="delay">Seconds to wait after the last call before showing. Default 1s.</param>
    public void ShowDebounced(string channelKey, string message, float delay = 1f)
    {
        _debouncePending[channelKey] = message;

        if (_debounceCoroutines.TryGetValue(channelKey, out Coroutine existing) && existing != null)
            StopCoroutine(existing);

        _debounceCoroutines[channelKey] = StartCoroutine(DebounceTimer(channelKey, delay));
    }

    // ------------------------------------------------------------------
    // Static trigger helpers
    // ------------------------------------------------------------------

    /// <summary>Triggers a Flood Rising notification.</summary>
    public static void TriggerFloodRising() =>
        Instance?.ShowNotification(NotificationType.FloodRising);

    /// <summary>Triggers a Check Phone notification.</summary>
    public static void TriggerCheckPhone() =>
        Instance?.ShowNotification(NotificationType.CheckPhone);

    /// <summary>Triggers a Use Flashlight notification.</summary>
    public static void TriggerUseFlashlight() =>
        Instance?.ShowNotification(NotificationType.UseFlashlight);

    /// <summary>Triggers a Trauma Rising notification.</summary>
    public static void TriggerTraumaRising() =>
        Instance?.ShowNotification(NotificationType.TraumaRising);

    /// <summary>Triggers an Objective Complete notification with optional name.</summary>
    public static void TriggerObjectiveComplete(string objectiveName = "")
    {
        if (Instance == null) return;
        if (!string.IsNullOrEmpty(objectiveName))
            Instance.ShowNotification($"Objective '{objectiveName}' complete!");
        else
            Instance.ShowNotification(NotificationType.ObjectiveComplete);
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    private IEnumerator DebounceTimer(string channelKey, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_debouncePending.TryGetValue(channelKey, out string message))
        {
            Enqueue(message);
            _debouncePending.Remove(channelKey);
        }

        _debounceCoroutines.Remove(channelKey);
    }

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

    private bool ValidateReferences()
    {
        if (animator != null && notifText != null) return true;
        Debug.LogError("[NotificationSystem] Missing Animator or TextMeshProUGUI reference.");
        enabled = false;
        return false;
    }

    private IEnumerator ProcessQueue()
    {
        _isShowing = true;

        while (_queue.Count > 0)
        {
            notifText.text = _queue.Dequeue();

            animator.Play("Notif_Popup", 0, 0f);
            yield return null;
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            yield return new WaitForSeconds(displayTime);

            animator.Play("Notif_Disappear", 0, 0f);
            yield return null;
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        _isShowing = false;
    }
}