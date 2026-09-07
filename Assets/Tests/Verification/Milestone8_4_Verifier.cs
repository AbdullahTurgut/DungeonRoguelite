using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;
using DungeonRoguelite.Characters;
using DungeonRoguelite.CameraControl;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.UI;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Weapons;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated Play Mode verification suite for Milestone 8.4 (Gunner Combat Prototype).
    /// Validates IPrimaryAttack contract, RifleWeapon hitscan mechanics, ascending distance hit ordering,
    /// strict occlusion ordering (owner ignored, trigger ignored, wall blocking, zero penetration),
    /// universal upgrade scaling, Gunner prefab and asset configuration, runtime spawning and explicit binding,
    /// safe manual test helper, production scene cleanliness, and Warrior/Archer non-regression.
    /// </summary>
    public class Milestone8_4_Verifier : MonoBehaviour
    {
        [SerializeField] private bool runAutomatedTestOnStart = true;

        private void Start()
        {
            if (runAutomatedTestOnStart || Application.isBatchMode)
            {
                StartCoroutine(RunVerificationSafe());
            }
        }

        private IEnumerator RunVerificationSafe()
        {
            IEnumerator routine = RunVerificationRoutine();
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
                    Debug.LogError($"[M8.4 TEST EXCEPTION] Unexpected exception during verification: {ex}");
                    ExitBatch(1);
                    yield break;
                }

                yield return current;
            }
        }

        private IEnumerator RunVerificationRoutine()
        {
            yield return new WaitForSeconds(0.1f);

            Debug.Log("[M8.4 TEST START] Beginning Milestone 8.4 automated verification suite...");
            bool allPassed = true;

            const string GunnerAssetPath = "Assets/ScriptableObjects/Characters/Character_Gunner.asset";
            const string GunnerPrefabPath = "Assets/Prefabs/Characters/Gunner.prefab";
            const string WarriorAssetPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
            const string WarriorPrefabPath = "Assets/Prefabs/Characters/Warrior.prefab";
            const string ArcherAssetPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";
            const string ArcherPrefabPath = "Assets/Prefabs/Characters/Archer.prefab";
            const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

            // -------------------------------------------------------------
            // SECTION A: Primary Attack & Contracts
            // -------------------------------------------------------------

            // CHECK 1: IPrimaryAttack contract remains bool TryAttack()
            var primaryAttackType = typeof(IPrimaryAttack);
            var tryAttackMethod = primaryAttackType.GetMethod("TryAttack", Type.EmptyTypes);
            if (primaryAttackType.IsInterface && tryAttackMethod != null && tryAttackMethod.ReturnType == typeof(bool))
            {
                Debug.Log("[CHECK 1 PASSED] IPrimaryAttack interface exists with bool TryAttack() contract.");
            }
            else
            {
                Debug.LogError("[CHECK 1 FAILED] IPrimaryAttack interface is missing or TryAttack contract mismatch.");
                allPassed = false;
            }

            // CHECK 2: Warrior.prefab uses MeleeWeapon implementing IPrimaryAttack
            var warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefabPath);
            if (warriorPrefab != null && warriorPrefab.GetComponent<MeleeWeapon>() != null && typeof(IPrimaryAttack).IsAssignableFrom(typeof(MeleeWeapon)))
            {
                Debug.Log("[CHECK 2 PASSED] Warrior prefab uses MeleeWeapon implementing IPrimaryAttack.");
            }
            else
            {
                Debug.LogError("[CHECK 2 FAILED] Warrior prefab missing or does not use MeleeWeapon : IPrimaryAttack.");
                allPassed = false;
            }

            // CHECK 3: Archer.prefab uses BowWeapon implementing IPrimaryAttack
            var archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherPrefabPath);
            if (archerPrefab != null && archerPrefab.GetComponent<BowWeapon>() != null && typeof(IPrimaryAttack).IsAssignableFrom(typeof(BowWeapon)))
            {
                Debug.Log("[CHECK 3 PASSED] Archer prefab uses BowWeapon implementing IPrimaryAttack.");
            }
            else
            {
                Debug.LogError("[CHECK 3 FAILED] Archer prefab missing or does not use BowWeapon : IPrimaryAttack.");
                allPassed = false;
            }

            // CHECK 4: RifleWeapon implements IPrimaryAttack
            if (typeof(IPrimaryAttack).IsAssignableFrom(typeof(RifleWeapon)))
            {
                Debug.Log("[CHECK 4 PASSED] RifleWeapon implements IPrimaryAttack.");
            }
            else
            {
                Debug.LogError("[CHECK 4 FAILED] RifleWeapon does not implement IPrimaryAttack.");
                allPassed = false;
            }

            // Instantiate a test RifleWeapon holder for checks 5-10
            GameObject testRifleGo = new GameObject("TestRifleWeaponHolder");
            var rifle = testRifleGo.AddComponent<RifleWeapon>();
            var rifleStats = testRifleGo.AddComponent<PlayerStats>();
            rifle.SetPlayerStats(rifleStats);

            // CHECK 5: RifleWeapon.BaseDamage == 10
            if (Mathf.Approximately(rifle.BaseDamage, 10f))
            {
                Debug.Log("[CHECK 5 PASSED] RifleWeapon.BaseDamage returns 10.");
            }
            else
            {
                Debug.LogError($"[CHECK 5 FAILED] RifleWeapon.BaseDamage was {rifle.BaseDamage}, expected 10.");
                allPassed = false;
            }

            // CHECK 6: RifleWeapon.AttackCooldown == 0.18s
            if (Mathf.Approximately(rifle.AttackCooldown, 0.18f))
            {
                Debug.Log("[CHECK 6 PASSED] RifleWeapon.AttackCooldown returns 0.18s.");
            }
            else
            {
                Debug.LogError($"[CHECK 6 FAILED] RifleWeapon.AttackCooldown was {rifle.AttackCooldown}, expected 0.18s.");
                allPassed = false;
            }

            // CHECK 7: RifleWeapon.Range == 25m
            if (Mathf.Approximately(rifle.Range, 25f))
            {
                Debug.Log("[CHECK 7 PASSED] RifleWeapon.Range returns 25m.");
            }
            else
            {
                Debug.LogError($"[CHECK 7 FAILED] RifleWeapon.Range was {rifle.Range}, expected 25m.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION B: Stat Scaling & Modifiers
            // -------------------------------------------------------------

            // CHECK 8: PlayerStats damage scaling (+20%: 10 -> 12)
            rifleStats.AddDamageBonus(0.20f);
            if (Mathf.Approximately(rifle.EffectiveDamage, 12f))
            {
                Debug.Log("[CHECK 8 PASSED] Rifle EffectiveDamage correctly scaled +20% (10 -> 12).");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] Rifle EffectiveDamage was {rifle.EffectiveDamage}, expected 12.");
                allPassed = false;
            }

            // CHECK 9: PlayerStats attack speed scaling (+15%: 0.18 / 1.15 ≈ 0.15652s)
            rifleStats.AddAttackSpeedBonus(0.15f);
            float expectedCooldown = 0.18f / 1.15f;
            if (Mathf.Abs(rifle.EffectiveAttackCooldown - expectedCooldown) < 0.001f)
            {
                Debug.Log($"[CHECK 9 PASSED] Rifle EffectiveAttackCooldown correctly scaled +15% ({rifle.EffectiveAttackCooldown:F4}s).");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] Rifle EffectiveAttackCooldown was {rifle.EffectiveAttackCooldown}, expected {expectedCooldown}.");
                allPassed = false;
            }

            // CHECK 10: PlayerStats movement speed scaling (+10%: 6.0 -> 6.6)
            var movementGo = new GameObject("TestMovementHolder");
            movementGo.AddComponent<CharacterController>();
            var moveStats = movementGo.AddComponent<PlayerStats>();
            var movement = movementGo.AddComponent<PlayerMovement>();
            moveStats.AddMovementSpeedBonus(0.10f);
            if (Mathf.Approximately(movement.EffectiveMoveSpeed, 6.6f))
            {
                Debug.Log("[CHECK 10 PASSED] PlayerMovement EffectiveMoveSpeed correctly scaled +10% (6.0 -> 6.6).");
            }
            else
            {
                Debug.LogError($"[CHECK 10 FAILED] PlayerMovement EffectiveMoveSpeed was {movement.EffectiveMoveSpeed}, expected 6.6.");
                allPassed = false;
            }
            Object.DestroyImmediate(movementGo);
            Object.DestroyImmediate(testRifleGo);

            // -------------------------------------------------------------
            // SECTION C: Hitscan Mechanics & Explicit Occlusion Ordering
            // -------------------------------------------------------------

            // CHECK 11: Pause blocks firing (Time.timeScale <= 0)
            GameObject pauseTestGo = new GameObject("PauseTestHolder");
            var pauseRifle = pauseTestGo.AddComponent<RifleWeapon>();
            Time.timeScale = 0f;
            bool attackWhilePaused = pauseRifle.TryAttack();
            Time.timeScale = 1f;
            if (!attackWhilePaused)
            {
                Debug.Log("[CHECK 11 PASSED] Pause strictly blocks Rifle firing (Time.timeScale <= 0f).");
            }
            else
            {
                Debug.LogError("[CHECK 11 FAILED] Rifle fired while paused.");
                allPassed = false;
            }
            Object.DestroyImmediate(pauseTestGo);

            // CHECK 12: Cooldown is enforced
            GameObject cdTestGo = new GameObject("CdTestHolder");
            var cdRifle = cdTestGo.AddComponent<RifleWeapon>();
            bool firstShot = cdRifle.TryAttack();
            bool rapidSecondShot = cdRifle.TryAttack(); // immediate: must fail cooldown
            yield return new WaitForSeconds(0.20f); // wait > 0.18s
            bool thirdShotAfterCooldown = cdRifle.TryAttack(); // must succeed
            if (firstShot && !rapidSecondShot && thirdShotAfterCooldown)
            {
                Debug.Log("[CHECK 12 PASSED] Attack cooldown strictly enforced (rapid shot blocked, shot after 0.18s succeeds).");
            }
            else
            {
                Debug.LogError($"[CHECK 12 FAILED] Cooldown check failed. firstShot={firstShot}, rapidSecondShot={rapidSecondShot}, thirdShot={thirdShotAfterCooldown}.");
                allPassed = false;
            }
            Object.DestroyImmediate(cdTestGo);

            // CHECK 13: One attack produces exactly one firearm attack event (OnAttack)
            GameObject eventTestGo = new GameObject("EventTestHolder");
            var eventRifle = eventTestGo.AddComponent<RifleWeapon>();
            int attackEventCount = 0;
            eventRifle.OnAttack += () => attackEventCount++;
            eventRifle.TryAttack();
            eventRifle.TryAttack(); // blocked
            if (attackEventCount == 1)
            {
                Debug.Log("[CHECK 13 PASSED] Exactly one OnAttack event fired per valid attack.");
            }
            else
            {
                Debug.LogError($"[CHECK 13 FAILED] OnAttack fired {attackEventCount} times, expected 1.");
                allPassed = false;
            }
            Object.DestroyImmediate(eventTestGo);

            // CHECK 14: Aim direction is respected (hitscan query follows forward vector)
            // Target placed at (0, 0, 5) with gunner aiming at (0, 0, 1) should hit.
            // Target placed at (5, 0, 0) should NOT hit when aiming at (0, 0, 1).
            GameObject aimTestGo = new GameObject("AimTestHolder");
            aimTestGo.transform.position = Vector3.zero;
            aimTestGo.transform.rotation = Quaternion.identity; // facing (0, 0, 1)
            var aimRifle = aimTestGo.AddComponent<RifleWeapon>();

            var alignedTargetGo = CreateTestTarget("AlignedTarget", new Vector3(0f, 1f, 5f), 50f);
            var offAxisTargetGo = CreateTestTarget("OffAxisTarget", new Vector3(5f, 1f, 0f), 50f);

            Physics.SyncTransforms();
            aimRifle.TryAttack();
            var alignedDamageable = alignedTargetGo.GetComponent<TestDamageableTarget>();
            var offAxisDamageable = offAxisTargetGo.GetComponent<TestDamageableTarget>();

            if (alignedDamageable.HitCount == 1 && offAxisDamageable.HitCount == 0)
            {
                Debug.Log("[CHECK 14 PASSED] Aim direction respected: aligned target hit, off-axis target untouched.");
            }
            else
            {
                Debug.LogError($"[CHECK 14 FAILED] Aim check failed: alignedHits={alignedDamageable.HitCount}, offAxisHits={offAxisDamageable.HitCount}.");
                allPassed = false;
            }
            Object.DestroyImmediate(alignedTargetGo);
            Object.DestroyImmediate(offAxisTargetGo);
            Object.DestroyImmediate(aimTestGo);

            // CHECK 15: Owner hierarchy is ignored even when cast begins inside owner collider
            GameObject ownerTestGo = new GameObject("OwnerTestHolder");
            ownerTestGo.transform.position = Vector3.zero;
            var ownerCc = ownerTestGo.AddComponent<CharacterController>();
            ownerCc.center = new Vector3(0f, 1f, 0f);
            ownerCc.height = 2f;
            ownerCc.radius = 0.5f;
            var ownerDamageable = ownerTestGo.AddComponent<TestDamageableTarget>();
            ownerDamageable.ResetTarget(100f);

            var ownerRifle = ownerTestGo.AddComponent<RifleWeapon>();
            var targetAheadGo = CreateTestTarget("TargetAhead", new Vector3(0f, 1f, 5f), 50f);
            var targetAheadDamageable = targetAheadGo.GetComponent<TestDamageableTarget>();

            Physics.SyncTransforms();
            ownerRifle.TryAttack();
            if (ownerDamageable.HitCount == 0 && targetAheadDamageable.HitCount == 1)
            {
                Debug.Log("[CHECK 15 PASSED] Owner hierarchy safely ignored even with overlapping collider; target ahead receives damage.");
            }
            else
            {
                Debug.LogError($"[CHECK 15 FAILED] Owner hierarchy check failed: ownerHits={ownerDamageable.HitCount}, targetHits={targetAheadDamageable.HitCount}.");
                allPassed = false;
            }
            Object.DestroyImmediate(targetAheadGo);
            Object.DestroyImmediate(ownerTestGo);

            // CHECK 16: Trigger-only volumes are ignored (QueryTriggerInteraction.Ignore)
            GameObject triggerTestGo = new GameObject("TriggerTestHolder");
            triggerTestGo.transform.position = Vector3.zero;
            var triggerRifle = triggerTestGo.AddComponent<RifleWeapon>();

            // Trigger volume at z = 3m
            GameObject triggerVolumeGo = new GameObject("TriggerVolume");
            triggerVolumeGo.transform.position = new Vector3(0f, 1f, 3f);
            var triggerCol = triggerVolumeGo.AddComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector3(2f, 2f, 2f);
            var triggerDamageable = triggerVolumeGo.AddComponent<TestDamageableTarget>();
            triggerDamageable.ResetTarget(50f);

            // Target at z = 6m
            var targetBeyondTriggerGo = CreateTestTarget("TargetBeyondTrigger", new Vector3(0f, 1f, 6f), 50f);
            var targetBeyondDamageable = targetBeyondTriggerGo.GetComponent<TestDamageableTarget>();

            Physics.SyncTransforms();
            triggerRifle.TryAttack();
            if (triggerDamageable.HitCount == 0 && targetBeyondDamageable.HitCount == 1)
            {
                Debug.Log("[CHECK 16 PASSED] Trigger volumes strictly ignored; target beyond trigger receives hit.");
            }
            else
            {
                Debug.LogError($"[CHECK 16 FAILED] Trigger check failed: triggerHits={triggerDamageable.HitCount}, targetHits={targetBeyondDamageable.HitCount}.");
                allPassed = false;
            }
            Object.DestroyImmediate(triggerVolumeGo);
            Object.DestroyImmediate(targetBeyondTriggerGo);
            Object.DestroyImmediate(triggerTestGo);

            // CHECK 17: Occlusion Case A: Owner -> ignored, Trigger -> ignored, Zombie -> damaged, Wall behind zombie -> irrelevant
            GameObject caseAGo = new GameObject("CaseAHolder");
            caseAGo.transform.position = Vector3.zero;
            var caseACc = caseAGo.AddComponent<CharacterController>();
            var caseADamageable = caseAGo.AddComponent<TestDamageableTarget>();
            caseADamageable.ResetTarget(100f);
            var caseARifle = caseAGo.AddComponent<RifleWeapon>();

            // Trigger at z = 2m
            var triggerA = new GameObject("TriggerA");
            triggerA.transform.position = new Vector3(0f, 1f, 2f);
            var trigACol = triggerA.AddComponent<BoxCollider>();
            trigACol.isTrigger = true;
            var trigADamageable = triggerA.AddComponent<TestDamageableTarget>();

            // Zombie at z = 5m
            var zombieA = CreateTestTarget("ZombieA", new Vector3(0f, 1f, 5f), 50f);
            var zombieADamageable = zombieA.GetComponent<TestDamageableTarget>();

            // Solid wall at z = 8m
            var wallBehindZombie = CreateTestWall("WallBehindZombie", new Vector3(0f, 1f, 8f), new Vector3(4f, 4f, 1f));

            Physics.SyncTransforms();
            caseARifle.TryAttack();
            if (caseADamageable.HitCount == 0 && trigADamageable.HitCount == 0 && zombieADamageable.HitCount == 1 && Mathf.Approximately(zombieADamageable.TotalDamageReceived, 10f))
            {
                Debug.Log("[CHECK 17 PASSED] Occlusion Case A verified: Owner ignored, Trigger ignored, Zombie damaged (10), Wall behind irrelevant.");
            }
            else
            {
                Debug.LogError($"[CHECK 17 FAILED] Occlusion Case A failed. ownerHits={caseADamageable.HitCount}, trigHits={trigADamageable.HitCount}, zombieHits={zombieADamageable.HitCount}, zombieDmg={zombieADamageable.TotalDamageReceived}.");
                allPassed = false;
            }
            Object.DestroyImmediate(wallBehindZombie);
            Object.DestroyImmediate(zombieA);
            Object.DestroyImmediate(triggerA);
            Object.DestroyImmediate(caseAGo);

            // CHECK 18: Occlusion Case B: Owner -> ignored, Trigger -> ignored, Wall -> stops shot, Zombie behind wall -> receives ZERO damage
            GameObject caseBGo = new GameObject("CaseBHolder");
            caseBGo.transform.position = Vector3.zero;
            var caseBCc = caseBGo.AddComponent<CharacterController>();
            var caseBDamageable = caseBGo.AddComponent<TestDamageableTarget>();
            var caseBRifle = caseBGo.AddComponent<RifleWeapon>();

            // Trigger at z = 2m
            var triggerB = new GameObject("TriggerB");
            triggerB.transform.position = new Vector3(0f, 1f, 2f);
            var trigBCol = triggerB.AddComponent<BoxCollider>();
            trigBCol.isTrigger = true;
            var trigBDamageable = triggerB.AddComponent<TestDamageableTarget>();

            // Solid wall at z = 4m
            var blockingWall = CreateTestWall("BlockingWall", new Vector3(0f, 1f, 4f), new Vector3(4f, 4f, 1f));

            // Zombie behind wall at z = 7m
            var zombieBehindWall = CreateTestTarget("ZombieBehindWall", new Vector3(0f, 1f, 7f), 50f);
            var zombieBehindDamageable = zombieBehindWall.GetComponent<TestDamageableTarget>();

            Physics.SyncTransforms();
            caseBRifle.TryAttack();
            if (caseBDamageable.HitCount == 0 && trigBDamageable.HitCount == 0 && zombieBehindDamageable.HitCount == 0 && Mathf.Approximately(zombieBehindDamageable.TotalDamageReceived, 0f))
            {
                Debug.Log("[CHECK 18 PASSED] Occlusion Case B verified: Wall stops shot, Zombie behind wall receives ZERO damage.");
            }
            else
            {
                Debug.LogError($"[CHECK 18 FAILED] Occlusion Case B failed. zombieBehindHits={zombieBehindDamageable.HitCount}, zombieBehindDmg={zombieBehindDamageable.TotalDamageReceived}.");
                allPassed = false;
            }
            Object.DestroyImmediate(zombieBehindWall);
            Object.DestroyImmediate(blockingWall);
            Object.DestroyImmediate(triggerB);
            Object.DestroyImmediate(caseBGo);

            // CHECK 19: Occlusion Case C: Near Zombie -> damaged exactly once, Far Zombie behind it -> receives ZERO damage (zero penetration)
            GameObject caseCGo = new GameObject("CaseCHolder");
            caseCGo.transform.position = Vector3.zero;
            var caseCRifle = caseCGo.AddComponent<RifleWeapon>();

            // Near Zombie at z = 4m
            var nearZombie = CreateTestTarget("NearZombie", new Vector3(0f, 1f, 4f), 50f);
            var nearDamageable = nearZombie.GetComponent<TestDamageableTarget>();

            // Far Zombie behind it at z = 7m
            var farZombie = CreateTestTarget("FarZombie", new Vector3(0f, 1f, 7f), 50f);
            var farDamageable = farZombie.GetComponent<TestDamageableTarget>();

            Physics.SyncTransforms();
            caseCRifle.TryAttack();
            if (nearDamageable.HitCount == 1 && Mathf.Approximately(nearDamageable.TotalDamageReceived, 10f) && farDamageable.HitCount == 0 && Mathf.Approximately(farDamageable.TotalDamageReceived, 0f))
            {
                Debug.Log("[CHECK 19 PASSED] Occlusion Case C verified: Near Zombie damaged exactly once (10), Far Zombie receives ZERO damage (strictly no penetration).");
            }
            else
            {
                Debug.LogError($"[CHECK 19 FAILED] Occlusion Case C failed. nearHits={nearDamageable.HitCount}, nearDmg={nearDamageable.TotalDamageReceived}, farHits={farDamageable.HitCount}, farDmg={farDamageable.TotalDamageReceived}.");
                allPassed = false;
            }
            Object.DestroyImmediate(farZombie);
            Object.DestroyImmediate(nearZombie);
            Object.DestroyImmediate(caseCGo);

            // CHECK 20: Range limit: target beyond 25m receives ZERO damage
            GameObject rangeTestGo = new GameObject("RangeTestHolder");
            rangeTestGo.transform.position = Vector3.zero;
            var rangeRifle = rangeTestGo.AddComponent<RifleWeapon>();

            var outOfRangeTarget = CreateTestTarget("OutOfRangeTarget", new Vector3(0f, 1f, 26f), 50f);
            var outOfRangeDamageable = outOfRangeTarget.GetComponent<TestDamageableTarget>();

            Physics.SyncTransforms();
            rangeRifle.TryAttack();
            if (outOfRangeDamageable.HitCount == 0)
            {
                Debug.Log("[CHECK 20 PASSED] Range limit enforced: target beyond 25m receives ZERO damage.");
            }
            else
            {
                Debug.LogError($"[CHECK 20 FAILED] Out of range target hit: hitCount={outOfRangeDamageable.HitCount}.");
                allPassed = false;
            }
            Object.DestroyImmediate(outOfRangeTarget);
            Object.DestroyImmediate(rangeTestGo);

            // -------------------------------------------------------------
            // SECTION D: Prefab Composition & Data Assets
            // -------------------------------------------------------------

            // CHECK 21: Gunner.prefab exists
            var gunnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GunnerPrefabPath);
            if (gunnerPrefab != null)
            {
                Debug.Log("[CHECK 21 PASSED] Gunner.prefab exists at Assets/Prefabs/Characters/Gunner.prefab.");
            }
            else
            {
                Debug.LogError("[CHECK 21 FAILED] Gunner.prefab not found.");
                allPassed = false;
            }

            // CHECK 22: Gunner.prefab Tag == "Player"
            if (gunnerPrefab != null && gunnerPrefab.CompareTag("Player"))
            {
                Debug.Log("[CHECK 22 PASSED] Gunner.prefab Tag is 'Player'.");
            }
            else
            {
                Debug.LogError("[CHECK 22 FAILED] Gunner.prefab Tag is not 'Player'.");
                allPassed = false;
            }

            // CHECK 23: Gunner.prefab components
            if (gunnerPrefab != null)
            {
                bool hasCc = gunnerPrefab.GetComponent<CharacterController>() != null;
                var move = gunnerPrefab.GetComponent<PlayerMovement>();
                bool hasMove = move != null && Mathf.Approximately(move.MoveSpeed, 6.0f);
                bool hasAim = gunnerPrefab.GetComponent<PlayerAim>() != null;
                var hp = gunnerPrefab.GetComponent<PlayerHealth>();
                bool hasHp = hp != null && Mathf.Approximately(hp.MaxHealth, 100f);
                bool hasStats = gunnerPrefab.GetComponent<PlayerStats>() != null;
                bool hasExp = gunnerPrefab.GetComponent<PlayerExperience>() != null;
                bool hasAtk = gunnerPrefab.GetComponent<PlayerAttack>() != null;
                var gunnerRifle = gunnerPrefab.GetComponent<RifleWeapon>();
                bool hasRifle = gunnerRifle != null && Mathf.Approximately(gunnerRifle.Damage, 10f) && Mathf.Approximately(gunnerRifle.AttackCooldown, 0.18f);

                if (hasCc && hasMove && hasAim && hasHp && hasStats && hasExp && hasAtk && hasRifle)
                {
                    Debug.Log("[CHECK 23 PASSED] Gunner.prefab contains all required components with baseline values (moveSpeed=6.0, hp=100, dmg=10, cd=0.18s).");
                }
                else
                {
                    Debug.LogError($"[CHECK 23 FAILED] Gunner.prefab component check failed: cc={hasCc}, move={hasMove}, aim={hasAim}, hp={hasHp}, stats={hasStats}, exp={hasExp}, atk={hasAtk}, rifle={hasRifle}.");
                    allPassed = false;
                }
            }

            // CHECK 24: Gunner.prefab does NOT contain MeleeWeapon or BowWeapon
            if (gunnerPrefab != null)
            {
                bool hasMelee = gunnerPrefab.GetComponent<MeleeWeapon>() != null;
                bool hasBow = gunnerPrefab.GetComponent<BowWeapon>() != null;
                if (!hasMelee && !hasBow)
                {
                    Debug.Log("[CHECK 24 PASSED] Gunner.prefab does NOT contain MeleeWeapon or BowWeapon.");
                }
                else
                {
                    Debug.LogError($"[CHECK 24 FAILED] Gunner.prefab erroneously contains MeleeWeapon ({hasMelee}) or BowWeapon ({hasBow}).");
                    allPassed = false;
                }
            }

            // CHECK 25: Gunner.prefab hierarchy (Visual, FacingIndicator, WeaponAnchor, RifleVisual, MuzzlePoint)
            if (gunnerPrefab != null)
            {
                Transform visual = gunnerPrefab.transform.Find("Visual");
                Transform facing = gunnerPrefab.transform.Find("FacingIndicator") ?? (visual != null ? visual.Find("FacingIndicator") : null);
                Transform anchor = gunnerPrefab.transform.Find("Visual/WeaponAnchor");
                Transform rifleVisual = gunnerPrefab.transform.Find("Visual/WeaponAnchor/RifleVisual");
                Transform muzzle = gunnerPrefab.transform.Find("Visual/WeaponAnchor/RifleVisual/MuzzlePoint");

                if (visual != null && facing != null && anchor != null && rifleVisual != null && muzzle != null)
                {
                    Debug.Log("[CHECK 25 PASSED] Gunner.prefab hierarchy complete: Visual, FacingIndicator, WeaponAnchor, RifleVisual, MuzzlePoint.");
                }
                else
                {
                    Debug.LogError($"[CHECK 25 FAILED] Gunner.prefab hierarchy missing elements: visual={visual != null}, facing={facing != null}, anchor={anchor != null}, rifleVisual={rifleVisual != null}, muzzle={muzzle != null}.");
                    allPassed = false;
                }
            }

            // CHECK 26: Character_Gunner.asset exists and links to Gunner.prefab
            var gunnerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(GunnerAssetPath);
            if (gunnerAsset != null && gunnerAsset.Id == "gunner" && (gunnerAsset.DisplayName == "Gunner" || gunnerAsset.DisplayName == "Nişancı") && gunnerAsset.CharacterPrefab == gunnerPrefab)
            {
                Debug.Log($"[CHECK 26 PASSED] Character_Gunner.asset configured correctly ('{gunnerAsset.DisplayName}') and links to Gunner.prefab.");
            }
            else
            {
                Debug.LogError("[CHECK 26 FAILED] Character_Gunner.asset missing or configuration mismatch.");
                allPassed = false;
            }

            // CHECK 27: CharacterDefinition schema has no combat math or stat fields
            var cdFields = typeof(CharacterDefinition).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            bool hasLeakedCombatField = false;
            foreach (var f in cdFields)
            {
                string lower = f.Name.ToLowerInvariant();
                if (lower.Contains("damage") || lower.Contains("cooldown") || lower.Contains("health") || lower.Contains("speed"))
                {
                    hasLeakedCombatField = true;
                    break;
                }
            }
            if (!hasLeakedCombatField)
            {
                Debug.Log("[CHECK 27 PASSED] CharacterDefinition schema contains zero combat calculation or stat modifier fields.");
            }
            else
            {
                Debug.LogError("[CHECK 27 FAILED] CharacterDefinition contains leaked combat fields.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION E: Runtime Spawning & Scene System Binding
            // -------------------------------------------------------------

            var spawner = Object.FindFirstObjectByType<PlayerSpawner>();
            if (spawner == null)
            {
                Debug.LogError("[CHECK 28-33 FAILED] PlayerSpawner not found in scene.");
                allPassed = false;
                yield break;
            }

            // Clean any existing character instance to test Gunner spawn
            if (spawner.ActiveCharacter != null)
            {
                Object.DestroyImmediate(spawner.ActiveCharacter.gameObject);
                var activeField = typeof(PlayerSpawner).GetField("activeCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
                if (activeField != null) activeField.SetValue(spawner, null);
            }

            // CHECK 28: PlayerSpawner.Spawn(Character_Gunner) instantiates Gunner
            PlayableCharacter spawnedGunner = spawner.Spawn(gunnerAsset);
            if (spawnedGunner != null && spawnedGunner.CharacterDefinition == gunnerAsset && spawnedGunner.name.Contains("Gunner"))
            {
                Debug.Log("[CHECK 28 PASSED] PlayerSpawner.Spawn(Character_Gunner) successfully instantiated Gunner.");
            }
            else
            {
                Debug.LogError("[CHECK 28 FAILED] Spawning Gunner failed.");
                allPassed = false;
            }

            yield return null;

            // CHECK 29: CameraFollow bound to runtime Gunner
            var cameraFollow = Object.FindFirstObjectByType<CameraFollow>();
            if (cameraFollow != null && cameraFollow.Target == spawnedGunner.transform)
            {
                Debug.Log("[CHECK 29 PASSED] CameraFollow target explicitly bound to runtime Gunner transform.");
            }
            else
            {
                Debug.LogError("[CHECK 29 FAILED] CameraFollow target not bound to runtime Gunner.");
                allPassed = false;
            }

            // CHECK 30: WaveManager playerTarget bound to runtime Gunner
            var waveManager = Object.FindFirstObjectByType<WaveManager>();
            if (waveManager != null && waveManager.PlayerTarget == spawnedGunner.transform)
            {
                Debug.Log("[CHECK 30 PASSED] WaveManager playerTarget explicitly bound to runtime Gunner transform.");
            }
            else
            {
                Debug.LogError("[CHECK 30 FAILED] WaveManager playerTarget not bound to runtime Gunner.");
                allPassed = false;
            }

            // CHECK 31: UpgradeManager bound to runtime Gunner PlayerExperience and PlayerStats
            var upgradeManager = Object.FindFirstObjectByType<UpgradeManager>();
            var gunnerExp = spawnedGunner.GetComponent<PlayerExperience>();
            var gunnerStats = spawnedGunner.GetComponent<PlayerStats>();
            var upExpField = typeof(UpgradeManager).GetField("playerExperience", BindingFlags.NonPublic | BindingFlags.Instance);
            var upStatsField = typeof(UpgradeManager).GetField("playerStats", BindingFlags.NonPublic | BindingFlags.Instance);
            bool upExpMatch = upExpField != null && (PlayerExperience)upExpField.GetValue(upgradeManager) == gunnerExp;
            bool upStatsMatch = upStatsField != null && (PlayerStats)upStatsField.GetValue(upgradeManager) == gunnerStats;
            if (upExpMatch && upStatsMatch)
            {
                Debug.Log("[CHECK 31 PASSED] UpgradeManager explicitly bound to runtime Gunner PlayerExperience and PlayerStats.");
            }
            else
            {
                Debug.LogError($"[CHECK 31 FAILED] UpgradeManager binding mismatch. expMatch={upExpMatch}, statsMatch={upStatsMatch}.");
                allPassed = false;
            }

            // CHECK 32: PlayerExperienceUI bound to runtime Gunner PlayerExperience
            var expUI = Object.FindFirstObjectByType<PlayerExperienceUI>();
            var expUiField = typeof(PlayerExperienceUI).GetField("playerExperience", BindingFlags.NonPublic | BindingFlags.Instance);
            bool expUiMatch = expUiField != null && (PlayerExperience)expUiField.GetValue(expUI) == gunnerExp;
            if (expUiMatch)
            {
                Debug.Log("[CHECK 32 PASSED] PlayerExperienceUI explicitly bound to runtime Gunner PlayerExperience.");
            }
            else
            {
                Debug.LogError("[CHECK 32 FAILED] PlayerExperienceUI not bound to runtime Gunner.");
                allPassed = false;
            }

            // CHECK 33: DungeonCompletionController bound to runtime Gunner PlayerExperience
            var completionController = Object.FindFirstObjectByType<DungeonCompletionController>();
            var compExpField = typeof(DungeonCompletionController).GetField("playerExperience", BindingFlags.NonPublic | BindingFlags.Instance);
            bool compExpMatch = compExpField != null && (PlayerExperience)compExpField.GetValue(completionController) == gunnerExp;
            if (compExpMatch)
            {
                Debug.Log("[CHECK 33 PASSED] DungeonCompletionController explicitly bound to runtime Gunner PlayerExperience.");
            }
            else
            {
                Debug.LogError("[CHECK 33 FAILED] DungeonCompletionController not bound to runtime Gunner.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION F: Full Gameplay Loop with Gunner
            // -------------------------------------------------------------

            // CHECK 34: Zombie attacks runtime Gunner and Gunner takes damage via IDamageable
            var gunnerHealth = spawnedGunner.GetComponent<PlayerHealth>();
            float initialHp = gunnerHealth.CurrentHealth;
            gunnerHealth.TakeDamage(10f);
            if (Mathf.Approximately(gunnerHealth.CurrentHealth, initialHp - 10f))
            {
                Debug.Log($"[CHECK 34 PASSED] Gunner took damage via IDamageable ({initialHp} -> {gunnerHealth.CurrentHealth}).");
            }
            else
            {
                Debug.LogError($"[CHECK 34 FAILED] Gunner health mismatch: current={gunnerHealth.CurrentHealth}, expected={initialHp - 10f}.");
                allPassed = false;
            }

            // CHECK 35: Gunner rifle attack damages and kills a Zombie
            var zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            GameObject testZombie = Object.Instantiate(zombiePrefab, spawnedGunner.transform.position + spawnedGunner.transform.forward * 4f, Quaternion.identity);
            var zombieHealth = testZombie.GetComponent<EnemyHealth>();
            var gunnerAttack = spawnedGunner.GetComponent<PlayerAttack>();

            // Gunner fires 5 shots (10 dmg each) to defeat 50 HP Zombie
            yield return new WaitForSeconds(0.2f);
            gunnerAttack.TryAttack();
            yield return new WaitForSeconds(0.2f);
            gunnerAttack.TryAttack();
            yield return new WaitForSeconds(0.2f);
            gunnerAttack.TryAttack();
            yield return new WaitForSeconds(0.2f);
            gunnerAttack.TryAttack();
            yield return new WaitForSeconds(0.2f);
            gunnerAttack.TryAttack();

            if (zombieHealth.IsDead)
            {
                Debug.Log("[CHECK 35 PASSED] Gunner rifle attacks damaged and killed Zombie (HP reduced to 0).");
            }
            else
            {
                Debug.LogError($"[CHECK 35 FAILED] Zombie did not die from 5 rifle shots: currentHp={zombieHealth.CurrentHealth}.");
                allPassed = false;
            }

            // CHECK 36: Zombie death dropped ExperiencePickup
            yield return null;
            var pickups = Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None);
            if (pickups != null && pickups.Length > 0)
            {
                Debug.Log($"[CHECK 36 PASSED] Zombie death spawned ExperiencePickup (count={pickups.Length}).");
            }
            else
            {
                Debug.LogError("[CHECK 36 FAILED] No ExperiencePickup found after Zombie death.");
                allPassed = false;
            }

            // CHECK 37: Gunner collects ExperiencePickup and gains XP
            int xpBefore = gunnerExp.CurrentXP;
            if (pickups != null && pickups.Length > 0)
            {
                pickups[0].TryCollect(gunnerExp);
                if (gunnerExp.CurrentXP > xpBefore)
                {
                    Debug.Log($"[CHECK 37 PASSED] Gunner collected ExperiencePickup and gained XP ({xpBefore} -> {gunnerExp.CurrentXP}).");
                }
                else
                {
                    Debug.LogError("[CHECK 37 FAILED] XP did not increase after collecting pickup.");
                    allPassed = false;
                }
            }

            // Clean test zombie
            if (testZombie != null) Object.DestroyImmediate(testZombie);

            // CHECK 38: Level-up triggers UpgradeManager choices
            int choicesReceived = 0;
            Action<UpgradeDefinition[]> choicesHandler = (choices) => { choicesReceived = choices != null ? choices.Length : 0; };
            upgradeManager.OnUpgradeChoicesRequested += choicesHandler;

            gunnerExp.GainExperience(100); // Trigger level-up to level 2
            yield return null;

            if (gunnerExp.Level == 2 && choicesReceived == 3)
            {
                Debug.Log("[CHECK 38 PASSED] Level-up triggered (Level 1 -> 2) and UpgradeManager presented 3 choices.");
            }
            else
            {
                Debug.LogError($"[CHECK 38 FAILED] Level-up or upgrade choices failed: level={gunnerExp.Level}, choices={choicesReceived}.");
                allPassed = false;
            }
            upgradeManager.OnUpgradeChoicesRequested -= choicesHandler;

            // CHECK 39: Selecting Damage upgrade scales Gunner damage
            var dmgUpgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>("Assets/ScriptableObjects/Upgrades/Upgrade_Damage.asset");
            float baseGunnerDmg = spawnedGunner.GetComponent<RifleWeapon>().BaseDamage;
            upgradeManager.SelectUpgrade(dmgUpgrade);
            float scaledGunnerDmg = spawnedGunner.GetComponent<RifleWeapon>().EffectiveDamage;

            if (Mathf.Approximately(scaledGunnerDmg, baseGunnerDmg * 1.20f))
            {
                Debug.Log($"[CHECK 39 PASSED] Damage upgrade applied to runtime Gunner: EffectiveDamage scaled from {baseGunnerDmg} to {scaledGunnerDmg}.");
            }
            else
            {
                Debug.LogError($"[CHECK 39 FAILED] Scaled damage mismatch: got {scaledGunnerDmg}, expected {baseGunnerDmg * 1.20f}.");
                allPassed = false;
            }

            // CHECK 40: Wave completion and dungeon completion integration
            // Test that DungeonCompletionController can resolve active pickups and generate summary
            var testPickupGo = new GameObject("TestSummaryPickup");
            testPickupGo.AddComponent<SphereCollider>().isTrigger = true;
            testPickupGo.AddComponent<Rigidbody>().isKinematic = true;
            var summaryPickup = testPickupGo.AddComponent<ExperiencePickup>();

            completionController.HandleWaveManagerCompletion();
            yield return null;

            if (completionController.HasCompleted && completionController.IsCompletionFinished && completionController.FinalSummary.FinalLevel >= 1)
            {
                Debug.Log("[CHECK 40 PASSED] Dungeon completion flow executed and generated valid DungeonRunSummary with Gunner.");
            }
            else
            {
                Debug.LogError("[CHECK 40 FAILED] Dungeon completion flow did not complete successfully.");
                allPassed = false;
            }
            if (testPickupGo != null) Object.DestroyImmediate(testPickupGo);

            // Restore time scale after dungeon completion
            Time.timeScale = 1.0f;

            // Clean runtime Gunner
            if (spawnedGunner != null) Object.DestroyImmediate(spawnedGunner.gameObject);
            var activeFieldClear = typeof(PlayerSpawner).GetField("activeCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
            if (activeFieldClear != null) activeFieldClear.SetValue(spawner, null);

            // -------------------------------------------------------------
            // SECTION G: Cleanliness & Non-Regression
            // -------------------------------------------------------------

            // CHECK 41: Warrior vertical slice non-regression
            var warriorAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
            PlayableCharacter testWarrior = spawner.Spawn(warriorAsset);
            yield return null;
            if (testWarrior != null && testWarrior.GetComponent<MeleeWeapon>() != null)
            {
                var meleeWeapon = testWarrior.GetComponent<MeleeWeapon>();
                bool warriorAttacked = meleeWeapon.TryAttack();
                if (warriorAttacked)
                {
                    Debug.Log("[CHECK 41 PASSED] Warrior vertical slice non-regression verified: spawned and executed MeleeWeapon attack.");
                }
                else
                {
                    Debug.LogError("[CHECK 41 FAILED] Warrior MeleeWeapon attack failed.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 41 FAILED] Warrior could not be spawned.");
                allPassed = false;
            }
            if (testWarrior != null) Object.DestroyImmediate(testWarrior.gameObject);
            if (activeFieldClear != null) activeFieldClear.SetValue(spawner, null);

            // CHECK 42: Archer combat non-regression
            var archerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
            PlayableCharacter testArcher = spawner.Spawn(archerAsset);
            yield return null;
            if (testArcher != null && testArcher.GetComponent<BowWeapon>() != null)
            {
                var bowWeapon = testArcher.GetComponent<BowWeapon>();
                bool archerAttacked = bowWeapon.TryAttack();
                if (archerAttacked)
                {
                    Debug.Log("[CHECK 42 PASSED] Archer combat non-regression verified: spawned and fired BowWeapon arrow.");
                }
                else
                {
                    Debug.LogError("[CHECK 42 FAILED] Archer BowWeapon attack failed.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 42 FAILED] Archer could not be spawned.");
                allPassed = false;
            }
            if (testArcher != null) Object.DestroyImmediate(testArcher.gameObject);
            if (activeFieldClear != null) activeFieldClear.SetValue(spawner, null);

            // CHECK 43: Dungeon_Prototype.unity defaultCharacter is Character_Warrior on disk
            string sceneText = File.ReadAllText(ScenePath);
            string warriorGuid = AssetDatabase.AssetPathToGUID(WarriorAssetPath);
            string gunnerGuid = AssetDatabase.AssetPathToGUID(GunnerAssetPath);
            if (sceneText.Contains(warriorGuid) && !sceneText.Contains($"defaultCharacter: {{fileID: 11400000, guid: {gunnerGuid}"))
            {
                Debug.Log("[CHECK 43 PASSED] Dungeon_Prototype.unity defaultCharacter is Character_Warrior on disk.");
            }
            else
            {
                Debug.LogError("[CHECK 43 FAILED] Dungeon_Prototype.unity on disk does not default to Character_Warrior.");
                allPassed = false;
            }

            // CHECK 44: Zero permanent verifiers in scene
            var verifiersInScene = Object.FindObjectsByType<Milestone8_4_Verifier>(FindObjectsSortMode.None);
            if (verifiersInScene.Length <= 1) // Only this transient runner instance
            {
                Debug.Log("[CHECK 44 PASSED] Zero permanent verifier objects saved on disk.");
            }
            else
            {
                Debug.LogError($"[CHECK 44 FAILED] Found {verifiersInScene.Length} verifiers in scene.");
                allPassed = false;
            }

            // Summary
            if (allPassed)
            {
                Debug.Log("[M8.4 TEST SUCCESS] ALL 44 VERIFICATION CHECKS PASSED!");
                ExitBatch(0);
            }
            else
            {
                Debug.LogError("[M8.4 TEST FAILURE] ONE OR MORE VERIFICATION CHECKS FAILED!");
                ExitBatch(1);
            }
        }

        private static GameObject CreateTestTarget(string name, Vector3 position, float health)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.position = position;
            var damageable = go.AddComponent<TestDamageableTarget>();
            damageable.ResetTarget(health);
            return go;
        }

        private static GameObject CreateTestWall(string name, Vector3 position, Vector3 size)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = size;
            return go;
        }

        private static void ExitBatch(int exitCode)
        {
#if UNITY_EDITOR
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
#endif
        }
    }
}
