using UnityEngine;
public class MopPickupAction : MonoBehaviour
{
    public MopTool heldMop;      // Mop (1)
    public GameObject worldMop;  // Mop

    public void PickupMop()
    {
        PlayerCleaning player = FindObjectOfType<PlayerCleaning>();
        if (player == null)
        {
            Debug.Log("PlayerCleaning not found!");
            return;
        }

        player.EquipMop(heldMop);
        heldMop.gameObject.SetActive(true);
        worldMop.SetActive(false);
        Debug.Log("Mop picked up!");
    }
}