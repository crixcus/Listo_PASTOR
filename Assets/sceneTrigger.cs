using UnityEngine;
using UnityEngine.Playables;

public class sceneTrigger : MonoBehaviour
{
    public GameObject timelineObject;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timelineObject.SetActive(true);
        }
    }
}
