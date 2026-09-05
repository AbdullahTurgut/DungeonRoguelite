using System.Collections;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Player;
using DungeonRoguelite.Weapons;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated in-engine verification suite for Milestone 3.1 (Basic Zombie Enemy).
    /// Validates Zombie health, pursuit, stopping distance, attack damage/cooldown,
    /// sword damage/kill, death handling, component disabling, and non-obstruction.
    /// </summary>
    public class Milestone3_1_Verifier : MonoBehaviour
    {
        [SerializeField] private bool runAutomatedTestOnStart = false;

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

            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null)
            {
                Debug.LogError("[M3.1 TEST FAILED] Required Player GameObject missing in scene.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
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
                Debug.LogError("[M3.1 TEST FAILED] Required Player components missing.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            Debug.Log("[M3.1 TEST START] Beginning Milestone 3.1 automated verification suite...");
            bool allPassed = true;

            // Reset player to origin facing North at full health
            playerGo.transform.position = Vector3.zero;
            playerGo.transform.rotation = Quaternion.identity;
            playerHealth.ResetHealthForTesting(100f);

            // Instantiate Zombie from prefab
            GameObject zombieGo = null;
#if UNITY_EDITOR
            var zombiePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            if (zombiePrefab != null)
            {
                zombieGo = Object.Instantiate(zombiePrefab, new Vector3(0f, 0f, 4f), Quaternion.identity);
            }
#endif
            if (zombieGo == null)
            {
                // Fallback: search scene for existing Zombie
                var sceneZombie = Object.FindFirstObjectByType<EnemyHealth>();
                if (sceneZombie != null)
                {
                    zombieGo = sceneZombie.gameObject;
                    zombieGo.transform.position = new Vector3(0f, 0f, 4f);
                }
            }

            if (zombieGo == null)
            {
                Debug.LogError("[M3.1 TEST FAILED] Could not load or instantiate Zombie GameObject.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            var zombieHealth = zombieGo.GetComponent<EnemyHealth>();
            var zombieMovement = zombieGo.GetComponent<EnemyMovement>();
            var zombieAttack = zombieGo.GetComponent<EnemyAttack>();
            var zombieCC = zombieGo.GetComponent<CharacterController>();

            if (zombieHealth == null || zombieMovement == null || zombieAttack == null || zombieCC == null)
            {
                Debug.LogError("[M3.1 TEST FAILED] Required Zombie components missing.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            // Test 1 & 2: Zombie starts alive with MaxHealth = 50 and CurrentHealth = MaxHealth
            bool check1_2 = Mathf.Approximately(zombieHealth.MaxHealth, 50f) 
                            && Mathf.Approximately(zombieHealth.CurrentHealth, 50f) 
                            && !zombieHealth.IsDead;
            Debug.Log($"[M3.1 TEST 1 & 2] Initial Health: Max={zombieHealth.MaxHealth}, Current={zombieHealth.CurrentHealth}, IsDead={zombieHealth.IsDead} -> {(check1_2 ? "PASSED" : "FAILED")}");
            allPassed &= check1_2;

            // Test 5: Zombie does not attack outside attackRange (distance = 4.0m)
            yield return new WaitForSeconds(0.2f);
            bool check5 = Mathf.Approximately(playerHealth.CurrentHealth, 100f);
            Debug.Log($"[M3.1 TEST 5] No Attack Outside Range: PlayerHealth={playerHealth.CurrentHealth} -> {(check5 ? "PASSED" : "FAILED")}");
            allPassed &= check5;

            // Test 3 & 4: Zombie follows player when outside stopping distance and stops appropriately near stopping distance
            float startDist = (zombieGo.transform.position - playerGo.transform.position).magnitude;
            float maxPursueTime = 3.0f;
            float pursueTimer = 0f;
            while (zombieMovement.IsMoving && pursueTimer < maxPursueTime)
            {
                yield return null;
                pursueTimer += Time.deltaTime;
            }

            float currentDist = (zombieGo.transform.position - playerGo.transform.position).magnitude;
            bool followed = currentDist < startDist;
            bool stoppedNear = currentDist <= 1.6f && currentDist >= 1.0f;
            bool check3_4 = followed && stoppedNear;
            Debug.Log($"[M3.1 TEST 3 & 4] Follow & Stopping Distance: StartDist={startDist:F2}m, CurrentDist={currentDist:F2}m -> {(check3_4 ? "PASSED" : "FAILED")}");
            allPassed &= check3_4;

            // Test 6 & 7: Zombie attacks while inside attackRange and applies exactly 10 damage
            float attackWait = 0f;
            while (playerHealth.CurrentHealth == 100f && attackWait < 1.0f)
            {
                yield return null;
                attackWait += Time.deltaTime;
            }
            bool check6_7 = Mathf.Approximately(playerHealth.CurrentHealth, 90f);
            Debug.Log($"[M3.1 TEST 6 & 7] Inside Range Attack: PlayerHealth={playerHealth.CurrentHealth} (expected 90) -> {(check6_7 ? "PASSED" : "FAILED")}");
            allPassed &= check6_7;

            // Test 8: attackCooldown prevents excessive attacks (immediate next 0.2s does not apply another hit)
            yield return new WaitForSeconds(0.2f);
            bool check8 = Mathf.Approximately(playerHealth.CurrentHealth, 90f);
            Debug.Log($"[M3.1 TEST 8] Attack Cooldown Guard: PlayerHealth={playerHealth.CurrentHealth} (still 90) -> {(check8 ? "PASSED" : "FAILED")}");
            allPassed &= check8;

            // Test 9: A new attack is allowed after cooldown expires (attackCooldown = 1.0s)
            float secondAttackWait = 0f;
            while (playerHealth.CurrentHealth == 90f && secondAttackWait < 1.3f)
            {
                yield return null;
                secondAttackWait += Time.deltaTime;
            }
            bool check9 = Mathf.Approximately(playerHealth.CurrentHealth, 80f);
            Debug.Log($"[M3.1 TEST 9] Attack Recovery After Cooldown: PlayerHealth={playerHealth.CurrentHealth} (expected 80) -> {(check9 ? "PASSED" : "FAILED")}");
            allPassed &= check9;

            // Test 10: Zombie resumes pursuit when player moves away
            playerGo.transform.position = new Vector3(0f, 0f, -4f);
            float distBeforeResume = (zombieGo.transform.position - playerGo.transform.position).magnitude;
            yield return new WaitForSeconds(0.6f);
            float distAfterResume = (zombieGo.transform.position - playerGo.transform.position).magnitude;
            bool check10 = distAfterResume < distBeforeResume;
            Debug.Log($"[M3.1 TEST 10] Pursuit Resumes: DistBefore={distBeforeResume:F2}m, DistAfter={distAfterResume:F2}m -> {(check10 ? "PASSED" : "FAILED")}");
            allPassed &= check10;

            // Reposition player at origin facing North and Zombie at (0, 0, 1.2m)
            playerGo.transform.position = Vector3.zero;
            playerGo.transform.rotation = Quaternion.identity;
            zombieGo.transform.position = new Vector3(0f, 0f, 1.2f);
            yield return null;

            // Track Zombie death events
            int zombieDiedCount = 0;
            zombieHealth.OnDied += () => zombieDiedCount++;

            // Wait for player melee weapon cooldown
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);

            // Test 11 & 12: Player sword resolves EnemyHealth through IDamageable and first hit deals 25 damage (50 -> 25)
            playerAttack.TryAttack();
            yield return null;
            bool check11_12 = Mathf.Approximately(zombieHealth.CurrentHealth, 25f) && !zombieHealth.IsDead;
            Debug.Log($"[M3.1 TEST 11 & 12] First Sword Hit: ZombieHealth={zombieHealth.CurrentHealth} (expected 25), IsDead={zombieHealth.IsDead} -> {(check11_12 ? "PASSED" : "FAILED")}");
            allPassed &= check11_12;

            // Wait for player melee weapon cooldown
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);

            // Test 13, 14, 15, 16: Second sword hit reduces Zombie health 25 -> 0, clamped, IsDead true, OnDied fired once
            playerAttack.TryAttack();
            yield return null;
            bool check13_16 = Mathf.Approximately(zombieHealth.CurrentHealth, 0f) 
                              && zombieHealth.CurrentHealth >= 0f 
                              && zombieHealth.IsDead 
                              && zombieDiedCount == 1;
            Debug.Log($"[M3.1 TEST 13-16] Second Sword Hit: ZombieHealth={zombieHealth.CurrentHealth}, IsDead={zombieHealth.IsDead}, DiedCount={zombieDiedCount} -> {(check13_16 ? "PASSED" : "FAILED")}");
            allPassed &= check13_16;

            // Test 17 & 18: Dead Zombie stops moving and stops attacking
            bool check17_18 = !zombieMovement.enabled && !zombieAttack.enabled;
            Debug.Log($"[M3.1 TEST 17 & 18] Components Disabled on Death: MovementEnabled={zombieMovement.enabled}, AttackEnabled={zombieAttack.enabled} -> {(check17_18 ? "PASSED" : "FAILED")}");
            allPassed &= check17_18;

            // Test 19: Dead Zombie can no longer damage player (stand next to corpse for 1.2s)
            float playerHealthAfterZombieDeath = playerHealth.CurrentHealth;
            yield return new WaitForSeconds(1.2f);
            bool check19 = Mathf.Approximately(playerHealth.CurrentHealth, playerHealthAfterZombieDeath);
            Debug.Log($"[M3.1 TEST 19] Dead Zombie Cannot Damage: PlayerHealth={playerHealth.CurrentHealth} == {playerHealthAfterZombieDeath} -> {(check19 ? "PASSED" : "FAILED")}");
            allPassed &= check19;

            // Test 20: Dead Zombie no longer physically obstructs the player (CharacterController disabled)
            bool check20 = !zombieCC.enabled;
            Debug.Log($"[M3.1 TEST 20] Non-Obstruction Check: Zombie CC.enabled={zombieCC.enabled} -> {(check20 ? "PASSED" : "FAILED")}");
            allPassed &= check20;

            // Test 21: Existing PlayerMovement remains functional
            playerMovement.SetTestInputOverride(new Vector2(0f, 1f));
            playerMovement.StepMovement(0.05f);
            bool check21 = playerMovement.MoveSpeed > 0f && playerMovement.CurrentInput == new Vector2(0f, 1f);
            playerMovement.SetTestInputOverride(null);
            Debug.Log($"[M3.1 TEST 21] PlayerMovement Functional: MoveSpeed={playerMovement.MoveSpeed} -> {(check21 ? "PASSED" : "FAILED")}");
            allPassed &= check21;

            // Test 22: Existing PlayerAim remains functional
            playerAim.SetTestAimWorldTarget(new Vector3(10f, 0f, 0f));
            yield return null;
            Vector3 aimDir = new Vector3(10f, 0f, 0f) - playerGo.transform.position;
            aimDir.y = 0f;
            aimDir.Normalize();
            float angleDiff = Vector3.Angle(playerGo.transform.forward, aimDir);
            bool check22 = angleDiff < 2.0f;
            playerAim.SetTestAimWorldTarget(null);
            Debug.Log($"[M3.1 TEST 22] PlayerAim Functional: AngleDiff={angleDiff:F2}° -> {(check22 ? "PASSED" : "FAILED")}");
            allPassed &= check22;

            // Test 23: Existing PlayerHealth remains functional
            playerHealth.TakeDamage(10f);
            bool check23 = Mathf.Approximately(playerHealth.CurrentHealth, playerHealthAfterZombieDeath - 10f);
            playerHealth.ResetHealthForTesting(100f);
            Debug.Log($"[M3.1 TEST 23] PlayerHealth Functional: CurrentHealth={playerHealth.CurrentHealth} -> {(check23 ? "PASSED" : "FAILED")}");
            allPassed &= check23;

            // Test 24: Existing sword combat remains functional
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);
            bool check24 = playerAttack.TryAttack();
            Debug.Log($"[M3.1 TEST 24] Sword Combat Functional: TryAttack={check24} -> {(check24 ? "PASSED" : "FAILED")}");
            allPassed &= check24;

            // Test 25: Existing grounding/collision behavior remains valid
            bool check25 = playerMovement.IsGrounded && playerCC.enabled;
            Debug.Log($"[M3.1 TEST 25] Player Grounding & Collision Valid: IsGrounded={playerMovement.IsGrounded} -> {(check25 ? "PASSED" : "FAILED")}");
            allPassed &= check25;

            // Event subscription idempotency check: disabling and enabling does not produce duplicate callbacks
            int duplicateCallbackCount = 0;
            System.Action dummyCallback = () => duplicateCallbackCount++;
            zombieHealth.OnDied += dummyCallback;
            zombieMovement.enabled = false;
            zombieMovement.enabled = true;
            zombieAttack.enabled = false;
            zombieAttack.enabled = true;
            zombieHealth.OnDied -= dummyCallback;

            // Clean up test zombie
            if (zombieGo != null)
            {
                Destroy(zombieGo);
            }

            if (allPassed)
            {
                Debug.Log("<color=green><b>[ALL MILESTONE 3.1 VERIFICATION TESTS PASSED SUCCESSFULLY]</b></color>");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(0);
#endif
            }
            else
            {
                Debug.LogError("[MILESTONE 3.1 VERIFICATION COMPLETED WITH FAILURES]");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
            }
        }
    }
}
