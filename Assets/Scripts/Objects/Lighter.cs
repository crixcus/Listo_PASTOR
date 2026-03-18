using UnityEngine;

/// <summary>
/// Attach to the lighter 3D model in the scene.
/// Wire the existing Interactable.OnInteraction() UnityEvent to Lighter.PickUp().
///
/// Setup:
///   1. Attach to the lighter GameObject
///   2. Keep existing Interactable component
///   3. In Interactable OnInteraction() → add Lighter.PickUp()
///   4. Optionally assign playerHandVisual for a held lighter visual
/// </summary>
public class Lighter : MonoBehaviour
{
    /// <summary>Whether the player currently has the lighter.</summary>
    public static bool PlayerHasLighter { get; private set; }

    [Tooltip("Optional: lighter visual parented to the camera to show when equipped.")]
    public GameObject playerHandVisual;

    /// <summary>
    /// Called via Interactable.OnInteraction() UnityEvent when player presses F.
    /// </summary>
    public void PickUp()
    {
        if (PlayerHasLighter) return;

        PlayerHasLighter = true;

        if (playerHandVisual != null)
            playerHandVisual.SetActive(true);

        NotificationSystem.Instance?.ShowNotification("You picked up a lighter.");

        // Disable the world object
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Resets lighter state when scene unloads.
    /// </summary>
    private void OnDestroy()
    {
        PlayerHasLighter = false;
    }
}