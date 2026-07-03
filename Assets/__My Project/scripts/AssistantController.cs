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

        public bool keepVisibleAfterLine;
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private GameStateController gameStateController;

    [Header("Office Intro")]
    [SerializeField] private DialogueLine[] officeIntroLines;

    [Header("Hippocampus Intro")]
    [SerializeField] private DialogueLine[] hippocampusIntroLines;

    [Header("Memory Revealed")]
    [SerializeField] private DialogueLine[] memoryRevealedLines;

    [Header("Memory Placement Hint")]
    [SerializeField] private DialogueLine[] memoryPlacementHintLines;

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

    private void Awake()
    {
        ConfigureAudioSource();
        StopAudio();
        ClearSubtitle();
    }

    public void PlayIntro()
    {
        PlayStage(officeIntroLines, () => SetGameState(GameStateController.GameState.AwaitCrystalBall));
    }

    public void PlayHippocampusIntro()
    {
        PlayStage(hippocampusIntroLines);
    }

    public void PlayMemoryRevealed()
    {
        PlayStage(memoryRevealedLines, PlayMemoryPlacementHint);
    }

    public void PlayMemoryPlacementHint()
    {
        PlayStage(memoryPlacementHintLines, () => SetGameState(GameStateController.GameState.AwaitMemoryPlacement));
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

    private float GetLineDuration(DialogueLine line)
    {
        if (line == null || line.audioClip == null)
            return 0f;

        float pitch = audioSource != null ? Mathf.Abs(audioSource.pitch) : 1f;
        if (pitch <= 0f)
            pitch = 1f;

        return line.audioClip.length / pitch;
    }

    private IEnumerator RunStage(DialogueLine[] lines, System.Action onComplete)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            DialogueLine line = lines[i];

            if (line == null)
                continue;

            SetSubtitle(line.text);

            if (TryPlayAudio(line))
            {
                yield return new WaitForSeconds(GetLineDuration(line));
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
