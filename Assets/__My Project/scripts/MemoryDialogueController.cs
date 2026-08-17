using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MemoryDialogueController : MonoBehaviour
{
    private enum ConversationStage
    {
        Idle,
        Round1,
        Round2
    }

    [Serializable]
    private class Round1ChoiceData
    {
        public bool goesToRound2 = false;

        [Range(0, 1)]
        public int nextBranchIndex = 0;

        [TextArea(2, 4)]
        public string responseText = string.Empty;

        public AudioClip responseClip = null;
    }

    [Serializable]
    private class Round2BranchData
    {
        public GameObject choicesRoot = null;

        [TextArea(2, 4)]
        public string promptText = string.Empty;
        public AudioClip promptClip = null;
        public Round2ChoiceData[] choices = new Round2ChoiceData[2];
    }

    [Serializable]
    private class Round2ChoiceData
    {
        [TextArea(2, 4)]
        public string responseText = string.Empty;
        public AudioClip responseClip = null;
        public bool completesMemory = false;
    }

    [Header("UI")]
    [SerializeField] private GameObject startButtonRoot;
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private GameObject round1ChoicesRoot;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Audio")]
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private WebGLEchoAudio webGLEchoAudio;
    [SerializeField, Min(0f)] private float durationWithoutAudio = 3f;
    [SerializeField, Min(0f)] private float openingLoopInterval = 1f;

    [Header("Opening")]
    [TextArea(2, 4)]
    [SerializeField] private string openingText;
    [SerializeField] private AudioClip openingClip;

    [Header("Round 1")]
    [SerializeField] private Round1ChoiceData[] round1Choices = new Round1ChoiceData[3];

    [Header("Round 2")]
    [SerializeField] private Round2BranchData[] round2Branches = new Round2BranchData[2];

    [Header("Completion")]
    [SerializeField] private AudioClip completionSfxClip;
    [SerializeField] private UnityEvent onComplete;

    private bool playerInArea;
    private bool conversationActive;
    private bool completed;
    private ConversationStage stage = ConversationStage.Idle;
    private int currentBranchIndex = -1;
    private Coroutine flowRoutine;
    private Coroutine openingLoopRoutine;

    private void Awake()
    {
        NormalizeData();
        ConfigureAudioSource();
        HideDialogue();
        RefreshStartButton();
    }

    private void OnEnable()
    {
        PlayOpeningLoop();
    }

    private void OnDisable()
    {
        StopFlowRoutine();
        StopAudio();
    }

    private void OnValidate()
    {
        NormalizeData();
    }

    public void SetPlayerInArea(bool inside)
    {
        playerInArea = inside;

        if (!conversationActive)
            RefreshStartButton();
    }

    public void BeginConversation()
    {
        if (completed || conversationActive)
            return;

        conversationActive = true;
        stage = ConversationStage.Round1;
        currentBranchIndex = -1;

        HideStartButton();
        ShowDialogue();
        ShowRound1Choices();
        ShowText(openingText);
    }

    public void SelectChoice(int choiceIndex)
    {
        if (!conversationActive || completed)
            return;

        if (stage == ConversationStage.Round1)
        {
            SelectRound1Choice(choiceIndex);
            return;
        }

        if (stage == ConversationStage.Round2)
            SelectRound2Choice(choiceIndex);
    }

    private void SelectRound1Choice(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= round1Choices.Length)
            return;

        Round1ChoiceData choice = round1Choices[choiceIndex];
        if (choice == null)
            return;

        if (!choice.goesToRound2)
        {
            HideChoiceRoots();
            ShowLine(choice.responseText, choice.responseClip);
            EndConversationAfterDelay(choice.responseClip, false);
            return;
        }

        currentBranchIndex = Mathf.Clamp(choice.nextBranchIndex, 0, round2Branches.Length - 1);
        AdvanceToRound2();
    }

    private void SelectRound2Choice(int choiceIndex)
    {
        if (currentBranchIndex < 0 || currentBranchIndex >= round2Branches.Length)
            return;

        Round2BranchData branch = round2Branches[currentBranchIndex];
        if (branch == null || branch.choices == null || choiceIndex < 0 || choiceIndex >= branch.choices.Length)
            return;

        Round2ChoiceData choice = branch.choices[choiceIndex];
        if (choice == null)
            return;

        HideChoiceRoots();
        ShowLine(choice.responseText, choice.responseClip);

        if (choice.completesMemory)
        {
            EndConversationAfterDelay(choice.responseClip, true);
            return;
        }

        EndConversationAfterDelay(choice.responseClip, false);
    }

    private void AdvanceToRound2()
    {
        if (!conversationActive || completed || currentBranchIndex < 0 || currentBranchIndex >= round2Branches.Length)
            return;

        Round2BranchData branch = round2Branches[currentBranchIndex];
        if (branch == null)
            return;

        stage = ConversationStage.Round2;
        ShowLine(branch.promptText, branch.promptClip);
        ShowRound2Choices();
    }

    private void EndConversationAfterDelay(AudioClip sourceClip, bool completeMemory)
    {
        StartFlowRoutine(EndConversationRoutine(sourceClip, completeMemory));
    }

    private IEnumerator EndConversationRoutine(AudioClip sourceClip, bool completeMemory)
    {
        yield return new WaitForSeconds(GetDelay(sourceClip));
        flowRoutine = null;

        if (completeMemory)
            CompleteConversation();
        else
            CloseConversation();
    }

    private void CompleteConversation()
    {
        if (completed)
            return;

        completed = true;
        PlayCompletionSfx();
        onComplete?.Invoke();
        CloseConversation();
    }

    private void PlayCompletionSfx()
    {
        if (completionSfxClip == null)
            return;

        Vector3 position = dialogueAudioSource != null
            ? dialogueAudioSource.transform.position
            : transform.position;

        AudioSource.PlayClipAtPoint(completionSfxClip, position);
    }

    private void CloseConversation()
    {
        conversationActive = false;
        stage = ConversationStage.Idle;
        currentBranchIndex = -1;

        HideDialogue();
        RefreshStartButton();

        if (!completed)
            PlayOpeningLoop();
    }

    private void ShowDialogue()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);
    }

    private void HideDialogue()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        HideChoiceRoots();
        StopAudio();
    }

    private void ShowRound1Choices()
    {
        if (round1ChoicesRoot != null)
            round1ChoicesRoot.SetActive(true);

        HideRound2ChoiceRoots();
    }

    private void ShowRound2Choices()
    {
        if (round1ChoicesRoot != null)
            round1ChoicesRoot.SetActive(false);

        HideRound2ChoiceRoots();

        if (currentBranchIndex < 0 || currentBranchIndex >= round2Branches.Length)
            return;

        Round2BranchData branch = round2Branches[currentBranchIndex];
        if (branch != null && branch.choicesRoot != null)
            branch.choicesRoot.SetActive(true);
    }

    private void HideChoiceRoots()
    {
        if (round1ChoicesRoot != null)
            round1ChoicesRoot.SetActive(false);

        HideRound2ChoiceRoots();
    }

    private void HideRound2ChoiceRoots()
    {
        if (round2Branches == null)
            return;

        foreach (Round2BranchData branch in round2Branches)
        {
            if (branch != null && branch.choicesRoot != null)
                branch.choicesRoot.SetActive(false);
        }
    }

    private void ShowLine(string text, AudioClip clip)
    {
        ShowText(text);

        if (dialogueAudioSource == null)
            return;

        StopOpeningLoop();
        PlayClip(clip);
    }

    private void ShowText(string text)
    {
        if (dialogueText != null)
            dialogueText.text = text;
    }

    private void StopAudio()
    {
        StopOpeningLoop();

        if (dialogueAudioSource == null)
            return;

        if (webGLEchoAudio != null)
            webGLEchoAudio.Stop();
        else
            dialogueAudioSource.Stop();

        dialogueAudioSource.loop = false;
        dialogueAudioSource.clip = null;
    }

    private void ConfigureAudioSource()
    {
        if (dialogueAudioSource == null)
            return;

        dialogueAudioSource.playOnAwake = false;
        dialogueAudioSource.loop = false;
        dialogueAudioSource.spatialBlend = 1f;

        if (webGLEchoAudio == null)
            webGLEchoAudio = GetComponent<WebGLEchoAudio>();

        if (webGLEchoAudio == null)
            webGLEchoAudio = gameObject.AddComponent<WebGLEchoAudio>();

        webGLEchoAudio.Initialize(dialogueAudioSource);
    }

    private void PlayOpeningLoop()
    {
        if (!isActiveAndEnabled || completed || conversationActive
            || dialogueAudioSource == null || openingClip == null)
        {
            return;
        }

        StopOpeningLoop();
        if (webGLEchoAudio != null)
            webGLEchoAudio.Stop();
        else
            dialogueAudioSource.Stop();

        dialogueAudioSource.clip = openingClip;
        openingLoopRoutine = StartCoroutine(OpeningLoopRoutine());
    }

    private IEnumerator OpeningLoopRoutine()
    {
        while (isActiveAndEnabled && !completed && dialogueAudioSource != null && openingClip != null)
        {
            PlayClip(openingClip);

            yield return new WaitForSeconds(openingClip.length);

            if (openingLoopInterval > 0f)
                yield return new WaitForSeconds(openingLoopInterval);
        }

        openingLoopRoutine = null;
    }

    private void StopOpeningLoop()
    {
        if (openingLoopRoutine == null)
            return;

        StopCoroutine(openingLoopRoutine);
        openingLoopRoutine = null;
    }

    private void RefreshStartButton()
    {
        if (startButtonRoot == null)
            return;

        startButtonRoot.SetActive(playerInArea && !completed && !conversationActive);
    }

    private void HideStartButton()
    {
        if (startButtonRoot != null)
            startButtonRoot.SetActive(false);
    }

    private float GetDelay(AudioClip clip)
    {
        float echoTail = webGLEchoAudio != null && webGLEchoAudio.IsFallbackActive
            ? webGLEchoAudio.TailDuration
            : 0f;

        if (clip != null && clip.length > 0f)
            return clip.length + echoTail;

        return durationWithoutAudio + echoTail;
    }

    private void PlayClip(AudioClip clip)
    {
        if (dialogueAudioSource == null)
            return;

        dialogueAudioSource.loop = false;

        if (webGLEchoAudio != null)
        {
            webGLEchoAudio.Play(clip);
            return;
        }

        dialogueAudioSource.Stop();
        dialogueAudioSource.clip = clip;

        if (clip != null)
            dialogueAudioSource.Play();
    }

    private void StartFlowRoutine(IEnumerator routine)
    {
        StopFlowRoutine();
        flowRoutine = StartCoroutine(routine);
    }

    private void StopFlowRoutine()
    {
        if (flowRoutine == null)
            return;

        StopCoroutine(flowRoutine);
        flowRoutine = null;
    }

    private void NormalizeData()
    {
        if (round1Choices == null || round1Choices.Length != 3)
            Array.Resize(ref round1Choices, 3);

        for (int i = 0; i < round1Choices.Length; i++)
        {
            if (round1Choices[i] == null)
                round1Choices[i] = new Round1ChoiceData();
        }

        if (round2Branches == null || round2Branches.Length != 2)
            Array.Resize(ref round2Branches, 2);

        for (int i = 0; i < round2Branches.Length; i++)
        {
            if (round2Branches[i] == null)
                round2Branches[i] = new Round2BranchData();

            Round2ChoiceData[] choices = round2Branches[i].choices;
            if (choices == null || choices.Length != 2)
            {
                choices = new Round2ChoiceData[2];
                round2Branches[i].choices = choices;
            }

            for (int j = 0; j < choices.Length; j++)
            {
                if (choices[j] == null)
                    choices[j] = new Round2ChoiceData();
            }
        }
    }
}
