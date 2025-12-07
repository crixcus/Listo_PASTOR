using UnityEngine;
using UnityEngine.EventSystems;

public class MenuAnimatorController : MonoBehaviour
{
    private EventTrigger eventTrigger;
    private Animator animator;

    void Awake()
    {
        eventTrigger = GetComponent<EventTrigger>();
        animator = GetComponent<Animator>();
    }

    // Called by PointerClick (or whatever handles navigation)
    public void ClickOutro()
    {
        // 1. Disable the Event Trigger immediately to prevent PointerEnter from firing.
        eventTrigger.enabled = false;

        // 2. Play the Outro animation.
       // animator.Play("Text_Outro");

        // 3. Start a coroutine to wait for the animation to finish.
        StartCoroutine(WaitForAnimation("Text_Outro"));
    }

    private System.Collections.IEnumerator WaitForAnimation(string clipName)
    {
        // Wait for the next frame
        yield return null;

        // Get the length of the clip being played
        float clipLength = GetClipLength(clipName);

        // Wait for the duration of the clip
        yield return new WaitForSeconds(clipLength);

        // 4. Once the animation is done, re-enable the Event Trigger.
        eventTrigger.enabled = true;

        // Optional: Play the Idle clip, or whatever the default state is.
        // animator.Play("Text_Idle"); 
    }

    // Helper to get clip duration
    private float GetClipLength(string clipName)
    {
        if (animator.runtimeAnimatorController == null) return 0f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0f;
    }
}