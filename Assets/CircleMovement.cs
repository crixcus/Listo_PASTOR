using UnityEngine;

public class CircleMovement : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up; // Axis to rotate on
    public float rotationSpeed = 90f;          // Degrees per second

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
