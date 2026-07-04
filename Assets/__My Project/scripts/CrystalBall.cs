using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class CrystalBall : MonoBehaviour, ICrystalBallEntry
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

    [Header("Haptic Feedback")]
    [SerializeField, Range(0f, 1f)] private float hapticAmplitude = 0.35f;
    [SerializeField] private float hapticDuration = 0.08f;
    [SerializeField] private float hapticInterval = 0.2f;

    private float timer;
    private float hapticTimer;
    private bool triggered;
    private Coroutine fadeResetRoutine;

    private void Update()
    {
        if (!enabledForEntry || triggered)
            return;

        if (crystalBallCenter == null || leftHandProxy == null || rightHandProxy == null)
            return;

        float leftDistance = Vector3.Distance(leftHandProxy.position, crystalBallCenter.position);
        float rightDistance = Vector3.Distance(rightHandProxy.position, crystalBallCenter.position);

        float minDistance = Mathf.Min(leftDistance, rightDistance);

        bool handInRange = minDistance <= holdDistance;

        if (handInRange)
        {
            UpdateHapticFeedback(leftDistance <= holdDistance, rightDistance <= holdDistance);
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
            ResetHapticFeedback();
            ResetHoldProgress();
        }
    }

    public void SetEnabled(bool value)
    {
        enabledForEntry = value;
        timer = 0f;
        ResetHapticFeedback();

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

    private void UpdateHapticFeedback(bool leftHandInRange, bool rightHandInRange)
    {
        if (!leftHandInRange && !rightHandInRange)
        {
            ResetHapticFeedback();
            return;
        }

        hapticTimer -= Time.deltaTime;

        if (hapticTimer > 0f)
            return;

        if (leftHandInRange)
            SendHapticImpulse(XRNode.LeftHand);

        if (rightHandInRange)
            SendHapticImpulse(XRNode.RightHand);

        hapticTimer = Mathf.Max(0.01f, hapticInterval);
    }

    private void SendHapticImpulse(XRNode handNode)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(handNode);

        if (!device.isValid)
            return;

        if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
            device.SendHapticImpulse(0u, hapticAmplitude, hapticDuration);
    }

    private void ResetHapticFeedback()
    {
        hapticTimer = 0f;
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
