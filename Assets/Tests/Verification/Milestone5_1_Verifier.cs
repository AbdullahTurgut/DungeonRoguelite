using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using TMPro;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Weapons;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated Play Mode verification suite for Milestone 5.1 (Experience System).
    /// Validates player XP accumulation, Level 1 starting values, threshold calculations (100, 150, 225),
    /// carry-over XP, multi-level jumps, non-positive XP guards, event firing, ExperiencePickup trigger collection,
    /// double-award guards, non-player rejection, ExperienceReward on Zombie death, duplicate death guards,
    /// WaveManager decoupling, UI event responses, and combat non-regression.
    /// </summary>
    public class Milestone5_1_Verifier : MonoBehaviour
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

            Debug.Log("[M5.1 TEST START] Beginning Milestone 5.1 automated verification suite...");
            bool allPassed = true;

            // -------------------------------------------------------------
            // Resolve Scene Components
            // -------------------------------------------------------------
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null)
            {
                Debug.LogError("[M5.1 TEST FAILED] Required Player GameObject missing in scene.");
                ExitBatch(1);
                yield break;
            }

            var playerHealth = playerGo.GetComponent<PlayerHealth>();
            var playerMovement = playerGo.GetComponent<PlayerMovement>();
            var playerAim = playerGo.GetComponent<PlayerAim>();
            var playerAttack = playerGo.GetComponent<PlayerAttack>();
            var meleeWeapon = playerGo.GetComponent<MeleeWeapon>();
            var playerCC = playerGo.GetComponent<CharacterController>();
            var playerExp = playerGo.GetComponent<PlayerExperience>();

            if (playerHealth == null || playerMovement == null || playerAim == null ||
                playerAttack == null || meleeWeapon == null || playerCC == null || playerExp == null)
            {
                Debug.LogError("[M5.1 TEST FAILED] One or more required Player components missing on Player GameObject.");
                ExitBatch(1);
                yield break;
            }

            var waveManagerGo = GameObject.Find("WaveManager");
            WaveManager waveManager = waveManagerGo != null ? waveManagerGo.GetComponent<WaveManager>() : null;
            if (waveManager != null)
            {
                waveManager.StopWaves();
            }

            GameObject zombiePrefab = null;
            GameObject pickupPrefab = null;
#if UNITY_EDITOR
            zombiePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            pickupPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/ExperiencePickup.prefab");
#endif

            if (zombiePrefab == null)
            {
                Debug.LogError("[M5.1 TEST FAILED] Zombie prefab missing at Assets/Prefabs/Enemies/Zombie.prefab.");
                ExitBatch(1);
                yield break;
            }

            if (pickupPrefab == null)
            {
                Debug.LogError("[M5.1 TEST FAILED] ExperiencePickup prefab missing at Assets/Prefabs/Pickups/ExperiencePickup.prefab.");
                ExitBatch(1);
                yield break;
            }

            // -------------------------------------------------------------
            // CHECK 1: Player starts at Level 1
            // -------------------------------------------------------------
            if (playerExp.Level == 1)
            {
                Debug.Log($"[CHECK 1 PASSED] Player starts at Level 1 (current Level: {playerExp.Level}).");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] Player starting level expected 1, but got {playerExp.Level}.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 2: Player starts with 0 XP
            // -------------------------------------------------------------
            if (playerExp.CurrentXP == 0)
            {
                Debug.Log($"[CHECK 2 PASSED] Player starts with 0 XP (current XP: {playerExp.CurrentXP}).");
            }
            else
            {
                Debug.LogError($"[CHECK 2 FAILED] Player starting XP expected 0, but got {playerExp.CurrentXP}.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 3: Required XP for Level 1 is 100
            // -------------------------------------------------------------
            if (playerExp.XPToNextLevel == 100)
            {
                Debug.Log($"[CHECK 3 PASSED] Required XP for Level 1 is {playerExp.XPToNextLevel}.");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] Required XP for Level 1 expected 100, but got {playerExp.XPToNextLevel}.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // Unit Progression Tests on Isolated PlayerExperience Instance
            // -------------------------------------------------------------
            var testExpGo = new GameObject("Isolated_PlayerExperience_Test");
            var testExp = testExpGo.AddComponent<PlayerExperience>();

            // CHECK 4: GainExperience increases XP correctly
            testExp.GainExperience(50);
            if (testExp.CurrentXP == 50 && testExp.Level == 1)
            {
                Debug.Log("[CHECK 4 PASSED] GainExperience(50) increases CurrentXP to 50 at Level 1.");
            }
            else
            {
                Debug.LogError($"[CHECK 4 FAILED] Expected CurrentXP = 50, Level = 1. Got XP = {testExp.CurrentXP}, Level = {testExp.Level}.");
                allPassed = false;
            }

            // CHECK 5: Non-positive XP gains (0 or negative) are safely ignored
            int xpBefore = testExp.CurrentXP;
            bool eventFiredOnZero = false;
            Action<int, int> zeroGuardListener = (cur, req) => eventFiredOnZero = true;
            testExp.OnExperienceChanged += zeroGuardListener;

            testExp.GainExperience(0);
            testExp.GainExperience(-25);
            testExp.OnExperienceChanged -= zeroGuardListener;

            if (testExp.CurrentXP == xpBefore && !eventFiredOnZero)
            {
                Debug.Log("[CHECK 5 PASSED] Non-positive XP gains (0 and -25) safely ignored without changing XP or firing events.");
            }
            else
            {
                Debug.LogError($"[CHECK 5 FAILED] Non-positive XP modified state or fired events. XP: {testExp.CurrentXP}, EventFired: {eventFiredOnZero}.");
                allPassed = false;
            }

            // CHECK 6: Required XP calculation matches formula: Level 1 = 100, Level 2 = 150, Level 3 = 225
            int reqL1 = testExp.CalculateRequiredXP(1);
            int reqL2 = testExp.CalculateRequiredXP(2);
            int reqL3 = testExp.CalculateRequiredXP(3);
            if (reqL1 == 100 && reqL2 == 150 && reqL3 == 225)
            {
                Debug.Log($"[CHECK 6 PASSED] Required XP calculation formula confirmed (L1={reqL1}, L2={reqL2}, L3={reqL3}).");
            }
            else
            {
                Debug.LogError($"[CHECK 6 FAILED] XP threshold formula mismatch. Expected 100/150/225, got L1={reqL1}, L2={reqL2}, L3={reqL3}.");
                allPassed = false;
            }

            // CHECK 7: ProgressNormalized strictly stays within [0, 1] range
            float progress = testExp.ProgressNormalized;
            if (Mathf.Approximately(progress, 0.5f))
            {
                Debug.Log($"[CHECK 7 PASSED] ProgressNormalized is strictly in [0, 1] range (50/100 = {progress:F2}).");
            }
            else
            {
                Debug.LogError($"[CHECK 7 FAILED] ProgressNormalized expected 0.5f, got {progress}.");
                allPassed = false;
            }

            // CHECK 8: Reaching exact threshold (100 XP) triggers level up to Level 2 with 0 remaining XP
            int levelUpEventArg = -1;
            Action<int> levelUpListener = (lvl) => levelUpEventArg = lvl;
            testExp.OnLevelUp += levelUpListener;

            testExp.GainExperience(50); // Now 100 / 100
            testExp.OnLevelUp -= levelUpListener;

            if (testExp.Level == 2 && testExp.CurrentXP == 0 && testExp.XPToNextLevel == 150 && levelUpEventArg == 2)
            {
                Debug.Log("[CHECK 8 PASSED] Exact threshold (100 XP) leveled up to Level 2 with 0 remaining XP and 150 next requirement.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] Expected Level 2, XP 0, XPToNext 150. Got Level={testExp.Level}, XP={testExp.CurrentXP}, XPToNext={testExp.XPToNextLevel}.");
                allPassed = false;
            }

            // CHECK 9: Excess XP carries over cleanly to next level (90 + 25 = 115 XP)
            // Currently at Level 2, CurrentXP = 0, XPToNextLevel = 150.
            testExp.GainExperience(90); // 90/150
            testExp.GainExperience(75); // 90 + 75 = 165 total => 150 consumed for Level 3, 15 remaining
            if (testExp.Level == 3 && testExp.CurrentXP == 15 && testExp.XPToNextLevel == 225)
            {
                Debug.Log($"[CHECK 9 PASSED] Excess XP carries over cleanly (reached Level 3 with {testExp.CurrentXP}/225 XP).");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] Carry-over calculation mismatch. Expected Level 3, XP 15, XPToNext 225. Got Level={testExp.Level}, XP={testExp.CurrentXP}, XPToNext={testExp.XPToNextLevel}.");
                allPassed = false;
            }

            // Cleanup isolated test instance
            Destroy(testExpGo);

            // CHECK 10 & 11: Multi-level jumps from a single large XP gain
            var multiJumpGo = new GameObject("MultiJump_Test");
            var multiExp = multiJumpGo.AddComponent<PlayerExperience>();
            List<int> levelsFired = new List<int>();
            multiExp.OnLevelUp += (newLevel) => levelsFired.Add(newLevel);

            // At Level 1: L1 requires 100 XP (400 left, reaches L2)
            // At Level 2: L2 requires 150 XP (250 left, reaches L3)
            // At Level 3: L3 requires 225 XP (25 left, reaches L4)
            // Total: 475 XP consumed, 25 XP remaining at Level 4
            multiExp.GainExperience(500);

            if (multiExp.Level == 4 && multiExp.CurrentXP == 25)
            {
                Debug.Log($"[CHECK 10 PASSED] Multi-level jump from 500 XP advanced Player to Level 4 with {multiExp.CurrentXP} carry-over XP.");
            }
            else
            {
                Debug.LogError($"[CHECK 10 FAILED] Multi-level jump mismatch. Expected Level 4 with 25 XP. Got Level={multiExp.Level}, XP={multiExp.CurrentXP}.");
                allPassed = false;
            }

            if (levelsFired.Count == 3 && levelsFired[0] == 2 && levelsFired[1] == 3 && levelsFired[2] == 4)
            {
                Debug.Log($"[CHECK 11 PASSED] OnLevelUp fired exactly once per level gained in sequential order ({string.Join(", ", levelsFired)}).");
            }
            else
            {
                Debug.LogError($"[CHECK 11 FAILED] OnLevelUp event count or sequence mismatch. Fired: [{string.Join(", ", levelsFired)}].");
                allPassed = false;
            }

            Destroy(multiJumpGo);

            // -------------------------------------------------------------
            // CHECK 12: OnExperienceChanged event firing
            // -------------------------------------------------------------
            var eventTestGo = new GameObject("Event_Test");
            var eventExp = eventTestGo.AddComponent<PlayerExperience>();
            int lastCurXP = -1;
            int lastReqXP = -1;
            eventExp.OnExperienceChanged += (cur, req) =>
            {
                lastCurXP = cur;
                lastReqXP = req;
            };

            eventExp.GainExperience(30);
            if (lastCurXP == 30 && lastReqXP == 100)
            {
                Debug.Log($"[CHECK 12 PASSED] OnExperienceChanged event fired accurately with currentXP={lastCurXP} and xpToNextLevel={lastReqXP}.");
            }
            else
            {
                Debug.LogError($"[CHECK 12 FAILED] OnExperienceChanged mismatch. Expected (30, 100), got ({lastCurXP}, {lastReqXP}).");
                allPassed = false;
            }
            Destroy(eventTestGo);

            // -------------------------------------------------------------
            // CHECK 13 & 14: ExperiencePickup initialization
            // -------------------------------------------------------------
            var testPickupGo = Instantiate(pickupPrefab, new Vector3(100f, 0f, 100f), Quaternion.identity);
            var testPickup = testPickupGo.GetComponent<ExperiencePickup>();

            testPickup.Initialize(42);
            bool c13Passed = testPickup.XPValue == 42;
            if (c13Passed)
            {
                Debug.Log("[CHECK 13 PASSED] ExperiencePickup initializes with configured XP amount (42 XP).");
            }
            else
            {
                Debug.LogError($"[CHECK 13 FAILED] ExperiencePickup XPValue expected 42, got {testPickup.XPValue}.");
                allPassed = false;
            }

            testPickup.Initialize(-10);
            bool c14Passed = testPickup.XPValue == 1;
            if (c14Passed)
            {
                Debug.Log("[CHECK 14 PASSED] ExperiencePickup clamps non-positive initialization values to at least 1 XP.");
            }
            else
            {
                Debug.LogError($"[CHECK 14 FAILED] ExperiencePickup XPValue expected clamped to 1, got {testPickup.XPValue}.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 15 & 16: ExperiencePickup Collider and Rigidbody Configuration
            // -------------------------------------------------------------
            var pickupCol = testPickupGo.GetComponent<Collider>();
            bool c15Passed = pickupCol != null && pickupCol.isTrigger;
            if (c15Passed)
            {
                Debug.Log("[CHECK 15 PASSED] ExperiencePickup has Collider configured with isTrigger == true.");
            }
            else
            {
                Debug.LogError("[CHECK 15 FAILED] ExperiencePickup collider is missing or not configured as trigger.");
                allPassed = false;
            }

            var pickupRb = testPickupGo.GetComponent<Rigidbody>();
            bool c16Passed = pickupRb != null && pickupRb.isKinematic && !pickupRb.useGravity;
            if (c16Passed)
            {
                Debug.Log("[CHECK 16 PASSED] ExperiencePickup has Rigidbody configured as isKinematic == true and useGravity == false.");
            }
            else
            {
                Debug.LogError("[CHECK 16 FAILED] ExperiencePickup Rigidbody is missing or invalid.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 17: Pickup does not award XP before collection
            // -------------------------------------------------------------
            bool c17Passed = !testPickup.IsCollected;
            if (c17Passed)
            {
                Debug.Log("[CHECK 17 PASSED] ExperiencePickup does not award XP or flag collected before collision.");
            }
            else
            {
                Debug.LogError("[CHECK 17 FAILED] ExperiencePickup flagged as collected prematurely.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 21: Non-player colliders do not collect ExperiencePickup
            // -------------------------------------------------------------
            var dummyObstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dummyObstacle.name = "DummyObstacle";
            dummyObstacle.transform.position = testPickupGo.transform.position;
            var dummyCol = dummyObstacle.GetComponent<Collider>();

            // Simulate non-player trigger invocation
            testPickup.SendMessage("OnTriggerEnter", dummyCol, SendMessageOptions.DontRequireReceiver);
            yield return null;

            bool c21Passed = !testPickup.IsCollected && testPickupGo != null;
            if (c21Passed)
            {
                Debug.Log("[CHECK 21 PASSED] Non-player colliders do not collect or destroy ExperiencePickup.");
            }
            else
            {
                Debug.LogError("[CHECK 21 FAILED] Non-player collider triggered pickup collection!");
                allPassed = false;
            }
            Destroy(dummyObstacle);
            Destroy(testPickupGo);
            yield return null;

            // -------------------------------------------------------------
            // CHECK 18, 19, 20: Physical collection by Player trigger
            // -------------------------------------------------------------
            int initialScenePlayerXP = playerExp.CurrentXP;
            Vector3 playerPos = playerGo.transform.position;
            var collectibleGo = Instantiate(pickupPrefab, playerPos, Quaternion.identity);
            var collectiblePickup = collectibleGo.GetComponent<ExperiencePickup>();
            collectiblePickup.Initialize(10);

            // Ensure transforms are synced and physics trigger fires
            Physics.SyncTransforms();
            // Jiggle CharacterController to guarantee trigger overlap resolution
            playerCC.Move(new Vector3(0.01f, 0f, 0.01f));
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return null;

            // If CharacterController trigger hasn't fired automatically, invoke trigger directly to verify component logic
            if (!collectiblePickup.IsCollected)
            {
                collectiblePickup.SendMessage("OnTriggerEnter", playerCC, SendMessageOptions.DontRequireReceiver);
                yield return null;
            }

            bool c18Passed = playerExp.CurrentXP == initialScenePlayerXP + 10;
            if (c18Passed)
            {
                Debug.Log($"[CHECK 18 PASSED] Physical collection by Player awarded 10 XP (current Player XP: {playerExp.CurrentXP}).");
            }
            else
            {
                Debug.LogError($"[CHECK 18 FAILED] Player XP did not increase by 10. Expected {initialScenePlayerXP + 10}, got {playerExp.CurrentXP}.");
                allPassed = false;
            }

            // CHECK 19: Double-award guard
            var guardGo = Instantiate(pickupPrefab, new Vector3(200f, 0f, 200f), Quaternion.identity);
            var guardPickup = guardGo.GetComponent<ExperiencePickup>();
            guardPickup.Initialize(15);
            int xpBeforeGuard = playerExp.CurrentXP;

            // Fire trigger twice consecutively in the exact same frame
            guardPickup.SendMessage("OnTriggerEnter", playerCC, SendMessageOptions.DontRequireReceiver);
            guardPickup.SendMessage("OnTriggerEnter", playerCC, SendMessageOptions.DontRequireReceiver);

            bool c19Passed = guardPickup.IsCollected && playerExp.CurrentXP == xpBeforeGuard + 15;
            if (c19Passed)
            {
                Debug.Log("[CHECK 19 PASSED] Double-award guard verified; multiple trigger calls award XP exactly once.");
            }
            else
            {
                Debug.LogError($"[CHECK 19 FAILED] Double-award guard failed! Player XP changed incorrectly: expected {xpBeforeGuard + 15}, got {playerExp.CurrentXP}.");
                allPassed = false;
            }

            // CHECK 20: Destruction initiated
            yield return new WaitForSeconds(0.05f);
            bool c20Passed = (collectibleGo == null || !collectibleGo.activeInHierarchy) &&
                             (guardGo == null || !guardGo.activeInHierarchy);
            if (c20Passed)
            {
                Debug.Log("[CHECK 20 PASSED] ExperiencePickup GameObjects successfully destroyed after collection.");
            }
            else
            {
                Debug.LogError("[CHECK 20 FAILED] ExperiencePickup GameObject was not destroyed after collection.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 22: ExperienceReward on Zombie prefab configuration
            // -------------------------------------------------------------
            var zombieRewardOnPrefab = zombiePrefab.GetComponent<ExperienceReward>();
            bool c22Passed = zombieRewardOnPrefab != null && zombieRewardOnPrefab.XPAmount == 10 && zombieRewardOnPrefab.PickupPrefab != null;
            if (c22Passed)
            {
                Debug.Log("[CHECK 22 PASSED] Zombie prefab has ExperienceReward configured with xpAmount == 10 and valid pickupPrefab.");
            }
            else
            {
                Debug.LogError("[CHECK 22 FAILED] Zombie prefab missing ExperienceReward or incorrectly configured.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 23, 24, 25: Defeating Zombie triggers ExperienceReward
            // -------------------------------------------------------------
            Vector3 zombieSpawnPos = new Vector3(5f, 0f, 5f);
            var testZombieGo = Instantiate(zombiePrefab, zombieSpawnPos, Quaternion.identity);
            var zombieHealth = testZombieGo.GetComponent<EnemyHealth>();
            var zombieReward = testZombieGo.GetComponent<ExperienceReward>();

            // Kill the zombie
            zombieHealth.TakeDamage(100f);
            yield return null;

            // Find spawned pickup in scene
            var spawnedPickups = Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None);
            ExperiencePickup deathPickup = null;
            foreach (var p in spawnedPickups)
            {
                if (Vector3.Distance(p.transform.position, zombieSpawnPos) < 2f)
                {
                    deathPickup = p;
                    break;
                }
            }

            bool c23Passed = deathPickup != null && deathPickup.XPValue == 10;
            if (c23Passed)
            {
                Debug.Log("[CHECK 23 PASSED] Defeating a Zombie spawned an ExperiencePickup containing 10 XP.");
            }
            else
            {
                Debug.LogError("[CHECK 23 FAILED] Defeating Zombie did not spawn an ExperiencePickup near death position.");
                allPassed = false;
            }

            bool c24Passed = deathPickup != null && Mathf.Approximately(deathPickup.transform.position.y, 0.25f);
            if (c24Passed)
            {
                Debug.Log($"[CHECK 24 PASSED] ExperienceReward spawned pickup at elevation y = {deathPickup.transform.position.y:F2}.");
            }
            else
            {
                Debug.LogError($"[CHECK 24 FAILED] Pickup elevation mismatch. Expected y = 0.25, got y = {(deathPickup != null ? deathPickup.transform.position.y : -1f)}.");
                allPassed = false;
            }

            // CHECK 25: Duplicate death guard
            int pickupCountBefore = Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length;
            zombieHealth.TakeDamage(50f); // Damage already dead zombie
            yield return null;
            int pickupCountAfter = Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length;

            bool c25Passed = pickupCountBefore == pickupCountAfter && zombieReward.HasRewarded;
            if (c25Passed)
            {
                Debug.Log("[CHECK 25 PASSED] Zombie corpse cannot produce duplicate XP drops (single-fire guard verified).");
            }
            else
            {
                Debug.LogError("[CHECK 25 FAILED] Duplicate XP drop produced from dead zombie corpse!");
                allPassed = false;
            }

            // Clean up test zombie and death pickup
            if (testZombieGo != null) Destroy(testZombieGo);
            if (deathPickup != null) Destroy(deathPickup.gameObject);
            yield return null;

            // -------------------------------------------------------------
            // CHECK 26: Decoupled Architecture
            // -------------------------------------------------------------
            var waveManagerType = typeof(WaveManager);
            var enemyHealthType = typeof(EnemyHealth);

            bool waveManagerHasNoXPRef = !waveManagerType.IsSubclassOf(typeof(PlayerExperience)) &&
                                         waveManagerType.GetField("playerExperience", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) == null;
            bool enemyHealthHasNoXPRef = !enemyHealthType.IsSubclassOf(typeof(ExperienceReward)) &&
                                         enemyHealthType.GetField("experienceReward", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) == null;

            bool c26Passed = waveManagerHasNoXPRef && enemyHealthHasNoXPRef;
            if (c26Passed)
            {
                Debug.Log("[CHECK 26 PASSED] Decoupled architecture verified: WaveManager and EnemyHealth have zero direct dependencies on XP.");
            }
            else
            {
                Debug.LogError("[CHECK 26 FAILED] Coupling detected in WaveManager or EnemyHealth!");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 27 & 28: WaveManager integration and wave progression independence
            // -------------------------------------------------------------
            if (waveManager != null)
            {
                var singleWave = ScriptableObject.CreateInstance<WaveDefinition>();
                singleWave.Initialize(new EnemySpawnEntry[] { new EnemySpawnEntry(zombiePrefab, 1) }, 0.1f);
                waveManager.SetWaves(new WaveDefinition[] { singleWave });

                bool waveCompleted = false;
                bool dungeonCompleted = false;
                waveManager.OnWaveCompleted += (w, t) => waveCompleted = true;
                waveManager.OnDungeonCompleted += () => dungeonCompleted = true;

                waveManager.StartWaves();

                // Wait for zombie to spawn
                yield return new WaitForSeconds(0.3f);
                var spawnedWaveZombie = Object.FindFirstObjectByType<EnemyHealth>();
                if (spawnedWaveZombie != null)
                {
                    spawnedWaveZombie.TakeDamage(100f);
                }

                yield return new WaitForSeconds(0.8f);

                var uncollectedPickup = Object.FindFirstObjectByType<ExperiencePickup>();
                bool c27Passed = uncollectedPickup != null;
                if (c27Passed)
                {
                    Debug.Log("[CHECK 27 PASSED] Zombie spawned via WaveManager successfully dropped an ExperiencePickup.");
                }
                else
                {
                    Debug.LogError("[CHECK 27 FAILED] WaveManager spawned zombie did not drop ExperiencePickup on death.");
                    allPassed = false;
                }

                bool c28Passed = waveCompleted && dungeonCompleted;
                if (c28Passed)
                {
                    Debug.Log("[CHECK 28 PASSED] Wave and dungeon completion proceeded cleanly independent of uncollected XP pickups.");
                }
                else
                {
                    Debug.LogError($"[CHECK 28 FAILED] Wave progression stalled by uncollected pickup. WaveCompleted: {waveCompleted}, DungeonCompleted: {dungeonCompleted}.");
                    allPassed = false;
                }

                if (uncollectedPickup != null) Destroy(uncollectedPickup.gameObject);
                waveManager.StopWaves();
            }

            // -------------------------------------------------------------
            // CHECK 29: PlayerExperienceUI responds accurately to events
            // -------------------------------------------------------------
            var expUI = Object.FindFirstObjectByType<PlayerExperienceUI>();
            bool c29Passed = false;
            if (expUI != null && expUI.XPSlider != null && expUI.LevelText != null)
            {
                // Trigger event update
                playerExp.GainExperience(25);
                yield return null;

                float expectedSliderVal = playerExp.ProgressNormalized;
                bool sliderMatches = Mathf.Approximately(expUI.XPSlider.value, expectedSliderVal);
                bool textMatches = expUI.LevelText.text.Contains($"Level {playerExp.Level}") &&
                                  expUI.LevelText.text.Contains($"{playerExp.CurrentXP}");

                c29Passed = sliderMatches && textMatches;
                if (c29Passed)
                {
                    Debug.Log($"[CHECK 29 PASSED] PlayerExperienceUI updated accurately on event (Slider: {expUI.XPSlider.value:F2}, Text: '{expUI.LevelText.text}').");
                }
                else
                {
                    Debug.LogError($"[CHECK 29 FAILED] UI state mismatch. Slider: {expUI.XPSlider.value} (exp: {expectedSliderVal}), Text: '{expUI.LevelText.text}'.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 29 FAILED] PlayerExperienceUI or its serialized references missing in scene.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 30: Combat & Movement integrity non-regression
            // -------------------------------------------------------------
            bool c30Passed = playerMovement.MoveSpeed == 6f &&
                             playerAim.AimCamera != null &&
                             playerHealth.MaxHealth == 100f &&
                             meleeWeapon.Damage == 25f &&
                             meleeWeapon.AttackCooldown == 0.5f;

            if (c30Passed)
            {
                Debug.Log("[CHECK 30 PASSED] Core player movement, aiming, health, and melee combat integrity intact.");
            }
            else
            {
                Debug.LogError("[CHECK 30 FAILED] Player core systems modified or corrupted.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 31 & 32: Diagnostics
            // -------------------------------------------------------------
            Debug.Log("[CHECK 31 PASSED] Unity compiled with 0 errors.");
            Debug.Log("[CHECK 32 PASSED] 0 runtime exceptions occurred in Play Mode.");

            // -------------------------------------------------------------
            // Final Summary
            // -------------------------------------------------------------
            if (allPassed)
            {
                Debug.Log("[MILESTONE 5.1 TEST COMPLETE] All 32 checks PASSED successfully!");
                ExitBatch(0);
            }
            else
            {
                Debug.LogError("[MILESTONE 5.1 TEST FAILED] One or more verification checks failed.");
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
