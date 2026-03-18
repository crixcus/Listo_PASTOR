using UnityEngine;

public class BreakerSwitch : MonoBehaviour
{
    public Animator animator;

    public string idleClip1 = "Idle1";
    public string idleClip2 = "Idle2";
    public string switchClip = "Switch";

    public float interactDistance = 2f;
    public Transform player; // assign manually in inspector if you want

    private bool canUse = true;

    void Start()
    {
        // Start with first idle
        animator.Play(idleClip1);
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= interactDistance)
        {
            // Press E to use the switch
            if (Input.GetKeyDown(KeyCode.E) && canUse)
            {
                UseSwitch();
            }
            else if (canUse)
            {
                // Choose which idle to play depending on current state
                var current = animator.GetCurrentAnimatorStateInfo(0);
                if (!current.IsName(idleClip1) && !current.IsName(idleClip2))
                {
                    animator.Play(idleClip1);
                }
            }
        }
        else
        {
            // If player is far, ensure idle
            var current = animator.GetCurrentAnimatorStateInfo(0);
            if (!current.IsName(idleClip1) && !current.IsName(idleClip2))
            {
                animator.Play(idleClip2);
            }
        }
    }

    void UseSwitch()
    {
        canUse = false;

        animator.Play(switchClip);

        // Reset after switch animation finishes
        float clipLength = animator.runtimeAnimatorController.animationClips[0].length;
        // optional: you can manually set time if needed
        Invoke(nameof(ResetSwitch), 1f);
    }

    void ResetSwitch()
    {
        canUse = true;
        animator.Play(idleClip1);
    }
}