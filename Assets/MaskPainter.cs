using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MaskPainter : MonoBehaviour
{
    public Camera cam;
    public Texture2D maskTexture;
    public Material material;

    [Header("Drip Settings")]
    public float dripSpeed = 50f;
    public float minDripLifeTime = 5f;
    public float maxDripLifeTime = 15f;

    [Header("Brush Settings")]
    public float minBrushRadius = 5f;
    public float maxBrushRadius = 15f;

    [Header("Paint Delay (Visual Only)")]
    public float paintDelay = 0.1f;

    [Header("Progress Settings")]
    [Range(0f, 1f)]
    public float completionThreshold = 0.8f;
    public Slider progressBar;


    [Header("Cleaning Range")]
    public float cleaningRange = 2.5f;

    public static bool IsPainting { get; private set; }


    private float initialWhiteAmount;
    private float currentWhiteAmount;

    class Drip
    {
        public Vector2 position;
        public float lifetime;
    }

    class PaintJob
    {
        public int x;
        public int y;
        public float executeTime;
    }

    List<Drip> drips = new List<Drip>();
    List<PaintJob> paintQueue = new List<PaintJob>();

    // ✅ Prevent duplicate paint jobs
    HashSet<Vector2Int> queuedPixels = new HashSet<Vector2Int>();

    void Start()
    {
        Texture2D newMask = new Texture2D(maskTexture.width, maskTexture.height, TextureFormat.RGBA32, false);
        newMask.SetPixels(maskTexture.GetPixels());
        newMask.Apply();

        maskTexture = newMask;
        material.SetTexture("_MaskTexture", maskTexture);

        initialWhiteAmount = CountWhitePixels();
        currentWhiteAmount = initialWhiteAmount;
    }

    void Update()
    {
        HandlePainting();
        UpdateDrips();
        ProcessPaintQueue();
        UpdateProgressUI();

        maskTexture.Apply();
    }

    void UpdateProgressUI()
    {
        float progress = GetCleaningProgress();
        progressBar.value = progress;
    }

    float CountWhitePixels()
    {
        Color[] pixels = maskTexture.GetPixels();
        float count = 0f;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].grayscale > 0.9f)
                count++;
        }

        return count;
    }

    public float GetCleaningProgress()
    {
        float cleaned = initialWhiteAmount - currentWhiteAmount;
        float progress = cleaned / initialWhiteAmount;
        float normalized = progress / completionThreshold;

        return Mathf.Clamp01(normalized);
    }


    void HandlePainting()
    {
        MopPickupAction mopAction = FindObjectOfType<MopPickupAction>(true);
        Debug.Log($"mopAction: {mopAction}, heldMop active: {mopAction?.heldMop.gameObject.activeSelf}, IsMaxDirty: {DirtAccumulate.IsMaxDirty}");
        if (mopAction == null || !mopAction.heldMop.gameObject.activeSelf)
        {
            IsPainting = false;
            return;
        }

        if (Input.GetKey(KeyCode.E) && !DirtAccumulate.IsMaxDirty)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, cleaningRange) && hit.collider.gameObject == gameObject)
            {
                IsPainting = true;
                mopAction.heldMop.SetCleaning(true);
                Vector2 uv = hit.textureCoord;
                float randomRadius = Random.Range(minBrushRadius, maxBrushRadius);
                Paint(uv, randomRadius);
                return;
            }
        }

        IsPainting = false;
        mopAction?.heldMop.SetCleaning(false);
    }

    void Paint(Vector2 uv, float radius)
    {
        int x = (int)(uv.x * maskTexture.width);
        int y = (int)(uv.y * maskTexture.height);

        int r = Mathf.RoundToInt(radius);

        for (int i = -r; i < r; i++)
        {
            for (int j = -r; j < r; j++)
            {
                int px = x + i;
                int py = y + j;

                if (px >= 0 && px < maskTexture.width &&
                    py >= 0 && py < maskTexture.height)
                {
                    float dist = Vector2.Distance(new Vector2(px, py), new Vector2(x, y));

                    if (dist <= radius)
                    {
                        Color current = maskTexture.GetPixel(px, py);

                        if (current.grayscale <= 0.1f)
                            continue;

                        Vector2Int pixelKey = new Vector2Int(px, py);

                        // ✅ Prevent duplicate queue entries
                        if (!queuedPixels.Contains(pixelKey))
                        {
                            queuedPixels.Add(pixelKey);

                            paintQueue.Add(new PaintJob
                            {
                                x = px,
                                y = py,
                                executeTime = Time.time + paintDelay
                            });

                            // Drip chance
                            if (Random.value < 0.05f)
                            {
                                drips.Add(new Drip
                                {
                                    position = new Vector2(px, py),
                                    lifetime = Random.Range(minDripLifeTime, maxDripLifeTime)
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    void ProcessPaintQueue()
    {
        for (int i = paintQueue.Count - 1; i >= 0; i--)
        {
            if (Time.time >= paintQueue[i].executeTime)
            {
                int x = paintQueue[i].x;
                int y = paintQueue[i].y;

                Color current = maskTexture.GetPixel(x, y);

                // ✅ ONLY count when actually turning white → black
                if (current.grayscale > 0.9f)
                {
                    currentWhiteAmount--;
                }

                maskTexture.SetPixel(x, y, Color.black);

                // ✅ Remove from tracking
                queuedPixels.Remove(new Vector2Int(x, y));
                paintQueue.RemoveAt(i);
            }
        }
    }

    void UpdateDrips()
    {
        for (int i = drips.Count - 1; i >= 0; i--)
        {
            Drip drip = drips[i];

            drip.lifetime -= Time.deltaTime;

            if (drip.lifetime <= 0)
            {
                drips.RemoveAt(i);
                continue;
            }

            drip.position.y -= dripSpeed * Time.deltaTime;

            int px = (int)drip.position.x;
            int py = (int)drip.position.y;

            if (px >= 0 && px < maskTexture.width &&
                py >= 0 && py < maskTexture.height)
            {
                Color current = maskTexture.GetPixel(px, py);

                if (current.grayscale > 0.9f)
                {
                    currentWhiteAmount--;
                    maskTexture.SetPixel(px, py, Color.black);
                }
            }
        }
    }
}