using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlacementSystem : MonoBehaviour
{
    public Rig mopRig;
    public Rig ragRig;
    public Rig holdPointRig;
    public DirtCleaner dirtCleaner;
    public MopPickupAction mopAction;

    public static PlacementSystem Instance { get; private set; }
    public bool IsCarryingObject => _carriedObject != null;

    [Header("References")]
    public Transform playerCamera;
    public Transform holdPoint;

    [Header("Interaction Settings")]
    public float pickupRange = 3f;
    public float holdSmoothing = 12f;

    [Header("Layer Masks")]
    public LayerMask placeableLayer;

    private PlaceableObject _carriedObject;
    private bool _aimingAtGhost;
    private int _totalPlaceables;
    private int _placedCount;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _totalPlaceables = FindObjectsByType<PlaceableObject>(FindObjectsSortMode.None).Length;
    }

    private void Update()
    {
        if (_carriedObject != null)
        {
            MoveHeldObject();
            CheckForGhost();

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (_aimingAtGhost)
                {
                    if (_carriedObject != null && !_carriedObject.IsClean())
                        NotificationSystem.Instance?.ShowDebounced("placement", "The object is still dirty! Clean it first.", 0.2f);
                    else
                        PlaceObject();
                }
                else
                {
                    DropObject();
                }
            }
        }
        else
        {
            CheckForPickup();
        }
    }

    void SetRigWeight(Rig rig, float weight)
    {
        if (rig != null) rig.weight = weight;
    }

    // Hides whichever tool is active and activates holdPoint rig
    void OnStartCarrying()
    {
        bool mopActive = mopAction != null &&
                         mopAction.heldMop != null &&
                         mopAction.heldMop.gameObject.activeSelf;

        bool ragActive = mopAction != null &&
                         mopAction.HeldRag != null &&
                         mopAction.HeldRag.gameObject.activeSelf;

        if (mopActive) mopAction.heldMop.gameObject.SetActive(false);
        if (ragActive) mopAction.HeldRag.gameObject.SetActive(false);

        SetRigWeight(mopRig, 0f);
        SetRigWeight(ragRig, 0f);
        SetRigWeight(holdPointRig, 1f);
    }

    // Restores whichever tool was active before carrying
    void OnStopCarrying()
    {
        SetRigWeight(holdPointRig, 0f);

        if (mopAction == null) return;

        bool mopPickedUp = mopAction.mopPickup;
        bool ragPickedUp = mopAction.ragPickup;

        // Restore whichever was the last equipped tool
        // MopPickupAction tracks this via its own state
        bool mopWasActive = mopPickedUp &&
                            (mopAction.HeldRag == null || !mopAction.HeldRag.gameObject.activeSelf);

        if (mopWasActive && mopPickedUp)
        {
            mopAction.heldMop.gameObject.SetActive(true);
            SetRigWeight(mopRig, 1f);
            SetRigWeight(ragRig, 0f);
        }
        else if (ragPickedUp)
        {
            mopAction.HeldRag.gameObject.SetActive(true);
            SetRigWeight(ragRig, 1f);
            SetRigWeight(mopRig, 0f);
        }
    }

    public void OnObjectPickedUp(PlaceableObject obj)
    {
        HUDController.instance?.EnableInteractionText("Drop");
    }

    public void OnObjectDropped()
    {
        HUDController.instance?.DisableInteractionText();
    }

    public void OnObjectPlaced(PlaceableObject obj)
    {
        _placedCount++;
        NotificationSystem.Instance?.ShowNotification($"Item restored! ({_placedCount}/{_totalPlaceables})");
        HUDController.instance?.DisableInteractionText();
        if (_placedCount >= _totalPlaceables) OnAllObjectsPlaced();
    }

    private void CheckForPickup()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, placeableLayer))
        {
            PlaceableObject placeable = hit.collider.GetComponent<PlaceableObject>();

            if (placeable != null && !placeable.IsPlaced)
            {
                HUDController.instance?.EnableInteractionText($"Pick up {placeable.displayName}");

                if (Input.GetKeyDown(KeyCode.F))
                    PickupObject(placeable);

                return;
            }
        }

        HUDController.instance?.DisableInteractionText();
    }

    private void PickupObject(PlaceableObject placeable)
    {
        PlayerEquipment equipment = FindObjectOfType<PlayerEquipment>();
        if (equipment != null)
        {
            try { equipment.ForceUnequip(); }
            catch (System.Exception e) { Debug.LogWarning("ForceUnequip failed: " + e.Message); }
        }

        OnStartCarrying();

        _carriedObject = placeable;
        _carriedObject.transform.SetParent(holdPoint);
        _carriedObject.OnPickedUp();

        MopTool mop = placeable.GetComponent<MopTool>();
        if (mop != null)
        {
            PlayerCleaning playerCleaning = FindObjectOfType<PlayerCleaning>();
            if (playerCleaning != null)
                playerCleaning.EquipMop(mop);
        }
    }

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

    private void CheckForGhost()
    {
        if (_carriedObject.holdOnly)
        {
            HUDController.instance?.EnableInteractionText("Drop");
            _aimingAtGhost = false;
            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        bool wasAiming = _aimingAtGhost;
        _aimingAtGhost = _carriedObject != null && _carriedObject.IsAimingAtGhost(ray);

        if (_aimingAtGhost != wasAiming)
        {
            _carriedObject?.SetGhostHighlighted(_aimingAtGhost);
            HUDController.instance?.EnableInteractionText(_aimingAtGhost ? "Place here" : "Drop");
        }
    }

    private void DropObject()
    {
        if (_carriedObject == null) return;

        MopTool mop = _carriedObject.GetComponent<MopTool>();
        if (mop != null)
        {
            PlayerCleaning playerCleaning = FindObjectOfType<PlayerCleaning>();
            if (playerCleaning != null)
                playerCleaning.EquipMop(null);
        }

        _carriedObject.transform.SetParent(null);
        _carriedObject.OnDropped();
        _carriedObject = null;
        _aimingAtGhost = false;

        OnStopCarrying();
    }

    private void PlaceObject()
    {
        if (_carriedObject == null) return;

        _carriedObject.OnPlaced();
        _carriedObject = null;
        _aimingAtGhost = false;

        OnStopCarrying();
    }

    private void OnAllObjectsPlaced()
    {
        NotificationSystem.Instance?.ShowNotification("All items restored! Level complete!");
    }
}