using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsPanel : MonoBehaviour
{
    public GameObject instPanel1;
    public GameObject instPanel2;
    public GameObject instPanel3;
    public GameObject playerHolder;

    private BasicMovements _basicMovements;
    private PlayerInteraction _playerInteraction;

    // Start is called before the first frame update
    void Start()
    {
        if (playerHolder != null)
        {
            _basicMovements = playerHolder.GetComponent<BasicMovements>();
            _playerInteraction = playerHolder.GetComponent<PlayerInteraction>();
        }
        StartCoroutine(InstructionPanels());
    }

    IEnumerator InstructionPanels()
    {
        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            yield return new WaitForSeconds(11f);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }
            
        if (_basicMovements != null) _basicMovements.enabled = false;
        if (_playerInteraction != null) _playerInteraction.enabled = false;
        UnlockCursor();
        instPanel1.SetActive(true);
        instPanel2.SetActive(false);
        instPanel3.SetActive(false);
        Time.timeScale = 0f;
    }

    public void NextPanel1()
    {
        instPanel1.SetActive(false);
        instPanel2.SetActive(true);
    }
    public void NextPanel2()
    {
        instPanel2.SetActive(false);
        instPanel3.SetActive(true);
    }
    public void Play()
    {
        instPanel3.SetActive(false);
        Time.timeScale = 1f;
        if (_basicMovements != null) _basicMovements.enabled = true;
        if (_playerInteraction != null) _playerInteraction.enabled = true;
        LockCursor();
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
