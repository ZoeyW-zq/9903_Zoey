using System;
using UnityEngine;
using UnityEngine.UI;

public class OfficeDialogueController : MonoBehaviour
{
    public enum DialogueMode
    {
        Recorded,
        ExternalConvai
    }

    [Header("References")]
    [SerializeField] private AssistantController assistantController;
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private GameObject choiceCanvas;
    [SerializeField] private Button jobExplanationButton;
    [SerializeField] private Button exploreOfficeButton;
    [SerializeField] private Button startWorkButton;
    [SerializeField] private Button finishReadingButton;
    [SerializeField] private GameObject computerScreenContent;

    [Header("Mode")]
    [SerializeField] private DialogueMode dialogueMode = DialogueMode.Recorded;

    [Header("Dialogue")]
    [SerializeField] private AssistantController.DialogueLine[] greetingLines;
    [SerializeField] private AssistantController.DialogueLine[] jobExplanationLines;
    [SerializeField] private AssistantController.DialogueLine[] explorationDepartureLines;
    [SerializeField] private AssistantController.DialogueLine[] explorationReturnLines;
    [SerializeField] private AssistantController.DialogueLine[] startWorkLines;
    [SerializeField] private AssistantController.DialogueLine[] crystalBallInstructionLines;

    public bool HasHeardJobExplanation { get; private set; }
    public bool IsReadyToStart { get; private set; }
    public bool IsExploring => isExploring;
    public bool IsWaitingForReadingConfirmation => waitingForReadingConfirmation;
    public bool UsesExternalDialogue => dialogueMode == DialogueMode.ExternalConvai;
    public bool IsJobExplanationAvailable => !isBusy && !isExploring && !IsReadyToStart
        && !HasHeardJobExplanation;
    public bool IsExploreOfficeAvailable => !isBusy && !isExploring && !IsReadyToStart;
    public bool IsStartWorkAvailable => !isBusy && !isExploring && !IsReadyToStart;

    private bool isBusy;
    private bool isExploring;
    private bool hasLeftExplorationReturnArea;
    private bool waitingForReadingConfirmation;

    public event Action ExternalDialogueBegan;
    public event Action ExternalExplorationReturned;

    private void Awake()
    {
        if (jobExplanationButton != null)
            jobExplanationButton.onClick.AddListener(SelectJobExplanation);
        if (exploreOfficeButton != null)
            exploreOfficeButton.onClick.AddListener(SelectExploreOffice);
        if (startWorkButton != null)
            startWorkButton.onClick.AddListener(SelectStartWork);
        if (finishReadingButton != null)
            finishReadingButton.onClick.AddListener(FinishReading);

        SetComputerScreenVisible(false);
        SetButtonVisible(finishReadingButton, false);
    }

    public void BeginOfficeDialogue()
    {
        HasHeardJobExplanation = false;
        IsReadyToStart = false;
        isExploring = false;
        hasLeftExplorationReturnArea = false;
        waitingForReadingConfirmation = false;
        isBusy = !UsesExternalDialogue;
        SetComputerScreenVisible(false);
        HideAllChoiceButtons();

        if (UsesExternalDialogue)
        {
            ExternalDialogueBegan?.Invoke();
            return;
        }

        Play(greetingLines, FinishDialogue);
    }

    public void SelectJobExplanation()
    {
        if (!IsJobExplanationAvailable)
            return;

        HasHeardJobExplanation = true;

        if (UsesExternalDialogue)
        {
            HideAllChoiceButtons();
            return;
        }

        BeginDialogue(jobExplanationLines, FinishDialogue);
    }

    public void SelectExploreOffice()
    {
        if (!IsExploreOfficeAvailable)
            return;

        isExploring = true;
        hasLeftExplorationReturnArea = false;

        if (UsesExternalDialogue)
        {
            HideAllChoiceButtons();
            return;
        }

        BeginDialogue(explorationDepartureLines, FinishExplorationDeparture);
    }

    public void ReturnFromExploration()
    {
        if (!isExploring || isBusy || !hasLeftExplorationReturnArea)
            return;

        isExploring = false;
        hasLeftExplorationReturnArea = false;

        if (UsesExternalDialogue)
        {
            HideAllChoiceButtons();
            ExternalExplorationReturned?.Invoke();
            return;
        }

        BeginDialogue(explorationReturnLines, FinishDialogue);
    }

    public void MarkExplorationReturnAreaExited()
    {
        if (isExploring)
            hasLeftExplorationReturnArea = true;
    }

    public void SelectStartWork()
    {
        if (!IsStartWorkAvailable)
            return;

        IsReadyToStart = true;

        if (UsesExternalDialogue)
        {
            isBusy = true;
            HideAllChoiceButtons();
            return;
        }

        BeginDialogue(startWorkLines, OnStartWorkDialogueComplete);
    }

    public void CompleteExternalStartWork()
    {
        if (!UsesExternalDialogue || !IsReadyToStart || waitingForReadingConfirmation)
            return;

        ShowReportForReading();
    }

    private void OnStartWorkDialogueComplete()
    {
        ShowReportForReading();
    }

    private void ShowReportForReading()
    {
        isBusy = false;
        waitingForReadingConfirmation = true;
        SetComputerScreenVisible(true);

        if (UsesExternalDialogue)
        {
            SetButtonVisible(finishReadingButton, false);
            if (choiceCanvas != null)
                choiceCanvas.SetActive(false);
            return;
        }

        SetButtonVisible(finishReadingButton, true);

        if (choiceCanvas != null)
            choiceCanvas.SetActive(true);
    }

    public void FinishReading()
    {
        if (!waitingForReadingConfirmation || isBusy)
            return;

        waitingForReadingConfirmation = false;
        isBusy = true;
        HideAllChoiceButtons();

        if (gameStateController != null)
            gameStateController.SetState(GameStateController.GameState.AwaitCrystalBall);

        if (UsesExternalDialogue)
        {
            isBusy = false;
            return;
        }

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
        if (UsesExternalDialogue)
            return;

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

        SetButtonVisible(jobExplanationButton, IsJobExplanationAvailable);
        SetButtonVisible(exploreOfficeButton, IsExploreOfficeAvailable);
        SetButtonVisible(startWorkButton, IsStartWorkAvailable);
        SetButtonVisible(finishReadingButton, false);

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
        SetButtonVisible(finishReadingButton, false);

        if (choiceCanvas != null)
            choiceCanvas.SetActive(false);
    }

    private void SetComputerScreenVisible(bool visible)
    {
        if (computerScreenContent != null)
            computerScreenContent.SetActive(visible);
    }
}
