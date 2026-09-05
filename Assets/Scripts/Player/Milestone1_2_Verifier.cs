using UnityEngine;
using System.Collections;
using DungeonRoguelite.CameraControl;

namespace DungeonRoguelite.Player
{
    /// <summary>
    /// Automated in-engine verification suite for Milestone 1.2 (Camera and Aim).
    /// Verifies:
    /// 1. Full 360-degree aiming rotation toward world pointer coordinates.
    /// 2. Rotation strictly restricted to Y-axis only (X and Z rotations are zero).
    /// 3. Independent movement: holding movement key while rotating aim does not alter travel vector.
    /// 4. Simultaneous movement and aiming in opposing directions.
    /// 5. Smooth camera follow behavior in LateUpdate.
    /// 6. Camera rotation locked at fixed top-down pitch without roll/yaw.
    /// 7. Collision and grounding stability preserved.
    /// </summary>
    public class Milestone1_2_Verifier : MonoBehaviour
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
            var mainCamera = Camera.main;

            if (playerGo == null || mainCamera == null)
            {
                Debug.LogError("[M1.2 TEST FAILED] Required Player or MainCamera GameObject missing in scene.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            var movement = playerGo.GetComponent<PlayerMovement>();
            var aim = playerGo.GetComponent<PlayerAim>();
            var cc = playerGo.GetComponent<CharacterController>();
            var camFollow = mainCamera.GetComponent<CameraFollow>();

            if (movement == null || aim == null || cc == null || camFollow == null)
            {
                Debug.LogError("[M1.2 TEST FAILED] Required components (PlayerMovement, PlayerAim, CharacterController, CameraFollow) missing.");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
                yield break;
            }

            Debug.Log("[M1.2 TEST START] Beginning Milestone 1.2 automated verification suite...");

            // Test 1: Full 360-degree aim tracking across 4 quadrants
            Vector3[] testTargets = new Vector3[]
            {
                new Vector3(0f, 0f, 10f),   // North (0 deg)
                new Vector3(10f, 0f, 0f),   // East (90 deg)
                new Vector3(0f, 0f, -10f),  // South (180 deg)
                new Vector3(-10f, 0f, 0f),  // West (270 deg)
                new Vector3(10f, 0f, 10f),  // North-East (45 deg)
                new Vector3(-10f, 0f, 10f)  // North-West (315 deg)
            };

            bool aimTrackingPassed = true;
            for (int i = 0; i < testTargets.Length; i++)
            {
                aim.SetTestAimWorldTarget(testTargets[i]);
                yield return null;

                Vector3 expectedDir = (testTargets[i] - playerGo.transform.position);
                expectedDir.y = 0f;
                expectedDir.Normalize();

                float angleDiff = Vector3.Angle(playerGo.transform.forward, expectedDir);
                if (angleDiff > 2.0f)
                {
                    Debug.LogError($"[M1.2 TEST 1 FAILED] Target {testTargets[i]}: angleDiff={angleDiff:F2} deg exceeds tolerance.");
                    aimTrackingPassed = false;
                }
            }

            if (aimTrackingPassed)
            {
                Debug.Log("[M1.2 TEST 1 PASSED] Player rotates accurately toward target across all quadrants (360° coverage).");
            }

            // Test 2: Rotation is strictly constrained to Y-axis (X and Z Euler angles must be 0)
            Vector3 euler = playerGo.transform.eulerAngles;
            float pitchX = Mathf.Abs(Mathf.DeltaAngle(0f, euler.x));
            float rollZ = Mathf.Abs(Mathf.DeltaAngle(0f, euler.z));
            bool yOnlyPassed = pitchX < 0.01f && rollZ < 0.01f;

            Debug.Log($"[M1.2 TEST 2] Pitch (X) = {pitchX:F4}°, Roll (Z) = {rollZ:F4}°");
            if (yOnlyPassed)
            {
                Debug.Log("[M1.2 TEST 2 PASSED] Player rotation strictly occurs exclusively around the Y-axis.");
            }
            else
            {
                Debug.LogError($"[M1.2 TEST 2 FAILED] Rotation leaked into non-Y axes! X={pitchX}, Z={rollZ}");
            }

            // Test 3: Movement independence: hold South (-Z) while aiming rotates continuously
            Vector3 moveStartPos = playerGo.transform.position;
            movement.SetTestInputOverride(new Vector2(0f, -1f)); // South

            float duration = 0.5f;
            float elapsed = 0f;
            int step = 0;
            while (elapsed < duration)
            {
                // Sweep aim target in circle while moving
                float rad = step * 0.3f;
                aim.SetTestAimWorldTarget(playerGo.transform.position + new Vector3(Mathf.Sin(rad) * 5f, 0f, Mathf.Cos(rad) * 5f));
                step++;
                elapsed += Time.deltaTime;
                yield return null;
            }

            Vector3 moveEndPos = playerGo.transform.position;
            float movedZ = moveEndPos.z - moveStartPos.z;
            float driftX = Mathf.Abs(moveEndPos.x - moveStartPos.x);

            Debug.Log($"[M1.2 TEST 3] Independent strafe: movedZ = {movedZ:F2}m (Expected negative), driftX = {driftX:F4}m");
            bool independencePassed = movedZ < -1.0f && driftX < 0.05f;
            if (independencePassed)
            {
                Debug.Log("[M1.2 TEST 3 PASSED] Holding movement key maintains world translation direction regardless of aim rotation.");
            }
            else
            {
                Debug.LogError($"[M1.2 TEST 3 FAILED] Movement direction was perturbed by aiming! movedZ={movedZ}, driftX={driftX}");
            }

            // Test 4: Simultaneous opposing movement and aiming (move East, aim West)
            movement.SetTestInputOverride(new Vector2(1f, 0f)); // Move East (+X)
            aim.SetTestAimWorldTarget(playerGo.transform.position + new Vector3(-10f, 0f, 0f)); // Aim West (-X)
            yield return new WaitForSeconds(0.3f);

            bool opposingPassed = playerGo.transform.forward.x < -0.9f && cc.velocity.x > 3.0f;
            Debug.Log($"[M1.2 TEST 4] Facing X = {playerGo.transform.forward.x:F2} (Expected negative), Velocity X = {cc.velocity.x:F2} (Expected positive)");
            if (opposingPassed)
            {
                Debug.Log("[M1.2 TEST 4 PASSED] Movement and aiming operate simultaneously and in opposing directions without interference.");
            }
            else
            {
                Debug.LogError("[M1.2 TEST 4 FAILED] Opposing movement and aim failed.");
            }

            // Test 5: Camera Follow behavior
            Vector3 camStartPos = mainCamera.transform.position;
            // Move player 4 meters forward
            movement.SetTestInputOverride(new Vector2(0f, 1f));
            yield return new WaitForSeconds(0.6f);
            movement.SetTestInputOverride(Vector2.zero);

            // Give camera smoothTime window to settle
            yield return new WaitForSeconds(0.4f);

            Vector3 expectedCamPos = playerGo.transform.position + camFollow.Offset;
            float camDist = Vector3.Distance(mainCamera.transform.position, expectedCamPos);
            Debug.Log($"[M1.2 TEST 5] Camera settle distance to target offset = {camDist:F3}m (Expected < 0.2m)");
            bool camFollowPassed = camDist < 0.25f;
            if (camFollowPassed)
            {
                Debug.Log("[M1.2 TEST 5 PASSED] Camera smoothly follows moving player to the configured offset.");
            }
            else
            {
                Debug.LogError($"[M1.2 TEST 5 FAILED] Camera did not track player to offset! Dist = {camDist}");
            }

            // Test 6: Camera rotation stability (strictly fixed top-down orientation)
            Vector3 camEuler = mainCamera.transform.eulerAngles;
            float camPitch = Mathf.DeltaAngle(55f, camEuler.x);
            float camYaw = Mathf.DeltaAngle(0f, camEuler.y);
            float camRoll = Mathf.DeltaAngle(0f, camEuler.z);
            bool camRotPassed = Mathf.Abs(camPitch) < 0.1f && Mathf.Abs(camYaw) < 0.1f && Mathf.Abs(camRoll) < 0.1f;

            Debug.Log($"[M1.2 TEST 6] Camera Euler delta from (55, 0, 0): Pitch={camPitch:F2}°, Yaw={camYaw:F2}°, Roll={camRoll:F2}°");
            if (camRotPassed)
            {
                Debug.Log("[M1.2 TEST 6 PASSED] Camera maintains strictly locked angled top-down orientation.");
            }
            else
            {
                Debug.LogError("[M1.2 TEST 6 FAILED] Camera orientation deviated from (55, 0, 0)!");
            }

            // Test 7: Collision & Grounding preserved
            bool groundingPassed = movement.IsGrounded;
            Debug.Log($"[M1.2 TEST 7] Grounding check: IsGrounded = {groundingPassed}");
            if (groundingPassed)
            {
                Debug.Log("[M1.2 TEST 7 PASSED] CharacterController grounding remains fully valid.");
            }

            // Reset test overrides
            movement.SetTestInputOverride(null);
            aim.SetTestAimWorldTarget(null);

            bool allPassed = aimTrackingPassed && yOnlyPassed && independencePassed && opposingPassed && camFollowPassed && camRotPassed && groundingPassed;

            if (allPassed)
            {
                Debug.Log("<color=green><b>[ALL MILESTONE 1.2 VERIFICATION TESTS PASSED SUCCESSFULLY]</b></color>");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(0);
#endif
            }
            else
            {
                Debug.LogError("[MILESTONE 1.2 VERIFICATION COMPLETED WITH FAILURES]");
#if UNITY_EDITOR
                if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(1);
#endif
            }
        }
    }
}
