using System.Collections;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated in-engine verification suite for Milestone 2.1 (Damage Architecture).
    /// Verifies:
    /// 1. PlayerHealth implements IDamageable.
    /// 2. PlayerHealth can be referenced through an IDamageable variable.
    /// 3. Damage applied through IDamageable.TakeDamage reduces health correctly.
    /// 4. Existing zero-damage guard still works through IDamageable.
    /// 5. Existing negative-damage guard still works through IDamageable.
    /// 6. Existing overkill clamping still works through IDamageable.
    /// 7. Existing death event still fires exactly once through IDamageable.
    /// 8. Post-death damage behavior remains unchanged through IDamageable.
    /// 9. PlayerMovement still works.
    /// 10. PlayerAim still works.
    /// 11. CharacterController grounding/collision remains valid.
    /// 12. Unity compiles with 0 errors.
    /// 13. No runtime exceptions occur in Play Mode.
    /// </summary>
    public class Milestone2_1_Verifier : MonoBehaviour
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
                Debug.LogError("[M2.1 TEST FAILED] Required Player GameObject with tag 'Player' missing in scene.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            var health = playerGo.GetComponent<PlayerHealth>();
            var movement = playerGo.GetComponent<PlayerMovement>();
            var aim = playerGo.GetComponent<PlayerAim>();
            var cc = playerGo.GetComponent<CharacterController>();

            if (health == null || movement == null || aim == null || cc == null)
            {
                Debug.LogError("[M2.1 TEST FAILED] Required components (PlayerHealth, PlayerMovement, PlayerAim, CharacterController) missing on Player.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            Debug.Log("[M2.1 TEST START] Beginning Milestone 2.1 automated verification suite...");
            bool allPassed = true;

            // Test 1: PlayerHealth implements IDamageable
            bool check1 = health is IDamageable;
            Debug.Log($"[M2.1 TEST 1] PlayerHealth is IDamageable: {check1} -> {(check1 ? "PASSED" : "FAILED")}");
            allPassed &= check1;

            // Test 2: PlayerHealth can be referenced through an IDamageable variable
            IDamageable damageable = playerGo.GetComponent<IDamageable>();
            bool check2 = damageable != null && ReferenceEquals(damageable, health);
            Debug.Log($"[M2.1 TEST 2] GetComponent<IDamageable>() resolves PlayerHealth: {check2} -> {(check2 ? "PASSED" : "FAILED")}");
            allPassed &= check2;

            if (!check2)
            {
                Debug.LogError("[M2.1 TEST FAILED] Could not resolve IDamageable reference from Player GameObject.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            // Reset health to 100 before testing damage flow
            health.ResetHealthForTesting(100f);

            // Subscribe to health events to verify damage notifications through IDamageable
            int healthChangedCount = 0;
            float lastCurrentHealth = -1f;
            float lastMaxHealth = -1f;
            int diedCount = 0;

            health.OnHealthChanged += (curr, max) =>
            {
                healthChangedCount++;
                lastCurrentHealth = curr;
                lastMaxHealth = max;
            };

            health.OnDied += () =>
            {
                diedCount++;
            };

            // Test 3: Damage applied through IDamageable.TakeDamage reduces health correctly
            damageable.TakeDamage(25f);
            bool check3 = Mathf.Approximately(health.CurrentHealth, 75f)
                          && healthChangedCount == 1
                          && Mathf.Approximately(lastCurrentHealth, 75f)
                          && Mathf.Approximately(lastMaxHealth, 100f);
            Debug.Log($"[M2.1 TEST 3] IDamageable.TakeDamage(25): Current={health.CurrentHealth}, ChangedEvents={healthChangedCount} -> {(check3 ? "PASSED" : "FAILED")}");
            allPassed &= check3;

            // Test 4: Existing zero-damage guard still works through IDamageable
            damageable.TakeDamage(0f);
            bool check4 = Mathf.Approximately(health.CurrentHealth, 75f) && healthChangedCount == 1;
            Debug.Log($"[M2.1 TEST 4] IDamageable.TakeDamage(0): Current={health.CurrentHealth}, ChangedEvents={healthChangedCount} -> {(check4 ? "PASSED" : "FAILED")}");
            allPassed &= check4;

            // Test 5: Existing negative-damage guard still works through IDamageable
            damageable.TakeDamage(-10f);
            bool check5 = Mathf.Approximately(health.CurrentHealth, 75f) && healthChangedCount == 1;
            Debug.Log($"[M2.1 TEST 5] IDamageable.TakeDamage(-10): Current={health.CurrentHealth}, ChangedEvents={healthChangedCount} -> {(check5 ? "PASSED" : "FAILED")}");
            allPassed &= check5;

            // Test 6: Existing overkill clamping still works through IDamageable
            damageable.TakeDamage(200f);
            bool check6 = Mathf.Approximately(health.CurrentHealth, 0f) && health.CurrentHealth >= 0f;
            Debug.Log($"[M2.1 TEST 6] IDamageable.TakeDamage(200) overkill clamp: Current={health.CurrentHealth} -> {(check6 ? "PASSED" : "FAILED")}");
            allPassed &= check6;

            // Test 7: Existing death event still fires exactly once through IDamageable
            bool check7 = health.IsDead && diedCount == 1;
            Debug.Log($"[M2.1 TEST 7] OnDied triggered through IDamageable: IsDead={health.IsDead}, DiedCount={diedCount} -> {(check7 ? "PASSED" : "FAILED")}");
            allPassed &= check7;

            // Test 8: Post-death damage behavior remains unchanged through IDamageable
            damageable.TakeDamage(50f);
            bool check8 = Mathf.Approximately(health.CurrentHealth, 0f) && diedCount == 1 && healthChangedCount == 2;
            Debug.Log($"[M2.1 TEST 8] Post-death damage through IDamageable: Current={health.CurrentHealth}, DiedCount={diedCount} -> {(check8 ? "PASSED" : "FAILED")}");
            allPassed &= check8;

            // Reset health for gameplay integrity checks
            health.ResetHealthForTesting(100f);

            // Test 9: PlayerMovement still works
            movement.SetTestInputOverride(new Vector2(1f, 0f));
            movement.StepMovement(0.05f);
            bool check9 = movement.MoveSpeed > 0f && movement.CurrentInput == new Vector2(1f, 0f);
            movement.SetTestInputOverride(null);
            Debug.Log($"[M2.1 TEST 9] PlayerMovement functional: MoveSpeed={movement.MoveSpeed} -> {(check9 ? "PASSED" : "FAILED")}");
            allPassed &= check9;

            // Test 10: PlayerAim still works
            aim.SetTestAimWorldTarget(new Vector3(0f, 0f, 10f));
            yield return null;
            Vector3 expectedAimDir = new Vector3(0f, 0f, 10f) - playerGo.transform.position;
            expectedAimDir.y = 0f;
            expectedAimDir.Normalize();
            float angleDiff = Vector3.Angle(playerGo.transform.forward, expectedAimDir);
            bool check10 = angleDiff < 2.0f;
            aim.SetTestAimWorldTarget(null);
            Debug.Log($"[M2.1 TEST 10] PlayerAim functional: AngleDiff={angleDiff:F2}° -> {(check10 ? "PASSED" : "FAILED")}");
            allPassed &= check10;

            // Test 11: CharacterController grounding/collision remains valid
            bool check11 = movement.IsGrounded && cc.enabled;
            Debug.Log($"[M2.1 TEST 11] Grounding & collision integrity: IsGrounded={movement.IsGrounded}, CC.enabled={cc.enabled} -> {(check11 ? "PASSED" : "FAILED")}");
            allPassed &= check11;

            if (allPassed)
            {
                Debug.Log("<color=green><b>[ALL MILESTONE 2.1 VERIFICATION TESTS PASSED SUCCESSFULLY]</b></color>");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(0);
#endif
            }
            else
            {
                Debug.LogError("[MILESTONE 2.1 VERIFICATION COMPLETED WITH FAILURES]");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
            }
        }
    }
}
