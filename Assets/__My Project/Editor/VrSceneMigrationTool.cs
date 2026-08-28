using System;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Object = UnityEngine.Object;

public static class VrSceneMigrationTool
{
    private const string WebScenePath = "Assets/__My Project/scene_WebGL.unity";
    private const string LegacyVrScenePath = "Assets/__My Project/scene_VR.unity";
    private const string MigratedVrScenePath = "Assets/__My Project/scene_VR_Migrated.unity";

    [MenuItem("Tools/Memory Organizer/Migrate WebGL Scene To VR")]
    public static void Migrate()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(WebScenePath) == null)
            throw new InvalidOperationException($"Missing source scene: {WebScenePath}");

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LegacyVrScenePath) == null)
            throw new InvalidOperationException($"Missing legacy VR scene: {LegacyVrScenePath}");

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MigratedVrScenePath) != null)
        {
            RepairExistingMigration();
            return;
        }

        if (!AssetDatabase.CopyAsset(WebScenePath, MigratedVrScenePath))
            throw new InvalidOperationException("Unity could not copy the WebGL scene.");

        AssetDatabase.Refresh();

        Scene migratedScene = EditorSceneManager.OpenScene(MigratedVrScenePath, OpenSceneMode.Single);
        int webObjectCount = CountGameObjects(migratedScene);

        GameStateController migratedGameState = FindComponentInScene<GameStateController>(migratedScene);
        FirstPersonController firstPersonController = FindComponentInScene<FirstPersonController>(migratedScene);
        CrystalBall_WebGL webCrystalBall = FindComponentInScene<CrystalBall_WebGL>(migratedScene);

        Require(migratedGameState, "GameStateController in migrated scene");
        Require(firstPersonController, "FirstPersonController in migrated scene");
        Require(webCrystalBall, "CrystalBall_WebGL in migrated scene");

        GameObject webPlayerRoot = firstPersonController.transform.root.gameObject;
        int removedWebPlayerObjects = CountHierarchy(webPlayerRoot.transform);

        Scene legacyVrScene = EditorSceneManager.OpenScene(LegacyVrScenePath, OpenSceneMode.Additive);
        int legacyVrObjectCount = CountGameObjects(legacyVrScene);

        GameStateController legacyGameState = FindComponentInScene<GameStateController>(legacyVrScene);
        CrystalBall legacyCrystalBall = FindComponentInScene<CrystalBall>(legacyVrScene);
        Require(legacyGameState, "GameStateController in legacy VR scene");
        Require(legacyCrystalBall, "CrystalBall in legacy VR scene");

        Transform legacyPlayer = GetObjectReference<Transform>(legacyGameState, "player");
        Require(legacyPlayer, "legacy VR player reference");

        GameObject legacyXrRoot = legacyPlayer.root.gameObject;
        GameObject migratedXrRoot = Object.Instantiate(legacyXrRoot);
        migratedXrRoot.name = legacyXrRoot.name;
        SceneManager.MoveGameObjectToScene(migratedXrRoot, migratedScene);
        int copiedXrObjects = CountHierarchy(migratedXrRoot.transform);

        ScreenFadeController legacyFade = GetObjectReference<ScreenFadeController>(legacyGameState, "screenFadeController");
        ScreenFadeController migratedFade = MapComponent(legacyXrRoot.transform, migratedXrRoot.transform, legacyFade)
            ?? migratedXrRoot.GetComponentInChildren<ScreenFadeController>(true);
        Require(migratedFade, "ScreenFadeController copied with XR Origin");

        AudioSource legacyHeartbeat = GetObjectReference<AudioSource>(legacyGameState, "heartBeatAudio");
        AudioSource migratedHeartbeat = MapComponent(legacyXrRoot.transform, migratedXrRoot.transform, legacyHeartbeat)
            ?? migratedXrRoot.GetComponentsInChildren<AudioSource>(true)
                .FirstOrDefault(source => source.name.IndexOf("heart", StringComparison.OrdinalIgnoreCase) >= 0);

        Camera xrCamera = migratedXrRoot.GetComponentsInChildren<Camera>(true)
            .FirstOrDefault(camera => camera.CompareTag("MainCamera"))
            ?? migratedXrRoot.GetComponentInChildren<Camera>(true);
        Require(xrCamera, "XR camera copied with XR Origin");

        CrystalBall migratedCrystalBall = ConvertCrystalBall(
            webCrystalBall,
            legacyCrystalBall,
            legacyXrRoot.transform,
            migratedXrRoot.transform,
            migratedGameState,
            migratedFade);

        SetObjectReference(migratedGameState, "player", migratedXrRoot.transform);
        SetObjectReference(migratedGameState, "screenFadeController", migratedFade);
        SetObjectReference(migratedGameState, "crystalBall", migratedCrystalBall);
        if (migratedHeartbeat != null)
            SetObjectReference(migratedGameState, "heartBeatAudio", migratedHeartbeat);

        ClownController clownController = FindComponentInScene<ClownController>(migratedScene);
        if (clownController != null)
        {
            SetObjectReference(clownController, "xrOrigin", migratedXrRoot.transform);
            SetObjectReference(clownController, "playerHead", xrCamera.transform);
        }

        SwallowController swallowController = FindComponentInScene<SwallowController>(migratedScene);
        if (swallowController != null)
        {
            SetObjectReference(swallowController, "xrOrigin", migratedXrRoot.transform);
            SetObjectReference(swallowController, "screenFadeController", migratedFade);

            SwallowController legacySwallow = FindComponentInScene<SwallowController>(legacyVrScene);
            AudioSource legacyAfterTeleportAudio = legacySwallow != null
                ? GetObjectReference<AudioSource>(legacySwallow, "afterTeleportAudio")
                : null;
            AudioSource migratedAfterTeleportAudio = MapComponent(
                legacyXrRoot.transform,
                migratedXrRoot.transform,
                legacyAfterTeleportAudio);
            if (migratedAfterTeleportAudio != null)
                SetObjectReference(swallowController, "afterTeleportAudio", migratedAfterTeleportAudio);
        }

        PlayerMovementLockController migratedMovementLock = FindComponentInScene<PlayerMovementLockController>(migratedScene);
        PlayerMovementLockController legacyMovementLock = FindComponentInScene<PlayerMovementLockController>(legacyVrScene);
        if (migratedMovementLock != null && legacyMovementLock != null)
        {
            CopyMappedComponentArray(
                legacyMovementLock,
                migratedMovementLock,
                "disableWhenMovementLocked",
                legacyXrRoot.transform,
                migratedXrRoot.transform);
            ClearArray(migratedMovementLock, "firstPersonControllers");
        }

        Object.DestroyImmediate(webPlayerRoot);

        int xrGrabCount = AddVrGrabSupport(migratedScene);
        int xrGrabStateSyncCount = SynchronizeVrGrabEnabledStates(migratedScene);
        int xrCanvasCount = AddVrCanvasSupport(migratedScene);

        EditorSceneManager.MarkSceneDirty(migratedScene);
        if (!EditorSceneManager.SaveScene(migratedScene, MigratedVrScenePath))
            throw new InvalidOperationException("Unity could not save the migrated VR scene.");

        EditorSceneManager.CloseScene(legacyVrScene, true);
        AssetDatabase.SaveAssets();

        int finalObjectCount = CountGameObjects(migratedScene);
        Debug.Log(
            "VR_SCENE_MIGRATION_COMPLETE\n" +
            $"Source WebGL objects: {webObjectCount}\n" +
            $"Legacy VR objects: {legacyVrObjectCount}\n" +
            $"Removed WebGL player objects: {removedWebPlayerObjects}\n" +
            $"Copied XR Origin objects: {copiedXrObjects}\n" +
            $"Added XRGrabInteractable components: {xrGrabCount}\n" +
            $"Synchronized XRGrabInteractable enabled states: {xrGrabStateSyncCount}\n" +
            $"Added TrackedDeviceGraphicRaycaster components: {xrCanvasCount}\n" +
            $"Final migrated objects: {finalObjectCount}\n" +
            $"Saved scene: {MigratedVrScenePath}");
    }

    private static void RepairExistingMigration()
    {
        Scene migratedScene = SceneManager.GetSceneByPath(MigratedVrScenePath);
        if (!migratedScene.IsValid() || !migratedScene.isLoaded)
            migratedScene = EditorSceneManager.OpenScene(MigratedVrScenePath, OpenSceneMode.Single);

        GameStateController migratedGameState = FindComponentInScene<GameStateController>(migratedScene);
        SwallowController migratedSwallow = FindComponentInScene<SwallowController>(migratedScene);
        CrystalBall migratedCrystalBall = FindComponentInScene<CrystalBall>(migratedScene);
        ClownController migratedClown = FindComponentInScene<ClownController>(migratedScene);
        Require(migratedGameState, "GameStateController in migrated scene");
        Require(migratedSwallow, "SwallowController in migrated scene");
        Require(migratedCrystalBall, "CrystalBall in migrated scene");
        Require(migratedClown, "ClownController in migrated scene");

        Transform migratedPlayer = GetObjectReference<Transform>(migratedGameState, "player");
        ScreenFadeController migratedFade = GetObjectReference<ScreenFadeController>(
            migratedGameState,
            "screenFadeController");
        Require(migratedPlayer, "migrated XR player reference");
        Require(migratedFade, "migrated screen fade reference");
        Transform migratedXrRoot = migratedPlayer.root;

        Scene legacyVrScene = EditorSceneManager.OpenScene(LegacyVrScenePath, OpenSceneMode.Additive);
        GameStateController legacyGameState = FindComponentInScene<GameStateController>(legacyVrScene);
        SwallowController legacySwallow = FindComponentInScene<SwallowController>(legacyVrScene);
        Require(legacyGameState, "GameStateController in legacy VR scene");
        Require(legacySwallow, "SwallowController in legacy VR scene");

        Transform legacyPlayer = GetObjectReference<Transform>(legacyGameState, "player");
        AudioSource legacyAfterTeleportAudio = GetObjectReference<AudioSource>(
            legacySwallow,
            "afterTeleportAudio");
        Require(legacyPlayer, "legacy XR player reference");
        Require(legacyAfterTeleportAudio, "legacy after-teleport audio reference");

        AudioSource migratedAfterTeleportAudio = MapComponent(
            legacyPlayer.root,
            migratedXrRoot,
            legacyAfterTeleportAudio);
        Require(migratedAfterTeleportAudio, "after-teleport audio copied with XR Origin");

        SetObjectReference(migratedSwallow, "xrOrigin", migratedXrRoot);
        SetObjectReference(migratedSwallow, "screenFadeController", migratedFade);
        SetObjectReference(migratedSwallow, "afterTeleportAudio", migratedAfterTeleportAudio);

        int xrGrabCount = AddVrGrabSupport(migratedScene);
        int xrGrabStateSyncCount = SynchronizeVrGrabEnabledStates(migratedScene);

        EditorSceneManager.MarkSceneDirty(migratedScene);
        if (!EditorSceneManager.SaveScene(migratedScene, MigratedVrScenePath))
            throw new InvalidOperationException("Unity could not save the repaired VR scene.");

        EditorSceneManager.CloseScene(legacyVrScene, true);
        AssetDatabase.SaveAssets();
        Debug.Log(
            "VR_SCENE_MIGRATION_REPAIRED\n" +
            "Rebound SwallowController.xrOrigin, screenFadeController, and afterTeleportAudio.\n" +
            $"Added missing XRGrabInteractable components: {xrGrabCount}\n" +
            $"Synchronized XRGrabInteractable enabled states: {xrGrabStateSyncCount}\n" +
            $"Saved scene: {MigratedVrScenePath}");
    }

    private static CrystalBall ConvertCrystalBall(
        CrystalBall_WebGL webCrystalBall,
        CrystalBall legacyCrystalBall,
        Transform legacyXrRoot,
        Transform migratedXrRoot,
        GameStateController gameState,
        ScreenFadeController screenFade)
    {
        GameObject ballObject = webCrystalBall.gameObject;
        Object.DestroyImmediate(webCrystalBall);

        CrystalBall migratedCrystalBall = ballObject.AddComponent<CrystalBall>();
        CopyValueProperties(legacyCrystalBall, migratedCrystalBall, new[]
        {
            "holdDistance",
            "holdTime",
            "fadeResetDuration",
            "entryFadeColor",
            "hapticAmplitude",
            "hapticDuration",
            "hapticInterval"
        });

        Transform legacyBallCenter = GetObjectReference<Transform>(legacyCrystalBall, "crystalBallCenter");
        Transform migratedBallCenter = MapTransform(
            legacyCrystalBall.transform,
            migratedCrystalBall.transform,
            legacyBallCenter) ?? migratedCrystalBall.transform;

        Transform legacyLeftHand = GetObjectReference<Transform>(legacyCrystalBall, "leftHandProxy");
        Transform legacyRightHand = GetObjectReference<Transform>(legacyCrystalBall, "rightHandProxy");
        Transform migratedLeftHand = MapTransform(legacyXrRoot, migratedXrRoot, legacyLeftHand);
        Transform migratedRightHand = MapTransform(legacyXrRoot, migratedXrRoot, legacyRightHand);

        Require(migratedLeftHand, "left hand proxy copied with XR Origin");
        Require(migratedRightHand, "right hand proxy copied with XR Origin");

        SetObjectReference(migratedCrystalBall, "crystalBallCenter", migratedBallCenter);
        SetObjectReference(migratedCrystalBall, "leftHandProxy", migratedLeftHand);
        SetObjectReference(migratedCrystalBall, "rightHandProxy", migratedRightHand);
        SetObjectReference(migratedCrystalBall, "gameStateController", gameState);
        SetObjectReference(migratedCrystalBall, "screenFadeController", screenFade);
        SetBool(migratedCrystalBall, "enabledForEntry", false);
        return migratedCrystalBall;
    }

    private static int AddVrGrabSupport(Scene scene)
    {
        int added = 0;
        foreach (MemoryPlacementItem item in FindComponentsInScene<MemoryPlacementItem>(scene))
        {
            if (item.GetComponent<XRGrabInteractable>() != null)
                continue;

            item.gameObject.AddComponent<XRGrabInteractable>();
            added++;
        }

        return added;
    }

    private static int SynchronizeVrGrabEnabledStates(Scene scene)
    {
        int synchronized = 0;
        foreach (MemoryPlacementItem item in FindComponentsInScene<MemoryPlacementItem>(scene))
        {
            XRGrabInteractable xrGrab = item.GetComponent<XRGrabInteractable>();
            if (xrGrab == null)
                continue;

            Holdable holdable = item.GetComponent<Holdable>();
            bool shouldBeEnabled = holdable == null || holdable.enabled;
            if (xrGrab.enabled == shouldBeEnabled)
                continue;

            xrGrab.enabled = shouldBeEnabled;
            synchronized++;
        }

        return synchronized;
    }

    private static int AddVrCanvasSupport(Scene scene)
    {
        int added = 0;
        foreach (Canvas canvas in FindComponentsInScene<Canvas>(scene))
        {
            if (canvas.renderMode != RenderMode.WorldSpace
                || canvas.GetComponent<GraphicRaycaster>() == null
                || canvas.GetComponent<TrackedDeviceGraphicRaycaster>() != null)
            {
                continue;
            }

            canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            added++;
        }

        return added;
    }

    private static void CopyMappedComponentArray(
        Component source,
        Component destination,
        string propertyName,
        Transform sourceRoot,
        Transform destinationRoot)
    {
        SerializedProperty sourceProperty = new SerializedObject(source).FindProperty(propertyName);
        SerializedObject destinationObject = new SerializedObject(destination);
        SerializedProperty destinationProperty = destinationObject.FindProperty(propertyName);
        if (sourceProperty == null || destinationProperty == null)
            throw new InvalidOperationException($"Missing serialized array: {propertyName}");

        destinationProperty.arraySize = sourceProperty.arraySize;
        for (int i = 0; i < sourceProperty.arraySize; i++)
        {
            Component sourceComponent = sourceProperty.GetArrayElementAtIndex(i).objectReferenceValue as Component;
            destinationProperty.GetArrayElementAtIndex(i).objectReferenceValue =
                MapComponentInternal(sourceRoot, destinationRoot, sourceComponent);
        }

        destinationObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ClearArray(Component component, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        property.arraySize = 0;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CopyValueProperties(Component source, Component destination, IEnumerable<string> propertyNames)
    {
        SerializedObject sourceObject = new SerializedObject(source);
        SerializedObject destinationObject = new SerializedObject(destination);
        foreach (string propertyName in propertyNames)
        {
            SerializedProperty sourceProperty = sourceObject.FindProperty(propertyName);
            SerializedProperty destinationProperty = destinationObject.FindProperty(propertyName);
            if (sourceProperty != null && destinationProperty != null)
                destinationProperty.serializedObject.CopyFromSerializedProperty(sourceProperty);
        }

        destinationObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T GetObjectReference<T>(Component component, string propertyName) where T : Object
    {
        SerializedProperty property = new SerializedObject(component).FindProperty(propertyName);
        return property != null ? property.objectReferenceValue as T : null;
    }

    private static void SetObjectReference(Component component, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName)
            ?? throw new InvalidOperationException($"Missing serialized property {component.GetType().Name}.{propertyName}");
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(Component component, string propertyName, bool value)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName)
            ?? throw new InvalidOperationException($"Missing serialized property {component.GetType().Name}.{propertyName}");
        property.boolValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T MapComponent<T>(Transform sourceRoot, Transform destinationRoot, T sourceComponent)
        where T : Component
    {
        return MapComponentInternal(sourceRoot, destinationRoot, sourceComponent) as T;
    }

    private static Component MapComponentInternal(
        Transform sourceRoot,
        Transform destinationRoot,
        Component sourceComponent)
    {
        if (sourceComponent == null)
            return null;

        Transform mappedTransform = MapTransform(sourceRoot, destinationRoot, sourceComponent.transform);
        if (mappedTransform == null)
            return null;

        Component[] sourceComponents = sourceComponent.transform.GetComponents(sourceComponent.GetType());
        int componentIndex = Array.IndexOf(sourceComponents, sourceComponent);
        Component[] destinationComponents = mappedTransform.GetComponents(sourceComponent.GetType());
        return componentIndex >= 0 && componentIndex < destinationComponents.Length
            ? destinationComponents[componentIndex]
            : null;
    }

    private static Transform MapTransform(Transform sourceRoot, Transform destinationRoot, Transform sourceTransform)
    {
        if (sourceTransform == null || !sourceTransform.IsChildOf(sourceRoot) && sourceTransform != sourceRoot)
            return null;

        string path = AnimationUtility.CalculateTransformPath(sourceTransform, sourceRoot);
        return string.IsNullOrEmpty(path) ? destinationRoot : destinationRoot.Find(path);
    }

    private static T FindComponentInScene<T>(Scene scene) where T : Component
    {
        return FindComponentsInScene<T>(scene).FirstOrDefault();
    }

    private static IEnumerable<T> FindComponentsInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (T component in root.GetComponentsInChildren<T>(true))
                yield return component;
        }
    }

    private static int CountGameObjects(Scene scene)
    {
        return scene.GetRootGameObjects().Sum(root => CountHierarchy(root.transform));
    }

    private static int CountHierarchy(Transform root)
    {
        int count = 1;
        foreach (Transform child in root)
            count += CountHierarchy(child);
        return count;
    }

    private static void Require(Object value, string description)
    {
        if (value == null)
            throw new InvalidOperationException($"Migration requirement not found: {description}");
    }
}
