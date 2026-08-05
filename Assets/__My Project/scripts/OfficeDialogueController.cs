using System;
using UnityEngine;
using UnityEngine.UI;

public class OfficeDialogueController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AssistantController assistantController;
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private GameObject choiceCanvas;
    [SerializeField] private Button jobExplanationButton;
    [SerializeField] private Button exploreOfficeButton;
    [SerializeField] private Button startWorkButton;

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
        && !HasHeardJobExplanation;
    public bool IsExploreOfficeAvailable => !isBusy && !isExploring && !IsReadyToStart;
    public bool IsStartWorkAvailable => !isBusy && !isExploring && !IsReadyToStart;

    private bool isBusy;
    private bool isExploring;

    private void Awake()
    {
        if (jobExplanationButton != null)
            jobExplanationButton.onClick.AddListener(SelectJobExplanation);
        if (exploreOfficeButton != null)
            exploreOfficeButton.onClick.AddListener(SelectExploreOffice);
        if (startWorkButton != null)
            startWorkButton.onClick.AddListener(SelectStartWork);
    }

    public void BeginOfficeDialogue()
    {
        HasHeardJobExplanation = false;
        HasExploredOffice = false;
        IsReadyToStart = false;
        isExploring = false;
        isBusy = true;
        HideAllChoiceButtons();
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
        HideAllChoiceButtons();
        Play(startWorkLines, OnStartWorkDialogueComplete);
    }

    private void OnStartWorkDialogueComplete()
    {
        if (gameStateController != null)
            gameStateController.SetState(GameStateController.GameState.AwaitCrystalBall);

        Play(crystalBallInstructionLines, null);
    }

    private void BeginDialogue(AssistantController.DialogueLine[] lines, Action onComplete)
    {
        isBusy = true;
        HideAllChoiceButtons();
        Play(lines, onComplete);
    }

    private void FinishDialogue()
    {
        isBusy = false;
        RefreshChoices();
    }

    private void FinishExplorationDeparture()
    {
        // Choices stay hidden while the player explores.
        // An external trigger (door, button, zone) must call ReturnFromExploration()
        // to bring the dialogue choices back.
        isBusy = false;
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
            HideAllChoiceButtons();
            return;
        }

        bool choicesAvailable = !isBusy && !isExploring;

        SetButtonVisible(jobExplanationButton, choicesAvailable && IsJobExplanationAvailable);
        SetButtonVisible(exploreOfficeButton, choicesAvailable);
        SetButtonVisible(startWorkButton, choicesAvailable);

        if (choiceCanvas != null)
            choiceCanvas.SetActive(choicesAvailable);
    }

    private void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
            button.gameObject.SetActive(visible);
    }

    private void HideAllChoiceButtons()
    {
        SetButtonVisible(jobExplanationButton, false);
        SetButtonVisible(exploreOfficeButton, false);
        SetButtonVisible(startWorkButton, false);

        if (choiceCanvas != null)
            choiceCanvas.SetActive(false);
    }
}
