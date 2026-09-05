using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Tests;

namespace DungeonRoguelite.Editor
{
    public static class Milestone3_1_Setup
    {
        private const string PrefabDir = "Assets/Prefabs/Enemies";
        private const string PrefabPath = "Assets/Prefabs/Enemies/Zombie.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 3.1")]
        public static void Setup()
        {
            Debug.Log("[Milestone 3.1] Setting up Zombie prefab and verification harness...");
            CreateOrUpdateZombiePrefab();
            UpdatePrototypeScene();
            Debug.Log("[Milestone 3.1] Setup completed successfully.");
        }

        public static void CreateOrUpdateZombiePrefab()
        {
            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
                AssetDatabase.Refresh();
            }

            bool wasLoadedFromPrefab = File.Exists(PrefabPath);
            GameObject rootGo = wasLoadedFromPrefab ? PrefabUtility.LoadPrefabContents(PrefabPath) : new GameObject("Zombie");

            // Configure CharacterController
            var cc = rootGo.GetComponent<CharacterController>();
            if (cc == null) cc = rootGo.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 2.0f;
            cc.radius = 0.5f;

            // Configure EnemyHealth
            var health = rootGo.GetComponent<EnemyHealth>();
            if (health == null) health = rootGo.AddComponent<EnemyHealth>();
            var sHealth = new SerializedObject(health);
            sHealth.FindProperty("maxHealth").floatValue = 50f;
            sHealth.ApplyModifiedProperties();

            // Configure EnemyMovement
            var movement = rootGo.GetComponent<EnemyMovement>();
            if (movement == null) movement = rootGo.AddComponent<EnemyMovement>();
            var sMovement = new SerializedObject(movement);
            sMovement.FindProperty("moveSpeed").floatValue = 3f;
            sMovement.FindProperty("stoppingDistance").floatValue = 1.3f;
            sMovement.FindProperty("gravity").floatValue = 20f;
            sMovement.ApplyModifiedProperties();

            // Configure EnemyAttack
            var attack = rootGo.GetComponent<EnemyAttack>();
            if (attack == null) attack = rootGo.AddComponent<EnemyAttack>();
            var sAttack = new SerializedObject(attack);
            sAttack.FindProperty("damage").floatValue = 10f;
            sAttack.FindProperty("attackRange").floatValue = 1.5f;
            sAttack.FindProperty("attackCooldown").floatValue = 1.0f;
            sAttack.ApplyModifiedProperties();

            // Setup Visual child
            var visual = rootGo.transform.Find("Visual");
            if (visual == null)
            {
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.name = "Visual";
                capsule.transform.SetParent(rootGo.transform, false);
                capsule.transform.localPosition = new Vector3(0f, 1f, 0f);
                capsule.transform.localScale = Vector3.one;

                // Remove collider from visual child to avoid interfering with root CharacterController
                var col = capsule.GetComponent<CapsuleCollider>();
                if (col != null) Object.DestroyImmediate(col);

                visual = capsule.transform;

                // Add FacingIndicator child
                var indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                indicator.name = "FacingIndicator";
                indicator.transform.SetParent(visual, false);
                indicator.transform.localPosition = new Vector3(0f, 0.5f, 0.45f);
                indicator.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);

                var boxCol = indicator.GetComponent<BoxCollider>();
                if (boxCol != null) Object.DestroyImmediate(boxCol);
            }

            PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
            if (wasLoadedFromPrefab)
            {
                PrefabUtility.UnloadPrefabContents(rootGo);
            }
            else
            {
                Object.DestroyImmediate(rootGo);
            }

            Debug.Log($"[Milestone 3.1] Created/Updated {PrefabPath} successfully.");
        }

        public static void UpdatePrototypeScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Clean up previous milestone verifiers
            string[] previousVerifiers = new string[]
            {
                "Milestone1_1_RuntimeVerifier",
                "Milestone1_2_RuntimeVerifier",
                "Milestone1_3_RuntimeVerifier",
                "Milestone2_1_RuntimeVerifier",
                "Milestone2_2_RuntimeVerifier"
            };

            foreach (var verifierName in previousVerifiers)
            {
                var oldGo = GameObject.Find(verifierName);
                if (oldGo != null) Object.DestroyImmediate(oldGo);
            }

            // Setup Milestone 3.1 verifier
            var verifierGo = GameObject.Find("Milestone3_1_RuntimeVerifier");
            if (verifierGo == null)
            {
                verifierGo = new GameObject("Milestone3_1_RuntimeVerifier");
            }
            if (verifierGo.GetComponent<Milestone3_1_Verifier>() == null)
            {
                verifierGo.AddComponent<Milestone3_1_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 3.1] Configured {ScenePath} with Milestone3_1_Verifier.");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 3.1 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 3.1] Running PlayMode verification...");
            CreateOrUpdateZombiePrefab();
            UpdatePrototypeScene();
            EditorApplication.EnterPlaymode();
        }
    }
}
