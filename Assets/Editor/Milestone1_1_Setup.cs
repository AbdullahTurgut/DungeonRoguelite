using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using DungeonRoguelite.Player;
using System.IO;

namespace DungeonRoguelite.Editor
{
    public static class Milestone1_1_Setup
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/Player.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("DungeonRoguelite/Setup Milestone 1.1")]
        public static void Setup()
        {
            if (EditorPrefs.GetBool("DungeonRoguelite_Milestone1_1_Done", false))
            {
                return;
            }

            Debug.Log("[Milestone 1.1] Beginning automated setup for Milestone 1.1...");

            EnsureDirectories();
            var playerPrefab = CreatePlayerPrefab();
            CreatePrototypeScene(playerPrefab);

            EditorPrefs.SetBool("DungeonRoguelite_Milestone1_1_Done", true);
            Debug.Log("[Milestone 1.1] Setup completed successfully.");
        }

        [MenuItem("DungeonRoguelite/Force Re-Setup Milestone 1.1")]
        public static void ForceSetup()
        {
            EditorPrefs.SetBool("DungeonRoguelite_Milestone1_1_Done", false);
            Setup();
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/Prefabs/Characters"))
                Directory.CreateDirectory("Assets/Prefabs/Characters");
            if (!Directory.Exists("Assets/Scenes/Dungeons"))
                Directory.CreateDirectory("Assets/Scenes/Dungeons");

            AssetDatabase.Refresh();
        }

        private static GameObject CreatePlayerPrefab()
        {
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                Debug.LogWarning($"[Milestone 1.1] Could not find InputActionAsset at {InputActionsPath}");
            }

            // Root GameObject
            var rootGo = new GameObject("Player");
            rootGo.tag = "Player";

            // CharacterController component
            var cc = rootGo.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.radius = 0.5f;
            cc.height = 2f;
            cc.skinWidth = 0.08f;
            cc.minMoveDistance = 0f;

            // PlayerInput component (configured with InputSystem_Actions)
            var playerInput = rootGo.AddComponent<PlayerInput>();
            if (inputActions != null)
            {
                playerInput.actions = inputActions;
                playerInput.defaultActionMap = "Player";
            }

            // PlayerMovement component
            var movement = rootGo.AddComponent<PlayerMovement>();
            var serializedMovement = new SerializedObject(movement);
            serializedMovement.FindProperty("moveSpeed").floatValue = 6f;
            serializedMovement.FindProperty("gravity").floatValue = 20f;
            if (inputActions != null)
            {
                serializedMovement.FindProperty("inputActions").objectReferenceValue = inputActions;
            }
            serializedMovement.ApplyModifiedProperties();

            // Visual child: Placeholder Capsule
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(rootGo.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);

            // Remove CapsuleCollider from visual to avoid interference with CharacterController
            var capsuleCol = visual.GetComponent<CapsuleCollider>();
            if (capsuleCol != null)
            {
                Object.DestroyImmediate(capsuleCol);
            }

            // Save Prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
            Object.DestroyImmediate(rootGo);

            Debug.Log($"[Milestone 1.1] Player prefab successfully created at {PrefabPath}");
            return prefab;
        }

        private static void CreatePrototypeScene(GameObject playerPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Light
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 2. Static Top-Down Camera (Milestone 1.1 specification: static camera for testing only)
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = new Vector3(0f, 16f, -11f);
            cameraGo.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            // 3. Environment: Floor
            var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorGo.name = "Floor";
            floorGo.transform.position = new Vector3(0f, -0.5f, 0f);
            floorGo.transform.localScale = new Vector3(30f, 1f, 30f);

            // 4. Environment: Boundary Walls & Collision Obstacles
            var obstaclesRoot = new GameObject("Environment_Obstacles");

            CreateWall(obstaclesRoot.transform, "Wall_North", new Vector3(0f, 1.5f, 15f), new Vector3(30f, 3f, 1f));
            CreateWall(obstaclesRoot.transform, "Wall_South", new Vector3(0f, 1.5f, -15f), new Vector3(30f, 3f, 1f));
            CreateWall(obstaclesRoot.transform, "Wall_East", new Vector3(15f, 1.5f, 0f), new Vector3(1f, 3f, 30f));
            CreateWall(obstaclesRoot.transform, "Wall_West", new Vector3(-15f, 1.5f, 0f), new Vector3(1f, 3f, 30f));

            // Interior obstacle pillars to test CharacterController collision
            CreateWall(obstaclesRoot.transform, "Pillar_NE", new Vector3(5f, 1.5f, 5f), new Vector3(2f, 3f, 2f));
            CreateWall(obstaclesRoot.transform, "Pillar_SW", new Vector3(-5f, 1.5f, -5f), new Vector3(2f, 3f, 2f));

            // 5. Instantiate Player
            GameObject playerInstance;
            if (playerPrefab != null)
            {
                playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            }
            else
            {
                var loaded = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(loaded);
            }
            playerInstance.name = "Player";
            playerInstance.transform.position = new Vector3(0f, 0f, 0f);

            // 6. Attach runtime verification test component to the scene
            var verifierGo = new GameObject("Milestone1_1_RuntimeVerifier");
            verifierGo.AddComponent<Milestone1_1_Verifier>();

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 1.1] Prototype scene created and saved to {ScenePath}");

            // Register in EditorBuildSettings
            var scenes = EditorBuildSettings.scenes;
            bool existsInBuild = false;
            foreach (var s in scenes)
            {
                if (s.path == ScenePath)
                {
                    existsInBuild = true;
                    break;
                }
            }

            if (!existsInBuild)
            {
                var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
                scenes.CopyTo(newScenes, 0);
                newScenes[scenes.Length] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettings.scenes = newScenes;
                Debug.Log($"[Milestone 1.1] Registered {ScenePath} in EditorBuildSettings.");
            }
        }

        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
        }

        [MenuItem("DungeonRoguelite/Run EditMode Simulation Verification")]
        public static void RunEditModeSimulationVerification()
        {
            Debug.Log("[Milestone 1.1] Running comprehensive EditMode physics and movement verification...");

            var scene = EditorSceneManager.OpenScene(ScenePath);
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null)
            {
                Debug.LogError("[TEST FAILED] Player GameObject not found.");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            var movement = playerGo.GetComponent<PlayerMovement>();
            var cc = playerGo.GetComponent<CharacterController>();

            // 1. Check inspector configuration
            Debug.Log($"[CHECK 1] Configured moveSpeed = {movement.MoveSpeed}, gravity = {movement.Gravity}");
            if (movement.MoveSpeed != 6f)
            {
                Debug.LogError($"[CHECK 1 FAILED] MoveSpeed expected 6f but was {movement.MoveSpeed}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            // 2. Cardinal movement check (+Z)
            Vector3 startPos = playerGo.transform.position;
            movement.SetTestInputOverride(Vector2.up);

            // Step 25 frames with 0.02s dt
            for (int i = 0; i < 25; i++)
            {
                movement.StepMovement(0.02f);
            }

            Vector3 endPos = playerGo.transform.position;
            float dz = endPos.z - startPos.z;
            float dx = Mathf.Abs(endPos.x - startPos.x);
            Debug.Log($"[CHECK 2] Cardinal movement (+Z): deltaZ={dz:F2}m (expected ~3.00m), deltaX={dx:F4}m");
            if (dz < 2.5f || dx > 0.01f)
            {
                Debug.LogError("[CHECK 2 FAILED] Cardinal movement did not move as expected.");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            // 3. Diagonal normalization check
            movement.SetTestInputOverride(new Vector2(1f, 1f));
            movement.StepMovement(0.02f);
            Vector3 diagVel = movement.Velocity;
            float diagSpeed = new Vector2(diagVel.x, diagVel.z).magnitude;
            Debug.Log($"[CHECK 3] Diagonal speed check: measured={diagSpeed:F2} m/s, max expected={movement.MoveSpeed:F2} m/s");
            if (diagSpeed > movement.MoveSpeed + 0.15f)
            {
                Debug.LogError($"[CHECK 3 FAILED] Diagonal speed is unnormalized: {diagSpeed:F2} > {movement.MoveSpeed:F2}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            // 4. CharacterController obstacle collision check
            cc.enabled = false;
            playerGo.transform.position = new Vector3(2.5f, 0f, 5f);
            cc.enabled = true;
            movement.SetTestInputOverride(new Vector2(1f, 0f)); // Towards Pillar_NE at (5, 1.5, 5)

            for (int i = 0; i < 50; i++)
            {
                movement.StepMovement(0.02f);
            }

            Vector3 blockedPos = playerGo.transform.position;
            Debug.Log($"[CHECK 4] Obstacle collision: Player stopped at X={blockedPos.x:F2} (Obstacle surface at X=4.0)");
            if (blockedPos.x >= 3.6f)
            {
                Debug.LogError($"[CHECK 4 FAILED] Player penetrated obstacle! Final X: {blockedPos.x:F2}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            // 5. Grounding check
            Debug.Log($"[CHECK 5] Grounding status: IsGrounded={movement.IsGrounded}");

            movement.SetTestInputOverride(null);
            Debug.Log("<color=green><b>[EDITMODE SIMULATION VERIFICATION: ALL CHECKS PASSED]</b></color>");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        [MenuItem("DungeonRoguelite/Enter PlayMode and Verify")]
        public static void EnterPlayModeAndVerify()
        {
            Debug.Log("[Milestone 1.1] Opening prototype scene and entering Play Mode...");
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.EnterPlaymode();
        }
    }
}
