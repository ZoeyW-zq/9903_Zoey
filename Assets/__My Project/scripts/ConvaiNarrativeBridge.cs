using System;
using System.Collections;
using Convai.Domain.DomainEvents.Transcript;
using Convai.Domain.Narrative;
using Convai.Modules.Narrative;
using Convai.Runtime;
using Convai.Runtime.Adapters.Networking;
using Convai.Runtime.Components;
using Convai.Runtime.Facades;
using UnityEngine;

public class ConvaiNarrativeBridge : MonoBehaviour
{
    [Header("Convai")]
    [SerializeField] private ConvaiCharacter character;
    [SerializeField] private ConvaiNarrativeDesignManager narrativeManager;

    [Header("Unity Flow")]
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private OfficeDialogueController officeDialogueController;

    [Header("Saved Narrative Triggers")]
    [SerializeField] private string officeEnteredTrigger = "office_entered";
    [SerializeField] private string explorationReturnedTrigger = "office_exploration_returned";
    [SerializeField] private string startWorkRequestedTrigger = "office_start_work_requested";
    [SerializeField] private string officeReportReadyTrigger = "office_report_ready";
    [SerializeField] private string officeReadingFinishedTrigger = "office_reading_finished";
    [SerializeField] private string memoryRoomEnteredTrigger = "memory_room_entered";
    [SerializeField] private string missingPainfulMemoriesTrigger = "missing_painful_memories_detected";
    [SerializeField] private string crisisStartedTrigger = "crisis_started";
    [SerializeField] private string swallowStartedTrigger = "swallow_started";
    [SerializeField] private string playerSwallowedTrigger = "player_swallowed";
    [SerializeField] private string allMemoriesResolvedTrigger = "All-resolved Trigger";
    [SerializeField] private string returnedToMemoryRoomTrigger = "returned_to_memory_room";
    [SerializeField] private string finalDistributionConfirmedTrigger = "final_distribution_confirmed";
    [SerializeField] private string officeReturnedTrigger = "office_returned";
    [SerializeField] private string reportClosedTrigger = "report_closed";

    [Header("Safety")]
    [SerializeField, Min(1f)] private float characterReadyWarningTimeout = 45f;
    [SerializeField, Min(0.5f)] private float decisionFallbackDelay = 2.5f;
    [SerializeField, Min(1f)] private float startWorkTurnTimeout = 20f;
    [SerializeField, Min(1f)] private float transitionSpeechTimeout = 25f;

    private bool officeEntryPending;
    private bool waitingForStartWorkTurn;
    private bool startWorkSpeechObserved;
    private bool waitingForMissingMemoriesTurn;
    private bool missingMemoriesSpeechObserved;
    private bool waitingForAllMemoriesTurn;
    private bool allMemoriesSpeechObserved;
    private bool waitingForFinalDistributionTurn;
    private bool finalDistributionSpeechObserved;
    private bool waitingForSessionCloseTurn;
    private bool sessionCloseSpeechObserved;
    private bool memoryRoomEnteredSent;
    private bool missingPainfulMemoriesSent;
    private bool crisisStartedSent;
    private bool swallowStartedSent;
    private bool playerSwallowedSent;
    private bool allMemoriesResolvedSent;
    private bool returnedToMemoryRoomSent;
    private bool finalDistributionConfirmedSent;
    private bool officeReturnedSent;
    private bool reportClosedSent;
    private string lastHandledSectionId = string.Empty;
    private ConvaiEvents convaiEvents;
    private ConvaiRoomManager roomManager;
    private MemoryPlacementController memoryPlacementController;
    private Coroutine characterReadyRoutine;
    private Coroutine characterReadyWarningRoutine;
    private Coroutine startWorkDecisionFallbackRoutine;
    private Coroutine readingFinishedDecisionFallbackRoutine;
    private Coroutine startWorkTimeoutRoutine;
    private Coroutine missingMemoriesTimeoutRoutine;
    private Coroutine allMemoriesTimeoutRoutine;
    private Coroutine finalDistributionTimeoutRoutine;
    private Coroutine sessionCloseTimeoutRoutine;

    private void Awake()
    {
        if (character == null)
            character = GetComponent<ConvaiCharacter>();
        if (narrativeManager == null)
            narrativeManager = GetComponent<ConvaiNarrativeDesignManager>();
        if (memoryPlacementController == null && gameStateController != null)
            memoryPlacementController = gameStateController.MemoryPlacementController;
    }

    private void OnEnable()
    {
        Debug.Log("ConvaiNarrativeBridge active: Office flow is connected to Narrative Design.", this);

        if (memoryPlacementController == null && gameStateController != null)
            memoryPlacementController = gameStateController.MemoryPlacementController;

        if (character != null)
        {
            character.OnCharacterReady += HandleCharacterReady;
            character.OnSpeechStarted += HandleSpeechStarted;
            character.OnSpeechStopped += HandleSpeechStopped;
            character.OnTurnCompleted += HandleTurnCompleted;
            character.NarrativeDesign.OnSectionDataReceived += HandleSectionDataReceived;
            character.NarrativeDesign.OnSectionChanged += HandleCharacterSectionChanged;
        }

        TryAttachConvaiEvents();

        if (narrativeManager != null)
            narrativeManager.OnAnySectionChanged.AddListener(HandleSectionChanged);

        if (officeDialogueController != null)
        {
            officeDialogueController.ExternalDialogueBegan += HandleExternalOfficeBegan;
            officeDialogueController.ExternalExplorationReturned += HandleExplorationReturned;
        }

        if (gameStateController != null)
        {
            gameStateController.StateChanged += HandleGameStateChanged;
            gameStateController.AllMemoriesResolved += HandleAllMemoriesResolved;
            gameStateController.FinalPlacementConfirmed += HandleFinalPlacementConfirmed;
            gameStateController.ReturnedToOffice += HandleReturnedToOffice;
            gameStateController.SessionCloseRequested += HandleSessionCloseRequested;
            gameStateController.MirrorMemoryResolved += HandleMirrorMemoryResolved;
        }

        if (memoryPlacementController != null)
            memoryPlacementController.MissingPainfulMemoriesDetected += HandleMissingPainfulMemoriesDetected;
    }

    private void OnDisable()
    {
        if (character != null)
        {
            character.OnCharacterReady -= HandleCharacterReady;
            character.OnSpeechStarted -= HandleSpeechStarted;
            character.OnSpeechStopped -= HandleSpeechStopped;
            character.OnTurnCompleted -= HandleTurnCompleted;
            character.NarrativeDesign.OnSectionDataReceived -= HandleSectionDataReceived;
            character.NarrativeDesign.OnSectionChanged -= HandleCharacterSectionChanged;
        }

        DetachConvaiEvents();

        if (narrativeManager != null)
            narrativeManager.OnAnySectionChanged.RemoveListener(HandleSectionChanged);

        if (officeDialogueController != null)
        {
            officeDialogueController.ExternalDialogueBegan -= HandleExternalOfficeBegan;
            officeDialogueController.ExternalExplorationReturned -= HandleExplorationReturned;
        }

        if (gameStateController != null)
        {
            gameStateController.StateChanged -= HandleGameStateChanged;
            gameStateController.AllMemoriesResolved -= HandleAllMemoriesResolved;
            gameStateController.FinalPlacementConfirmed -= HandleFinalPlacementConfirmed;
            gameStateController.ReturnedToOffice -= HandleReturnedToOffice;
            gameStateController.SessionCloseRequested -= HandleSessionCloseRequested;
            gameStateController.MirrorMemoryResolved -= HandleMirrorMemoryResolved;
        }

        if (memoryPlacementController != null)
            memoryPlacementController.MissingPainfulMemoriesDetected -= HandleMissingPainfulMemoriesDetected;

        StopCharacterReadyWait();
        StopStartWorkDecisionFallback();
        StopReadingFinishedDecisionFallback();
        StopStartWorkTimeout();
        StopTransitionTimeout(ref missingMemoriesTimeoutRoutine);
        StopTransitionTimeout(ref allMemoriesTimeoutRoutine);
        StopTransitionTimeout(ref finalDistributionTimeoutRoutine);
        StopTransitionTimeout(ref sessionCloseTimeoutRoutine);
    }

    private void Update()
    {
        string sectionId = narrativeManager != null ? narrativeManager.CurrentSectionID : string.Empty;
        if (string.IsNullOrEmpty(sectionId) && character != null)
            sectionId = character.NarrativeDesign.CurrentSectionId;

        if (!string.IsNullOrEmpty(sectionId) && sectionId != lastHandledSectionId)
            HandleSectionChanged(sectionId);
    }

    private void HandleExternalOfficeBegan()
    {
        SetOfficeContext("first_entry");
        officeEntryPending = true;

        if (character != null && character.IsInConversation)
        {
            SendPendingOfficeEntry();
            return;
        }

        Debug.Log("Convai Office flow: waiting for Mio to become ready before sending office_entered.", this);
        StartCharacterReadyWarning();
    }

    private void HandleCharacterReady()
    {
        Debug.Log($"Convai flow: {GetCharacterLabel()} is ready.", this);
        TryAttachConvaiEvents();
        CacheRoomManager();
        LogNarrativeSessionState("Character Ready");
        UpdateRuntimeContextForState(gameStateController != null
            ? gameStateController.State
            : GameStateController.GameState.None);

        if (!officeEntryPending || characterReadyRoutine != null)
            return;

        // Convai raises OnCharacterReady before it flushes queued template keys.
        // Wait one frame so office context reaches the backend before the trigger.
        characterReadyRoutine = StartCoroutine(SendPendingOfficeEntryNextFrame());
    }

    private void HandleFinalPlayerTranscript(FinalUserTranscriptionReceived transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript.Text))
            return;

        string sectionId = character != null ? character.NarrativeDesign.CurrentSectionId : string.Empty;
        string sectionName = ResolveSectionName(sectionId);
        string sectionLabel = string.IsNullOrEmpty(sectionName) ? "no section" : sectionName;
        Debug.Log($"Convai player transcript processed-final ({sectionLabel}): {transcript.Text}", this);

        if (IsStartWorkIntent(transcript.Text))
            ScheduleStartWorkDecisionFallback(sectionId);

        if (sectionName == "office_report_reading" && IsReadingFinishedIntent(transcript.Text))
            ScheduleReadingFinishedDecisionFallback(sectionId);
    }

    private void HandleCharacterSectionChanged(string previousSectionId, string newSectionId)
    {
        Debug.Log(
            $"Convai character section event: {ResolveSectionName(previousSectionId)} ({previousSectionId}) -> {ResolveSectionName(newSectionId)} ({newSectionId})",
            this);
    }

    private IEnumerator SendPendingOfficeEntryNextFrame()
    {
        yield return null;
        characterReadyRoutine = null;
        SendPendingOfficeEntry();
    }

    private void SendPendingOfficeEntry()
    {
        if (!officeEntryPending)
            return;

        if (character == null || !character.IsInConversation)
        {
            StartCharacterReadyWarning();
            return;
        }

        officeEntryPending = false;
        StopCharacterReadyWait();
        InvokeSavedTrigger(officeEnteredTrigger);
    }

    private void HandleExplorationReturned()
    {
        SetOfficeContext("exploration_return");
        InvokeSavedTrigger(explorationReturnedTrigger);
    }

    private void HandleSectionChanged(string sectionId)
    {
        if (string.IsNullOrEmpty(sectionId) || sectionId == lastHandledSectionId)
            return;

        lastHandledSectionId = sectionId;
        string sectionName = ResolveSectionName(sectionId);
        Debug.Log($"Convai Narrative section entered: {sectionName} ({sectionId})", this);

        if (ShouldRejectOfficeRegression(sectionName))
        {
            Debug.LogWarning(
                $"Convai Narrative rejected stale Office section '{sectionName}' because Unity is in {gameStateController.State}. Restoring the authoritative stage section.",
                this);
            RestoreAuthoritativeNarrativeSection();
            return;
        }

        if (sectionName == "office_start_work")
            StopStartWorkDecisionFallback();
        if (sectionName == "office_crystal_ball")
            StopReadingFinishedDecisionFallback();

        switch (sectionName)
        {
            case "office_job_explanation":
                if (officeDialogueController != null && officeDialogueController.IsJobExplanationAvailable)
                    officeDialogueController.SelectJobExplanation();
                SetOfficeContextValue("has_heard_job_explanation", "true");
                break;

            case "office_exploration":
                if (officeDialogueController != null && officeDialogueController.IsExploreOfficeAvailable)
                    officeDialogueController.SelectExploreOffice();
                break;

            case "office_start_work":
                BeginStartWorkSection();
                break;

            case "office_crystal_ball":
                if (officeDialogueController != null && officeDialogueController.IsWaitingForReadingConfirmation)
                {
                    officeDialogueController.FinishReading();
                    Debug.Log("Convai Office flow: crystal ball enabled after report confirmation.", this);
                }
                else
                {
                    Debug.LogWarning("Convai Office flow: office_crystal_ball arrived before the report was ready.", this);
                }
                break;

            case "memory_room_initial_placement":
                if (gameStateController != null && gameStateController.State != GameStateController.GameState.Hippocampus)
                {
                    Debug.LogError(
                        "Convai Narrative entered memory_room_initial_placement before Unity reached Hippocampus. Remove any Decision that connects office_crystal_ball directly to this Section; only the memory_room_entered Unity trigger should enter it.",
                        this);
                }
                break;

            case "session_complete":
                if (!waitingForSessionCloseTurn && gameStateController != null)
                    gameStateController.HandleCloseSessionRequested();
                break;
        }
    }

    private void HandleSectionDataReceived(NarrativeSectionData sectionData)
    {
        if (sectionData == null || string.IsNullOrEmpty(sectionData.SectionId))
            return;

        HandleSectionChanged(sectionData.SectionId);
    }

    private void BeginStartWorkSection()
    {
        if (officeDialogueController == null || !officeDialogueController.IsStartWorkAvailable)
            return;

        officeDialogueController.SelectStartWork();
        waitingForStartWorkTurn = true;
        startWorkSpeechObserved = character != null && character.IsSpeaking;
        StopStartWorkTimeout();
        startWorkTimeoutRoutine = StartCoroutine(CompleteStartWorkAfterTimeout());
        Debug.Log($"Convai Office flow: waiting for {GetCharacterLabel()} to finish the Start Work response.", this);
    }

    private void HandleSpeechStarted()
    {
        if (waitingForStartWorkTurn)
            startWorkSpeechObserved = true;
        if (waitingForMissingMemoriesTurn)
            missingMemoriesSpeechObserved = true;
        if (waitingForAllMemoriesTurn)
            allMemoriesSpeechObserved = true;
        if (waitingForFinalDistributionTurn)
            finalDistributionSpeechObserved = true;
        if (waitingForSessionCloseTurn)
            sessionCloseSpeechObserved = true;
    }

    private void HandleSpeechStopped()
    {
        if (waitingForStartWorkTurn && startWorkSpeechObserved)
            CompleteStartWorkSection();
        if (waitingForMissingMemoriesTurn && missingMemoriesSpeechObserved)
            CompleteMissingMemoriesTurn();
        if (waitingForAllMemoriesTurn && allMemoriesSpeechObserved)
            CompleteAllMemoriesTurn();
        if (waitingForFinalDistributionTurn && finalDistributionSpeechObserved)
            CompleteFinalDistributionTurn();
        if (waitingForSessionCloseTurn && sessionCloseSpeechObserved)
            CompleteSessionCloseTurn();
    }

    private void HandleTurnCompleted(bool wasInterrupted)
    {
        string sectionId = character != null ? character.NarrativeDesign.CurrentSectionId : string.Empty;
        Debug.Log($"Convai turn completed (interrupted={wasInterrupted}, section={ResolveSectionName(sectionId)} [{sectionId}]).", this);

        if (waitingForStartWorkTurn
            && (startWorkSpeechObserved || wasInterrupted)
            && (character == null || !character.IsSpeaking))
        {
            CompleteStartWorkSection();
        }

        if (character == null || !character.IsSpeaking)
        {
            if (waitingForMissingMemoriesTurn && (missingMemoriesSpeechObserved || wasInterrupted))
                CompleteMissingMemoriesTurn();
            if (waitingForAllMemoriesTurn && (allMemoriesSpeechObserved || wasInterrupted))
                CompleteAllMemoriesTurn();
            if (waitingForFinalDistributionTurn && (finalDistributionSpeechObserved || wasInterrupted))
                CompleteFinalDistributionTurn();
            if (waitingForSessionCloseTurn && (sessionCloseSpeechObserved || wasInterrupted))
                CompleteSessionCloseTurn();
        }
    }

    private IEnumerator CompleteStartWorkAfterTimeout()
    {
        yield return new WaitForSeconds(startWorkTurnTimeout);
        startWorkTimeoutRoutine = null;

        if (waitingForStartWorkTurn)
        {
            Debug.LogWarning("ConvaiNarrativeBridge: Start Work turn timed out; continuing with the local report flow.", this);
            CompleteStartWorkSection();
        }
    }

    private void CompleteStartWorkSection()
    {
        waitingForStartWorkTurn = false;
        startWorkSpeechObserved = false;
        StopStartWorkTimeout();

        if (officeDialogueController == null)
            return;

        officeDialogueController.CompleteExternalStartWork();
        Debug.Log("Convai Office flow: client report is now visible.", this);

        if (officeDialogueController.IsWaitingForReadingConfirmation)
            InvokeSavedTrigger(officeReportReadyTrigger);
    }

    private void HandleGameStateChanged(GameStateController.GameState previousState, GameStateController.GameState newState)
    {
        UpdateRuntimeContextForState(newState);

        switch (newState)
        {
            case GameStateController.GameState.Hippocampus when !memoryRoomEnteredSent:
                memoryRoomEnteredSent = true;
                Debug.Log("Convai flow: Unity reached Hippocampus; sending memory_room_entered.", this);
                InvokeSavedTrigger(memoryRoomEnteredTrigger);
                break;

            case GameStateController.GameState.GiantCrisis when !crisisStartedSent:
                crisisStartedSent = true;
                Debug.Log("Convai flow: Giant Crisis started; sending crisis_started.", this);
                InvokeSavedTrigger(crisisStartedTrigger);
                break;

            case GameStateController.GameState.SwallowTransition when !swallowStartedSent:
                swallowStartedSent = true;
                Debug.Log("Convai flow: swallowing started; sending swallow_started.", this);
                InvokeSavedTrigger(swallowStartedTrigger);
                break;

            case GameStateController.GameState.MirrorChamber when !playerSwallowedSent:
                playerSwallowedSent = true;
                Debug.Log("Convai flow: Unity reached Mirror Chamber; sending player_swallowed.", this);
                InvokeSavedTrigger(playerSwallowedTrigger);
                break;

            case GameStateController.GameState.FinalMemoryPlacement when !returnedToMemoryRoomSent:
                returnedToMemoryRoomSent = true;
                Debug.Log("Convai flow: Unity returned to Memory Room; sending returned_to_memory_room.", this);
                InvokeSavedTrigger(returnedToMemoryRoomTrigger);
                break;

        }
    }

    private void HandleMissingPainfulMemoriesDetected()
    {
        if (missingPainfulMemoriesSent)
            return;

        missingPainfulMemoriesSent = true;
        waitingForMissingMemoriesTurn = true;
        missingMemoriesSpeechObserved = false;
        RestartTransitionTimeout(ref missingMemoriesTimeoutRoutine, CompleteMissingMemoriesTurn, "missing-memory observation");
        Debug.Log("Convai flow: missing painful memories detected; sending matching trigger.", this);
        InvokeSavedTrigger(missingPainfulMemoriesTrigger);
    }

    private void HandleAllMemoriesResolved()
    {
        if (allMemoriesResolvedSent)
            return;

        allMemoriesResolvedSent = true;
        waitingForAllMemoriesTurn = true;
        allMemoriesSpeechObserved = false;
        RestartTransitionTimeout(ref allMemoriesTimeoutRoutine, CompleteAllMemoriesTurn, "all-memories praise");
        Debug.Log("Convai flow: all mirror memories resolved; sending All-resolved Trigger.", this);
        InvokeSavedTrigger(allMemoriesResolvedTrigger);
    }

    private void HandleFinalPlacementConfirmed()
    {
        if (finalDistributionConfirmedSent)
            return;

        finalDistributionConfirmedSent = true;
        waitingForFinalDistributionTurn = true;
        finalDistributionSpeechObserved = false;
        RestartTransitionTimeout(
            ref finalDistributionTimeoutRoutine,
            CompleteFinalDistributionTurn,
            "final-distribution confirmation");
        Debug.Log("Convai flow: final distribution confirmed; waiting for kiki before returning to Office.", this);
        InvokeSavedTrigger(finalDistributionConfirmedTrigger);
    }

    private void HandleReturnedToOffice()
    {
        if (officeReturnedSent)
            return;

        officeReturnedSent = true;
        Debug.Log("Convai flow: Unity reached the Office; sending office_returned.", this);
        InvokeSavedTrigger(officeReturnedTrigger);
    }

    private void HandleMirrorMemoryResolved(int resolvedCount)
    {
        if (character == null)
            return;

        int clampedCount = Mathf.Clamp(resolvedCount, 0, 4);
        character.DynamicContext.SetState(
            "Resolved mirror count",
            clampedCount.ToString(),
            ConvaiRespondMode.Silent);
        character.DynamicContext.SetState(
            "Latest confirmed scene event",
            "A mirror shattered after its painful memory was successfully resolved. The memory was released, not erased.",
            ConvaiRespondMode.Silent);
        character.DynamicContext.Flush();
        Debug.Log($"Convai context: {clampedCount}/4 mirror memories resolved.", this);
    }

    private void UpdateRuntimeContextForState(GameStateController.GameState state)
    {
        if (character == null)
            return;

        string room;
        string availableMemories;
        string currentTask;

        switch (state)
        {
            case GameStateController.GameState.OfficeDialogue:
            case GameStateController.GameState.AwaitCrystalBall:
            case GameStateController.GameState.TransitionToHippocampus:
                room = "Office";
                availableMemories = "No memory objects are available for placement yet.";
                currentTask = "Continue the Office onboarding and begin the assignment when the player is ready.";
                break;

            case GameStateController.GameState.Hippocampus:
                room = "Memory Room";
                availableMemories = "Water bottle, sunset photograph, and LEGO bricks.";
                currentTask = "Help the player understand the three attention shelves and consider the three surface memories without choosing for them.";
                break;

            case GameStateController.GameState.GiantCrisis:
                room = "Memory Room during an unknown emergency";
                availableMemories = "The three surface memories have been placed.";
                currentTask = "React to the confirmed danger without pretending to understand or control it.";
                break;

            case GameStateController.GameState.SwallowTransition:
                room = "Being swallowed; destination unknown";
                availableMemories = "No memory-placement task is currently available.";
                currentTask = "React briefly to the swallowing event without predicting the destination.";
                break;

            case GameStateController.GameState.MirrorChamber:
                room = "An unfamiliar space inside the giant";
                availableMemories = "Second-place medal, old alarm clock, old phone, and red correction pen, each associated with a mirror.";
                currentTask = "Help the player approach the mirrors. Answer active questions using cautious inferences and confirmed mirror-resolution state.";
                break;

            case GameStateController.GameState.FinalMemoryPlacement:
                room = "Memory Room after returning from the giant";
                availableMemories = "Water bottle, sunset photograph, LEGO bricks, second-place medal, old alarm clock, old phone, and red correction pen.";
                currentTask = "Help the player reconsider all seven memories across Focus, Context, and Background without declaring one correct arrangement.";
                break;

            case GameStateController.GameState.BackToOffice:
                room = "Office after the completed memory session";
                availableMemories = "The seven-memory distribution is recorded in the final report.";
                currentTask = "Invite the player to review the final report and wait for the Close button.";
                break;

            case GameStateController.GameState.SessionComplete:
                room = "Office session closing";
                availableMemories = "The final report has been closed.";
                currentTask = "Finish the farewell; no further task is available.";
                break;

            default:
                room = "Runtime state not yet initialized";
                availableMemories = "None confirmed.";
                currentTask = "Wait for Unity to confirm the current stage.";
                break;
        }

        character.DynamicContext.SetStates(
            new System.Collections.Generic.Dictionary<string, string>
            {
                { "Current room", room },
                { "Current Unity stage", state.ToString() },
                { "Currently available memory objects", availableMemories },
                { "Current player task", currentTask }
            },
            ConvaiRespondMode.Silent);
        character.DynamicContext.Flush();
    }

    private void HandleSessionCloseRequested()
    {
        if (reportClosedSent)
            return;

        reportClosedSent = true;
        waitingForSessionCloseTurn = true;
        sessionCloseSpeechObserved = false;
        RestartTransitionTimeout(
            ref sessionCloseTimeoutRoutine,
            CompleteSessionCloseTurn,
            "session-closing farewell");

        string currentSectionId = character != null
            ? character.NarrativeDesign.CurrentSectionId
            : string.Empty;
        if (ResolveSectionName(currentSectionId) != "session_complete")
        {
            Debug.Log("Convai flow: Report closed; sending report_closed and waiting for kiki's farewell.", this);
            InvokeSavedTrigger(reportClosedTrigger);
        }
    }

    private void CompleteMissingMemoriesTurn()
    {
        if (!waitingForMissingMemoriesTurn)
            return;

        waitingForMissingMemoriesTurn = false;
        missingMemoriesSpeechObserved = false;
        StopTransitionTimeout(ref missingMemoriesTimeoutRoutine);
        memoryPlacementController?.CompleteMissingPainfulMemoriesCue();
    }

    private void CompleteAllMemoriesTurn()
    {
        if (!waitingForAllMemoriesTurn)
            return;

        waitingForAllMemoriesTurn = false;
        allMemoriesSpeechObserved = false;
        StopTransitionTimeout(ref allMemoriesTimeoutRoutine);
        gameStateController?.ContinueAfterAllMemoriesResolved();
    }

    private void CompleteFinalDistributionTurn()
    {
        if (!waitingForFinalDistributionTurn)
            return;

        waitingForFinalDistributionTurn = false;
        finalDistributionSpeechObserved = false;
        StopTransitionTimeout(ref finalDistributionTimeoutRoutine);
        gameStateController?.ContinueAfterFinalPlacementConfirmed();
    }

    private void CompleteSessionCloseTurn()
    {
        if (!waitingForSessionCloseTurn)
            return;

        waitingForSessionCloseTurn = false;
        sessionCloseSpeechObserved = false;
        StopTransitionTimeout(ref sessionCloseTimeoutRoutine);
        gameStateController?.ContinueAfterSessionClosing();
    }

    private void RestartTransitionTimeout(ref Coroutine routine, Action completion, string label)
    {
        StopTransitionTimeout(ref routine);
        routine = StartCoroutine(CompleteTransitionAfterTimeout(completion, label));
    }

    private IEnumerator CompleteTransitionAfterTimeout(Action completion, string label)
    {
        yield return new WaitForSecondsRealtime(transitionSpeechTimeout);
        Debug.LogWarning(
            $"ConvaiNarrativeBridge: {label} speech timed out after {transitionSpeechTimeout:0}s; continuing the Unity flow.",
            this);
        completion?.Invoke();
    }

    private void StopTransitionTimeout(ref Coroutine routine)
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }

    private bool ShouldRejectOfficeRegression(string sectionName)
    {
        if (gameStateController == null || string.IsNullOrEmpty(sectionName)
            || !sectionName.StartsWith("office_", StringComparison.Ordinal))
        {
            return false;
        }

        GameStateController.GameState state = gameStateController.State;
        return state == GameStateController.GameState.Hippocampus
            || state == GameStateController.GameState.GiantCrisis
            || state == GameStateController.GameState.SwallowTransition
            || state == GameStateController.GameState.MirrorChamber
            || state == GameStateController.GameState.FinalMemoryPlacement;
    }

    private void RestoreAuthoritativeNarrativeSection()
    {
        if (gameStateController == null)
            return;

        switch (gameStateController.State)
        {
            case GameStateController.GameState.Hippocampus:
                InvokeSavedTrigger(missingPainfulMemoriesSent
                    ? missingPainfulMemoriesTrigger
                    : memoryRoomEnteredTrigger);
                break;
            case GameStateController.GameState.GiantCrisis:
                InvokeSavedTrigger(crisisStartedTrigger);
                break;
            case GameStateController.GameState.SwallowTransition:
            case GameStateController.GameState.MirrorChamber:
                InvokeSavedTrigger(playerSwallowedTrigger);
                break;
            case GameStateController.GameState.FinalMemoryPlacement:
                InvokeSavedTrigger(returnedToMemoryRoomTrigger);
                break;
        }
    }

    private void SetOfficeContext(string entryReason)
    {
        SetOfficeContextValue("office_entry_reason", entryReason);
        SetOfficeContextValue(
            "has_heard_job_explanation",
            officeDialogueController != null && officeDialogueController.HasHeardJobExplanation ? "true" : "false");
    }

    private void SetOfficeContextValue(string key, string value)
    {
        if (character == null)
            return;

        character.NarrativeDesign.SetTemplateKey(key, value);
    }

    private void InvokeSavedTrigger(string triggerName)
    {
        if (character == null || string.IsNullOrWhiteSpace(triggerName))
            return;

        bool readyForImmediateDelivery = character.IsInConversation;
        bool accepted = character.NarrativeDesign.InvokeTrigger(triggerName);

        if (!accepted)
        {
            Debug.LogWarning($"ConvaiNarrativeBridge: Narrative trigger '{triggerName}' was not accepted for immediate delivery and remains queued by the SDK.", this);
        }
        else if (!readyForImmediateDelivery)
        {
            Debug.Log($"Convai Narrative trigger queued until Character Ready: {triggerName}", this);
        }
        else
        {
            Debug.Log($"Convai Narrative trigger handed to transport: {triggerName}", this);
            LogNarrativeSessionState($"Trigger {triggerName}");
        }
    }

    private void CacheRoomManager()
    {
        if (roomManager != null)
            return;

        ConvaiManager manager = ConvaiManager.ActiveManager;
        if (manager != null)
            roomManager = manager.GetComponent<ConvaiRoomManager>();
    }

    private void LogNarrativeSessionState(string source)
    {
        CacheRoomManager();

        string roomSessionId = roomManager != null ? roomManager.CurrentSessionId : string.Empty;
        string roomCharacterSessionId = roomManager != null
            ? roomManager.CurrentCharacterSessionId
            : string.Empty;
        string characterSessionId = character != null ? character.CharacterSessionId : string.Empty;
        string sectionId = character != null ? character.NarrativeDesign.CurrentSectionId : string.Empty;

        Debug.Log(
            $"Convai Narrative session state [{source}]: RoomSessionId={FormatDiagnosticValue(roomSessionId)}, "
            + $"RoomCharacterSessionId={FormatDiagnosticValue(roomCharacterSessionId)}, "
            + $"CharacterSessionId={FormatDiagnosticValue(characterSessionId)}, "
            + $"CurrentSectionId={FormatDiagnosticValue(sectionId)}.",
            this);
    }

    private static string FormatDiagnosticValue(string value) =>
        string.IsNullOrEmpty(value) ? "<empty>" : value;

    private void TryAttachConvaiEvents()
    {
        if (convaiEvents != null || ConvaiManager.ActiveManager == null)
            return;

        try
        {
            convaiEvents = ConvaiManager.ActiveManager.Events;
            convaiEvents.OnFinalUserTranscriptionReceived += HandleFinalPlayerTranscript;
        }
        catch (InvalidOperationException)
        {
            convaiEvents = null;
        }
    }

    private void DetachConvaiEvents()
    {
        if (convaiEvents == null)
            return;

        convaiEvents.OnFinalUserTranscriptionReceived -= HandleFinalPlayerTranscript;
        convaiEvents = null;
    }

    private void ScheduleStartWorkDecisionFallback(string sourceSectionId)
    {
        if (officeDialogueController == null || !officeDialogueController.IsStartWorkAvailable)
            return;

        StopStartWorkDecisionFallback();
        startWorkDecisionFallbackRoutine = StartCoroutine(
            InvokeStartWorkFallbackAfterDelay(sourceSectionId));
    }

    private IEnumerator InvokeStartWorkFallbackAfterDelay(string sourceSectionId)
    {
        yield return new WaitForSecondsRealtime(decisionFallbackDelay);

        float speechWaitDeadline = Time.realtimeSinceStartup + startWorkTurnTimeout;
        while (character != null
               && character.IsSpeaking
               && Time.realtimeSinceStartup < speechWaitDeadline)
        {
            yield return null;
        }

        string currentSectionId = character != null
            ? character.NarrativeDesign.CurrentSectionId
            : string.Empty;
        string currentSectionName = ResolveSectionName(currentSectionId);

        if (currentSectionName == "office_start_work"
            || officeDialogueController == null
            || !officeDialogueController.IsStartWorkAvailable)
        {
            startWorkDecisionFallbackRoutine = null;
            yield break;
        }

        if (!string.IsNullOrEmpty(sourceSectionId)
            && !string.IsNullOrEmpty(currentSectionId)
            && sourceSectionId != currentSectionId)
        {
            startWorkDecisionFallbackRoutine = null;
            yield break;
        }

        startWorkDecisionFallbackRoutine = null;
        Debug.LogWarning(
            $"Convai Narrative Decision did not enter office_start_work within {decisionFallbackDelay:0.0}s. Sending the whitelisted office_start_work_requested trigger.",
            this);
        InvokeSavedTrigger(startWorkRequestedTrigger);
    }

    private void StopStartWorkDecisionFallback()
    {
        if (startWorkDecisionFallbackRoutine == null)
            return;

        StopCoroutine(startWorkDecisionFallbackRoutine);
        startWorkDecisionFallbackRoutine = null;
    }

    private void ScheduleReadingFinishedDecisionFallback(string sourceSectionId)
    {
        if (officeDialogueController == null
            || !officeDialogueController.IsWaitingForReadingConfirmation)
        {
            return;
        }

        StopReadingFinishedDecisionFallback();
        readingFinishedDecisionFallbackRoutine = StartCoroutine(
            InvokeReadingFinishedFallbackAfterDelay(sourceSectionId));
    }

    private IEnumerator InvokeReadingFinishedFallbackAfterDelay(string sourceSectionId)
    {
        yield return new WaitForSecondsRealtime(decisionFallbackDelay);

        float speechWaitDeadline = Time.realtimeSinceStartup + startWorkTurnTimeout;
        while (character != null
               && character.IsSpeaking
               && Time.realtimeSinceStartup < speechWaitDeadline)
        {
            yield return null;
        }

        string currentSectionId = character != null
            ? character.NarrativeDesign.CurrentSectionId
            : string.Empty;
        string currentSectionName = ResolveSectionName(currentSectionId);

        if (currentSectionName == "office_crystal_ball"
            || officeDialogueController == null
            || !officeDialogueController.IsWaitingForReadingConfirmation)
        {
            readingFinishedDecisionFallbackRoutine = null;
            yield break;
        }

        if (!string.IsNullOrEmpty(sourceSectionId)
            && !string.IsNullOrEmpty(currentSectionId)
            && sourceSectionId != currentSectionId)
        {
            readingFinishedDecisionFallbackRoutine = null;
            yield break;
        }

        readingFinishedDecisionFallbackRoutine = null;
        Debug.LogWarning(
            $"Convai Narrative Decision did not enter office_crystal_ball within {decisionFallbackDelay:0.0}s. Sending office_reading_finished.",
            this);
        InvokeSavedTrigger(officeReadingFinishedTrigger);
    }

    private void StopReadingFinishedDecisionFallback()
    {
        if (readingFinishedDecisionFallbackRoutine == null)
            return;

        StopCoroutine(readingFinishedDecisionFallbackRoutine);
        readingFinishedDecisionFallbackRoutine = null;
    }

    private static bool IsStartWorkIntent(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
            return false;

        string normalized = transcript.Trim().ToLowerInvariant();
        bool mentionsStart = normalized.Contains("start")
            || normalized.Contains("begin")
            || normalized.Contains("ready");
        bool mentionsWork = normalized.Contains("work")
            || normalized.Contains("session")
            || normalized.Contains("assignment");

        return mentionsStart && mentionsWork;
    }

    private static bool IsReadingFinishedIntent(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
            return false;

        string normalized = transcript.Trim().ToLowerInvariant();
        bool mentionsReading = normalized.Contains("read")
            || normalized.Contains("reading")
            || normalized.Contains("report");
        bool confirmsCompletion = normalized.Contains("finished")
            || normalized.Contains("finish")
            || normalized.Contains("done")
            || normalized.Contains("already")
            || normalized.Contains("go on")
            || normalized.Contains("continue")
            || normalized.Contains("next step");

        return mentionsReading && confirmsCompletion;
    }

    private string ResolveSectionName(string sectionId)
    {
        if (narrativeManager == null || string.IsNullOrEmpty(sectionId))
            return string.Empty;

        foreach (UnitySectionEventConfig config in narrativeManager.SectionConfigs)
        {
            if (config != null && config.SectionId == sectionId)
                return config.SectionName;
        }

        Debug.LogWarning($"ConvaiNarrativeBridge: No synced section name found for ID '{sectionId}'.", this);
        return string.Empty;
    }

    private void StopStartWorkTimeout()
    {
        if (startWorkTimeoutRoutine == null)
            return;

        StopCoroutine(startWorkTimeoutRoutine);
        startWorkTimeoutRoutine = null;
    }

    private void StartCharacterReadyWarning()
    {
        if (characterReadyWarningRoutine == null)
            characterReadyWarningRoutine = StartCoroutine(WarnIfCharacterNotReady());
    }

    private IEnumerator WarnIfCharacterNotReady()
    {
        yield return new WaitForSecondsRealtime(characterReadyWarningTimeout);
        characterReadyWarningRoutine = null;

        if (officeEntryPending)
        {
            Debug.LogError(
                $"ConvaiNarrativeBridge: {GetCharacterLabel()} did not become ready within {characterReadyWarningTimeout:0} seconds. office_entered has not been sent. Check the room connection and Character Ready event.",
                this);
        }
    }

    private void StopCharacterReadyWait()
    {
        if (characterReadyRoutine != null)
        {
            StopCoroutine(characterReadyRoutine);
            characterReadyRoutine = null;
        }

        if (characterReadyWarningRoutine != null)
        {
            StopCoroutine(characterReadyWarningRoutine);
            characterReadyWarningRoutine = null;
        }
    }

    private string GetCharacterLabel() =>
        character != null && !string.IsNullOrWhiteSpace(character.CharacterName)
            ? character.CharacterName
            : "Convai character";
}
