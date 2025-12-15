using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Objective : MonoBehaviour
{
    public TMP_Text objectiveText;

    [Header("Required Items")]
    public List<Item> requiredItems;

    private Inventory inventory;

    void Start()
    {
        inventory = Inventory.instance;
        UpdateObjectives();
    }

    void Update()
    {
        UpdateObjectives();
    }

    void UpdateObjectives()
    {
        string text = "OBJECTIVES\n";

        foreach (Item item in requiredItems)
        {
            if (inventory.items.Contains(item))
                text += "[X] " + item.itemName + "\n";
            else
                text += "[ ] " + item.itemName + "\n";
        }

        objectiveText.text = text;
    }

}
