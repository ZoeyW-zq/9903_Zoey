using System.Reflection;
using Convai.Runtime.Components;
using NUnit.Framework;
using UnityEngine;

public class ConvaiNarrativeBridgeTests
{
    [TestCase("I want to start work now", true)]
    [TestCase("I'm ready to begin the session", true)]
    [TestCase("Tell me about the work", false)]
    [TestCase("I want to explore the office", false)]
    public void StartWorkFallbackUsesARestrictedIntentWhitelist(string transcript, bool expected)
    {
        MethodInfo method = typeof(ConvaiNarrativeBridge)
            .GetMethod("IsStartWorkIntent", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        Assert.That(method.Invoke(null, new object[] { transcript }), Is.EqualTo(expected));
    }

    [TestCase("I have finished reading", true)]
    [TestCase("I am ready. I have already finished reading.", true)]
    [TestCase("I'll finish reading. We can just go on.", true)]
    [TestCase("Where is the crystal ball?", false)]
    [TestCase("What does this report mean?", false)]
    public void ReadingFallbackUsesARestrictedIntentWhitelist(string transcript, bool expected)
    {
        MethodInfo method = typeof(ConvaiNarrativeBridge)
            .GetMethod("IsReadingFinishedIntent", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        Assert.That(method.Invoke(null, new object[] { transcript }), Is.EqualTo(expected));
    }

    [Test]
    public void OfficeEntryWaitsForCharacterReadyBeforeInvokingTrigger()
    {
        GameObject gameObject = new GameObject("Convai Narrative Bridge Test");
        try
        {
            gameObject.SetActive(false);

            ConvaiCharacter character = gameObject.AddComponent<ConvaiCharacter>();
            OfficeDialogueController office = gameObject.AddComponent<OfficeDialogueController>();
            ConvaiNarrativeBridge bridge = gameObject.AddComponent<ConvaiNarrativeBridge>();

            SetPrivateField(office, "dialogueMode", OfficeDialogueController.DialogueMode.ExternalConvai);
            SetPrivateField(bridge, "character", character);
            SetPrivateField(bridge, "officeDialogueController", office);

            bool triggerInvoked = false;
            character.NarrativeDesign.OnTriggerInvoked += _ => triggerInvoked = true;

            gameObject.SetActive(true);
            office.BeginOfficeDialogue();

            Assert.That(triggerInvoked, Is.False);
            Assert.That(GetPrivateField<bool>(bridge, "officeEntryPending"), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void FinalConfirmationAllowsOfficeReportBeforePhysicalTeleport()
    {
        GameObject gameObject = new GameObject("Convai Narrative Guard Test");
        try
        {
            GameStateController gameState = gameObject.AddComponent<GameStateController>();
            ConvaiNarrativeBridge bridge = gameObject.AddComponent<ConvaiNarrativeBridge>();
            gameState.enabled = false;
            bridge.enabled = false;

            SetPrivateField(bridge, "gameStateController", gameState);
            SetPrivateField(bridge, "waitingForFinalDistributionTurn", true);
            gameState.SetState(GameStateController.GameState.FinalMemoryPlacement);

            MethodInfo guard = typeof(ConvaiNarrativeBridge)
                .GetMethod("ShouldRejectOfficeRegression", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(guard.Invoke(bridge, new object[] { "office_report" }), Is.False);
            Assert.That(guard.Invoke(bridge, new object[] { "office_job_explanation" }), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void SessionCloseWaitsForExternalFarewellCompletion()
    {
        GameObject gameObject = new GameObject("Convai Session Close Test");
        try
        {
            GameStateController gameState = gameObject.AddComponent<GameStateController>();
            gameState.enabled = false;
            SetPrivateField(gameState, "state", GameStateController.GameState.BackToOffice);

            bool closeRequested = false;
            gameState.SessionCloseRequested += () => closeRequested = true;

            gameState.HandleCloseSessionRequested();

            Assert.That(closeRequested, Is.True);
            Assert.That(gameState.State, Is.EqualTo(GameStateController.GameState.BackToOffice));

            gameState.ContinueAfterSessionClosing();

            Assert.That(gameState.State, Is.EqualTo(GameStateController.GameState.SessionComplete));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void EverySuccessfulMirrorResolutionPublishesTheUpdatedCount()
    {
        GameObject gameObject = new GameObject("Mirror Resolution Context Test");
        try
        {
            GameStateController gameState = gameObject.AddComponent<GameStateController>();
            gameState.enabled = false;
            SetPrivateField(gameState, "state", GameStateController.GameState.MirrorChamber);

            int latestCount = 0;
            int eventCount = 0;
            gameState.MirrorMemoryResolved += count =>
            {
                latestCount = count;
                eventCount++;
            };

            for (int i = 0; i < 4; i++)
                gameState.HandleFirstMemoryReleased();

            Assert.That(eventCount, Is.EqualTo(4));
            Assert.That(latestCount, Is.EqualTo(4));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        return (T)target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(target);
    }
}
