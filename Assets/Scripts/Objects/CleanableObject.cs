using UnityEngine;

public class CleanableObject : MonoBehaviour
{
    [Range(0f, 1f)]
    public float dirtAmount = 1f;  // 1 = fully dirty, 0 = fully clean

    private SpriteRenderer spriteRenderer;

    public bool IsClean => dirtAmount <= 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    /// <summary>
    /// Call this to clean the object over time.
    /// </summary>
    /// <param name="amountPerSecond">Amount to reduce per second</param>
    public void Clean(float amountPerSecond)
    {
        if (IsClean) return;

        dirtAmount -= amountPerSecond * Time.deltaTime;
        dirtAmount = Mathf.Clamp01(dirtAmount);

        UpdateVisuals();

        if (IsClean)
        {
            OnCleaned();
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        Color c = spriteRenderer.color;
        c.a = dirtAmount; // alpha = dirt amount
        spriteRenderer.color = c;
        Debug.Log($"{gameObject.name} dirt amount: {dirtAmount}");
    }

    private void OnCleaned()
    {
        Debug.Log($"{gameObject.name} fully cleaned!");
        // Optional: disable outline, play sound, particles, etc.
    }
}
