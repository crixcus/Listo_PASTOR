using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    public float scrollSpeed = 30f;
    public RectTransform content;

    [Tooltip("Starting Y position when credits open")]
    public float startY = -1200f;

    [Tooltip("Y position where scrolling stops")]
    public float stopY = 0f;

    private bool hasStopped = false;

    void OnEnable()
    {
        if (content == null) return;

        Vector2 pos = content.anchoredPosition;
        pos.y = startY;
        content.anchoredPosition = pos;

        hasStopped = false;
    }

    void Update()
    {
        if (hasStopped || content == null) return;

        Vector2 pos = content.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;

        if (pos.y >= stopY)
        {
            pos.y = stopY;
            hasStopped = true;
        }

        content.anchoredPosition = pos;
    }
}
