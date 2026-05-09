using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CallForLoad : MonoBehaviour
{
    public static CallForLoad Instance;
        public bool loaded = false;

    // Start is called before the first frame update
    void Awake()
    {
        Instance  = this;
        DontDestroyOnLoad(gameObject);
        loaded = true;
        LevelManager.Instance.LoadScene("Level 2", "polskrin mo");
    }
}
