using UnityEngine;

/// <summary>
/// Attach to any object that should be stackable.
/// Works with SimpleStackingSystem on the player.
///
/// heightOffset: how high above the target's pivot point this object sits when stacked.
/// Set this to roughly the height of the object so stacking looks correct.
/// Example: a box 1 unit tall should have heightOffset = 1.0
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SimpleStackable : MonoBehaviour
{
    [Tooltip("How high above the target box this object sits when stacked. " +
             "Set to roughly the height of this object.")]
    public float heightOffset = 1f;

    /// <summary>
    /// The SimpleStackable currently stacked on top of this one, if any.
    /// Used to calculate total stack height for correct placement.
    /// </summary>
    public SimpleStackable StackedOnTop { get; private set; }

    /// <summary>
    /// Whether this object is currently being carried by the player.
    /// Prevents it being targeted as a stack destination while held.
    /// </summary>
    public bool IsCarried { get; private set; }

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Called by SimpleStackingSystem when the player picks this up.
    /// Clears it from any previous stack relationship.
    /// </summary>
    public void Pickup(Transform holdPoint)
    {
        IsCarried = true;
        _rb.isKinematic = true;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Called by SimpleStackingSystem when the player drops without stacking.
    /// </summary>
    public void Drop()
    {
        IsCarried = false;
        transform.SetParent(null);
        _rb.isKinematic = false;
    }

    /// <summary>
    /// Stacks this object on top of the target.
    /// Calculates the correct Y position by walking up the target's stack
    /// to find the topmost occupied position.
    /// </summary>
    public void StackOn(SimpleStackable target)
    {
        IsCarried = false;
        transform.SetParent(null);

        // Find the TOP of the target's current stack
        // Walk up the chain until we find a box with nothing on top
        SimpleStackable topOfStack = target.GetTopOfStack();

        // Place this box on top of the topmost box
        Vector3 topPos = topOfStack.transform.position
                       + Vector3.up * topOfStack.heightOffset;

        transform.position = topPos;
        transform.rotation = topOfStack.transform.rotation;

        _rb.isKinematic = true;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Register this as sitting on top of the topmost box
        topOfStack.StackedOnTop = this;
    }

    /// <summary>
    /// Clears the StackedOnTop reference when the object on top is picked up.
    /// Called by SimpleStackingSystem when picking up a stacked object.
    /// </summary>
    public void ClearTop()
    {
        StackedOnTop = null;
    }

    /// <summary>
    /// Walks up the stack chain and returns the topmost SimpleStackable
    /// that has nothing stacked on it yet.
    /// Prevents infinite loops with a depth limit of 20.
    /// </summary>
    public SimpleStackable GetTopOfStack()
    {
        SimpleStackable current = this;
        int safetyLimit = 20;

        while (current.StackedOnTop != null && safetyLimit > 0)
        {
            current = current.StackedOnTop;
            safetyLimit--;
        }

        return current;
    }
}