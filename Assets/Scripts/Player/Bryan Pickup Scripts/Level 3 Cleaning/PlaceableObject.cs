using UnityEngine;

/// <summary>
/// Attach to any furniture object that was displaced by the flood.
/// 
/// Setup per object:
///   1. Place the object at its CORRECT position in the scene editor.
///   2. Create an empty GameObject where the object should appear displaced
///      (e.g. knocked over in a corner) and assign it to displacedTransform.
///   3. On Play, the object moves to the displaced position.
///   4. Player picks it up and places it back at the original position.
///   5. A ghost (transparent blue copy) marks the original position while carrying.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PlaceableObject : MonoBehaviour
{
    [Tooltip("Display name shown in the HUD when looking at this object.")]
    public string displayName = "Object";

    [Tooltip("Empty GameObject marking where the flood displaced this object to.")]
    public Transform displacedTransform;

    [Tooltip("Transparent blue material for the ghost preview.")]
    public Material ghostMaterial;

    [Tooltip("How close the player needs to aim at the ghost to place the object.")]
    public float placementDistance = 4f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private Rigidbody _rb;
    private Collider _collider;
    private GameObject _ghost;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    public bool IsPlaced { get; private set; }
    public bool IsBeingCarried { get; private set; }
    public bool holdOnly = false;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    /// <summary>
    /// Saves the original position, builds the ghost at that position,
    /// then moves the object to its flood-displaced location.
    /// </summary>
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        if (holdOnly)
        {
            // Don't fall, don't displace, don't build ghost
            _rb.isKinematic = true;
            _rb.useGravity = false;
            return;
        }

        BuildGhost();
        SetGhostVisible(false);

        if (displacedTransform != null)
        {
            transform.SetPositionAndRotation(
                displacedTransform.position,
                displacedTransform.rotation
            );
        }
        else
        {
            Debug.LogWarning($"[PlaceableObject] '{name}' has no displacedTransform assigned.");
        }
    }

    // ------------------------------------------------------------------
    // Dirt check
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns true if the object has no dirt remaining.
    /// Checks the _DirtStrength property on the object's material.
    /// </summary>
    public bool IsClean()
    {
        Renderer rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (rend == null) return true;

        Material mat = rend.material;
        if (!mat.HasProperty("_DirtStrength")) return true;

        return mat.GetFloat("_DirtStrength") <= 0f;
    }

    // ------------------------------------------------------------------
    // Public API — called by PlacementSystem
    // ------------------------------------------------------------------

    /// <summary>
    /// Called when the player picks up this object.
    /// Freezes physics and shows the ghost at the correct position.
    /// </summary>
    public void OnPickedUp()
    {
        if (IsPlaced) return;

        IsBeingCarried = true;
        _rb.isKinematic = true;
        _rb.useGravity = false;

        SetGhostVisible(true);
        PlacementSystem.Instance?.OnObjectPickedUp(this);
    }

    /// <summary>
    /// Called when the player drops this object without placing it.
    /// Re-enables physics and hides the ghost.
    /// </summary>
    public void OnDropped()
    {
        IsBeingCarried = false;

        if (holdOnly)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            // Unparent first so it drops from current position
            transform.SetParent(null);
        }
        else
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

        SetGhostVisible(false);
        SetGhostHighlighted(false);
        PlacementSystem.Instance?.OnObjectDropped();
    }

    /// <summary>
    /// Called when the player successfully places this object back.
    /// Snaps it to its original position and locks it permanently.
    /// Blocked if the object still has dirt remaining.
    /// </summary>
    public void OnPlaced()
    {
        // Block placement if not clean yet
        if (!IsClean())
        {
            NotificationSystem.Instance?.ShowNotification("Clean the object first!");
            return;
        }

        IsPlaced = true;
        IsBeingCarried = false;

        // Unparent before snapping to avoid offset issues
        transform.SetParent(null);

        // Snap back to original position and rotation
        transform.SetPositionAndRotation(_originalPosition, _originalRotation);

        // Kill all physics
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Clean up ghost and prevent re-pickup
        if (_ghost != null) Destroy(_ghost);
        _collider.enabled = false;

        PlacementSystem.Instance?.OnObjectPlaced(this);
    }

    /// <summary>
    /// Returns true if the player's raycast is hitting this object's ghost.
    /// Called every frame by PlacementSystem while this object is being carried.
    /// Only returns true if the object is clean.
    /// </summary>
    public bool IsAimingAtGhost(Ray ray)
    {
        if (_ghost == null) return false;

        Collider ghostCollider = _ghost.GetComponent<Collider>();
        if (ghostCollider == null) return false;

        return ghostCollider.Raycast(ray, out _, placementDistance);
    }

    /// <summary>
    /// Brightens or dims the ghost to indicate the player can place here.
    /// </summary>
    public void SetGhostHighlighted(bool highlighted)
    {
        if (_ghost == null) return;

        Renderer r = _ghost.GetComponent<Renderer>();
        if (r == null) return;

        Color c = r.material.color;
        c.a = highlighted ? 0.6f : 0.35f;
        r.material.color = c;
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Builds a transparent ghost copy of this object at the original position.
    /// The ghost gets a BoxCollider so the placement raycast can detect it.
    /// </summary>
    private void BuildGhost()
    {
        // Try to find the mesh — check self first, then children
        MeshFilter sourceMesh = GetComponent<MeshFilter>()
                             ?? GetComponentInChildren<MeshFilter>();

        if (sourceMesh == null)
        {
            Debug.LogWarning($"[PlaceableObject] '{name}' has no MeshFilter — ghost won't be visible.");
            return;
        }

        _ghost = new GameObject($"Ghost_{name}");
        _ghost.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
        _ghost.transform.localScale = transform.lossyScale;

        // Copy mesh
        MeshFilter mf = _ghost.AddComponent<MeshFilter>();
        mf.sharedMesh = sourceMesh.sharedMesh;

        // Apply ghost material
        MeshRenderer mr = _ghost.AddComponent<MeshRenderer>();
        if (ghostMaterial != null)
            mr.material = new Material(ghostMaterial) { color = new Color(0.2f, 0.3f, 0.5f, 0.25f) };
        else
            Debug.LogWarning($"[PlaceableObject] '{name}' has no ghost material assigned.");

        // Add a BoxCollider sized to the object's renderer bounds
        BoxCollider bc = _ghost.AddComponent<BoxCollider>();
        Renderer sourceRenderer = GetComponent<Renderer>();
        if (sourceRenderer != null)
        {
            bc.center = sourceRenderer.localBounds.center;
            bc.size = sourceRenderer.localBounds.size;
        }
    }

    /// <summary>Shows or hides the ghost GameObject.</summary>
    private void SetGhostVisible(bool visible)
    {
        if (_ghost != null)
            _ghost.SetActive(visible);
    }
}