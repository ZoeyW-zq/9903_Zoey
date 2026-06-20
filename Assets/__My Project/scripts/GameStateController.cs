using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public enum GameState
    {
        OfficeIntro,
        AwaitCrystalBall,
        TransitionToHippocampus,
        Hippocampus
    }
    public GameState State;

    [SerializeField] AssistantController assistantController;
    [SerializeField] CrystalBall crystalBall;
    [SerializeField] Transform player;
    [SerializeField] Transform hippocampusSpawnPoint;

    void Start()
    {
        State=GameState.OfficeIntro;
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case GameState.OfficeIntro:
                assistantController.PlayIntro();
                break;
            case GameState.AwaitCrystalBall:
                crystalBall.SetEnabled(true);
                break;
            case GameState.TransitionToHippocampus:
                GoToHippocampus();
                break;
            case GameState.Hippocampus:
                break;
        }

        void GoToHippocampus()
        {

            player.position = hippocampusSpawnPoint.position;
            player.rotation = hippocampusSpawnPoint.rotation;

            State=GameState.Hippocampus;
        }
    }


}
