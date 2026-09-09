using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    public class D4WaveTransitionVerifier : MonoBehaviour
    {
        private void Start() => StartCoroutine(Verify());
        private IEnumerator Verify()
        {
                var d4 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_04.asset");
                var player = new GameObject("D4WaveTransitionPlayer").transform;
                var manager = new GameObject("D4WaveTransitionManager").AddComponent<WaveManager>();
                manager.SetWaves(d4.Waves); manager.SetPlayerTarget(player); manager.SetSpawnPoints(new[] { manager.transform });
                int waveTwoStarts = 0; manager.OnWaveStarted += (wave, total) => { if (wave == 2 && total == 4) waveTwoStarts++; };
                manager.BeginDungeon();
                yield return new WaitForSeconds(5.2f);
                Check(manager.CurrentWaveIndex == 0 && manager.LivingEnemyCount == 9, "Wave 1 active with 9 living enemies");
                foreach (var enemy in manager.ActiveEnemies.ToArray()) enemy.TakeDamage(9999f);
                yield return new WaitForSeconds(6.2f);
                var spawned = manager.CurrentWaveSpawned.Where(e => e != null).ToArray();
                Check(waveTwoStarts == 1 && manager.CurrentWaveIndex == 1 && spawned.Length == 11, "Wave 1 clear advances and spawns Wave 2");
                Check(spawned.Count(e => e.gameObject.name.StartsWith("Runner")) == 7 && spawned.Count(e => e.gameObject.name.StartsWith("Ranged")) == 4, "Wave 2 composition is 7 Runner and 4 Ranged");
                Debug.Log("[D4 WAVE TRANSITION COMPLETE] PASSED: 3 checks passed.");
                Debug.Log("[D4 WAVE TRANSITION STATE RESTORED]");
                if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        private static void Check(bool ok, string name) { if (!ok) throw new InvalidOperationException(name); Debug.Log("[CHECK PASSED] " + name); }
    }
}
