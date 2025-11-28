using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public GameObject crosshair;

    void Start()
    {
        if (crosshair != null)
            crosshair.SetActive(true); // or false to hide initially
    }
}
