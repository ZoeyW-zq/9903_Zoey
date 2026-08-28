using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GameStateController : MonoBehaviour
{
    public enum GameState
    {
        None,
        OfficeDialogue,
        AwaitCrystalBall,
        TransitionToHippocampus,
        Hippocampus,
        FinalMemoryPlacement,
        GiantCrisis,
        SwallowTransition,
        MirrorChamber,
        BackToOffice,
        SessionComplete
    }

    [Header("References")]
    [SerializeField] private AssistantController assistantController;
    [SerializeField] private OfficeDialogueController officeDialogueController;
    [SerializeField] private MemoryPlacementController memoryPlacementController;
    [SerializeField] private FinalReportController finalReportController;
    [SerializeField] private MonoBehaviour crystalBall;
    [SerializeField] private ScreenFadeController screenFadeController;
    [SerializeField] private Transform player;
    [SerializeField] private Transform assistantRobot;
    [SerializeField] private Transform assistantHippocampusSpawnPoint;
    [SerializeField] private Transform hippocampusSpawnPoint;
    [SerializeField] private Transform assistantMirrorChamberSpawnPoint;
    [SerializeField] private Transform backToOfficePlayerSpawnPoint;
    [SerializeField] private Transform backToOfficeAssistantSpawnPoint;

    [Header("Resolved Memories")]
    [SerializeField] private Transform[] resolvedMemoryObjects = new Transform[4];
    [SerializeField] private Transform[] memoryRoomReturnPoints = new Transform[4];
    [SerializeField] private Transform memoryRoomReturnParent;
    [SerializeField, Min(0f)] private float memoryRoomReturnFadeOutDuration = 1f;
    [SerializeField, Min(0f)] private float memoryRoomReturnFadeInDuration = 1f;
    [SerializeField, Min(0f)] private float resolvedMemoriesNightmareFadeDuration = 4f;
    [SerializeField] private ClownController clownController;
    [SerializeField] private SwallowController swallowController;
    [SerializeField] private PlayerMovementLockController playerMovementLockController;

    [Header("Scene Roots")]
    [SerializeField] private GameObject officeRoot;
    [SerializeField] private GameObject hippoRoot;
    [SerializeField] private GameObject dungeonRoot;

    [Header("Global Volumes")]
    [SerializeField] private Volume globalVolumeOffice;
    [SerializeField] private Volume globalVolumeHippo;
    [SerializeField] private Volume globalVolumeNightmare;
    [SerializeField, Min(0f)] private float globalVolumeFadeDuration = 1f;

    [Header("Crisis Particles")]
    [SerializeField] private ParticleSystem hippocampusParticleSystem;
    [SerializeField, Min(0f)] private float hippocampusParticleFadeDuration = 1f;

    [Header("Crisis Audio")]
    [SerializeField] private AudioSource heartBeatAudio;

    [Header("Transition Colors")]
    [SerializeField] private Color hippocampusFadeColor = Color.white;
    [SerializeField] private Color backToOfficeFadeColor = Color.black;

    [Header("Memory Room Return Environment")]
    [SerializeField] private Material memoryRoomReturnSkybox;

    [Header("Back To Office Transition")]
    [SerializeField] private float backToOfficeFadeOutDuration = 1f;
    [SerializeField] private float backToOfficeFadeInDuration = 1f;

    [Header("Close Session")]
    [SerializeField, Min(0f)] private float closeSessionFadeDuration = 1f;

    public GameState State => state;
    public MemoryPlacementController MemoryPlacementController => memoryPlacementController;
    public event System.Action<GameState, GameState> StateChanged;
    public event System.Action AllMemoriesResolved;
    public event System.Action FinalPlacementConfirmed;
    public event System.Action ReturnedToOffice;
    public event System.Action SessionCloseRequested;
    public event System.Action<int> MirrorMemoryResolved;

    private GameState state = GameState.None;
    private Coroutine nightmareVolumeRoutine;
    private Coroutine giantCrisisRoutine;
    private Coroutine hippocampusParticleRoutine;
    private Material officeSkybox;
    private float hippocampusParticleNormalEmissionRate;
    private bool firstMemoryReleasedResponsePlayed;
    private int mirrorMemoriesResolved;
    private bool memoryRoomReturnStarted;
    private bool usesExternalAllMemoriesDialogue;
    private bool finalConfirmationTransitionStarted;
    private bool closeSessionRequested;
    private Coroutine closeSessionRoutine;

    private void Awake()
    {
        officeSkybox = RenderSettings.skybox;

        if (memoryPlacementController == null && hippoRoot != null)
            memoryPlacementController = hippoRoot.GetComponentInChildren<MemoryPlacementController>(true);

        if (finalReportController == null && officeRoot != null)
            finalReportController = officeRoot.GetComponentInChildren<FinalReportController>(true);

        CacheHippocampusParticleEmissionRate();
        InitializeGlobalVolumeWeights();
    }

    private void Start()
    {
        SetState(GameState.OfficeDialogue);
    }

    public void SetState(GameState newState)
    {
        if (state == newState)
            return;

        GameState previousState = state;
        ExitState(state);

        state = newState;
        Debug.Log("Game State changed to: " + state);

        EnterState(state);
        StateChanged?.Invoke(previousState, state);
    }

    public void HandleAllMemoriesResolved()
    {
        if (state != GameState.MirrorChamber || memoryRoomReturnStarted)
            return;

        memoryRoomReturnStarted = true;
        StartResolvedMemoriesNightmareFadeOut();

        usesExternalAllMemoriesDialogue = AllMemoriesResolved != null;
        AllMemoriesResolved?.Invoke();

        if (!usesExternalAllMemoriesDialogue)
            ContinueAfterAllMemoriesResolved();
    }

    public void HandleFirstMemoryReleased()
    {
        if (state != GameState.MirrorChamber)
            return;

        mirrorMemoriesResolved = Mathf.Min(4, mirrorMemoriesResolved + 1);
        MirrorMemoryResolved?.Invoke(mirrorMemoriesResolved);

        if (firstMemoryReleasedResponsePlayed)
            return;

        firstMemoryReleasedResponsePlayed = true;

        if (assistantController != null)
            assistantController.PlayMemoryReleased();
    }

    public void HandleFinalPlacementConfirmed()
    {
        if (state != GameState.FinalMemoryPlacement || finalConfirmationTransitionStarted)
            return;

        finalConfirmationTransitionStarted = true;

        bool externalDialogueHandler = FinalPlacementConfirmed != null;
        FinalPlacementConfirmed?.Invoke();

        if (externalDialogueHandler)
            return;

        if (assistantController != null)
            assistantController.PlayConfirmationResponse(ReturnToOfficeAfterConfirmation);
        else
            ReturnToOfficeAfterConfirmation();
    }

    public void ContinueAfterAllMemoriesResolved()
    {
        if (state == GameState.MirrorChamber && memoryRoomReturnStarted)
            StartCoroutine(ReturnToMemoryRoomRoutine());
    }

    public void ContinueAfterFinalPlacementConfirmed()
    {
        if (state == GameState.FinalMemoryPlacement && finalConfirmationTransitionStarted)
            ReturnToOfficeAfterConfirmation();
    }

    public void HandleCloseSessionRequested()
    {
        if (state != GameState.BackToOffice || closeSessionRequested)
            return;

        closeSessionRequested = true;

        bool externalDialogueHandler = SessionCloseRequested != null;
        SessionCloseRequested?.Invoke();

        if (externalDialogueHandler)
            return;

        if (assistantController != null)
            assistantController.PlaySessionClosing(EnterSessionComplete);
        else
            EnterSessionComplete();
    }

    public void ContinueAfterSessionClosing()
    {
        if (state == GameState.BackToOffice && closeSessionRequested)
            EnterSessionComplete();
    }

    // Kept only so the imported breakable-glass package remains source-compatible.
    // The WebGL scene no longer uses a manual glass-shatter transition.
    public void HandleFinalChamberGlassShattered()
    {
    }

    public void ReleaseClownPlayerControl()
    {
        if (clownController != null)
            clownController.ReleasePlayerControl();
    }

    public void SetPlayerMovementLocked(bool locked)
    {
        if (playerMovementLockController != null)
            playerMovementLockController.SetMovementLocked(locked);
    }

    private void EnterState(GameState newState)
    {
        switch (newState)
        {
            case GameState.OfficeDialogue:
                SetActiveSceneRoot(SceneRoot.Office);
                SwitchToOfficeVolume();
                SetCrystalBallEnabled(false);

                if (officeDialogueController != null)
                    officeDialogueController.BeginOfficeDialogue();
                else
                    Debug.LogWarning("GameStateController: OfficeDialogueController is not assigned.", this);
                break;

            case GameState.AwaitCrystalBall:
                SetActiveSceneRoot(SceneRoot.Office);
                SetCrystalBallEnabled(true);
                break;

            case GameState.TransitionToHippocampus:
                StartCoroutine(TransitionToHippocampusRoutine());
                break;

            case GameState.Hippocampus:
                SetActiveSceneRoot(SceneRoot.Hippo);
                SwitchToHippocampusVolume();
                RestoreHippocampusParticles();
                SetCrystalBallEnabled(false);
                if (assistantController != null)
                    assistantController.PlayHippocampusIntro();
                break;

            case GameState.FinalMemoryPlacement:
                SetActiveSceneRoot(SceneRoot.Hippo);

                if (memoryPlacementController != null)
                    memoryPlacementController.BeginFinalPlacement(this);
                else
                    Debug.LogWarning("GameStateController: MemoryPlacementController is not assigned.", this);
                break;

            case GameState.GiantCrisis:
                giantCrisisRoutine = StartCoroutine(EnterGiantCrisisRoutine());
                break;

            case GameState.SwallowTransition:
                swallowController.StartSwallowTransition();
                break;

            case GameState.MirrorChamber:
                if (memoryPlacementController != null)
                    memoryPlacementController.SaveInitialItemTransformsForRoomExit();

                SetActiveSceneRoot(SceneRoot.Dungeon);
                MoveToSpawn(assistantRobot, assistantMirrorChamberSpawnPoint);
                if (assistantController != null)
                    assistantController.PlayMirrorChamberIntro();
                break;

            case GameState.BackToOffice:
                StartCoroutine(ReturnToOfficeRoutine());
                break;

            case GameState.SessionComplete:
                closeSessionRoutine = StartCoroutine(CloseSessionRoutine());
                break;
        }
    }

    private void ExitState(GameState oldState)
    {
        switch (oldState)
        {
            case GameState.AwaitCrystalBall:
                SetCrystalBallEnabled(false);
                break;

            case GameState.GiantCrisis:
                StopRoutine(ref giantCrisisRoutine);
                break;

            case GameState.SessionComplete:
                StopRoutine(ref closeSessionRoutine);
                break;
        }
    }

    private void SetCrystalBallEnabled(bool value)
    {
        if (crystalBall == null)
            return;

        if (crystalBall is ICrystalBallEntry crystalBallEntry)
        {
            crystalBallEntry.SetEnabled(value);
            return;
        }

        Debug.LogError("GameStateController: assigned Crystal Ball component must implement ICrystalBallEntry.", crystalBall);
    }

    private enum SceneRoot
    {
        None,
        Office,
        Hippo,
        Dungeon
    }

    private void SetActiveSceneRoot(SceneRoot activeRoot)
    {
        // Each stage uses a single scene root so inactive areas stop rendering and interacting.
        ApplyEnvironmentLighting(activeRoot);
        SetRootActive(officeRoot, activeRoot == SceneRoot.Office);
        SetRootActive(hippoRoot, activeRoot == SceneRoot.Hippo);
        SetRootActive(dungeonRoot, activeRoot == SceneRoot.Dungeon);
    }

    private void ApplyEnvironmentLighting(SceneRoot activeRoot)
    {
        bool useOfficeLighting = activeRoot == SceneRoot.Office;
        AmbientMode targetMode = useOfficeLighting ? AmbientMode.Skybox : AmbientMode.Trilight;
        bool environmentChanged = false;

        if (useOfficeLighting && RenderSettings.skybox != officeSkybox)
        {
            RenderSettings.skybox = officeSkybox;
            environmentChanged = true;
        }

        if (RenderSettings.ambientMode != targetMode)
        {
            RenderSettings.ambientMode = targetMode;
            environmentChanged = true;
        }

        if (environmentChanged)
            DynamicGI.UpdateEnvironment();
    }

    private static void SetRootActive(GameObject root, bool active)
    {
        if (root != null && root.activeSelf != active)
            root.SetActive(active);
    }

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }

    private void CacheHippocampusParticleEmissionRate()
    {
        if (hippocampusParticleSystem == null)
            return;

        hippocampusParticleNormalEmissionRate = GetParticleEmissionRate(hippocampusParticleSystem);
    }

    private void RestoreHippocampusParticles()
    {
        if (hippocampusParticleSystem == null)
            return;

        StopRoutine(ref hippocampusParticleRoutine);
        SetParticleEmissionRate(hippocampusParticleSystem, hippocampusParticleNormalEmissionRate);

        if (!hippocampusParticleSystem.isPlaying)
            hippocampusParticleSystem.Play(true);
    }

    private void StartHippocampusParticleFadeOut()
    {
        if (hippocampusParticleSystem == null)
            return;

        StopRoutine(ref hippocampusParticleRoutine);
        hippocampusParticleRoutine = StartCoroutine(FadeParticleEmissionRate(
            hippocampusParticleSystem,
            0f,
            hippocampusParticleFadeDuration
        ));
    }

    private IEnumerator FadeParticleEmissionRate(ParticleSystem particleSystem, float targetRate, float duration)
    {
        if (particleSystem == null)
            yield break;

        float startRate = GetParticleEmissionRate(particleSystem);

        if (duration <= 0f || Mathf.Approximately(startRate, targetRate))
        {
            SetParticleEmissionRate(particleSystem, targetRate);
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            hippocampusParticleRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            SetParticleEmissionRate(particleSystem, Mathf.Lerp(startRate, targetRate, progress));
            yield return null;
        }

        SetParticleEmissionRate(particleSystem, targetRate);
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        hippocampusParticleRoutine = null;
    }

    private static float GetParticleEmissionRate(ParticleSystem particleSystem)
    {
        return particleSystem.emission.rateOverTime.constantMax;
    }

    private static void SetParticleEmissionRate(ParticleSystem particleSystem, float rate)
    {
        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Max(0f, rate));
    }

    private void InitializeGlobalVolumeWeights()
    {
        SetVolumeImmediate(globalVolumeOffice, 1f, true);
        SetVolumeImmediate(globalVolumeHippo, 0f, true);
        SetVolumeImmediate(globalVolumeNightmare, 0f, true);
    }

    private void SwitchToHippocampusVolume()
    {
        StopRoutine(ref nightmareVolumeRoutine);

        SetVolumeImmediate(globalVolumeOffice, 0f, false);
        SetVolumeImmediate(globalVolumeHippo, 1f, true);
        SetVolumeImmediate(globalVolumeNightmare, 0f, true);
    }

    private void SwitchToOfficeVolume()
    {
        StopRoutine(ref nightmareVolumeRoutine);

        SetVolumeImmediate(globalVolumeOffice, 1f, true);
        SetVolumeImmediate(globalVolumeHippo, 0f, false);
        SetVolumeImmediate(globalVolumeNightmare, 0f, false);
    }

    private void SetVolumeImmediate(Volume volume, float weight, bool active)
    {
        if (volume == null)
            return;

        if (volume.gameObject.activeSelf != active)
            volume.gameObject.SetActive(active);

        volume.weight = Mathf.Clamp01(weight);
    }

    private IEnumerator EnterGiantCrisisRoutine()
    {
        SetActiveSceneRoot(SceneRoot.Hippo);
        StartHippocampusParticleFadeOut();

        StopRoutine(ref nightmareVolumeRoutine);

        yield return FadeVolumeWeight(globalVolumeHippo, 0f, false);

        if (state != GameState.GiantCrisis)
            yield break;

        yield return FadeVolumeWeight(globalVolumeNightmare, 1f, true);

        if (state != GameState.GiantCrisis)
            yield break;

        giantCrisisRoutine = null;

        if (heartBeatAudio != null)
            heartBeatAudio.Play();

        if (clownController != null)
            clownController.StartCrisisSequence();
    }

    private void StartResolvedMemoriesNightmareFadeOut()
    {
        StopRoutine(ref nightmareVolumeRoutine);
        nightmareVolumeRoutine = StartCoroutine(FadeResolvedMemoriesNightmareVolume());
    }

    private IEnumerator FadeResolvedMemoriesNightmareVolume()
    {
        yield return FadeVolumeWeight(
            globalVolumeNightmare,
            0f,
            false,
            resolvedMemoriesNightmareFadeDuration
        );
        nightmareVolumeRoutine = null;
    }

    private IEnumerator FadeVolumeWeight(
        Volume volume,
        float targetWeight,
        bool activateBeforeFade,
        float duration = -1f)
    {
        if (volume == null)
            yield break;

        targetWeight = Mathf.Clamp01(targetWeight);
        if (duration < 0f)
            duration = globalVolumeFadeDuration;

        if (activateBeforeFade && !volume.gameObject.activeSelf)
        {
            if (targetWeight > volume.weight)
                volume.weight = 0f;

            volume.gameObject.SetActive(true);
        }

        float startWeight = volume.weight;

        if (duration <= 0f || Mathf.Approximately(startWeight, targetWeight))
        {
            volume.weight = targetWeight;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            volume.weight = Mathf.Lerp(startWeight, targetWeight, progress);
            yield return null;
        }

        volume.weight = targetWeight;
    }

    private void ApplyMemoryRoomReturnSkybox()
    {
        if (memoryRoomReturnSkybox == null)
            return;

        RenderSettings.skybox = memoryRoomReturnSkybox;
        DynamicGI.UpdateEnvironment();
    }

    private void MovePlayerAndAssistantToOffice()
    {
        MoveToSpawn(player, backToOfficePlayerSpawnPoint);
        MoveToSpawn(assistantRobot, backToOfficeAssistantSpawnPoint);
    }

    private IEnumerator ReturnToMemoryRoomRoutine()
    {
        bool releaseDialogueComplete = usesExternalAllMemoriesDialogue || assistantController == null;

        if (!usesExternalAllMemoriesDialogue && assistantController != null)
        {
            assistantController.PlayAllMemoriesReleasedSequence(() => releaseDialogueComplete = true);

            while (!releaseDialogueComplete)
                yield return null;
        }

        if (screenFadeController != null)
        {
            screenFadeController.SetColor(backToOfficeFadeColor);
            yield return screenFadeController.FadeTo(1f, memoryRoomReturnFadeOutDuration);
        }

        HideGiantClown();
        SetActiveSceneRoot(SceneRoot.Hippo);
        ApplyMemoryRoomReturnSkybox();
        SwitchToHippocampusVolume();
        RestoreHippocampusParticles();
        SetCrystalBallEnabled(false);
        MoveToSpawn(player, hippocampusSpawnPoint);
        MoveToSpawn(assistantRobot, assistantHippocampusSpawnPoint);
        RestoreResolvedMemoryObjects();
        SetState(GameState.FinalMemoryPlacement);

        if (screenFadeController != null)
            yield return screenFadeController.FadeTo(0f, memoryRoomReturnFadeInDuration);

        bool returnDialogueComplete = usesExternalAllMemoriesDialogue || assistantController == null;
        if (!usesExternalAllMemoriesDialogue && assistantController != null)
        {
            assistantController.PlayReturnToMemorySpaceSequence(() => returnDialogueComplete = true);

            while (!returnDialogueComplete)
                yield return null;
        }

        memoryRoomReturnStarted = false;
    }

    private void HideGiantClown()
    {
        if (clownController != null)
            clownController.gameObject.SetActive(false);
    }

    private void ReturnToOfficeAfterConfirmation()
    {
        if (state == GameState.FinalMemoryPlacement)
            SetState(GameState.BackToOffice);
    }

    private void EnterSessionComplete()
    {
        if (state == GameState.BackToOffice)
            SetState(GameState.SessionComplete);
    }

    private void RestoreResolvedMemoryObjects()
    {
        if (resolvedMemoryObjects == null)
            return;

        Transform parent = memoryRoomReturnParent != null
            ? memoryRoomReturnParent
            : hippoRoot != null ? hippoRoot.transform : null;
        Vector3 fallbackOrigin = hippocampusSpawnPoint != null
            ? hippocampusSpawnPoint.position
            : parent != null ? parent.position : Vector3.zero;

        for (int i = 0; i < resolvedMemoryObjects.Length; i++)
        {
            Transform memoryObject = resolvedMemoryObjects[i];
            if (memoryObject == null)
                continue;

            if (parent != null)
                memoryObject.SetParent(parent, true);

            Transform returnPoint = memoryRoomReturnPoints != null && i < memoryRoomReturnPoints.Length
                ? memoryRoomReturnPoints[i]
                : null;

            if (returnPoint != null)
            {
                memoryObject.SetPositionAndRotation(returnPoint.position, returnPoint.rotation);
            }
            else
            {
                int column = i % 2;
                int row = i / 2;
                memoryObject.position = fallbackOrigin + new Vector3(column * 0.45f, 0.35f + row * 0.3f, row * 0.35f);
                memoryObject.rotation = Quaternion.identity;
            }

            memoryObject.gameObject.SetActive(true);

            Rigidbody body = memoryObject.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = false;
                body.useGravity = true;
            }

            MemoryPlacementItem placementItem = memoryObject.GetComponent<MemoryPlacementItem>();
            if (placementItem != null)
                placementItem.SetGrabbable(true);
        }
    }

    private static void MoveToSpawn(Transform target, Transform spawnPoint)
    {
        if (target == null || spawnPoint == null)
            return;

        target.position = spawnPoint.position;
        target.rotation = spawnPoint.rotation;
    }

    private IEnumerator ReturnToOfficeRoutine()
    {
        if (screenFadeController == null)
        {
            Debug.LogError("ReturnToOffice: missing ScreenFadeController.");
            SetActiveSceneRoot(SceneRoot.Office);
            SwitchToOfficeVolume();
            MovePlayerAndAssistantToOffice();
            PrepareFinalReport();

            bool externalDialogueHandler = ReturnedToOffice != null;
            ReturnedToOffice?.Invoke();

            if (!externalDialogueHandler && assistantController != null)
                assistantController.PlayOfficeReturnAndReport();

            yield break;
        }

        screenFadeController.SetColor(backToOfficeFadeColor);
        yield return screenFadeController.FadeTo(1f, backToOfficeFadeOutDuration);

        SetActiveSceneRoot(SceneRoot.Office);
        SwitchToOfficeVolume();
        MovePlayerAndAssistantToOffice();
        PrepareFinalReport();

        yield return screenFadeController.FadeTo(0f, backToOfficeFadeInDuration);

        bool usesExternalOfficeReturnDialogue = ReturnedToOffice != null;
        ReturnedToOffice?.Invoke();

        if (!usesExternalOfficeReturnDialogue && assistantController != null)
            assistantController.PlayOfficeReturnAndReport();
    }

    private void PrepareFinalReport()
    {
        if (finalReportController != null)
        {
            finalReportController.PrepareReportReady(
                memoryPlacementController,
                HandleCloseSessionRequested
            );
        }
    }

    private IEnumerator CloseSessionRoutine()
    {
        if (assistantController != null)
            assistantController.ClearSubtitle();

        if (screenFadeController != null)
        {
            screenFadeController.SetColor(Color.black);
            yield return screenFadeController.FadeTo(1f, closeSessionFadeDuration);
        }
        else
        {
            Debug.LogError("CloseSession: missing ScreenFadeController.", this);
        }

        SetPlayerMovementLocked(true);
        SetActiveSceneRoot(SceneRoot.None);
        closeSessionRoutine = null;
    }

    private IEnumerator TransitionToHippocampusRoutine()
    {
        if (screenFadeController == null || player == null || hippocampusSpawnPoint == null
            || assistantRobot == null || assistantHippocampusSpawnPoint == null)
        {
            Debug.LogError("TransitionToHippocampus: missing required references.");
            yield break;
        }

        screenFadeController.SetColor(hippocampusFadeColor);
        screenFadeController.SetAlpha(1f);

        SetActiveSceneRoot(SceneRoot.Hippo);
        SwitchToHippocampusVolume();

        MoveToSpawn(player, hippocampusSpawnPoint);
        MoveToSpawn(assistantRobot, assistantHippocampusSpawnPoint);

        yield return screenFadeController.FadeTo(0f, 1f);

        SetState(GameState.Hippocampus);
    }
}
