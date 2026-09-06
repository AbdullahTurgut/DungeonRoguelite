using System;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.Weapons
{
    /// <summary>
    /// Executes ranged bow attacks by instantiating ArrowProjectile instances along the character facing direction.
    /// Implements IPrimaryAttack for polymorphic integration with PlayerAttack.
    /// Strictly handles attack cooldown and projectile instantiation; decoupled from XP, enemies, UI, and progression.
    /// </summary>
    public class BowWeapon : MonoBehaviour, IPrimaryAttack
    {
        [Header("Weapon Configuration")]
        [Tooltip("Base damage dealt per arrow.")]
        [SerializeField] private float damage = 20f;

        [Tooltip("Minimum time in seconds between arrow shots.")]
        [SerializeField] private float attackCooldown = 0.6f;

        [Tooltip("Arrow travel speed in meters per second.")]
        [SerializeField] private float projectileSpeed = 18f;

        [Tooltip("Maximum lifetime of an arrow in seconds before auto-destruction.")]
        [SerializeField] private float projectileLifetime = 2.0f;

        [Header("Spawn Configuration")]
        [Tooltip("The arrow projectile prefab instantiated upon firing.")]
        [SerializeField] private GameObject arrowPrefab;

        [Tooltip("Transform determining the arrow spawn location and orientation.")]
        [SerializeField] private Transform projectileSpawnPoint;

        private float nextAttackTime = 0f;
        private Transform ownerTransform;
        private IDamageable ownerDamageable;
        private PlayerStats playerStats;

        public float Damage => damage;
        public float BaseDamage => damage;
        public float EffectiveDamage => damage * (playerStats != null ? playerStats.DamageMultiplier : 1f);
        public float AttackCooldown => attackCooldown;
        public float EffectiveAttackCooldown => (playerStats != null && playerStats.AttackSpeedMultiplier > 0f)
            ? (attackCooldown / playerStats.AttackSpeedMultiplier)
            : attackCooldown;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public GameObject ArrowPrefab => arrowPrefab;
        public Transform ProjectileSpawnPoint => projectileSpawnPoint;
        public bool CanAttack => Time.timeScale > 0f && Time.time >= nextAttackTime;

        /// <summary>
        /// Fired whenever an arrow is successfully fired.
        /// </summary>
        public event Action OnAttack;

        private void Awake()
        {
            ResolveOwner();
        }

        private void ResolveOwner()
        {
            ownerTransform = transform.root;
            ownerDamageable = GetComponentInParent<IDamageable>();
            playerStats = GetComponentInParent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            if (projectileSpawnPoint == null)
            {
                Transform foundSpawn = transform.Find("ProjectileSpawnPoint");
                if (foundSpawn == null && ownerTransform != null)
                {
                    var transforms = ownerTransform.GetComponentsInChildren<Transform>();
                    for (int i = 0; i < transforms.Length; i++)
                    {
                        if (transforms[i].name == "ProjectileSpawnPoint")
                        {
                            foundSpawn = transforms[i];
                            break;
                        }
                    }
                }
                projectileSpawnPoint = foundSpawn;
            }
        }

        /// <summary>
        /// Explicitly binds a PlayerStats component.
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            playerStats = stats;
        }

        /// <summary>
        /// Explicitly binds the arrow prefab.
        /// </summary>
        public void SetArrowPrefab(GameObject prefab)
        {
            arrowPrefab = prefab;
        }

        /// <summary>
        /// Explicitly binds the projectile spawn point.
        /// </summary>
        public void SetProjectileSpawnPoint(Transform spawnPoint)
        {
            projectileSpawnPoint = spawnPoint;
        }

        /// <summary>
        /// Attempts to execute a bow attack. Respects attackCooldown and pause state.
        /// </summary>
        /// <returns>True if the arrow was fired; false if blocked by cooldown, pause, or missing prefab.</returns>
        public bool TryAttack()
        {
            if (Time.timeScale <= 0f)
            {
                return false;
            }

            if (Time.time < nextAttackTime)
            {
                return false;
            }

            if (arrowPrefab == null)
            {
                Debug.LogError("[BowWeapon] Cannot fire: arrowPrefab is unassigned.", this);
                return false;
            }

            if (ownerTransform == null)
            {
                ResolveOwner();
            }

            nextAttackTime = Time.time + EffectiveAttackCooldown;
            ExecuteAttack();
            OnAttack?.Invoke();
            return true;
        }

        private void ExecuteAttack()
        {
            Vector3 spawnPosition;
            Vector3 direction;

            if (projectileSpawnPoint != null)
            {
                spawnPosition = projectileSpawnPoint.position;
                direction = projectileSpawnPoint.forward;
            }
            else
            {
                Transform refTransform = ownerTransform != null ? ownerTransform : transform;
                direction = refTransform.forward;
                spawnPosition = refTransform.position + direction * 0.8f + Vector3.up * 0.5f;
            }

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }
            else
            {
                direction.Normalize();
            }

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            GameObject arrowObj = Instantiate(arrowPrefab, spawnPosition, rotation);

            ArrowProjectile projectile = arrowObj.GetComponent<ArrowProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    ownerTransform != null ? ownerTransform : transform,
                    direction,
                    projectileSpeed,
                    EffectiveDamage,
                    projectileLifetime
                );
            }
            else
            {
                Debug.LogWarning("[BowWeapon] Instantiated arrowPrefab is missing ArrowProjectile component.", arrowObj);
            }
        }
    }
}
