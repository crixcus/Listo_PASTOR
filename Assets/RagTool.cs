using UnityEngine;

/// <summary>
/// Attached to the rag GameObject. Handles cleaning logic, animation,
/// and wiping sound playback. Applies stamina multiplier from StaminaSystem
/// to reduce cleaning effectiveness when the player is exhausted.
/// </summary>
public class RagTool : MonoBehaviour
{
    [Header("Cleaning")]
    public float cleaningSpeed = 0.3f;

    [Header("Audio")]
    [Tooltip("AudioSource that plays the wiping sound. Enable Loop on the AudioSource.")]
    public AudioSource ragAudio;

    private Animator _animator;
    private bool _isCleaning;

    /// <summary>Whether the player is currently wiping. Read by StaminaSystem.</summary>
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
    /// Sets the cleaning state — drives animator and rag audio.
    /// Called by PlayerCleaning when player starts or stops wiping.
    /// </summary>
    public void SetCleaning(bool isCleaning)
    {
        if (_isCleaning == isCleaning) return;
        _isCleaning = isCleaning;

        if (_animator != null)
            _animator.SetBool("isCleaning", isCleaning);

        if (ragAudio == null) return;

        if (isCleaning && !ragAudio.isPlaying)
            ragAudio.Play();
        else if (!isCleaning && ragAudio.isPlaying)
            ragAudio.Stop();
    }
}