using System.Collections;
using UnityEngine;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated in-engine verification suite for Milestone 1.3 (Player Health).
    /// Validates health state, clamping, damage events, single death execution,
    /// and ensures movement, aiming, and grounding remain unaffected.
    /// </summary>
    public class Milestone1_3_Verifier : MonoBehaviour
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
                Debug.LogError("[M1.3 TEST FAILED] Required Player GameObject with tag 'Player' missing in scene.");
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
                Debug.LogError("[M1.3 TEST FAILED] Required components (PlayerHealth, PlayerMovement, PlayerAim, CharacterController) missing on Player.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            Debug.Log("[M1.3 TEST START] Beginning Milestone 1.3 automated verification suite...");
            bool allPassed = true;

            // Reset health to clean initial state if needed
            health.ResetHealthForTesting(100f);

            // 1. Initial CurrentHealth == MaxHealth == 100
            bool check1 = Mathf.Approximately(health.CurrentHealth, 100f) && Mathf.Approximately(health.MaxHealth, 100f);
            Debug.Log($"[M1.3 TEST 1] Initial Health: Current={health.CurrentHealth}, Max={health.MaxHealth} -> {(check1 ? "PASSED" : "FAILED")}");
            allPassed &= check1;

            // 2. Initial IsDead == false
            bool check2 = !health.IsDead;
            Debug.Log($"[M1.3 TEST 2] Initial IsDead: {health.IsDead} -> {(check2 ? "PASSED" : "FAILED")}");
            allPassed &= check2;

            // Setup event listeners
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

            // 3. TakeDamage(25) produces 75 health
            health.TakeDamage(25f);
            bool check3 = Mathf.Approximately(health.CurrentHealth, 75f) 
                          && healthChangedCount == 1 
                          && Mathf.Approximately(lastCurrentHealth, 75f) 
                          && Mathf.Approximately(lastMaxHealth, 100f);
            Debug.Log($"[M1.3 TEST 3] TakeDamage(25): Current={health.CurrentHealth}, ChangedEvents={healthChangedCount} -> {(check3 ? "PASSED" : "FAILED")}");
            allPassed &= check3;

            // 4. TakeDamage(0) does nothing
            health.TakeDamage(0f);
            bool check4 = Mathf.Approximately(health.CurrentHealth, 75f) && healthChangedCount == 1;
            Debug.Log($"[M1.3 TEST 4] TakeDamage(0): Current={health.CurrentHealth}, ChangedEvents={healthChangedCount} -> {(check4 ? "PASSED" : "FAILED")}");
            allPassed &= check4;

            // 5. TakeDamage(-10) does nothing
            health.TakeDamage(-10f);
            bool check5 = Mathf.Approximately(health.CurrentHealth, 75f) && healthChangedCount == 1;
            Debug.Log($"[M1.3 TEST 5] TakeDamage(-10): Current={health.CurrentHealth}, ChangedEvents={healthChangedCount} -> {(check5 ? "PASSED" : "FAILED")}");
            allPassed &= check5;

            // 6 & 7. Overkill damage clamps health to 0 and health never becomes negative
            health.TakeDamage(200f);
            bool check6_7 = Mathf.Approximately(health.CurrentHealth, 0f) && health.CurrentHealth >= 0f;
            Debug.Log($"[M1.3 TEST 6 & 7] Overkill TakeDamage(200): Current={health.CurrentHealth} (>= 0) -> {(check6_7 ? "PASSED" : "FAILED")}");
            allPassed &= check6_7;

            // 8. IsDead becomes true at 0 health
            bool check8 = health.IsDead;
            Debug.Log($"[M1.3 TEST 8] IsDead after zero health: {health.IsDead} -> {(check8 ? "PASSED" : "FAILED")}");
            allPassed &= check8;

            // 9. OnDied fires exactly once
            bool check9 = diedCount == 1;
            Debug.Log($"[M1.3 TEST 9] OnDied invocation count: {diedCount} -> {(check9 ? "PASSED" : "FAILED")}");
            allPassed &= check9;

            // 10. Further damage after death does not fire OnDied again and keeps health at 0
            health.TakeDamage(50f);
            bool check10 = Mathf.Approximately(health.CurrentHealth, 0f) && diedCount == 1 && healthChangedCount == 2;
            Debug.Log($"[M1.3 TEST 10] Damage while dead: Current={health.CurrentHealth}, DiedCount={diedCount}, ChangedEvents={healthChangedCount} -> {(check10 ? "PASSED" : "FAILED")}");
            allPassed &= check10;

            // 11. HealthNormalized remains within 0..1
            bool check11_dead = Mathf.Approximately(health.HealthNormalized, 0f);
            health.ResetHealthForTesting(100f);
            bool check11_full = Mathf.Approximately(health.HealthNormalized, 1f);
            health.TakeDamage(50f);
            bool check11_half = Mathf.Approximately(health.HealthNormalized, 0.5f);
            bool check11 = check11_dead && check11_full && check11_half && health.HealthNormalized >= 0f && health.HealthNormalized <= 1f;
            Debug.Log($"[M1.3 TEST 11] HealthNormalized: Dead={check11_dead}, Full={check11_full}, Half={check11_half} -> {(check11 ? "PASSED" : "FAILED")}");
            allPassed &= check11;

            // 12. PlayerMovement still functions
            movement.SetTestInputOverride(new Vector2(0f, 1f));
            movement.StepMovement(0.05f);
            bool check12 = movement.MoveSpeed > 0f && movement.CurrentInput == new Vector2(0f, 1f);
            movement.SetTestInputOverride(null);
            Debug.Log($"[M1.3 TEST 12] PlayerMovement functionality: MoveSpeed={movement.MoveSpeed} -> {(check12 ? "PASSED" : "FAILED")}");
            allPassed &= check12;

            // 13. PlayerAim still functions
            aim.SetTestAimWorldTarget(new Vector3(10f, 0f, 0f));
            yield return null;
            Vector3 expectedAimDir = new Vector3(10f, 0f, 0f) - playerGo.transform.position;
            expectedAimDir.y = 0f;
            expectedAimDir.Normalize();
            float angleDiff = Vector3.Angle(playerGo.transform.forward, expectedAimDir);
            bool check13 = angleDiff < 2.0f;
            aim.SetTestAimWorldTarget(null);
            Debug.Log($"[M1.3 TEST 13] PlayerAim functionality: AngleDiff={angleDiff:F2}° -> {(check13 ? "PASSED" : "FAILED")}");
            allPassed &= check13;

            // 14. Existing grounding and collision behavior remains valid
            bool check14 = movement.IsGrounded && cc.enabled;
            Debug.Log($"[M1.3 TEST 14] Grounding & CharacterController integrity: IsGrounded={movement.IsGrounded}, CC.enabled={cc.enabled} -> {(check14 ? "PASSED" : "FAILED")}");
            allPassed &= check14;

            // Restore healthy state for clean scene completion
            health.ResetHealthForTesting(100f);

            if (allPassed)
            {
                Debug.Log("<color=green><b>[ALL MILESTONE 1.3 VERIFICATION TESTS PASSED SUCCESSFULLY]</b></color>");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(0);
#endif
            }
            else
            {
                Debug.LogError("[MILESTONE 1.3 VERIFICATION COMPLETED WITH FAILURES]");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
            }
        }
    }
}
