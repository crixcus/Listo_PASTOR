using UnityEngine;

/// <summary>
/// Attach to any object that can be picked up AND stacked on top of other objects.
///
/// SETUP PER OBJECT:
///   1. Attach Stackable.cs to the object
///   2. Make sure it has a Rigidbody and Collider
///   3. Keep the existing Interactable component
///   4. In Interactable → OnInteraction() → wire to StackingSystem.TryPickup()
///   5. Add a child empty GameObject at the TOP → add StackPoint.cs to it
///   6. Set the object's layer to "Stackable"
///   7. If ghost mesh doesn't appear, manually assign targetMesh
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Stackable : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Display name shown in HUD when player looks at this object.")]
    public string displayName = "Box";

    [Tooltip("Offset from the StackPoint position. Fine-tune snapping height here.")]
    public Vector3 snapOffset = Vector3.zero;

    [Header("Mesh Override")]
    [Tooltip("If the ghost preview shows nothing, manually drag the correct " +
             "MeshFilter child here (e.g. the pCube that represents the box shape).")]
    public MeshFilter targetMesh;

    // ------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------

    public bool IsCarried { get; private set; }
    public bool IsStacked { get; private set; }
    public StackPoint CurrentStackPoint { get; private set; }

    private Rigidbody _rb;
    private Collider _collider;
    private Transform _originalParent;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Called by StackingSystem when player picks up this object.
    /// </summary>
    public void OnPickedUp(Transform holdParent)
    {
        IsCarried = true;
        IsStacked = false;

        if (CurrentStackPoint != null)
        {
            CurrentStackPoint.SetVacant();
            CurrentStackPoint = null;
        }

        _originalParent = transform.parent;
        _rb.isKinematic = true;
        _rb.useGravity = false;

        transform.SetParent(holdParent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Called by StackingSystem when player drops without stacking.
    /// </summary>
    public void OnDropped()
    {
        IsCarried = false;

        transform.SetParent(_originalParent);
        _rb.isKinematic = false;
        _rb.useGravity = true;
    }

    /// <summary>
    /// Called by StackingSystem when player stacks this on a StackPoint.
    /// Snaps position and freezes physics. Object can still be picked up again.
    /// </summary>
    public void OnStacked(StackPoint stackPoint)
    {
        IsCarried = false;
        IsStacked = true;
        CurrentStackPoint = stackPoint;

        transform.SetParent(null);
        transform.position = stackPoint.transform.position + snapOffset;
        transform.rotation = stackPoint.transform.rotation;

        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        stackPoint.SetOccupied(this);

        NotificationSystem.Instance?.ShowNotification($"{displayName} stacked!");
    }
}