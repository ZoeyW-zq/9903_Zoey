using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FinalReportController : MonoBehaviour
{
    [Header("Computer Pages")]
    [SerializeField] private GameObject screenContentRoot;
    [SerializeField] private ItemCycler itemCycler;
    [SerializeField, Min(0)] private int reportEntryPageIndex;
    [SerializeField] private GameObject finalReportPage;

    [Header("Buttons")]
    [SerializeField] private Button viewReportButton;
    [FormerlySerializedAs("backToReadyButton")]
    [SerializeField] private Button closeSessionButton;

    [Header("Report Text")]
    [SerializeField] private TMP_Text distributionSummaryText;
    [FormerlySerializedAs("focusItemsText")]
    [SerializeField] private TMP_Text outcomeText;

    [Header("Attention Label Colours")]
    [SerializeField] private Color backgroundAttentionColour = new Color32(86, 122, 142, 255);
    [SerializeField] private Color contextAttentionColour = new Color32(73, 127, 105, 255);
    [SerializeField] private Color focusAttentionColour = new Color32(166, 97, 76, 255);

    private MemoryPlacementController placementController;
    private System.Action closeSessionAction;
    private int finalReportPageIndex = -1;
    private bool closeSessionRequested;

    private void Awake()
    {
        if (viewReportButton != null)
            viewReportButton.onClick.AddListener(ShowReport);

        if (closeSessionButton != null)
            closeSessionButton.onClick.AddListener(RequestCloseSession);

        SetViewReportButtonVisible(false);

        if (finalReportPage != null)
            finalReportPage.SetActive(false);
    }

    public void PrepareReportReady(
        MemoryPlacementController source,
        System.Action onCloseSession)
    {
        placementController = source;
        closeSessionAction = onCloseSession;
        closeSessionRequested = false;

        if (closeSessionButton != null)
            closeSessionButton.interactable = true;

        if (screenContentRoot != null)
            screenContentRoot.SetActive(true);

        RegisterFinalReportPage();
        BuildReport();
        SetViewReportButtonVisible(true);
        SelectPage(reportEntryPageIndex);
    }

    private void ShowReport()
    {
        SelectPage(finalReportPageIndex);
    }

    private void RequestCloseSession()
    {
        if (closeSessionRequested)
            return;

        if (closeSessionAction == null)
        {
            Debug.LogWarning("FinalReportController: close-session action is not available.", this);
            return;
        }

        closeSessionRequested = true;

        if (closeSessionButton != null)
            closeSessionButton.interactable = false;

        closeSessionAction.Invoke();
    }

    private void BuildReport()
    {
        if (placementController == null)
        {
            Debug.LogWarning("FinalReportController: MemoryPlacementController is not available.", this);
            return;
        }

        List<string> outcomes = new();
        int focusCount = 0;
        int contextCount = 0;
        int backgroundCount = 0;

        IReadOnlyList<MemoryPlacementItem> finalItems = placementController.FinalRequiredItems;
        for (int i = 0; i < finalItems.Count; i++)
        {
            MemoryPlacementItem item = finalItems[i];
            if (item == null
                || !placementController.TryGetFinalPlacementZone(item, out MemoryPlacementZoneType zoneType))
            {
                continue;
            }

            string displayName = GetDisplayName(item.memoryId);
            switch (zoneType)
            {
                case MemoryPlacementZoneType.Focus:
                    focusCount++;
                    break;

                case MemoryPlacementZoneType.Context:
                    contextCount++;
                    break;

                case MemoryPlacementZoneType.Background:
                    backgroundCount++;
                    break;
            }

            outcomes.Add(FormatOutcome(displayName, item.memoryId, zoneType));
        }

        int assignedCount = focusCount + contextCount + backgroundCount;
        SetText(
            distributionSummaryText,
            $"{assignedCount}/{finalItems.Count} MEMORIES PROCESSED\n"
            + $"FOCUS {focusCount}  |  CONTEXT {contextCount}  |  BACKGROUND {backgroundCount}"
        );
        SetText(
            outcomeText,
            outcomes.Count > 0
                ? string.Join("\n\n", outcomes)
                : "No memory outcomes are available."
        );
    }

    private string FormatOutcome(
        string displayName,
        string memoryId,
        MemoryPlacementZoneType zoneType)
    {
        string zoneLabel = zoneType.ToString().ToUpperInvariant();
        string zoneColour = ColorUtility.ToHtmlStringRGB(GetAttentionColour(zoneType));
        string outcome = GetOutcome(memoryId, zoneType);

        return $"<b>{displayName}</b>  <color=#{zoneColour}><b>{zoneLabel}</b></color>\n{outcome}";
    }

    private void SelectPage(int pageIndex)
    {
        if (itemCycler == null)
        {
            Debug.LogWarning("FinalReportController: ItemCycler is not assigned.", this);
            return;
        }

        if (itemCycler.items == null || pageIndex < 0 || pageIndex >= itemCycler.items.Length)
        {
            Debug.LogWarning(
                $"FinalReportController: page index {pageIndex} is outside the ItemCycler items list.",
                this
            );
            return;
        }

        itemCycler.SelectItem(pageIndex);
    }

    private void RegisterFinalReportPage()
    {
        if (itemCycler == null)
            return;

        List<GameObject> pages = itemCycler.items != null
            ? new List<GameObject>(itemCycler.items)
            : new List<GameObject>();

        finalReportPageIndex = RegisterPage(pages, finalReportPage);
        itemCycler.items = pages.ToArray();
    }

    private static int RegisterPage(List<GameObject> pages, GameObject page)
    {
        if (page == null)
            return -1;

        int existingIndex = pages.IndexOf(page);
        if (existingIndex >= 0)
            return existingIndex;

        pages.Add(page);
        return pages.Count - 1;
    }

    private void SetViewReportButtonVisible(bool visible)
    {
        if (viewReportButton != null)
            viewReportButton.gameObject.SetActive(visible);
    }

    private Color GetAttentionColour(MemoryPlacementZoneType zoneType)
    {
        switch (zoneType)
        {
            case MemoryPlacementZoneType.Focus:
                return focusAttentionColour;
            case MemoryPlacementZoneType.Context:
                return contextAttentionColour;
            case MemoryPlacementZoneType.Background:
                return backgroundAttentionColour;
            default:
                return Color.white;
        }
    }

    private static string GetDisplayName(string memoryId)
    {
        switch (memoryId)
        {
            case "bottle":
                return "Water Bottle";
            case "sunsetPhoto":
                return "Sunset Photograph";
            case "legoBricks":
                return "LEGO Bricks";
            case "clock":
                return "Old Alarm Clock";
            case "pen":
                return "Red Correction Pen";
            case "medal":
                return "Second-Place Medal";
            case "phone":
                return "Old Phone";
            default:
                return string.IsNullOrWhiteSpace(memoryId) ? "Unnamed Memory" : memoryId;
        }
    }

    private static string GetOutcome(string memoryId, MemoryPlacementZoneType zoneType)
    {
        switch (memoryId)
        {
            case "bottle":
                return SelectOutcome(
                    zoneType,
                    "Work-while-ill distress recedes, but physical warning signs may be overlooked.",
                    "Illness and overwork are recognized earlier, supporting rest, medication, and help-seeking.",
                    "Pain and work pressure remain highly active, reinforcing fear and resentment."
                );

            case "sunsetPhoto":
                return SelectOutcome(
                    zoneType,
                    "The peaceful memory remains available, but work may continue to overshadow restorative moments.",
                    "The memory supports occasional pauses without abandoning responsibilities.",
                    "Restorative moments become an active part of daily life beyond work and productivity."
                );

            case "legoBricks":
                return SelectOutcome(
                    zoneType,
                    "The achievement remains accessible, but external judgment may still dominate self-worth.",
                    "Private achievement balances criticism without turning the hobby into another performance test.",
                    "Patient creation becomes a stable source of satisfaction and self-worth."
                );

            case "medal":
                return SelectOutcome(
                    zoneType,
                    "Parental disappointment loses influence, but the achievement may also feel less personally meaningful.",
                    "Effort can be valued without needing first place to prove personal worth.",
                    "Ranking and disappointment remain active, keeping achievement closely tied to self-worth."
                );

            case "clock":
                return SelectOutcome(
                    zoneType,
                    "Guilt about rest fades, but signs of exhaustion may also receive less attention.",
                    "Physical limits are recognized, and rest becomes part of responsible self-care.",
                    "Rest must still feel earned, turning self-care into another standard to perform correctly."
                );

            case "phone":
                return SelectOutcome(
                    zoneType,
                    "Rejection has less control over help-seeking, making trusted support easier to approach.",
                    "Support feels legitimate, though trust is chosen carefully and some hesitation remains.",
                    "Anticipated rejection encourages overexplaining, withdrawal, and carrying pressure alone."
                );

            case "pen":
                return SelectOutcome(
                    zoneType,
                    "Past humiliation no longer defines ability, though public criticism may still feel destabilizing.",
                    "Mistakes can be reviewed and corrected without becoming proof of total incompetence.",
                    "Mistakes and authority judgment remain highly active, encouraging perfectionism and repeated checking."
                );

            default:
                return "No outcome summary is available for this memory.";
        }
    }

    private static string SelectOutcome(
        MemoryPlacementZoneType zoneType,
        string backgroundOutcome,
        string contextOutcome,
        string focusOutcome)
    {
        switch (zoneType)
        {
            case MemoryPlacementZoneType.Background:
                return backgroundOutcome;
            case MemoryPlacementZoneType.Context:
                return contextOutcome;
            case MemoryPlacementZoneType.Focus:
                return focusOutcome;
            default:
                return "No attention outcome is available for this memory.";
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }
}
