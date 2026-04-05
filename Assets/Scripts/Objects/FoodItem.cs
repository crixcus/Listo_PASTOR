using UnityEngine;
using UnityEngine.UI;

public class FoodItem : MonoBehaviour
{
    [Header("Food Settings")]
    [Range(0f, 1f)]
    public float staminaRestore = 0.6f;

    public float eatDuration = 4f;
    public string foodName = "Food";

    [Header("Raycast Settings")]
    public float interactRange = 3f;
    public LayerMask foodLayer;
    public Transform cameraTransform;

    [Header("Eat Progress Ring")]
    public Image eatProgressRing;
    public Color eatRingColor = new Color(0.9f, 0.7f, 0.2f, 1f);

    private bool _isEating = false;
    private bool _eaten = false;
    private float _eatTimer = 0f;

    void Start()
    {
        SetRingVisible(false);
    }

    void Update()
    {
        if (_eaten) return;

        bool isLookingAtThis = CheckIfLookingAtThis();

        if (isLookingAtThis)
        {
            if (Input.GetKey(KeyCode.F))
            {
                if (!_isEating)
                    StartEating();

                _eatTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_eatTimer / eatDuration);
                UpdateRing(progress);

                HUDController.instance?.EnableInteractionText(
                    $"Eating {foodName}... {Mathf.RoundToInt(progress * 100)}%");

                if (_eatTimer >= eatDuration)
                    CompleteEating();
            }
            else
            {
                if (_isEating)
                    CancelEating();

                HUDController.instance?.EnableInteractionText($"Hold E to eat {foodName}");
            }
        }
        else
        {
            if (_isEating)
                CancelEating();

            HUDController.instance?.DisableInteractionText();
        }
    }

    bool CheckIfLookingAtThis()
    {
        if (cameraTransform == null) return false;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, foodLayer))
        {
            return hit.collider.GetComponentInParent<FoodItem>() == this;
        }

        return false;
    }

    void StartEating()
    {
        _isEating = true;
        _eatTimer = 0f;
        SetRingVisible(true);
    }

    void CancelEating()
    {
        _isEating = false;
        _eatTimer = 0f;
        SetRingVisible(false);
        UpdateRing(0f);
    }

    void CompleteEating()
    {
        _eaten = true;
        _isEating = false;

        SetRingVisible(false);
        HUDController.instance?.DisableInteractionText();

        StaminaSystem.Instance?.RestoreStamina(staminaRestore);

        gameObject.SetActive(false);
    }

    void UpdateRing(float progress)
    {
        if (eatProgressRing == null) return;
        eatProgressRing.fillAmount = progress;
        eatProgressRing.color = eatRingColor;
    }

    void SetRingVisible(bool visible)
    {
        if (eatProgressRing == null) return;
        Color c = eatProgressRing.color;
        c.a = visible ? 1f : 0f;
        eatProgressRing.color = c;
        eatProgressRing.fillAmount = 0f;
    }
}