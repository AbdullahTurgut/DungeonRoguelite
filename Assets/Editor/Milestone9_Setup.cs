using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Editor
{
    /// <summary>
    /// Automated setup and gate runner for Phase 9 (Dungeon Progression & Campaign Flow).
    /// Orchestrates setup and deterministic verification execution across Milestones 9.1 to 9.5.
    /// </summary>
    public static class Milestone9_Setup
    {
        public const string DungeonFolder = "Assets/ScriptableObjects/Dungeons";
        public const string Dungeon01Path = "Assets/ScriptableObjects/Dungeons/Dungeon_01.asset";
        public const string Dungeon02Path = "Assets/ScriptableObjects/Dungeons/Dungeon_02.asset";
        public const string DungeonCatalogPath = "Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset";

        public const string DungeonPrototypeScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
        public const string CharacterSelectionScenePath = "Assets/Scenes/CharacterSelect/CharacterSelection.unity";
        public const string WorldMapScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";
        public const string Dungeon02ScenePath = "Assets/Scenes/Dungeons/Dungeon_02.unity";

        public const string Wave01Path = "Assets/ScriptableObjects/Waves/Wave_01.asset";
        public const string Wave02Path = "Assets/ScriptableObjects/Waves/Wave_02.asset";
        public const string Wave03Path = "Assets/ScriptableObjects/Waves/Wave_03.asset";

        [MenuItem("DungeonRoguelite/Phase 9/Setup Gate 9.1 Assets")]
        public static void SetupGate9_1()
        {
            Debug.Log("[Milestone 9.1 Setup] Creating/verifying Dungeon definitions...");
            EnsureDirectories();
            CreateOrUpdateDungeonDefinitions();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Milestone 9.1 Setup] Setup completed successfully.");
        }

        public static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }

            if (!AssetDatabase.IsValidFolder(DungeonFolder))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Dungeons");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scenes/WorldMap"))
            {
                AssetDatabase.CreateFolder("Assets/Scenes", "WorldMap");
            }
        }

        public static void CreateOrUpdateDungeonDefinitions()
        {
            var wave1 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave01Path);
            var wave2 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave02Path);
            var wave3 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave03Path);

            var wavesD1 = new WaveDefinition[] { wave1, wave2, wave3 };

            // Dungeon 1
            var d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon01Path);
            if (d1 == null)
            {
                d1 = ScriptableObject.CreateInstance<DungeonDefinition>();
                d1.SetConfiguration(
                    "dungeon_1",
                    "Bölüm 1: Unutulmuş Mezarlık",
                    "Zombilerin kol gezdiği kadim mezarlık. 3 dalga hayatta kal.",
                    "Dungeon_Prototype",
                    "",
                    wavesD1
                );
                AssetDatabase.CreateAsset(d1, Dungeon01Path);
                Debug.Log($"[Milestone 9.1 Setup] Created {Dungeon01Path}");
            }
            else
            {
                d1.SetConfiguration(
                    "dungeon_1",
                    "Bölüm 1: Unutulmuş Mezarlık",
                    "Zombilerin kol gezdiği kadim mezarlık. 3 dalga hayatta kal.",
                    "Dungeon_Prototype",
                    "",
                    wavesD1
                );
                EditorUtility.SetDirty(d1);
            }

            // Dungeon 2
            var d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon02Path);
            if (d2 == null)
            {
                d2 = ScriptableObject.CreateInstance<DungeonDefinition>();
                d2.SetConfiguration(
                    "dungeon_2",
                    "Bölüm 2: Karanlık Mahzen",
                    "Daha dar ve yoğun düşman akınları. 4 dalga hayatta kal.",
                    "Dungeon_02",
                    "dungeon_1",
                    wavesD1
                );
                AssetDatabase.CreateAsset(d2, Dungeon02Path);
                Debug.Log($"[Milestone 9.1 Setup] Created {Dungeon02Path}");
            }
            else
            {
                d2.SetConfiguration(
                    "dungeon_2",
                    "Bölüm 2: Karanlık Mahzen",
                    "Daha dar ve yoğun düşman akınları. 4 dalga hayatta kal.",
                    "Dungeon_02",
                    "dungeon_1",
                    wavesD1
                );
                EditorUtility.SetDirty(d2);
            }
        }

        [MenuItem("DungeonRoguelite/Phase 9/Run Gate 9.1 Verification")]
        public static void RunGate9_1Verification()
        {
            Debug.Log("[GATE 9.1] Running setup before verification...");
            SetupGate9_1();

            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);

            // Clean any existing verifiers
            var existingVerifiers = UnityEngine.Object.FindObjectsByType<DungeonRoguelite.Tests.Milestone9_1_Verifier>(FindObjectsSortMode.None);
            foreach (var v in existingVerifiers)
            {
                UnityEngine.Object.DestroyImmediate(v.gameObject);
            }

            var verifierGo = new GameObject("Gate9_1_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone9_1_Verifier>();

            Debug.Log("[GATE 9.1] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }
    }
}
