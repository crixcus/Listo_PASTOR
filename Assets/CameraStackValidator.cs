using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraStackValidator : MonoBehaviour
{
    void Start()
    {
        var camData = GetComponent<UniversalAdditionalCameraData>();
        if (camData == null)
        {
            Debug.LogError("CameraStackValidator: No UniversalAdditionalCameraData found on " + gameObject.name);
            return;
        }
        camData.cameraStack.RemoveAll(cam => cam == null);
        Debug.Log("Camera stack cleaned on " + gameObject.name);
    }
}