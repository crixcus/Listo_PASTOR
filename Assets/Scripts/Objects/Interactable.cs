using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    // para sa outline component
    private Outline outline;

    // message na pwede mong gamitin sa UI
    public string message;

    // event na ita-trigger pag ginamit ang object
    public UnityEvent onInteraction;

    private void Start()
    {
        outline = GetComponent<Outline>();
        DisableOutline();
    }

    // i-call ni player
    public void Interact()
    {
        if (onInteraction != null)
        {
            onInteraction.Invoke();
        }
    }

    public void EnableOutline()
    {
        if (outline != null)
            outline.enabled = true;
    }

    public void DisableOutline()
    {
        if (outline != null)
            outline.enabled = false;
    }
}
