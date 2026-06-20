using UnityEngine;

public class CrystalBall : MonoBehaviour
{
    [SerializeField] float holdTime = 2f;
    [SerializeField]  GameStateController gameStateController;


    [SerializeField] bool enabledForEntry;
    [SerializeField] bool handInside;
    [SerializeField] float timer;

    public void SetEnabled(bool value)
    {
        enabledForEntry = value;
        //handInside = false;
        timer = 0f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!enabledForEntry) return;

        if (other.CompareTag("PlayerHand"))
        {
            Debug.Log("Hand Enter");
            handInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabledForEntry) return;

        if (other.CompareTag("PlayerHand"))
        {
            Debug.Log("Hand Exit");
            handInside = false;
            timer = 0f;
        }
    }

    private void Update()
    {
        if (!enabledForEntry || !handInside) return;

        timer += Time.unscaledDeltaTime;
        Debug.Log($"Timer: {timer}");

        if (timer >= holdTime)
        {
            enabledForEntry = false;
            gameStateController.State = GameStateController.GameState.TransitionToHippocampus;
        }
    }

}