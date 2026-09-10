using System;
using System.Collections;
using UnityEngine;
using DungeonRoguelite.Combat;

namespace DungeonRoguelite.Enemies
{
    /// <summary>
    /// Handles enemy melee attack logic against an IDamageable target within range.
    /// Strictly manages attack timing and damage application; contains no movement logic.
    /// Cleanly reacts to EnemyHealth.OnDied by ceasing all attack processing.
    /// </summary>
    public class EnemyAttack : MonoBehaviour, IEnemyAttack
    {
        [Header("Attack Configuration")]
        [Tooltip("Damage applied per successful attack.")]
        [SerializeField] private float damage = 10f;

        [Tooltip("Maximum distance from target to execute an attack in meters.")]
        [SerializeField] private float attackRange = 1.5f;

        [Tooltip("Cooldown period in seconds between attacks.")]
        [SerializeField] private float attackCooldown = 1.0f;
        [SerializeField, Min(0.05f)] private float windupDuration = 0.4f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.12f;
        [SerializeField, Min(0f)] private float recoveryDuration = 0.18f;
        [SerializeField, Range(1f,180f)] private float attackArc = 120f;

        [Header("Targeting")]
        [Tooltip("The target Transform to attack. Auto-discovers tag 'Player' if unassigned.")]
        [SerializeField] private Transform target;

        private EnemyHealth health;
        private IDamageable targetDamageable;
        private float nextAttackTime = 0f;
        private bool isDead;
        private EnemyVisualFeedback feedback;
        public bool IsAttacking { get; private set; }

        public float Damage => damage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public bool CanAttack => isActiveAndEnabled && !isDead && !IsAttacking && Time.time >= nextAttackTime;
        public Transform Target => target;

        /// <summary>
        /// Fired when the committed swing executes, including a swing that misses.
        /// </summary>
        public event Action OnAttack;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            feedback = GetComponent<EnemyVisualFeedback>();
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
            CancelAttack();
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
        /// Begins a committed wind-up if cooldown and range conditions are met.
        /// </summary>
        /// <returns>True if the attack started; damage is resolved only in its active window.</returns>
        public bool TryAttack()
        {
            if (!CanAttack || Time.timeScale <= 0f)
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
            Vector3 direction = distance > 0.001f ? toTarget / distance : transform.forward;
            IsAttacking = true;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            StartCoroutine(PerformAttack(transform.position, direction));
            return true;
        }

        private IEnumerator PerformAttack(Vector3 origin, Vector3 direction)
        {
            feedback?.ShowAttackCue(origin, direction, attackRange, attackArc, false);
            yield return new WaitForSeconds(windupDuration);
            feedback?.ShowAttackCue(origin, direction, attackRange, attackArc, true);
            OnAttack?.Invoke();
            float endsAt = Time.time + activeDuration;
            bool hit = false;
            while (Time.time < endsAt)
            {
                if (Time.timeScale > 0f && !hit && target != null && targetDamageable != null)
                {
                    Vector3 delta = target.position - origin; delta.y = 0f;
                    if (delta.sqrMagnitude <= attackRange * attackRange && Vector3.Angle(direction,delta) <= attackArc * 0.5f)
                    {
                        hit = true;
                        targetDamageable.TakeDamage(damage);
                    }
                }
                yield return null;
            }
            feedback?.HideAttackCue();
            yield return new WaitForSeconds(recoveryDuration);
            IsAttacking = false;
        }

        private void CancelAttack()
        {
            StopAllCoroutines(); IsAttacking = false; feedback?.HideAttackCue();
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
            if (target != newTarget) CancelAttack();
            target = newTarget;
            targetDamageable = null;
            ResolveTarget();
        }

        /// <summary>
        /// Scales the enemy instance's attack damage by a dungeon difficulty multiplier.
        /// Strictly affects this runtime instance; does not modify prefab assets.
        /// </summary>
        /// <param name="damageMultiplier">Multiplier to scale damage (e.g. 1.1 for +10%).</param>
        public void InitializeAttack(float damageMultiplier)
        {
            if (damageMultiplier <= 0f)
            {
                damageMultiplier = 1f;
            }

            damage = Mathf.Round(damage * damageMultiplier);
            if (damage < 1f)
            {
                damage = 1f;
            }
        }
    }
}
