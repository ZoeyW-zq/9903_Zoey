using System.Collections.Generic;
using UnityEngine;

public enum MemoryPlacementZoneType
{
    None,
    Focus,
    Context,
    Background
}

public class MemoryPlacementZone : MonoBehaviour
{
    [SerializeField] private MemoryPlacementController placementController;
    [SerializeField] private MemoryPlacementZoneType zoneType;

    private readonly Dictionary<MemoryPlacementItem, int> colliderCounts = new();

    private void OnTriggerEnter(Collider other)
    {
        MemoryPlacementItem item = other.GetComponentInParent<MemoryPlacementItem>();
        if (item == null || placementController == null)
            return;

        colliderCounts.TryGetValue(item, out int count);
        colliderCounts[item] = count + 1;

        if (count == 0)
            placementController.SetItemZone(item, zoneType);
    }

    private void OnTriggerExit(Collider other)
    {
        MemoryPlacementItem item = other.GetComponentInParent<MemoryPlacementItem>();
        if (item == null || !colliderCounts.TryGetValue(item, out int count))
            return;

        if (count > 1)
        {
            colliderCounts[item] = count - 1;
            return;
        }

        colliderCounts.Remove(item);
        placementController.ClearItemZone(item, zoneType);
    }

    private void OnDisable()
    {
        if (placementController == null)
            return;

        foreach (MemoryPlacementItem item in colliderCounts.Keys)
            placementController.ClearItemZone(item, zoneType);

        colliderCounts.Clear();
    }
}
