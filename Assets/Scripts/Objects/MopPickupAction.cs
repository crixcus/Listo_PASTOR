using UnityEngine;
using UnityEngine.Animations.Rigging;

public class MopPickupAction : MonoBehaviour
{
    [Header("Mop")]
    public MopTool heldMop;
    public GameObject worldMop;
    public Rig mopRig;
    public bool mopPickup = false;
    public bool mopEquipped = false;

    [Header("Rag")]
    public RagTool heldRag;
    public GameObject worldRag;
    public Rig ragRig;
    public bool ragPickup = false;


    private static MopPickupAction instance;

    public MopTool HeldMop => heldMop;
    public RagTool HeldRag => heldRag;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (heldMop != null) heldMop.gameObject.SetActive(false);
        if (heldRag != null) heldRag.gameObject.SetActive(false);
        SetRigWeight(mopRig, 0f);
        SetRigWeight(ragRig, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && mopPickup) EquipMop();
        if (Input.GetKeyDown(KeyCode.Q) && ragPickup) EquipRag();
    }

    // --- Mop ---

    public void PickupMop()
    {
        mopPickup = true;
        PlayerCleaning player = FindObjectOfType<PlayerCleaning>();
        if (player == null) return;

        player.EquipMop(heldMop);
        EquipMop();
        worldMop.SetActive(false);
    }

    void EquipMop()
    {
        mopEquipped = true;
        if (heldMop == null) return;

        Debug.Log("EquipMop called");
        heldMop.gameObject.SetActive(true);
        if (heldRag != null) heldRag.gameObject.SetActive(false);
        SetRigWeight(mopRig, 1f);
        SetRigWeight(ragRig, 0f);
        Debug.Log($"mopRig weight: {mopRig?.weight} | ragRig weight: {ragRig?.weight}");
        Debug.Log($"heldMop active: {heldMop.gameObject.activeSelf} | heldRag active: {heldRag?.gameObject.activeSelf}");
    }

    // --- Rag ---

    public void PickupRag()
    {
        ragPickup = true;
        PlayerCleaning player = FindObjectOfType<PlayerCleaning>();
        if (player == null) return;

        player.EquipRag(heldRag);
        EquipRag();
        if (worldRag != null) worldRag.SetActive(false);
    }

    void EquipRag()
    {
        mopEquipped = false;
        if (heldRag == null) return;

        Debug.Log("EquipRag called");
        heldRag.gameObject.SetActive(true);
        if (heldMop != null) heldMop.gameObject.SetActive(false);
        SetRigWeight(ragRig, 1f);
        SetRigWeight(mopRig, 0f);
        Debug.Log($"ragRig weight: {ragRig?.weight} | mopRig weight: {mopRig?.weight}");
        Debug.Log($"heldRag active: {heldRag.gameObject.activeSelf} | heldMop active: {heldMop?.gameObject.activeSelf}");
    }

    // --- Rig helper ---

    void SetRigWeight(Rig rig, float weight)
    {
        if (rig != null) rig.weight = weight;
    }

    // --- Static helpers ---

    public static void ShowWorldMop()
    {
        if (instance == null) return;
        instance.heldMop.gameObject.SetActive(false);
        instance.worldMop.SetActive(true);
        instance.SetRigWeight(instance.mopRig, 0f);
    }

    public static void ShowWorldRag()
    {
        if (instance == null) return;
        if (instance.heldRag != null) instance.heldRag.gameObject.SetActive(false);
        if (instance.worldRag != null) instance.worldRag.SetActive(true);
        instance.SetRigWeight(instance.ragRig, 0f);
    }
}