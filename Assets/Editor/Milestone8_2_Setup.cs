using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DungeonRoguelite.Characters;
using DungeonRoguelite.CameraControl;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.UI;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.Editor
{
    public static class Milestone8_2_Setup
    {
        private const string WarriorAssetPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 8.2")]
        public static void Setup()
        {
            Debug.Log("[Milestone 8.2] Configuring PlayerSpawnPoint, PlayerSpawner, and explicit scene system bindings...");
            UpdatePrototypeScene(includeVerifier: false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Milestone 8.2] Setup completed successfully.");
        }

        public static void UpdatePrototypeScene(bool includeVerifier = false)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // 1. Remove any pre-placed Warrior / Player instances
            var existingPlayableCharacters = Object.FindObjectsByType<PlayableCharacter>(FindObjectsSortMode.None);
            foreach (var pc in existingPlayableCharacters)
            {
                if (pc != null)
                {
                    Debug.Log($"[Milestone 8.2] Removing pre-placed character instance: {pc.gameObject.name}");
                    Object.DestroyImmediate(pc.gameObject);
                }
            }

            var oldWarrior = GameObject.Find("Warrior");
            if (oldWarrior != null)
            {
                Object.DestroyImmediate(oldWarrior);
            }
            var oldPlayer = GameObject.Find("Player");
            if (oldPlayer != null)
            {
                Object.DestroyImmediate(oldPlayer);
            }

            // 2. Setup PlayerSpawnPoint
            GameObject spawnPointGo = GameObject.Find("PlayerSpawnPoint");
            if (spawnPointGo == null)
            {
                spawnPointGo = new GameObject("PlayerSpawnPoint");
                Undo.RegisterCreatedObjectUndo(spawnPointGo, "Create PlayerSpawnPoint");
            }
            spawnPointGo.transform.position = Vector3.zero;
            spawnPointGo.transform.rotation = Quaternion.identity;
            EditorUtility.SetDirty(spawnPointGo);

            // 3. Setup PlayerSpawner
            GameObject spawnerGo = GameObject.Find("PlayerSpawner");
            if (spawnerGo == null)
            {
                spawnerGo = new GameObject("PlayerSpawner");
                Undo.RegisterCreatedObjectUndo(spawnerGo, "Create PlayerSpawner");
            }
            spawnerGo.transform.position = Vector3.zero;

            var spawner = spawnerGo.GetComponent<PlayerSpawner>();
            if (spawner == null)
            {
                spawner = spawnerGo.AddComponent<PlayerSpawner>();
            }

            var warriorAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
            if (warriorAsset == null)
            {
                Debug.LogError($"[Milestone 8.2] Character_Warrior asset not found at {WarriorAssetPath}!");
            }

            var soSpawner = new SerializedObject(spawner);
            soSpawner.FindProperty("defaultCharacter").objectReferenceValue = warriorAsset;
            soSpawner.FindProperty("spawnPoint").objectReferenceValue = spawnPointGo.transform;
            soSpawner.FindProperty("autoSpawn").boolValue = true;
            soSpawner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawner);

            // 4. Update CameraFollow
            var cameraFollow = Object.FindFirstObjectByType<CameraFollow>();
            if (cameraFollow != null)
            {
                var so = new SerializedObject(cameraFollow);
                so.FindProperty("playerSpawner").objectReferenceValue = spawner;
                so.FindProperty("target").objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(cameraFollow);
            }

            // 5. Update WaveManager
            var waveManager = Object.FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                var so = new SerializedObject(waveManager);
                so.FindProperty("playerSpawner").objectReferenceValue = spawner;
                so.FindProperty("playerTarget").objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(waveManager);
            }

            // 6. Update UpgradeManager
            var upgradeManager = Object.FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                var so = new SerializedObject(upgradeManager);
                so.FindProperty("playerSpawner").objectReferenceValue = spawner;
                so.FindProperty("playerExperience").objectReferenceValue = null;
                so.FindProperty("playerStats").objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(upgradeManager);
            }

            // 7. Update PlayerExperienceUI
            var xpUI = Object.FindFirstObjectByType<PlayerExperienceUI>();
            if (xpUI != null)
            {
                var so = new SerializedObject(xpUI);
                so.FindProperty("playerSpawner").objectReferenceValue = spawner;
                so.FindProperty("playerExperience").objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(xpUI);
            }

            // 8. Update DungeonRunStats (listens to WaveManager, no PlayerSpawner dependency)
            var runStats = Object.FindFirstObjectByType<DungeonRunStats>();
            if (runStats != null)
            {
                var so = new SerializedObject(runStats);
                so.FindProperty("waveManager").objectReferenceValue = waveManager;
                so.FindProperty("playerExperience").objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(runStats);
            }

            // 9. Update DungeonCompletionController
            var completionController = Object.FindFirstObjectByType<DungeonCompletionController>();
            if (completionController != null)
            {
                var so = new SerializedObject(completionController);
                so.FindProperty("playerSpawner").objectReferenceValue = spawner;
                so.FindProperty("playerExperience").objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(completionController);
            }

            // 10. Clean and optionally attach verifier
            CleanupVerifierFromScene();

            if (includeVerifier)
            {
                var verifierGo = new GameObject("Milestone8_2_RuntimeVerifier");
                verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone8_2_Verifier>();
                Debug.Log("[Milestone 8.2] Attached Milestone8_2_RuntimeVerifier to scene.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Milestone 8.2] Configured Dungeon_Prototype scene with zero pre-placed players. includeVerifier={includeVerifier}");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 8.2 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 8.2] Launching PlayMode verification harness...");
            UpdatePrototypeScene(includeVerifier: false);

            var verifierGo = new GameObject("Milestone8_2_RuntimeVerifier");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone8_2_Verifier>();
            Debug.Log("[Milestone 8.2] Attached transient Milestone8_2_RuntimeVerifier in memory.");

            EditorApplication.EnterPlaymode();
        }

        public static void CleanupVerifierFromScene()
        {
            var oldVerifier = GameObject.Find("Milestone8_2_RuntimeVerifier");
            if (oldVerifier != null)
            {
                Object.DestroyImmediate(oldVerifier);
            }

            var allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var c in allComponents)
            {
                if (c != null && c.GetType().Name.Contains("Verifier"))
                {
                    Debug.Log($"[Milestone 8.2] Removing verifier component from scene: {c.GetType().Name}");
                    Object.DestroyImmediate(c.gameObject);
                }
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isLoaded && !Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
            }
        }
    }
}
