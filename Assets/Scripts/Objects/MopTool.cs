using UnityEngine;

/// <summary>
/// Attached to the mop GameObject. Handles cleaning logic, animation,
/// and mopping sound playback.
///
/// Setup:
///   - Assign mopAudio to a GameObject with an AudioSource (your mopping sound).
///   - Make sure the AudioSource has Loop checked for continuous mopping sound.
/// </summary>
public class MopTool : MonoBehaviour
{
    [Header("Cleaning")]
    public float cleaningSpeed = 0.3f;

    [Header("Audio")]
    [Tooltip("AudioSource that plays the mopping sound. Enable Loop on the AudioSource.")]
    public AudioSource mopAudio;

    private Animator _animator;
    private bool _isCleaning;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Cleans the target CleanableObject by the cleaning speed this frame.
    /// </summary>
    public void CleanTarget(CleanableObject target)
    {
        target.Clean(cleaningSpeed);
    }

    /// <summary>
    /// Sets the cleaning state — drives the animator and starts/stops the mop sound.
    /// Called by PlayerCleaning when the player starts or stops mopping.
    /// </summary>
    public void SetCleaning(bool isCleaning)
    {
        if (_isCleaning == isCleaning) return;
        _isCleaning = isCleaning;

        // Drive animation
        if (_animator != null)
            _animator.SetBool("isCleaning", isCleaning);

        // Drive audio
        if (mopAudio == null) return;

        if (isCleaning && !mopAudio.isPlaying)
            mopAudio.Play();
        else if (!isCleaning && mopAudio.isPlaying)
            mopAudio.Stop();
    }
}