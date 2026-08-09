using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemoryPlacementController : MonoBehaviour
{
    [Header("Required memories")]
    [SerializeField] private List<MemoryPlacementItem> requiredItems = new();

    [Header("Confirmation")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string incompleteMessage =
        "Not all memory items have been placed. Please finish placing them and try again.";
    [SerializeField] private string successMessage = "Placement confirmed.";
    [SerializeField] private ClownController clownController;

    private readonly Dictionary<MemoryPlacementItem, MemoryPlacementZoneType> itemZones = new();
    private bool confirmed;

    private void Awake()
    {
        HideFeedback();
    }

    public void SetItemZone(MemoryPlacementItem item, MemoryPlacementZoneType zoneType)
    {
        if (!requiredItems.Contains(item) || confirmed)
            return;

        itemZones[item] = zoneType;
        HideFeedback();
    }

    public void ClearItemZone(MemoryPlacementItem item, MemoryPlacementZoneType zoneType)
    {
        if (confirmed)
            return;

        if (itemZones.TryGetValue(item, out MemoryPlacementZoneType currentZone)
            && currentZone == zoneType)
        {
            itemZones.Remove(item);
            HideFeedback();
        }
    }

    public void ConfirmPlacement()
    {
        if (confirmed)
            return;

        if (!AreAllItemsPlaced())
        {
            ShowFeedback(incompleteMessage);
            return;
        }

        confirmed = true;
        ShowFeedback(successMessage);

        if (clownController != null)
            clownController.TriggerCrisis();
    }

    public bool AreAllItemsPlaced()
    {
        if (requiredItems.Count == 0)
            return false;

        foreach (MemoryPlacementItem item in requiredItems)
        {
            if (item == null || !itemZones.ContainsKey(item)
                || itemZones[item] == MemoryPlacementZoneType.None)
                return false;
        }

        return true;
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
    }

    private void HideFeedback()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }
}
