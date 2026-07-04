using StarterAssets;
using UnityEngine;

public class PlayerMovementLockController : MonoBehaviour
{
    [Header("VR Movement And Teleport Components")]
    [Tooltip("Drag movement and teleport components here. Do not drag turn providers if turning should remain available.")]
    [SerializeField] private Behaviour[] disableWhenMovementLocked;

    [Header("WebGL Movement Controllers")]
    [Tooltip("Drag WebGL FirstPersonController components here. Mouse look remains available while movement is locked.")]
    [SerializeField] private FirstPersonController[] firstPersonControllers;

    private bool movementLocked;
    private bool[] disabledComponentStates;
    private bool[] firstPersonMovementStates;

    public void SetMovementLocked(bool locked)
    {
        if (movementLocked == locked)
            return;

        movementLocked = locked;

        if (locked)
        {
            CacheCurrentStates();
            ApplyLockedState();
        }
        else
        {
            RestoreCachedStates();
        }
    }

    private void CacheCurrentStates()
    {
        int disabledComponentCount = disableWhenMovementLocked != null ? disableWhenMovementLocked.Length : 0;
        disabledComponentStates = new bool[disabledComponentCount];
        for (int i = 0; i < disabledComponentCount; i++)
        {
            Behaviour component = disableWhenMovementLocked[i];
            disabledComponentStates[i] = component != null && component.enabled;
        }

        int firstPersonControllerCount = firstPersonControllers != null ? firstPersonControllers.Length : 0;
        firstPersonMovementStates = new bool[firstPersonControllerCount];
        for (int i = 0; i < firstPersonControllerCount; i++)
        {
            FirstPersonController controller = firstPersonControllers[i];
            firstPersonMovementStates[i] = controller != null && controller.MovementInputEnabled;
        }
    }

    private void ApplyLockedState()
    {
        if (disableWhenMovementLocked != null)
        {
            foreach (Behaviour component in disableWhenMovementLocked)
            {
                if (component != null)
                    component.enabled = false;
            }
        }

        if (firstPersonControllers != null)
        {
            foreach (FirstPersonController controller in firstPersonControllers)
            {
                if (controller != null)
                    controller.SetMovementInputEnabled(false);
            }
        }
    }

    private void RestoreCachedStates()
    {
        int disabledComponentCount = disableWhenMovementLocked != null ? disableWhenMovementLocked.Length : 0;
        for (int i = 0; i < disabledComponentCount; i++)
        {
            Behaviour component = disableWhenMovementLocked[i];
            if (component != null)
                component.enabled = disabledComponentStates != null && i < disabledComponentStates.Length && disabledComponentStates[i];
        }

        int firstPersonControllerCount = firstPersonControllers != null ? firstPersonControllers.Length : 0;
        for (int i = 0; i < firstPersonControllerCount; i++)
        {
            FirstPersonController controller = firstPersonControllers[i];
            if (controller != null)
                controller.SetMovementInputEnabled(firstPersonMovementStates != null && i < firstPersonMovementStates.Length && firstPersonMovementStates[i]);
        }
    }
}
