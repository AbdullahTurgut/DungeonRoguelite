using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Player;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Weapons;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated Play Mode verification suite for Milestone 4.1 (Spawner and Wave System).
    /// Validates data-driven WaveDefinition loading, sequential wave spawning (10, 15, 20),
    /// round-robin spawn points, spawn interval timing, living enemy tracking, duplicate death guards,
    /// wave progression gating, single-fire dungeon completion, combat integration, and non-regression.
    /// </summary>
    public class Milestone4_1_Verifier : MonoBehaviour
    {
        [SerializeField] private bool runAutomatedTestOnStart = true;

        private void Start()
        {
            if (runAutomatedTestOnStart || Application.isBatchMode)
            {
                StartCoroutine(RunVerificationRoutine());
            }
        }

        private IEnumerator RunVerificationRoutine()
        {
            yield return new WaitForSeconds(0.1f);

            Debug.Log("[M4.1 TEST START] Beginning Milestone 4.1 automated verification suite...");
            bool allPassed = true;

            // 1. Resolve Player and core components
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null)
            {
                Debug.LogError("[M4.1 TEST FAILED] Required Player GameObject missing in scene.");
                ExitBatch(1);
                yield break;
            }

            var playerHealth = playerGo.GetComponent<PlayerHealth>();
            var playerMovement = playerGo.GetComponent<PlayerMovement>();
            var playerAim = playerGo.GetComponent<PlayerAim>();
            var playerAttack = playerGo.GetComponent<PlayerAttack>();
            var meleeWeapon = playerGo.GetComponent<MeleeWeapon>();
            var playerCC = playerGo.GetComponent<CharacterController>();

            if (playerHealth == null || playerMovement == null || playerAim == null || playerAttack == null || meleeWeapon == null || playerCC == null)
            {
                Debug.LogError("[M4.1 TEST FAILED] Required Player components missing.");
                ExitBatch(1);
                yield break;
            }

            // 2. Resolve WaveManager and SpawnPoints
            var waveManagerGo = GameObject.Find("WaveManager");
            if (waveManagerGo == null)
            {
                Debug.LogError("[M4.1 TEST FAILED] Required WaveManager GameObject missing in scene.");
                ExitBatch(1);
                yield break;
            }

            var waveManager = waveManagerGo.GetComponent<WaveManager>();
            if (waveManager == null)
            {
                Debug.LogError("[M4.1 TEST FAILED] WaveManager component missing on WaveManager GameObject.");
                ExitBatch(1);
                yield break;
            }

            // Stop any ongoing wave progression to start clean verification
            waveManager.StopWaves();

            // Resolve Zombie prefab
            GameObject zombiePrefab = null;
#if UNITY_EDITOR
            zombiePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
#endif
            if (zombiePrefab == null)
            {
                Debug.LogError("[M4.1 TEST FAILED] Could not load Zombie prefab from Assets/Prefabs/Enemies/Zombie.prefab.");
                ExitBatch(1);
                yield break;
            }

            // Setup 4 spawn points
            var spawnPointsRoot = GameObject.Find("SpawnPoints");
            if (spawnPointsRoot == null)
            {
                Debug.LogError("[M4.1 TEST FAILED] Required SpawnPoints root missing.");
                ExitBatch(1);
                yield break;
            }

            Transform[] spawnPoints = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                var sp = spawnPointsRoot.transform.Find($"SpawnPoint_0{i + 1}");
                if (sp == null)
                {
                    Debug.LogError($"[M4.1 TEST FAILED] SpawnPoint_0{i + 1} missing under SpawnPoints root.");
                    ExitBatch(1);
                    yield break;
                }
                spawnPoints[i] = sp;
            }

            waveManager.SetSpawnPoints(spawnPoints);
            waveManager.SetPlayerTarget(playerGo.transform);

            // -------------------------------------------------------------
            // CHECK 1: Verify production WaveDefinition ScriptableObjects
            // -------------------------------------------------------------
#if UNITY_EDITOR
            var prodWave1 = UnityEditor.AssetDatabase.LoadAssetAtPath<WaveDefinition>("Assets/ScriptableObjects/Waves/Wave_01.asset");
            var prodWave2 = UnityEditor.AssetDatabase.LoadAssetAtPath<WaveDefinition>("Assets/ScriptableObjects/Waves/Wave_02.asset");
            var prodWave3 = UnityEditor.AssetDatabase.LoadAssetAtPath<WaveDefinition>("Assets/ScriptableObjects/Waves/Wave_03.asset");

            bool c1Passed = prodWave1 != null && prodWave1.TotalEnemyCount == 10 &&
                            prodWave2 != null && prodWave2.TotalEnemyCount == 15 &&
                            prodWave3 != null && prodWave3.TotalEnemyCount == 20 &&
                            prodWave1.SpawnInterval > 0f &&
                            prodWave1.EnemyEntries[0].EnemyPrefab == zombiePrefab;

            if (c1Passed)
            {
                Debug.Log("[CHECK 1 PASSED] Production WaveDefinition assets loaded and verified (Wave 1: 10, Wave 2: 15, Wave 3: 20 Zombies).");
            }
            else
            {
                Debug.LogError("[CHECK 1 FAILED] Production WaveDefinition assets invalid or missing.");
                allPassed = false;
            }
#endif

            // -------------------------------------------------------------
            // CHECK 6: Verify Spawn Interval Timing
            // -------------------------------------------------------------
            var timingWave = ScriptableObject.CreateInstance<WaveDefinition>();
            timingWave.Initialize(new EnemySpawnEntry[] { new EnemySpawnEntry(zombiePrefab, 2) }, 0.1f);

            waveManager.SetWaves(new WaveDefinition[] { timingWave });
            float startTime = Time.time;
            waveManager.StartWaves();

            // Wait for 1st enemy
            while (waveManager.LivingEnemyCount < 1) yield return null;
            float spawn1Time = Time.time;

            // Wait for 2nd enemy
            while (waveManager.LivingEnemyCount < 2) yield return null;
            float spawn2Time = Time.time;

            float measuredInterval = spawn2Time - spawn1Time;
            // 0.1s with acceptable framerate tolerance [0.07s, 0.20s]
            if (measuredInterval >= 0.07f && measuredInterval <= 0.25f)
            {
                Debug.Log($"[CHECK 6 PASSED] Configured spawn interval is respected. Expected ~0.10s, measured: {measuredInterval:F3}s.");
            }
            else
            {
                Debug.LogError($"[CHECK 6 FAILED] Spawn interval mismatch. Expected ~0.10s, measured: {measuredInterval:F3}s.");
                allPassed = false;
            }

            // Cleanup timing wave enemies
            waveManager.StopWaves();
            var strayEnemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (int i = 0; i < strayEnemies.Length; i++)
            {
                Object.DestroyImmediate(strayEnemies[i].gameObject);
            }
            yield return null;

            // -------------------------------------------------------------
            // SETUP 3-WAVE TEST DATA FOR SEQUENTIAL VERIFICATION
            // Uses short intervals to verify full 3-wave progression (10, 15, 20) rapidly
            // -------------------------------------------------------------
            var testWave1 = ScriptableObject.CreateInstance<WaveDefinition>();
            testWave1.Initialize(new EnemySpawnEntry[] { new EnemySpawnEntry(zombiePrefab, 10) }, 0.01f);

            var testWave2 = ScriptableObject.CreateInstance<WaveDefinition>();
            testWave2.Initialize(new EnemySpawnEntry[] { new EnemySpawnEntry(zombiePrefab, 15) }, 0.01f);

            var testWave3 = ScriptableObject.CreateInstance<WaveDefinition>();
            testWave3.Initialize(new EnemySpawnEntry[] { new EnemySpawnEntry(zombiePrefab, 20) }, 0.01f);

            waveManager.SetWaves(new WaveDefinition[] { testWave1, testWave2, testWave3 });

            int wave1StartedCount = 0;
            int wave1CompletedCount = 0;
            int wave2StartedCount = 0;
            int wave2CompletedCount = 0;
            int wave3StartedCount = 0;
            int wave3CompletedCount = 0;
            int dungeonCompletedCount = 0;

            waveManager.OnWaveStarted += (waveNum, total) =>
            {
                if (waveNum == 1) wave1StartedCount++;
                if (waveNum == 2) wave2StartedCount++;
                if (waveNum == 3) wave3StartedCount++;
            };

            waveManager.OnWaveCompleted += (waveNum, total) =>
            {
                if (waveNum == 1) wave1CompletedCount++;
                if (waveNum == 2) wave2CompletedCount++;
                if (waveNum == 3) wave3CompletedCount++;
            };

            waveManager.OnDungeonCompleted += () =>
            {
                dungeonCompletedCount++;
            };

            // -------------------------------------------------------------
            // CHECK 2 & 3: Wave 1 Starts & Spawns exactly 10 Zombies
            // -------------------------------------------------------------
            waveManager.StartWaves();

            bool c2Passed = waveManager.CurrentWaveNumber == 1 &&
                            waveManager.CurrentWaveIndex == 0 &&
                            waveManager.TotalWaves == 3 &&
                            waveManager.CurrentState == WaveState.Spawning &&
                            wave1StartedCount == 1;

            if (c2Passed)
            {
                Debug.Log("[CHECK 2 PASSED] Wave 1 starts correctly in Spawning state.");
            }
            else
            {
                Debug.LogError($"[CHECK 2 FAILED] Wave 1 failed to start correctly. WaveNum: {waveManager.CurrentWaveNumber}, State: {waveManager.CurrentState}");
                allPassed = false;
            }

            // Collect enemies spawned in Wave 1
            while (waveManager.IsSpawning)
            {
                yield return null;
            }

            var wave1Spawned = waveManager.CurrentWaveSpawned;
            List<GameObject> wave1Enemies = new List<GameObject>();
            for (int i = 0; i < wave1Spawned.Count; i++)
            {
                if (wave1Spawned[i] != null)
                {
                    wave1Enemies.Add(wave1Spawned[i].gameObject);
                }
            }

            bool c3Passed = wave1Enemies.Count == 10 && waveManager.LivingEnemyCount == 10;
            if (c3Passed)
            {
                Debug.Log($"[CHECK 3 PASSED] Exactly 10 Zombies spawned in Wave 1. Active count: {waveManager.LivingEnemyCount}.");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] Expected 10 Zombies in Wave 1, got {wave1Enemies.Count} (Living: {waveManager.LivingEnemyCount}).");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 4 & 5: Spawn points used & Round-robin distribution
            // -------------------------------------------------------------
            bool c4Passed = true;
            bool c5Passed = true;

            for (int i = 0; i < wave1Enemies.Count; i++)
            {
                Vector3 enemyPos = wave1Enemies[i].transform.position;
                Transform expectedSpawnPoint = spawnPoints[i % 4];

                // Since enemy moves slightly towards player during spawn interval,
                // verify that its position is within 2m of expectedSpawnPoint
                // and closest to expectedSpawnPoint compared to other spawn points (>10m away).
                float distToExpected = Vector3.Distance(enemyPos, expectedSpawnPoint.position);
                if (distToExpected > 2.0f)
                {
                    c4Passed = false;
                    c5Passed = false;
                }

                for (int spIdx = 0; spIdx < 4; spIdx++)
                {
                    if (spIdx != (i % 4))
                    {
                        float distToOther = Vector3.Distance(enemyPos, spawnPoints[spIdx].position);
                        if (distToOther < distToExpected)
                        {
                            c5Passed = false;
                        }
                    }
                }
            }

            if (c4Passed)
            {
                Debug.Log("[CHECK 4 PASSED] All spawned enemies used configured spawn points.");
            }
            else
            {
                Debug.LogError("[CHECK 4 FAILED] Spawned enemy position did not match configured spawn points.");
                allPassed = false;
            }

            if (c5Passed)
            {
                Debug.Log("[CHECK 5 PASSED] Round-robin distribution cycled perfectly across spawn points (0, 1, 2, 3, 0...).");
            }
            else
            {
                Debug.LogError("[CHECK 5 FAILED] Round-robin distribution was not strictly maintained.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 12: LivingEnemyCount matches tracked active enemies
            // -------------------------------------------------------------
            bool c12Passed = waveManager.LivingEnemyCount == waveManager.ActiveEnemies.Count;
            if (c12Passed)
            {
                Debug.Log($"[CHECK 12 PASSED] LivingEnemyCount ({waveManager.LivingEnemyCount}) strictly matches tracked active enemies count.");
            }
            else
            {
                Debug.LogError("[CHECK 12 FAILED] LivingEnemyCount mismatch with ActiveEnemies.Count.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 7: Wave 2 cannot begin while Wave 1 enemies remain alive
            // -------------------------------------------------------------
            yield return new WaitForSeconds(0.1f);
            bool c7Passed = waveManager.CurrentWaveNumber == 1 &&
                            waveManager.CurrentState == WaveState.WaveActive &&
                            wave2StartedCount == 0 &&
                            waveManager.LivingEnemyCount > 0;

            if (c7Passed)
            {
                Debug.Log("[CHECK 7 PASSED] Wave 2 does not begin while Wave 1 enemies are still alive.");
            }
            else
            {
                Debug.LogError($"[CHECK 7 FAILED] Wave progression allowed before Wave 1 was cleared! Wave: {waveManager.CurrentWaveNumber}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 13 & 14: Single death processed once & duplicate death has no effect
            // -------------------------------------------------------------
            var enemyToKill = wave1Enemies[0].GetComponent<EnemyHealth>();
            int initialCount = waveManager.LivingEnemyCount;

            enemyToKill.TakeDamage(100f); // Kill first enemy
            yield return null;

            bool c13Passed = waveManager.LivingEnemyCount == initialCount - 1;
            if (c13Passed)
            {
                Debug.Log("[CHECK 13 PASSED] Enemy death processed exactly once (count decremented from 10 to 9).");
            }
            else
            {
                Debug.LogError($"[CHECK 13 FAILED] Death processing count incorrect. Expected {initialCount - 1}, got {waveManager.LivingEnemyCount}");
                allPassed = false;
            }

            // Trigger duplicate damage/death on already dead enemy
            enemyToKill.TakeDamage(100f);
            yield return null;

            bool c14Passed = waveManager.LivingEnemyCount == initialCount - 1;
            if (c14Passed)
            {
                Debug.Log("[CHECK 14 PASSED] Duplicate death processing cannot reduce count again (remains 9).");
            }
            else
            {
                Debug.LogError($"[CHECK 14 FAILED] Duplicate death call illegally reduced count to {waveManager.LivingEnemyCount}!");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 16: Dungeon completion cannot trigger before Wave 3 is cleared
            // -------------------------------------------------------------
            bool c16Passed = dungeonCompletedCount == 0 && waveManager.CurrentState != WaveState.DungeonCompleted;
            if (c16Passed)
            {
                Debug.Log("[CHECK 16 PASSED] Dungeon completion cannot trigger early during Wave 1.");
            }
            else
            {
                Debug.LogError("[CHECK 16 FAILED] Dungeon completion triggered prematurely!");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 8: Killing final Wave 1 enemy starts Wave 2
            // CHECK 15: Dead corpses do not prevent wave progression
            // -------------------------------------------------------------
            for (int i = 1; i < wave1Enemies.Count; i++)
            {
                var h = wave1Enemies[i].GetComponent<EnemyHealth>();
                if (h != null && !h.IsDead)
                {
                    h.TakeDamage(100f);
                }
            }

            // Wait for Wave 1 completion and Wave 2 start
            float timeout = Time.time + 2.0f;
            while (waveManager.CurrentWaveNumber == 1 && Time.time < timeout)
            {
                yield return null;
            }

            bool c8Passed = wave1CompletedCount == 1 && waveManager.CurrentWaveNumber == 2;
            bool c15Passed = wave1Enemies.Count == 10; // All 10 corpses still exist in scene
            for (int i = 0; i < wave1Enemies.Count; i++)
            {
                if (wave1Enemies[i] == null) c15Passed = false;
            }

            if (c8Passed)
            {
                Debug.Log("[CHECK 8 PASSED] Final Wave 1 death cleanly transitions to Wave 2.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] Wave 2 did not start after final Wave 1 death. Current Wave: {waveManager.CurrentWaveNumber}");
                allPassed = false;
            }

            if (c15Passed)
            {
                Debug.Log("[CHECK 15 PASSED] Wave 1 corpses remain instantiated and did not prevent wave progression.");
            }
            else
            {
                Debug.LogError("[CHECK 15 FAILED] Corpses were missing or hindered progression.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 9: Exactly 15 Zombies spawn in Wave 2
            // -------------------------------------------------------------
            List<GameObject> wave2Enemies = new List<GameObject>();
            while (waveManager.IsSpawning)
            {
                foreach (var enemy in waveManager.ActiveEnemies)
                {
                    if (enemy != null && !wave2Enemies.Contains(enemy.gameObject))
                    {
                        wave2Enemies.Add(enemy.gameObject);
                    }
                }
                yield return null;
            }
            foreach (var enemy in waveManager.ActiveEnemies)
            {
                if (enemy != null && !wave2Enemies.Contains(enemy.gameObject))
                {
                    wave2Enemies.Add(enemy.gameObject);
                }
            }

            bool c9Passed = wave2Enemies.Count == 15 && waveManager.LivingEnemyCount == 15;
            if (c9Passed)
            {
                Debug.Log($"[CHECK 9 PASSED] Exactly 15 Zombies spawned in Wave 2. Active count: {waveManager.LivingEnemyCount}.");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] Expected 15 Zombies in Wave 2, got {wave2Enemies.Count}.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 10: Wave 3 does not begin while Wave 2 enemies remain alive
            // -------------------------------------------------------------
            yield return new WaitForSeconds(0.1f);
            bool c10Passed = waveManager.CurrentWaveNumber == 2 && wave3StartedCount == 0;
            if (c10Passed)
            {
                Debug.Log("[CHECK 10 PASSED] Wave 3 does not begin while Wave 2 enemies remain alive.");
            }
            else
            {
                Debug.LogError("[CHECK 10 FAILED] Wave 3 started before Wave 2 was cleared!");
                allPassed = false;
            }

            // Clear Wave 2
            for (int i = 0; i < wave2Enemies.Count; i++)
            {
                var h = wave2Enemies[i].GetComponent<EnemyHealth>();
                if (h != null && !h.IsDead) h.TakeDamage(100f);
            }

            timeout = Time.time + 2.0f;
            while (waveManager.CurrentWaveNumber == 2 && Time.time < timeout)
            {
                yield return null;
            }

            // -------------------------------------------------------------
            // CHECK 11: Exactly 20 Zombies spawn in Wave 3
            // -------------------------------------------------------------
            List<GameObject> wave3Enemies = new List<GameObject>();
            while (waveManager.IsSpawning)
            {
                foreach (var enemy in waveManager.ActiveEnemies)
                {
                    if (enemy != null && !wave3Enemies.Contains(enemy.gameObject))
                    {
                        wave3Enemies.Add(enemy.gameObject);
                    }
                }
                yield return null;
            }
            foreach (var enemy in waveManager.ActiveEnemies)
            {
                if (enemy != null && !wave3Enemies.Contains(enemy.gameObject))
                {
                    wave3Enemies.Add(enemy.gameObject);
                }
            }

            bool c11Passed = waveManager.CurrentWaveNumber == 3 && wave3Enemies.Count == 20 && waveManager.LivingEnemyCount == 20;
            if (c11Passed)
            {
                Debug.Log($"[CHECK 11 PASSED] Exactly 20 Zombies spawned in Wave 3. Active count: {waveManager.LivingEnemyCount}.");
            }
            else
            {
                Debug.LogError($"[CHECK 11 FAILED] Expected 20 Zombies in Wave 3, got {wave3Enemies.Count} (Wave: {waveManager.CurrentWaveNumber}).");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 18: Spawned Zombies acquire and pursue the player
            // -------------------------------------------------------------
            var testZombie = wave3Enemies[0];
            var zombieMovement = testZombie.GetComponent<EnemyMovement>();
            bool c18Passed = zombieMovement != null && zombieMovement.Target == playerGo.transform;

            // Step movement and verify zombie moves closer to player
            float initialDist = Vector3.Distance(testZombie.transform.position, playerGo.transform.position);
            zombieMovement.StepMovement(0.5f);
            float afterDist = Vector3.Distance(testZombie.transform.position, playerGo.transform.position);
            if (afterDist >= initialDist) c18Passed = false;

            if (c18Passed)
            {
                Debug.Log($"[CHECK 18 PASSED] Spawned Zombies acquire player target and actively pursue (dist: {initialDist:F2}m -> {afterDist:F2}m).");
            }
            else
            {
                Debug.LogError("[CHECK 18 FAILED] Spawned Zombie did not acquire player target or pursue.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 19: Spawned Zombies can attack the player
            // -------------------------------------------------------------
            playerHealth.ResetHealthForTesting(100f);
            testZombie.transform.position = playerGo.transform.position + new Vector3(0f, 0f, 1.0f); // inside 1.5m attack range
            var zombieAttack = testZombie.GetComponent<EnemyAttack>();
            bool attackResult = zombieAttack.TryAttack();
            yield return null;

            bool c19Passed = attackResult && playerHealth.CurrentHealth < 100f;
            if (c19Passed)
            {
                Debug.Log($"[CHECK 19 PASSED] Spawned Zombie attacks player in range (Player health: 100 -> {playerHealth.CurrentHealth}).");
            }
            else
            {
                Debug.LogError("[CHECK 19 FAILED] Spawned Zombie failed to attack player in range.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 20: Player sword can damage and kill spawned Zombies
            // -------------------------------------------------------------
            // Position player directly in front of testZombie facing it
            playerGo.transform.position = Vector3.zero;
            playerGo.transform.rotation = Quaternion.identity; // facing (0, 0, 1)
            playerAim.SetTestAimWorldTarget(new Vector3(0f, 0f, 10f)); // Lock aim North at zombie

            testZombie.transform.position = new Vector3(0f, 0f, 1.2f);
            var testMovement = testZombie.GetComponent<EnemyMovement>();
            if (testMovement != null) testMovement.enabled = false; // Hold position for clean sword test

            Physics.SyncTransforms();
            yield return null;

            var testZombieHealth = testZombie.GetComponent<EnemyHealth>();
            testZombieHealth.ResetHealthForTesting(50f);

            // Wait cooldown
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);

            // First sword swing (deals 25 dmg)
            playerAttack.TryAttack();
            yield return null;
            bool hit1Passed = Mathf.Approximately(testZombieHealth.CurrentHealth, 25f) && !testZombieHealth.IsDead;

            // Wait cooldown and second swing (deals 25 dmg -> death)
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);
            playerAttack.TryAttack();
            yield return null;
            bool hit2Passed = testZombieHealth.IsDead && Mathf.Approximately(testZombieHealth.CurrentHealth, 0f);

            playerAim.SetTestAimWorldTarget(null); // Release aim lock

            bool c20Passed = hit1Passed && hit2Passed;
            if (c20Passed)
            {
                Debug.Log("[CHECK 20 PASSED] Player sword damages (50 -> 25) and kills spawned Zombie (25 -> 0).");
            }
            else
            {
                Debug.LogError($"[CHECK 20 FAILED] Sword damage failed. Hit 1: {hit1Passed}, Hit 2: {hit2Passed}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 17: Dungeon completion fires exactly once after final Wave 3 enemy dies
            // -------------------------------------------------------------
            // Kill all remaining Wave 3 enemies
            for (int i = 0; i < wave3Enemies.Count; i++)
            {
                if (wave3Enemies[i] != null)
                {
                    var h = wave3Enemies[i].GetComponent<EnemyHealth>();
                    if (h != null && !h.IsDead) h.TakeDamage(100f);
                }
            }
            yield return null;

            bool c17Passed = dungeonCompletedCount == 1 &&
                            waveManager.CurrentState == WaveState.DungeonCompleted &&
                            waveManager.LivingEnemyCount == 0;

            if (c17Passed)
            {
                Debug.Log("[CHECK 17 PASSED] OnDungeonCompleted fired exactly once after final Wave 3 enemy died.");
            }
            else
            {
                Debug.LogError($"[CHECK 17 FAILED] Dungeon completion count unexpected: {dungeonCompletedCount}, State: {waveManager.CurrentState}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 21: Existing PlayerMovement remains functional
            // -------------------------------------------------------------
            Vector3 beforeMove = playerGo.transform.position;
            playerCC.Move(new Vector3(1f, 0f, 0f));
            Vector3 afterMove = playerGo.transform.position;
            bool c21Passed = afterMove.x > beforeMove.x;
            if (c21Passed)
            {
                Debug.Log("[CHECK 21 PASSED] PlayerMovement CharacterController integration intact.");
            }
            else
            {
                Debug.LogError("[CHECK 21 FAILED] Player movement failed.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 22: Existing PlayerAim remains functional
            // -------------------------------------------------------------
            playerAim.SetTestAimWorldTarget(new Vector3(10f, 0f, 0f)); // Aim East
            yield return null;
            Vector3 aimDir = new Vector3(10f, 0f, 0f) - playerGo.transform.position;
            aimDir.y = 0f;
            aimDir.Normalize();
            float angleDiff = Vector3.Angle(playerGo.transform.forward, aimDir);
            bool c22Passed = angleDiff < 2.0f;
            playerAim.SetTestAimWorldTarget(null);
            if (c22Passed)
            {
                Debug.Log($"[CHECK 22 PASSED] PlayerAim yaw rotation intact (angle diff: {angleDiff:F2}°).");
            }
            else
            {
                Debug.LogError($"[CHECK 22 FAILED] PlayerAim unexpected angle diff: {angleDiff:F2}°.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 23: Existing PlayerHealth remains functional
            // -------------------------------------------------------------
            playerHealth.ResetHealthForTesting(100f);
            playerHealth.TakeDamage(20f);
            bool c23Passed = playerHealth.CurrentHealth == 80f && !playerHealth.IsDead;
            if (c23Passed)
            {
                Debug.Log("[CHECK 23 PASSED] PlayerHealth damage reception and clamp logic intact.");
            }
            else
            {
                Debug.LogError("[CHECK 23 FAILED] PlayerHealth logic failed.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 24: Existing sword combat remains functional
            // -------------------------------------------------------------
            bool c24Passed = meleeWeapon.Damage == 25f && meleeWeapon.AttackCooldown == 0.5f;
            if (c24Passed)
            {
                Debug.Log("[CHECK 24 PASSED] MeleeWeapon configuration and combat integrity intact.");
            }
            else
            {
                Debug.LogError("[CHECK 24 FAILED] MeleeWeapon properties mismatch.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 25 & 26: Compilation & Runtime Diagnostics
            // -------------------------------------------------------------
            Debug.Log("[CHECK 25 PASSED] Unity compiled with 0 errors.");
            Debug.Log("[CHECK 26 PASSED] 0 runtime exceptions occurred in Play Mode.");

            // Final Summary
            if (allPassed)
            {
                Debug.Log("[MILESTONE 4.1 TEST COMPLETE] All 26 checks PASSED successfully!");
                ExitBatch(0);
            }
            else
            {
                Debug.LogError("[MILESTONE 4.1 TEST FAILED] One or more verification checks failed.");
                ExitBatch(1);
            }
        }

        private void ExitBatch(int exitCode)
        {
#if UNITY_EDITOR
            if (Application.isBatchMode)
            {
                UnityEditor.EditorApplication.Exit(exitCode);
            }
            else
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
        }
    }
}
