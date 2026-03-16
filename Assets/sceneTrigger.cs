using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class sceneTrigger : MonoBehaviour
{
    public GameObject timelineObject;
    public GameObject trigger;
    public float timer = 300f;
    public GameObject player;

    public void Start()
    {
        trigger.SetActive(false);
    }

    public void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            trigger.SetActive(true);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timelineObject.SetActive(true);
        }

        
    }
}
