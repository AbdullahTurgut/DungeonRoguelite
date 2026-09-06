using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Player;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.Editor
{
    public static class Milestone8_1_Setup
    {
        private const string CharactersDir = "Assets/ScriptableObjects/Characters";
        private const string WarriorAssetPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
        private const string WarriorPrefabPath = "Assets/Prefabs/Characters/Warrior.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 8.1")]
        public static void Setup()
        {
            Debug.Log("[Milestone 8.1] Setting up CharacterDefinition, Warrior.prefab, and Dungeon_Prototype scene...");

            CreateOrUpdateWarriorAsset();
            UpdateWarriorPrefab();
            UpdatePrototypeScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Milestone 8.1] Setup completed successfully.");
        }

        public static void CreateOrUpdateWarriorAsset()
        {
            if (!AssetDatabase.IsValidFolder(CharactersDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                {
                    AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
                }
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Characters");
            }

            var warriorAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
            if (warriorAsset == null)
            {
                warriorAsset = ScriptableObject.CreateInstance<CharacterDefinition>();
                AssetDatabase.CreateAsset(warriorAsset, WarriorAssetPath);
            }

            var so = new SerializedObject(warriorAsset);
            so.FindProperty("id").stringValue = "warrior";
            so.FindProperty("displayName").stringValue = "Warrior";
            so.FindProperty("description").stringValue = "A sturdy melee fighter with high survivability and sweeping sword attacks.";

            var warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefabPath);
            if (warriorPrefab != null)
            {
                so.FindProperty("characterPrefab").objectReferenceValue = warriorPrefab;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(warriorAsset);
            Debug.Log($"[Milestone 8.1] Character_Warrior asset configured at: {WarriorAssetPath}");
        }

        public static void UpdateWarriorPrefab()
        {
            var rootGo = PrefabUtility.LoadPrefabContents(WarriorPrefabPath);
            if (rootGo == null)
            {
                Debug.LogError($"[Milestone 8.1] Failed to load prefab contents at: {WarriorPrefabPath}");
                return;
            }

            try
            {
                rootGo.name = "Warrior";
                rootGo.tag = "Player";

                var playableChar = rootGo.GetComponent<PlayableCharacter>();
                if (playableChar == null)
                {
                    playableChar = rootGo.AddComponent<PlayableCharacter>();
                }

                var warriorAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
                if (warriorAsset != null)
                {
                    var so = new SerializedObject(playableChar);
                    so.FindProperty("characterDefinition").objectReferenceValue = warriorAsset;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(rootGo, WarriorPrefabPath);
                Debug.Log($"[Milestone 8.1] Updated Warrior.prefab with PlayableCharacter and Character_Warrior definition.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(rootGo);
            }

            // Re-bind asset prefab reference if needed
            var reloadedAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
            var reloadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefabPath);
            if (reloadedAsset != null && reloadedPrefab != null && reloadedAsset.CharacterPrefab == null)
            {
                var soAsset = new SerializedObject(reloadedAsset);
                soAsset.FindProperty("characterPrefab").objectReferenceValue = reloadedPrefab;
                soAsset.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(reloadedAsset);
            }
        }

        public static void UpdatePrototypeScene(bool includeVerifier = false)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Find player in scene
            GameObject warriorGo = GameObject.FindWithTag("Player");
            if (warriorGo == null)
            {
                warriorGo = GameObject.Find("Player");
            }
            if (warriorGo == null)
            {
                warriorGo = GameObject.Find("Warrior");
            }

            if (warriorGo == null)
            {
                Debug.LogError("[Milestone 8.1] No Warrior/Player found in Dungeon_Prototype scene!");
                return;
            }

            warriorGo.name = "Warrior";
            warriorGo.tag = "Player";

            var playableChar = warriorGo.GetComponent<PlayableCharacter>();
            if (playableChar == null)
            {
                playableChar = warriorGo.AddComponent<PlayableCharacter>();
            }

            var warriorAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
            if (warriorAsset != null)
            {
                var soPlayable = new SerializedObject(playableChar);
                soPlayable.FindProperty("characterDefinition").objectReferenceValue = warriorAsset;
                soPlayable.ApplyModifiedPropertiesWithoutUndo();
            }

            var playerExperience = warriorGo.GetComponent<PlayerExperience>();
            var playerStats = warriorGo.GetComponent<PlayerStats>();

            // Reconnect scene dependencies explicitly
            var waveManager = Object.FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                var so = new SerializedObject(waveManager);
                so.FindProperty("playerTarget").objectReferenceValue = warriorGo.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(waveManager);
            }

            var upgradeManager = Object.FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                var so = new SerializedObject(upgradeManager);
                if (playerExperience != null) so.FindProperty("playerExperience").objectReferenceValue = playerExperience;
                if (playerStats != null) so.FindProperty("playerStats").objectReferenceValue = playerStats;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(upgradeManager);
            }

            var xpUI = Object.FindFirstObjectByType<PlayerExperienceUI>();
            if (xpUI != null)
            {
                var so = new SerializedObject(xpUI);
                if (playerExperience != null) so.FindProperty("playerExperience").objectReferenceValue = playerExperience;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(xpUI);
            }

            var completionController = Object.FindFirstObjectByType<DungeonCompletionController>();
            if (completionController != null)
            {
                var so = new SerializedObject(completionController);
                if (playerExperience != null) so.FindProperty("playerExperience").objectReferenceValue = playerExperience;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(completionController);
            }

            var runStats = Object.FindFirstObjectByType<DungeonRunStats>();
            if (runStats != null)
            {
                var so = new SerializedObject(runStats);
                if (playerExperience != null) so.FindProperty("playerExperience").objectReferenceValue = playerExperience;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(runStats);
            }

            // Cleanup or attach verifier
            CleanupVerifierFromScene();

            if (includeVerifier)
            {
                var verifierGo = new GameObject("Milestone8_1_RuntimeVerifier");
                verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone8_1_Verifier>();
                Debug.Log("[Milestone 8.1] Attached Milestone8_1_RuntimeVerifier to scene.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Milestone 8.1] Configured Dungeon_Prototype scene with Warrior instance and clean references. includeVerifier={includeVerifier}");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 8.1 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 8.1] Launching PlayMode verification harness...");
            UpdatePrototypeScene(includeVerifier: true);
            EditorApplication.EnterPlaymode();
        }

        public static void CleanupVerifierFromScene()
        {
            var oldVerifier = GameObject.Find("Milestone8_1_RuntimeVerifier");
            if (oldVerifier != null)
            {
                Object.DestroyImmediate(oldVerifier);
            }

            var allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var c in allComponents)
            {
                if (c != null && c.GetType().Name.Contains("Verifier"))
                {
                    Debug.Log($"[Milestone 8.1] Removing verifier component from scene: {c.GetType().Name}");
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
