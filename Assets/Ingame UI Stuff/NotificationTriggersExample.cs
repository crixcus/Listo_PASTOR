using UnityEngine;

/// <summary>
/// Example script showing how to trigger notifications for Phone and Flashlight.
/// Copy these patterns into your existing scripts or use this as a reference.
/// </summary>
public class NotificationTriggersExample : MonoBehaviour
{
    [Header("Phone Notification Settings")]
    public bool hasNewPhoneMessage = false;
    public float phoneCheckCooldown = 30f; // Don't spam notifications
    private float lastPhoneNotificationTime = 0f;

    [Header("Flashlight Notification Settings")]
    public bool isInDarkArea = false;
    public bool isFlashlightOn = false;
    public float darknessThreshold = 0.3f; // Adjust based on your lighting system
    private bool hasNotifiedFlashlight = false;

    void Update()
    {
        // Example: Check Phone Notification
        CheckPhoneNotification();
        
        // Example: Flashlight Notification
        CheckFlashlightNotification();
    }

    // ------------------------------
    // PHONE NOTIFICATION TRIGGERS
    // ------------------------------
    
    /// <summary>
    /// Call this when a new message arrives on the phone
    /// </summary>
    public void OnNewPhoneMessage()
    {
        hasNewPhoneMessage = true;
        if (Time.time - lastPhoneNotificationTime >= phoneCheckCooldown)
        {
            NotificationSystem.TriggerCheckPhone();
            lastPhoneNotificationTime = Time.time;
        }
    }

    /// <summary>
    /// Call this when player should check phone (e.g., entering specific area, after certain events)
    /// </summary>
    public void TriggerPhoneCheck()
    {
        if (Time.time - lastPhoneNotificationTime >= phoneCheckCooldown)
        {
            NotificationSystem.TriggerCheckPhone();
            lastPhoneNotificationTime = Time.time;
        }
    }

    private void CheckPhoneNotification()
    {
        // Example: Trigger when player has unread messages
        if (hasNewPhoneMessage && Time.time - lastPhoneNotificationTime >= phoneCheckCooldown)
        {
            NotificationSystem.TriggerCheckPhone();
            lastPhoneNotificationTime = Time.time;
            hasNewPhoneMessage = false; // Reset after notification
        }
    }

    // ------------------------------
    // FLASHLIGHT NOTIFICATION TRIGGERS
    // ------------------------------
    
    /// <summary>
    /// Call this when player enters a dark area
    /// </summary>
    public void OnEnterDarkArea()
    {
        isInDarkArea = true;
        if (!isFlashlightOn && !hasNotifiedFlashlight)
        {
            NotificationSystem.TriggerUseFlashlight();
            hasNotifiedFlashlight = true;
        }
    }

    /// <summary>
    /// Call this when player exits dark area or turns on flashlight
    /// </summary>
    public void OnExitDarkArea()
    {
        isInDarkArea = false;
        hasNotifiedFlashlight = false; // Reset so it can notify again if they enter another dark area
    }

    /// <summary>
    /// Call this when player turns on flashlight
    /// </summary>
    public void OnFlashlightToggled(bool isOn)
    {
        isFlashlightOn = isOn;
        if (isOn)
        {
            hasNotifiedFlashlight = true; // Don't notify if they already turned it on
        }
    }

    private void CheckFlashlightNotification()
    {
        // Example: Check if area is dark and flashlight is off
        // You would integrate this with your lighting system
        // float lightLevel = GetCurrentLightLevel(); // Your lighting check method
        
        // if (lightLevel < darknessThreshold && !isFlashlightOn && !hasNotifiedFlashlight)
        // {
        //     NotificationSystem.TriggerUseFlashlight();
        //     hasNotifiedFlashlight = true;
        // }
    }

    // ------------------------------
    // OBJECTIVE COMPLETE EXAMPLE
    // ------------------------------
    
    /// <summary>
    /// Example: Call this when an objective is completed
    /// </summary>
    public void CompleteObjective(string objectiveName)
    {
        NotificationSystem.TriggerObjectiveComplete(objectiveName);
    }
}

