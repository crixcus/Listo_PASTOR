using UnityEngine;

public class MopPickupAction : MonoBehaviour
{
    public MopTool mopTool;

    public void PickupMop()
    {
        PlayerCleaning player = FindObjectOfType<PlayerCleaning>();
        if (player == null) return;

        player.EquipMop(mopTool);

        mopTool.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}