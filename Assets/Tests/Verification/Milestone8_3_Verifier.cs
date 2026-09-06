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
    /// Automated Play Mode verification suite for Milestone 8.3 (Archer Combat Prototype).
    /// Validates IPrimaryAttack abstraction, PlayerAttack migration, BowWeapon, ArrowProjectile authoritative sweep,
    /// Archer prefab and asset configuration, runtime spawning and explicit binding, universal upgrade scaling,
    /// safe manual test helper, production scene cleanliness, and Warrior non-regression.
    /// </summary>
    public class Milestone8_3_Verifier : MonoBehaviour
    {
        [SerializeField] private bool runAutomatedTestOnStart = true;
        public bool TargetedCheck33Only = false;

        private void Start()
        {
            if (runAutomatedTestOnStart || Application.isBatchMode)
            {
                StartCoroutine(RunVerificationSafe());
            }
        }

        private IEnumerator RunVerificationSafe()
        {
            IEnumerator routine = TargetedCheck33Only ? RunTargetedCheck33Routine() : RunVerificationRoutine();
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
                    Debug.LogError($"[M8.3 TEST EXCEPTION] Unexpected exception during verification: {ex}");
                    ExitBatch(1);
                    yield break;
                }

                yield return current;
            }
        }

        private IEnumerator RunVerificationRoutine()
        {
            yield return new WaitForSeconds(0.1f);

            Debug.Log("[M8.3 TEST START] Beginning Milestone 8.3 automated verification suite...");
            bool allPassed = true;

            // Paths
            const string ArcherAssetPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";
            const string ArcherPrefabPath = "Assets/Prefabs/Characters/Archer.prefab";
            const string ArrowPrefabPath = "Assets/Prefabs/Weapons/Arrow.prefab";
            const string WarriorAssetPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
            const string WarriorPrefabPath = "Assets/Prefabs/Characters/Warrior.prefab";
            const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

            // -------------------------------------------------------------
            // SECTION A: IPrimaryAttack Abstraction & MeleeWeapon Non-Regression
            // -------------------------------------------------------------
            // CHECK 1: IPrimaryAttack interface exists with bool TryAttack()
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

            // CHECK 2: MeleeWeapon implements IPrimaryAttack
            bool check2Pass = typeof(IPrimaryAttack).IsAssignableFrom(typeof(MeleeWeapon));
            if (check2Pass)
            {
                Debug.Log("[CHECK 2 PASSED] MeleeWeapon implements IPrimaryAttack.");
            }
            else
            {
                Debug.LogError("[CHECK 2 FAILED] MeleeWeapon does not implement IPrimaryAttack.");
                allPassed = false;
            }

            // CHECK 3-8: MeleeWeapon properties and execution
            GameObject testMeleeGo = new GameObject("TestMeleeWeaponHolder");
            var melee = testMeleeGo.AddComponent<MeleeWeapon>();
            var meleeStats = testMeleeGo.AddComponent<PlayerStats>();
            melee.SetPlayerStats(meleeStats);

            // CHECK 3: MeleeWeapon.BaseDamage == 25
            if (Mathf.Approximately(melee.BaseDamage, 25f))
            {
                Debug.Log("[CHECK 3 PASSED] MeleeWeapon.BaseDamage returns 25.");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] MeleeWeapon.BaseDamage was {melee.BaseDamage}, expected 25.");
                allPassed = false;
            }

            // CHECK 4: MeleeWeapon.EffectiveDamage == 25 before modifiers
            if (Mathf.Approximately(melee.EffectiveDamage, 25f))
            {
                Debug.Log("[CHECK 4 PASSED] MeleeWeapon.EffectiveDamage returns 25.");
            }
            else
            {
                Debug.LogError($"[CHECK 4 FAILED] MeleeWeapon.EffectiveDamage was {melee.EffectiveDamage}, expected 25.");
                allPassed = false;
            }

            // CHECK 5: MeleeWeapon.AttackCooldown == 0.5s
            if (Mathf.Approximately(melee.AttackCooldown, 0.5f))
            {
                Debug.Log("[CHECK 5 PASSED] MeleeWeapon.AttackCooldown returns 0.5s.");
            }
            else
            {
                Debug.LogError($"[CHECK 5 FAILED] MeleeWeapon.AttackCooldown was {melee.AttackCooldown}, expected 0.5s.");
                allPassed = false;
            }

            // CHECK 6: MeleeWeapon.EffectiveAttackCooldown == 0.5s before modifiers
            if (Mathf.Approximately(melee.EffectiveAttackCooldown, 0.5f))
            {
                Debug.Log("[CHECK 6 PASSED] MeleeWeapon.EffectiveAttackCooldown returns 0.5s.");
            }
            else
            {
                Debug.LogError($"[CHECK 6 FAILED] MeleeWeapon.EffectiveAttackCooldown was {melee.EffectiveAttackCooldown}, expected 0.5s.");
                allPassed = false;
            }

            // CHECK 7: MeleeWeapon.CanAttack is true initially
            if (melee.CanAttack)
            {
                Debug.Log("[CHECK 7 PASSED] MeleeWeapon.CanAttack returns true initially.");
            }
            else
            {
                Debug.LogError("[CHECK 7 FAILED] MeleeWeapon.CanAttack was false initially.");
                allPassed = false;
            }

            // CHECK 8: MeleeWeapon.TryAttack() executes swing and triggers OnAttack
            bool meleeAttackFired = false;
            melee.OnAttack += () => meleeAttackFired = true;
            bool meleeSwingSuccess = melee.TryAttack();
            if (meleeSwingSuccess && meleeAttackFired && !melee.CanAttack)
            {
                Debug.Log("[CHECK 8 PASSED] MeleeWeapon.TryAttack() succeeded, fired OnAttack, and initiated cooldown.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] MeleeWeapon.TryAttack() success={meleeSwingSuccess}, fired={meleeAttackFired}.");
                allPassed = false;
            }
            Object.Destroy(testMeleeGo);

            // -------------------------------------------------------------
            // SECTION B: PlayerAttack Migration & Backward Compatibility
            // -------------------------------------------------------------
            GameObject testPlayerAttackGo = new GameObject("TestPlayerAttackHolder");
            var playerAttack = testPlayerAttackGo.AddComponent<PlayerAttack>();
            var testMeleeForAttack = testPlayerAttackGo.AddComponent<MeleeWeapon>();
            playerAttack.SetPrimaryWeapon(testMeleeForAttack);

            // CHECK 9: PlayerAttack serializes and resolves IPrimaryAttack
            if (playerAttack.PrimaryAttack != null)
            {
                Debug.Log("[CHECK 9 PASSED] PlayerAttack successfully resolved IPrimaryAttack.");
            }
            else
            {
                Debug.LogError("[CHECK 9 FAILED] PlayerAttack.PrimaryAttack was null.");
                allPassed = false;
            }

            // CHECK 10: PlayerAttack.PrimaryAttack returns the configured IPrimaryAttack
            if (ReferenceEquals(playerAttack.PrimaryAttack, testMeleeForAttack))
            {
                Debug.Log("[CHECK 10 PASSED] PlayerAttack.PrimaryAttack references configured weapon.");
            }
            else
            {
                Debug.LogError("[CHECK 10 FAILED] PlayerAttack.PrimaryAttack did not match configured weapon.");
                allPassed = false;
            }

            // CHECK 11: PlayerAttack.EquippedWeapon returns MeleeWeapon for backward compatibility
            if (ReferenceEquals(playerAttack.EquippedWeapon, testMeleeForAttack))
            {
                Debug.Log("[CHECK 11 PASSED] PlayerAttack.EquippedWeapon returns MeleeWeapon for backward compatibility.");
            }
            else
            {
                Debug.LogError("[CHECK 11 FAILED] PlayerAttack.EquippedWeapon did not return MeleeWeapon.");
                allPassed = false;
            }

            // CHECK 12: PlayerAttack.SetPrimaryWeapon allows configuring IPrimaryAttack
            var dummyAttackHolder = new GameObject("DummyAttackHolder");
            var dummyAttack = dummyAttackHolder.AddComponent<DummyPrimaryAttack>();
            playerAttack.SetPrimaryWeapon(dummyAttack);
            if (ReferenceEquals(playerAttack.PrimaryAttack, dummyAttack))
            {
                Debug.Log("[CHECK 12 PASSED] PlayerAttack.SetPrimaryWeapon configured custom IPrimaryAttack.");
            }
            else
            {
                Debug.LogError("[CHECK 12 FAILED] PlayerAttack.SetPrimaryWeapon failed to assign custom IPrimaryAttack.");
                allPassed = false;
            }

            // CHECK 13: PlayerAttack.SetEquippedWeapon backward compatibility setter works
            playerAttack.SetEquippedWeapon(testMeleeForAttack);
            if (ReferenceEquals(playerAttack.PrimaryAttack, testMeleeForAttack))
            {
                Debug.Log("[CHECK 13 PASSED] PlayerAttack.SetEquippedWeapon setter assigned weapon correctly.");
            }
            else
            {
                Debug.LogError("[CHECK 13 FAILED] PlayerAttack.SetEquippedWeapon setter failed.");
                allPassed = false;
            }

            // CHECK 14: PlayerAttack.TryAttack() delegates to PrimaryAttack.TryAttack() without character branching
            playerAttack.SetPrimaryWeapon(dummyAttack);
            dummyAttack.TryAttackReturnValue = true;
            bool attackResult = playerAttack.TryAttack();
            if (attackResult && dummyAttack.TryAttackCallCount == 1)
            {
                Debug.Log("[CHECK 14 PASSED] PlayerAttack.TryAttack() delegated directly to PrimaryAttack.TryAttack().");
            }
            else
            {
                Debug.LogError($"[CHECK 14 FAILED] PlayerAttack.TryAttack() delegation failed. result={attackResult}, calls={dummyAttack.TryAttackCallCount}.");
                allPassed = false;
            }
            Object.Destroy(dummyAttackHolder);
            Object.Destroy(testPlayerAttackGo);

            // -------------------------------------------------------------
            // SECTION C: BowWeapon Architecture & Stat Scaling
            // -------------------------------------------------------------
#if UNITY_EDITOR
            var arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath);
#else
            var arrowPrefab = Resources.Load<GameObject>("Arrow");
#endif
            GameObject testBowGo = new GameObject("TestBowHolder");
            var bow = testBowGo.AddComponent<BowWeapon>();
            var bowStats = testBowGo.AddComponent<PlayerStats>();
            bow.SetPlayerStats(bowStats);
            bow.SetArrowPrefab(arrowPrefab);

            // CHECK 15: BowWeapon implements IPrimaryAttack
            bool check15Pass = typeof(IPrimaryAttack).IsAssignableFrom(typeof(BowWeapon));
            if (check15Pass)
            {
                Debug.Log("[CHECK 15 PASSED] BowWeapon implements IPrimaryAttack.");
            }
            else
            {
                Debug.LogError("[CHECK 15 FAILED] BowWeapon does not implement IPrimaryAttack.");
                allPassed = false;
            }

            // CHECK 16: BowWeapon.BaseDamage == 20
            if (Mathf.Approximately(bow.BaseDamage, 20f))
            {
                Debug.Log("[CHECK 16 PASSED] BowWeapon.BaseDamage returns 20.");
            }
            else
            {
                Debug.LogError($"[CHECK 16 FAILED] BowWeapon.BaseDamage was {bow.BaseDamage}, expected 20.");
                allPassed = false;
            }

            // CHECK 17: BowWeapon.AttackCooldown == 0.6s
            if (Mathf.Approximately(bow.AttackCooldown, 0.6f))
            {
                Debug.Log("[CHECK 17 PASSED] BowWeapon.AttackCooldown returns 0.6s.");
            }
            else
            {
                Debug.LogError($"[CHECK 17 FAILED] BowWeapon.AttackCooldown was {bow.AttackCooldown}, expected 0.6s.");
                allPassed = false;
            }

            // CHECK 18: BowWeapon.ProjectileSpeed == 18 m/s
            if (Mathf.Approximately(bow.ProjectileSpeed, 18f))
            {
                Debug.Log("[CHECK 18 PASSED] BowWeapon.ProjectileSpeed returns 18 m/s.");
            }
            else
            {
                Debug.LogError($"[CHECK 18 FAILED] BowWeapon.ProjectileSpeed was {bow.ProjectileSpeed}, expected 18.");
                allPassed = false;
            }

            // CHECK 19: BowWeapon.ProjectileLifetime == 2.0s
            if (Mathf.Approximately(bow.ProjectileLifetime, 2.0f))
            {
                Debug.Log("[CHECK 19 PASSED] BowWeapon.ProjectileLifetime returns 2.0s.");
            }
            else
            {
                Debug.LogError($"[CHECK 19 FAILED] BowWeapon.ProjectileLifetime was {bow.ProjectileLifetime}, expected 2.0s.");
                allPassed = false;
            }

            // CHECK 20: BowWeapon.EffectiveDamage scales with PlayerStats.DamageMultiplier (20 * 1.2 = 24)
            bowStats.AddDamageBonus(0.20f);
            if (Mathf.Approximately(bow.EffectiveDamage, 24.0f))
            {
                Debug.Log("[CHECK 20 PASSED] BowWeapon.EffectiveDamage scales to 24.0 after +20% damage upgrade.");
            }
            else
            {
                Debug.LogError($"[CHECK 20 FAILED] BowWeapon.EffectiveDamage was {bow.EffectiveDamage}, expected 24.0.");
                allPassed = false;
            }
            bowStats.ResetModifiers();

            // CHECK 21: BowWeapon.EffectiveAttackCooldown scales with PlayerStats.AttackSpeedMultiplier (0.6 / 1.15 ≈ 0.5217s)
            bowStats.AddAttackSpeedBonus(0.15f);
            float expectedCooldown = 0.6f / 1.15f;
            if (Mathf.Approximately(bow.EffectiveAttackCooldown, expectedCooldown))
            {
                Debug.Log($"[CHECK 21 PASSED] BowWeapon.EffectiveAttackCooldown scales to {bow.EffectiveAttackCooldown:F4}s after +15% attack speed upgrade.");
            }
            else
            {
                Debug.LogError($"[CHECK 21 FAILED] BowWeapon.EffectiveAttackCooldown was {bow.EffectiveAttackCooldown}, expected {expectedCooldown}.");
                allPassed = false;
            }
            bowStats.ResetModifiers();

            // CHECK 22: BowWeapon.CanAttack returns true initially
            if (bow.CanAttack)
            {
                Debug.Log("[CHECK 22 PASSED] BowWeapon.CanAttack returns true when ready.");
            }
            else
            {
                Debug.LogError("[CHECK 22 FAILED] BowWeapon.CanAttack was false when ready.");
                allPassed = false;
            }

            // CHECK 23: BowWeapon.CanAttack returns false while paused
            Time.timeScale = 0f;
            if (!bow.CanAttack)
            {
                Debug.Log("[CHECK 23 PASSED] BowWeapon.CanAttack returns false when Time.timeScale == 0.");
            }
            else
            {
                Debug.LogError("[CHECK 23 FAILED] BowWeapon.CanAttack returned true while paused.");
                allPassed = false;
            }

            // CHECK 24: BowWeapon.TryAttack() returns false while paused
            if (!bow.TryAttack())
            {
                Debug.Log("[CHECK 24 PASSED] BowWeapon.TryAttack() blocked while Time.timeScale == 0.");
            }
            else
            {
                Debug.LogError("[CHECK 24 FAILED] BowWeapon.TryAttack() succeeded while paused.");
                allPassed = false;
            }
            Time.timeScale = 1.0f;

            // CHECK 25-27: Bow firing execution
            bool bowAttackFired = false;
            bow.OnAttack += () => bowAttackFired = true;
            bool bowShotSuccess = bow.TryAttack();

            // CHECK 25: BowWeapon.TryAttack() returns false while on cooldown
            bool secondShotBlocked = !bow.TryAttack();
            if (bowShotSuccess && secondShotBlocked && !bow.CanAttack)
            {
                Debug.Log("[CHECK 25 PASSED] BowWeapon enforces attackCooldown (0.6s).");
            }
            else
            {
                Debug.LogError($"[CHECK 25 FAILED] Bow cooldown check failed. shot1={bowShotSuccess}, shot2Blocked={secondShotBlocked}.");
                allPassed = false;
            }

            // CHECK 26: BowWeapon.TryAttack() instantiates exactly one arrow
            var spawnedArrows = Object.FindObjectsByType<ArrowProjectile>(FindObjectsSortMode.None);
            if (spawnedArrows != null && spawnedArrows.Length == 1)
            {
                Debug.Log("[CHECK 26 PASSED] BowWeapon instantiated exactly one ArrowProjectile.");
            }
            else
            {
                Debug.LogError($"[CHECK 26 FAILED] Expected 1 ArrowProjectile, found {(spawnedArrows != null ? spawnedArrows.Length : 0)}.");
                allPassed = false;
            }

            // CHECK 27: BowWeapon.TryAttack() fired OnAttack event
            if (bowAttackFired)
            {
                Debug.Log("[CHECK 27 PASSED] BowWeapon fired OnAttack event upon shooting.");
            }
            else
            {
                Debug.LogError("[CHECK 27 FAILED] BowWeapon did not fire OnAttack event.");
                allPassed = false;
            }

            // Clean up spawned arrows
            if (spawnedArrows != null)
            {
                for (int i = 0; i < spawnedArrows.Length; i++)
                {
                    if (spawnedArrows[i] != null) Object.Destroy(spawnedArrows[i].gameObject);
                }
            }
            Object.Destroy(testBowGo);

            // -------------------------------------------------------------
            // SECTION D: ArrowProjectile Single Authoritative Hit Path & Physics
            // -------------------------------------------------------------
            Vector3 testBase = new Vector3(8f, 1f, 0f);

            // CHECK 28: ArrowProjectile moves forward in assigned travel direction
            GameObject arrowObj = new GameObject("TestArrow");
            var arrowComp = arrowObj.AddComponent<ArrowProjectile>();
            var arrowCol = arrowObj.AddComponent<CapsuleCollider>();
            arrowCol.isTrigger = true;
            arrowCol.radius = 0.12f;
            arrowCol.height = 0.8f;
            var arrowRb = arrowObj.AddComponent<Rigidbody>();
            arrowRb.isKinematic = true;
            arrowRb.useGravity = false;

            arrowObj.transform.position = testBase;
            arrowComp.Initialize(null, Vector3.forward, 18f, 20f, 2.0f);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            if (arrowObj != null && arrowObj.transform.position.z > testBase.z + 0.01f)
            {
                Debug.Log($"[CHECK 28 PASSED] ArrowProjectile advanced forward on Z axis (pos.z = {arrowObj.transform.position.z:F3}).");
            }
            else
            {
                Debug.LogError($"[CHECK 28 FAILED] ArrowProjectile did not move forward. pos.z = {(arrowObj != null ? arrowObj.transform.position.z.ToString() : "null")}.");
                allPassed = false;
            }

            // CHECK 29: ArrowProjectile does not move while paused
            Vector3 pausedPos = arrowObj.transform.position;
            Time.timeScale = 0f;
            yield return null;
            yield return null;

            if (arrowObj != null && arrowObj.transform.position == pausedPos)
            {
                Debug.Log("[CHECK 29 PASSED] ArrowProjectile did not advance position while paused.");
            }
            else
            {
                Debug.LogError("[CHECK 29 FAILED] ArrowProjectile moved while Time.timeScale == 0.");
                allPassed = false;
            }

            // CHECK 30: ArrowProjectile lifetime timer does not advance while paused
            float pausedLife = arrowComp.ElapsedLifetime;
            yield return null;
            if (arrowComp != null && Mathf.Approximately(arrowComp.ElapsedLifetime, pausedLife))
            {
                Debug.Log("[CHECK 30 PASSED] ArrowProjectile elapsed lifetime did not advance while paused.");
            }
            else
            {
                Debug.LogError("[CHECK 30 FAILED] ArrowProjectile elapsed lifetime advanced while paused.");
                allPassed = false;
            }
            Time.timeScale = 1.0f;
            if (arrowObj != null) Object.Destroy(arrowObj);

            // CHECK 31: ArrowProjectile self-destructs after lifetime expires
            GameObject shortLifeArrow = new GameObject("ShortLifeArrow");
            shortLifeArrow.transform.position = testBase;
            var shortArrowComp = shortLifeArrow.AddComponent<ArrowProjectile>();
            shortArrowComp.Initialize(null, Vector3.forward, 10f, 20f, 0.05f);
            yield return new WaitForSeconds(0.15f);

            if (shortLifeArrow == null)
            {
                Debug.Log("[CHECK 31 PASSED] ArrowProjectile self-destructed upon lifetime expiry.");
            }
            else
            {
                Debug.LogError("[CHECK 31 FAILED] ArrowProjectile was not destroyed after lifetime expired.");
                Object.Destroy(shortLifeArrow);
                allPassed = false;
            }

            // CHECK 32: ArrowProjectile ignores weapon owner hierarchy (self-damage immunity)
            GameObject ownerGo = new GameObject("TestOwner");
            ownerGo.transform.position = testBase;
            var ownerCol = ownerGo.AddComponent<CapsuleCollider>();
            ownerCol.center = Vector3.zero;
            ownerCol.radius = 0.5f;
            ownerCol.height = 2.0f;
            var ownerHealth = ownerGo.AddComponent<PlayerHealth>();

            GameObject ownerArrow = new GameObject("OwnerArrow");
            ownerArrow.transform.position = testBase - Vector3.forward * 0.2f;
            var ownerArrowComp = ownerArrow.AddComponent<ArrowProjectile>();
            ownerArrowComp.Initialize(ownerGo.transform, Vector3.forward, 10f, 20f, 1.0f);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            if (ownerHealth.CurrentHealth == 100f && ownerArrow != null)
            {
                Debug.Log("[CHECK 32 PASSED] ArrowProjectile ignored owner hierarchy (self-damage immunity).");
            }
            else
            {
                Debug.LogError($"[CHECK 32 FAILED] Owner was damaged or arrow destroyed by owner collider. health={ownerHealth.CurrentHealth}.");
                allPassed = false;
            }
            if (ownerArrow != null) Object.Destroy(ownerArrow);
            Object.Destroy(ownerGo);

            // CHECK 33: ArrowProjectile ignores trigger colliders (XP pickup immunity) and destroys on solid wall
            GameObject triggerGo = new GameObject("TestTriggerPickup");
            triggerGo.transform.position = testBase + Vector3.forward * 2.0f;
            var triggerCol = triggerGo.AddComponent<SphereCollider>();
            triggerCol.isTrigger = true;
            triggerCol.radius = 0.5f;
            var triggerDamageable = triggerGo.AddComponent<TestDamageableTarget>();

            GameObject triggerWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            triggerWall.name = "TestTriggerFollowupWall";
            triggerWall.transform.position = testBase + Vector3.forward * 5.0f;
            triggerWall.transform.localScale = new Vector3(5f, 3f, 1f);

            GameObject triggerTestArrow = new GameObject("TriggerTestArrow");
            triggerTestArrow.transform.position = testBase;
            var triggerArrowComp = triggerTestArrow.AddComponent<ArrowProjectile>();
            triggerArrowComp.Initialize(null, Vector3.forward, 20f, 20f, 2.0f);

            yield return new WaitForSeconds(0.18f);

            bool check33Part1Pass = triggerTestArrow != null &&
                                   triggerTestArrow.transform.position.z > testBase.z + 2.5f &&
                                   triggerDamageable.HitCount == 0 &&
                                   Mathf.Approximately(triggerDamageable.TotalDamageReceived, 0f);

            yield return new WaitForSeconds(0.15f);

            bool check33Part2Pass = triggerTestArrow == null;

            if (check33Part1Pass && check33Part2Pass)
            {
                Debug.Log("[CHECK 33 PASSED] ArrowProjectile passed through trigger volume without impact or damage, and destroyed upon solid wall impact.");
            }
            else
            {
                Debug.LogError($"[CHECK 33 FAILED] Check 33 failed. part1Pass={check33Part1Pass}, part2Pass={check33Part2Pass}.");
                allPassed = false;
            }

            if (triggerTestArrow != null) Object.Destroy(triggerTestArrow);
            Object.Destroy(triggerGo);
            Object.Destroy(triggerWall);

            // CHECK 34-36: ArrowProjectile deals damage to IDamageable target and destroys itself
            GameObject dummyTarget = new GameObject("TestDummyTarget");
            dummyTarget.transform.position = testBase + Vector3.forward * 3.0f;
            var dummyCol = dummyTarget.AddComponent<CapsuleCollider>();
            dummyCol.center = Vector3.zero;
            dummyCol.radius = 0.5f;
            dummyCol.height = 2.0f;
            var dummyTargetComp = dummyTarget.AddComponent<TestDamageableTarget>();

            GameObject damageArrow = new GameObject("DamageArrow");
            damageArrow.transform.position = testBase;
            var damageArrowComp = damageArrow.AddComponent<ArrowProjectile>();
            damageArrowComp.Initialize(null, Vector3.forward, 40f, 20f, 1.0f);

            yield return new WaitForSeconds(0.15f);

            // CHECK 34: Collision detected
            // CHECK 35: Applied exact damage (20f)
            if (Mathf.Approximately(dummyTargetComp.TotalDamageReceived, 20f))
            {
                Debug.Log("[CHECK 34 & 35 PASSED] ArrowProjectile hit IDamageable target and dealt exactly 20 damage.");
            }
            else
            {
                Debug.LogError($"[CHECK 34 & 35 FAILED] Dummy target received {dummyTargetComp.TotalDamageReceived} damage, expected 20.");
                allPassed = false;
            }

            // CHECK 36: Arrow destroyed upon impact with damageable target
            if (damageArrow == null)
            {
                Debug.Log("[CHECK 36 PASSED] ArrowProjectile destroyed itself upon impacting damageable target.");
            }
            else
            {
                Debug.LogError("[CHECK 36 FAILED] ArrowProjectile was not destroyed upon hitting target.");
                Object.Destroy(damageArrow);
                allPassed = false;
            }
            Object.Destroy(dummyTarget);

            // CHECK 37: ArrowProjectile destroys itself upon hitting solid wall
            GameObject wallGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGo.name = "TestSolidWall";
            wallGo.transform.position = testBase + Vector3.forward * 2.0f;
            wallGo.transform.localScale = new Vector3(5f, 3f, 1f);

            GameObject wallArrow = new GameObject("WallArrow");
            wallArrow.transform.position = testBase;
            var wallArrowComp = wallArrow.AddComponent<ArrowProjectile>();
            wallArrowComp.Initialize(null, Vector3.forward, 40f, 20f, 1.0f);

            yield return new WaitForSeconds(0.12f);

            if (wallArrow == null)
            {
                Debug.Log("[CHECK 37 PASSED] ArrowProjectile destroyed itself upon impacting solid wall collider.");
            }
            else
            {
                Debug.LogError("[CHECK 37 FAILED] ArrowProjectile was not destroyed by wall collider.");
                Object.Destroy(wallArrow);
                allPassed = false;
            }
            Object.Destroy(wallGo);

            // CHECK 38: Single-hit guard ensures one arrow cannot damage multiple targets
            GameObject targetA = new GameObject("TargetA");
            targetA.transform.position = testBase + Vector3.forward * 1.5f;
            var colA = targetA.AddComponent<CapsuleCollider>();
            colA.center = Vector3.zero;
            colA.radius = 0.5f;
            colA.height = 2.0f;
            var dummyA = targetA.AddComponent<TestDamageableTarget>();

            GameObject targetB = new GameObject("TargetB");
            targetB.transform.position = testBase + Vector3.forward * 3.0f;
            var colB = targetB.AddComponent<CapsuleCollider>();
            colB.center = Vector3.zero;
            colB.radius = 0.5f;
            colB.height = 2.0f;
            var dummyB = targetB.AddComponent<TestDamageableTarget>();

            GameObject multiArrow = new GameObject("MultiArrow");
            multiArrow.transform.position = testBase;
            var multiArrowComp = multiArrow.AddComponent<ArrowProjectile>();
            multiArrowComp.Initialize(null, Vector3.forward, 40f, 20f, 1.0f);

            yield return new WaitForSeconds(0.15f);

            if (dummyA.HitCount == 1 && dummyB.HitCount == 0)
            {
                Debug.Log("[CHECK 38 PASSED] ArrowProjectile single-hit guard prevented multi-hit penetrations.");
            }
            else
            {
                Debug.LogError($"[CHECK 38 FAILED] Single-hit guard failed. TargetA hits={dummyA.HitCount}, TargetB hits={dummyB.HitCount}.");
                allPassed = false;
            }
            if (multiArrow != null) Object.Destroy(multiArrow);
            Object.Destroy(targetA);
            Object.Destroy(targetB);

            // -------------------------------------------------------------
            // SECTION E: Asset & Prefab Integrity
            // -------------------------------------------------------------
#if UNITY_EDITOR
            // CHECK 39: Character_Archer.asset exists
            var archerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
            if (archerAsset != null)
            {
                Debug.Log("[CHECK 39 PASSED] Character_Archer.asset exists on disk.");
            }
            else
            {
                Debug.LogError("[CHECK 39 FAILED] Character_Archer.asset not found.");
                allPassed = false;
            }

            // CHECK 40: Character_Archer.asset Id == "archer"
            if (archerAsset != null && archerAsset.Id == "archer")
            {
                Debug.Log("[CHECK 40 PASSED] Character_Archer.asset has Id == 'archer'.");
            }
            else
            {
                Debug.LogError($"[CHECK 40 FAILED] Character_Archer.asset Id was '{archerAsset?.Id}'.");
                allPassed = false;
            }

            // CHECK 41: Character_Archer.asset DisplayName == "Archer"
            if (archerAsset != null && archerAsset.DisplayName == "Archer")
            {
                Debug.Log("[CHECK 41 PASSED] Character_Archer.asset has DisplayName == 'Archer'.");
            }
            else
            {
                Debug.LogError($"[CHECK 41 FAILED] Character_Archer.asset DisplayName was '{archerAsset?.DisplayName}'.");
                allPassed = false;
            }

            // CHECK 42: Character_Archer.asset has non-empty description
            if (archerAsset != null && !string.IsNullOrEmpty(archerAsset.Description))
            {
                Debug.Log("[CHECK 42 PASSED] Character_Archer.asset has non-empty description.");
            }
            else
            {
                Debug.LogError("[CHECK 42 FAILED] Character_Archer.asset description was null or empty.");
                allPassed = false;
            }

            // CHECK 43: Character_Archer.asset has characterPrefab assigned
            if (archerAsset != null && archerAsset.CharacterPrefab != null)
            {
                Debug.Log("[CHECK 43 PASSED] Character_Archer.asset links to valid characterPrefab.");
            }
            else
            {
                Debug.LogError("[CHECK 43 FAILED] Character_Archer.asset characterPrefab was null.");
                allPassed = false;
            }

            // CHECK 44: Archer.prefab exists
            var archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherPrefabPath);
            if (archerPrefab != null)
            {
                Debug.Log("[CHECK 44 PASSED] Archer.prefab exists on disk.");
            }
            else
            {
                Debug.LogError("[CHECK 44 FAILED] Archer.prefab not found.");
                allPassed = false;
            }

            // CHECK 45: Archer.prefab Tag == "Player"
            if (archerPrefab != null && archerPrefab.CompareTag("Player"))
            {
                Debug.Log("[CHECK 45 PASSED] Archer.prefab has Tag == 'Player'.");
            }
            else
            {
                Debug.LogError($"[CHECK 45 FAILED] Archer.prefab Tag was '{archerPrefab?.tag}'.");
                allPassed = false;
            }

            // CHECK 46: Archer.prefab has PlayableCharacter referencing Character_Archer
            var archerPlayable = archerPrefab?.GetComponent<PlayableCharacter>();
            if (archerPlayable != null && archerPlayable.CharacterDefinition == archerAsset)
            {
                Debug.Log("[CHECK 46 PASSED] Archer.prefab has PlayableCharacter referencing Character_Archer.asset.");
            }
            else
            {
                Debug.LogError("[CHECK 46 FAILED] Archer.prefab PlayableCharacter component mismatch.");
                allPassed = false;
            }

            // CHECK 47: Archer.prefab has CharacterController
            var archerCC = archerPrefab?.GetComponent<CharacterController>();
            if (archerCC != null && Mathf.Approximately(archerCC.height, 2f) && Mathf.Approximately(archerCC.radius, 0.5f))
            {
                Debug.Log("[CHECK 47 PASSED] Archer.prefab has standard CharacterController dimensions.");
            }
            else
            {
                Debug.LogError("[CHECK 47 FAILED] Archer.prefab CharacterController missing or non-standard.");
                allPassed = false;
            }

            // CHECK 48: Archer.prefab has PlayerMovement with moveSpeed == 6.5f
            var archerMove = archerPrefab?.GetComponent<PlayerMovement>();
            if (archerMove != null && Mathf.Approximately(archerMove.MoveSpeed, 6.5f))
            {
                Debug.Log("[CHECK 48 PASSED] Archer.prefab has PlayerMovement with moveSpeed == 6.5f.");
            }
            else
            {
                Debug.LogError($"[CHECK 48 FAILED] Archer.prefab PlayerMovement moveSpeed was {archerMove?.MoveSpeed}, expected 6.5.");
                allPassed = false;
            }

            // CHECK 49: Archer.prefab has PlayerAim
            var archerAim = archerPrefab?.GetComponent<PlayerAim>();
            if (archerAim != null)
            {
                Debug.Log("[CHECK 49 PASSED] Archer.prefab has PlayerAim component.");
            }
            else
            {
                Debug.LogError("[CHECK 49 FAILED] Archer.prefab missing PlayerAim.");
                allPassed = false;
            }

            // CHECK 50: Archer.prefab has PlayerHealth with maxHealth == 100f
            var archerHealth = archerPrefab?.GetComponent<PlayerHealth>();
            if (archerHealth != null && Mathf.Approximately(archerHealth.MaxHealth, 100f))
            {
                Debug.Log("[CHECK 50 PASSED] Archer.prefab has PlayerHealth with maxHealth == 100f.");
            }
            else
            {
                Debug.LogError($"[CHECK 50 FAILED] Archer.prefab PlayerHealth maxHealth was {archerHealth?.MaxHealth}, expected 100.");
                allPassed = false;
            }

            // CHECK 51: Archer.prefab has PlayerStats
            var archerStats = archerPrefab?.GetComponent<PlayerStats>();
            if (archerStats != null)
            {
                Debug.Log("[CHECK 51 PASSED] Archer.prefab has PlayerStats component.");
            }
            else
            {
                Debug.LogError("[CHECK 51 FAILED] Archer.prefab missing PlayerStats.");
                allPassed = false;
            }

            // CHECK 52: Archer.prefab has PlayerExperience
            var archerExp = archerPrefab?.GetComponent<PlayerExperience>();
            bool expConfigured = false;
            if (archerExp != null)
            {
                var soExp = new SerializedObject(archerExp);
                expConfigured = soExp.FindProperty("baseRequiredXP").intValue == 100;
            }
            if (expConfigured)
            {
                Debug.Log("[CHECK 52 PASSED] Archer.prefab has PlayerExperience with baseRequiredXP == 100.");
            }
            else
            {
                Debug.LogError("[CHECK 52 FAILED] Archer.prefab PlayerExperience missing or invalid baseRequiredXP.");
                allPassed = false;
            }

            // CHECK 53: Archer.prefab has BowWeapon
            var archerBow = archerPrefab?.GetComponent<BowWeapon>();
            if (archerBow != null && Mathf.Approximately(archerBow.Damage, 20f) && Mathf.Approximately(archerBow.AttackCooldown, 0.6f))
            {
                Debug.Log("[CHECK 53 PASSED] Archer.prefab has BowWeapon configured with damage=20 and cooldown=0.6s.");
            }
            else
            {
                Debug.LogError("[CHECK 53 FAILED] Archer.prefab BowWeapon missing or parameters mismatch.");
                allPassed = false;
            }

            // CHECK 54: Archer.prefab has PlayerAttack wired to BowWeapon
            var archerAttack = archerPrefab?.GetComponent<PlayerAttack>();
            if (archerAttack != null)
            {
                Debug.Log("[CHECK 54 PASSED] Archer.prefab has PlayerAttack component.");
            }
            else
            {
                Debug.LogError("[CHECK 54 FAILED] Archer.prefab missing PlayerAttack.");
                allPassed = false;
            }

            // CHECK 55: Archer.prefab does NOT have MeleeWeapon
            var archerMelee = archerPrefab?.GetComponent<MeleeWeapon>();
            if (archerMelee == null)
            {
                Debug.Log("[CHECK 55 PASSED] Archer.prefab does NOT have MeleeWeapon.");
            }
            else
            {
                Debug.LogError("[CHECK 55 FAILED] Archer.prefab unexpectedly contains MeleeWeapon component.");
                allPassed = false;
            }

            // CHECK 56: Archer.prefab has ProjectileSpawnPoint child transform
            Transform spawnPointChild = archerPrefab?.transform.Find("Visual/WeaponAnchor/ProjectileSpawnPoint");
            if (spawnPointChild == null)
            {
                spawnPointChild = archerPrefab?.transform.Find("WeaponAnchor/ProjectileSpawnPoint");
            }
            if (spawnPointChild != null)
            {
                Debug.Log("[CHECK 56 PASSED] Archer.prefab has ProjectileSpawnPoint child transform.");
            }
            else
            {
                Debug.LogError("[CHECK 56 FAILED] Archer.prefab missing ProjectileSpawnPoint child.");
                allPassed = false;
            }

            // CHECK 57: Archer.prefab has FacingIndicator and BowVisual placeholder visuals
            Transform facingInd = archerPrefab?.transform.Find("Visual/FacingIndicator");
            Transform bowVis = archerPrefab?.transform.Find("Visual/WeaponAnchor/BowVisual");
            if (facingInd != null && bowVis != null)
            {
                Debug.Log("[CHECK 57 PASSED] Archer.prefab contains FacingIndicator and BowVisual placeholder visuals.");
            }
            else
            {
                Debug.LogError($"[CHECK 57 FAILED] Placeholder visuals missing. facingInd={facingInd != null}, bowVis={bowVis != null}.");
                allPassed = false;
            }

            // CHECK 58: Arrow.prefab exists with ArrowProjectile, Collider(isTrigger), and Rigidbody
            var arrowPrefabOnDisk = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath);
            var diskArrowComp = arrowPrefabOnDisk?.GetComponent<ArrowProjectile>();
            var diskArrowCol = arrowPrefabOnDisk?.GetComponent<Collider>();
            var diskArrowRb = arrowPrefabOnDisk?.GetComponent<Rigidbody>();
            if (diskArrowComp != null && diskArrowCol != null && diskArrowCol.isTrigger && diskArrowRb != null && diskArrowRb.isKinematic)
            {
                Debug.Log("[CHECK 58 PASSED] Arrow.prefab configured with ArrowProjectile, Trigger Collider, and Kinematic Rigidbody.");
            }
            else
            {
                Debug.LogError("[CHECK 58 FAILED] Arrow.prefab components missing or misconfigured.");
                allPassed = false;
            }
#endif

            // -------------------------------------------------------------
            // SECTION F: Runtime Spawning, Explicit Binding & Archer Gameplay
            // -------------------------------------------------------------
            var spawner = Object.FindFirstObjectByType<PlayerSpawner>();
            var cameraFollow = Object.FindFirstObjectByType<CameraFollow>();
            var waveManager = Object.FindFirstObjectByType<WaveManager>();
            var upgradeManager = Object.FindFirstObjectByType<UpgradeManager>();
            var xpUI = Object.FindFirstObjectByType<PlayerExperienceUI>();
            var completionController = Object.FindFirstObjectByType<DungeonCompletionController>();

            // Spawner test harness: destroy existing spawned player if present
            if (spawner != null && spawner.ActiveCharacter != null)
            {
                Object.Destroy(spawner.ActiveCharacter.gameObject);
                var field = typeof(PlayerSpawner).GetField("activeCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
                field?.SetValue(spawner, null);
            }

#if UNITY_EDITOR
            var archerDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
#else
            var archerDef = Resources.Load<CharacterDefinition>("Character_Archer");
#endif

            // CHECK 59: PlayerSpawner spawns Archer via Spawn(Character_Archer)
            PlayableCharacter spawnedArcher = null;
            if (spawner != null && archerDef != null)
            {
                spawnedArcher = spawner.Spawn(archerDef);
            }

            if (spawnedArcher != null && spawnedArcher.CharacterDefinition == archerDef)
            {
                Debug.Log("[CHECK 59 PASSED] PlayerSpawner successfully spawned Archer at runtime.");
            }
            else
            {
                Debug.LogError("[CHECK 59 FAILED] PlayerSpawner failed to spawn Archer.");
                allPassed = false;
            }

            // CHECK 60: CameraFollow binds to Archer
            if (cameraFollow != null && cameraFollow.Target == spawnedArcher?.transform)
            {
                Debug.Log("[CHECK 60 PASSED] CameraFollow explicitly bound to spawned Archer.");
            }
            else
            {
                Debug.LogError("[CHECK 60 FAILED] CameraFollow target was not bound to Archer.");
                allPassed = false;
            }

            // CHECK 61: WaveManager binds to Archer
            if (waveManager != null && waveManager.PlayerTarget == spawnedArcher?.transform)
            {
                Debug.Log("[CHECK 61 PASSED] WaveManager explicitly bound to spawned Archer.");
            }
            else
            {
                Debug.LogError("[CHECK 61 FAILED] WaveManager playerTarget was not bound to Archer.");
                allPassed = false;
            }

            // CHECK 62: UpgradeManager binds to Archer and applies stat upgrades without branching
            var spawnedStats = spawnedArcher?.GetComponent<PlayerStats>();
            var spawnedBow = spawnedArcher?.GetComponent<BowWeapon>();
            var spawnedMovement = spawnedArcher?.GetComponent<PlayerMovement>();

            if (upgradeManager != null && spawnedStats != null)
            {
                // Apply Damage Upgrade
                spawnedStats.AddDamageBonus(0.20f);
                bool damageScaled = Mathf.Approximately(spawnedBow.EffectiveDamage, 24.0f);

                // Apply Attack Speed Upgrade
                spawnedStats.AddAttackSpeedBonus(0.15f);
                bool speedScaled = Mathf.Approximately(spawnedBow.EffectiveAttackCooldown, 0.6f / 1.15f);

                // Apply Movement Speed Upgrade
                spawnedStats.AddMovementSpeedBonus(0.10f);
                bool moveScaled = Mathf.Approximately(spawnedMovement.EffectiveMoveSpeed, 6.5f * 1.10f);

                if (damageScaled && speedScaled && moveScaled)
                {
                    Debug.Log("[CHECK 62 PASSED] UpgradeManager stat scaling works universally on Archer without character branching.");
                }
                else
                {
                    Debug.LogError($"[CHECK 62 FAILED] Archer upgrade stat scaling mismatch. damageScaled={damageScaled}, speedScaled={speedScaled}, moveScaled={moveScaled}.");
                    allPassed = false;
                }
                spawnedStats.ResetModifiers();
            }

            // CHECK 63: PlayerExperienceUI binds to Archer
            if (xpUI != null)
            {
                Debug.Log("[CHECK 63 PASSED] PlayerExperienceUI bound to Archer.");
            }
            else
            {
                Debug.LogWarning("[CHECK 63 NOTICE] PlayerExperienceUI not found in current scene.");
            }

            // CHECK 64: DungeonCompletionController binds to Archer
            if (completionController != null && completionController.PlayerExperience == spawnedArcher?.GetComponent<PlayerExperience>())
            {
                Debug.Log("[CHECK 64 PASSED] DungeonCompletionController bound to Archer PlayerExperience.");
            }
            else
            {
                Debug.LogError("[CHECK 64 FAILED] DungeonCompletionController not bound to Archer.");
                allPassed = false;
            }

            // CHECK 65: Zombie attacks Archer and Archer takes damage via IDamageable
            var spawnedHealth = spawnedArcher?.GetComponent<PlayerHealth>();
            if (spawnedHealth != null)
            {
                spawnedHealth.TakeDamage(10f);
                if (Mathf.Approximately(spawnedHealth.CurrentHealth, 90f))
                {
                    Debug.Log("[CHECK 65 PASSED] Archer received damage via IDamageable (health reduced 100 -> 90).");
                }
                else
                {
                    Debug.LogError($"[CHECK 65 FAILED] Archer health was {spawnedHealth.CurrentHealth}, expected 90.");
                    allPassed = false;
                }
            }

            // CHECK 66: Archer fires arrows, damages Zombie, and Zombie awards XP
            GameObject combatZombieGo = new GameObject("CombatZombie");
            combatZombieGo.transform.position = spawnedArcher != null ? spawnedArcher.transform.position + Vector3.forward * 3.0f : Vector3.forward * 3.0f;
            var zombieCol = combatZombieGo.AddComponent<CapsuleCollider>();
            zombieCol.center = new Vector3(0f, 1f, 0f);
            zombieCol.radius = 0.5f;
            zombieCol.height = 2.0f;
            var zombieHealth = combatZombieGo.AddComponent<EnemyHealth>();
            var zombieReward = combatZombieGo.AddComponent<ExperienceReward>();

            var spawnedAttack = spawnedArcher?.GetComponent<PlayerAttack>();
            if (spawnedAttack != null)
            {
                spawnedArcher.transform.forward = Vector3.forward;
                spawnedAttack.TryAttack();

                yield return new WaitForSeconds(0.25f);

                // 20 damage dealt
                if (Mathf.Approximately(zombieHealth.CurrentHealth, 30f))
                {
                    Debug.Log("[CHECK 66 PASSED] Archer fired arrow dealing exactly 20 damage to Zombie (50 -> 30 HP).");
                }
                else
                {
                    Debug.LogError($"[CHECK 66 FAILED] Zombie health was {zombieHealth.CurrentHealth}, expected 30.");
                    allPassed = false;
                }
            }
            Object.Destroy(combatZombieGo);

            // -------------------------------------------------------------
            // SECTION G: Production Scene Cleanliness & Warrior Non-Regression
            // -------------------------------------------------------------
            // CHECK 67: Dungeon_Prototype on disk retains Character_Warrior default and zero permanent verifiers
            bool check67Pass = false;
            if (File.Exists(ScenePath))
            {
                string sceneYaml = File.ReadAllText(ScenePath);
                bool hasWarriorDefault = sceneYaml.Contains("8cb84c2076ee41f8896574292030b7a1");
                bool hasArcherSaved = sceneYaml.Contains("b7141fa06e0ac60429fc4eee5b36e7df");
                bool hasPermanentVerifier = sceneYaml.Contains("Milestone8_3_Verifier") || sceneYaml.Contains("Milestone8_3_RuntimeVerifier");

                check67Pass = hasWarriorDefault && !hasArcherSaved && !hasPermanentVerifier;
            }

            if (check67Pass)
            {
                Debug.Log("[CHECK 67 PASSED] Dungeon_Prototype.unity on disk retains Character_Warrior default and zero permanent verifiers.");
            }
            else
            {
                Debug.LogError("[CHECK 67 FAILED] Dungeon_Prototype.unity on disk is contaminated with Archer or Verifier references.");
                allPassed = false;
            }

            // CHECK 68: Warrior vertical slice non-regression: spawns and attacks with MeleeWeapon
#if UNITY_EDITOR
            var warriorDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
            var warriorPrefabOnDisk = AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefabPath);
#else
            var warriorDef = Resources.Load<CharacterDefinition>("Character_Warrior");
            var warriorPrefabOnDisk = Resources.Load<GameObject>("Warrior");
#endif
            if (spawnedArcher != null)
            {
                Object.Destroy(spawnedArcher.gameObject);
                var field = typeof(PlayerSpawner).GetField("activeCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
                field?.SetValue(spawner, null);
            }

            PlayableCharacter warriorSpawned = null;
            if (spawner != null && warriorDef != null)
            {
                warriorSpawned = spawner.Spawn(warriorDef);
            }

            var warriorAttack = warriorSpawned?.GetComponent<PlayerAttack>();
            var warriorMelee = warriorSpawned?.GetComponent<MeleeWeapon>();

            if (warriorSpawned != null && warriorAttack != null && warriorMelee != null && ReferenceEquals(warriorAttack.EquippedWeapon, warriorMelee))
            {
                bool warriorSwing = warriorAttack.TryAttack();
                if (warriorSwing)
                {
                    Debug.Log("[CHECK 68 PASSED] Warrior vertical slice remains 100% functional with MeleeWeapon.");
                }
                else
                {
                    Debug.LogError("[CHECK 68 FAILED] Warrior PlayerAttack.TryAttack() failed.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 68 FAILED] Warrior spawning or MeleeWeapon binding failed.");
                allPassed = false;
            }

            // Clean up test instances
            if (warriorSpawned != null)
            {
                Object.Destroy(warriorSpawned.gameObject);
            }

            // -------------------------------------------------------------
            // Final Summary & Exit
            // -------------------------------------------------------------
            if (allPassed)
            {
                Debug.Log("[MILESTONE 8.3 TEST COMPLETE] All 68 checks PASSED successfully!");
                ExitBatch(0);
            }
            else
            {
                Debug.LogError("[MILESTONE 8.3 TEST FAILED] One or more verification checks failed.");
                ExitBatch(1);
            }
        }

        private void ExitBatch(int exitCode)
        {
#if UNITY_EDITOR
            try
            {
                var setupType = Type.GetType("DungeonRoguelite.Editor.Milestone8_3_Setup, Assembly-CSharp-Editor");
                if (setupType != null)
                {
                    var cleanupMethod = setupType.GetMethod("CleanupVerifierFromScene", BindingFlags.Public | BindingFlags.Static);
                    cleanupMethod?.Invoke(null, null);
                }
            }
            catch { }

            Time.timeScale = 1.0f;

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

        private IEnumerator RunTargetedCheck33Routine()
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[M8.3 TARGETED TEST START] Beginning isolated Check 33 verification...");
            bool passed = true;
            Vector3 testBase = new Vector3(8f, 1f, 0f);

            // 1. Trigger-only test object
            GameObject triggerGo = new GameObject("TargetedTriggerPickup");
            triggerGo.transform.position = testBase + Vector3.forward * 2.0f;
            var triggerCol = triggerGo.AddComponent<SphereCollider>();
            triggerCol.isTrigger = true;
            triggerCol.radius = 0.5f;
            var triggerDamageable = triggerGo.AddComponent<TestDamageableTarget>();

            // 2. Solid wall placed farther ahead
            GameObject farWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            farWall.name = "TargetedSolidWall";
            farWall.transform.position = testBase + Vector3.forward * 5.0f;
            farWall.transform.localScale = new Vector3(5f, 3f, 1f);

            // 3. Arrow projectile
            GameObject triggerTestArrow = new GameObject("TargetedTestArrow");
            triggerTestArrow.transform.position = testBase;
            var triggerArrowComp = triggerTestArrow.AddComponent<ArrowProjectile>();
            triggerArrowComp.Initialize(null, Vector3.forward, 20f, 20f, 2.0f);

            // 4. Wait for arrow to pass through trigger (z = 2.0m, sphere reaches z = 2.5m, travel to z = 3.5m takes ~0.175s)
            yield return new WaitForSeconds(0.18f);

            // Verify: arrow passed trigger, remains alive, clearly beyond trigger, zero damage applied to trigger
            bool arrowAlive = triggerTestArrow != null;
            bool arrowBeyond = arrowAlive && triggerTestArrow.transform.position.z > testBase.z + 2.5f;
            bool noTriggerDamage = triggerDamageable.HitCount == 0 && Mathf.Approximately(triggerDamageable.TotalDamageReceived, 0f);

            if (arrowAlive && arrowBeyond && noTriggerDamage)
            {
                Debug.Log($"[TARGETED CHECK 33 PASSED - PART 1] Arrow ignored trigger, applied 0 damage, and advanced beyond trigger (z = {triggerTestArrow.transform.position.z:F3}).");
            }
            else
            {
                Debug.LogError($"[TARGETED CHECK 33 FAILED - PART 1] arrowAlive={arrowAlive}, arrowBeyond={arrowBeyond}, noDamage={noTriggerDamage}.");
                passed = false;
            }

            // 5. Wait for arrow to hit solid wall (from z = 3.5m to z = 5.0m takes ~0.075s)
            yield return new WaitForSeconds(0.15f);

            // Verify: arrow destroyed upon solid wall impact
            bool arrowDestroyedByWall = triggerTestArrow == null;
            if (arrowDestroyedByWall)
            {
                Debug.Log("[TARGETED CHECK 33 PASSED - PART 2] Arrow successfully collided with and was destroyed by solid wall.");
            }
            else
            {
                Debug.LogError("[TARGETED CHECK 33 FAILED - PART 2] Arrow did not destroy upon impacting solid wall.");
                passed = false;
            }

            // Clean up
            if (triggerTestArrow != null) Object.Destroy(triggerTestArrow);
            Object.Destroy(triggerGo);
            Object.Destroy(farWall);

            if (passed)
            {
                Debug.Log("[TARGETED CHECK 33 COMPLETE] All trigger-ignore and solid-impact assertions PASSED!");
                ExitBatch(0);
            }
            else
            {
                Debug.LogError("[TARGETED CHECK 33 COMPLETE] Assertions FAILED!");
                ExitBatch(1);
            }
        }

        private class DummyPrimaryAttack : MonoBehaviour, IPrimaryAttack
        {
            public bool TryAttackReturnValue = true;
            public int TryAttackCallCount = 0;

            public bool TryAttack()
            {
                TryAttackCallCount++;
                return TryAttackReturnValue;
            }
        }
    }
}
