using UnityEngine;

/// <summary>
/// Attach to PlayerHolder alongside PlayerInteraction.
/// Handles raycasting for both the lighter pickup and candle lighting.
///
/// Priority order each frame:
///   1. If player doesn't have lighter — look for lighter to pick up
///   2. If player has lighter — look for unlit candles to light
///
/// Setup:
///   - Attach to PlayerHolder
///   - Assign playerCamera
///   - Set interactRange (should match PlayerInteraction.playerReach)
///   - Make sure lighter is tagged "Interactable"
///   - Make sure candles are tagged "Candle" (create this tag)
/// </summary>
public class CandleInteraction : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's camera transform.")]
    public Transform playerCamera;

    [Header("Settings")]
    [Tooltip("Max raycast distance for interaction.")]
    public float interactRange = 3f;

    [Header("Layer Masks")]
    public LayerMask interactableLayer;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Update()
    {
        if (!Lighter.PlayerHasLighter)
            CheckForLighter();
        else
            CheckForCandle();
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    /// <summary>
    /// Raycasts for a Lighter object. Shows pickup prompt and handles pickup.
    /// </summary>
    private void CheckForLighter()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            Lighter lighter = hit.collider.GetComponent<Lighter>();

            if (lighter != null)
            {

                if (Input.GetKeyDown(KeyCode.F))
                    lighter.PickUp();

                return;
            }
        }
    }

    /// <summary>
    /// Raycasts for an unlit Candle. Shows light prompt and handles lighting.
    /// </summary>
    private void CheckForCandle()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            Candle candle = hit.collider.GetComponent<Candle>();

            if (candle != null && !candle.IsLit)
            {
                HUDController.instance?.EnableInteractionText("Light candle (F)");

                if (Input.GetKeyDown(KeyCode.F))
                    candle.Light();

                return;
            }

            // Already lit candle — show status
            if (candle != null && candle.IsLit)
            {
                HUDController.instance?.EnableInteractionText("Candle is lit");
                return;
            }
        }
    }
}