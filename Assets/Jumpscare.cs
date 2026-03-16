using UnityEngine;

/// <summary>
/// Tripwire trigger that plays a jumpscare sound and slams the player's trauma
/// when entered. Place on an invisible trigger collider in the scene.
///
/// Setup:
///   - Attach to a GameObject with a Collider (Is Trigger checked).
///   - Assign the AudioSource with your jumpscare sound.
///   - Tune traumaAmount in the Inspector (0-1). 0.35 = a big hit.
/// </summary>
public class JumpScare : MonoBehaviour
{
    [Tooltip("AudioSource with the jumpscare sound clip.")]
    public AudioSource audioSource;

    [Tooltip("How much trauma to add when triggered (0-1). 0.35 = intense hit.")]
    [Range(0f, 1f)]
    public float traumaAmount = 0.35f;

    private bool _hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasPlayed || !other.CompareTag("Player")) return;

        _hasPlayed = true;

        // Play the jumpscare sound
        audioSource?.Play();

        // Slam trauma hard
        TraumaBar.Instance?.AddTrauma(traumaAmount);
    }
}