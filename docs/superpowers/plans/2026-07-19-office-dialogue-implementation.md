# Office Dialogue Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Inspector-configurable Office dialogue and three XR world-space choices while enabling the crystal ball only after Start Work.

**Architecture:** Reuse `AssistantController` as the dialogue player. Add one `OfficeDialogueController` containing all Office state, UI references, and callbacks. Update `GameStateController` to start that flow.

**Tech Stack:** Unity 6000.4.4f1, C#, Unity UI, TextMeshPro, XR Interaction Toolkit, NUnit.

---

## Files

- Modify: `Assets/__My Project/scripts/AssistantController.cs`
- Create: `Assets/__My Project/scripts/OfficeDialogueController.cs`
- Modify: `Assets/__My Project/scripts/GameStateController.cs`
- Create: `Assets/__My Project/tests/Editor/OfficeDialogueControllerTests.cs`
- Modify: `PROJECT_CONTEXT.md`

### Task 1: Reuse the Existing Dialogue Player

**Files:** `AssistantController.cs`, `OfficeDialogueControllerTests.cs`

- [ ] Write the failing test:

```csharp
[Test]
public void AudioLessLineUsesConfiguredFallbackDuration()
{
    var line = new AssistantController.DialogueLine { fallbackSubtitleDuration = 2.5f };
    Assert.That(AssistantController.GetLineDuration(line, 1f), Is.EqualTo(2.5f));
}
```

- [ ] Run it in Unity Edit Mode Test Runner. Expected: compile failure because the field and overload do not exist.

- [ ] Add to `AssistantController`:

```csharp
[Min(0f)] public float fallbackSubtitleDuration = 2f;

public void PlayDialogue(DialogueLine[] lines, System.Action onComplete = null)
{
    PlayStage(lines, onComplete);
}

public static float GetLineDuration(DialogueLine line, float pitch)
{
    if (line == null) return 0f;
    if (line.audioClip == null) return Mathf.Max(0f, line.fallbackSubtitleDuration);
    return line.audioClip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
}
```

Update `RunStage` to wait for that duration even when `audioClip` is null, but only call `TryPlayAudio` when an audio clip exists.

- [ ] Re-run the test. Expected: pass.

### Task 2: Add the Single Office Controller

**Files:** `OfficeDialogueController.cs`, `OfficeDialogueControllerTests.cs`

- [ ] Write failing tests for the public state:

```csharp
[Test]
public void JobExplanationIsUnavailableAfterSelection()
{
    var controller = CreateController();
    controller.SelectJobExplanation();
    Assert.That(controller.HasHeardJobExplanation, Is.True);
    Assert.That(controller.IsJobExplanationAvailable, Is.False);
}

[Test]
public void ReturnFromExplorationDoesNothingBeforeExplorationBegins()
{
    var controller = CreateController();
    controller.ReturnFromExploration();
    Assert.That(controller.HasExploredOffice, Is.False);
}

[Test]
public void StartWorkMakesAllChoicesUnavailable()
{
    var controller = CreateController();
    controller.SelectStartWork();
    Assert.That(controller.IsReadyToStart, Is.True);
    Assert.That(controller.IsStartWorkAvailable, Is.False);
}
```

- [ ] Run the tests. Expected: compile failure because `OfficeDialogueController` does not exist.

- [ ] Create `OfficeDialogueController.cs` with only these Inspector dependencies:

```csharp
[SerializeField] private AssistantController assistantController;
[SerializeField] private GameStateController gameStateController;
[SerializeField] private GameObject choiceCanvas;
[SerializeField] private Button[] choiceButtons = new Button[3];
[SerializeField] private TMP_Text[] choiceLabels = new TMP_Text[3];
[SerializeField] private AssistantController.DialogueLine[] greetingLines;
[SerializeField] private AssistantController.DialogueLine[] jobExplanationLines;
[SerializeField] private AssistantController.DialogueLine[] explorationDepartureLines;
[SerializeField] private AssistantController.DialogueLine[] explorationReturnLines;
[SerializeField] private AssistantController.DialogueLine[] startWorkLines;
[SerializeField] private AssistantController.DialogueLine[] crystalBallInstructionLines;
```

Implement `BeginOfficeDialogue`, `SelectJobExplanation`, `SelectExploreOffice`, `ReturnFromExploration`, `SelectStartWork`, `RefreshChoices`, and `HideChoices`. Store only `isBusy`, `isExploring`, `HasHeardJobExplanation`, `HasExploredOffice`, and `IsReadyToStart`. `RefreshChoices` removes old listeners, binds currently valid options, labels them, and hides unused buttons. Start Work plays its two arrays in sequence and then sets `AwaitCrystalBall`.

- [ ] Re-run all Office Edit Mode tests. Expected: pass.

### Task 3: Connect Game State and Verify the Scene

**Files:** `GameStateController.cs`, `scene_VR.unity`, `PROJECT_CONTEXT.md`

- [ ] Add `OfficeDialogue` to the game-state enum, a serialized `OfficeDialogueController` reference, and change `Start()` to enter that state.

```csharp
case GameState.OfficeDialogue:
    SetActiveSceneRoot(SceneRoot.Office);
    SwitchToOfficeVolume();
    SetCrystalBallEnabled(false);
    if (officeDialogueController != null)
        officeDialogueController.BeginOfficeDialogue();
    else
        Debug.LogWarning("GameStateController: OfficeDialogueController is not assigned.", this);
    break;
```

Keep `OfficeIntro` and every later state untouched.

- [ ] In the scene, create a World Space Canvas with three Buttons/TMP labels, `TrackedDeviceGraphicRaycaster`, and an EventSystem using `XR UI Input Module`. Assign its references and all Office content in `OfficeDialogueController`. Bind an exploration return trigger to `ReturnFromExploration()`.

- [ ] Run `dotnet build Assembly-CSharp.csproj --no-restore` and the Unity Edit Mode test suite. Expected: zero build errors and passing Office tests.

- [ ] Play Mode verify: greeting shows all choices; Job Explanation disappears after use; exploration requires explicit return; Start Work plays both arrays and enables the crystal ball; repeated clicks during dialogue do nothing.

- [ ] Update `PROJECT_CONTEXT.md`, run `git diff --check`, then commit only the feature files and the two revised docs.

## Self-Review

This plan adds one runtime script only. It preserves Inspector authoring, uses existing dialogue playback, avoids duplicate UI/state helpers, and covers every Office rule in the approved design.
