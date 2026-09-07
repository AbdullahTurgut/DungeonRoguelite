using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DungeonRoguelite.Editor
{
    /// <summary>
    /// Automated setup and verification runner for Phase 10 (Campaign Run Progression & Scaling).
    /// </summary>
    public static class Milestone10_Setup
    {
        public const string CharacterSelectionScenePath = "Assets/Scenes/CharacterSelect/CharacterSelection.unity";
        public const string WorldMapScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";
        public const string DungeonPrototypeScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
        public const string Dungeon02ScenePath = "Assets/Scenes/Dungeons/Dungeon_02.unity";
        public const string Dungeon03ScenePath = "Assets/Scenes/Dungeons/Dungeon_03.unity";

        public static void CleanAllVerifiers()
        {
            var oldVerifiers = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in oldVerifiers)
            {
                if (mb != null && mb.GetType().Name.Contains("Verifier"))
                {
                    UnityEngine.Object.DestroyImmediate(mb.gameObject);
                }
            }
        }

        [MenuItem("DungeonRoguelite/Phase 10/Run Gate 10.1 Verification")]
        public static void RunGate10_1Verification()
        {
            Debug.Log("[GATE 10.1] Preparing Gate 10.1 Play Mode verification...");

            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate10_1_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone10_1_Verifier>();

            Debug.Log("[GATE 10.1] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Phase 10/Run Gate 10.2 Verification")]
        public static void RunGate10_2Verification()
        {
            Debug.Log("[GATE 10.2] Preparing Gate 10.2 Play Mode verification...");

            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate10_2_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone10_2_Verifier>();

            Debug.Log("[GATE 10.2] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }
    }
}
