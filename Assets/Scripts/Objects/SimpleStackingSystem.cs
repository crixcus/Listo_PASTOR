using UnityEngine;

/// <summary>
/// Manages the stacking mechanic. Attach to PlayerHolder.
///
/// CONTROLS:
///   E — pick up a box / stack held box onto target / drop held box
///
/// FLOW:
///   1. Look at a box → press E → pick it up
///   2. Look at ANOTHER box → press E → stacks on top of that box's stack
///   3. Look at nothing / same box → press E → drops the held box
///
/// SETUP:
///   - Assign cameraTransform → Main Camera
///   - Assign holdPoint → empty child in front of camera
///   - Set stackableLayer → layer your boxes are on
/// </summary>
public class SimpleStackingSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main camera transform for raycasting.")]
    public Transform cameraTransform;

    [Tooltip("Empty child transform in front of camera where held object floats.")]
    public Transform holdPoint;

    [Header("Settings")]
    [Tooltip("Max distance to pick up or stack objects.")]
    public float range = 3f;

    [Tooltip("Layer that stackable objects are on.")]
    public LayerMask stackableLayer;

    [Tooltip("How smoothly the held object follows the hold point.")]
    public float holdSmoothing = 15f;

    // The object currently being carried
    private SimpleStackable _carried;

    // One frame cooldown after pickup to prevent instant drop
    // (E key fires pickup and would immediately trigger drop on the same frame)
    private bool _justPickedUp = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_carried == null)
            {
                TryPickup();
            }
            else
            {
                // Skip drop/stack on the same frame as pickup
                if (!_justPickedUp)
                    TryDropOrStack();
            }
        }

        // Reset one-frame cooldown
        if (_justPickedUp)
            _justPickedUp = false;

        if (_carried != null)
            MoveHeld();
    }

    /// <summary>
    /// Raycasts forward and picks up the first SimpleStackable hit.
    /// Only picks up objects not currently being carried.
    /// Uses GetComponentInParent to handle complex multi-mesh objects.
    /// </summary>
    private void TryPickup()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, range, stackableLayer)) return;

        SimpleStackable s = hit.collider.GetComponentInParent<SimpleStackable>();
        if (s == null || s.IsCarried) return;

        // If this box has something stacked on top, clear that relationship
        // since we're removing it from the stack
        FindAndClearParentReference(s);

        _carried = s;
        _justPickedUp = true;
        s.Pickup(holdPoint);
    }

    /// <summary>
    /// Raycasts forward while carrying an object.
    /// If a DIFFERENT SimpleStackable is hit → stack the carried object on it.
    /// If nothing is hit or the same object is hit → drop the carried object.
    /// </summary>
    private void TryDropOrStack()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, stackableLayer))
        {
            // Use GetComponentInParent to correctly detect stacked boxes
            // even if the raycast hits a child collider
            SimpleStackable target = hit.collider.GetComponentInParent<SimpleStackable>();

            // Only stack if we hit a DIFFERENT box that isn't being carried
            if (target != null && target != _carried && !target.IsCarried)
            {
                _carried.StackOn(target);
                _carried = null;
                return;
            }
        }

        // Nothing valid to stack on — just drop
        _carried.Drop();
        _carried = null;
    }

    /// <summary>
    /// Smoothly moves the held object toward the hold point each frame.
    /// </summary>
    private void MoveHeld()
    {
        _carried.transform.position = Vector3.Lerp(
            _carried.transform.position,
            holdPoint.position,
            Time.deltaTime * holdSmoothing
        );

        _carried.transform.rotation = Quaternion.Lerp(
            _carried.transform.rotation,
            holdPoint.rotation,
            Time.deltaTime * holdSmoothing
        );
    }

    /// <summary>
    /// When picking up a box that's in the middle or bottom of a stack,
    /// finds the box below it and clears its StackedOnTop reference.
    /// This prevents the stack chain from pointing to a carried/missing object.
    /// </summary>
    private void FindAndClearParentReference(SimpleStackable target)
    {
        // Find all SimpleStackables in the scene and check if any has
        // target as its StackedOnTop — if so, clear that reference
        SimpleStackable[] all = FindObjectsByType<SimpleStackable>(FindObjectsSortMode.None);
        foreach (var s in all)
        {
            if (s.StackedOnTop == target)
            {
                s.ClearTop();
                break;
            }
        }
    }
}