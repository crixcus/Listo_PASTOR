using UnityEngine;

public class PushObject : MonoBehaviour
{
    public float pushForce = 10f;

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.collider.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Get direction from this object to the other object
            Vector3 pushDirection = collision.transform.position - transform.position;
            pushDirection.y = 0f; // optional: keep push horizontal

            rb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
        }
    }
}