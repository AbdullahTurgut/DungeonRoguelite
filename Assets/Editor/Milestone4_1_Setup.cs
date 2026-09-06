using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Tests;

namespace DungeonRoguelite.Editor
{
    public static class Milestone4_1_Setup
    {
        private const string WavesDir = "Assets/ScriptableObjects/Waves";
        private const string Wave1Path = "Assets/ScriptableObjects/Waves/Wave_01.asset";
        private const string Wave2Path = "Assets/ScriptableObjects/Waves/Wave_02.asset";
        private const string Wave3Path = "Assets/ScriptableObjects/Waves/Wave_03.asset";
        private const string ZombiePrefabPath = "Assets/Prefabs/Enemies/Zombie.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 4.1")]
        public static void Setup()
        {
            Debug.Log("[Milestone 4.1] Setting up WaveDefinition assets and prototype scene...");
            CreateWaveDefinitionAssets();
            UpdatePrototypeScene();
            Debug.Log("[Milestone 4.1] Setup completed successfully.");
        }

        public static void CreateWaveDefinitionAssets()
        {
            if (!Directory.Exists(WavesDir))
            {
                Directory.CreateDirectory(WavesDir);
                AssetDatabase.Refresh();
            }

            GameObject zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ZombiePrefabPath);
            if (zombiePrefab == null)
            {
                Debug.LogError($"[Milestone 4.1] Zombie prefab not found at {ZombiePrefabPath}");
                return;
            }

            CreateOrUpdateWaveAsset(Wave1Path, zombiePrefab, 10, 0.5f);
            CreateOrUpdateWaveAsset(Wave2Path, zombiePrefab, 15, 0.5f);
            CreateOrUpdateWaveAsset(Wave3Path, zombiePrefab, 20, 0.5f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateOrUpdateWaveAsset(string path, GameObject prefab, int count, float interval)
        {
            WaveDefinition wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
            bool isNew = false;
            if (wave == null)
            {
                wave = ScriptableObject.CreateInstance<WaveDefinition>();
                isNew = true;
            }

            var entries = new EnemySpawnEntry[]
            {
                new EnemySpawnEntry(prefab, count)
            };
            wave.Initialize(entries, interval);
            EditorUtility.SetDirty(wave);

            if (isNew)
            {
                AssetDatabase.CreateAsset(wave, path);
            }

            Debug.Log($"[Milestone 4.1] Configured {path} with {count} enemies at {interval}s interval.");
        }

        public static void UpdatePrototypeScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Clean up previous milestone verifiers
            string[] oldVerifiers = new string[]
            {
                "Milestone1_1_RuntimeVerifier",
                "Milestone1_2_RuntimeVerifier",
                "Milestone1_3_RuntimeVerifier",
                "Milestone2_1_RuntimeVerifier",
                "Milestone2_2_RuntimeVerifier",
                "Milestone3_1_RuntimeVerifier"
            };

            foreach (var verifierName in oldVerifiers)
            {
                var oldGo = GameObject.Find(verifierName);
                if (oldGo != null)
                {
                    Object.DestroyImmediate(oldGo);
                }
            }

            // Ensure Player target exists
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null)
            {
                Debug.LogError("[Milestone 4.1] Player GameObject with tag 'Player' not found in scene.");
            }

            // Setup SpawnPoints hierarchy
            var spawnPointsRoot = GameObject.Find("SpawnPoints");
            if (spawnPointsRoot == null)
            {
                spawnPointsRoot = new GameObject("SpawnPoints");
                spawnPointsRoot.transform.position = Vector3.zero;
            }

            Vector3[] spawnCoords = new Vector3[]
            {
                new Vector3(-10f, 0f, 10f),  // NW
                new Vector3(10f, 0f, 10f),   // NE
                new Vector3(10f, 0f, -10f),  // SE
                new Vector3(-10f, 0f, -10f)  // SW
            };

            string[] spawnNames = new string[]
            {
                "SpawnPoint_01",
                "SpawnPoint_02",
                "SpawnPoint_03",
                "SpawnPoint_04"
            };

            Transform[] spawnPointTransforms = new Transform[spawnCoords.Length];

            for (int i = 0; i < spawnCoords.Length; i++)
            {
                var spTransform = spawnPointsRoot.transform.Find(spawnNames[i]);
                GameObject spGo;
                if (spTransform == null)
                {
                    spGo = new GameObject(spawnNames[i]);
                    spGo.transform.SetParent(spawnPointsRoot.transform, false);
                }
                else
                {
                    spGo = spTransform.gameObject;
                }

                spGo.transform.position = spawnCoords[i];
                spGo.transform.rotation = Quaternion.identity;
                spawnPointTransforms[i] = spGo.transform;
            }

            // Setup WaveManager GameObject
            var waveManagerGo = GameObject.Find("WaveManager");
            if (waveManagerGo == null)
            {
                waveManagerGo = new GameObject("WaveManager");
            }

            var waveManager = waveManagerGo.GetComponent<WaveManager>();
            if (waveManager == null)
            {
                waveManager = waveManagerGo.AddComponent<WaveManager>();
            }

            // Load wave assets
            WaveDefinition wave1 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave1Path);
            WaveDefinition wave2 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave2Path);
            WaveDefinition wave3 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave3Path);

            var sWaveManager = new SerializedObject(waveManager);
            
            // Set waves array
            var wavesProp = sWaveManager.FindProperty("waves");
            wavesProp.arraySize = 3;
            wavesProp.GetArrayElementAtIndex(0).objectReferenceValue = wave1;
            wavesProp.GetArrayElementAtIndex(1).objectReferenceValue = wave2;
            wavesProp.GetArrayElementAtIndex(2).objectReferenceValue = wave3;

            // Set spawn points array
            var spProp = sWaveManager.FindProperty("spawnPoints");
            spProp.arraySize = spawnPointTransforms.Length;
            for (int i = 0; i < spawnPointTransforms.Length; i++)
            {
                spProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPointTransforms[i];
            }

            // Set player target
            if (playerGo != null)
            {
                sWaveManager.FindProperty("playerTarget").objectReferenceValue = playerGo.transform;
            }

            sWaveManager.FindProperty("autoStart").boolValue = true;
            sWaveManager.FindProperty("waveTransitionDelay").floatValue = 0.5f;
            sWaveManager.ApplyModifiedProperties();

            // Setup Milestone 4.1 Verifier
            var verifierGo = GameObject.Find("Milestone4_1_RuntimeVerifier");
            if (verifierGo == null)
            {
                verifierGo = new GameObject("Milestone4_1_RuntimeVerifier");
            }

            if (verifierGo.GetComponent<Milestone4_1_Verifier>() == null)
            {
                verifierGo.AddComponent<Milestone4_1_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 4.1] Scene {ScenePath} configured with SpawnPoints, WaveManager, and Milestone4_1_Verifier.");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 4.1 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 4.1] Running PlayMode verification...");
            CreateWaveDefinitionAssets();
            UpdatePrototypeScene();
            EditorApplication.EnterPlaymode();
        }
    }
}
