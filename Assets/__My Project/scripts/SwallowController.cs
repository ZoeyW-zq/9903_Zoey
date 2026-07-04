using System.Collections;
using UnityEngine;

public class SwallowController : MonoBehaviour
{
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private ScreenFadeController screenFadeController;
    [SerializeField] private AssistantController assistantController;

    [Header("Player")]
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform pipeStartPoint;
    [SerializeField] private Transform stomachLandingPoint;

    [Header("Audio")]
    [SerializeField] private AudioSource afterTeleportAudio;

    [Header("Timing")]
    [SerializeField] private float fadeOutDuration = 2f;
    [Tooltip("How long the screen stays fully black after the swallow dialogue starts.")]
    [SerializeField] private float blackHoldDuration = 2f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fallDuration = 4f;

    [Header("Transition Color")]
    [SerializeField] private Color swallowFadeColor = Color.black;

    [Header("Motion")]
    [SerializeField] private float horizontalSwayAmount = 0.08f;
    [SerializeField] private float horizontalSwaySpeed = 5f;

    private bool transitionRunning;

    public void StartSwallowTransition()
    {
        if (transitionRunning)
            return;

        StartCoroutine(RunSwallowTransition());
    }

    private IEnumerator RunSwallowTransition()
    {
        transitionRunning = true;

        if (screenFadeController != null)
            screenFadeController.SetColor(swallowFadeColor);

        if (assistantController != null)
            assistantController.PlaySwallowTransition();

        yield return screenFadeController.FadeTo(1f, fadeOutDuration);

        //yield return new WaitForSeconds(0.5f);

        if (gameStateController != null)
            gameStateController.ReleaseClownPlayerControl();


        if (afterTeleportAudio != null)
            afterTeleportAudio.Play();

        if (blackHoldDuration > 0f)
            yield return new WaitForSeconds(blackHoldDuration);

        // Teleport while fully faded out, then let the player fall into the next space.
        xrOrigin.position = pipeStartPoint.position;
        xrOrigin.rotation = pipeStartPoint.rotation;

        gameStateController.SetState(GameStateController.GameState.MirrorChamber);

        yield return screenFadeController.FadeTo(0f, fadeInDuration);

        yield return ControlledFall();

        transitionRunning = false;
    }

    private IEnumerator ControlledFall()
    {
        Vector3 startPosition = pipeStartPoint.position;
        Vector3 endPosition = stomachLandingPoint.position;

        Quaternion startRotation = pipeStartPoint.rotation;
        Quaternion endRotation = stomachLandingPoint.rotation;

        float timer = 0f;

        while (timer < fallDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fallDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 basePosition = Vector3.Lerp(startPosition, endPosition, easedT);

            Vector3 sway = new Vector3(
                Mathf.Sin(timer * horizontalSwaySpeed) * horizontalSwayAmount,
                0f,
                Mathf.Cos(timer * horizontalSwaySpeed) * horizontalSwayAmount
            );

            xrOrigin.position = basePosition + sway;
            xrOrigin.rotation = Quaternion.Slerp(startRotation, endRotation, easedT);

            yield return null;
        }

        xrOrigin.position = endPosition;
        xrOrigin.rotation = endRotation;

        if (gameStateController != null)
            gameStateController.SetPlayerMovementLocked(false);
    }
}
