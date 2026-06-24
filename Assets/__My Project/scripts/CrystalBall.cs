using UnityEngine;

public class CrystalBall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform crystalBallCenter;
    [SerializeField] private Transform leftHandProxy;
    [SerializeField] private Transform rightHandProxy;
    [SerializeField] private GameStateController gameStateController;

    [Header("Settings")]
    [SerializeField] private float holdDistance = 0.2f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private bool enabledForEntry = true;

    [Header("Debug")]
    [SerializeField] private bool handInRange;
    [SerializeField] private float timer;
    [SerializeField] private float leftDistance;
    [SerializeField] private float rightDistance;
    [SerializeField] private float minDistance;

    private bool triggered;

    private void Update()
    {
        if (!enabledForEntry || triggered)
            return;

        if (crystalBallCenter == null || leftHandProxy == null || rightHandProxy == null)
            return;

        leftDistance = Vector3.Distance(leftHandProxy.position, crystalBallCenter.position);
        rightDistance = Vector3.Distance(rightHandProxy.position, crystalBallCenter.position);

        float minDistance = Mathf.Min(leftDistance, rightDistance);

        handInRange = minDistance <= holdDistance;

        if (handInRange)
        {
            timer += Time.deltaTime;

            if (timer >= holdTime)
            {
                triggered = true;
                Debug.Log("Crystal Ball entry triggered.");

                gameStateController.SetState(GameStateController.GameState.TransitionToHippocampus);
            }
        }
        else
        {
            timer = 0f;
        }
    }

    public void SetEnabled(bool value)
    {
        enabledForEntry = value;
        timer = 0f;
        handInRange = false;
        triggered = false;
    }
}