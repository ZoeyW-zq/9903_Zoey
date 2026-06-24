using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryContentDisplay : MonoBehaviour
{
    [SerializeField] private GameObject displayRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image memoryImage;

    [Header("Diary Memory")]
    [SerializeField] private Sprite diaryImage;

    [Header("Cold Medicine Memory")]
    [SerializeField] private Sprite coldMedicineImage;

    [Header("Sunset Toy Memory")]
    [SerializeField] private Sprite sunsetToyImage;

    private void Awake()
    {
        Hide();
    }

    public void ShowDiaryMemory()
    {
        ShowContent("Diary", diaryImage);
    }

    public void ShowColdMedicineMemory()
    {
        ShowContent("Cold Medicine", coldMedicineImage);
    }

    public void ShowSunsetToyMemory()
    {
        ShowContent("Little Sun Toy", sunsetToyImage);
    }

    public void Hide()
    {
        if (displayRoot != null)
            displayRoot.SetActive(false);
    }

    private void ShowContent(string title, Sprite image)
    {
        if (displayRoot != null)
            displayRoot.SetActive(true);

        if (titleText != null)
            titleText.text = title;

        if (memoryImage != null)
        {
            memoryImage.sprite = image;
            memoryImage.enabled = image != null;
        }
    }
}