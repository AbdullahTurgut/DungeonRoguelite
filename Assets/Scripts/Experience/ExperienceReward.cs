using UnityEngine;
using DungeonRoguelite.Enemies;

namespace DungeonRoguelite.Experience
{
    /// <summary>
    /// Listens to EnemyHealth.OnDied and instantiates an ExperiencePickup containing the configured XP reward.
    /// Strictly manages XP drop generation; decoupled from EnemyHealth, combat, and wave logic.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public class ExperienceReward : MonoBehaviour
    {
        [Header("Reward Configuration")]
        [Tooltip("Amount of experience awarded when this enemy is defeated.")]
        [SerializeField] private int xpAmount = 10;

        [Tooltip("Prefab to spawn when this enemy dies.")]
        [SerializeField] private GameObject pickupPrefab;

        private EnemyHealth health;
        private bool hasRewarded = false;

        public int XPAmount => xpAmount;
        public GameObject PickupPrefab => pickupPrefab;
        public bool HasRewarded => hasRewarded;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
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

        /// <summary>
        /// Spawns the physical experience pickup upon enemy death.
        /// Guarded against duplicate death events.
        /// </summary>
        private void HandleDied()
        {
            if (hasRewarded)
            {
                return;
            }

            hasRewarded = true;

            if (pickupPrefab != null)
            {
                Vector3 spawnPos = transform.position;
                spawnPos.y = 0.25f;

                GameObject pickupGo = Instantiate(pickupPrefab, spawnPos, Quaternion.identity);
                var pickup = pickupGo.GetComponent<ExperiencePickup>();
                if (pickup != null)
                {
                    pickup.Initialize(xpAmount);
                }
            }
            else
            {
                Debug.LogWarning($"[ExperienceReward] Pickup prefab unassigned on '{name}'. No pickup spawned.");
            }
        }

        /// <summary>
        /// Sets the pickup prefab at runtime or during setup.
        /// </summary>
        public void SetPickupPrefab(GameObject prefab)
        {
            pickupPrefab = prefab;
        }

        /// <summary>
        /// Sets the experience amount at runtime or during setup.
        /// </summary>
        public void SetXPAmount(int amount)
        {
            xpAmount = Mathf.Max(1, amount);
        }
    }
}
