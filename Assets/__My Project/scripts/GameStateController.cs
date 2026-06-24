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
        Hippocampus
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