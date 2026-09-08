using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Waves;

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

        public const string Dungeon01Path = "Assets/ScriptableObjects/Dungeons/Dungeon_01.asset";
        public const string Dungeon02Path = "Assets/ScriptableObjects/Dungeons/Dungeon_02.asset";
        public const string Dungeon03Path = "Assets/ScriptableObjects/Dungeons/Dungeon_03.asset";
        public const string DungeonCatalogPath = "Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset";

        public const string Wave03_01Path = "Assets/ScriptableObjects/Waves/Wave_03_01.asset";
        public const string Wave03_02Path = "Assets/ScriptableObjects/Waves/Wave_03_02.asset";
        public const string Wave03_03Path = "Assets/ScriptableObjects/Waves/Wave_03_03.asset";
        public const string Wave03_04Path = "Assets/ScriptableObjects/Waves/Wave_03_04.asset";
        public const string ZombiePrefabPath = "Assets/Prefabs/Enemies/Zombie.prefab";

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

        [MenuItem("DungeonRoguelite/Phase 10/Run Gate 10.3 Verification")]
        public static void RunGate10_3Verification()
        {
            Debug.Log("[GATE 10.3] Preparing Gate 10.3 Play Mode verification...");

            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate10_3_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone10_3_Verifier>();

            Debug.Log("[GATE 10.3] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        private static WaveDefinition CreateOrUpdateWave(string path, GameObject enemyPrefab, int count, float interval)
        {
            var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
            var entries = new EnemySpawnEntry[] { new EnemySpawnEntry(enemyPrefab, count) };
            if (wave == null)
            {
                wave = ScriptableObject.CreateInstance<WaveDefinition>();
                wave.Initialize(entries, interval);
                AssetDatabase.CreateAsset(wave, path);
            }
            else
            {
                wave.Initialize(entries, interval);
                EditorUtility.SetDirty(wave);
            }
            return wave;
        }

        [MenuItem("DungeonRoguelite/Phase 10/Setup Gate 10.4 Assets and Scene")]
        public static void SetupGate10_4AssetsAndScene()
        {
            Debug.Log("[GATE 10.4] Setting up Dungeon 3 assets, catalog, scene, and build settings...");

            var zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ZombiePrefabPath);

            // 1. Create or update 4 waves for Dungeon 3
            var w1 = CreateOrUpdateWave(Wave03_01Path, zombiePrefab, 10, 0.6f);
            var w2 = CreateOrUpdateWave(Wave03_02Path, zombiePrefab, 12, 0.5f);
            var w3 = CreateOrUpdateWave(Wave03_03Path, zombiePrefab, 13, 0.5f);
            var w4 = CreateOrUpdateWave(Wave03_04Path, zombiePrefab, 15, 0.4f);
            var wavesD3 = new WaveDefinition[] { w1, w2, w3, w4 };

            // 2. Create or update Dungeon_03.asset
            var d3 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon03Path);
            if (d3 == null)
            {
                d3 = ScriptableObject.CreateInstance<DungeonDefinition>();
                d3.SetConfiguration("dungeon_3", "Bölüm 3: Mahzenin Derinlikleri", "Mahzenin en karanlık köşesi. 4 dalga hızlı zombi akını.", "Dungeon_03", "dungeon_2", wavesD3, 1.2f, 1.1f);
                AssetDatabase.CreateAsset(d3, Dungeon03Path);
            }
            else
            {
                d3.SetConfiguration("dungeon_3", "Bölüm 3: Mahzenin Derinlikleri", "Mahzenin en karanlık köşesi. 4 dalga hızlı zombi akını.", "Dungeon_03", "dungeon_2", wavesD3, 1.2f, 1.1f);
                EditorUtility.SetDirty(d3);
            }

            // 3. Update DungeonCatalog.asset with [D1, D2, D3]
            var d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon01Path);
            var d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon02Path);
            var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>(DungeonCatalogPath);
            if (catalog != null)
            {
                catalog.SetDungeons(new DungeonDefinition[] { d1, d2, d3 });
                EditorUtility.SetDirty(catalog);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            wavesD3 = new WaveDefinition[]
            {
                AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave03_01Path),
                AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave03_02Path),
                AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave03_03Path),
                AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave03_04Path)
            };

            // 4. Create Dungeon_03.unity scene by copying Dungeon_02.unity
            if (!File.Exists(Dungeon03ScenePath))
            {
                AssetDatabase.CopyAsset(Dungeon02ScenePath, Dungeon03ScenePath);
                AssetDatabase.Refresh();
            }

            var d3Scene = EditorSceneManager.OpenScene(Dungeon03ScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            // Configure WaveManager in Dungeon_03
            var waveManager = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.SetWaves(wavesD3);
                var sWave = new SerializedObject(waveManager);
                var wavesProp = sWave.FindProperty("waves");
                wavesProp.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    wavesProp.GetArrayElementAtIndex(i).objectReferenceValue = wavesD3[i];
                }
                sWave.ApplyModifiedProperties();
                EditorUtility.SetDirty(waveManager);
            }

            // Atmospheric lighting for Dungeon 3: deeper red/crimson crypt tint
            var dirLightGo = GameObject.Find("Directional Light");
            if (dirLightGo != null)
            {
                var light = dirLightGo.GetComponent<Light>();
                if (light != null)
                {
                    light.color = new Color(0.75f, 0.60f, 0.65f, 1f);
                    light.intensity = 0.7f;
                    EditorUtility.SetDirty(light);
                }
            }

            EditorSceneManager.MarkSceneDirty(d3Scene);
            EditorSceneManager.SaveScene(d3Scene, Dungeon03ScenePath);

            // 5. Update Build Settings: 5 scenes in exact order
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(CharacterSelectionScenePath, true),
                new EditorBuildSettingsScene(WorldMapScenePath, true),
                new EditorBuildSettingsScene(DungeonPrototypeScenePath, true),
                new EditorBuildSettingsScene(Dungeon02ScenePath, true),
                new EditorBuildSettingsScene(Dungeon03ScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GATE 10.4] Gate 10.4 setup completed successfully.");
        }

        [MenuItem("DungeonRoguelite/Phase 10/Run Gate 10.4 Verification")]
        public static void RunGate10_4Verification()
        {
            Debug.Log("[GATE 10.4] Running Gate 10.4 setup before verification...");
            SetupGate10_4AssetsAndScene();

            var scene = EditorSceneManager.OpenScene(Dungeon03ScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate10_4_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone10_4_Verifier>();

            Debug.Log("[GATE 10.4] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Phase 10/Run Gate 10.5 Verification")]
        public static void RunGate10_5Verification()
        {
            Debug.Log("[GATE 10.5] Preparing Gate 10.5 Play Mode verification...");

            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate10_5_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone10_5_Verifier>();

            Debug.Log("[GATE 10.5] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Phase 10/Setup WorldMap 3 Cards")]
        public static void SetupWorldMapCards()
        {
            Debug.Log("[Milestone 10] Setting up 3 cards in WorldMap scene...");
            Milestone9_Setup.SetupGate9_4();

            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(CharacterSelectionScenePath, true),
                new EditorBuildSettingsScene(WorldMapScenePath, true),
                new EditorBuildSettingsScene(DungeonPrototypeScenePath, true),
                new EditorBuildSettingsScene(Dungeon02ScenePath, true),
                new EditorBuildSettingsScene(Dungeon03ScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Milestone 10] WorldMap 3 cards setup complete.");
        }

        [MenuItem("DungeonRoguelite/Phase 10/Run Manual QA Verification")]
        public static void RunManualQAVerification()
        {
            Debug.Log("[MANUAL QA FIX VERIFICATION] Preparing test suite...");
            SetupWorldMapCards();

            var scene = EditorSceneManager.OpenScene(WorldMapScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Milestone10_QA_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone10_QA_Verifier>();

            Debug.Log("[MANUAL QA FIX VERIFICATION] Entering Play Mode...");
            EditorApplication.EnterPlaymode();
        }
    }
}
