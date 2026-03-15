using UnityEngine;
using UnityEngine.Playables;

public class PlayTimelineOnEnable : MonoBehaviour
{
    public PlayableDirector timeline;

    void OnEnable()
    {
        if (timeline != null)
        {
            timeline.Play();
        }
    }
}