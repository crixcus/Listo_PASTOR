using UnityEngine;
public class MopTool : MonoBehaviour
{
    public float cleaningSpeed = 0.3f;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void CleanTarget(CleanableObject target)
    {
        target.Clean(cleaningSpeed * Time.deltaTime);
    }

    public void SetCleaning(bool isCleaning)
    {
        if (animator == null) return;
        animator.SetBool("isCleaning", isCleaning);
    }
}