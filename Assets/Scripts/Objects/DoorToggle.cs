using UnityEngine;

public class DoorToggle : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 3f;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(0f, openAngle, 0f) * closedRotation;
    }

    void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            target,
            Time.deltaTime * speed
        );
    }

    // CALLED by Interactable
    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }
}
