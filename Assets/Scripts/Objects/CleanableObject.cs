using UnityEngine;
using System.Collections.Generic;

public class CleanableObject : MonoBehaviour
{
    // ======================
    // STATIC CONTAINER
    // ======================
    public static List<CleanableObject> AllDirtObjects = new List<CleanableObject>();
    private static int cleanedCount = 0;

    [Range(0.3f, 1f)]
    public float dirtAmount = 1f; // 1 = fully dirty, 0 = fully clean

    private SpriteRenderer spriteRenderer;
    private bool hasReportedClean = false;

    public bool IsClean => dirtAmount <= 0f;

    // ======================
    // LIFECYCLE
    // ======================
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisuals();

        // Register this dirt object
        if (!AllDirtObjects.Contains(this))
        {
            AllDirtObjects.Add(this);
        }
    }

    void OnDestroy()
    {
        // Safety cleanup
        if (AllDirtObjects.Contains(this))
        {
            AllDirtObjects.Remove(this);
        }
    }

    // ======================
    // CLEANING LOGIC
    // ======================
    public void Clean(float amountPerSecond)
    {
        if (IsClean) return;

        dirtAmount -= amountPerSecond * Time.deltaTime;
        dirtAmount = Mathf.Clamp01(dirtAmount);

        UpdateVisuals();

        if (IsClean && !hasReportedClean)
        {
            hasReportedClean = true;
            OnCleaned();
        }
    }

    // ======================
    // VISUALS
    // ======================
    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        Color c = spriteRenderer.color;
        c.a = dirtAmount; // alpha = dirt amount
        spriteRenderer.color = c;
    }

    // ======================
    // CLEAN COMPLETE
    // ======================
    private void OnCleaned()
    {
        cleanedCount++;

        float percentCleaned = GetCleanPercentage();

        Debug.Log($"{gameObject.name} cleaned. Total progress: {percentCleaned}%");

        if (NotificationSystem.Instance != null)
        {
            NotificationSystem.Instance.ShowNotification(
                $"Cleaning progress: {percentCleaned:0}%"
            );
        }
    }

    // ======================
    // STATIC HELPERS
    // ======================
    public static float GetCleanPercentage()
    {
        if (AllDirtObjects.Count == 0) return 100f;

        return (cleanedCount / (float)AllDirtObjects.Count) * 100f;
    }
}
