using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MemoryPlacementController : MonoBehaviour
{
    private enum PlacementPhase
    {
        Initial,
        Suspended,
        Final,
        Completed
    }

    [Header("Required Memories")]
    [FormerlySerializedAs("requiredItems")]
    [SerializeField] private List<MemoryPlacementItem> initialRequiredItems = new();
    [SerializeField] private List<MemoryPlacementItem> finalRequiredItems = new();
    [SerializeField] private Transform memoryRoomItemsRoot;

    [Header("Initial Memory Room Flow")]
    [SerializeField] private AssistantController assistantController;
    [SerializeField, Min(1)] private int missingMemoryCueCount = 2;
    [SerializeField] private ClownController clownController;

    [Header("Confirmation")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string incompleteMessage =
        "Not all memory items have been placed. Please finish placing them and try again.";

    private readonly Dictionary<MemoryPlacementItem, MemoryPlacementZoneType> itemZones = new();
    private readonly Dictionary<MemoryPlacementItem, MemoryPlacementZoneType> savedInitialZones = new();
    private PlacementPhase phase = PlacementPhase.Initial;
    private bool missingMemoryCueStarted;
    private bool missingMemoryCueComplete;
    private bool crisisPending;
    private bool initialTransformsSaved;
    private GameStateController finalFlowController;

    public IReadOnlyList<MemoryPlacementItem> FinalRequiredItems => finalRequiredItems;

    private void Awake()
    {
        if (memoryRoomItemsRoot == null)
            memoryRoomItemsRoot = transform.root;

        HideFeedback();
    }

    public void SetItemZone(MemoryPlacementItem item, MemoryPlacementZoneType zoneType)
    {
        if (!IsRequiredInCurrentPhase(item))
            return;

        itemZones[item] = zoneType;
        HideFeedback();

        if (phase == PlacementPhase.Initial)
            EvaluateInitialMemoryRoomProgress();
    }

    public void ClearItemZone(MemoryPlacementItem item, MemoryPlacementZoneType zoneType)
    {
        if (!IsRequiredInCurrentPhase(item))
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
        if (phase != PlacementPhase.Final)
            return;

        if (!AreAllItemsPlaced(finalRequiredItems))
        {
            ShowFeedback(incompleteMessage);
            return;
        }

        phase = PlacementPhase.Completed;
        HideFeedback();

        if (finalFlowController != null)
            finalFlowController.HandleFinalPlacementConfirmed();
        else if (assistantController != null)
            assistantController.PlayConfirmationResponse();
        else
            Debug.LogWarning("MemoryPlacementController: final flow references are not assigned.", this);
    }

    public bool TryGetFinalPlacementZone(
        MemoryPlacementItem item,
        out MemoryPlacementZoneType zoneType)
    {
        if (item != null
            && itemZones.TryGetValue(item, out zoneType)
            && zoneType != MemoryPlacementZoneType.None)
        {
            return true;
        }

        zoneType = MemoryPlacementZoneType.None;
        return false;
    }

    public void BeginFinalPlacement(GameStateController gameStateController)
    {
        finalFlowController = gameStateController;

        if (phase == PlacementPhase.Final || phase == PlacementPhase.Completed)
            return;

        itemZones.Clear();

        foreach (KeyValuePair<MemoryPlacementItem, MemoryPlacementZoneType> placement in savedInitialZones)
        {
            if (placement.Key != null && finalRequiredItems.Contains(placement.Key))
                itemZones[placement.Key] = placement.Value;
        }

        phase = PlacementPhase.Final;

        foreach (MemoryPlacementItem item in initialRequiredItems)
        {
            if (item != null)
                item.RestoreAfterRoomReturn();
        }

        HideFeedback();
    }

    public void SaveInitialItemTransformsForRoomExit()
    {
        if (phase != PlacementPhase.Suspended || initialTransformsSaved)
            return;

        foreach (MemoryPlacementItem item in initialRequiredItems)
        {
            if (item != null)
                item.SaveForRoomReturn(memoryRoomItemsRoot);
        }

        initialTransformsSaved = true;
    }

    private bool AreAllItemsPlaced(List<MemoryPlacementItem> requiredItems)
    {
        if (requiredItems == null || requiredItems.Count == 0)
            return false;

        foreach (MemoryPlacementItem item in requiredItems)
        {
            if (item == null || !itemZones.ContainsKey(item)
                || itemZones[item] == MemoryPlacementZoneType.None)
                return false;
        }

        return true;
    }

    private void EvaluateInitialMemoryRoomProgress()
    {
        if (phase != PlacementPhase.Initial
            || initialRequiredItems == null
            || initialRequiredItems.Count == 0)
            return;

        int placedCount = GetPlacedItemCount(initialRequiredItems);
        int cueCount = Mathf.Clamp(missingMemoryCueCount, 1, initialRequiredItems.Count);

        if (!missingMemoryCueStarted && placedCount >= cueCount)
        {
            missingMemoryCueStarted = true;

            if (assistantController != null)
            {
                assistantController.PlayMissingPainfulMemories(OnMissingMemoryCueComplete);
            }
            else
            {
                missingMemoryCueComplete = true;
            }
        }

        if (AreAllItemsPlaced(initialRequiredItems))
        {
            crisisPending = true;
            PreserveInitialZoneAssignments();
            HideFeedback();
            TryTriggerPendingCrisis();
        }
    }

    private int GetPlacedItemCount(List<MemoryPlacementItem> requiredItems)
    {
        int count = 0;

        foreach (MemoryPlacementItem item in requiredItems)
        {
            if (item != null && itemZones.TryGetValue(item, out MemoryPlacementZoneType zoneType)
                && zoneType != MemoryPlacementZoneType.None)
            {
                count++;
            }
        }

        return count;
    }

    private bool IsRequiredInCurrentPhase(MemoryPlacementItem item)
    {
        if (item == null)
            return false;

        if (phase == PlacementPhase.Initial)
            return initialRequiredItems.Contains(item);

        if (phase == PlacementPhase.Final)
            return finalRequiredItems.Contains(item);

        return false;
    }

    private void PreserveInitialZoneAssignments()
    {
        phase = PlacementPhase.Suspended;
        savedInitialZones.Clear();

        foreach (MemoryPlacementItem item in initialRequiredItems)
        {
            if (item == null)
                continue;

            if (itemZones.TryGetValue(item, out MemoryPlacementZoneType zoneType)
                && zoneType != MemoryPlacementZoneType.None)
            {
                savedInitialZones[item] = zoneType;
            }
        }
    }

    private void OnMissingMemoryCueComplete()
    {
        missingMemoryCueComplete = true;
        TryTriggerPendingCrisis();
    }

    private void TryTriggerPendingCrisis()
    {
        if (!crisisPending || (missingMemoryCueStarted && !missingMemoryCueComplete))
            return;

        crisisPending = false;

        if (clownController != null)
        {
            clownController.TriggerCrisis();
        }
        else
        {
            Debug.LogError("MemoryPlacementController: ClownController is not assigned.", this);
        }
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
