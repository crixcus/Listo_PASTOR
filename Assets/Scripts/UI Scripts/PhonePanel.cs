using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhonePanel : MonoBehaviour
{
    public GameObject tip1;
    public GameObject tip2;
    public GameObject tip3;
    public GameObject tip4;

    // Start is called before the first frame update
    void Start()
    {
        tip1.SetActive(true);
        tip2.SetActive(false); 
        tip3.SetActive(false);
        tip4.SetActive(false);
    }

    // Update is called once per frame
    public void Next1()
    {
        tip2.SetActive(true);
        tip1.SetActive(false);
        tip3.SetActive(false);
        tip4.SetActive(false);
    }

    public void Next2()
    {
        tip1.SetActive(false);
        tip2.SetActive(false);
        tip3.SetActive(true);
        tip4.SetActive(false);
    }

    public void Next3()
    {
        tip1.SetActive(false);
        tip2.SetActive(false);
        tip3.SetActive(false);
        tip4.SetActive(true);
    }

    public void Prev2()
    {
        tip1.SetActive(true);
        tip2.SetActive(false);
        tip3.SetActive(false);
        tip4.SetActive(false);
    }

    public void Prev3()
    {
        tip1.SetActive(false);
        tip2.SetActive(true);
        tip3.SetActive(false);
        tip4.SetActive(false);
    }

    public void Prev4()
    {
        tip1.SetActive(false);
        tip2.SetActive(false);
        tip3.SetActive(true);
        tip4.SetActive(false);
    }
}
