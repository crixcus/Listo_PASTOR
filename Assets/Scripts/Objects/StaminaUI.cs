using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's stamina as a radial fill ring in the bottom right.
///
/// SETUP:
///   1. In your HUD Canvas create an empty GameObject → name it "StaminaUI"
///   2. Position it bottom right of the screen
///   3. Add these children:
///      - RingBG   : Image (circle sprite, grey — background ring)
///      - RingFill : Image → Image Type: Filled, Fill Method: Radial 360,
///                   Fill Origin: Top, clockwise
///      - Label    : TMP Text → "STAMINA", small, centered below ring
///   4. Add StaminaUI.cs to the StaminaUI GameObject
///   5. Assign ringFill and label in Inspector
/// </summary>
public class StaminaUI : MonoBehaviour
{
    public static StaminaUI Instance { get; private set; }

    [Header("Ring")]
    [Tooltip("Radial fill Image. Image Type: Filled, Fill Method: Radial 360.")]
    public Image ringFill;

    [Tooltip("Color at full stamina.")]
    public Color fullColor = new Color(0.2f, 0.8f, 0.4f, 1f);

    [Tooltip("Color at low stamina / exhaustion.")]
    public Color exhaustedColor = new Color(0.9f, 0.3f, 0.2f, 1f);

    [Header("Label")]
    public TMP_Text label;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Updates ring fill and color based on current stamina (0-1).
    /// Called by StaminaSystem every time stamina changes.
    /// </summary>
    public void UpdateUI(float stamina)
    {
        if (ringFill == null) return;

        ringFill.fillAmount = stamina;
        ringFill.color = Color.Lerp(exhaustedColor, fullColor, stamina);
    }
}