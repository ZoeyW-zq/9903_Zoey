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
    [SerializeField] private GameStateController gameStateController;

    [Header("Hippocampus Intro")]
    [SerializeField] private DialogueLine[] hippocampusIntroLines;

    [Header("Per-Object Memory Reveal")]
    [SerializeField] private DialogueLine[] waterBottleMemoryLines;
    [SerializeField] private DialogueLine[] sunsetPhotoMemoryLines;
    [SerializeField] private DialogueLine[] legoBricksMemoryLines;

    [Header("Nightmare Warning")]
    [SerializeField] private DialogueLine[] nightmareWarningLines;

    [Header("Swallow Transition")]
    [SerializeField] private DialogueLine[] swallowTransitionLines;

    [Header("Dungeon Intro")]
    [SerializeField] private DialogueLine[] mirrorChamberIntroLines;

    [Header("Break Glass")]
    [SerializeField] private DialogueLine[] breakGlassLines;

    [Header("Glass Broken Praise")]
    [SerializeField] private DialogueLine[] glassBrokenPraiseLines;

    [Header("Return To Office")]
    [SerializeField] private DialogueLine[] returnToOfficeLines;

    private Coroutine dialogueRoutine;
    private bool glassBrokenSequenceStarted;
    private bool waterBottleMemoryPlayed;
    private bool sunsetPhotoMemoryPlayed;
    private bool legoBricksMemoryPlayed;

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
        if (waterBottleMemoryPlayed)
            return;

        waterBottleMemoryPlayed = true;
        PlayStage(waterBottleMemoryLines);
    }

    public void PlaySunsetPhotoMemory()
    {
        if (sunsetPhotoMemoryPlayed)
            return;

        sunsetPhotoMemoryPlayed = true;
        PlayStage(sunsetPhotoMemoryLines);
    }

    public void PlayLegoBricksMemory()
    {
        if (legoBricksMemoryPlayed)
            return;

        legoBricksMemoryPlayed = true;
        PlayStage(legoBricksMemoryLines);
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
        glassBrokenSequenceStarted = false;
        PlayStage(mirrorChamberIntroLines, PlayBreakGlassPrompt);
    }

    public void PlayBreakGlassPrompt()
    {
        PlayStage(breakGlassLines);
    }

    public void PlayGlassBrokenReturnSequence()
    {
        if (glassBrokenSequenceStarted)
            return;

        glassBrokenSequenceStarted = true;
        SetGameState(GameStateController.GameState.BreakGlass);
        PlayStage(glassBrokenPraiseLines, () =>
        {
            PlayStage(returnToOfficeLines, () => SetGameState(GameStateController.GameState.BackToOffice));
        });
    }

    public void ClearSubtitle()
    {
        if (subtitleText != null)
            subtitleText.text = "";
    }

    public void PlayDialogue(DialogueLine[] lines, System.Action onComplete = null)
    {
        PlayStage(lines, onComplete);
    }

    private void PlayStage(DialogueLine[] lines, System.Action onComplete = null)
    {
        StopActiveDialogue();

        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        dialogueRoutine = StartCoroutine(RunStage(lines, onComplete));
    }

    private void StopActiveDialogue()
    {
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = null;
        StopAudio();
        ClearSubtitle();
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

    private void SetGameState(GameStateController.GameState state)
    {
        if (gameStateController != null)
            gameStateController.SetState(state);
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

        ConfigureAudioSource();
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
        onComplete?.Invoke();
    }
}
