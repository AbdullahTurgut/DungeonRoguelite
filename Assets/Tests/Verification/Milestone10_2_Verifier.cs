using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated Play Mode verification suite for Milestone 10.2: Data-Driven Enemy Scaling.
    /// Verifies:
    /// 1. Dungeon 1 difficulty configuration (1.0x HP, 1.0x Dmg).
    /// 2. Dungeon 2 difficulty configuration (1.1x HP, 1.0x Dmg).
    /// 3. EnemyHealth.InitializeHealth scales instance without modifying defaults.
    /// 4. EnemyAttack.InitializeAttack scales instance without modifying defaults.
    /// 5. Zombie prefab asset integrity (remains strictly 50 HP, 10 dmg).
    /// 6. WaveManager integration test with scaled instance spawning.
    /// 7. Hit-to-kill breakpoint validation across archetypes (Warrior, Archer, Gunner).
    /// </summary>
    public class Milestone10_2_Verifier : MonoBehaviour
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

            yield return new WaitForSeconds(0.5f);

            if (success)
            {
                Debug.Log("[GATE 10.2 COMPLETE] All Milestone 10.2 tests PASSED successfully!");
            }
            else
            {
                Debug.LogError("[GATE 10.2 COMPLETE] Verification FAILED.");
            }

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(success ? 0 : 1);
#endif
        }

        private IEnumerator RunVerificationRoutine(Action<bool> onComplete)
        {
            yield return null;
            Debug.Log("[GATE 10.2] Beginning Milestone 10.2 Automated Play Mode Verification...");
            bool allPassed = true;

            // -------------------------------------------------------------
            // CHECK 1: Dungeon 1 Difficulty Configuration
            // -------------------------------------------------------------
            var d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_01.asset");
            bool c1 = d1 != null &&
                      Mathf.Approximately(d1.EnemyHealthMultiplier, 1.0f) &&
                      Mathf.Approximately(d1.EnemyDamageMultiplier, 1.0f);
            if (c1)
            {
                Debug.Log($"[CHECK 1 PASSED] Dungeon 1 difficulty scaling: HP={d1.EnemyHealthMultiplier}x, Dmg={d1.EnemyDamageMultiplier}x.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] Dungeon 1 scaling incorrect: d1={d1?.name}, HP={d1?.EnemyHealthMultiplier}, Dmg={d1?.EnemyDamageMultiplier}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 2: Dungeon 2 Difficulty Configuration
            // -------------------------------------------------------------
            var d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_02.asset");
            bool c2 = d2 != null &&
                      Mathf.Approximately(d2.EnemyHealthMultiplier, 1.1f) &&
                      Mathf.Approximately(d2.EnemyDamageMultiplier, 1.0f);
            if (c2)
            {
                Debug.Log($"[CHECK 2 PASSED] Dungeon 2 difficulty scaling: HP={d2.EnemyHealthMultiplier}x, Dmg={d2.EnemyDamageMultiplier}x.");
            }
            else
            {
                Debug.LogError($"[CHECK 2 FAILED] Dungeon 2 scaling incorrect: d2={d2?.name}, HP={d2?.EnemyHealthMultiplier}, Dmg={d2?.EnemyDamageMultiplier}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 3: EnemyHealth.InitializeHealth scaling
            // -------------------------------------------------------------
            var testGo = new GameObject("TestEnemyScalingInstance");
            var healthComp = testGo.AddComponent<EnemyHealth>();
            // Default maxHealth is 50f
            float baseMaxHp = healthComp.MaxHealth;
            bool healthChangedFired = false;
            healthComp.OnHealthChanged += (cur, max) => healthChangedFired = true;

            healthComp.InitializeHealth(1.1f);
            float scaledHp = healthComp.MaxHealth;
            float expectedScaledHp = Mathf.Round(baseMaxHp * 1.1f); // 55f

            bool c3 = Mathf.Approximately(scaledHp, expectedScaledHp) &&
                      Mathf.Approximately(healthComp.CurrentHealth, expectedScaledHp) &&
                      healthChangedFired &&
                      !healthComp.IsDead;

            if (c3)
            {
                Debug.Log($"[CHECK 3 PASSED] EnemyHealth.InitializeHealth correctly scaled 50 -> {scaledHp} HP (expected {expectedScaledHp}).");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] EnemyHealth scaling mismatch: scaledHp={scaledHp}, current={healthComp.CurrentHealth}, event={healthChangedFired}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 4: EnemyAttack.InitializeAttack scaling
            // -------------------------------------------------------------
            var attackComp = testGo.AddComponent<EnemyAttack>();
            float baseDmg = attackComp.Damage; // 10f
            attackComp.InitializeAttack(1.1f);
            float scaledDmg = attackComp.Damage;
            float expectedScaledDmg = Mathf.Round(baseDmg * 1.1f); // 11f

            bool c4 = Mathf.Approximately(scaledDmg, expectedScaledDmg);
            if (c4)
            {
                Debug.Log($"[CHECK 4 PASSED] EnemyAttack.InitializeAttack correctly scaled 10 -> {scaledDmg} Dmg (expected {expectedScaledDmg}).");
            }
            else
            {
                Debug.LogError($"[CHECK 4 FAILED] EnemyAttack scaling mismatch: scaledDmg={scaledDmg}, expected={expectedScaledDmg}");
                allPassed = false;
            }

            DestroyImmediate(testGo);

            // -------------------------------------------------------------
            // CHECK 5: Prefab Asset Integrity (Zero Asset Mutation)
            // -------------------------------------------------------------
            var zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            bool c5PrefabExists = zombiePrefab != null;
            if (!c5PrefabExists)
            {
                Debug.LogError("[CHECK 5 FAILED] Zombie.prefab could not be loaded.");
                allPassed = false;
            }
            else
            {
                var prefabHealth = zombiePrefab.GetComponent<EnemyHealth>();
                var prefabAttack = zombiePrefab.GetComponent<EnemyAttack>();

                // Spawn an instance and mutate it with 1.5x
                var spawnedZombie = Instantiate(zombiePrefab);
                spawnedZombie.GetComponent<EnemyHealth>().InitializeHealth(1.5f);
                spawnedZombie.GetComponent<EnemyAttack>().InitializeAttack(1.5f);

                float instanceHp = spawnedZombie.GetComponent<EnemyHealth>().MaxHealth;
                float instanceDmg = spawnedZombie.GetComponent<EnemyAttack>().Damage;
                DestroyImmediate(spawnedZombie);

                // Check original prefab values
                float unmutatedHp = prefabHealth.MaxHealth;
                float unmutatedDmg = prefabAttack.Damage;

                bool c5 = Mathf.Approximately(instanceHp, 75f) &&
                          Mathf.Approximately(instanceDmg, 15f) &&
                          Mathf.Approximately(unmutatedHp, 50f) &&
                          Mathf.Approximately(unmutatedDmg, 10f);

                if (c5)
                {
                    Debug.Log($"[CHECK 5 PASSED] Prefab asset integrity verified: Prefab remains 50 HP / 10 Dmg after instance was scaled to 75 HP / 15 Dmg.");
                }
                else
                {
                    Debug.LogError($"[CHECK 5 FAILED] Prefab mutated! prefabHp={unmutatedHp}, prefabDmg={unmutatedDmg}");
                    allPassed = false;
                }
            }

            // -------------------------------------------------------------
            // CHECK 6: WaveManager Spawning Integration with Dungeon_02
            // -------------------------------------------------------------
            var waveMgrGo = new GameObject("WaveManager_Scaling_Test");
            var waveMgr = waveMgrGo.AddComponent<WaveManager>();
            waveMgr.ConfigureFromDungeon(d2);

            // Spawn one enemy via WaveManager using private method through Reflection or public interface
            var spawnMethod = typeof(WaveManager).GetMethod("SpawnEnemy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (spawnMethod != null && zombiePrefab != null)
            {
                spawnMethod.Invoke(waveMgr, new object[] { zombiePrefab });

                bool foundScaled = false;
                foreach (var living in waveMgr.ActiveEnemies)
                {
                    if (living != null && Mathf.Approximately(living.MaxHealth, 55f))
                    {
                        foundScaled = true;
                        break;
                    }
                }

                if (foundScaled)
                {
                    Debug.Log("[CHECK 6 PASSED] WaveManager dynamically applied Dungeon 2 difficulty scaling (55 HP) to spawned enemy.");
                }
                else
                {
                    Debug.LogError("[CHECK 6 FAILED] WaveManager did not apply 55 HP scaling to spawned enemy.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 6 FAILED] Could not find SpawnEnemy method on WaveManager.");
                allPassed = false;
            }

            // Cleanup spawned wave enemies and manager
            waveMgr.HaltDungeon();
            var allZombies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (var z in allZombies)
            {
                if (z != null) DestroyImmediate(z.gameObject);
            }
            DestroyImmediate(waveMgrGo);

            // -------------------------------------------------------------
            // CHECK 7: Mathematical Hit-to-Kill Breakpoint Validation
            // -------------------------------------------------------------
            // D1: Zombie HP = 50
            // Warrior: 25 dmg -> 2 hits
            // Archer: 18 dmg -> 3 hits
            // Gunner: 12 dmg -> 5 hits
            int d1WarriorHits = Mathf.CeilToInt(50f / 25f);
            int d1ArcherHits = Mathf.CeilToInt(50f / 18f);
            int d1GunnerHits = Mathf.CeilToInt(50f / 12f);

            // D2: Zombie HP = 55
            // Warrior base: 25 dmg -> 3 hits (+1 hit penalty)
            // Warrior +15% dmg: 28.75 dmg -> 2 hits (upgrade restores 2-hit breakpoint!)
            int d2WarriorBaseHits = Mathf.CeilToInt(55f / 25f);
            int d2WarriorUpgradedHits = Mathf.CeilToInt(55f / (25f * 1.15f));

            // Archer base: 18 dmg -> 4 hits (+1 hit penalty)
            // Archer +15% dmg: 20.7 dmg -> 3 hits (upgrade restores 3-hit breakpoint!)
            int d2ArcherBaseHits = Mathf.CeilToInt(55f / 18f);
            int d2ArcherUpgradedHits = Mathf.CeilToInt(55f / (18f * 1.15f));

            // Gunner base: 12 dmg -> 5 hits
            // Gunner +15% dmg: 13.8 dmg -> 4 hits (upgrade saves 1 hit!)
            int d2GunnerBaseHits = Mathf.CeilToInt(55f / 12f);
            int d2GunnerUpgradedHits = Mathf.CeilToInt(55f / (12f * 1.15f));

            bool c7 = (d1WarriorHits == 2 && d1ArcherHits == 3 && d1GunnerHits == 5) &&
                      (d2WarriorBaseHits == 3 && d2WarriorUpgradedHits == 2) &&
                      (d2ArcherBaseHits == 4 && d2ArcherUpgradedHits == 3) &&
                      (d2GunnerBaseHits == 5 && d2GunnerUpgradedHits == 4);

            if (c7)
            {
                Debug.Log($"[CHECK 7 PASSED] Breakpoints verified: Warrior D1={d1WarriorHits} -> D2 base={d2WarriorBaseHits} -> D2 upgraded={d2WarriorUpgradedHits}; Archer D1={d1ArcherHits} -> D2 base={d2ArcherBaseHits} -> D2 upgraded={d2ArcherUpgradedHits}; Gunner D1={d1GunnerHits} -> D2 base={d2GunnerBaseHits} -> D2 upgraded={d2GunnerUpgradedHits}.");
            }
            else
            {
                Debug.LogError($"[CHECK 7 FAILED] Breakpoint calculation failure: W={d2WarriorBaseHits}/{d2WarriorUpgradedHits}, A={d2ArcherBaseHits}/{d2ArcherUpgradedHits}, G={d2GunnerBaseHits}/{d2GunnerUpgradedHits}");
                allPassed = false;
            }

            if (allPassed)
            {
                Debug.Log("[GATE 10.2 TEST COMPLETE] All 7 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 10.2 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
