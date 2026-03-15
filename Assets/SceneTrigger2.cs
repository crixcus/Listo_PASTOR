using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class sceneTrigger2 : MonoBehaviour
{
    public GameObject timelineObject;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timelineObject.SetActive(true);
        }

        SceneManager.LoadScene("Level 2");
    }
}
