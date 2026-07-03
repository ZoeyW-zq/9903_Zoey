using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MemoryContentDisplay : MonoBehaviour
{
    [SerializeField] private GameObject displayRoot;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoOutput;
    [SerializeField] private Vector2Int renderTextureSize = new Vector2Int(1920, 1080);

    private RenderTexture runtimeRenderTexture;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);

        EnsureVideoOutput();
        Hide();
    }

    private void OnDestroy()
    {
        if (runtimeRenderTexture == null)
            return;

        if (videoPlayer != null && videoPlayer.targetTexture == runtimeRenderTexture)
            videoPlayer.targetTexture = null;

        runtimeRenderTexture.Release();
        Destroy(runtimeRenderTexture);
    }

    public void ShowDiaryMemory()
    {
        ShowContent();
    }

    public void ShowColdMedicineMemory()
    {
        ShowContent();
    }

    public void ShowSunsetToyMemory()
    {
        ShowContent();
    }

    public void Hide()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (displayRoot != null)
            displayRoot.SetActive(false);
    }

    private void ShowContent()
    {
        if (displayRoot != null)
            displayRoot.SetActive(true);

        if (videoPlayer == null)
        {
            Debug.LogError($"{nameof(MemoryContentDisplay)}：没有找到 VideoPlayer。请在当前物体或子物体上挂载 VideoPlayer，或在 Inspector 中手动指定。", this);
            return;
        }

        EnsureVideoOutput();
        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void EnsureVideoOutput()
    {
        if (videoPlayer == null)
            return;

        Transform outputParent = displayRoot != null ? displayRoot.transform : transform;

        if (videoOutput == null)
            videoOutput = outputParent.GetComponentInChildren<RawImage>(true);

        if (videoOutput == null)
        {
            GameObject outputObject = new GameObject("Video Output");
            outputObject.layer = outputParent.gameObject.layer;
            outputObject.transform.SetParent(outputParent, false);

            RectTransform rectTransform = outputObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            videoOutput = outputObject.AddComponent<RawImage>();
            videoOutput.raycastTarget = false;
        }

        if (videoPlayer.targetTexture == null)
        {
            int width = Mathf.Max(1, renderTextureSize.x);
            int height = Mathf.Max(1, renderTextureSize.y);

            runtimeRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                name = $"{nameof(MemoryContentDisplay)} Render Texture"
            };
            runtimeRenderTexture.Create();

            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = runtimeRenderTexture;
        }

        videoOutput.texture = videoPlayer.targetTexture;
        videoOutput.enabled = true;
    }
}
