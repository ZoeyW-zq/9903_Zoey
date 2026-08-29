using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GameStateControllerMemoryInteractionTests
{
    [Test]
    public void DeepMemoriesStayLockedUntilTheyReturnToTheMemoryRoom()
    {
        GameObject controllerObject = new GameObject("Game State Test");
        GameObject[] memoryObjects = new GameObject[4];

        try
        {
            GameStateController gameState = controllerObject.AddComponent<GameStateController>();
            gameState.enabled = false;

            Transform[] memoryTransforms = new Transform[memoryObjects.Length];
            for (int i = 0; i < memoryObjects.Length; i++)
            {
                memoryObjects[i] = new GameObject($"Deep Memory {i}");
                memoryObjects[i].AddComponent<MemoryPlacementItem>();
                XRGrabInteractable grabInteractable = memoryObjects[i].AddComponent<XRGrabInteractable>();
                grabInteractable.enabled = true;
                memoryTransforms[i] = memoryObjects[i].transform;
            }

            SetPrivateField(gameState, "resolvedMemoryObjects", memoryTransforms);

            gameState.SetState(GameStateController.GameState.MirrorChamber);

            foreach (GameObject memoryObject in memoryObjects)
                Assert.That(memoryObject.GetComponent<XRGrabInteractable>().enabled, Is.False);

            InvokePrivateMethod(gameState, "RestoreResolvedMemoryObjects");

            foreach (GameObject memoryObject in memoryObjects)
                Assert.That(memoryObject.GetComponent<XRGrabInteractable>().enabled, Is.True);
        }
        finally
        {
            foreach (GameObject memoryObject in memoryObjects)
            {
                if (memoryObject != null)
                    Object.DestroyImmediate(memoryObject);
            }

            Object.DestroyImmediate(controllerObject);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, null);
    }
}
