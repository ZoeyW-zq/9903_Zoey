using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ClownController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private AssistantController assistantController;

    [Header("Animation")]
    [SerializeField] private Animator giantAnimator;
    [SerializeField] private string bendPickTriggerName = "BendPick";
    [SerializeField] private float animationLeadTime = 0.8f;

    [Header("Rigging")]
    [SerializeField] private Rig rightArmRig;
    [SerializeField] private Transform handIKTarget;
    [SerializeField] private Transform handGrabAnchor;

    [Header("Path Points")]
    [SerializeField] private Transform roofPoint;
    [SerializeField] private Transform dropRoofPoint;
    [SerializeField] private Transform mouthPoint;

    [Header("Objects")]
    [SerializeField] private Transform roofPiece;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform playerHead;

    [Header("Audio")]
    [SerializeField] private AudioSource footstepsAudio;
    [SerializeField] private AudioSource rumbleAudio;

    [Header("Timing")]
    [SerializeField] private float rigBlendDuration = 0.5f;
    [SerializeField] private float moveToRoofDuration = 1.2f;
    [SerializeField] private float pullRoofDuration = 1.2f;
    [SerializeField] private float moveToMouthDuration = 2f;

    [Header("Distances")]
    [SerializeField] private float grabDistance = 0.45f;

    private Coroutine crisisRoutine;
    private bool crisisTriggered;

    public void TriggerCrisis()
    {
        if (crisisTriggered)
            return;

        crisisTriggered = true;
        gameStateController.SetState(GameStateController.GameState.GiantCrisis);
    }

    public void StartCrisisSequence()
    {
        if (crisisRoutine != null)
            StopCoroutine(crisisRoutine);

        crisisRoutine = StartCoroutine(CrisisRoutine());
    }

    private IEnumerator CrisisRoutine()
    {
        if (footstepsAudio != null)
            footstepsAudio.Play();

        if (rumbleAudio != null)
            rumbleAudio.Play();

        if (assistantController != null)
            assistantController.PlayNightmareWarning();

        if (giantAnimator != null)
            giantAnimator.SetTrigger(bendPickTriggerName);

        yield return new WaitForSeconds(animationLeadTime);

        yield return BlendRigWeight(1f, rigBlendDuration);

        yield return MoveHandTo(roofPoint.position, moveToRoofDuration);

        AttachRoofToHand();

        yield return MoveHandTo(dropRoofPoint.position, pullRoofDuration);

        ReleaseRoof();

        yield return ReachTowardPlayer();

        AttachPlayerToHand();

        yield return MoveHandTo(mouthPoint.position, moveToMouthDuration);

        DetachPlayerFromHand();

        gameStateController.SetState(GameStateController.GameState.SwallowTransition);
    }

    private IEnumerator MoveHandTo(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = handIKTarget.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            handIKTarget.position = Vector3.Lerp(startPosition, targetPosition, easedT);

            yield return null;
        }

        handIKTarget.position = targetPosition;
    }

    private IEnumerator ReachTowardPlayer()
    {
        while (Vector3.Distance(handIKTarget.position, playerHead.position) > grabDistance)
        {
            handIKTarget.position = Vector3.Lerp(
                handIKTarget.position,
                playerHead.position,
                Time.deltaTime * 2.5f
            );

            yield return null;
        }
    }

    private IEnumerator BlendRigWeight(float targetWeight, float duration)
    {
        if (rightArmRig == null)
            yield break;

        float startWeight = rightArmRig.weight;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            rightArmRig.weight = Mathf.Lerp(startWeight, targetWeight, t);

            yield return null;
        }

        rightArmRig.weight = targetWeight;
    }

    private void AttachRoofToHand()
    {
        if (roofPiece == null || handGrabAnchor == null)
            return;

        Rigidbody rb = roofPiece.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        roofPiece.SetParent(handGrabAnchor, true);
    }

    private void ReleaseRoof()
    {
        if (roofPiece == null)
            return;

        roofPiece.SetParent(null, true);

        Rigidbody rb = roofPiece.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;
    }

    private void AttachPlayerToHand()
    {
        if (xrOrigin == null || handGrabAnchor == null)
            return;

        xrOrigin.SetParent(handGrabAnchor, true);
    }

    private void DetachPlayerFromHand()
    {
        if (xrOrigin == null)
            return;

        xrOrigin.SetParent(null, true);
    }
}