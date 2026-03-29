using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger zone on the upper floor tracking how many SimpleStackable
/// objects are inside. Shows/hides progress UI when player enters/exits.
///
/// To change the number of items — just update totalItems here.
/// The UI dots regenerate automatically. No hierarchy changes needed.
/// </summary>
public class SafeZone : MonoBehaviour
{

    private HashSet<SimpleStackable> _itemsInside = new HashSet<SimpleStackable>();
    public static SafeZone Instance { get; private set; }

    [Tooltip("Total stackable items in the scene. Change freely — UI updates automatically.")]
    public int totalItems = 5;

    public int SavedCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        StackProgressUI.Instance?.UpdateUI(0, totalItems);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StackProgressUI.Instance?.SetVisible(true);
            return;
        }

        var item = other.GetComponentInParent<SimpleStackable>();
        if (item == null) return;

        // Prevent duplicate counting
        if (_itemsInside.Contains(item)) return;

        _itemsInside.Add(item);

        SavedCount = _itemsInside.Count;
        StackProgressUI.Instance?.UpdateUI(SavedCount, totalItems);

        if (SavedCount >= totalItems)
            NotificationSystem.Instance?.ShowNotification("All items saved!");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StackProgressUI.Instance?.SetVisible(false);
            return;
        }

        var item = other.GetComponentInParent<SimpleStackable>();
        if (item == null) return;

        if (_itemsInside.Contains(item))
            _itemsInside.Remove(item);

        SavedCount = _itemsInside.Count;
        StackProgressUI.Instance?.UpdateUI(SavedCount, totalItems);
    }
}