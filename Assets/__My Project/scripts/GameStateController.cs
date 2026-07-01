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
        MirrorChamber
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
    [SerializeField] private ClownController clownController;
    [SerializeField] private SwallowController swallowController;

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

    private void EnterState(GameState newState)
    {
        switch (newState)
        {
            case GameState.OfficeIntro:
                assistantController.PlayIntro();
                break;

            case GameState.AwaitCrystalBall:
                crystalBall.SetEnabled(true);
                break;

            case GameState.TransitionToHippocampus:
                StartCoroutine(TransitionToHippocampusRoutine());
                break;

            case GameState.Hippocampus:
                crystalBall.SetEnabled(false);
                assistantController.PlayHippocampusIntro();
                break;
            
            case GameState.AwaitMemoryPlacement:
                break;

            case GameState.GiantCrisis:
                clownController.StartCrisisSequence();
                break;

            case GameState.SwallowTransition:
                swallowController.StartSwallowTransition();
                break;

            case GameState.MirrorChamber:
                if (assistantRobot != null && assistantMirrorChamberSpawnPoint != null)
                {
                    assistantRobot.position = assistantMirrorChamberSpawnPoint.position;
                    assistantRobot.rotation = assistantMirrorChamberSpawnPoint.rotation;
                }
                assistantController.PlayMirrorChamberIntro();
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


    private IEnumerator TransitionToHippocampusRoutine()
    {
        if (screenFadeController == null || player == null || hippocampusSpawnPoint == null
            || assistantRobot == null || assistantHippocampusSpawnPoint == null)
        {
            Debug.LogError("TransitionToHippocampus: missing required references.");
            yield break;
        }

        yield return screenFadeController.FadeTo(1f, 1f);

        yield return new WaitForSeconds(1);
        player.position = hippocampusSpawnPoint.position;
        player.rotation = hippocampusSpawnPoint.rotation;

        assistantRobot.position = assistantHippocampusSpawnPoint.position;
        assistantRobot.rotation = assistantHippocampusSpawnPoint.rotation;

        yield return screenFadeController.FadeTo(0f, 1f);

        SetState(GameState.Hippocampus);
    }
}