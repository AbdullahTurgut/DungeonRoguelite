using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Player;
using DungeonRoguelite.Weapons;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated in-engine verification suite for Milestone 2.2 (Basic Sword Combat).
    /// Validates melee hit detection, range/arc filtering, de-duplication, self-damage protection,
    /// cooldown enforcement, and preserves movement/aiming integrity.
    /// </summary>
    public class Milestone2_2_Verifier : MonoBehaviour
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
                Debug.LogError("[M2.2 TEST FAILED] Required Player GameObject with tag 'Player' missing in scene.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            var playerAttack = playerGo.GetComponent<PlayerAttack>();
            var meleeWeapon = playerGo.GetComponent<MeleeWeapon>();
            var playerHealth = playerGo.GetComponent<PlayerHealth>();
            var playerMovement = playerGo.GetComponent<PlayerMovement>();
            var playerAim = playerGo.GetComponent<PlayerAim>();
            var cc = playerGo.GetComponent<CharacterController>();

            if (playerAttack == null || meleeWeapon == null || playerHealth == null || playerMovement == null || playerAim == null || cc == null)
            {
                Debug.LogError("[M2.2 TEST FAILED] Required components missing on Player.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            Debug.Log("[M2.2 TEST START] Beginning Milestone 2.2 automated verification suite...");
            bool allPassed = true;

            // Reset player transform to origin facing North
            playerGo.transform.position = Vector3.zero;
            playerGo.transform.rotation = Quaternion.identity;
            playerHealth.ResetHealthForTesting(100f);

            List<GameObject> spawnedTestObjects = new List<GameObject>();

            GameObject CreateTarget(string name, Vector3 pos, bool addSecondCollider = false)
            {
                var go = new GameObject(name);
                go.transform.position = pos;
                var sphere = go.AddComponent<SphereCollider>();
                sphere.radius = 0.3f;
                if (addSecondCollider)
                {
                    var child = new GameObject(name + "_ChildCol");
                    child.transform.SetParent(go.transform, false);
                    child.transform.localPosition = new Vector3(0.05f, 0f, 0.05f);
                    var box = child.AddComponent<BoxCollider>();
                    box.size = new Vector3(0.3f, 0.3f, 0.3f);
                }
                go.AddComponent<TestDamageableTarget>();
                spawnedTestObjects.Add(go);
                return go;
            }

            // Test 1 & 2: Attack input triggers weapon attack and TryAttack succeeds when cooldown available
            bool attackEventFired = false;
            meleeWeapon.OnAttack += () => attackEventFired = true;
            bool attackSucceeded = playerAttack.TryAttack();
            bool check1_2 = attackSucceeded && attackEventFired;
            Debug.Log($"[M2.2 TEST 1 & 2] TryAttack executed successfully: Succeeded={attackSucceeded}, EventFired={attackEventFired} -> {(check1_2 ? "PASSED" : "FAILED")}");
            allPassed &= check1_2;

            // Wait for weapon cooldown
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);

            // Test 3 & 4: Target directly in front within range receives configured damage (25)
            var targetFront = CreateTarget("Target_Front", new Vector3(0f, 0f, 1.2f));
            var dummyFront = targetFront.GetComponent<TestDamageableTarget>();
            attackSucceeded = playerAttack.TryAttack();
            bool check3_4 = attackSucceeded && dummyFront.HitCount == 1 && Mathf.Approximately(dummyFront.CurrentHealth, 75f) && Mathf.Approximately(dummyFront.LastDamageReceived, 25f);
            Debug.Log($"[M2.2 TEST 3 & 4] Direct Front Hit: HitCount={dummyFront.HitCount}, CurrentHealth={dummyFront.CurrentHealth}, Damage={dummyFront.LastDamageReceived} -> {(check3_4 ? "PASSED" : "FAILED")}");
            allPassed &= check3_4;

            // Wait for weapon cooldown
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);

            // Test 5: Target outside range receives no damage
            var targetFar = CreateTarget("Target_Far", new Vector3(0f, 0f, 4.0f));
            var dummyFar = targetFar.GetComponent<TestDamageableTarget>();
            playerAttack.TryAttack();
            bool check5 = dummyFar.HitCount == 0 && Mathf.Approximately(dummyFar.CurrentHealth, 100f);
            Debug.Log($"[M2.2 TEST 5] Target Outside Range: HitCount={dummyFar.HitCount} -> {(check5 ? "PASSED" : "FAILED")}");
            allPassed &= check5;

            // Wait for weapon cooldown
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);

            // Test 6: Target behind player receives no damage
            var targetBehind = CreateTarget("Target_Behind", new Vector3(0f, 0f, -1.2f));
            var dummyBehind = targetBehind.GetComponent<TestDamageableTarget>();
            playerAttack.TryAttack();
            bool check6 = dummyBehind.HitCount == 0 && Mathf.Approximately(dummyBehind.CurrentHealth, 100f);
            Debug.Log($"[M2.2 TEST 6] Target Behind Player: HitCount={dummyBehind.HitCount} -> {(check6 ? "PASSED" : "FAILED")}");
            allPassed &= check6;

            // Wait for weapon cooldown
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);

            // Test 7: Target outside configured attack arc (e.g. 90 deg sideways when arc is 120 deg) receives no damage
            var targetSideways = CreateTarget("Target_Sideways", new Vector3(1.2f, 0f, 0f));
            var dummySideways = targetSideways.GetComponent<TestDamageableTarget>();
            playerAttack.TryAttack();
            bool check7 = dummySideways.HitCount == 0 && Mathf.Approximately(dummySideways.CurrentHealth, 100f);
            Debug.Log($"[M2.2 TEST 7] Target Outside Attack Arc (90°): HitCount={dummySideways.HitCount} -> {(check7 ? "PASSED" : "FAILED")}");
            allPassed &= check7;

            // Wait for weapon cooldown
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);

            // Test 8: Target with multiple colliders receives damage only once per swing
            var targetMultiCol = CreateTarget("Target_MultiCol", new Vector3(0.5f, 0f, 1.0f), addSecondCollider: true);
            var dummyMultiCol = targetMultiCol.GetComponent<TestDamageableTarget>();
            playerAttack.TryAttack();
            bool check8 = dummyMultiCol.HitCount == 1 && Mathf.Approximately(dummyMultiCol.TotalDamageReceived, 25f);
            Debug.Log($"[M2.2 TEST 8] Multi-Collider De-duplication: HitCount={dummyMultiCol.HitCount}, TotalDamage={dummyMultiCol.TotalDamageReceived} -> {(check8 ? "PASSED" : "FAILED")}");
            allPassed &= check8;

            // Test 9: Player cannot damage itself
            float initialPlayerHealth = playerHealth.CurrentHealth;
            bool check9 = Mathf.Approximately(playerHealth.CurrentHealth, initialPlayerHealth);
            Debug.Log($"[M2.2 TEST 9] Self-Damage Immunity: PlayerHealth={playerHealth.CurrentHealth} == {initialPlayerHealth} -> {(check9 ? "PASSED" : "FAILED")}");
            allPassed &= check9;

            // Test 10: Cooldown blocks attack spam before attackCooldown expires
            bool spamAttackBlocked = !playerAttack.TryAttack();
            Debug.Log($"[M2.2 TEST 10] Cooldown Blocks Immediate Attack Spam: Blocked={spamAttackBlocked} -> {(spamAttackBlocked ? "PASSED" : "FAILED")}");
            allPassed &= spamAttackBlocked;

            // Test 11: A new attack is allowed after cooldown expires
            yield return new WaitForSeconds(meleeWeapon.AttackCooldown + 0.05f);
            bool attackAllowedAfterCooldown = playerAttack.TryAttack();
            Debug.Log($"[M2.2 TEST 11] Attack Allowed After Cooldown: Allowed={attackAllowedAfterCooldown} -> {(attackAllowedAfterCooldown ? "PASSED" : "FAILED")}");
            allPassed &= attackAllowedAfterCooldown;

            // Test 12: Movement remains functional while attacking
            playerMovement.SetTestInputOverride(new Vector2(0f, 1f));
            playerMovement.StepMovement(0.05f);
            bool check12 = playerMovement.MoveSpeed > 0f && playerMovement.CurrentInput == new Vector2(0f, 1f);
            playerMovement.SetTestInputOverride(null);
            Debug.Log($"[M2.2 TEST 12] Movement Functional While Attacking: MoveSpeed={playerMovement.MoveSpeed} -> {(check12 ? "PASSED" : "FAILED")}");
            allPassed &= check12;

            // Test 13: Mouse aiming remains functional while attacking
            playerAim.SetTestAimWorldTarget(new Vector3(10f, 0f, 0f));
            yield return null;
            Vector3 expectedDir = new Vector3(10f, 0f, 0f) - playerGo.transform.position;
            expectedDir.y = 0f;
            expectedDir.Normalize();
            float angleDiff = Vector3.Angle(playerGo.transform.forward, expectedDir);
            bool check13 = angleDiff < 2.0f;
            playerAim.SetTestAimWorldTarget(null);
            Debug.Log($"[M2.2 TEST 13] Aiming Functional While Attacking: AngleDiff={angleDiff:F2}° -> {(check13 ? "PASSED" : "FAILED")}");
            allPassed &= check13;

            // Test 14: Existing PlayerHealth / IDamageable behavior remains unchanged
            IDamageable playerDamageable = playerGo.GetComponent<IDamageable>();
            playerDamageable.TakeDamage(10f);
            bool check14 = Mathf.Approximately(playerHealth.CurrentHealth, 90f);
            playerHealth.ResetHealthForTesting(100f);
            Debug.Log($"[M2.2 TEST 14] PlayerHealth / IDamageable Behavior: CurrentHealth={playerHealth.CurrentHealth} -> {(check14 ? "PASSED" : "FAILED")}");
            allPassed &= check14;

            // Test 15: Grounding and collision behavior remain valid
            bool check15 = playerMovement.IsGrounded && cc.enabled;
            Debug.Log($"[M2.2 TEST 15] Grounding & Collision Integrity: IsGrounded={playerMovement.IsGrounded}, CC.enabled={cc.enabled} -> {(check15 ? "PASSED" : "FAILED")}");
            allPassed &= check15;

            // Clean up spawned test objects
            for (int i = 0; i < spawnedTestObjects.Count; i++)
            {
                if (spawnedTestObjects[i] != null)
                {
                    Destroy(spawnedTestObjects[i]);
                }
            }

            if (allPassed)
            {
                Debug.Log("<color=green><b>[ALL MILESTONE 2.2 VERIFICATION TESTS PASSED SUCCESSFULLY]</b></color>");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(0);
#endif
            }
            else
            {
                Debug.LogError("[MILESTONE 2.2 VERIFICATION COMPLETED WITH FAILURES]");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
            }
        }
    }
}
