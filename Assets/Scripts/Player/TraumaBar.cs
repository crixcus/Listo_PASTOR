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

    [Header("UI PopUps")]
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
            experience = Mathf.Clamp(experience, 0f, traumaLimit); // Added Clamp for safety
            traumaBar.value = experience;
            CheckTraumaThresholds();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            experience -= 0.10f;
            experience = Mathf.Clamp(experience, 0f, traumaLimit); // Added Clamp for safety
            traumaBar.value = experience;
            CheckTraumaThresholds();
        }
        if (experience >= 1)
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
        experience = Mathf.Clamp(experience, 0f, traumaLimit); // Added Clamp for safety
        traumaBar.value = experience;
        CheckTraumaThresholds();
    }

    private void CheckTraumaThresholds()
    {
        float traumaPercent = experience / traumaLimit;
        int healthEquivalent = Mathf.RoundToInt((1f - traumaPercent) * 100);

        int currentStatus;

        if (healthEquivalent >= 66)
        {
            currentStatus = 1; // Fine Damaged / Fine Full (66-100)
            // Note: PopupAnimator is linked to TraumaBar's logic, not StatusPopupUI
            Debug.Log(currentStatus);
        }
        else if (healthEquivalent >= 30)
        {
            currentStatus = 2; // Caution (30-65)
            Debug.Log(currentStatus);
        }
        else // healthEquivalent >= 1 && healthEquivalent <= 29
        {
            currentStatus = 3; // Danger (1-29)
            Debug.Log(currentStatus);
        }

        // CORRECT BLOCK: Calls the DisplayHealthPopUp with two arguments ONLY when the status changes
        if (currentStatus != lastDisplayedStatus)
        {
            if (statusPopupUI != null)
            {
                // Passing BOTH healthEquivalent (1st arg) and currentStatus (2nd arg)
                statusPopupUI.DisplayHealthPopUp(healthEquivalent, currentStatus);
            }

            lastDisplayedStatus = currentStatus;
        }

        // --- YOUR ORIGINAL THRESHOLD WARNING LOGIC (can be kept or adapted) ---
        if (traumaPercent >= warningThreshold1 && !hasWarned30)
        {
            hasWarned30 = true;
        }
        else if (traumaPercent >= warningThreshold2 && !hasWarned60)
        {
            hasWarned60 = true;
        }
        else if (traumaPercent >= warningThreshold3 && !hasWarned80)
        {
            hasWarned80 = true;
        }
    }
}