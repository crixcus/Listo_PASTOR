using UnityEngine;

/// <summary>
/// Attached to the mop GameObject. Handles cleaning logic, animation,
/// and mopping sound playback. Applies stamina multiplier from StaminaSystem
/// to reduce cleaning effectiveness when the player is exhausted.
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

    /// <summary>Whether the player is currently mopping. Read by StaminaSystem.</summary>
    public bool IsCleaning => _isCleaning;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Cleans the target CleanableObject. Applies stamina multiplier so
    /// cleaning slows down when the player is exhausted.
    /// </summary>
    public void CleanTarget(CleanableObject target)
    {
        float multiplier = StaminaSystem.Instance != null
            ? StaminaSystem.Instance.GetCleanMultiplier()
            : 1f;

        target.Clean(cleaningSpeed * multiplier);
    }

    /// <summary>
    /// Sets the cleaning state — drives animator and mop audio.
    /// Called by PlayerCleaning when player starts or stops mopping.
    /// </summary>
    public void SetCleaning(bool isCleaning)
    {
        if (_isCleaning == isCleaning) return;
        _isCleaning = isCleaning;

        if (_animator != null)
            _animator.SetBool("isCleaning", isCleaning);

        if (mopAudio == null) return;

        if (isCleaning && !mopAudio.isPlaying)
            mopAudio.Play();
        else if (!isCleaning && mopAudio.isPlaying)
            mopAudio.Stop();
    }
}