using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floatero : MonoBehaviour
{
    public Rigidbody rb;
    public float depthBeforeSubmerged = 1f;
    public float displacementAmount = 3f;

    // Optional wave & water system
    public Transform waterObject;

    // Random movement
    public float floatSpeed = 1f; // How fast the object drifts
    public float floatChangeInterval = 2f; // How often it changes direction
    private Vector3 floatDirection;

    private bool isInWater = false;
    private float waterHeight = 0f;
    private float floatTimer = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
        }
    }

    private void FixedUpdate()
    {
        if (!isInWater) return; // Only float if touching water

        // --- Determine water height ---
        waterHeight = 0f;

        if (WaveManager.instance != null)
        {
            waterHeight = WaveManager.instance.GetWaveHeight(transform.position.x);
        }

        if (waterObject != null)
        {
            waterHeight = Mathf.Max(waterHeight, waterObject.position.y);
        }

        // --- Apply buoyancy ---
        float objectHeight = transform.position.y;

        if (objectHeight < waterHeight)
        {
            float submergedPercent = Mathf.Clamp01((waterHeight - objectHeight) / depthBeforeSubmerged);

            // Heavier objects stay lower by using mass automatically (mass affects force)
            float buoyancyForce = Mathf.Abs(Physics.gravity.y) * submergedPercent * displacementAmount;

            rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Acceleration);
        }

        // --- Apply random horizontal movement ---
        floatTimer += Time.fixedDeltaTime;
        if (floatTimer > floatChangeInterval)
        {
            // Pick a new random horizontal direction
            floatDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            floatTimer = 0f;
        }

        rb.AddForce(floatDirection * floatSpeed, ForceMode.Acceleration);
    }
}
