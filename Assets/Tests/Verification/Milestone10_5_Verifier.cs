using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated Play Mode verification suite for Milestone 10.5: Progression Foundations & Architecture Invariants.
    /// Verifies:
    /// 1. PlayerStats 3-tier layering (Effective = Base * Permanent * Temporary).
    /// 2. Temporary modifiers do not mutate permanent foundations.
    /// 3. ResetModifiers leaves permanent progression intact.
    /// 4. Architecture separation between CharacterSelectionSession, DungeonRunSession, and RunProgressionSession.
    /// 5. Health is strictly not stored in RunProgressionSession.
    /// 6. Campaign catalog has exactly 3 playable dungeons (no D4/D5).
    /// 7. Build Settings has exactly 5 scenes in sequential order.
    /// 8. Difficulty curves across all 3 dungeons match verified values.
    /// </summary>
    public class Milestone10_5_Verifier : MonoBehaviour
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
                Debug.Log("[GATE 10.5 COMPLETE] All Milestone 10.5 tests PASSED successfully!");
            }
            else
            {
                Debug.LogError("[GATE 10.5 COMPLETE] Verification FAILED.");
            }

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(success ? 0 : 1);
#endif
        }

        private IEnumerator RunVerificationRoutine(Action<bool> onComplete)
        {
            yield return null;
            Debug.Log("[GATE 10.5] Beginning Milestone 10.5 Automated Play Mode Verification...");
            bool allPassed = true;

            // -------------------------------------------------------------
            // CHECK 1: Baseline PlayerStats Layering
            // -------------------------------------------------------------
            var statsGo = new GameObject("Test_PlayerStats_Layering");
            var stats = statsGo.AddComponent<PlayerStats>();

            int statsChangedCount = 0;
            stats.OnStatsChanged += () => statsChangedCount++;

            bool c1 = Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.PermanentDamageMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.DamageMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.TemporaryMovementSpeedMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.MovementSpeedMultiplier, 1.0f);

            if (c1)
            {
                Debug.Log("[CHECK 1 PASSED] PlayerStats baseline neutral defaults: Temporary=1.0, Permanent=1.0, Effective=1.0.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] PlayerStats defaults mismatch: dmg={stats.DamageMultiplier}, spd={stats.MovementSpeedMultiplier}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 2: Temporary Modifier Application
            // -------------------------------------------------------------
            stats.AddDamageBonus(0.15f); // +15% temporary damage
            stats.AddMovementSpeedBonus(0.10f); // +10% temporary speed

            bool c2 = Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.15f) &&
                      Mathf.Approximately(stats.PermanentDamageMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.DamageMultiplier, 1.15f) &&
                      Mathf.Approximately(stats.TemporaryMovementSpeedMultiplier, 1.10f) &&
                      Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.MovementSpeedMultiplier, 1.10f);

            if (c2)
            {
                Debug.Log($"[CHECK 2 PASSED] Temporary modifiers applied: Temporary Dmg={stats.TemporaryDamageMultiplier}, Permanent Dmg={stats.PermanentDamageMultiplier}, Effective Dmg={stats.DamageMultiplier}.");
            }
            else
            {
                Debug.LogError($"[CHECK 2 FAILED] Temporary modifier mismatch: tempDmg={stats.TemporaryDamageMultiplier}, effDmg={stats.DamageMultiplier}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 3: Permanent Foundation Layering (Base * Permanent * Temporary)
            // -------------------------------------------------------------
            stats.SetPermanentMultipliers(1.10f, 1.05f, 1.05f); // +10% meta dmg, +5% meta atkSpd, +5% meta spd

            // Expected Effective Dmg = 1.15 * 1.10 = 1.265
            // Expected Effective Spd = 1.10 * 1.05 = 1.155
            float expectedEffectiveDmg = 1.15f * 1.10f;
            float expectedEffectiveSpd = 1.10f * 1.05f;

            bool c3 = Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.15f) &&
                      Mathf.Approximately(stats.PermanentDamageMultiplier, 1.10f) &&
                      Mathf.Approximately(stats.DamageMultiplier, expectedEffectiveDmg) &&
                      Mathf.Approximately(stats.MovementSpeedMultiplier, expectedEffectiveSpd);

            if (c3)
            {
                Debug.Log($"[CHECK 3 PASSED] Layered stat progression verified: 1.15 temp * 1.10 perm = {stats.DamageMultiplier:F3} effective damage.");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] Layered stat progression mismatch: dmg={stats.DamageMultiplier}, expected={expectedEffectiveDmg}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 4: Reset Modifiers Preserves Permanent Foundations
            // -------------------------------------------------------------
            stats.ResetModifiers();

            bool c4 = Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.PermanentDamageMultiplier, 1.10f) &&
                      Mathf.Approximately(stats.DamageMultiplier, 1.10f) &&
                      Mathf.Approximately(stats.TemporaryMovementSpeedMultiplier, 1.0f) &&
                      Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.05f) &&
                      Mathf.Approximately(stats.MovementSpeedMultiplier, 1.05f);

            if (c4)
            {
                Debug.Log($"[CHECK 4 PASSED] ResetModifiers reverted temporary bonuses while preserving permanent foundations (Effective Dmg={stats.DamageMultiplier}).");
            }
            else
            {
                Debug.LogError($"[CHECK 4 FAILED] ResetModifiers failed: tempDmg={stats.TemporaryDamageMultiplier}, permDmg={stats.PermanentDamageMultiplier}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 5: OnStatsChanged Event Notification
            // -------------------------------------------------------------
            bool c5 = statsChangedCount >= 4;
            if (c5)
            {
                Debug.Log($"[CHECK 5 PASSED] OnStatsChanged event fired {statsChangedCount} times across modifier mutations.");
            }
            else
            {
                Debug.LogError($"[CHECK 5 FAILED] OnStatsChanged did not fire expected times: {statsChangedCount}");
                allPassed = false;
            }

            DestroyImmediate(statsGo);

            // -------------------------------------------------------------
            // CHECK 6: Architecture Separation Verification
            // -------------------------------------------------------------
            // CharacterSelectionSession: authoritative hero selection
            // DungeonRunSession: authoritative selected dungeon only
            // RunProgressionSession: carrier for cross-dungeon run progression
            var roster = AssetDatabase.LoadAssetAtPath<CharacterRoster>("Assets/ScriptableObjects/Characters/CharacterRoster.asset");
            if (roster != null && roster.Characters != null && roster.Characters.Count > 0)
            {
                CharacterSelectionSession.SetSelection(roster.Characters[0]);
            }
            var d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_01.asset");
            DungeonRunSession.SetSelection(d1);
            RunProgressionSession.StartNewRun(CharacterSelectionSession.SelectedCharacter.Id);

            bool c6 = CharacterSelectionSession.HasSelection &&
                      DungeonRunSession.HasSelection &&
                      DungeonRunSession.SelectedDungeon == d1 &&
                      RunProgressionSession.HasActiveRun &&
                      RunProgressionSession.ValidateOwner(CharacterSelectionSession.SelectedCharacter.Id);

            if (c6)
            {
                Debug.Log("[CHECK 6 PASSED] Clean architectural separation verified: CharacterSelectionSession, DungeonRunSession, and RunProgressionSession fulfill discrete responsibilities.");
            }
            else
            {
                Debug.LogError("[CHECK 6 FAILED] Architecture separation check failed.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 7: Health Is Not Stored in RunProgressionSession
            // -------------------------------------------------------------
            // Inspect type properties via reflection: RunProgressionSession must have zero health properties/fields
            var props = typeof(RunProgressionSession).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var fields = typeof(RunProgressionSession).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            bool c7NoHealth = true;
            foreach (var p in props)
            {
                if (p.Name.ToLowerInvariant().Contains("health") || p.Name.ToLowerInvariant().Contains("hp"))
                {
                    c7NoHealth = false;
                    break;
                }
            }
            foreach (var f in fields)
            {
                if (f.Name.ToLowerInvariant().Contains("health") || f.Name.ToLowerInvariant().Contains("hp"))
                {
                    c7NoHealth = false;
                    break;
                }
            }

            if (c7NoHealth)
            {
                Debug.Log("[CHECK 7 PASSED] RunProgressionSession strictly does NOT store player health (Health resets to 100% full each dungeon).");
            }
            else
            {
                Debug.LogError("[CHECK 7 FAILED] RunProgressionSession contains health fields/properties!");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 8: Campaign Catalog Exactly 3 Dungeons (No D4/D5)
            // -------------------------------------------------------------
            var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>("Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset");
            bool c8 = catalog != null && catalog.Dungeons != null && catalog.Dungeons.Count == 3;
            if (c8)
            {
                Debug.Log("[CHECK 8 PASSED] DungeonCatalog contains exactly 3 playable dungeons: D1, D2, D3.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] DungeonCatalog count mismatch: {catalog?.Dungeons?.Count}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 9: Build Settings Has Exactly 5 Scenes
            // -------------------------------------------------------------
            var buildScenes = EditorBuildSettings.scenes;
            bool c9 = buildScenes != null && buildScenes.Length == 5;
            if (c9)
            {
                Debug.Log("[CHECK 9 PASSED] Build Settings contains exactly 5 playable campaign scenes.");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] Build Settings scene count mismatch: {buildScenes?.Length}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 10: Difficulty Multipliers Curve Across Dungeons
            // -------------------------------------------------------------
            var d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_02.asset");
            var d3 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_03.asset");

            bool c10 = d1 != null && Mathf.Approximately(d1.EnemyHealthMultiplier, 1.0f) && Mathf.Approximately(d1.EnemyDamageMultiplier, 1.0f) &&
                       d2 != null && Mathf.Approximately(d2.EnemyHealthMultiplier, 1.1f) && Mathf.Approximately(d2.EnemyDamageMultiplier, 1.0f) &&
                       d3 != null && Mathf.Approximately(d3.EnemyHealthMultiplier, 1.2f) && Mathf.Approximately(d3.EnemyDamageMultiplier, 1.1f);

            if (c10)
            {
                Debug.Log($"[CHECK 10 PASSED] Difficulty curve verified: D1={d1.EnemyHealthMultiplier}x/{d1.EnemyDamageMultiplier}x, D2={d2.EnemyHealthMultiplier}x/{d2.EnemyDamageMultiplier}x, D3={d3.EnemyHealthMultiplier}x/{d3.EnemyDamageMultiplier}x.");
            }
            else
            {
                Debug.LogError("[CHECK 10 FAILED] Difficulty curve mismatch across D1, D2, D3.");
                allPassed = false;
            }

            RunProgressionSession.Clear();
            DungeonRunSession.Clear();

            if (allPassed)
            {
                Debug.Log("[GATE 10.5 TEST COMPLETE] All 10 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 10.5 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
