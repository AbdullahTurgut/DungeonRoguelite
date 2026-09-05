using UnityEngine;
using System.Collections;

namespace DungeonRoguelite.Player
{
    /// <summary>
    /// Automated in-game verification harness for Milestone 1.1.
    /// Runs tests across multiple frames to verify:
    /// 1. WASD movement on the X/Z plane.
    /// 2. Configurable speed compliance.
    /// 3. Diagonal normalization (diagonal not faster than cardinal).
    /// 4. CharacterController collision against obstacles.
    /// 5. Grounding & gravity stability.
    /// </summary>
    public class Milestone1_1_Verifier : MonoBehaviour
    {
        [SerializeField] private bool runAutomatedTestOnStart = false;
        private PlayerMovement playerMovement;
        private CharacterController characterController;

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
                Debug.LogError("[TEST FAILED] Player GameObject with tag 'Player' was not found in the scene.");
                yield break;
            }

            playerMovement = playerGo.GetComponent<PlayerMovement>();
            characterController = playerGo.GetComponent<CharacterController>();

            if (playerMovement == null || characterController == null)
            {
                Debug.LogError("[TEST FAILED] Player is missing PlayerMovement or CharacterController component.");
                yield break;
            }

            Debug.Log("[TEST START] Starting Milestone 1.1 automated verification suite...");

            // Test 1: Grounding at rest
            yield return new WaitForFixedUpdate();
            bool initialGrounded = playerMovement.IsGrounded;
            Debug.Log($"[TEST 1] Grounding check: IsGrounded = {initialGrounded} (Expected: true)");
            if (!initialGrounded)
            {
                Debug.LogWarning("[TEST 1 WARNING] CharacterController.isGrounded initially false before first Move call; progressing to movement step.");
            }

            // Test 2: Cardinal movement (W / Forward / +Z)
            Vector3 startPos = playerGo.transform.position;
            playerMovement.SetTestInputOverride(Vector2.up); // Forward (+Z)

            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Vector3 postCardinalPos = playerGo.transform.position;
            float deltaZ = postCardinalPos.z - startPos.z;
            float deltaX = Mathf.Abs(postCardinalPos.x - startPos.x);
            float expectedZ = playerMovement.MoveSpeed * duration;

            Debug.Log($"[TEST 2] Cardinal Movement (+Z): deltaZ = {deltaZ:F2}m (Expected ~{expectedZ:F2}m), deltaX = {deltaX:F4}m (Expected ~0m)");
            bool cardinalPassed = deltaZ > 0.5f && deltaX < 0.05f;
            if (cardinalPassed)
            {
                Debug.Log("[TEST 2 PASSED] Cardinal movement strictly operates on Z axis with no drift on X.");
            }
            else
            {
                Debug.LogError($"[TEST 2 FAILED] Cardinal movement deltaZ={deltaZ}, deltaX={deltaX}");
            }

            // Test 3: Diagonal movement normalization
            // Input: (1, 1) diagonal
            playerMovement.SetTestInputOverride(new Vector2(1f, 1f));
            yield return new WaitForSeconds(0.1f);

            Vector3 currentVel = characterController.velocity;
            Vector2 horizontalVel = new Vector2(currentVel.x, currentVel.z);
            float diagonalSpeed = horizontalVel.magnitude;
            float configuredSpeed = playerMovement.MoveSpeed;

            Debug.Log($"[TEST 3] Diagonal speed check: measured speed = {diagonalSpeed:F2} m/s, configured max speed = {configuredSpeed:F2} m/s");
            // If not normalized, (1, 1) would yield speed * sqrt(2) = 6 * 1.414 = 8.48 m/s.
            // With normalization, speed should be within tolerance of configuredSpeed (6 m/s).
            bool diagonalNormalized = diagonalSpeed <= (configuredSpeed + 0.15f);
            if (diagonalNormalized)
            {
                Debug.Log($"[TEST 3 PASSED] Diagonal movement speed ({diagonalSpeed:F2} m/s) does NOT exceed configured speed ({configuredSpeed:F2} m/s).");
            }
            else
            {
                Debug.LogError($"[TEST 3 FAILED] Diagonal movement is unnormalized! Speed: {diagonalSpeed:F2} m/s > {configuredSpeed:F2} m/s");
            }

            // Test 4: Collision detection with obstacles
            // Teleport player near Pillar_NE (which is at (5, 1.5, 5), size 2x3x2 => bounds x in [4, 6], z in [4, 6])
            playerMovement.SetTestInputOverride(Vector2.zero);
            characterController.enabled = false;
            playerGo.transform.position = new Vector3(2.5f, 0f, 5f);
            characterController.enabled = true;
            yield return null;

            // Move East directly into Pillar_NE
            playerMovement.SetTestInputOverride(new Vector2(1f, 0f)); // East (+X)
            elapsed = 0f;
            duration = 0.8f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Vector3 blockedPos = playerGo.transform.position;
            // The pillar boundary is at x = 4.0 - player radius (0.5) = ~3.5. Player x must not penetrate inside 4.0.
            Debug.Log($"[TEST 4] Obstacle Collision: Player blocked at X = {blockedPos.x:F2} (Obstacle surface at X = 4.0)");
            bool collisionPassed = blockedPos.x < 3.6f;
            if (collisionPassed)
            {
                Debug.Log("[TEST 4 PASSED] CharacterController stopped cleanly at obstacle collider boundary without penetrating.");
            }
            else
            {
                Debug.LogError($"[TEST 4 FAILED] Player penetrated obstacle! Final X: {blockedPos.x:F2}");
            }

            // Test 5: Grounding stability
            bool remainsGrounded = playerMovement.IsGrounded;
            Debug.Log($"[TEST 5] Grounding stability: IsGrounded = {remainsGrounded} (Expected: true)");
            if (remainsGrounded)
            {
                Debug.Log("[TEST 5 PASSED] Player remains grounded throughout movement.");
            }

            // Reset test override back to normal live input so WASD can be used interactively
            playerMovement.SetTestInputOverride(null);

            if (cardinalPassed && diagonalNormalized && collisionPassed && remainsGrounded)
            {
                Debug.Log("<color=green><b>[ALL MILESTONE 1.1 VERIFICATION TESTS PASSED SUCCESSFULLY]</b></color>");
#if UNITY_EDITOR
                if (Application.isBatchMode)
                {
                    UnityEditor.EditorApplication.Exit(0);
                }
#endif
            }
            else
            {
                Debug.LogError("[VERIFICATION SUITE COMPLETED WITH ONE OR MORE FAILURES]");
#if UNITY_EDITOR
                if (Application.isBatchMode)
                {
                    UnityEditor.EditorApplication.Exit(1);
                }
#endif
            }
        }
    }
}
