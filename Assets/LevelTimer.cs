using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private float timer = 300f; // seconds before activation
    [SerializeField] private GameObject objectToActivate;

    private float currentTime;

    void Start()
    {
        currentTime = timer;

        if (objectToActivate != null)
            objectToActivate.SetActive(false); // make sure it starts disabled
    }

    void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            if (objectToActivate != null)
                objectToActivate.SetActive(true);

            enabled = false; // stop the script after activation
        }
    }
}