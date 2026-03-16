using UnityEngine;

/// <summary>
/// Tripwire trigger that has a 1 in 3 chance of triggering a jumpscare
/// when the player enters it. Once the roll is made (pass or fail),
/// it won't fire again.
///
/// Setup:
///   - Attach to a GameObject with a Collider (Is Trigger checked).
///   - Assign the AudioSource with your jumpscare sound.
///   - Tune traumaAmount in the Inspector (0-1). 0.35 = a big hit.
///   - Change 'chance' to adjust odds. 3 = 1 in 3, 2 = 1 in 2, etc.
/// </summary>
public class JumpScare : MonoBehaviour
{
    [Tooltip("AudioSource with the jumpscare sound clip.")]
    public AudioSource audioSource;

    [Tooltip("How much trauma to add when triggered (0-1). 0.35 = intense hit.")]
    [Range(0f, 1f)]
    public float traumaAmount = 0.35f;

    [Tooltip("1 in X chance of triggering. 3 = 33% chance, 2 = 50% chance.")]
    public int chance = 3;

    private bool _hasRolled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasRolled || !other.CompareTag("Player")) return;

        // Only rolls once regardless of pass or fail
        _hasRolled = true;

        // Random.Range(0, chance) returns 0 to chance-1
        // So == 0 is exactly 1 in 'chance' probability
        bool triggered = Random.Range(0, chance) == 0;

        if (!triggered) return;

        audioSource?.Play();
        TraumaBar.Instance?.AddTrauma(traumaAmount);
    }
}