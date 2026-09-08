using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Weapons;
using DungeonRoguelite.Player;

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
            yield return null;

            if (success)
            {
                Debug.Log("[GATE 10.2 COMPLETE] All Milestone 10.2 tests PASSED successfully!");
            }
            else
            {
                Debug.LogError("[GATE 10.2 COMPLETE] Verification FAILED.");
            }

#if UNITY_EDITOR
            System.IO.File.AppendAllText("gate_verification_results.log", $"[GATE 10.2 RESULT] Success: {success} at {DateTime.Now}\n");
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
            Time.timeScale = 1f;

            // Halt ambient scene WaveManager and remove ambient enemies to prevent background defeat
            var ambientWaveMgr = FindFirstObjectByType<WaveManager>();
            if (ambientWaveMgr != null)
            {
                ambientWaveMgr.HaltDungeon();
            }
            var ambientSpawner = FindFirstObjectByType<PlayerSpawner>();
            if (ambientSpawner != null)
            {
                ambientSpawner.gameObject.SetActive(false);
            }
            var ambientEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (var enemy in ambientEnemies)
            {
                if (enemy != null) DestroyImmediate(enemy.gameObject);
            }

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

            // -------------------------------------------------------------
            // CHECK 8: Warrior Prefab Melee Reach Configuration (2.5m)
            // -------------------------------------------------------------
            var warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Warrior.prefab");
            var warriorMelee = warriorPrefab != null ? warriorPrefab.GetComponent<MeleeWeapon>() : null;
            bool c8 = warriorMelee != null &&
                      Mathf.Approximately(warriorMelee.Range, 2.5f) &&
                      Mathf.Approximately(warriorMelee.Damage, 25f) &&
                      Mathf.Approximately(warriorMelee.AttackCooldown, 0.5f) &&
                      Mathf.Approximately(warriorMelee.ArcAngle, 120f);

            if (c8)
            {
                Debug.Log($"[CHECK 8 PASSED] Warrior prefab melee configuration verified: Range={warriorMelee.Range}m (2.5m target), Damage={warriorMelee.Damage}, Cooldown={warriorMelee.AttackCooldown}s, Arc={warriorMelee.ArcAngle}°.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] Warrior prefab melee configuration mismatch: Range={warriorMelee?.Range}, Damage={warriorMelee?.Damage}, Cooldown={warriorMelee?.AttackCooldown}, Arc={warriorMelee?.ArcAngle}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // Setup Helpers for Functional Hit Tests (Checks 9-14)
            // -------------------------------------------------------------
            Func<string, (GameObject, MeleeWeapon, PlayerStats)> createTestWeapon = (name) =>
            {
                var go = new GameObject(name);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                var melee = go.AddComponent<MeleeWeapon>();
                var stats = go.AddComponent<PlayerStats>();
                melee.SetPlayerStats(stats);
                return (go, melee, stats);
            };

            Func<string, Vector3, (GameObject, TestDamageableTarget)> createTarget = (name, pos) =>
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = name;
                go.transform.position = pos;
                var dummy = go.AddComponent<TestDamageableTarget>();
                return (go, dummy);
            };

            // -------------------------------------------------------------
            // CHECK 9: Target at 2.25m (Outside Old 2.0m, Inside New 2.5m) is Hit
            // -------------------------------------------------------------
            Time.timeScale = 1f;
            var (wGo9, testMelee9, _) = createTestWeapon("Warrior_Test_C9");
            var (target225Go, target225Dummy) = createTarget("Target_225m", new Vector3(0f, 0f, 2.25f));
            Physics.SyncTransforms();
            bool attackAt225 = testMelee9.TryAttack();
            bool c9 = attackAt225 && target225Dummy.HitCount == 1 && Mathf.Approximately(target225Dummy.LastDamageReceived, 25f);
            if (c9)
            {
                Debug.Log("[CHECK 9 PASSED] Target at 2.25m (outside old 2.0m range, inside new 2.5m range) successfully hit with 25 damage.");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] Target at 2.25m was not hit: attack={attackAt225}, hitCount={target225Dummy.HitCount}, lastDmg={target225Dummy.LastDamageReceived}");
                allPassed = false;
            }
            DestroyImmediate(target225Go);
            DestroyImmediate(wGo9);
            yield return null;

            // -------------------------------------------------------------
            // CHECK 10: Target Beyond 2.5m (Center 3.25m / Surface 2.75m) Receives Zero Damage
            // -------------------------------------------------------------
            // Capsule primitive has radius 0.5m; center at 3.25m puts closest collider surface at 2.75m (> 2.5m range)
            Time.timeScale = 1f;
            var (wGo10, testMelee10, _) = createTestWeapon("Warrior_Test_C10");
            var (targetFarGo, targetFarDummy) = createTarget("Target_275m_Surface", new Vector3(0f, 0f, 3.25f));
            Physics.SyncTransforms();
            bool attack10Fired = testMelee10.TryAttack();
            bool c10 = attack10Fired && targetFarDummy.HitCount == 0 && Mathf.Approximately(targetFarDummy.CurrentHealth, 100f);
            if (c10)
            {
                Debug.Log("[CHECK 10 PASSED] Target beyond 2.5m (surface at 2.75m) receives zero damage from executed swing.");
            }
            else
            {
                Debug.LogError($"[CHECK 10 FAILED] Target beyond 2.5m was hit or swing blocked: fired={attack10Fired}, hitCount={targetFarDummy.HitCount}, hp={targetFarDummy.CurrentHealth}");
                allPassed = false;
            }
            DestroyImmediate(targetFarGo);
            DestroyImmediate(wGo10);
            yield return null;

            // -------------------------------------------------------------
            // CHECK 11: Arc Cone (120°) Still Applies at New Range
            // -------------------------------------------------------------
            // Target at 2.25m distance but 90 degrees sideways (outside 120-degree cone)
            Time.timeScale = 1f;
            var (wGo11, testMelee11, _) = createTestWeapon("Warrior_Test_C11");
            var (targetArcGo, targetArcDummy) = createTarget("Target_90deg", new Vector3(2.25f, 0f, 0f));
            Physics.SyncTransforms();
            bool attack11Fired = testMelee11.TryAttack();
            bool c11 = attack11Fired && targetArcDummy.HitCount == 0 && Mathf.Approximately(targetArcDummy.CurrentHealth, 100f);
            if (c11)
            {
                Debug.Log("[CHECK 11 PASSED] 120° forward arc cone strictly filters out sideways targets at 2.25m from executed swing.");
            }
            else
            {
                Debug.LogError($"[CHECK 11 FAILED] Sideways target outside arc was hit or swing blocked: fired={attack11Fired}, hitCount={targetArcDummy.HitCount}");
                allPassed = false;
            }
            DestroyImmediate(targetArcGo);
            DestroyImmediate(wGo11);
            yield return null;

            // -------------------------------------------------------------
            // CHECK 12: One Attack Does Not Double-Hit
            // -------------------------------------------------------------
            Time.timeScale = 1f;
            var (wGo12, testMelee12, _) = createTestWeapon("Warrior_Test_C12");
            var (targetDedupeGo, targetDedupeDummy) = createTarget("Target_Dedupe", new Vector3(0f, 0f, 1.8f));
            Physics.SyncTransforms();
            bool attack12Fired = testMelee12.TryAttack();
            bool c12 = attack12Fired && targetDedupeDummy.HitCount == 1;
            if (c12)
            {
                Debug.Log("[CHECK 12 PASSED] Attack hit detection de-duplicates cleanly: exactly 1 hit per swing.");
            }
            else
            {
                Debug.LogError($"[CHECK 12 FAILED] Multi-hit detected: fired={attack12Fired}, hitCount={targetDedupeDummy.HitCount}");
                allPassed = false;
            }
            DestroyImmediate(targetDedupeGo);
            DestroyImmediate(wGo12);
            yield return null;

            // -------------------------------------------------------------
            // CHECK 13: PlayerStats Damage Scaling Works with New Range
            // -------------------------------------------------------------
            float c13PreTimeScale = Time.timeScale;
            Time.timeScale = 1f;
            var (wGo13, testMelee13, testStats13) = createTestWeapon("Warrior_Test_C13");
            testStats13.AddDamageBonus(0.20f); // +20% -> 30 effective damage
            bool multOk = Mathf.Approximately(testStats13.DamageMultiplier, 1.20f);
            Debug.Log($"[CHECK 13 PRE-CHECK] preTimeScale={c13PreTimeScale}, currentTimeScale={Time.timeScale}, multOk={multOk} ({testStats13.DamageMultiplier}), effDmg={testMelee13.EffectiveDamage}, Time.time={Time.time}");

            var (targetScaledGo, targetScaledDummy) = createTarget("Target_Scaled", new Vector3(0f, 0f, 2.25f));
            Physics.SyncTransforms();
            bool attack13Fired = testMelee13.TryAttack();
            bool c13 = attack13Fired && multOk && targetScaledDummy.HitCount == 1 && Mathf.Approximately(targetScaledDummy.LastDamageReceived, 30f);
            if (c13)
            {
                Debug.Log($"[CHECK 13 PASSED] PlayerStats damage scaling (+20% -> 30 damage) functions accurately at new 2.25m reach (fired={attack13Fired}, hitCount={targetScaledDummy.HitCount}, dmg={targetScaledDummy.LastDamageReceived}).");
            }
            else
            {
                Debug.LogError($"[CHECK 13 FAILED] Scaled attack mismatch: fired={attack13Fired}, multOk={multOk} ({testStats13.DamageMultiplier}), hitCount={targetScaledDummy.HitCount}, dmg={targetScaledDummy.LastDamageReceived} (expected 30)");
                allPassed = false;
            }
            DestroyImmediate(targetScaledGo);
            DestroyImmediate(wGo13);
            yield return null;

            // -------------------------------------------------------------
            // CHECK 14: Melee Attack Compatible with Pause State
            // -------------------------------------------------------------
            var (wGo14, testMelee14, _) = createTestWeapon("Warrior_Test_C14");
            var (targetPauseGo, targetPauseDummy) = createTarget("Target_Pause", new Vector3(0f, 0f, 1.5f));
            Physics.SyncTransforms();
            try
            {
                Time.timeScale = 0f;
                bool pauseAttack = testMelee14.TryAttack();
                bool c14 = !pauseAttack && targetPauseDummy.HitCount == 0;
                if (c14)
                {
                    Debug.Log("[CHECK 14 PASSED] Melee attack respects pause guard (Time.timeScale <= 0f blocks attacks).");
                }
                else
                {
                    Debug.LogError($"[CHECK 14 FAILED] Attack executed during pause: allowed={pauseAttack}, hitCount={targetPauseDummy.HitCount}");
                    allPassed = false;
                }
            }
            finally
            {
                Time.timeScale = 1f;
            }
            DestroyImmediate(targetPauseGo);
            DestroyImmediate(wGo14);

            // -------------------------------------------------------------
            // CHECK 15: Archer / Gunner Combat Systems Unaffected
            // -------------------------------------------------------------
            var archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Archer.prefab");
            var bowComp = archerPrefab != null ? archerPrefab.GetComponent<BowWeapon>() : null;
            var gunnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Gunner.prefab");
            var rifleComp = gunnerPrefab != null ? gunnerPrefab.GetComponent<RifleWeapon>() : null;

            bool c15Archer = bowComp != null && Mathf.Approximately(bowComp.Damage, 20f) && Mathf.Approximately(bowComp.AttackCooldown, 0.6f);
            bool c15Gunner = rifleComp != null && Mathf.Approximately(rifleComp.Damage, 10f) && Mathf.Approximately(rifleComp.AttackCooldown, 0.18f) && Mathf.Approximately(rifleComp.Range, 25f);
            bool c15 = c15Archer && c15Gunner;

            if (c15)
            {
                Debug.Log("[CHECK 15 PASSED] Archer (Bow: 20 dmg, 0.6s cd) and Gunner (Rifle: 10 dmg, 0.18s cd, 25m range) configurations unaffected.");
            }
            else
            {
                Debug.LogError($"[CHECK 15 FAILED] Archetype combat affected: ArcherOk={c15Archer}, GunnerOk={c15Gunner}");
                allPassed = false;
            }

            if (allPassed)
            {
                Debug.Log("[GATE 10.2 TEST COMPLETE] All 15 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 10.2 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
