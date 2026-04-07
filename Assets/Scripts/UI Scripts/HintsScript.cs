using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintsScript : MonoBehaviour
{
    public GameObject hint1;
    public GameObject hint2;

    public int hintCount = 0;

    public bool hint1Done = false;
    public bool hint2Done = false;

    // Start is called before the first frame update
    void Start()
    {
        hint1.SetActive(false);
        hint2.SetActive(false);

        LockCursor();
    }

    public void Update()
    {
        if (hint1.activeInHierarchy || hint2.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LockCursor();
                if (hint1.activeInHierarchy)
                {
                    Continue();
                }
                else if (hint2.activeInHierarchy)
                {
                    Continue();
                }
                else
                {
                    return;
                }
            }
        }
    }

    public void Hint1Show()
    {
        if (hintCount <= 0)
        {
            hint1.SetActive(true);
            UnlockCursor();
            hintCount++;
            hint1Done = true;
        }
        else
        {
            return;
        }
    }

    public void Hint2Show()
    {
        if (hintCount == 1)
        {
            UnlockCursor();
            hint2.SetActive(true);
            hintCount++;
            hint2Done = true;
        }
        else
        {
            return;
        }
    }

    public void Continue()
    {
        hint1.SetActive(false);
        hint2.SetActive(false);
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
