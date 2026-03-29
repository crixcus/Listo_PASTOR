using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Scene2OutroTrigger : MonoBehaviour
{
    public PlayableDirector outroLevel2;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            outroLevel2.gameObject.SetActive(true);

            outroLevel2.Play();

        }
    }
}
