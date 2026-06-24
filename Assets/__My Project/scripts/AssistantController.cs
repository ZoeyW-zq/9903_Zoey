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

        public float fallbackDuration = 3f;

        public bool keepVisibleAfterLine;
    }

    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private GameStateController gameStateController;

    [Header("Office Intro")]
    [SerializeField] private DialogueLine[] officeIntroLines;

    [Header("Hippocampus Intro")]
    [SerializeField] private DialogueLine[] hippocampusIntroLines;

    private Coroutine dialogueRoutine;

    public void PlayIntro()
    {
        if (animator != null)
            animator.SetTrigger("Intro");

        PlayDialogue(officeIntroLines, () =>
        {
            gameStateController.SetState(GameStateController.GameState.AwaitCrystalBall);
        });
    }

    public void PlayHippocampusIntro()
    {
        PlayDialogue(hippocampusIntroLines, null);
    }

    private void PlayDialogue(DialogueLine[] lines, System.Action onComplete)
    {
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = StartCoroutine(RunDialogue(lines, onComplete));
    }

    private IEnumerator RunDialogue(DialogueLine[] lines, System.Action onComplete)
    {
        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            DialogueLine line = lines[i];

            if (subtitleText != null)
                subtitleText.text = line.text;

            if (audioSource != null && line.audioClip != null)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(line.audioClip);

                yield return new WaitForSeconds(line.audioClip.length);
            }
            else
            {
                yield return new WaitForSeconds(line.fallbackDuration);
            }

            bool isLastLine = i == lines.Length - 1;

            if (!line.keepVisibleAfterLine && !isLastLine)
            {
                if (subtitleText != null)
                    subtitleText.text = "";
            }
        }

        DialogueLine lastLine = lines[lines.Length - 1];

        if (!lastLine.keepVisibleAfterLine)
        {
            if (subtitleText != null)
                subtitleText.text = "";
        }

        dialogueRoutine = null;
        onComplete?.Invoke();
    }
}