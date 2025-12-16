using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class InteractableItems : MonoBehaviour
{
    // para sa outline component
    private Outline outline;

    // message na pwede mong gamitin sa UI
    public string message;

    // event na ita-trigger pag ginamit ang object
    public UnityEvent onInteraction;

    public bool isPaused;
    public GameObject itemPanel;
    public bool canGather = true;
    public GameObject items;
    public GameObject Bag;

    private void Start()
    {
        outline = GetComponent<Outline>();
        DisableOutline();
    }

    // i-call ni player
    public void InteractItem()
    {
        if (onInteraction != null)
        {
            Debug.Log($"Interacted with: {gameObject.name}");
            showPanel();
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

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        itemPanel.SetActive(false);

        items.SetActive(true);
    }

    public void Gather()
    {
        canGather = true;
    }

    public void showPanel()
    {
        itemPanel.SetActive (true);

        Time.timeScale = 0f;

        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
