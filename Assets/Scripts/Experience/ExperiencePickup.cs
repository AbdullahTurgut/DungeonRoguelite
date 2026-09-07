using UnityEngine;

namespace DungeonRoguelite.Experience
{
    /// <summary>
    /// Represents a collectible experience orb/gem dropped in the dungeon.
    /// Automatically acquires the active player receiver and travels toward the player for collection.
    /// Also detects player collision via trigger volume as immediate collection.
    /// Awards experience exactly once and destroys itself.
    /// Strictly handles collection state; contains no hard-coded enemy or character logic.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ExperiencePickup : MonoBehaviour
    {
        [Header("Experience Configuration")]
        [Tooltip("Amount of XP awarded to the player upon collection.")]
        [SerializeField] private int xpValue = 10;

        [Header("Homing Settings")]
        [Tooltip("Initial delay in seconds before the pickup starts traveling toward the player.")]
        [SerializeField] private float homingDelay = 0.2f;

        [Tooltip("Target travel speed in meters per second.")]
        [SerializeField] private float moveSpeed = 12f;

        [Tooltip("Acceleration in meters per second squared.")]
        [SerializeField] private float acceleration = 25f;

        [Tooltip("Distance threshold in meters at which the pickup is automatically collected.")]
        [SerializeField] private float collectionDistance = 0.75f;

        private Transform targetTransform;
        private PlayerExperience targetExperience;
        private float currentSpeed = 0f;
        private float lifetime = 0f;
        private bool isCollected = false;

        public int XPValue => xpValue;
        public bool IsCollected => isCollected;
        public float HomingDelay => homingDelay;
        public float MoveSpeed => moveSpeed;
        public float CollectionDistance => collectionDistance;
        public Transform TargetTransform => targetTransform;
        public PlayerExperience TargetExperience => targetExperience;

        /// <summary>
        /// Sets the experience value of this pickup instance.
        /// Ignores non-positive values, ensuring at least 1 XP.
        /// </summary>
        /// <param name="value">The XP value to assign.</param>
        public void Initialize(int value)
        {
            xpValue = Mathf.Max(1, value);
        }

        /// <summary>
        /// Explicitly assigns the target transform for homing and resolves its PlayerExperience.
        /// </summary>
        public void SetTarget(Transform target)
        {
            targetTransform = target;
            if (target != null)
            {
                targetExperience = target.GetComponentInParent<PlayerExperience>();
                if (targetExperience == null)
                {
                    targetExperience = target.GetComponent<PlayerExperience>();
                }
            }
        }

        /// <summary>
        /// Explicitly assigns the target PlayerExperience for homing and collection.
        /// </summary>
        public void SetTarget(PlayerExperience exp)
        {
            targetExperience = exp;
            if (exp != null)
            {
                targetTransform = exp.transform;
            }
        }

        private void Update()
        {
            if (isCollected)
            {
                return;
            }

            if (Time.timeScale <= 0f)
            {
                return;
            }

            EnsureTarget();

            lifetime += Time.deltaTime;
            if (lifetime < homingDelay)
            {
                return;
            }

            Vector3 targetPos;
            if (targetTransform != null)
            {
                targetPos = targetTransform.position + Vector3.up * 0.75f;
            }
            else if (targetExperience != null)
            {
                targetPos = targetExperience.transform.position + Vector3.up * 0.75f;
            }
            else
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, targetPos);
            if (distance <= collectionDistance)
            {
                if (targetExperience != null)
                {
                    TryCollect(targetExperience);
                }
                return;
            }

            currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);
        }

        private void EnsureTarget()
        {
            if (targetExperience != null && targetTransform != null)
            {
                return;
            }

            if (targetExperience == null)
            {
                if (targetTransform != null)
                {
                    targetExperience = targetTransform.GetComponentInParent<PlayerExperience>();
                    if (targetExperience == null)
                    {
                        targetExperience = targetTransform.GetComponent<PlayerExperience>();
                    }
                }
                else if (PlayerExperience.ActiveInstance != null)
                {
                    targetExperience = PlayerExperience.ActiveInstance;
                    targetTransform = targetExperience.transform;
                }
            }

            if (targetTransform == null && targetExperience != null)
            {
                targetTransform = targetExperience.transform;
            }
        }

        /// <summary>
        /// Attempts to collect this pickup for the specified PlayerExperience.
        /// Returns true if collected; false if already collected or if playerExperience is null.
        /// Guarded against duplicate collection and deferred destruction.
        /// </summary>
        /// <param name="playerExperience">The PlayerExperience to award XP to.</param>
        /// <returns>True if XP was awarded; false if already collected or invalid target.</returns>
        public bool TryCollect(PlayerExperience playerExperience)
        {
            if (isCollected || playerExperience == null)
            {
                return false;
            }

            isCollected = true;
            playerExperience.GainExperience(xpValue);
            Destroy(gameObject);
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected)
            {
                return;
            }

            var playerExperience = other.GetComponentInParent<PlayerExperience>();
            if (playerExperience == null)
            {
                playerExperience = other.GetComponent<PlayerExperience>();
            }

            if (playerExperience != null)
            {
                TryCollect(playerExperience);
            }
        }
    }
}
