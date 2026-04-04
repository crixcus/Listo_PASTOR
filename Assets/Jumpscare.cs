using UnityEngine;

/// <summary>
/// Tripwire that triggers a jumpscare when the player enters.
/// Has a 1 in 3 chance of triggering. Once the roll is made it won't fire again.
///
/// Previously required flood to reach max height before activating.
/// Now triggers at any time — place anywhere in the scene.
///
/// Setup:
///   - Attach to a GameObject with a Collider (Is Trigger checked)
///   - Assign the AudioSource with your jumpscare sound
///   - Tune traumaAmount and chance in Inspector
/// </summary>
public class JumpScare : MonoBehaviour
{
    [Tooltip("AudioSource with the jumpscare sound clip.")]
    public AudioSource audioSource;

    [Tooltip("How much trauma to add when triggered (0-1). 0.35 = intense hit.")]
    [Range(0f, 1f)]
    public float traumaAmount = 0.35f;

    [Tooltip("1 in X chance of triggering. 3 = 33% chance, 2 = 50%, etc.")]
    public int chance = 3;

    private bool _hasRolled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasRolled || !other.CompareTag("Player")) return;

        _hasRolled = true;

        bool triggered = Random.Range(0, chance) == 0;
        if (!triggered) return;

        audioSource?.Play();
        TraumaBar.Instance?.AddTrauma(traumaAmount);
    }
}