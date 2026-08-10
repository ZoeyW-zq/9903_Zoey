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
        public bool goesToRound2;

        [Range(0, 1)]
        public int nextBranchIndex;

        [TextArea(2, 4)]
        public string responseText;

        public AudioClip responseClip;
    }

    [Serializable]
    private class Round2BranchData
    {
        public GameObject choicesRoot;

        [TextArea(2, 4)]
        public string promptText;
        public AudioClip promptClip;
        public Round2ChoiceData[] choices = new Round2ChoiceData[2];
    }

    [Serializable]
    private class Round2ChoiceData
    {
        [TextArea(2, 4)]
        public string responseText;
        public AudioClip responseClip;
        public bool completesMemory;
    }

    [Header("UI")]
    [SerializeField] private GameObject startButtonRoot;
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private GameObject round1ChoicesRoot;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Audio")]
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private float durationWithoutAudio = 3f;

    [Header("Opening")]
    [TextArea(2, 4)]
    [SerializeField] private string openingText;
    [SerializeField] private AudioClip openingClip;

    [Header("Round 1")]
    [SerializeField] private Round1ChoiceData[] round1Choices = new Round1ChoiceData[3];

    [Header("Round 2")]
    [SerializeField] private Round2BranchData[] round2Branches = new Round2BranchData[2];

    [Header("Completion")]
    [SerializeField] private UnityEvent onComplete;

    private bool playerInArea;
    private bool conversationActive;
    private bool completed;
    private ConversationStage stage = ConversationStage.Idle;
    private int currentBranchIndex = -1;
    private Coroutine flowRoutine;

    private void Awake()
    {
        NormalizeData();
        HideDialogue();
        RefreshStartButton();
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
        ShowLine(openingText, openingClip);
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
            CloseConversationAfterDelay(choice.responseClip);
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
            CompleteConversationAfterDelay(choice.responseClip);
            return;
        }

        CloseConversationAfterDelay(choice.responseClip);
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

    private void CompleteConversationAfterDelay(AudioClip sourceClip)
    {
        StartFlowRoutine(CompleteConversationRoutine(sourceClip));
    }

    private IEnumerator CompleteConversationRoutine(AudioClip sourceClip)
    {
        yield return new WaitForSeconds(GetDelay(sourceClip));
        flowRoutine = null;
        CompleteConversation();
    }

    private void CloseConversationAfterDelay(AudioClip sourceClip)
    {
        StartFlowRoutine(CloseConversationRoutine(sourceClip));
    }

    private IEnumerator CloseConversationRoutine(AudioClip sourceClip)
    {
        yield return new WaitForSeconds(GetDelay(sourceClip));
        flowRoutine = null;
        CloseConversation();
    }

    private void CompleteConversation()
    {
        if (completed)
            return;

        completed = true;
        onComplete?.Invoke();
        CloseConversation();
    }

    private void CloseConversation()
    {
        conversationActive = false;
        stage = ConversationStage.Idle;
        currentBranchIndex = -1;

        HideDialogue();
        RefreshStartButton();
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
        if (dialogueText != null)
            dialogueText.text = text;

        if (dialogueAudioSource == null)
            return;

        dialogueAudioSource.Stop();
        dialogueAudioSource.clip = clip;

        if (clip != null)
            dialogueAudioSource.Play();
    }

    private void StopAudio()
    {
        if (dialogueAudioSource == null)
            return;

        dialogueAudioSource.Stop();
        dialogueAudioSource.clip = null;
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
        if (clip != null && clip.length > 0f)
            return clip.length;

        return durationWithoutAudio;
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
