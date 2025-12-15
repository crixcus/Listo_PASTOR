using UnityEngine;

public class PickUpBlock : MonoBehaviour
{
    public GameObject pickUpText;
    public Item item;

    bool playerInRange;

    void Start()
    {
        pickUpText.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Inventory.instance.AddItem(item);
            pickUpText.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            pickUpText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            pickUpText.SetActive(false);
        }
    }
}
