using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFadeController : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    public float Alpha => fadeImage != null ? fadeImage.color.a : 0f;

    private void Awake()
    {
        SetAlpha(0f);
    }

    public IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeImage == null)
            yield break;

        if (duration <= 0f)
        {
            SetAlpha(targetAlpha);
            yield break;
        }

        float startAlpha = fadeImage.color.a;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = timer / duration;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    public void SetColor(Color color)
    {
        if (fadeImage == null)
            return;

        color.a = fadeImage.color.a;
        fadeImage.color = color;
    }

    public void SetAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        fadeImage.color = color;
    }
}
