using System.Reflection;
using NUnit.Framework;

public class AssistantControllerDialogueTests
{
    [Test]
    public void DialogueLineSupportsInspectorFallbackDurationWithoutAudio()
    {
        FieldInfo fallbackDuration = typeof(AssistantController.DialogueLine)
            .GetField("fallbackSubtitleDuration", BindingFlags.Instance | BindingFlags.Public);
        MethodInfo getLineDuration = typeof(AssistantController)
            .GetMethod("GetLineDuration", BindingFlags.Static | BindingFlags.Public, null,
                new[] { typeof(AssistantController.DialogueLine), typeof(float) }, null);

        Assert.That(fallbackDuration, Is.Not.Null);
        Assert.That(getLineDuration, Is.Not.Null);
    }
}
