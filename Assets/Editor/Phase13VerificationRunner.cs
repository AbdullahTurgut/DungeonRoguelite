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
            bool success = false;
            try
            {
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

        private static void Check(bool value, string name)
        {
            if (!value) throw new InvalidOperationException("[CHECK FAILED] " + name);
            Debug.Log("[CHECK PASSED] " + name);
        }
    }
}
