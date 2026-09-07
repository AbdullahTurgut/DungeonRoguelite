using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated Play Mode verification suite for Milestone 10.4: Playable Dungeon 3 & Multi-Dungeon Continuity.
    /// Verifies:
    /// 1. Dungeon 3 asset properties (scaling, prerequisite, 4 waves, 50 enemies).
    /// 2. Dungeon catalog integrity (exactly 3 dungeons: D1, D2, D3; no D4/D5).
    /// 3. Build Settings scene list (5 scenes in exact order).
    /// 4. Dungeon 3 enemy difficulty scaling (60 HP, 11 Dmg).
    /// 5. Full 3-dungeon campaign run continuity across D1 -> D2 -> D3.
    /// </summary>
    public class Milestone10_4_Verifier : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(RunVerificationSafe());
        }

        private IEnumerator RunVerificationSafe()
        {
            yield return null;
            yield return null;

            bool success = false;
            IEnumerator routine = RunVerificationRoutine(result => success = result);
            while (true)
            {
                object current = null;
                try
                {
                    if (!routine.MoveNext())
                    {
                        break;
                    }
                    current = routine.Current;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    success = false;
                    break;
                }

                yield return current;
            }

            yield return new WaitForSeconds(0.5f);

            if (success)
            {
                Debug.Log("[GATE 10.4 COMPLETE] All Milestone 10.4 tests PASSED successfully!");
            }
            else
            {
                Debug.LogError("[GATE 10.4 COMPLETE] Verification FAILED.");
            }

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(success ? 0 : 1);
#endif
        }

        private IEnumerator RunVerificationRoutine(Action<bool> onComplete)
        {
            yield return null;
            Debug.Log("[GATE 10.4] Beginning Milestone 10.4 Automated Play Mode Verification...");
            bool allPassed = true;

            // -------------------------------------------------------------
            // CHECK 1: Dungeon 3 Asset Configuration
            // -------------------------------------------------------------
            var d3 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_03.asset");
            int totalEnemiesD3 = 0;
            if (d3 != null && d3.Waves != null)
            {
                foreach (var w in d3.Waves)
                {
                    if (w != null && w.EnemyEntries != null)
                    {
                        foreach (var e in w.EnemyEntries)
                        {
                            totalEnemiesD3 += e.Count;
                        }
                    }
                }
            }

            bool c1 = d3 != null &&
                      d3.Id == "dungeon_3" &&
                      d3.SceneName == "Dungeon_03" &&
                      d3.RequiredDungeonId == "dungeon_2" &&
                      Mathf.Approximately(d3.EnemyHealthMultiplier, 1.2f) &&
                      Mathf.Approximately(d3.EnemyDamageMultiplier, 1.1f) &&
                      d3.Waves != null && d3.Waves.Length == 4 &&
                      totalEnemiesD3 == 50;

            if (c1)
            {
                Debug.Log($"[CHECK 1 PASSED] Dungeon 3 configuration verified: id='{d3.Id}', scene='{d3.SceneName}', req='{d3.RequiredDungeonId}', HP={d3.EnemyHealthMultiplier}x, Dmg={d3.EnemyDamageMultiplier}x, waves={d3.Waves.Length}, totalEnemies={totalEnemiesD3}.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] Dungeon 3 configuration invalid: d3={d3}, waves={d3?.Waves?.Length}, enemies={totalEnemiesD3}, HP={d3?.EnemyHealthMultiplier}, Dmg={d3?.EnemyDamageMultiplier}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 2: Dungeon Catalog Exactly 3 Dungeons (D1, D2, D3)
            // -------------------------------------------------------------
            var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>("Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset");
            bool c2 = catalog != null &&
                      catalog.Dungeons != null &&
                      catalog.Dungeons.Count == 3 &&
                      catalog.Dungeons[0] != null && catalog.Dungeons[0].Id == "dungeon_1" &&
                      catalog.Dungeons[1] != null && catalog.Dungeons[1].Id == "dungeon_2" &&
                      catalog.Dungeons[2] != null && catalog.Dungeons[2].Id == "dungeon_3";

            if (c2)
            {
                Debug.Log("[CHECK 2 PASSED] DungeonCatalog contains exactly 3 valid campaign dungeons: [dungeon_1, dungeon_2, dungeon_3]. No placeholder D4/D5.");
            }
            else
            {
                Debug.LogError($"[CHECK 2 FAILED] DungeonCatalog mismatch: length={catalog?.Dungeons?.Count}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 3: Build Settings Order (5 Scenes)
            // -------------------------------------------------------------
            var buildScenes = EditorBuildSettings.scenes;
            bool c3 = buildScenes != null && buildScenes.Length == 5 &&
                      buildScenes[0].path.EndsWith("CharacterSelection.unity") &&
                      buildScenes[1].path.EndsWith("WorldMap.unity") &&
                      buildScenes[2].path.EndsWith("Dungeon_Prototype.unity") &&
                      buildScenes[3].path.EndsWith("Dungeon_02.unity") &&
                      buildScenes[4].path.EndsWith("Dungeon_03.unity");

            if (c3)
            {
                Debug.Log("[CHECK 3 PASSED] Build Settings contains exactly 5 scenes in correct order: CharacterSelection (0), WorldMap (1), Dungeon_Prototype (2), Dungeon_02 (3), Dungeon_03 (4).");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] Build Settings scene count or order mismatch: count={buildScenes?.Length}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 4: Dungeon 3 Dynamic Enemy Difficulty Scaling (60 HP, 11 Dmg)
            // -------------------------------------------------------------
            var zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            var waveMgr = FindFirstObjectByType<WaveManager>();
            bool c4WaveMgr = waveMgr != null;

            bool c4EnemyScaled = false;
            if (c4WaveMgr && zombiePrefab != null && d3 != null)
            {
                waveMgr.ConfigureFromDungeon(d3);
                var spawnMethod = typeof(WaveManager).GetMethod("SpawnEnemy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (spawnMethod != null)
                {
                    spawnMethod.Invoke(waveMgr, new object[] { zombiePrefab });

                    foreach (var living in waveMgr.ActiveEnemies)
                    {
                        if (living != null)
                        {
                            var attack = living.GetComponent<EnemyAttack>();
                            // 50 * 1.2 = 60 HP, 10 * 1.1 = 11 Dmg
                            if (Mathf.Approximately(living.MaxHealth, 60f) &&
                                attack != null && Mathf.Approximately(attack.Damage, 11f))
                            {
                                c4EnemyScaled = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (c4EnemyScaled)
            {
                Debug.Log("[CHECK 4 PASSED] Dungeon 3 spawned enemies scaled to verified values: 60 HP (1.2x) and 11 Damage (1.1x).");
            }
            else
            {
                Debug.LogError("[CHECK 4 FAILED] Dungeon 3 enemy scaling failed (expected 60 HP, 11 Dmg).");
                allPassed = false;
            }

            // Cleanup spawned enemy
            if (waveMgr != null) waveMgr.HaltDungeon();
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (var e in enemies)
            {
                if (e != null) DestroyImmediate(e.gameObject);
            }

            // -------------------------------------------------------------
            // CHECK 5: Multi-Dungeon Continuity Simulation (D1 -> D2 -> D3)
            // -------------------------------------------------------------
            // Step A: Start Archer run
            RunProgressionSession.StartNewRun("archer");
            bool sA = RunProgressionSession.HasActiveRun && RunProgressionSession.Level == 1;

            // Step B: Win Dungeon 1 -> commits Level 2, 30 XP, 1 upgrade
            RunProgressionSession.CommitDungeonVictory(2, 30, 110, new List<string> { "upg_damage_01" });
            bool sB = RunProgressionSession.Level == 2 &&
                      RunProgressionSession.CurrentXP == 30 &&
                      RunProgressionSession.CommittedUpgradeIds.Count == 1;

            // Step C: Enter Dungeon 2 -> checkpoint captures Level 2, 30 XP, 1 upgrade
            RunProgressionSession.CreateDungeonCheckpoint();
            bool sC = RunProgressionSession.CheckpointLevel == 2 &&
                      RunProgressionSession.CheckpointCurrentXP == 30;

            // Step D: Win Dungeon 2 -> commits Level 3, 40 XP, 2 upgrades
            RunProgressionSession.CommitDungeonVictory(3, 40, 240, new List<string> { "upg_damage_01", "upg_speed_01" });
            bool sD = RunProgressionSession.Level == 3 &&
                      RunProgressionSession.CurrentXP == 40 &&
                      RunProgressionSession.CommittedUpgradeIds.Count == 2;

            // Step E: Enter Dungeon 3 -> checkpoint captures Level 3, 40 XP, 2 upgrades
            RunProgressionSession.CreateDungeonCheckpoint();

            // Step F: Spawn fresh character in Dungeon 3 and verify full reconstruction
            var playerHolder = new GameObject("D3_Player_Test");
            var xp = playerHolder.AddComponent<PlayerExperience>();
            var health = playerHolder.AddComponent<PlayerHealth>();
            var testStats = playerHolder.AddComponent<PlayerStats>();
            var upgMgrGo = new GameObject("D3_UpgradeManager");
            var upgMgr = upgMgrGo.AddComponent<UpgradeManager>();

            // Setup upgrades in UpgradeManager catalog
            var dmgUpgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            dmgUpgrade.Initialize("upg_damage_01", "Damage Boost", "Damage +15%", UpgradeType.Damage, 0.15f);
            var spdUpgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            spdUpgrade.Initialize("upg_speed_01", "Speed Boost", "Speed +10%", UpgradeType.MovementSpeed, 0.10f);
            upgMgr.BindPlayer(xp, testStats);
            upgMgr.SetAvailableUpgrades(new[] { dmgUpgrade, spdUpgrade });

            // Restore progression state onto fresh D3 instance
            xp.RestoreState(RunProgressionSession.Level, RunProgressionSession.CurrentXP, RunProgressionSession.TotalXP);
            upgMgr.ReconstructUpgrades(RunProgressionSession.CommittedUpgradeIds);

            bool sF = xp.Level == 3 &&
                      xp.CurrentXP == 40 &&
                      xp.TotalXPEarned == 240 &&
                      upgMgr.CollectedUpgradeIds.Count == 2 &&
                      Mathf.Approximately(health.CurrentHealth, health.MaxHealth); // Health is 100% full!

            // Step G: Defeat Retry rolls back to D3 checkpoint
            RunProgressionSession.RestoreCheckpointOnRetry();
            bool sG = RunProgressionSession.Level == 3 &&
                      RunProgressionSession.CurrentXP == 40 &&
                      RunProgressionSession.CommittedUpgradeIds.Count == 2;

            // Step H: Defeat Return to Map ends campaign run
            RunProgressionSession.EndRun();
            bool sH = !RunProgressionSession.HasActiveRun && RunProgressionSession.Level == 1;

            DestroyImmediate(playerHolder);
            DestroyImmediate(upgMgrGo);
            DestroyImmediate(dmgUpgrade);
            DestroyImmediate(spdUpgrade);

            bool c5 = sA && sB && sC && sD && sF && sG && sH;
            if (c5)
            {
                Debug.Log("[CHECK 5 PASSED] Full 3-dungeon campaign progression continuity across D1 -> D2 -> D3 verified successfully.");
            }
            else
            {
                Debug.LogError($"[CHECK 5 FAILED] Continuity failure: sA={sA}, sB={sB}, sC={sC}, sD={sD}, sF={sF}, sG={sG}, sH={sH}");
                allPassed = false;
            }

            if (allPassed)
            {
                Debug.Log("[GATE 10.4 TEST COMPLETE] All 5 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 10.4 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
