using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class TraumaBar : MonoBehaviour
{
    int experience;
    int traumaLimit;

    public Slider traumaBar;
    public GameOverPanel gameOver;

    // Use this for initialization
    void Start()
    {
        experience = 0;
        traumaLimit = 500;

        traumaBar.value = experience;
        traumaBar.maxValue = traumaLimit;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            experience += 10;
            traumaBar.value = experience;
        }

        if (experience >= 500)
        {
            gameOver.TriggerGameOver();
        }
    }
}