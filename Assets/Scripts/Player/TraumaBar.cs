using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class TraumaBar : MonoBehaviour
{

    float experience;
    int traumaLimit;

    [Header("Trauma Settings")]
    public Slider traumaBar;
    public GameOverPanel gameOver;

    [Header ("UI PopUps")]
    [SerializeField] StatusPopupUI statusPopupUI;

    [Header("Notification Thresholds")]
    public float warningThreshold1 = 0.3f; // 30% - First warning
    public float warningThreshold2 = 0.6f; // 60% - Second warning
    public float warningThreshold3 = 0.8f; // 80% - Critical warning
    
    private bool hasWarned30 = false;
    private bool hasWarned60 = false;
    private bool hasWarned80 = false;
    private int lastDisplayedStatus = 0; // 1=FineDamaged, 2=Caution, 3=Danger

    // Use this for initialization
    void Start()
    {
        experience = 0;
        traumaLimit = 1;

        traumaBar.value = experience;
        traumaBar.maxValue = traumaLimit;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            experience += 0.10f;
            traumaBar.value = experience;
            CheckTraumaThresholds();
        }

        if (experience >= 500)
        {
            gameOver.TriggerGameOver();
        }
    }
    
    /// <summary>
    /// Call this method whenever trauma increases to check for notification thresholds
    /// </summary>
    public void AddTrauma(int amount)
    {
        experience += amount;
        traumaBar.value = experience;
        CheckTraumaThresholds();
    }

    private void CheckTraumaThresholds()
    {
        float traumaPercent = (float)experience / traumaLimit;

        // Scale trauma percent (0.0 to 1.0) to StatusPopupUI's health scale (0 to 100)
        // NOTE: Since the Trauma bar represents "bad" health, we invert the value
        // Max Trauma (100% full) is the lowest "health" (0)
        // Min Trauma (0% full) is the highest "health" (100)
        int healthEquivalent = Mathf.RoundToInt((1f - traumaPercent) * 100);

        // Check if the status has changed enough to trigger a new popup
        int currentStatus;
        if (healthEquivalent >= 66)
        {
            currentStatus = 1; // Fine Damaged / Fine Full (66-100)
        }
        else if (healthEquivalent >= 30)
        {
            currentStatus = 2; // Caution (30-65)
        }
        else // healthEquivalent >= 1 && healthEquivalent <= 29
        {
            currentStatus = 3; // Danger (1-29)
        }

        // Only call the display function if the status category has changed
        // This prevents the popup from constantly trying to fade in while the trauma is stable
        if (currentStatus != lastDisplayedStatus)
        {
            // Call the display function in the StatusPopupUI script
            if (statusPopupUI != null)
            {
                statusPopupUI.DisplayHealthPopUp(healthEquivalent);
            }

            lastDisplayedStatus = currentStatus;
        }

        // --- YOUR ORIGINAL THRESHOLD WARNING LOGIC (can be kept or adapted) ---
        // This logic is separate and can still be used for other one-time events (like sound effects)
        if (traumaPercent >= warningThreshold1 && !hasWarned30)
        {
            hasWarned30 = true;
            // NotificationSystem.TriggerTraumaRising(); // Assuming this is defined elsewhere
        }
        else if (traumaPercent >= warningThreshold2 && !hasWarned60)
        {
            hasWarned60 = true;
            // NotificationSystem.TriggerTraumaRising();
        }
        else if (traumaPercent >= warningThreshold3 && !hasWarned80)
        {
            hasWarned80 = true;
            // NotificationSystem.TriggerTraumaRising();
        }
        // ------------------------------------------------------------------------
    }
}