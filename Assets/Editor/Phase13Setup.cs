using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Editor
{
    /// <summary>Idempotent authoring helpers for the Dungeon 4 campaign gates.</summary>
    public static class Phase13Setup
    {
        public const string Dungeon04ScenePath = "Assets/Scenes/Dungeons/Dungeon_04.unity";
        private const string Dungeon03ScenePath = "Assets/Scenes/Dungeons/Dungeon_03.unity";
        private const string Dungeon04Path = "Assets/ScriptableObjects/Dungeons/Dungeon_04.asset";
        private const string CatalogPath = "Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset";

        [MenuItem("DungeonRoguelite/Phase 13/Setup Gate 13.1 Foundation")]
        public static void SetupGate13_1()
        {
            if (!File.Exists(Dungeon04ScenePath))
            {
                AssetDatabase.CopyAsset(Dungeon03ScenePath, Dungeon04ScenePath);
                AssetDatabase.Refresh();
            }

            var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon04Path);
            var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>(CatalogPath);
            var d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_01.asset");
            var d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_02.asset");
            var d3 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_03.asset");
            if (dungeon == null || catalog == null || d1 == null || d2 == null || d3 == null)
            {
                Debug.LogError("[PHASE 13] Missing required dungeon foundation asset.");
                return;
            }

            dungeon.SetConfiguration("dungeon_4", "Bölüm 4: Sütunlu Salon", "Geniş sütunlar ve dar geçitlerle çevrili büyük salon. 4 dalga hayatta kal.", "Dungeon_04", "dungeon_3", new WaveDefinition[0], 1.3f, 1.2f);
            catalog.SetDungeons(new[] { d1, d2, d3, dungeon });
            EditorUtility.SetDirty(dungeon);
            EditorUtility.SetDirty(catalog);

            var scene = EditorSceneManager.OpenScene(Dungeon04ScenePath, OpenSceneMode.Single);
            var manager = Object.FindFirstObjectByType<WaveManager>();
            if (manager != null) manager.SetWaves(new WaveDefinition[0]);
            EnsureSpawnPoints(6);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene("Assets/Scenes/CharacterSelect/CharacterSelection.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/WorldMap/WorldMap.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Dungeons/Dungeon_Prototype.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Dungeons/Dungeon_02.unity", true),
                new EditorBuildSettingsScene(Dungeon03ScenePath, true),
                new EditorBuildSettingsScene(Dungeon04ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GATE 13.1 SETUP COMPLETE] Dungeon 4 foundation saved.");
        }

        [MenuItem("DungeonRoguelite/Phase 13/Setup Gate 13.2 Arena")]
        public static void SetupGate13_2()
        {
            SetupGate13_1();
            var scene = EditorSceneManager.OpenScene(Dungeon04ScenePath, OpenSceneMode.Single);
            var root = GameObject.Find("ColonnadeArena") ?? new GameObject("ColonnadeArena");
            ClearChildren(root.transform);
            CreateBlock(root.transform, "Floor", new Vector3(0f, -0.3f, 0f), new Vector3(36f, 0.5f, 28f), new Color(0.18f, 0.16f, 0.14f));
            CreateBlock(root.transform, "NorthWall", new Vector3(0f, 1.5f, 14f), new Vector3(38f, 3f, 1f), new Color(0.22f, 0.20f, 0.18f));
            CreateBlock(root.transform, "SouthWall", new Vector3(0f, 1.5f, -14f), new Vector3(38f, 3f, 1f), new Color(0.22f, 0.20f, 0.18f));
            CreateBlock(root.transform, "WestWall", new Vector3(-18f, 1.5f, 0f), new Vector3(1f, 3f, 29f), new Color(0.22f, 0.20f, 0.18f));
            CreateBlock(root.transform, "EastWall", new Vector3(18f, 1.5f, 0f), new Vector3(1f, 3f, 29f), new Color(0.22f, 0.20f, 0.18f));
            Vector3[] pillars = { new Vector3(-5.5f, 1.75f, 4.5f), new Vector3(5.5f, 1.75f, 4.5f), new Vector3(-5.5f, 1.75f, -4.5f), new Vector3(5.5f, 1.75f, -4.5f) };
            for (int i = 0; i < pillars.Length; i++) CreateBlock(root.transform, "Pillar_0" + (i + 1), pillars[i], new Vector3(2.2f, 3.5f, 2.2f), new Color(0.35f, 0.32f, 0.27f));
            var playerSpawn = GameObject.Find("PlayerSpawnPoint");
            if (playerSpawn != null) { playerSpawn.transform.position = new Vector3(0f, 0f, -10.5f); playerSpawn.transform.rotation = Quaternion.identity; }
            EnsureSpawnPoints(6);
            Vector3[] positions = { new Vector3(-14f, 0f, 10f), new Vector3(0f, 0f, 11f), new Vector3(14f, 0f, 10f), new Vector3(-15f, 0f, 0f), new Vector3(15f, 0f, 0f), new Vector3(0f, 0f, 12f) };
            var points = GameObject.Find("SpawnPoints").transform;
            for (int i = 0; i < 6; i++) points.Find("SpawnPoint_0" + (i + 1)).position = positions[i];
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[GATE 13.2 SETUP COMPLETE] Colonnade arena saved.");
        }

        [MenuItem("DungeonRoguelite/Phase 13/Setup Gate 13.3 Waves")]
        public static void SetupGate13_3()
        {
            SetupGate13_2();
            var zombie = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            var runner = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Runner.prefab");
            var tank = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Tank.prefab");
            var ranged = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Ranged.prefab");
            var waves = new[]
            {
                CreateOrUpdateWave("Assets/ScriptableObjects/Waves/Wave_04_01.asset", new[] { new EnemySpawnEntry(zombie, 6), new EnemySpawnEntry(ranged, 3) }, .55f),
                CreateOrUpdateWave("Assets/ScriptableObjects/Waves/Wave_04_02.asset", new[] { new EnemySpawnEntry(runner, 7), new EnemySpawnEntry(ranged, 4) }, .50f),
                CreateOrUpdateWave("Assets/ScriptableObjects/Waves/Wave_04_03.asset", new[] { new EnemySpawnEntry(tank, 3), new EnemySpawnEntry(ranged, 4), new EnemySpawnEntry(zombie, 3) }, .60f),
                CreateOrUpdateWave("Assets/ScriptableObjects/Waves/Wave_04_04.asset", new[] { new EnemySpawnEntry(tank, 3), new EnemySpawnEntry(ranged, 4), new EnemySpawnEntry(zombie, 4), new EnemySpawnEntry(runner, 4) }, .50f)
            };
            var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon04Path);
            dungeon.SetConfiguration("dungeon_4", "Bölüm 4: Sütunlu Salon", "Geniş sütunlar ve dar geçitlerle çevrili büyük salon. 4 dalga hayatta kal.", "Dungeon_04", "dungeon_3", waves, 1.3f, 1.2f);
            EditorUtility.SetDirty(dungeon);
            var scene = EditorSceneManager.OpenScene(Dungeon04ScenePath, OpenSceneMode.Single);
            var manager = Object.FindFirstObjectByType<WaveManager>();
            if (manager != null) { manager.SetWaves(waves); EditorUtility.SetDirty(manager); }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[GATE 13.3 SETUP COMPLETE] Dungeon 4 production waves saved.");
        }

        private static WaveDefinition CreateOrUpdateWave(string path, EnemySpawnEntry[] entries, float interval)
        {
            var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
            if (wave == null) { wave = ScriptableObject.CreateInstance<WaveDefinition>(); AssetDatabase.CreateAsset(wave, path); }
            wave.Initialize(entries, interval); EditorUtility.SetDirty(wave); return wave;
        }

        private static void EnsureSpawnPoints(int count)
        {
            var root = GameObject.Find("SpawnPoints") ?? new GameObject("SpawnPoints");
            for (int i = 0; i < count; i++)
            {
                string name = "SpawnPoint_0" + (i + 1);
                if (root.transform.Find(name) == null) new GameObject(name).transform.SetParent(root.transform, false);
            }
            var manager = Object.FindFirstObjectByType<WaveManager>();
            if (manager != null)
            {
                var result = new Transform[count];
                for (int i = 0; i < count; i++) result[i] = root.transform.Find("SpawnPoint_0" + (i + 1));
                manager.SetSpawnPoints(result);
                EditorUtility.SetDirty(manager);
            }
        }

        private static void CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial.color = color;
        }

        private static void ClearChildren(Transform root)
        {
            while (root.childCount > 0) Object.DestroyImmediate(root.GetChild(0).gameObject);
        }
    }
}
