using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class TraumaBar : MonoBehaviour
{
    int experience;
    int traumaLimit;

    [Header("Trauma Settings")]
    public Slider traumaBar;
    public GameOverPanel gameOver;
    
    [Header("Notification Thresholds")]
    public float warningThreshold1 = 0.3f; // 30% - First warning
    public float warningThreshold2 = 0.6f; // 60% - Second warning
    public float warningThreshold3 = 0.8f; // 80% - Critical warning
    
    private bool hasWarned30 = false;
    private bool hasWarned60 = false;
    private bool hasWarned80 = false;

    // Use this for initialization
    void Start()
    {
        experience = 0;
        traumaLimit = 500;

        traumaBar.value = experience;
        traumaBar.maxValue = traumaLimit;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            experience += 10;
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
        
        // Check thresholds and trigger notifications (only once per threshold)
        if (traumaPercent >= warningThreshold1 && !hasWarned30)
        {
            hasWarned30 = true;
            NotificationSystem.TriggerTraumaRising();
        }
        else if (traumaPercent >= warningThreshold2 && !hasWarned60)
        {
            hasWarned60 = true;
            NotificationSystem.TriggerTraumaRising();
        }
        else if (traumaPercent >= warningThreshold3 && !hasWarned80)
        {
            hasWarned80 = true;
            NotificationSystem.TriggerTraumaRising();
        }
    }
}