using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class OfficeDialogueControllerTests
{
    [Test]
    public void OfficeControllerExposesTheConfiguredOfficeFlow()
    {
        Type controllerType = Type.GetType("OfficeDialogueController, Assembly-CSharp");

        Assert.That(controllerType, Is.Not.Null);
        Assert.That(controllerType.GetMethod("BeginOfficeDialogue", BindingFlags.Instance | BindingFlags.Public), Is.Not.Null);
        Assert.That(controllerType.GetMethod("SelectJobExplanation", BindingFlags.Instance | BindingFlags.Public), Is.Not.Null);
        Assert.That(controllerType.GetMethod("SelectExploreOffice", BindingFlags.Instance | BindingFlags.Public), Is.Not.Null);
        Assert.That(controllerType.GetMethod("ReturnFromExploration", BindingFlags.Instance | BindingFlags.Public), Is.Not.Null);
        Assert.That(controllerType.GetMethod("SelectStartWork", BindingFlags.Instance | BindingFlags.Public), Is.Not.Null);
    }

    [Test]
    public void GameStateIncludesOfficeDialogueBeforeCrystalBallEntry()
    {
        Assert.That(Enum.IsDefined(typeof(GameStateController.GameState), "OfficeDialogue"), Is.True);
    }

    [Test]
    public void LegacyOfficeIntroEntryIsRemoved()
    {
        Assert.That(typeof(AssistantController).GetField("officeIntroLines", BindingFlags.Instance | BindingFlags.NonPublic), Is.Null);
        Assert.That(typeof(AssistantController).GetMethod("PlayIntro", BindingFlags.Instance | BindingFlags.Public), Is.Null);
        Assert.That(Enum.IsDefined(typeof(GameStateController.GameState), "OfficeIntro"), Is.False);
    }

    [Test]
    public void MissingAssistantDoesNotCompleteOfficeDialogue()
    {
        GameObject gameObject = new GameObject("Office Dialogue Test");
        OfficeDialogueController controller = gameObject.AddComponent<OfficeDialogueController>();
        MethodInfo playMethod = typeof(OfficeDialogueController)
            .GetMethod("Play", BindingFlags.Instance | BindingFlags.NonPublic);
        bool completed = false;

        playMethod.Invoke(controller, new object[]
        {
            Array.Empty<AssistantController.DialogueLine>(),
            new Action(() => completed = true)
        });

        UnityEngine.Object.DestroyImmediate(gameObject);

        Assert.That(completed, Is.False);
    }

    [Test]
    public void FinishReadingBeginsCrystalBallInstructionDialogue()
    {
        GameObject assistantObject = new GameObject("Assistant Test");
        GameObject gameStateObject = new GameObject("Game State Test");
        GameObject officeObject = new GameObject("Office Dialogue Test");
        AssistantController assistant = assistantObject.AddComponent<AssistantController>();
        GameStateController gameState = gameStateObject.AddComponent<GameStateController>();
        OfficeDialogueController office = officeObject.AddComponent<OfficeDialogueController>();
        gameState.enabled = false;

        SetPrivateField(office, "assistantController", assistant);
        SetPrivateField(office, "gameStateController", gameState);
        SetPrivateField(office, "startWorkLines", Array.Empty<AssistantController.DialogueLine>());
        SetPrivateField(office, "crystalBallInstructionLines", new[]
        {
            new AssistantController.DialogueLine { fallbackSubtitleDuration = 2f }
        });

        office.SelectStartWork();
        office.FinishReading();

        Assert.That(gameState.State, Is.EqualTo(GameStateController.GameState.AwaitCrystalBall));
        Assert.That(typeof(AssistantController)
            .GetField("dialogueRoutine", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(assistant), Is.Not.Null);

        UnityEngine.Object.DestroyImmediate(assistantObject);
        UnityEngine.Object.DestroyImmediate(gameStateObject);
        UnityEngine.Object.DestroyImmediate(officeObject);
    }

    [Test]
    public void JobExplanationCompletionEnablesTheRemainingChoices()
    {
        AssistantController assistant;
        OfficeDialogueController office = CreateOfficeController(out assistant);

        office.SelectJobExplanation();

        Assert.That(office.IsJobExplanationAvailable, Is.False);
        Assert.That(office.IsExploreOfficeAvailable, Is.True);
        Assert.That(office.IsStartWorkAvailable, Is.True);

        DestroyOfficeController(assistant, office);
    }

    [Test]
    public void ExplorationReturnLeavesOnlyWaitAndStartChoices()
    {
        AssistantController assistant;
        OfficeDialogueController office = CreateOfficeController(out assistant);

        office.SelectExploreOffice();
        office.MarkExplorationReturnAreaExited();
        office.ReturnFromExploration();

        Assert.That(office.IsJobExplanationAvailable, Is.False);
        Assert.That(office.IsExploreOfficeAvailable, Is.True);
        Assert.That(office.IsStartWorkAvailable, Is.True);

        DestroyOfficeController(assistant, office);
    }

    [Test]
    public void ExternalModeInitializesWithoutRecordedAssistant()
    {
        GameObject officeObject = new GameObject("External Office Test");
        OfficeDialogueController office = officeObject.AddComponent<OfficeDialogueController>();
        SetPrivateField(office, "dialogueMode", OfficeDialogueController.DialogueMode.ExternalConvai);

        bool began = false;
        office.ExternalDialogueBegan += () => began = true;
        office.BeginOfficeDialogue();

        Assert.That(began, Is.True);
        Assert.That(office.IsJobExplanationAvailable, Is.True);
        Assert.That(office.IsExploreOfficeAvailable, Is.True);
        Assert.That(office.IsStartWorkAvailable, Is.True);

        UnityEngine.Object.DestroyImmediate(officeObject);
    }

    [Test]
    public void ExternalExplorationReturnRaisesBridgeEvent()
    {
        GameObject officeObject = new GameObject("External Office Test");
        OfficeDialogueController office = officeObject.AddComponent<OfficeDialogueController>();
        SetPrivateField(office, "dialogueMode", OfficeDialogueController.DialogueMode.ExternalConvai);
        office.BeginOfficeDialogue();
        office.SelectExploreOffice();

        bool returned = false;
        office.ExternalExplorationReturned += () => returned = true;
        office.MarkExplorationReturnAreaExited();
        office.ReturnFromExploration();

        Assert.That(returned, Is.True);
        Assert.That(office.IsExploring, Is.False);

        UnityEngine.Object.DestroyImmediate(officeObject);
    }

    [Test]
    public void ExplorationCannotReturnBeforePlayerLeavesTheReturnArea()
    {
        GameObject officeObject = new GameObject("External Office Test");
        OfficeDialogueController office = officeObject.AddComponent<OfficeDialogueController>();
        SetPrivateField(office, "dialogueMode", OfficeDialogueController.DialogueMode.ExternalConvai);
        office.BeginOfficeDialogue();
        office.SelectExploreOffice();

        bool returned = false;
        office.ExternalExplorationReturned += () => returned = true;
        office.ReturnFromExploration();

        Assert.That(returned, Is.False);
        Assert.That(office.IsExploring, Is.True);

        office.MarkExplorationReturnAreaExited();
        office.ReturnFromExploration();

        Assert.That(returned, Is.True);
        Assert.That(office.IsExploring, Is.False);

        UnityEngine.Object.DestroyImmediate(officeObject);
    }

    [Test]
    public void ExternalStartWorkWaitsForTurnCompletionBeforeShowingReport()
    {
        GameObject officeObject = new GameObject("External Office Test");
        GameObject report = new GameObject("Report");
        report.SetActive(false);
        OfficeDialogueController office = officeObject.AddComponent<OfficeDialogueController>();
        SetPrivateField(office, "dialogueMode", OfficeDialogueController.DialogueMode.ExternalConvai);
        SetPrivateField(office, "computerScreenContent", report);
        office.BeginOfficeDialogue();

        office.SelectStartWork();

        Assert.That(office.IsWaitingForReadingConfirmation, Is.False);
        Assert.That(report.activeSelf, Is.False);

        office.CompleteExternalStartWork();

        Assert.That(office.IsWaitingForReadingConfirmation, Is.True);
        Assert.That(report.activeSelf, Is.True);

        UnityEngine.Object.DestroyImmediate(report);
        UnityEngine.Object.DestroyImmediate(officeObject);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        typeof(OfficeDialogueController)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private static OfficeDialogueController CreateOfficeController(out AssistantController assistant)
    {
        GameObject assistantObject = new GameObject("Assistant Test");
        GameObject officeObject = new GameObject("Office Dialogue Test");
        assistant = assistantObject.AddComponent<AssistantController>();
        OfficeDialogueController office = officeObject.AddComponent<OfficeDialogueController>();

        SetPrivateField(office, "assistantController", assistant);
        SetPrivateField(office, "jobExplanationLines", Array.Empty<AssistantController.DialogueLine>());
        SetPrivateField(office, "explorationDepartureLines", Array.Empty<AssistantController.DialogueLine>());
        SetPrivateField(office, "explorationReturnLines", Array.Empty<AssistantController.DialogueLine>());

        return office;
    }

    private static void DestroyOfficeController(AssistantController assistant, OfficeDialogueController office)
    {
        UnityEngine.Object.DestroyImmediate(assistant.gameObject);
        UnityEngine.Object.DestroyImmediate(office.gameObject);
    }
}
