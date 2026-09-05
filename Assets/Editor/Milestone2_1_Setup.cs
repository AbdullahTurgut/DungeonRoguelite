using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DungeonRoguelite.Tests;

namespace DungeonRoguelite.Editor
{
    public static class Milestone2_1_Setup
    {
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 2.1")]
        public static void Setup()
        {
            Debug.Log("[Milestone 2.1] Setting up IDamageable verification harness...");
            UpdatePrototypeScene();
            Debug.Log("[Milestone 2.1] Setup completed successfully.");
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
            var oldVerifierGo3 = GameObject.Find("Milestone1_3_RuntimeVerifier");
            if (oldVerifierGo3 != null)
            {
                Object.DestroyImmediate(oldVerifierGo3);
            }

            // Setup Milestone 2.1 verifier
            var verifierGo = GameObject.Find("Milestone2_1_RuntimeVerifier");
            if (verifierGo == null)
            {
                verifierGo = new GameObject("Milestone2_1_RuntimeVerifier");
            }
            if (verifierGo.GetComponent<Milestone2_1_Verifier>() == null)
            {
                verifierGo.AddComponent<Milestone2_1_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 2.1] Configured {ScenePath} with Milestone2_1_Verifier.");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 2.1 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 2.1] Running PlayMode verification...");
            UpdatePrototypeScene();
            EditorApplication.EnterPlaymode();
        }
    }
}
