using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ClownController : MonoBehaviour
{
    [Header("Flow References")]
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private AssistantController assistantController;

    [Header("Giant Animation")]
    [SerializeField] private Animator giantAnimator;
    [SerializeField] private string bendPickTriggerName = "BendPick";
    [SerializeField] private float animationLeadTime = 0.5f;

    [Header("Arm IK")]
    [SerializeField] private Rig rightArmRig;
    [SerializeField] private Transform handIKTarget;
    [Tooltip("Optional visible hand or wrist reference used for arrival checks.")]
    [SerializeField] private Transform handTipReference;
    [Tooltip("Shared anchor for the roof, player follow, and grab arrival checks.")]
    [SerializeField] private Transform grabAnchor;

    [Header("Path Points")]
    [SerializeField] private Transform roofPoint;
    [SerializeField] private Transform dropRoofPoint;
    [SerializeField] private Transform mouthPoint;

    [Header("Scene Objects")]
    [SerializeField] private Transform roofPiece;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform playerHead;

    [Header("Audio")]
    [SerializeField] private AudioSource footstepsAudio;
    [SerializeField] private AudioSource rumbleAudio;
    [SerializeField] private AudioSource groanAudio;
    [SerializeField] private AudioSource postGroanAudio;

    [Header("Timing")]
    [SerializeField] private float rigBlendDuration = 0.5f;
    [SerializeField] private float moveToRoofDuration = 0.5f;
    [SerializeField] private float roofAttachDuration = 0.45f;
    [SerializeField] private bool alignRoofRotationToGrabAnchor;
    [SerializeField] private float pullRoofDuration = 0.5f;
    [SerializeField] private float moveToMouthDuration = 4f;

    [Header("Post Grab Animation")]
    [SerializeField] private float rigBlendOutDuration;
    [Tooltip("How long the original clown animation continues after the player is attached.")]
    [SerializeField] private float postGrabAnimationHoldDuration;
    [SerializeField] private float rigBlendInDuration;

    [Header("Distances And Timeouts")]
    [SerializeField] private float grabDistance = 1f;
    [SerializeField] private float reachToPlayerSpeed = 2.5f;
    [Tooltip("How close the visible hand reference must be to the mouth before the swallow transition can continue.")]
    [SerializeField] private float mouthArrivalDistance = 0.1f;
    [Tooltip("Safety timeout for the mouth arrival check.")]
    [SerializeField] private float mouthArrivalTimeout = 5f;
    [Tooltip("How quickly the IK target keeps catching up to the animated mouth point while waiting.")]
    [SerializeField] private float mouthTrackingCatchupSpeed = 6f;

    private Coroutine crisisRoutine;
    private bool crisisTriggered;
    private bool playerFollowingGrabAnchor;
    private Vector3 playerGrabOffset;
    private Coroutine grabAudioRoutine;

    private void LateUpdate()
    {
        if (!playerFollowingGrabAnchor || xrOrigin == null || grabAnchor == null)
            return;

        ApplyPlayerFollowPosition();
    }

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

    public void ReleasePlayerControl()
    {
        DetachPlayerFromHand();
    }

    private IEnumerator CrisisRoutine()
    {
        // Assistant dialogue, giant audio, animation, IK, and player movement are intentionally sequenced here.
        if (assistantController != null)
            assistantController.PlayNightmareWarning();

        yield return PlayAudioAndWait(footstepsAudio);

        if (rumbleAudio != null)
        {
            PlayAudio(rumbleAudio);
            yield return new WaitForSeconds(1f);
        }

        if (giantAnimator != null)
            giantAnimator.SetTrigger(bendPickTriggerName);

        yield return new WaitForSeconds(animationLeadTime);
        yield return BlendRigWeight(1f, rigBlendDuration);
        yield return MoveHandTo(roofPoint, moveToRoofDuration);
        yield return AttachRoofToHand();
        yield return MoveHandTo(dropRoofPoint, pullRoofDuration);

        ReleaseRoof();

        yield return ReachTowardPlayer();

        AttachPlayerToHand();
        PlayGrabAudioSequence();

        yield return BlendRigWeight(0f, rigBlendOutDuration);

        if (postGrabAnimationHoldDuration > 0f)
            yield return new WaitForSeconds(postGrabAnimationHoldDuration);

        SyncHandIKTargetToVisibleHand();

        yield return BlendRigWeight(1f, rigBlendInDuration);

        gameStateController.SetState(GameStateController.GameState.SwallowTransition);

        yield return MoveHandTo(mouthPoint, moveToMouthDuration, true);
        yield return WaitForHandNear(mouthPoint, mouthArrivalDistance, mouthArrivalTimeout);
        DetachPlayerFromHand();
    }

    private void PlayAudio(AudioSource audioSource)
    {
        if (audioSource == null)
            return;

        audioSource.Play();
    }

    private IEnumerator PlayAudioAndWait(AudioSource audioSource)
    {
        // Wait only for this source, so assistant dialogue and other audio systems stay independent.
        if (audioSource == null)
            yield break;

        PlayAudio(audioSource);

        while (audioSource.isPlaying)
            yield return null;
    }

    private void PlayGrabAudioSequence()
    {
        if (grabAudioRoutine != null)
            StopCoroutine(grabAudioRoutine);

        grabAudioRoutine = StartCoroutine(PlayGrabAudioSequenceRoutine());
    }

    private IEnumerator PlayGrabAudioSequenceRoutine()
    {
        if (groanAudio != null)
            yield return PlayAudioAndWait(groanAudio);

        PlayAudio(postGroanAudio);

        grabAudioRoutine = null;
    }

    private IEnumerator MoveHandTo(Transform target, float duration, bool trackTargetEachFrame = false)
    {
        // Dynamic targets such as the mouth can move with animation, so sample target.position every frame.
        if (handIKTarget == null || target == null)
            yield break;

        if (duration <= 0f)
        {
            handIKTarget.position = target.position;
            yield break;
        }

        Vector3 startPosition = handIKTarget.position;
        Vector3 targetPosition = target.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            if (trackTargetEachFrame)
                targetPosition = target.position;

            handIKTarget.position = Vector3.Lerp(startPosition, targetPosition, easedT);

            yield return null;
        }

        handIKTarget.position = trackTargetEachFrame ? target.position : targetPosition;
    }

    private IEnumerator WaitForHandNear(Transform target, float distance, float timeout)
    {
        // Arrival checks prefer the visible hand reference because the IK target can arrive before the mesh does.
        if (handIKTarget == null || target == null)
            yield break;

        Transform arrivalReference = GetHandArrivalReference();
        float timer = 0f;

        while (Vector3.Distance(arrivalReference.position, target.position) > distance)
        {
            if (timeout > 0f && timer >= timeout)
            {
                Debug.LogWarning(
                    $"{nameof(ClownController)}: hand did not reach {target.name} before the swallow timeout. Check the IK chain, markers, target point, and arrival distance.",
                    this
                );
                yield break;
            }

            timer += Time.deltaTime;

            handIKTarget.position = Vector3.Lerp(
                handIKTarget.position,
                target.position,
                Time.deltaTime * mouthTrackingCatchupSpeed
            );

            yield return null;
        }
    }

    private Transform GetHandArrivalReference()
    {
        if (handTipReference != null)
            return handTipReference;

        if (grabAnchor != null)
            return grabAnchor;

        return handIKTarget;
    }

    private IEnumerator ReachTowardPlayer()
    {
        bool missingReferenceLogged = false;
        while (handIKTarget == null || playerHead == null)
        {
            if (!missingReferenceLogged)
            {
                Debug.LogError($"{nameof(ClownController)}: missing handIKTarget or playerHead, so the giant cannot grab the player.", this);
                missingReferenceLogged = true;
            }

            yield return null;
        }

        if (gameStateController != null)
            gameStateController.SetPlayerMovementLocked(true);

        Transform grabReference = GetGrabReference();

        while (Vector3.Distance(grabReference.position, playerHead.position) > grabDistance)
        {
            handIKTarget.position = Vector3.Lerp(
                handIKTarget.position,
                playerHead.position,
                Time.deltaTime * reachToPlayerSpeed
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

    private IEnumerator AttachRoofToHand()
    {
        if (roofPiece == null)
            yield break;

        Transform attachAnchor = GetGrabAnchor();
        if (attachAnchor == null)
            yield break;

        Rigidbody rb = roofPiece.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        roofPiece.SetParent(attachAnchor, true);

        if (roofAttachDuration <= 0f)
        {
            roofPiece.position = attachAnchor.position;
            if (alignRoofRotationToGrabAnchor)
                roofPiece.rotation = attachAnchor.rotation;
            yield break;
        }

        Vector3 startPosition = roofPiece.position;
        Quaternion startRotation = roofPiece.rotation;
        float timer = 0f;

        while (timer < roofAttachDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / roofAttachDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            roofPiece.position = Vector3.Lerp(startPosition, attachAnchor.position, easedT);

            if (alignRoofRotationToGrabAnchor)
                roofPiece.rotation = Quaternion.Slerp(startRotation, attachAnchor.rotation, easedT);

            yield return null;
        }

        roofPiece.position = attachAnchor.position;
        if (alignRoofRotationToGrabAnchor)
            roofPiece.rotation = attachAnchor.rotation;
    }

    private void ReleaseRoof()
    {
        if (roofPiece == null)
            return;

        roofPiece.SetParent(null, true);

        Rigidbody rb = roofPiece.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.WakeUp();
        }
    }

    private void AttachPlayerToHand()
    {
        if (xrOrigin == null)
            return;

        Transform attachAnchor = GetGrabAnchor();
        if (attachAnchor == null)
            return;

        playerGrabOffset = xrOrigin.position - attachAnchor.position;

        playerFollowingGrabAnchor = true;
        ApplyPlayerFollowPosition();
    }

    private void DetachPlayerFromHand()
    {
        playerFollowingGrabAnchor = false;
    }

    private void ApplyPlayerFollowPosition()
    {
        if (xrOrigin == null || grabAnchor == null)
            return;

        // Follow position only; parenting or rotating the XR Origin can make VR movement uncomfortable.
        if (playerHead != null)
        {
            xrOrigin.position += grabAnchor.position - playerHead.position;
            return;
        }

        xrOrigin.position = grabAnchor.position + playerGrabOffset;
    }

    private Transform GetGrabAnchor()
    {
        if (grabAnchor != null)
            return grabAnchor;

        if (handTipReference == null)
            return null;

        GameObject anchorObject = new GameObject("HandGrabAnchor");
        grabAnchor = anchorObject.transform;
        grabAnchor.SetParent(handTipReference, false);
        grabAnchor.localPosition = Vector3.zero;
        grabAnchor.localRotation = Quaternion.identity;

        return grabAnchor;
    }

    private Transform GetGrabReference()
    {
        Transform anchor = GetGrabAnchor();
        if (anchor != null)
            return anchor;

        return GetHandArrivalReference();
    }

    private void SyncHandIKTargetToVisibleHand()
    {
        if (handIKTarget == null || handTipReference == null)
            return;

        // Prevent the arm from snapping when control returns from animation to IK.
        handIKTarget.position = handTipReference.position;
    }
}
