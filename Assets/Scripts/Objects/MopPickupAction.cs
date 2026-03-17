using UnityEngine;
public class MopPickupAction : MonoBehaviour
{
    public MopTool heldMop;
    public GameObject worldMop;
    private static MopPickupAction instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (heldMop != null)
            heldMop.gameObject.SetActive(false);
    }

    public void PickupMop()
    {
        PlayerCleaning player = FindObjectOfType<PlayerCleaning>();
        if (player == null) return;

        player.EquipMop(heldMop);
        heldMop.gameObject.SetActive(true);
        worldMop.SetActive(false);
    }

    public static void ShowWorldMop()
    {
        if (instance == null) return;
        instance.heldMop.gameObject.SetActive(false);
        instance.worldMop.SetActive(true);
    }
}