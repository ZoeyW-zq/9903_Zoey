using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfficeDialogueController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AssistantController assistantController;
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private GameObject choiceCanvas;
    [SerializeField] private Button[] choiceButtons = new Button[3];
    [SerializeField] private TMP_Text[] choiceLabels = new TMP_Text[3];

    [Header("Option Labels")]
    [SerializeField] private string jobExplanationLabel = "Learn about the Memory Organizer role";
    [SerializeField] private string exploreOfficeLabel = "Explore the office";
    [SerializeField] private string waitALittleLabel = "I want to wait a bit";
    [SerializeField] private string startWorkLabel = "Start work now";

    [Header("Dialogue")]
    [SerializeField] private AssistantController.DialogueLine[] greetingLines;
    [SerializeField] private AssistantController.DialogueLine[] jobExplanationLines;
    [SerializeField] private AssistantController.DialogueLine[] explorationDepartureLines;
    [SerializeField] private AssistantController.DialogueLine[] explorationReturnLines;
    [SerializeField] private AssistantController.DialogueLine[] startWorkLines;
    [SerializeField] private AssistantController.DialogueLine[] crystalBallInstructionLines;

    public bool HasHeardJobExplanation { get; private set; }
    public bool HasExploredOffice { get; private set; }
    public bool IsReadyToStart { get; private set; }
    public bool IsJobExplanationAvailable => !isBusy && !isExploring && !IsReadyToStart
        && !HasHeardJobExplanation && !HasExploredOffice;
    public bool IsExploreOfficeAvailable => !isBusy && !isExploring && !IsReadyToStart;
    public bool IsStartWorkAvailable => !isBusy && !isExploring && !IsReadyToStart;

    private bool isBusy;
    private bool isExploring;

    public void BeginOfficeDialogue()
    {
        HasHeardJobExplanation = false;
        HasExploredOffice = false;
        IsReadyToStart = false;
        isExploring = false;
        isBusy = true;
        HideChoices();
        Play(greetingLines, FinishDialogue);
    }

    public void SelectJobExplanation()
    {
        if (!IsJobExplanationAvailable)
            return;

        HasHeardJobExplanation = true;
        BeginDialogue(jobExplanationLines, FinishDialogue);
    }

    public void SelectExploreOffice()
    {
        if (!IsExploreOfficeAvailable)
            return;

        isExploring = true;
        BeginDialogue(explorationDepartureLines, FinishExplorationDeparture);
    }

    public void ReturnFromExploration()
    {
        if (!isExploring || isBusy)
            return;

        isExploring = false;
        HasExploredOffice = true;
        BeginDialogue(explorationReturnLines, FinishDialogue);
    }

    public void SelectStartWork()
    {
        if (!IsStartWorkAvailable)
            return;

        IsReadyToStart = true;
        isBusy = true;
        HideChoices();
        Play(startWorkLines, StartCrystalBallEntry);
    }

    private void BeginDialogue(AssistantController.DialogueLine[] lines, Action onComplete)
    {
        isBusy = true;
        HideChoices();
        Play(lines, onComplete);
    }

    private void FinishDialogue()
    {
        isBusy = false;
        RefreshChoices();
    }

    private void FinishExplorationDeparture()
    {
        isBusy = false;
    }

    private void StartCrystalBallEntry()
    {
        if (gameStateController == null)
        {
            Debug.LogWarning("OfficeDialogueController: GameStateController is not assigned.", this);
            return;
        }

        gameStateController.SetState(GameStateController.GameState.AwaitCrystalBall);
        Play(crystalBallInstructionLines, null);
    }

    private void Play(AssistantController.DialogueLine[] lines, Action onComplete)
    {
        if (assistantController != null)
        {
            assistantController.PlayDialogue(lines, onComplete);
            return;
        }

        Debug.LogWarning("OfficeDialogueController: AssistantController is not assigned.", this);
    }

    private void RefreshChoices()
    {
        if (IsReadyToStart)
        {
            HideChoices();
            return;
        }

        Choice[] choices = BuildChoices();

        if (choiceCanvas != null)
            choiceCanvas.SetActive(choices.Length > 0);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            Button button = choiceButtons[i];
            if (button == null)
                continue;

            bool visible = i < choices.Length;
            button.gameObject.SetActive(visible);
            button.onClick.RemoveAllListeners();

            if (!visible)
                continue;

            if (i < choiceLabels.Length && choiceLabels[i] != null)
                choiceLabels[i].text = choices[i].label;

            int choiceIndex = i;
            button.onClick.AddListener(() => choices[choiceIndex].action());
        }
    }

    private Choice[] BuildChoices()
    {
        if (HasExploredOffice)
        {
            return new[]
            {
                new Choice(waitALittleLabel, SelectExploreOffice),
                new Choice(startWorkLabel, SelectStartWork)
            };
        }

        if (IsJobExplanationAvailable)
        {
            return new[]
            {
                new Choice(jobExplanationLabel, SelectJobExplanation),
                new Choice(exploreOfficeLabel, SelectExploreOffice),
                new Choice(startWorkLabel, SelectStartWork)
            };
        }

        return new[]
        {
            new Choice(exploreOfficeLabel, SelectExploreOffice),
            new Choice(startWorkLabel, SelectStartWork)
        };
    }

    private void HideChoices()
    {
        if (choiceCanvas != null)
            choiceCanvas.SetActive(false);
    }

    private readonly struct Choice
    {
        public readonly string label;
        public readonly Action action;

        public Choice(string label, Action action)
        {
            this.label = label;
            this.action = action;
        }
    }
}
