using UnityEngine;

/// <summary>
/// Manages the object restoration mechanic in Level 3.
/// Attach to PlayerHolder alongside PlayerInteraction.
///
/// Setup:
///   - Assign playerCamera and holdPoint in the Inspector.
///   - holdPoint: empty child Transform in front of the camera (Z = 1.5).
///   - Assign placeableLayer to the Placeable layer.
///   - No Ghost layer needed — ghosts are detected directly via PlaceableObject.IsAimingAtGhost().
/// </summary>
public class PlacementSystem : MonoBehaviour
{
    public static PlacementSystem Instance { get; private set; }

    [Header("References")]
    [Tooltip("The player's camera transform.")]
    public Transform playerCamera;

    [Tooltip("Empty transform in front of the camera where the held object floats.")]
    public Transform holdPoint;

    [Header("Interaction Settings")]
    [Tooltip("Max distance to pick up a placeable object.")]
    public float pickupRange = 3f;

    [Tooltip("How smoothly the held object follows the hold point.")]
    public float holdSmoothing = 12f;

    [Header("Layer Masks")]
    [Tooltip("Layer that PlaceableObjects are on.")]
    public LayerMask placeableLayer;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private PlaceableObject _carriedObject;
    private bool _aimingAtGhost;
    private int _totalPlaceables;
    private int _placedCount;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        _totalPlaceables = FindObjectsByType<PlaceableObject>(FindObjectsSortMode.None).Length;
    }

    private void Update()
    {
        if (_carriedObject != null)
        {
            MoveHeldObject();
            CheckForGhost();

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E))
            {
                if (_aimingAtGhost)
                    PlaceObject();
                else
                    DropObject();
            }
        }
        else
        {
            CheckForPickup();
        }
    }

    // ------------------------------------------------------------------
    // Public callbacks
    // ------------------------------------------------------------------

    /// <summary>Called by PlaceableObject when picked up.</summary>
    public void OnObjectPickedUp(PlaceableObject obj)
    {
        HUDController.instance?.EnableInteractionText("Drop");
    }

    /// <summary>Called by PlaceableObject when dropped without placing.</summary>
    public void OnObjectDropped()
    {
        HUDController.instance?.DisableInteractionText();
    }

    /// <summary>Called by PlaceableObject when successfully placed.</summary>
    public void OnObjectPlaced(PlaceableObject obj)
    {
        _placedCount++;

        NotificationSystem.Instance?.ShowNotification(
            $"Item restored! ({_placedCount}/{_totalPlaceables})");

        HUDController.instance?.DisableInteractionText();

        if (_placedCount >= _totalPlaceables)
            OnAllObjectsPlaced();
    }

    // ------------------------------------------------------------------
    // Private
    // ------------------------------------------------------------------

    /// <summary>
    /// Raycast for PlaceableObjects in range and show pickup prompt.
    /// </summary>
    private void CheckForPickup()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, placeableLayer))
        {
            PlaceableObject placeable = hit.collider.GetComponent<PlaceableObject>();

            if (placeable != null && !placeable.IsPlaced)
            {
                HUDController.instance?.EnableInteractionText($"Pick up {placeable.displayName}");

                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E))
                    PickupObject(placeable);

                return;
            }
        }

        HUDController.instance?.DisableInteractionText();
    }

    /// <summary>Picks up the given object and begins carrying it.</summary>
    private void PickupObject(PlaceableObject placeable)
    {
        _carriedObject = placeable;
        _carriedObject.transform.SetParent(holdPoint);
        _carriedObject.OnPickedUp();
    }

    /// <summary>Smoothly moves the held object to the hold point.</summary>
    private void MoveHeldObject()
    {
        if (_carriedObject == null) return;

        _carriedObject.transform.localPosition = Vector3.Lerp(
            _carriedObject.transform.localPosition,
            Vector3.zero,
            Time.deltaTime * holdSmoothing
        );

        _carriedObject.transform.localRotation = Quaternion.Lerp(
            _carriedObject.transform.localRotation,
            Quaternion.identity,
            Time.deltaTime * holdSmoothing
        );
    }

    /// <summary>
    /// Asks the carried object whether the player is aiming at its ghost.
    /// Updates highlight and HUD accordingly.
    /// </summary>
    private void CheckForGhost()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        bool wasAiming = _aimingAtGhost;

        _aimingAtGhost = _carriedObject != null && _carriedObject.IsAimingAtGhost(ray);

        // Only update highlight when state changes
        if (_aimingAtGhost != wasAiming)
        {
            _carriedObject?.SetGhostHighlighted(_aimingAtGhost);

            HUDController.instance?.EnableInteractionText(
                _aimingAtGhost ? "Place here" : "Drop");
        }
    }

    /// <summary>Drops the carried object without restoring it.</summary>
    private void DropObject()
    {
        if (_carriedObject == null) return;

        _carriedObject.transform.SetParent(null);
        _carriedObject.OnDropped();
        _carriedObject = null;
        _aimingAtGhost = false;
    }

    /// <summary>Places the carried object at its correct position.</summary>
    private void PlaceObject()
    {
        if (_carriedObject == null) return;

        _carriedObject.OnPlaced();
        _carriedObject = null;
        _aimingAtGhost = false;
    }

    /// <summary>All objects restored — fire completion notification.</summary>
    private void OnAllObjectsPlaced()
    {
        NotificationSystem.Instance?.ShowNotification("All items restored! Level complete!");
        // TODO: load next scene or trigger cutscene here
    }
}