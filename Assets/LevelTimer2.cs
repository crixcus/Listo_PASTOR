using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelTimer2 : MonoBehaviour
{
    public GameObject csTrigger;

    [SerializeField] private float delay = 5f;

    void Start()
    {
        if (!csTrigger.activeInHierarchy)
            StartCoroutine(ActivateAfterDelay());
    }

    IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        csTrigger.SetActive(true);
    }

    public void ActivateEarly()
    {
        csTrigger.SetActive(true);
    }
}