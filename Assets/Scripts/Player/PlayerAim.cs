using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonRoguelite.Player
{
    /// <summary>
    /// Handles player aiming by projecting screen pointer coordinates onto a mathematical horizontal plane at the player's height.
    /// Rotates the player exclusively around the Y-axis. Completely independent from PlayerMovement.
    /// </summary>
    public class PlayerAim : MonoBehaviour
    {
        [Header("Camera Reference")]
        [Tooltip("The camera used for screen-to-world raycasting. If unassigned, defaults to Camera.main.")]
        [SerializeField] private Camera aimCamera;

        [Header("Aim Rotation Settings")]
        [Tooltip("If true, the character snaps instantly to the aim direction. If false, turns at rotationSpeed.")]
        [SerializeField] private bool instantRotation = true;

        [Tooltip("Angular speed in degrees per second when smooth rotation is enabled.")]
        [SerializeField] private float rotationSpeed = 1080f;

        private Vector3 aimDirection = Vector3.forward;
        private Vector3 aimPoint = Vector3.zero;
        private bool hasValidAim;

        // Testing & simulation hooks
        private bool testWorldTargetOverrideActive;
        private Vector3 testWorldTargetOverride;
        private bool testScreenPosOverrideActive;
        private Vector2 testScreenPosOverride;

        public Vector3 AimDirection => aimDirection;
        public Vector3 AimPoint => aimPoint;
        public bool HasValidAim => hasValidAim;
        public Camera AimCamera => aimCamera;

        private void Awake()
        {
            ResolveCamera();
        }

        private void Start()
        {
            ResolveCamera();
        }

        private void Update()
        {
            UpdateAim();
        }

        private void ResolveCamera()
        {
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            if (aimCamera == null)
            {
                aimCamera = FindFirstObjectByType<Camera>();
            }
        }

        private void UpdateAim()
        {
            // 1. Check for automated test world target override
            if (testWorldTargetOverrideActive)
            {
                ApplyWorldAimTarget(testWorldTargetOverride);
                return;
            }

            // 2. Resolve camera safely
            if (aimCamera == null)
            {
                ResolveCamera();
                if (aimCamera == null)
                {
                    hasValidAim = false;
                    return;
                }
            }

            // 3. Read screen pointer position
            Vector2 screenPos;
            if (testScreenPosOverrideActive)
            {
                screenPos = testScreenPosOverride;
            }
            else
            {
                if (Pointer.current != null)
                {
                    screenPos = Pointer.current.position.ReadValue();
                }
                else if (Mouse.current != null)
                {
                    screenPos = Mouse.current.position.ReadValue();
                }
                else
                {
                    hasValidAim = false;
                    return;
                }
            }

            // 4. Mathematical horizontal plane intersection at player height
            Ray ray = aimCamera.ScreenPointToRay(screenPos);
            Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

            if (horizontalPlane.Raycast(ray, out float enterDistance))
            {
                Vector3 worldHit = ray.GetPoint(enterDistance);
                ApplyWorldAimTarget(worldHit);
            }
            else
            {
                hasValidAim = false;
            }
        }

        private void ApplyWorldAimTarget(Vector3 worldTarget)
        {
            aimPoint = worldTarget;

            // Calculate horizontal aim vector
            Vector3 dir = worldTarget - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
            {
                aimDirection = dir.normalized;
                hasValidAim = true;
                RotatePlayerToward(aimDirection);
            }
        }

        private void RotatePlayerToward(Vector3 targetDirection)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

            if (instantRotation)
            {
                // Strictly constrain rotation to Y-axis only
                transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
            }
            else
            {
                Quaternion currentYOnly = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                float step = rotationSpeed * Time.deltaTime;
                Quaternion newRot = Quaternion.RotateTowards(currentYOnly, targetRotation, step);
                transform.rotation = Quaternion.Euler(0f, newRot.eulerAngles.y, 0f);
            }
        }

        /// <summary>
        /// Explicitly set camera for aiming (e.g. from editor or dynamic camera systems).
        /// </summary>
        public void SetCamera(Camera camera)
        {
            aimCamera = camera;
        }

        /// <summary>
        /// Sets a simulated world position target for automated verification.
        /// </summary>
        public void SetTestAimWorldTarget(Vector3? worldTarget)
        {
            if (worldTarget.HasValue)
            {
                testWorldTargetOverrideActive = true;
                testWorldTargetOverride = worldTarget.Value;
            }
            else
            {
                testWorldTargetOverrideActive = false;
                testWorldTargetOverride = Vector3.zero;
            }
        }

        /// <summary>
        /// Sets a simulated screen position for automated verification.
        /// </summary>
        public void SetTestScreenPosition(Vector2? screenPosition)
        {
            if (screenPosition.HasValue)
            {
                testScreenPosOverrideActive = true;
                testScreenPosOverride = screenPosition.Value;
            }
            else
            {
                testScreenPosOverrideActive = false;
                testScreenPosOverride = Vector2.zero;
            }
        }
    }
}
