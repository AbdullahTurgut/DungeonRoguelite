using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DungeonRoguelite.Player;
using DungeonRoguelite.Tests;

namespace DungeonRoguelite.Editor
{
    public static class Milestone1_3_Setup
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/Warrior.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 1.3")]
        public static void Setup()
        {
            Debug.Log("[Milestone 1.3] Setting up PlayerHealth and verification harness...");
            UpdatePlayerPrefab();
            UpdatePrototypeScene();
            Debug.Log("[Milestone 1.3] Setup completed successfully.");
        }

        public static void UpdatePlayerPrefab()
        {
            var rootGo = PrefabUtility.LoadPrefabContents(PrefabPath);

            var health = rootGo.GetComponent<PlayerHealth>();
            if (health == null)
            {
                health = rootGo.AddComponent<PlayerHealth>();
            }

            var serializedHealth = new SerializedObject(health);
            serializedHealth.FindProperty("maxHealth").floatValue = 100f;
            serializedHealth.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
            PrefabUtility.UnloadPrefabContents(rootGo);

            Debug.Log($"[Milestone 1.3] Updated {PrefabPath} with PlayerHealth (maxHealth = 100).");
        }

        public static void UpdatePrototypeScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Clean up previous milestone verifiers if present
            var oldVerifierGo1 = GameObject.Find("Milestone1_1_RuntimeVerifier");
            if (oldVerifierGo1 != null)
            {
                Object.DestroyImmediate(oldVerifierGo1);
            }
            var oldVerifierGo2 = GameObject.Find("Milestone1_2_RuntimeVerifier");
            if (oldVerifierGo2 != null)
            {
                Object.DestroyImmediate(oldVerifierGo2);
            }

            // Ensure Player in scene has PlayerHealth component (from prefab or added)
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null && playerGo.GetComponent<PlayerHealth>() == null)
            {
                playerGo.AddComponent<PlayerHealth>();
            }

            // Setup Milestone 1.3 verifier
            var verifierGo = GameObject.Find("Milestone1_3_RuntimeVerifier");
            if (verifierGo == null)
            {
                verifierGo = new GameObject("Milestone1_3_RuntimeVerifier");
            }
            if (verifierGo.GetComponent<Milestone1_3_Verifier>() == null)
            {
                verifierGo.AddComponent<Milestone1_3_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 1.3] Configured {ScenePath} with Milestone1_3_Verifier.");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 1.3 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 1.3] Running PlayMode verification...");
            UpdatePlayerPrefab();
            UpdatePrototypeScene();
            EditorApplication.EnterPlaymode();
        }
    }
}
