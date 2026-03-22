using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DirtCleaner : MonoBehaviour
{
    public Camera cam;
    public float cleanSpeed = 1.5f; // how fast dirt is removed

    // Track objects currently being cleaned
    private Dictionary<Renderer, Coroutine> activeCleaning = new Dictionary<Renderer, Coroutine>();

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();

                if (rend != null)
                {
                    Material mat = rend.material;

                    if (mat.HasProperty("_DirtStrength"))
                    {
                        float dirt = mat.GetFloat("_DirtStrength");

                        if (dirt > 0f)
                        {
                            // Start cleaning if not already running
                            if (!activeCleaning.ContainsKey(rend))
                            {
                                Coroutine c = StartCoroutine(CleanDirt(rend, mat));
                                activeCleaning.Add(rend, c);
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator CleanDirt(Renderer rend, Material mat)
    {
        while (mat.GetFloat("_DirtStrength") > 0f)
        {
            float current = mat.GetFloat("_DirtStrength");

            // Gradually decrease
            current -= cleanSpeed * Time.deltaTime;
            current = Mathf.Clamp01(current);

            mat.SetFloat("_DirtStrength", current);

            yield return null;
        }

        // Ensure it's exactly zero
        mat.SetFloat("_DirtStrength", 0f);

        // Remove from active cleaning list
        activeCleaning.Remove(rend);
    }
}