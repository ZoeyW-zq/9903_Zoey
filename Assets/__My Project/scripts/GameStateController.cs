using System.Collections;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public enum GameState
    {
        None,
        OfficeIntro,
        AwaitCrystalBall,
        TransitionToHippocampus,
        Hippocampus,
        AwaitMemoryPlacement,
        GiantCrisis,
        SwallowTransition,
        MirrorChamber,
        BackToOffice
    }

    [Header("Current State")]
    [SerializeField] private GameState state = GameState.None;


    [Header("References")]
    [SerializeField] private AssistantController assistantController;
    [SerializeField] private CrystalBall crystalBall;
    [SerializeField] private ScreenFadeController screenFadeController;
    [SerializeField] private Transform player;
    [SerializeField] private Transform assistantRobot;
    [SerializeField] private Transform assistantHippocampusSpawnPoint;
    [SerializeField] private Transform hippocampusSpawnPoint;
    [SerializeField] private Transform assistantMirrorChamberSpawnPoint;
    [SerializeField] private Transform backToOfficePlayerSpawnPoint;
    [SerializeField] private Transform backToOfficeAssistantSpawnPoint;
    [SerializeField] private ClownController clownController;
    [SerializeField] private SwallowController swallowController;

    [Header("Scene Roots")]
    [SerializeField] private GameObject officeRoot;
    [SerializeField] private GameObject hippoRoot;
    [SerializeField] private GameObject dungeonRoot;

    [Header("Transition Colors")]
    [SerializeField] private Color hippocampusFadeColor = Color.white;
    [SerializeField] private Color backToOfficeFadeColor = Color.black;

    [Header("Back To Office Transition")]
    [SerializeField] private float backToOfficeFadeOutDuration = 1f;
    [SerializeField] private float backToOfficeFadeInDuration = 1f;

    public GameState State => state;

    private void Start()
    {
        SetState(GameState.OfficeIntro);
    }

    public void SetState(GameState newState)
    {
        if (state == newState)
            return;

        ExitState(state);

        state = newState;
        Debug.Log("Game State changed to: " + state);

        EnterState(state);
    }

    public void HandleFinalChamberGlassShattered()
    {
        if (state != GameState.MirrorChamber)
            return;

        if (assistantController != null)
            assistantController.PlayGlassBrokenReturnSequence();
        else
            SetState(GameState.BackToOffice);
    }

    private void EnterState(GameState newState)
    {
        switch (newState)
        {
            case GameState.OfficeIntro:
                SetActiveSceneRoot(SceneRoot.Office);
                assistantController.PlayIntro();
                break;

            case GameState.AwaitCrystalBall:
                SetActiveSceneRoot(SceneRoot.Office);
                crystalBall.SetEnabled(true);
                break;

            case GameState.TransitionToHippocampus:
                StartCoroutine(TransitionToHippocampusRoutine());
                break;

            case GameState.Hippocampus:
                SetActiveSceneRoot(SceneRoot.Hippo);
                crystalBall.SetEnabled(false);
                assistantController.PlayHippocampusIntro();
                break;
            
            case GameState.AwaitMemoryPlacement:
                SetActiveSceneRoot(SceneRoot.Hippo);
                break;

            case GameState.GiantCrisis:
                SetActiveSceneRoot(SceneRoot.Hippo);
                clownController.StartCrisisSequence();
                break;

            case GameState.SwallowTransition:
                swallowController.StartSwallowTransition();
                break;

            case GameState.MirrorChamber:
                SetActiveSceneRoot(SceneRoot.Dungeon);
                if (assistantRobot != null && assistantMirrorChamberSpawnPoint != null)
                {
                    assistantRobot.position = assistantMirrorChamberSpawnPoint.position;
                    assistantRobot.rotation = assistantMirrorChamberSpawnPoint.rotation;
                }
                assistantController.PlayMirrorChamberIntro();
                break;

            case GameState.BackToOffice:
                StartCoroutine(ReturnToOfficeRoutine());
                break;
        }
    }

    private void ExitState(GameState oldState)
    {
        switch (oldState)
        {
            case GameState.AwaitCrystalBall:
                crystalBall.SetEnabled(false);
                break;
        }
    }

    private enum SceneRoot
    {
        Office,
        Hippo,
        Dungeon
    }

    private void SetActiveSceneRoot(SceneRoot activeRoot)
    {
        SetRootActive(officeRoot, activeRoot == SceneRoot.Office);
        SetRootActive(hippoRoot, activeRoot == SceneRoot.Hippo);
        SetRootActive(dungeonRoot, activeRoot == SceneRoot.Dungeon);
    }

    private void SetRootActive(GameObject root, bool active)
    {
        if (root != null && root.activeSelf != active)
            root.SetActive(active);
    }

    private void MovePlayerAndAssistantToOffice()
    {
        if (player != null && backToOfficePlayerSpawnPoint != null)
        {
            player.position = backToOfficePlayerSpawnPoint.position;
            player.rotation = backToOfficePlayerSpawnPoint.rotation;
        }

        if (assistantRobot != null && backToOfficeAssistantSpawnPoint != null)
        {
            assistantRobot.position = backToOfficeAssistantSpawnPoint.position;
            assistantRobot.rotation = backToOfficeAssistantSpawnPoint.rotation;
        }
    }

    private IEnumerator ReturnToOfficeRoutine()
    {
        if (screenFadeController == null)
        {
            Debug.LogError("ReturnToOffice: missing ScreenFadeController.");
            SetActiveSceneRoot(SceneRoot.Office);
            MovePlayerAndAssistantToOffice();
            yield break;
        }

        screenFadeController.SetColor(backToOfficeFadeColor);
        yield return screenFadeController.FadeTo(1f, backToOfficeFadeOutDuration);

        SetActiveSceneRoot(SceneRoot.Office);
        MovePlayerAndAssistantToOffice();

        yield return screenFadeController.FadeTo(0f, backToOfficeFadeInDuration);
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

        player.position = hippocampusSpawnPoint.position;
        player.rotation = hippocampusSpawnPoint.rotation;

        assistantRobot.position = assistantHippocampusSpawnPoint.position;
        assistantRobot.rotation = assistantHippocampusSpawnPoint.rotation;

        yield return screenFadeController.FadeTo(0f, 1f);

        SetState(GameState.Hippocampus);
    }
}
