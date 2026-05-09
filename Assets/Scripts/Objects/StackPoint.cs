using UnityEngine;

/// <summary>
/// Attach to an empty child GameObject positioned at the TOP of any stackable object.
/// This marks where another Stackable object can be placed on top.
///
/// SETUP PER OBJECT:
///   1. Select your box/object in the hierarchy
///   2. Right click → Create Empty → name it "StackPoint_Top"
///   3. Move it to the TOP CENTER of the object (slightly above the surface)
///   4. Add StackPoint.cs to it
///   5. Add a BoxCollider (size 0.5/0.1/0.5) — Is Trigger OFF
///   6. Set its layer to "StackPoint"
///   7. Assign ghostMaterial in the Inspector
///   8. Optionally assign targetMesh if auto-detection fails (complex objects)
/// </summary>
public class StackPoint : MonoBehaviour
{
    [Header("Ghost Preview")]
    [Tooltip("Transparent material used to preview the stacked object.")]
    public Material ghostMaterial;

    [Tooltip("Highlight color when player aims at this stack point.")]
    public Color highlightColor = new Color(0.4f, 0.8f, 1f, 0.6f);

    [Tooltip("Default ghost color when visible but not highlighted.")]
    public Color defaultColor = new Color(0.2f, 0.5f, 1f, 0.35f);

    [Header("Manual Mesh Override")]
    [Tooltip("If the ghost mesh isn't showing, manually drag the correct MeshFilter here. " +
             "Use this for complex objects where auto-detection fails.")]
    public MeshFilter targetMesh;

    /// <summary>Whether this stack point currently has an object snapped to it.</summary>
    public bool IsOccupied { get; private set; }

    /// <summary>The Stackable currently occupying this point, if any.</summary>
    public Stackable OccupiedBy { get; private set; }

    private GameObject _ghost;
    private Renderer _ghostRenderer;
    private bool _isHighlighted;

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Shows a ghost preview shaped like the given Stackable at this stack point.
    /// First checks the manually assigned targetMesh, then searches all children
    /// recursively including inactive ones.
    /// Called by StackingSystem when the player starts carrying a Stackable.
    /// </summary>
    public void ShowGhost(Stackable stackable)
    {
        if (_ghost != null) HideGhost();

        // Priority 1: manually assigned mesh on the Stackable
        MeshFilter source = stackable.targetMesh;

        // Priority 2: auto-detect from Stackable itself
        if (source == null)
            source = stackable.GetComponent<MeshFilter>();

        // Priority 3: search all children recursively including inactive
        if (source == null)
            source = stackable.GetComponentInChildren<MeshFilter>(true);

        Debug.Log($"[StackPoint] ShowGhost on {name}. MeshFilter: {source != null}, Material: {ghostMaterial != null}");

        if (source == null)
        {
            Debug.LogWarning($"[StackPoint] Could not find MeshFilter on {stackable.name}. " +
                             "Manually assign the targetMesh field on the Stackable component.");
            return;
        }

        _ghost = new GameObject("StackGhost");
        _ghost.transform.position = transform.position;
        _ghost.transform.rotation = stackable.transform.rotation;
        _ghost.transform.localScale = stackable.transform.lossyScale;

        MeshFilter mf = _ghost.AddComponent<MeshFilter>();
        mf.sharedMesh = source.sharedMesh;

        MeshRenderer mr = _ghost.AddComponent<MeshRenderer>();
        if (ghostMaterial != null)
            mr.material = new Material(ghostMaterial) { color = defaultColor };
        else
            Debug.LogWarning($"[StackPoint] No ghost material assigned on {name}.");

        _ghostRenderer = mr;
        _ghost.SetActive(true);
    }

    /// <summary>
    /// Hides and destroys the ghost preview.
    /// </summary>
    public void HideGhost()
    {
        if (_ghost != null)
        {
            Destroy(_ghost);
            _ghost = null;
            _ghostRenderer = null;
        }
        _isHighlighted = false;
    }

    /// <summary>
    /// Highlights or un-highlights the ghost preview.
    /// </summary>
    public void SetHighlighted(bool highlighted)
    {
        if (_isHighlighted == highlighted || _ghostRenderer == null) return;
        _isHighlighted = highlighted;
        _ghostRenderer.material.color = highlighted ? highlightColor : defaultColor;
    }

    /// <summary>
    /// Marks this stack point as occupied. Disables collider to prevent
    /// receiving another object while occupied.
    /// </summary>
    public void SetOccupied(Stackable stackable)
    {
        IsOccupied = true;
        OccupiedBy = stackable;
        HideGhost();

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    /// <summary>
    /// Clears occupied state when stacked object is picked up again.
    /// Re-enables collider so this point can receive a new object.
    /// </summary>
    public void SetVacant()
    {
        IsOccupied = false;
        OccupiedBy = null;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsOccupied
            ? new Color(1f, 0f, 0f, 0.5f)
            : new Color(0f, 1f, 0f, 0.5f);

        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.3f);
    }
}