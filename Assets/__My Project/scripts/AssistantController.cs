using System.Collections;
using TMPro;
using UnityEngine;

public class AssistantController : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea]
        public string text;

        public AudioClip audioClip;

        [Min(0f)]
        public float fallbackSubtitleDuration = 2f;

        public bool keepVisibleAfterLine;
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Hippocampus Intro")]
    [SerializeField] private DialogueLine[] hippocampusIntroLines;

    [Header("Per-Object Memory Reveal")]
    [SerializeField] private DialogueLine[] waterBottleMemoryLines;
    [SerializeField] private DialogueLine[] sunsetPhotoMemoryLines;
    [SerializeField] private DialogueLine[] legoBricksMemoryLines;

    [Header("Missing Painful Memories")]
    [SerializeField] private DialogueLine[] missingPainfulMemoryLines;

    [Header("Nightmare Warning")]
    [SerializeField] private DialogueLine[] nightmareWarningLines;

    [Header("Swallow Transition")]
    [SerializeField] private DialogueLine[] swallowTransitionLines;

    [Header("Dungeon Intro")]
    [SerializeField] private DialogueLine[] mirrorChamberIntroLines;

    [Header("Mirror Resolution")]
    [SerializeField] private DialogueLine[] memoryReleasedLines;
    [SerializeField] private DialogueLine[] allMemoriesReleasedLines;
    [SerializeField] private DialogueLine[] returnToMemorySpaceLines;

    [Header("Final Redistribution")]
    [SerializeField] private DialogueLine[] confirmationResponseLines;
    [SerializeField] private DialogueLine[] officeReturnAndReportLines;

    [Header("Session Closing")]
    [SerializeField] private DialogueLine[] sessionClosingLines =
    {
        new DialogueLine
        {
            text = "The client is still asleep. But when they wake, they may notice that something has changed.",
            fallbackSubtitleDuration = 4f
        },
        new DialogueLine
        {
            text = "We'll find out soon enough. For now, this session is complete. Closing the connection.",
            fallbackSubtitleDuration = 4f
        }
    };

    private Coroutine dialogueRoutine;
    private bool waterBottleMemoryPlayed;
    private bool sunsetPhotoMemoryPlayed;
    private bool legoBricksMemoryPlayed;
    private bool missingPainfulMemoryPlaying;
    private bool missingPainfulMemoryPlayed;
    private System.Action dialogueInterruptedCallback;

    private void Awake()
    {
        ConfigureAudioSource();
        StopAudio();
        ClearSubtitle();
    }

    public void PlayHippocampusIntro()
    {
        PlayStage(hippocampusIntroLines);
    }

    public void PlayWaterBottleMemory()
    {
        PlayOnce(ref waterBottleMemoryPlayed, waterBottleMemoryLines);
    }

    public void PlaySunsetPhotoMemory()
    {
        PlayOnce(ref sunsetPhotoMemoryPlayed, sunsetPhotoMemoryLines);
    }

    public void PlayLegoBricksMemory()
    {
        PlayOnce(ref legoBricksMemoryPlayed, legoBricksMemoryLines);
    }

    public void PlayMissingPainfulMemories(System.Action onComplete = null)
    {
        if (missingPainfulMemoryPlayed || missingPainfulMemoryPlaying)
        {
            onComplete?.Invoke();
            return;
        }

        System.Action finishMissingMemoryCue = () =>
        {
            missingPainfulMemoryPlaying = false;
            missingPainfulMemoryPlayed = true;
            onComplete?.Invoke();
        };

        missingPainfulMemoryPlaying = true;
        PlayStage(missingPainfulMemoryLines, finishMissingMemoryCue, finishMissingMemoryCue);
    }

    public void PlayNightmareWarning()
    {
        PlayStage(nightmareWarningLines);
    }

    public void PlaySwallowTransition()
    {
        PlayStage(swallowTransitionLines);
    }

    public void PlayMirrorChamberIntro()
    {
        PlayStage(mirrorChamberIntroLines);
    }

    public void PlayMemoryReleased()
    {
        PlayStage(memoryReleasedLines);
    }

    public void PlayAllMemoriesReleasedSequence(System.Action onComplete = null)
    {
        PlayStage(allMemoriesReleasedLines, onComplete);
    }

    public void PlayReturnToMemorySpaceSequence(System.Action onComplete = null)
    {
        PlayStage(returnToMemorySpaceLines, onComplete);
    }

    public void PlayConfirmationResponse(System.Action onComplete = null)
    {
        PlayStage(confirmationResponseLines, onComplete);
    }

    public void PlayOfficeReturnAndReport(System.Action onComplete = null)
    {
        PlayStage(officeReturnAndReportLines, onComplete);
    }

    public void PlaySessionClosing(System.Action onComplete = null)
    {
        PlayStage(sessionClosingLines, onComplete);
    }

    public void ClearSubtitle()
    {
        if (subtitleText != null)
            subtitleText.text = string.Empty;
    }

    public void PlayDialogue(DialogueLine[] lines, System.Action onComplete = null)
    {
        PlayStage(lines, onComplete);
    }

    private void PlayOnce(ref bool played, DialogueLine[] lines)
    {
        if (played)
            return;

        played = true;
        PlayStage(lines);
    }

    private void PlayStage(
        DialogueLine[] lines,
        System.Action onComplete = null,
        System.Action onInterrupted = null)
    {
        StopActiveDialogue();

        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        dialogueInterruptedCallback = onInterrupted;
        dialogueRoutine = StartCoroutine(RunStage(lines, onComplete));
    }

    private void StopActiveDialogue()
    {
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = null;
        System.Action onInterrupted = dialogueInterruptedCallback;
        dialogueInterruptedCallback = null;
        StopAudio();
        ClearSubtitle();
        onInterrupted?.Invoke();
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
            return;

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void StopAudio()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
        audioSource.clip = null;
    }

    private void SetSubtitle(string text)
    {
        if (subtitleText != null)
            subtitleText.text = text;
    }

    private bool TryPlayAudio(DialogueLine line)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Assistant dialogue has no AudioSource assigned.", this);
            return false;
        }

        if (line.audioClip == null)
        {
            Debug.LogWarning("Assistant dialogue line has no audio clip assigned.", this);
            return false;
        }

        audioSource.Stop();
        audioSource.clip = line.audioClip;
        audioSource.Play();

        return true;
    }

    public static float GetLineDuration(DialogueLine line, float pitch)
    {
        if (line == null)
            return 0f;

        if (line.audioClip == null)
            return Mathf.Max(0f, line.fallbackSubtitleDuration);

        return line.audioClip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
    }

    private IEnumerator RunStage(DialogueLine[] lines, System.Action onComplete)
    {
        // Dialogue timing follows the assigned audio clips so text and voice stay in sync from the Inspector.
        for (int i = 0; i < lines.Length; i++)
        {
            DialogueLine line = lines[i];

            if (line == null)
                continue;

            SetSubtitle(line.text);

            bool hasAudio = line.audioClip != null && TryPlayAudio(line);
            float pitch = audioSource != null ? audioSource.pitch : 1f;
            float duration = GetLineDuration(line, pitch);

            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            if (hasAudio)
            {
                StopAudio();
            }

            if (!line.keepVisibleAfterLine)
                ClearSubtitle();
        }

        StopAudio();
        dialogueRoutine = null;
        dialogueInterruptedCallback = null;
        onComplete?.Invoke();
    }
}
