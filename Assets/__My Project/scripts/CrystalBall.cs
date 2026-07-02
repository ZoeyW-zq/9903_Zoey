using System.Collections;
using UnityEngine;

public class CrystalBall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform crystalBallCenter;
    [SerializeField] private Transform leftHandProxy;
    [SerializeField] private Transform rightHandProxy;
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private ScreenFadeController screenFadeController;

    [Header("Settings")]
    [SerializeField] private float holdDistance = 0.2f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float fadeResetDuration = 0.35f;
    [SerializeField] private Color entryFadeColor = Color.white;
    [SerializeField] private bool enabledForEntry = true;

    [Header("Debug")]
    [SerializeField] private bool handInRange;
    [SerializeField] private float timer;
    [SerializeField] private float leftDistance;
    [SerializeField] private float rightDistance;
    [SerializeField] private float minDistance;

    private bool triggered;
    private Coroutine fadeResetRoutine;

    private void Update()
    {
        if (!enabledForEntry || triggered)
            return;

        if (crystalBallCenter == null || leftHandProxy == null || rightHandProxy == null)
            return;

        leftDistance = Vector3.Distance(leftHandProxy.position, crystalBallCenter.position);
        rightDistance = Vector3.Distance(rightHandProxy.position, crystalBallCenter.position);

        minDistance = Mathf.Min(leftDistance, rightDistance);

        handInRange = minDistance <= holdDistance;

        if (handInRange)
        {
            StopFadeReset();

            SetEntryFadeColor();

            timer += Time.deltaTime;
            UpdateFadeProgress();

            if (timer >= holdTime)
            {
                triggered = true;
                timer = holdTime;
                Debug.Log("Crystal Ball entry triggered.");

                if (screenFadeController != null)
                {
                    SetEntryFadeColor();
                    screenFadeController.SetAlpha(1f);
                }

                gameStateController.SetState(GameStateController.GameState.TransitionToHippocampus);
            }
        }
        else
        {
            ResetHoldProgress();
        }
    }

    public void SetEnabled(bool value)
    {
        enabledForEntry = value;
        timer = 0f;
        handInRange = false;

        if (value)
        {
            triggered = false;
            StopFadeReset();

            if (screenFadeController != null)
            {
                SetEntryFadeColor();
                screenFadeController.SetAlpha(0f);
            }
        }
        else if (!triggered)
        {
            ResetFadeToTransparent();
        }
    }

    private void UpdateFadeProgress()
    {
        if (screenFadeController == null)
            return;

        float progress = holdTime > 0f ? timer / holdTime : 1f;
        SetEntryFadeColor();
        screenFadeController.SetAlpha(progress);
    }

    private void SetEntryFadeColor()
    {
        if (screenFadeController == null)
            return;

        screenFadeController.SetColor(entryFadeColor);
    }

    private void ResetHoldProgress()
    {
        if (timer <= 0f)
            return;

        // Moving away cancels the entry attempt completely.
        timer = 0f;
        ResetFadeToTransparent();
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

    private void StopFadeReset()
    {
        if (fadeResetRoutine == null)
            return;

        StopCoroutine(fadeResetRoutine);
        fadeResetRoutine = null;
    }
}
