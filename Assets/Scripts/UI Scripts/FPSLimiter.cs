using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FPSLimiter : MonoBehaviour
{
    void Awake()
    {
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = 60;
    }
}
