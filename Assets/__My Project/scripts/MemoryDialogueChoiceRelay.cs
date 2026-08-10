using UnityEngine;

public class MemoryDialogueChoiceRelay : MonoBehaviour
{
    [SerializeField] private MemoryDialogueController controller;
    [SerializeField, Min(0)] private int choiceIndex;

    private void Awake()
    {
        FindControllerIfNeeded();
    }

    public void TriggerChoice()
    {
        FindControllerIfNeeded();

        if (controller == null)
            return;

        controller.SelectChoice(choiceIndex);
    }

    private void FindControllerIfNeeded()
    {
        if (controller == null)
            controller = GetComponentInParent<MemoryDialogueController>();
    }
}
