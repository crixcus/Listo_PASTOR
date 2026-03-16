using UnityEngine;

/// <summary>
/// Tripwire that only activates once the flood has fully risen to max height.
/// Has a 1 in 3 chance of triggering when the player enters.
///
/// Setup:
///   - Attach to a GameObject with a Collider (Is Trigger checked).
///   - Assign the AudioSource with your jumpscare sound.
///   - No Inspector wiring needed — activates automatically when flood completes.
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

    private bool _isActive = false;
    private bool _hasRolled = false;

    private void OnEnable()
    {
        WaterRiser.OnFloodComplete += Activate;
    }

    private void OnDisable()
    {
        WaterRiser.OnFloodComplete -= Activate;
    }

    /// <summary>
    /// Called by WaterRiser.OnFloodComplete when water reaches max height.
    /// </summary>
    private void Activate()
    {
        _isActive = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive || _hasRolled || !other.CompareTag("Player")) return;

        _hasRolled = true;

        bool triggered = Random.Range(0, chance) == 0;
        if (!triggered) return;

        audioSource?.Play();
        TraumaBar.Instance?.AddTrauma(traumaAmount);
    }
}