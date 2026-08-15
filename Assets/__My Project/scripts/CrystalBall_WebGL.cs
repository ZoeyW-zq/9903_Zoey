using System.Collections;
using UnityEngine;

public class CrystalBall_WebGL : MonoBehaviour, ICrystalBallEntry
{
    [Header("References")]
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private ScreenFadeController screenFadeController;

    [Header("Settings")]
    [SerializeField] private float transitionFadeDuration = 2f;
    [SerializeField] private float fadeResetDuration = 0.35f;
    [SerializeField] private Color entryFadeColor = Color.white;
    [SerializeField] private bool enabledForEntry = true;

    private bool triggered;
    private Coroutine transitionRoutine;
    private Coroutine fadeResetRoutine;

    public void Transition()
    {
        if (!enabledForEntry || triggered)
            return;

        if (gameStateController == null)
        {
            Debug.LogError("CrystalBall_WebGL: missing GameStateController reference.", this);
            return;
        }

        triggered = true;
        StopFadeReset();

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine());
    }

    public void SetEnabled(bool value)
    {
        enabledForEntry = value;

        if (value)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            triggered = false;
            StopTransition();
            StopFadeReset();

            if (screenFadeController != null)
            {
                SetEntryFadeColor();
                screenFadeController.SetAlpha(0f);
            }
        }
        else
        {
            StopTransition();
            StopFadeReset();

            if (!triggered && screenFadeController != null)
            {
                SetEntryFadeColor();
                screenFadeController.SetAlpha(0f);
            }

            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }

    private IEnumerator TransitionRoutine()
    {
        Debug.Log("Crystal Ball WebGL entry triggered.");

        if (screenFadeController != null)
        {
            SetEntryFadeColor();
            yield return screenFadeController.FadeTo(1f, transitionFadeDuration);
            screenFadeController.SetAlpha(1f);
        }

        transitionRoutine = null;
        gameStateController.SetState(GameStateController.GameState.TransitionToHippocampus);
    }

    private void SetEntryFadeColor()
    {
        if (screenFadeController == null)
            return;

        screenFadeController.SetColor(entryFadeColor);
    }

    private void ResetFadeToTransparent()
    {
        if (screenFadeController == null)
            return;

        StopFadeReset();
        fadeResetRoutine = StartCoroutine(FadeToTransparentRoutine());
    }

    private IEnumerator FadeToTransparentRoutine()
    {
        yield return screenFadeController.FadeTo(0f, fadeResetDuration);
        fadeResetRoutine = null;
    }

    private void StopTransition()
    {
        if (transitionRoutine == null)
            return;

        StopCoroutine(transitionRoutine);
        transitionRoutine = null;
    }

    private void StopFadeReset()
    {
        if (fadeResetRoutine == null)
            return;

        StopCoroutine(fadeResetRoutine);
        fadeResetRoutine = null;
    }
}
