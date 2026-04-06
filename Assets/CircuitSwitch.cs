using UnityEngine;

public class CircuitSwitch : MonoBehaviour
{
    public Animator animator;

    public float interactDistance = 2f;

    public AudioSource audioSource; // audio

    private Transform player;
    private bool canUse = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= interactDistance)
        {
            if (Input.GetKeyDown(KeyCode.F) && canUse)
            {
                UseSwitch();
            }
        }
    }

    void UseSwitch()
    {
        canUse = false;

        audioSource.Play(); // play sound

        animator.SetTrigger("OFF");

        // Notification trigger
        NotificationSystem.TriggerObjectiveComplete("Circuit Breaker OFF");

        Invoke(nameof(ResetSwitch), 1f); // time of animation
    }

    void ResetSwitch()
    {
        canUse = true;
    }
}