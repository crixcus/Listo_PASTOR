using UnityEngine;

public class UnderwaterBlur : MonoBehaviour
{
    [Header("Water Settings")]
    [Tooltip("The water surface object to detect against")]
    public Transform waterObject;
    [Tooltip("Offset above the water surface to trigger effects")]
    public float waterSurfaceOffset = 0.1f;

    [Header("Underwater Volume (Blur + Fog)")]
    [Tooltip("GameObject containing underwater visual effects")]
    public GameObject underwaterVolume;

    private bool isUnderwater = false;

    void Update()
    {
        // Only check if references are valid
        if (waterObject != null && underwaterVolume != null)
        {
            CheckUnderwater();
        }
        else
        {
            Debug.LogWarning("UnderwaterEffects: Missing required references in Inspector!");
        }
    }

    void CheckUnderwater()
    {
        float camY = transform.position.y;
        float waterY = waterObject.position.y + waterSurfaceOffset;

        if (camY < waterY)
        {
            if (!isUnderwater)
                EnterUnderwater();
        }
        else
        {
            if (isUnderwater)
                ExitUnderwater();
        }
    }

    void EnterUnderwater()
    {
        isUnderwater = true;
        underwaterVolume.SetActive(true);
        Debug.Log("Entered underwater - effects enabled");
    }

    void ExitUnderwater()
    {
        isUnderwater = false;
        underwaterVolume.SetActive(false);
        Debug.Log("Exited underwater - effects disabled");
    }
}