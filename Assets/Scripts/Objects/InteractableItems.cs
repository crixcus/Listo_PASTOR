using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class InteractableItems : MonoBehaviour
{
    public static int GlobalCounter { get; private set; } = 0;

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
    public int objCounter = 0;
    public GameObject pickupText;

    public GameObject playerHolder;

    private BasicMovements _basicMovements;
    private PlayerInteraction _playerInteraction;

    private void Start()
    {
        if (playerHolder != null)
        {
            _basicMovements = playerHolder.GetComponent<BasicMovements>();
            _playerInteraction = playerHolder.GetComponent<PlayerInteraction>();
        }

        outline = GetComponent<Outline>();
        DisableOutline();
    }

    // i-call ni player
    public void InteractItem()
    {
        if (onInteraction != null)
        {
            Debug.Log($"Interacted with: {gameObject.name}");
            objCounter++;
            GlobalCounter++;
            showPanel();
            onInteraction.Invoke();
            //if (objCounter == 11)
            //{
            //    SceneManager.LoadScene("Level 2");
            //}
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
        pickupText.SetActive(true);

        isPaused = false;

        if (_basicMovements != null) _basicMovements.enabled = true;
        if (_playerInteraction != null) _playerInteraction.enabled = true;
        LockCursor();

        itemPanel.SetActive(false);

        items.SetActive(true);

        if (objCounter > 10)
        {
            SceneManager.LoadScene("Level 2");
        }
    }

    public void Gather()
    {
        canGather = true;
    }

    public void showPanel()
    {
        pickupText.SetActive(false);
        itemPanel.SetActive (true);

        if (_basicMovements != null) _basicMovements.enabled = false;
        if (_playerInteraction != null) _playerInteraction.enabled = false;
        UnlockCursor();

        Time.timeScale = 0f;

        isPaused = true;

        Cursor.visible = true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
