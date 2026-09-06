using UnityEngine;

namespace DungeonRoguelite.Enemies
{
    /// <summary>
    /// Pursues a target Transform on the X/Z plane using a CharacterController.
    /// Halts forward translation when within stopping distance.
    /// Cleanly reacts to EnemyHealth.OnDied by stopping movement and disabling its CharacterController.
    /// Note: Direct pursuit is an intentional prototype solution and does not provide full pathfinding around complex dungeon geometry.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Movement Configuration")]
        [Tooltip("Horizontal pursuit speed in meters per second.")]
        [SerializeField] private float moveSpeed = 3f;

        [Tooltip("Distance in meters at which the enemy stops approaching the target.")]
        [SerializeField] private float stoppingDistance = 1.3f;

        [Tooltip("Downward acceleration applied for CharacterController grounding stability.")]
        [SerializeField] private float gravity = 20f;

        [Tooltip("Angular turning speed in degrees per second.")]
        [SerializeField] private float turnSpeed = 720f;

        [Header("Targeting")]
        [Tooltip("The target Transform to pursue. Auto-discovers tag 'Player' if unassigned.")]
        [SerializeField] private Transform target;

        private CharacterController characterController;
        private EnemyHealth health;
        private bool isDead;
        private bool isMoving;

        public float MoveSpeed => moveSpeed;
        public float StoppingDistance => stoppingDistance;
        public bool IsMoving => isMoving;
        public Transform Target => target;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            health = GetComponent<EnemyHealth>();
            ResolveTarget();
        }

        private void Start()
        {
            if (target == null)
            {
                ResolveTarget();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        private void ResolveTarget()
        {
            if (target == null)
            {
                var playerGo = GameObject.FindWithTag("Player");
                if (playerGo != null)
                {
                    target = playerGo.transform;
                }
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                isMoving = false;
                return;
            }

            float dt = Time.deltaTime;
            StepMovement(dt);
        }

        /// <summary>
        /// Steps pursuit calculation and character movement.
        /// Useful for fixed update or deterministic test harnesses.
        /// </summary>
        public void StepMovement(float dt)
        {
            if (Time.timeScale <= 0f || dt <= 0f || isDead || characterController == null || !characterController.enabled)
            {
                isMoving = false;
                return;
            }

            if (target == null)
            {
                ResolveTarget();
                if (target == null)
                {
                    isMoving = false;
                    characterController.Move(Vector3.down * gravity * dt);
                    return;
                }
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > stoppingDistance)
            {
                isMoving = true;
                Vector3 moveDir = toTarget.normalized;

                // Rotate smoothly toward movement direction
                if (moveDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * dt);
                }

                Vector3 motion = (moveDir * moveSpeed + Vector3.down * gravity) * dt;
                characterController.Move(motion);
            }
            else
            {
                isMoving = false;

                // Maintain facing direction toward target even when stopped
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * dt);
                }

                characterController.Move(Vector3.down * gravity * dt);
            }
        }

        private void HandleDied()
        {
            isDead = true;
            isMoving = false;

            if (characterController != null)
            {
                characterController.enabled = false;
            }

            enabled = false;
        }

        /// <summary>
        /// Sets a specific target to pursue.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
