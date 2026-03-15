using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelTimer2 : MonoBehaviour
{
    [SerializeField] private float delay = 5f;

    void Start()
    {
        StartCoroutine(ActivateAfterDelay());
    }

    IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene("Level 3");
    }
}