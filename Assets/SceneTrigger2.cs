using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class sceneTrigger2 : MonoBehaviour
{
    public GameObject timelineObject;
    public float timer = 300f;
    public GameObject player;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.SetActive(false);
            timelineObject.SetActive(true);

            StartCoroutine(NextLevel());
            
        }
    }

    IEnumerator NextLevel()
    {
        yield return new WaitForSeconds(timer);
        player.SetActive(true);
        SceneManager.LoadScene("Level 2");
    }
}
