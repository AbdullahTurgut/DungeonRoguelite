using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Editor
{
    /// <summary>Bounded editor verification for Phase 13 asset and scene gates.</summary>
    public static class Phase13VerificationRunner
    {
        [MenuItem("DungeonRoguelite/Phase 13/Run Gate 13.1 Verification")]
        public static void Run()
        {
            string[] args = Environment.GetCommandLineArgs();
            int gateArg = Array.IndexOf(args, "-gateSuite");
            string gate = gateArg >= 0 && gateArg + 1 < args.Length ? args[gateArg + 1] : "13_1";
            bool success = false;
            try
            {
                if (gate == "13_2")
                {
                    VerifyGate13_2();
                    success = true;
                    Debug.Log("[GATE 13.2 COMPLETE] All Gate 13.2 checks PASSED.");
                    return;
                }
                if (gate == "13_3")
                {
                    VerifyGate13_3();
                    success = true;
                    Debug.Log("[GATE 13.3 COMPLETE] All Gate 13.3 checks PASSED.");
                    return;
                }
                Phase13Setup.SetupGate13_1();
                var d4 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_04.asset");
                var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>("Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset");
                bool definition = d4 != null && d4.Id == "dungeon_4" && d4.DisplayName == "Bölüm 4: Sütunlu Salon" && d4.SceneName == "Dungeon_04" && d4.RequiredDungeonId == "dungeon_3" && d4.Waves != null && d4.Waves.Length == 0 && Mathf.Approximately(d4.EnemyHealthMultiplier, 1.3f) && Mathf.Approximately(d4.EnemyDamageMultiplier, 1.2f);
                Check(definition, "D4 definition, empty foundation waves, and scaling");
                Check(catalog != null && catalog.Dungeons.Count == 4 && catalog.Dungeons.Select(d => d.Id).SequenceEqual(new[] { "dungeon_1", "dungeon_2", "dungeon_3", "dungeon_4" }), "catalog preserves D1-D3 and registers D4 fourth");
                Check(EditorBuildSettings.scenes.Length == 6 && EditorBuildSettings.scenes[5].path.EndsWith("Dungeon_04.unity"), "Build Settings D4 route");
                string source = System.IO.File.ReadAllText("Assets/Scripts/UI/WorldMapController.cs");
                Check(source.Contains("activeCount >= 4 ? 380f") && source.Contains("activeCount >= 4 ? 420f"), "World Map four-card layout dimensions");
                Check(PermanentProgression.GetDungeonFirstClearPoints("DUNGEON_4") == 3, "D4 first-clear reward is 3 points");
                string old = PlayerPrefs.GetString(DungeonProgression.PrefsKey, null);
                DungeonProgression.ResetProgression();
                Check(!DungeonProgression.IsDungeonUnlocked(d4), "D4 locked before D3 completion");
                DungeonProgression.RecordDungeonCompleted("dungeon_3");
                Check(DungeonProgression.IsDungeonUnlocked(d4) && DungeonProgression.IsDungeonUnlocked(d4), "D4 unlock and replay availability after D3");
                if (old == null) PlayerPrefs.DeleteKey(DungeonProgression.PrefsKey); else PlayerPrefs.SetString(DungeonProgression.PrefsKey, old);
                PlayerPrefs.Save(); DungeonProgression.ResetProgression(); if (old != null) { PlayerPrefs.SetString(DungeonProgression.PrefsKey, old); PlayerPrefs.Save(); }
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/Dungeons/Dungeon_04.unity", OpenSceneMode.Single);
                var manager = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
                var spawnRoot = GameObject.Find("SpawnPoints");
                Check(GameObject.Find("PlayerSpawnPoint") != null && Camera.main != null && manager != null, "player spawn, camera, and WaveManager foundation");
                var spawnReferences = new SerializedObject(manager).FindProperty("spawnPoints");
                Check(spawnRoot != null && Enumerable.Range(1, 6).All(i => spawnRoot.transform.Find("SpawnPoint_0" + i) != null) && spawnReferences.arraySize == 6, "six serialized spawn-point references");
                success = true;
                Debug.Log("[GATE 13.1 COMPLETE] All Gate 13.1 checks PASSED.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError("[GATE 13.1 COMPLETE] Verification FAILED.");
            }
            finally
            {
                Debug.Log("[PHASE 13 STATE RESTORED]");
                if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static void VerifyGate13_2()
        {
            Phase13Setup.SetupGate13_2();
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Dungeons/Dungeon_04.unity", OpenSceneMode.Single);
            var arena = GameObject.Find("ColonnadeArena");
            Check(arena != null, "Colonnade arena root");
            var floor = arena.transform.Find("Floor");
            Check(floor != null && Approximately(floor.localScale, new Vector3(36f, 0.5f, 28f)), "36m x 28m floor containment");
            Check(arena.transform.Find("NorthWall") != null && arena.transform.Find("SouthWall") != null && arena.transform.Find("EastWall") != null && arena.transform.Find("WestWall") != null, "four boundary walls");
            Vector3[] expected = { new Vector3(-5.5f, 1.75f, 4.5f), new Vector3(5.5f, 1.75f, 4.5f), new Vector3(-5.5f, 1.75f, -4.5f), new Vector3(5.5f, 1.75f, -4.5f) };
            for (int i = 0; i < expected.Length; i++) Check(arena.transform.Find("Pillar_0" + (i + 1)) != null && Approximately(arena.transform.Find("Pillar_0" + (i + 1)).position, expected[i]), "symmetric pillar " + (i + 1));
            var playerSpawn = GameObject.Find("PlayerSpawnPoint");
            Check(playerSpawn != null && Approximately(playerSpawn.transform.position, new Vector3(0f, 0f, -10.5f)) && Vector3.Dot(playerSpawn.transform.forward, Vector3.forward) > .99f, "south player spawn facing north");
            var spawnRoot = GameObject.Find("SpawnPoints");
            Check(spawnRoot != null && Enumerable.Range(1, 6).All(i => spawnRoot.transform.Find("SpawnPoint_0" + i) != null), "six tactical spawn points");
            var colliders = arena.GetComponentsInChildren<Collider>();
            Check(colliders.Length >= 9, "floor, wall, and pillar colliders for containment and projectile blocking");
        }

        private static bool Approximately(Vector3 left, Vector3 right) => Vector3.Distance(left, right) < 0.01f;

        private static void VerifyGate13_3()
        {
            Phase13Setup.SetupGate13_3();
            var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_04.asset");
            Check(dungeon != null && dungeon.Waves.Length == 4, "four production waves wired to D4");
            int enemies = 0; int xp = 0; string[] expected = { "Zombie,Ranged", "Runner,Ranged", "Tank,Ranged,Zombie", "Tank,Ranged,Zombie,Runner" };
            for (int i = 0; i < dungeon.Waves.Length; i++)
            {
                var wave = dungeon.Waves[i];
                Check(wave != null && wave.SpawnInterval > 0f && wave.EnemyEntries.Count > 0, "wave " + (i + 1) + " pacing and entries");
                string order = string.Join(",", wave.EnemyEntries.Select(e => e.EnemyPrefab.name));
                Check(order == expected[i] && wave.EnemyEntries.All(e => e.EnemyPrefab != null && e.Count > 0), "wave " + (i + 1) + " grouped entry order");
                foreach (var entry in wave.EnemyEntries) { enemies += entry.Count; xp += entry.Count * entry.EnemyPrefab.GetComponent<DungeonRoguelite.Experience.ExperienceReward>().XPAmount; }
            }
            Check(enemies == 45 && xp == 705, "exact D4 production total: 45 enemies / 705 XP");
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Dungeons/Dungeon_04.unity", OpenSceneMode.Single);
            var manager = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
            Check(manager != null && manager.TotalWaves == 4, "WaveManager final-enemy completion foundation has four waves");
        }

        private static void Check(bool value, string name)
        {
            if (!value) throw new InvalidOperationException("[CHECK FAILED] " + name);
            Debug.Log("[CHECK PASSED] " + name);
        }
    }
}
