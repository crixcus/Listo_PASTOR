using UnityEngine;

public class MopTool : MonoBehaviour
{
    public float cleaningSpeed = 0.3f;

    public void CleanTarget(CleanableObject target)
    {
        target.Clean(cleaningSpeed * Time.deltaTime);
    }
}