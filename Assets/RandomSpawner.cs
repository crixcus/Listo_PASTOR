using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject prefab;
    public int spawnCount = 10;

    [Header("Radius Settings")]
    public float spawnRadius = 5f;
    public bool spawnOnGroundOnly = false; // If true, ignores Y randomness

    [Header("Scale Settings")]
    public Vector3 minScale = Vector3.one;
    public Vector3 maxScale = Vector3.one * 2f;
    public float scaleMultiplier = 1f;

    [Header("Randomization Toggles")]
    public bool randomizePosition = true;
    public bool randomizeScale = true;

    void Start()
    {
        SpawnObjects();
    }

    public void SpawnObjects()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject obj = Instantiate(prefab);

            // 🎯 Random Position within Radius
            if (randomizePosition)
            {
                Vector3 randomOffset;

                if (spawnOnGroundOnly)
                {
                    // Circle (XZ plane)
                    Vector2 circle = Random.insideUnitCircle * spawnRadius;
                    randomOffset = new Vector3(circle.x, 0f, circle.y);
                }
                else
                {
                    // Sphere (3D)
                    randomOffset = Random.insideUnitSphere * spawnRadius;
                }

                obj.transform.position = transform.position + randomOffset;
            }

            // 📏 Random Scale
            if (randomizeScale)
            {
                Vector3 randomScale = new Vector3(
                    Random.Range(minScale.x, maxScale.x),
                    Random.Range(minScale.y, maxScale.y),
                    Random.Range(minScale.z, maxScale.z)
                );

                obj.transform.localScale = randomScale * scaleMultiplier;
            }
        }
    }

    // 🟡 Visualize the radius in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}