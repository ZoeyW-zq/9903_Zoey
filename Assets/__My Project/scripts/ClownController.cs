using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ClownController : MonoBehaviour
{
    [Header("流程引用")]
    // 负责切换游戏阶段。小丑危机结束后会通过它进入吞咽转场阶段。
    [SerializeField] private GameStateController gameStateController;
    // 小助手对白控制器。进入噩梦阶段时用于播放提醒对白。
    [SerializeField] private AssistantController assistantController;

    [Header("巨人动画")]
    // 巨人模型身上的 Animator。脚本只触发动画参数，具体动作由 Animator Controller 决定。
    [SerializeField] private Animator giantAnimator;
    // 抓取流程开始时触发的动画 Trigger 名称，需要和 Animator Controller 里的参数一致。
    [SerializeField] private string bendPickTriggerName = "BendPick";
    // 触发动画后等待多久再开始接管手部反向运动学，给原始动画预留起手时间。
    [SerializeField] private float animationLeadTime = 0.5f;

    [Header("手臂反向运动学")]
    // 右手臂所在的 Rig。这里调的是整条右手臂 Rig 的权重，不是 TwoBoneIKConstraint 自身的权重。
    [SerializeField] private Rig rightArmRig;
    // 右手反向运动学目标点。移动这个 Transform 会驱动巨人的手朝目标位置移动。
    [SerializeField] private Transform handIKTarget;
    [Tooltip("可选的真实手掌或手腕骨骼引用，用来确认可见手部是否真的到达嘴部。")]
    // 可见手部参考点。等待到嘴边时优先检查这个点，而不是只检查 handIKTarget。
    [SerializeField] private Transform handTipReference;
    [Tooltip("屋顶和玩家共用的抓取挂点。如果为空，运行时会在 handTipReference 下创建一个。")]
    // 真正用于挂载屋顶、跟随玩家、判断抓取到达的统一抓取点。
    [SerializeField] private Transform grabAnchor;

    [Header("路径点")]
    // 屋顶初始抓取点。手会先移动到这里，再让屋顶贴近 grabAnchor。
    [SerializeField] private Transform roofPoint;
    // 丢弃屋顶的位置。屋顶挂到手上后，手会带着屋顶移动到这里。
    [SerializeField] private Transform dropRoofPoint;
    // 嘴部目标点。通常应挂在头部或嘴部骨骼下，以便跟随巨人动画移动。
    [SerializeField] private Transform mouthPoint;

    [Header("场景物体")]
    // 被巨人掀开并丢掉的屋顶物体，需要配合 Rigidbody 才能在释放后掉落。
    [SerializeField] private Transform roofPiece;
    // 玩家 XR Origin。脚本只移动位置，不继承手部旋转，以避免强行旋转玩家视角。
    [SerializeField] private Transform xrOrigin;
    // 玩家头部位置，用来作为巨人伸手抓取玩家时的目标。
    [SerializeField] private Transform playerHead;

    [Header("音频")]
    // 巨人靠近或行动时的脚步声。
    [SerializeField] private AudioSource footstepsAudio;
    // 环境震动或低频压迫感音效。
    [SerializeField] private AudioSource rumbleAudio;

    [Header("时间参数")]
    // 开始接管手臂时，将 Rig 权重混合到 1 所需的时间。
    [SerializeField] private float rigBlendDuration = 0.5f;
    // 手从当前位置移动到屋顶抓取点所需的时间。
    [SerializeField] private float moveToRoofDuration = 0.5f;
    // 屋顶贴近 grabAnchor 的过渡时间。值越大，屋顶越不会突然吸到手上。
    [SerializeField] private float roofAttachDuration = 0.45f;
    // 是否让屋顶旋转也对齐 grabAnchor。默认关闭，避免屋顶因为手掌朝向产生突兀翻转。
    [SerializeField] private bool alignRoofRotationToGrabAnchor;
    // 手带着屋顶移动到丢弃点所需的时间。
    [SerializeField] private float pullRoofDuration = 0.5f;
    // 手从抓住玩家后的恢复位置移动到嘴部所需的时间。
    [SerializeField] private float moveToMouthDuration = 4f;

    [Header("抓住玩家后的动画衔接")]
    // 抓住玩家后，将右手 Rig 权重从当前值混合到 0 的时间，让原始动画重新接管手臂。
    [SerializeField] private float rigBlendOutDuration;
    [Tooltip("玩家挂到手部后，让小丑原始动画继续运行的持续时间。")]
    // Rig 权重为 0 后停留多久，再把手部控制权交还给反向运动学去移动到嘴边。
    [SerializeField] private float postGrabAnimationHoldDuration;
    // 原始动画阶段结束后，将右手 Rig 权重重新混合到 1 的时间。
    [SerializeField] private float rigBlendInDuration;

    [Header("距离与超时")]
    // grabAnchor 与玩家头部小于这个距离时，才认为巨人真的抓到了玩家。
    [SerializeField] private float grabDistance = 1f;
    // 伸手抓玩家阶段，handIKTarget 追向玩家头部的速度。
    [SerializeField] private float reachToPlayerSpeed = 2.5f;
    [Tooltip("手部参考点距离嘴部多近时，才允许进入吞咽转场。")]
    // 手部参考点与嘴部小于这个距离时，才认为手已经到达嘴边。
    [SerializeField] private float mouthArrivalDistance = 0.1f;
    [Tooltip("嘴部到达检测的安全超时，避免反向运动学设置异常时流程永久卡住。")]
    // 如果手部始终到不了嘴边，超过这个时间后会继续流程并打印警告。
    [SerializeField] private float mouthArrivalTimeout = 5f;
    [Tooltip("等待到达嘴部时，反向运动学目标继续追踪动画嘴部的速度。")]
    // 嘴部可能跟随头部动画移动，所以等待到达期间需要持续追踪，而不是只采样一次位置。
    [SerializeField] private float mouthTrackingCatchupSpeed = 6f;

    // 当前正在运行的危机流程协程。重新开始流程时会先停止旧协程，避免两个流程同时控制手臂。
    private Coroutine crisisRoutine;
    // 防止同一个触发器多次启动 GiantCrisis。
    private bool crisisTriggered;
    // 玩家是否正在跟随 grabAnchor。这里不用 SetParent，是为了避免玩家视角继承手部旋转。
    private bool playerFollowingGrabAnchor;
    // 玩家被抓住瞬间与 grabAnchor 的位置偏移。后续跟随时保持这个偏移，避免玩家被吸到手掌中心。
    private Vector3 playerGrabOffset;

    private void LateUpdate()
    {
        if (!playerFollowingGrabAnchor || xrOrigin == null || grabAnchor == null)
            return;

        // 玩家被抓住后，每帧跟随 grabAnchor 的位置。
        // 这里只改位置，不改旋转；在 VR 中强制旋转玩家视角会破坏玩家自主控制，也容易造成眩晕。
        xrOrigin.position = grabAnchor.position + playerGrabOffset;
    }

    public void TriggerCrisis()
    {
        if (crisisTriggered)
            return;

        crisisTriggered = true;

        // 外部触发器只负责切换游戏状态，真正的巨人动作流程由 GameStateController 进入 GiantCrisis 后启动。
        gameStateController.SetState(GameStateController.GameState.GiantCrisis);
    }

    public void StartCrisisSequence()
    {
        // 如果之前已经启动过流程，先停掉旧协程，避免两个协程同时写 handIKTarget 或 Rig 权重。
        if (crisisRoutine != null)
            StopCoroutine(crisisRoutine);

        crisisRoutine = StartCoroutine(CrisisRoutine());
    }

    private IEnumerator CrisisRoutine()
    {
        // 1. 小助手提示是独立系统，先触发后不等待它结束。
        // 这样脚步声、屋顶震动声和后续巨人动作都不会被小助手对白长度影响。
        if (assistantController != null)
            assistantController.PlayNightmareWarning();

        // 2. 先播放脚步声，并等待脚步声真正结束。
        // 如果 AudioSource 被设置成 Loop，这里会一直等待，直到外部停止脚步声。
        if (footstepsAudio != null)
        {
            footstepsAudio.Play();
            yield return WaitForAudioToFinish(footstepsAudio);
        }

        // 3. 脚步声结束后再播放屋顶震动或环境压迫音效，避免脚步声和震动声同时响起。
        if (rumbleAudio != null)
        {
            rumbleAudio.Play();
            yield return new WaitForSeconds(1f);
        }

        // 4. 震动声开始后触发巨人动画。声音可继续播放，不阻塞后续动作。
        if (giantAnimator != null)
            giantAnimator.SetTrigger(bendPickTriggerName);

        // 5. 给巨人原始动画一点起手时间，避免 IK 立即抢走动画姿势。
        yield return new WaitForSeconds(animationLeadTime);

        // 6. 提高右手 Rig 权重，让手部开始受 handIKTarget 控制。
        yield return BlendRigWeight(1f, rigBlendDuration);

        // 7. 手先移动到屋顶抓取点。
        yield return MoveHandTo(roofPoint.position, moveToRoofDuration);

        // 8. 屋顶平滑贴近 grabAnchor，并挂到 grabAnchor 下。
        yield return AttachRoofToHand();

        // 9. 手带着屋顶移动到 dropRoofPoint。
        yield return MoveHandTo(dropRoofPoint.position, pullRoofDuration);

        // 10. 到达丢弃点后释放屋顶，开启重力，让屋顶自然掉落。
        ReleaseRoof();

        // 11. 手伸向玩家。这里必须等真实抓取参考点到达玩家附近，而不是只看 handIKTarget。
        yield return ReachTowardPlayer();

        // 12. 玩家开始跟随 grabAnchor；随后暂时关闭 Rig，让小丑原始动画继续演一段。
        AttachPlayerToHand();

        yield return BlendRigWeight(0f, rigBlendOutDuration);

        // 13. Rig 权重为 0 的停留阶段。这个时间用于播放小丑抓住玩家后的原始动画。
        if (postGrabAnimationHoldDuration > 0f)
            yield return new WaitForSeconds(postGrabAnimationHoldDuration);

        // 14. 重新开启 IK 前，先把 handIKTarget 放到当前可见手部位置，避免权重恢复时手臂瞬移。
        SyncHandIKTargetToVisibleHand();

        yield return BlendRigWeight(1f, rigBlendInDuration);

        // 15. 嘴部可能跟随头部动画移动，所以移动到嘴边时持续采样 mouthPoint，而不是只取一次坐标。
        yield return MoveHandToTarget(mouthPoint, moveToMouthDuration);

        // 16. 等可见手部真的靠近嘴边后，再进入吞咽转场；超时用于防止错误配置卡死流程。
        yield return WaitForHandNear(mouthPoint, mouthArrivalDistance, mouthArrivalTimeout);

        // 17. 玩家脱离手部跟随，游戏状态进入 SwallowTransition，由 SwallowController 接管黑屏与传送。
        DetachPlayerFromHand();

        gameStateController.SetState(GameStateController.GameState.SwallowTransition);
    }

    private IEnumerator WaitForAudioToFinish(AudioSource audioSource)
    {
        // 只等待指定 AudioSource 自己的播放状态，不影响小助手对白等其它音频系统。
        if (audioSource == null)
            yield break;

        while (audioSource.isPlaying)
            yield return null;
    }

    private IEnumerator MoveHandTo(Vector3 targetPosition, float duration)
    {
        // 固定目标点移动：适合 roofPoint、dropRoofPoint 这类不会在动画中移动的位置。
        if (handIKTarget == null)
            yield break;

        if (duration <= 0f)
        {
            // 时长为 0 时直接设置位置，用于调试或需要瞬移目标点的情况。
            handIKTarget.position = targetPosition;
            yield break;
        }

        Vector3 startPosition = handIKTarget.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            // SmoothStep 让移动在开始和结束时更柔和，减少机械感。
            handIKTarget.position = Vector3.Lerp(startPosition, targetPosition, easedT);

            yield return null;
        }

        // 最后一帧强制落到目标点，消除浮点误差造成的微小偏差。
        handIKTarget.position = targetPosition;
    }

    private IEnumerator MoveHandToTarget(Transform target, float duration)
    {
        // 动态目标移动：适合 mouthPoint 这类可能挂在动画骨骼下、每帧位置都可能变化的目标。
        if (handIKTarget == null || target == null)
            yield break;

        if (duration <= 0f)
        {
            handIKTarget.position = target.position;
            yield break;
        }

        Vector3 startPosition = handIKTarget.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            // 每帧重新读取 target.position，确保嘴部跟随动画移动时，手也持续追踪新的嘴部位置。
            handIKTarget.position = Vector3.Lerp(startPosition, target.position, easedT);

            yield return null;
        }

        // 结束时再采样一次 target.position，保证目标点落在嘴部当前最新位置。
        handIKTarget.position = target.position;
    }

    private IEnumerator WaitForHandNear(Transform target, float distance, float timeout)
    {
        // 到达判定不能只看 handIKTarget，因为 IK 目标到了不代表可见手部已经完全解算到位。
        if (handIKTarget == null || target == null)
            yield break;

        // 优先使用真实手部骨骼作为到达参考；没有时再退回到 grabAnchor 或 handIKTarget。
        Transform arrivalReference = GetHandArrivalReference();
        float timer = 0f;

        while (Vector3.Distance(arrivalReference.position, target.position) > distance)
        {
            if (timeout > 0f && timer >= timeout)
            {
                Debug.LogWarning(
                    $"{nameof(ClownController)}：手部在吞咽超时前没有到达 {target.name}。请检查反向运动学链条、提示点、目标点和到达距离。",
                    this
                );
                yield break;
            }

            timer += Time.deltaTime;

            // 等待期间继续让 handIKTarget 追嘴部，避免嘴部动画移动后手停在旧位置。
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
        // 手部到嘴边的判定优先看真实手部骨骼，因为它最接近玩家实际看到的手。
        if (handTipReference != null)
            return handTipReference;

        // 如果没有真实手部骨骼，则退回到抓取挂点。
        if (grabAnchor != null)
            return grabAnchor;

        // 最后才使用 handIKTarget。它只是控制目标，不一定等于可见手部位置。
        return handIKTarget;
    }

    private IEnumerator ReachTowardPlayer()
    {
        // 伸手抓玩家阶段：handIKTarget 负责驱动手，grabReference 负责判断可见抓取点是否真的到达玩家。
        bool missingReferenceLogged = false;
        while (handIKTarget == null || playerHead == null)
        {
            if (!missingReferenceLogged)
            {
                Debug.LogError($"{nameof(ClownController)}：缺少 handIKTarget 或 playerHead，巨人无法抓到玩家，流程会停在抓取阶段。", this);
                missingReferenceLogged = true;
            }

            yield return null;
        }

        Transform grabReference = GetGrabReference();

        while (Vector3.Distance(grabReference.position, playerHead.position) > grabDistance)
        {
            // 继续把 IK 目标推向玩家头部，但是否真正抓到玩家由 grabReference 与玩家的距离决定。
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
        // 混合右手 Rig 权重。targetWeight 为 1 时 IK 完全接管，targetWeight 为 0 时原始动画完全接管。
        if (rightArmRig == null)
            yield break;

        float startWeight = rightArmRig.weight;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            // 权重线性插值即可；位置移动已经在其它协程里做了缓动。
            rightArmRig.weight = Mathf.Lerp(startWeight, targetWeight, t);

            yield return null;
        }

        // 最后一帧强制设置目标权重，确保 Inspector 中看到的是准确的 0 或 1。
        rightArmRig.weight = targetWeight;
    }

    private IEnumerator AttachRoofToHand()
    {
        // 屋顶附着分两步：先关闭物理影响，再把 Transform 挂到 grabAnchor，并用插值完成位置贴近。
        if (roofPiece == null)
            yield break;

        Transform attachAnchor = GetGrabAnchor();
        if (attachAnchor == null)
            yield break;

        Rigidbody rb = roofPiece.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 抓住屋顶时关闭重力并设为运动学，避免物理系统和脚本 Transform 控制互相拉扯。
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // 保持世界坐标挂载，避免父节点切换瞬间改变屋顶当前外观位置。
        roofPiece.SetParent(attachAnchor, true);

        if (roofAttachDuration <= 0f)
        {
            // 如果不需要过渡，直接把屋顶放到 grabAnchor。
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

            // 每帧重新采样 attachAnchor.position，让屋顶贴近移动中的手，而不是飞向旧位置。
            roofPiece.position = Vector3.Lerp(startPosition, attachAnchor.position, easedT);

            // 默认不旋转屋顶；需要屋顶跟手掌朝向一致时再开启这个选项。
            if (alignRoofRotationToGrabAnchor)
                roofPiece.rotation = Quaternion.Slerp(startRotation, attachAnchor.rotation, easedT);

            yield return null;
        }

        // 过渡结束后精确对齐，避免最后残留一点距离。
        roofPiece.position = attachAnchor.position;
        if (alignRoofRotationToGrabAnchor)
            roofPiece.rotation = attachAnchor.rotation;
    }

    private void ReleaseRoof()
    {
        // 丢掉屋顶时解除父子关系，并把物理控制权交还给 Rigidbody。
        if (roofPiece == null)
            return;

        // true 表示保持当前世界坐标，避免释放瞬间位置跳变。
        roofPiece.SetParent(null, true);

        Rigidbody rb = roofPiece.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 释放后打开物理模拟和重力，让屋顶自然掉落。
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.WakeUp();
        }
    }

    private void AttachPlayerToHand()
    {
        // 玩家不直接 SetParent 到手上，因为父节点旋转会强行旋转 XR Origin，破坏 VR 视角控制。
        if (xrOrigin == null)
            return;

        Transform attachAnchor = GetGrabAnchor();
        if (attachAnchor == null)
            return;

        // 记录玩家被抓瞬间相对 grabAnchor 的偏移，后续只跟随位置并保持这个偏移。
        playerGrabOffset = xrOrigin.position - attachAnchor.position;
        playerFollowingGrabAnchor = true;
    }

    private void DetachPlayerFromHand()
    {
        // 停止 LateUpdate 中的位置跟随。玩家随后会由 SwallowController 或其它流程移动。
        playerFollowingGrabAnchor = false;
    }

    private Transform GetGrabAnchor()
    {
        // 如果场景里已经手动指定 grabAnchor，就直接使用它。
        if (grabAnchor != null)
            return grabAnchor;

        // 如果没有指定 grabAnchor，但有真实手部参考点，就运行时创建一个默认挂点。
        if (handTipReference == null)
            return null;

        GameObject anchorObject = new GameObject("HandGrabAnchor");
        grabAnchor = anchorObject.transform;
        // 默认挂点放在 handTipReference 的局部原点，之后可在场景中手动创建更精确的 grabAnchor。
        grabAnchor.SetParent(handTipReference, false);
        grabAnchor.localPosition = Vector3.zero;
        grabAnchor.localRotation = Quaternion.identity;

        return grabAnchor;
    }

    private Transform GetGrabReference()
    {
        // 抓玩家阶段优先使用 grabAnchor，因为玩家和屋顶都跟随这个点。
        Transform anchor = GetGrabAnchor();
        if (anchor != null)
            return anchor;

        // 极端情况下没有 grabAnchor，就退回到手部到达参考点，保证流程仍然有参照物。
        return GetHandArrivalReference();
    }

    private void SyncHandIKTargetToVisibleHand()
    {
        if (handIKTarget == null || handTipReference == null)
            return;

        // 在重新提高 Rig 权重之前，先把 IK 目标放到当前可见手部位置。
        // 这样从原始动画切回 IK 时，手臂不会因为目标点相距太远而突然弹跳。
        handIKTarget.position = handTipReference.position;
    }
}
