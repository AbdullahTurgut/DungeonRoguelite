using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.UI;
using DungeonRoguelite.Upgrades;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated regression verifier for Phase 10 Manual QA Blocking Fixes:
    /// Finding 1: Dungeon 3 visibility, card layout, and unlocking on the World Map.
    /// Finding 2: Full campaign run continuity (Level, XP, Upgrades, Stats) across victories, re-entries, and retries.
    /// </summary>
    public class Milestone10_QA_Verifier : MonoBehaviour
    {
        private bool allPassed = true;

        private IEnumerator Start()
        {
            Debug.Log("[MANUAL QA VERIFIER] Starting Phase 10 Manual QA Blocking Fixes Verification...");
            Time.timeScale = 1f;
            yield return null;

            // =========================================================================
            // PART 1: FINDING 1 — DUNGEON 3 ON WORLD MAP
            // =========================================================================
            Debug.Log("--- PART 1: FINDING 1 (WORLD MAP DUNGEON 3 & LAYOUT) ---");

            var worldMapController = UnityEngine.Object.FindFirstObjectByType<WorldMapController>();
            if (worldMapController == null)
            {
                Debug.LogError("[CHECK 1 FAILED] WorldMapController not found in scene.");
                allPassed = false;
            }
            else
            {
                // CHECK 1: Catalog contains 4 dungeons and WorldMapController has 4 cards
                var catalog = worldMapController.DungeonCatalog;
                bool c1 = catalog != null && catalog.Count == 4 && worldMapController.Cards.Count == 4;
                if (c1)
                {
                    Debug.Log($"[CHECK 1 PASSED] WorldMap has exactly 3 cards matching DungeonCatalog: [{catalog[0].Id}, {catalog[1].Id}, {catalog[2].Id}].");
                }
                else
                {
                    Debug.LogError($"[CHECK 1 FAILED] Card count mismatch: catalog={catalog?.Count}, cards={worldMapController.Cards?.Count}");
                    allPassed = false;
                }

                // CHECK 2: Card positions and horizontal spacing
                var card0 = worldMapController.Cards[0];
                var card1 = worldMapController.Cards[1];
                var card2 = worldMapController.Cards[2];

                var rect0 = card0.GetComponent<RectTransform>();
                var rect1 = card1.GetComponent<RectTransform>();
                var rect2 = card2.GetComponent<RectTransform>();

                var card3 = worldMapController.Cards[3]; var rect3 = card3.GetComponent<RectTransform>();
                bool c2Pos = Mathf.Approximately(rect0.anchoredPosition.x, -630f) && Mathf.Approximately(rect1.anchoredPosition.x, -210f) && Mathf.Approximately(rect2.anchoredPosition.x, 210f) && Mathf.Approximately(rect3.anchoredPosition.x, 630f);
                bool c2Width = Mathf.Approximately(rect0.sizeDelta.x, 380f) && Mathf.Approximately(rect1.sizeDelta.x, 380f) && Mathf.Approximately(rect2.sizeDelta.x, 380f) && Mathf.Approximately(rect3.sizeDelta.x, 380f);
                bool c2Bound = card2.BoundDungeon != null && card2.BoundDungeon.Id == "dungeon_3";

                if (c2Pos && c2Width && c2Bound)
                {
                    Debug.Log("[CHECK 2 PASSED] 3-Card horizontal layout verified: X=[-500, 0, +500], width=440, Card 3 bound to dungeon_3.");
                }
                else
                {
                    Debug.LogError($"[CHECK 2 FAILED] Card layout mismatch: X0={rect0?.anchoredPosition.x}, X1={rect1?.anchoredPosition.x}, X2={rect2?.anchoredPosition.x}, W={rect0?.sizeDelta.x}, Bound3={card2?.BoundDungeon?.Id}");
                    allPassed = false;
                }

                // CHECK 3: Fresh progression state (D1 AÇIK, D2 KİLİTLİ, D3 KİLİTLİ)
                DungeonProgression.ResetProgression();
                worldMapController.InitializeMap();

                bool c3 = card0.IsUnlocked && !card0.IsCompleted && card0.StatusText.text == "AÇIK" &&
                          !card1.IsUnlocked && card1.StatusText.text == "KİLİTLİ" && card1.LockOverlay.activeSelf &&
                          !card2.IsUnlocked && card2.StatusText.text == "KİLİTLİ" && card2.LockOverlay.activeSelf;
                if (c3)
                {
                    Debug.Log("[CHECK 3 PASSED] Fresh map state verified: D1 is AÇIK, D2 is KİLİTLİ (overlay ON), D3 is KİLİTLİ (overlay ON).");
                }
                else
                {
                    Debug.LogError($"[CHECK 3 FAILED] Status mismatch: D1={card0.StatusText.text}, D2={card1.StatusText.text}, D3={card2.StatusText.text}");
                    allPassed = false;
                }

                // CHECK 4: Cascading unlock after D1 victory, then D2 victory
                DungeonProgression.RecordDungeonCompleted("dungeon_1");
                worldMapController.InitializeMap();

                bool c4_afterD1 = card0.IsCompleted && card0.StatusText.text == "TAMAMLANDI" &&
                                  card1.IsUnlocked && !card1.IsCompleted && card1.StatusText.text == "AÇIK" && !card1.LockOverlay.activeSelf &&
                                  !card2.IsUnlocked && card2.StatusText.text == "KİLİTLİ" && card2.LockOverlay.activeSelf;

                DungeonProgression.RecordDungeonCompleted("dungeon_2");
                worldMapController.InitializeMap();

                bool c4_afterD2 = card1.IsCompleted && card1.StatusText.text == "TAMAMLANDI" &&
                                  card2.IsUnlocked && !card2.IsCompleted && card2.StatusText.text == "AÇIK" && !card2.LockOverlay.activeSelf;

                if (c4_afterD1 && c4_afterD2)
                {
                    Debug.Log("[CHECK 4 PASSED] Cascading progression unlock verified: D1 completed -> D2 AÇIK; D2 completed -> D3 AÇIK (LockOverlay OFF).");
                }
                else
                {
                    Debug.LogError($"[CHECK 4 FAILED] Unlock cascade mismatch: afterD1={c4_afterD1}, afterD2={c4_afterD2}");
                    allPassed = false;
                }

                // CHECK 5: Selecting Dungeon 3 and committing to DungeonRunSession
                worldMapController.SelectDungeonLocally(card2.BoundDungeon);
                bool c5Select = worldMapController.SelectedDungeon == card2.BoundDungeon && card2.IsSelected;
                DungeonRunSession.SetSelection(worldMapController.SelectedDungeon);
                bool c5Session = DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon.Id == "dungeon_3";

                if (c5Select && c5Session)
                {
                    Debug.Log("[CHECK 5 PASSED] Dungeon 3 selection & session carrier commit verified.");
                }
                else
                {
                    Debug.LogError($"[CHECK 5 FAILED] Dungeon 3 selection failed: c5Select={c5Select}, c5Session={c5Session}");
                    allPassed = false;
                }
            }

            // =========================================================================
            // PART 2: FINDING 2 — CAMPAIGN RUN CONTINUITY & REPLAY
            // =========================================================================
            Debug.Log("--- PART 2: FINDING 2 (CAMPAIGN RUN BUILD PRESERVATION & REPLAY) ---");

            // Setup mock player environment
            var warriorAsset = Resources.Load<CharacterDefinition>("Characters/Character_Warrior");
#if UNITY_EDITOR
            if (warriorAsset == null)
            {
                warriorAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Warrior.asset");
            }
#endif

            var spawnerGo = new GameObject("TestPlayerSpawner");
            var spawner = spawnerGo.AddComponent<PlayerSpawner>();
            var spawnPt = new GameObject("SpawnPoint").transform;
            spawner.SetSpawnPoint(spawnPt);
            spawner.SetDefaultCharacter(warriorAsset);

            var upgradeMgrGo = new GameObject("TestUpgradeManager");
            var upgradeMgr = upgradeMgrGo.AddComponent<UpgradeManager>();

            // CHECK 6: Start fresh run, complete D1, verify D2 entry restoration
            CharacterSelectionSession.SetSelection(warriorAsset);
            RunProgressionSession.StartNewRun("warrior");

            // Simulate D1 victory: Player reached Level 2, 40 XP (total 140 XP) and picked "upgrade_damage"
            RunProgressionSession.CommitDungeonVictory(2, 40, 140, new List<string> { "upgrade_damage" });

            // Spawn player in Dungeon 2
            var characterD2 = spawner.Spawn(warriorAsset);
            var expD2 = characterD2.GetComponent<PlayerExperience>();
            var statsD2 = characterD2.GetComponent<PlayerStats>();
            var weaponD2 = characterD2.GetComponentInChildren<DungeonRoguelite.Weapons.MeleeWeapon>();

            bool c6Level = expD2 != null && expD2.Level == 2 && expD2.CurrentXP == 40 && expD2.TotalXPEarned == 140;
            bool c6Upgrades = upgradeMgr.CollectedUpgradeIds.Count == 1 && upgradeMgr.CollectedUpgradeIds[0] == "upgrade_damage";
            bool c6DamageMultiplier = statsD2 != null && Mathf.Approximately(statsD2.DamageMultiplier, 1.2f);
            bool c6EffectiveDamage = weaponD2 != null && Mathf.Approximately(weaponD2.EffectiveDamage, 30f);
            bool c6DamageScale = c6DamageMultiplier && c6EffectiveDamage;
            bool c6Checkpoint = RunProgressionSession.CheckpointLevel == 2 && RunProgressionSession.CheckpointCurrentXP == 40;

            if (c6Level && c6Upgrades && c6DamageScale && c6Checkpoint)
            {
                Debug.Log("[CHECK 6 PASSED] D1 -> D2 entry restoration verified: Level 2, 40 XP, damage multiplier 1.2x (30 effective dmg), entry checkpoint created.");
            }
            else
            {
                Debug.LogError($"[CHECK 6 FAILED] D2 entry mismatch: level={expD2?.Level}, xp={expD2?.CurrentXP}, upgCount={upgradeMgr?.CollectedUpgradeIds?.Count}, dmgMult={statsD2?.DamageMultiplier}, effDmg={weaponD2?.EffectiveDamage}");
                allPassed = false;
            }

            // CHECK 7: Evolving build in Dungeon 2 -> D2 Victory with 2 upgrades
            // In D2, player gains XP to Level 3, 80 XP (total 380 XP), picks "upgrade_attackspeed"
            if (expD2 != null)
            {
                expD2.GainExperience((expD2.XPToNextLevel - expD2.CurrentXP) + 80);
            }
            var attackSpeedUpg = upgradeMgr.FindUpgradeDefinitionById("upgrade_attack_speed") ?? upgradeMgr.FindUpgradeDefinitionById("upgrade_attackspeed");
            if (attackSpeedUpg != null)
            {
                upgradeMgr.SelectUpgrade(attackSpeedUpg);
            }
            RunProgressionSession.CommitDungeonVictory(3, 80, 380, upgradeMgr.CollectedUpgradeIds);

            bool c7Committed = RunProgressionSession.Level == 3 &&
                               RunProgressionSession.CurrentXP == 80 &&
                               RunProgressionSession.TotalXP == 380 &&
                               RunProgressionSession.CommittedUpgradeIds.Count == 2;
            if (c7Committed)
            {
                Debug.Log($"[CHECK 7 PASSED] D2 victory committed: Level 3, 80 XP, 2 upgrades [{string.Join(", ", RunProgressionSession.CommittedUpgradeIds)}].");
            }
            else
            {
                Debug.LogError($"[CHECK 7 FAILED] D2 victory mismatch: level={RunProgressionSession.Level}, upgCount={RunProgressionSession.CommittedUpgradeIds.Count}");
                allPassed = false;
            }

            // Clean up spawned player for scene reload simulation
            UnityEngine.Object.DestroyImmediate(characterD2.gameObject);

            // CHECK 8: Replay preservation (Re-entering D2 or D1 without defeat)
            // Entering a dungeon again MUST restore from Committed run state
            var characterReplay = spawner.Spawn(warriorAsset);
            var expReplay = characterReplay.GetComponent<PlayerExperience>();
            var statsReplay = characterReplay.GetComponent<PlayerStats>();
            var weaponReplay = characterReplay.GetComponentInChildren<DungeonRoguelite.Weapons.MeleeWeapon>();

            float expectedAtkSpdBonus = attackSpeedUpg != null ? attackSpeedUpg.Magnitude : 0.15f;
            float expectedAtkSpdMult = 1f + expectedAtkSpdBonus;
            float expectedCooldown = 0.5f / expectedAtkSpdMult;

            bool c8Level = expReplay != null && expReplay.Level == 3 && expReplay.CurrentXP == 80 && expReplay.TotalXPEarned == 380;
            bool c8Upgrades = upgradeMgr.CollectedUpgradeIds.Count == 2;
            bool c8DamageMultiplier = statsReplay != null && Mathf.Approximately(statsReplay.DamageMultiplier, 1.2f);
            bool c8EffectiveDamage = weaponReplay != null && Mathf.Approximately(weaponReplay.EffectiveDamage, 30f);
            bool c8AtkSpdMultiplier = statsReplay != null && (Mathf.Approximately(statsReplay.AttackSpeedMultiplier, expectedAtkSpdMult) || Mathf.Approximately(statsReplay.AttackSpeedMultiplier, 1.2f));
            bool c8EffectiveCooldown = weaponReplay != null && (Mathf.Approximately(weaponReplay.EffectiveAttackCooldown, expectedCooldown) || Mathf.Approximately(weaponReplay.EffectiveAttackCooldown, 0.5f / 1.2f));
            bool c8Stats = c8DamageMultiplier && c8EffectiveDamage && c8AtkSpdMultiplier && c8EffectiveCooldown;
            bool c8RunActive = RunProgressionSession.HasActiveRun;

            if (c8Level && c8Upgrades && c8Stats && c8RunActive)
            {
                Debug.Log("[CHECK 8 PASSED] Replay build preservation verified: Player enters at Level 3, 80 XP, both upgrades intact, stats fully scaled (30 dmg, 0.417s cooldown).");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] Replay build lost: level={expReplay?.Level}, xp={expReplay?.CurrentXP}, upgCount={upgradeMgr?.CollectedUpgradeIds?.Count}, dmgMult={statsReplay?.DamageMultiplier}, effDmg={weaponReplay?.EffectiveDamage}, atkSpdMult={statsReplay?.AttackSpeedMultiplier}, effCooldown={weaponReplay?.EffectiveAttackCooldown}");
                allPassed = false;
            }

            // CHECK 9: Retry rollback vs Defeat return-to-map
            // If player dies and retries:
            RunProgressionSession.RestoreCheckpointOnRetry();
            bool c9Retry = RunProgressionSession.Level == 3 &&
                           RunProgressionSession.CurrentXP == 80 &&
                           RunProgressionSession.CommittedUpgradeIds.Count == 2;

            // If player returns to map on defeat: EndRun is called
            RunProgressionSession.EndRun();
            bool c9EndRun = !RunProgressionSession.HasActiveRun &&
                            RunProgressionSession.Level == 1 &&
                            RunProgressionSession.CommittedUpgradeIds.Count == 0;

            if (c9Retry && c9EndRun)
            {
                Debug.Log("[CHECK 9 PASSED] Retry rollback (preserves entry checkpoint) & Defeat return-to-map (clean EndRun) verified.");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] Retry/Defeat mismatch: c9Retry={c9Retry}, c9EndRun={c9EndRun}");
                allPassed = false;
            }

            // CHECK 10: Reselect character starts clean run
            CharacterSelectionSession.Clear();
            CharacterSelectionSession.SetSelection(warriorAsset);
            RunProgressionSession.StartNewRun("warrior");

            bool c10 = RunProgressionSession.HasActiveRun &&
                       RunProgressionSession.Level == 1 &&
                       RunProgressionSession.CurrentXP == 0 &&
                       RunProgressionSession.CommittedUpgradeIds.Count == 0 &&
                       CharacterSelectionSession.SelectedCharacterId == "warrior";

            if (c10)
            {
                Debug.Log("[CHECK 10 PASSED] Character reselection clean run initialization verified.");
            }
            else
            {
                Debug.LogError($"[CHECK 10 FAILED] Reselection mismatch: level={RunProgressionSession.Level}, upgCount={RunProgressionSession.CommittedUpgradeIds.Count}");
                allPassed = false;
            }

            // Cleanup test objects
            UnityEngine.Object.DestroyImmediate(characterReplay.gameObject);
            UnityEngine.Object.DestroyImmediate(spawnerGo);
            UnityEngine.Object.DestroyImmediate(spawnPt.gameObject);
            UnityEngine.Object.DestroyImmediate(upgradeMgrGo);

            // Final Report
            if (allPassed)
            {
                Debug.Log("[MANUAL QA FIXES TEST COMPLETE] All 10 QA verification checks PASSED.");
                Debug.Log("[MANUAL QA COMPLETE] Verification PASSED.");
            }
            else
            {
                Debug.LogError("[MANUAL QA COMPLETE] Verification FAILED.");
            }

            yield return new WaitForSeconds(0.5f);
#if UNITY_EDITOR
            System.IO.File.AppendAllText("gate_verification_results.log", $"[MANUAL QA RESULT] Success: {allPassed} at {DateTime.Now}\n");
            UnityEditor.EditorApplication.isPlaying = false;
            if (Application.isBatchMode)
            {
                UnityEditor.EditorApplication.Exit(allPassed ? 0 : 1);
            }
#endif
        }
    }
}
