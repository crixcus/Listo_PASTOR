using UnityEngine;

/// <summary>
/// Detects when the player's HEAD goes below the flood water surface and
/// triggers underwater effects, trauma damage, and movement slow.
///
/// WHY HEAD-BASED DETECTION:
///   Using OnTriggerEnter fires when the player's FEET touch water, which is
///   too early. Instead, this script checks the player's camera (eye level)
///   position against the water surface Y every frame for precise detection.
///
/// WHAT HAPPENS WHEN HEAD IS UNDERWATER:
///   - Trauma rises continuously (traumaPerSecond)
///   - Instant trauma spike on first submersion (entryTraumaSpike)
///   - Player movement slows down (via BasicMovements.SetInWater)
///   - Underwater post-processing activates (via UnderwaterEffects.SetSubmerged)
///   - Notification shown once on entry
///
/// WHAT HAPPENS WHEN HEAD SURFACES:
///   - Trauma stops rising from water
///   - Movement speed restores to normal
///   - Post-processing fades out
///
/// HOW IT FOLLOWS THE RISING WATER:
///   The script reads WaterRiser.transform.position.y every frame and moves
///   this GameObject to match — the BoxCollider always sits at water level.
///   The collider extends far downward (triggerDepthBelow) so the player
///   is always inside it once submerged, regardless of depth.
///
/// DEPENDENCIES:
///   - WaterRiser: provides the current water surface Y position
///   - BasicMovements: receives SetInWater(bool) calls to slow the player
///   - TraumaBar: receives AddTrauma() calls for flood water damage
///   - UnderwaterEffects: receives SetSubmerged(bool) calls for visuals
///   - NotificationSystem: shows entry notification
///
/// SETUP STEPS:
///   1. Create an empty GameObject named "WaterTriggerVolume"
///   2. Attach this script to it
///   3. Assign waterRiser → drag SeaVisual (or object with WaterRiser) in
///   4. Assign playerHead → drag Main Camera in (camera = eye level)
///   5. Assign playerMovement → drag PlayerHolder in
///   6. Set triggerWidth and triggerDepth to cover your entire map
///   7. Position the GameObject at the center of your map (X/Z only — Y is auto)
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class WaterTraumaTrigger : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Inspector Fields
    // ------------------------------------------------------------------

    [Header("References")]

    /// <summary>
    /// The GameObject with the WaterRiser script attached.
    /// Used to read the current water surface Y position every frame.
    /// Drag your SeaVisual object here.
    /// </summary>
    public WaterRiser waterRiser;

    /// <summary>
    /// The player's head/camera Transform.
    /// Submersion is detected when this transform's Y position drops below
    /// the water surface Y. Drag Main Camera here.
    /// </summary>
    public Transform playerHead;

    /// <summary>
    /// The player's BasicMovements component.
    /// Called with SetInWater(true/false) to slow/restore movement speed and FOV.
    /// Drag PlayerHolder here.
    /// </summary>
    public BasicMovements playerMovement;

    [Header("Trigger Collider Size")]

    /// <summary>
    /// Width (X axis) of the invisible trigger volume.
    /// Should be large enough to cover your entire map.
    /// Check your map dimensions and set accordingly.
    /// </summary>
    public float triggerWidth = 200f;

    /// <summary>
    /// Depth (Z axis) of the invisible trigger volume.
    /// Should be large enough to cover your entire map.
    /// </summary>
    public float triggerDepth = 200f;

    /// <summary>
    /// How far the trigger extends BELOW the water surface.
    /// Should be deep enough that the player can never reach the bottom.
    /// Recommended: 50 units.
    /// </summary>
    public float triggerDepthBelow = 50f;

    /// <summary>
    /// How far the trigger extends ABOVE the water surface.
    /// Keep small (0.5) — just enough so the top edge sits at water level.
    /// </summary>
    public float triggerHeightAbove = 0.5f;

    [Header("Trauma Settings")]

    /// <summary>
    /// Trauma added per second while the player's head is submerged.
    /// Stacks with passive trauma rise from TraumaBar.
    /// Recommended: 0.01 - 0.03 (higher = more punishing flood water)
    /// </summary>
    public float traumaPerSecond = 0.02f;

    /// <summary>
    /// Instant trauma added the moment the player's head first goes under.
    /// Creates a sudden impact feeling on submersion.
    /// Recommended: 0.03 - 0.08
    /// </summary>
    public float entryTraumaSpike = 0.05f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private BoxCollider _collider;

    /// <summary>Tracks whether the player's head is currently below water.</summary>
    public bool _headUnderwater = false;

    /// <summary>Current water surface Y position, updated every frame from WaterRiser.</summary>
    private float _waterSurfaceY = 0f;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    /// <summary>
    /// Builds the BoxCollider sized to cover the map and offset downward
    /// so its top edge aligns with the water surface.
    /// The collider exists only to keep physics detection active —
    /// actual submersion logic uses head position, not OnTriggerEnter.
    /// </summary>
    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _collider.isTrigger = true;

        // Total height = depth below surface + small amount above
        float totalHeight = triggerDepthBelow + triggerHeightAbove;
        _collider.size = new Vector3(triggerWidth, totalHeight, triggerDepth);

        // Offset center downward so the TOP of the collider sits at water level
        _collider.center = new Vector3(0f, -(triggerDepthBelow / 2f), 0f);
    }

    /// <summary>
    /// Validates that required references are assigned and logs warnings if not.
    /// </summary>
    private void Start()
    {
        if (playerHead == null)
            Debug.LogWarning("[WaterTraumaTrigger] playerHead not assigned. Drag Main Camera in.");
        if (playerMovement == null)
            Debug.LogWarning("[WaterTraumaTrigger] playerMovement not assigned. Drag PlayerHolder in.");
        if (waterRiser == null)
            Debug.LogWarning("[WaterTraumaTrigger] waterRiser not assigned. Drag SeaVisual in.");
    }

    /// <summary>
    /// Every frame:
    ///   1. Moves this GameObject's Y to match the current water surface
    ///   2. Checks if the player's head has crossed the water surface
    ///   3. Fires OnHeadSubmerged / OnHeadSurfaced on state change
    ///   4. Applies continuous trauma while head is underwater
    /// </summary>
    private void Update()
    {
        // Step 1 — Follow water surface Y position
        if (waterRiser != null)
        {
            _waterSurfaceY = waterRiser.transform.position.y;
            Vector3 pos = transform.position;
            pos.y = _waterSurfaceY;
            transform.position = pos;
        }

        if (playerHead == null) return;

        // Step 2 — Check if head crossed the water surface this frame
        bool headIsUnder = playerHead.position.y < _waterSurfaceY;

        if (headIsUnder && !_headUnderwater)
        {
            // Head just went below water surface
            _headUnderwater = true;
            OnHeadSubmerged();
        }
        else if (!headIsUnder && _headUnderwater)
        {
            // Head just came back above water surface
            _headUnderwater = false;
            OnHeadSurfaced();
        }

        // Step 3 — Continuous trauma drain while submerged
        if (_headUnderwater)
            TraumaBar.Instance?.AddTrauma(traumaPerSecond * Time.deltaTime);
    }

    // ------------------------------------------------------------------
    // Private — submersion events
    // ------------------------------------------------------------------

    /// <summary>
    /// Called once when the player's head first goes below the water surface.
    /// Triggers all underwater systems: movement slow, trauma spike,
    /// post-processing effects, and entry notification.
    /// </summary>
    private void OnHeadSubmerged()
    {
        playerMovement?.SetInWater(true);
        TraumaBar.Instance?.AddTrauma(entryTraumaSpike);
        NotificationSystem.Instance?.ShowNotification("You're in the flood water!");
        UnderwaterEffects.Instance?.SetSubmerged(true);
    }

    /// <summary>
    /// Called once when the player's head comes back above the water surface.
    /// Restores movement speed and FOV, and fades out post-processing effects.
    /// </summary>
    private void OnHeadSurfaced()
    {
        playerMovement?.SetInWater(false);
        UnderwaterEffects.Instance?.SetSubmerged(false);
    }
}