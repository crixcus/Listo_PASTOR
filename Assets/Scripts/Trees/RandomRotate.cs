using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    [Header("Rotation Limits (Degrees)")]
    public Vector3 minRotation = new Vector3(-30f, -30f, -30f);
    public Vector3 maxRotation = new Vector3(30f, 30f, 30f);

    [Header("Rotation Settings")]
    public float changeInterval = 2f;   // Time before picking a new random rotation
    public float rotateSpeed = 3f;      // Higher = snappier, lower = smoother

    private Quaternion targetRotation;
    private float timer;

    void Start()
    {
        PickNewRandomRotation();
    }

    void Update()
    {
        // Smoothly rotate toward the target
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );

        // Timer to pick a new rotation
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            timer = 0f;
            PickNewRandomRotation();
        }
    }

    void PickNewRandomRotation()
    {
        Vector3 randomEuler = new Vector3(
            Random.Range(minRotation.x, maxRotation.x),
            Random.Range(minRotation.y, maxRotation.y),
            Random.Range(minRotation.z, maxRotation.z)
        );

        targetRotation = Quaternion.Euler(randomEuler);
    }
}
