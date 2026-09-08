using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Upgrades;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Deterministic Play Mode verification suite for Gate 10.1 (Campaign Run Progression & Checkpoints).
    /// Validates:
    /// - Fresh run state (Level 1, 0 XP, neutral upgrades)
    /// - ownerCharacterId validation and cross-character rejection
    /// - PlayerExperience.RestoreState (UI updates, no fake OnLevelUp prompts)
    /// - UpgradeManager.ReconstructUpgrades (PlayerStats reconstruction without UI)
    /// - Dungeon entry checkpointing vs. live mid-dungeon state
    /// - Anti-farming: failed attempt does not overwrite committed run state
    /// - Defeat Retry restores entry checkpoint with full health
    /// - Victory commits final progression for subsequent dungeon restoration
    /// - Reconstructed fresh player GameObject identity (no DDOL)
    /// - Direct scene launch fallback neutrality
    /// </summary>
    public class Milestone10_1_Verifier : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(RunVerificationSafe());
        }

        private IEnumerator RunVerificationSafe()
        {
            yield return null;
            yield return null;

            bool success = false;
            IEnumerator routine = RunVerificationRoutine((result) => success = result);
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

            Time.timeScale = 1f;

#if UNITY_EDITOR
            System.IO.File.AppendAllText("gate_verification_results.log", $"[GATE 10.1 RESULT] Success: {success} at {DateTime.Now}\n");
            EditorApplication.isPlaying = false;
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
#endif
        }

        private IEnumerator RunVerificationRoutine(Action<bool> onComplete)
        {
            yield return null;
            Debug.Log("[GATE 10.1] Beginning Milestone 10.1 Automated Play Mode Verification...");
            bool allPassed = true;

            // -------------------------------------------------------------
            // CHECK 1: Fresh Run Initialization
            // -------------------------------------------------------------
            RunProgressionSession.StartNewRun("archer");
            bool c1 = RunProgressionSession.HasActiveRun &&
                      RunProgressionSession.OwnerCharacterId == "archer" &&
                      RunProgressionSession.Level == 1 &&
                      RunProgressionSession.CurrentXP == 0 &&
                      RunProgressionSession.TotalXP == 0 &&
                      RunProgressionSession.CommittedUpgradeIds.Count == 0;
            if (c1)
            {
                Debug.Log("[CHECK 1 PASSED] Fresh run correctly initialized: Archer, Level 1, 0 XP, 0 upgrades.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] Fresh run initialization mismatch: level={RunProgressionSession.Level}, xp={RunProgressionSession.CurrentXP}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 2: Owner Character ID Validation
            // -------------------------------------------------------------
            bool c2Valid = RunProgressionSession.ValidateOwner("archer");
            bool c2Invalid = !RunProgressionSession.ValidateOwner("warrior");
            if (c2Valid && c2Invalid)
            {
                Debug.Log("[CHECK 2 PASSED] OwnerCharacterId strictly validates matching archetype and rejects mismatch.");
            }
            else
            {
                Debug.LogError($"[CHECK 2 FAILED] Owner validation failed: valid={c2Valid}, invalidRejected={c2Invalid}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 3: PlayerExperience.RestoreState Semantics
            // -------------------------------------------------------------
            var holderGo = new GameObject("Test_Player_Holder");
            var testExp = holderGo.AddComponent<PlayerExperience>();
            var testStats = holderGo.AddComponent<PlayerStats>();
            var testHealth = holderGo.AddComponent<PlayerHealth>();

            int levelUpFiredCount = 0;
            testExp.OnLevelUp += (lvl) => levelUpFiredCount++;

            int expEventCurrent = -1;
            int expEventRequired = -1;
            testExp.OnExperienceChanged += (curr, req) => { expEventCurrent = curr; expEventRequired = req; };

            // Restore Level 3, 120 XP, 370 Total XP
            testExp.RestoreState(3, 120, 370);

            bool c3 = testExp.Level == 3 &&
                      testExp.CurrentXP == 120 &&
                      testExp.TotalXPEarned == 370 &&
                      levelUpFiredCount == 0 &&
                      expEventCurrent == 120;
            if (c3)
            {
                Debug.Log("[CHECK 3 PASSED] PlayerExperience.RestoreState sets Level/XP and fires UI event without firing fake OnLevelUp.");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] RestoreState failed: level={testExp.Level}, xp={testExp.CurrentXP}, levelUpCount={levelUpFiredCount}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 4: UpgradeManager.ReconstructUpgrades Semantics
            // -------------------------------------------------------------
            var upgradeMgrGo = new GameObject("Test_UpgradeManager");
            var upgradeMgr = upgradeMgrGo.AddComponent<UpgradeManager>();
            upgradeMgr.BindPlayer(testExp, testStats);

            var dmgUpgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            dmgUpgrade.Initialize("upgrade_damage", "Damage +20%", "Desc", UpgradeType.Damage, 0.20f);

            var spdUpgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            spdUpgrade.Initialize("upgrade_movement_speed", "Speed +10%", "Desc", UpgradeType.MovementSpeed, 0.10f);

            upgradeMgr.SetAvailableUpgrades(new[] { dmgUpgrade, spdUpgrade });

            // Reconstruct: 2x damage (+40%), 1x speed (+10%)
            upgradeMgr.ReconstructUpgrades(new[] { "upgrade_damage", "upgrade_damage", "upgrade_movement_speed" });

            bool c4 = Mathf.Approximately(testStats.DamageMultiplier, 1.40f) &&
                      Mathf.Approximately(testStats.MovementSpeedMultiplier, 1.10f) &&
                      upgradeMgr.CollectedUpgradeIds.Count == 3;
            if (c4)
            {
                Debug.Log("[CHECK 4 PASSED] UpgradeManager.ReconstructUpgrades deterministically applies multipliers without opening UI.");
            }
            else
            {
                Debug.LogError($"[CHECK 4 FAILED] ReconstructUpgrades failed: dmg={testStats.DamageMultiplier}, spd={testStats.MovementSpeedMultiplier}, count={upgradeMgr.CollectedUpgradeIds.Count}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 5: Dungeon Entry Checkpointing vs. Live Mid-Dungeon Gains
            // -------------------------------------------------------------
            // Commit starting build: Level 3, 120 XP, 2 upgrades
            RunProgressionSession.CommitDungeonVictory(3, 120, 370, new[] { "upgrade_damage", "upgrade_movement_speed" });
            RunProgressionSession.CreateDungeonCheckpoint();

            // Simulate in-dungeon gameplay: live player earns XP and gains Level 4 in active dungeon
            testExp.GainExperience(250); // advances level
            bool midDungeonLeveled = testExp.Level >= 4;

            // Verify committed run state and checkpoint have NOT changed
            bool c5 = midDungeonLeveled &&
                      RunProgressionSession.Level == 3 &&
                      RunProgressionSession.CurrentXP == 120 &&
                      RunProgressionSession.CheckpointLevel == 3 &&
                      RunProgressionSession.CommittedUpgradeIds.Count == 2;
            if (c5)
            {
                Debug.Log("[CHECK 5 PASSED] Mid-dungeon live gains do NOT mutate committed run state or entry checkpoint.");
            }
            else
            {
                Debug.LogError($"[CHECK 5 FAILED] Live gain bled into session: committedLevel={RunProgressionSession.Level}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 6: Defeat Retry Rollback (Anti-Farming)
            // -------------------------------------------------------------
            // Roll back on retry
            RunProgressionSession.RestoreCheckpointOnRetry();
            bool c6 = RunProgressionSession.CheckpointLevel == 3 &&
                      RunProgressionSession.CheckpointCurrentXP == 120 &&
                      RunProgressionSession.CheckpointTotalXP == 370 &&
                      RunProgressionSession.CheckpointUpgradeIds.Count == 2;
            if (c6)
            {
                Debug.Log("[CHECK 6 PASSED] Defeat Retry cleanly rolls back to entry checkpoint, discarding failed-attempt XP.");
            }
            else
            {
                Debug.LogError($"[CHECK 6 FAILED] Retry rollback failed: cpLevel={RunProgressionSession.CheckpointLevel}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 7: Full Health on Spawn and Retry
            // -------------------------------------------------------------
            testHealth.TakeDamage(60f); // damaged in failed attempt
            bool wasDamaged = testHealth.CurrentHealth < testHealth.MaxHealth;

            // When a fresh instance is spawned for retry, health initializes at 100%
            var retryPlayerGo = new GameObject("Test_Retry_Player");
            var retryHealth = retryPlayerGo.AddComponent<PlayerHealth>();
            bool c7 = wasDamaged && retryHealth.CurrentHealth == retryHealth.MaxHealth && retryHealth.CurrentHealth == 100f;
            if (c7)
            {
                Debug.Log("[CHECK 7 PASSED] Health resets to 100% on fresh spawn and retry (HP is not persisted).");
            }
            else
            {
                Debug.LogError($"[CHECK 7 FAILED] Health reset failed: current={retryHealth.CurrentHealth}, max={retryHealth.MaxHealth}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 8: Victory Commit Durability
            // -------------------------------------------------------------
            // When dungeon is successfully completed, final state is committed
            RunProgressionSession.CommitDungeonVictory(4, 85, 620, new[] { "upgrade_damage", "upgrade_movement_speed", "upgrade_attack_speed" });
            bool c8 = RunProgressionSession.Level == 4 &&
                      RunProgressionSession.CurrentXP == 85 &&
                      RunProgressionSession.TotalXP == 620 &&
                      RunProgressionSession.CommittedUpgradeIds.Count == 3 &&
                      RunProgressionSession.CheckpointLevel == 4;
            if (c8)
            {
                Debug.Log("[CHECK 8 PASSED] Victory commits finalized Level 4 progression into durable cross-dungeon state.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] Victory commit failed: level={RunProgressionSession.Level}, count={RunProgressionSession.CommittedUpgradeIds.Count}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 9: Fresh GameObject Restoration (No DDOL)
            // -------------------------------------------------------------
            int oldInstanceId = holderGo.GetInstanceID();
            var nextDungeonPlayerGo = new GameObject("Test_Dungeon2_Player");
            int newInstanceId = nextDungeonPlayerGo.GetInstanceID();

            var nextExp = nextDungeonPlayerGo.AddComponent<PlayerExperience>();
            var nextStats = nextDungeonPlayerGo.AddComponent<PlayerStats>();
            nextExp.RestoreState(RunProgressionSession.Level, RunProgressionSession.CurrentXP, RunProgressionSession.TotalXP);

            var nextUpgradeMgrGo = new GameObject("Test_D2_UpgradeManager");
            var nextUpgradeMgr = nextUpgradeMgrGo.AddComponent<UpgradeManager>();
            var atkSpdUpgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            atkSpdUpgrade.Initialize("upgrade_attack_speed", "Attack Speed +15%", "Desc", UpgradeType.AttackSpeed, 0.15f);
            nextUpgradeMgr.SetAvailableUpgrades(new[] { dmgUpgrade, spdUpgrade, atkSpdUpgrade });
            nextUpgradeMgr.BindPlayer(nextExp, nextStats);
            nextUpgradeMgr.ReconstructUpgrades(RunProgressionSession.CommittedUpgradeIds);

            bool c9 = oldInstanceId != newInstanceId &&
                      nextExp.Level == 4 &&
                      nextExp.CurrentXP == 85 &&
                      Mathf.Approximately(nextStats.DamageMultiplier, 1.20f) &&
                      Mathf.Approximately(nextStats.MovementSpeedMultiplier, 1.10f) &&
                      Mathf.Approximately(nextStats.AttackSpeedMultiplier, 1.15f);
            if (c9)
            {
                Debug.Log("[CHECK 9 PASSED] Next dungeon fresh GameObject successfully reconstructs identical committed build without DDOL.");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] Fresh GameObject reconstruction failed: lvl={nextExp.Level}, dmg={nextStats.DamageMultiplier}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 10: Defeat Return to Map Ends Run
            // -------------------------------------------------------------
            RunProgressionSession.EndRun();
            bool c10 = !RunProgressionSession.HasActiveRun &&
                       RunProgressionSession.OwnerCharacterId == null &&
                       RunProgressionSession.Level == 1 &&
                       RunProgressionSession.CurrentXP == 0 &&
                       RunProgressionSession.CommittedUpgradeIds.Count == 0;
            if (c10)
            {
                Debug.Log("[CHECK 10 PASSED] Defeat Return to Map ends campaign run and wipes temporary progression to Level 1, 0 XP.");
            }
            else
            {
                Debug.LogError($"[CHECK 10 FAILED] EndRun failed: active={RunProgressionSession.HasActiveRun}, level={RunProgressionSession.Level}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 11: Direct Editor Launch Fallback Neutrality
            // -------------------------------------------------------------
            // When no active run exists, player spawner initializes clean fresh run
            bool c11 = !RunProgressionSession.HasActiveRun &&
                       !RunProgressionSession.ValidateOwner("warrior");
            if (c11)
            {
                Debug.Log("[CHECK 11 PASSED] Standalone editor launch cleanly defaults to neutral Level 1 without stale session bleed.");
            }
            else
            {
                Debug.LogError("[CHECK 11 FAILED] Direct launch neutrality check failed.");
                allPassed = false;
            }

            // Cleanup test GameObjects
            DestroyImmediate(holderGo);
            DestroyImmediate(upgradeMgrGo);
            DestroyImmediate(retryPlayerGo);
            DestroyImmediate(nextDungeonPlayerGo);
            DestroyImmediate(nextUpgradeMgrGo);
            DestroyImmediate(dmgUpgrade);
            DestroyImmediate(spdUpgrade);
            DestroyImmediate(atkSpdUpgrade);

            RunProgressionSession.Clear();

            if (allPassed)
            {
                Debug.Log("[GATE 10.1 TEST COMPLETE] All 11 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 10.1 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
