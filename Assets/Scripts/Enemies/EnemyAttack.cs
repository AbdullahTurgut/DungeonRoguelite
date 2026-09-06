using System;
using UnityEngine;
using DungeonRoguelite.Combat;

namespace DungeonRoguelite.Enemies
{
    /// <summary>
    /// Handles enemy melee attack logic against an IDamageable target within range.
    /// Strictly manages attack timing and damage application; contains no movement logic.
    /// Cleanly reacts to EnemyHealth.OnDied by ceasing all attack processing.
    /// </summary>
    public class EnemyAttack : MonoBehaviour
    {
        [Header("Attack Configuration")]
        [Tooltip("Damage applied per successful attack.")]
        [SerializeField] private float damage = 10f;

        [Tooltip("Maximum distance from target to execute an attack in meters.")]
        [SerializeField] private float attackRange = 1.5f;

        [Tooltip("Cooldown period in seconds between attacks.")]
        [SerializeField] private float attackCooldown = 1.0f;

        [Header("Targeting")]
        [Tooltip("The target Transform to attack. Auto-discovers tag 'Player' if unassigned.")]
        [SerializeField] private Transform target;

        private EnemyHealth health;
        private IDamageable targetDamageable;
        private float nextAttackTime = 0f;
        private bool isDead;

        public float Damage => damage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public bool CanAttack => !isDead && Time.time >= nextAttackTime;
        public Transform Target => target;

        /// <summary>
        /// Fired whenever an attack is successfully performed.
        /// </summary>
        public event Action OnAttack;

        private void Awake()
        {
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

            if (target != null)
            {
                targetDamageable = target.GetComponentInParent<IDamageable>();
                if (targetDamageable == null)
                {
                    targetDamageable = target.GetComponent<IDamageable>();
                }
            }
        }

        private void Update()
        {
            if (isDead || Time.timeScale <= 0f)
            {
                return;
            }

            if (target == null)
            {
                ResolveTarget();
                if (target == null) return;
            }

            if (Time.time < nextAttackTime)
            {
                return;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance <= attackRange)
            {
                TryAttack();
            }
        }

        /// <summary>
        /// Attempts to execute an attack against the current target if cooldown and range conditions are met.
        /// </summary>
        /// <returns>True if the attack succeeded; false otherwise.</returns>
        public bool TryAttack()
        {
            if (isDead || Time.timeScale <= 0f || Time.time < nextAttackTime)
            {
                return false;
            }

            if (target == null)
            {
                ResolveTarget();
                if (target == null) return false;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > attackRange)
            {
                return false;
            }

            nextAttackTime = Time.time + attackCooldown;

            if (targetDamageable == null)
            {
                ResolveTarget();
            }

            if (targetDamageable != null)
            {
                targetDamageable.TakeDamage(damage);
            }

            OnAttack?.Invoke();
            return true;
        }

        private void HandleDied()
        {
            isDead = true;
            enabled = false;
        }

        /// <summary>
        /// Sets a specific target to attack.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            targetDamageable = null;
            ResolveTarget();
        }
    }
}
