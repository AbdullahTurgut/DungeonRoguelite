using System;
using System.Collections.Generic;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.Weapons
{
    /// <summary>
    /// Handles melee attack execution, hit detection, and damage application.
    /// Performs an instantaneous Physics.OverlapSphere query filtered by a forward arc cone.
    /// Decoupled from input handling and specific character implementations.
    /// </summary>
    public class MeleeWeapon : MonoBehaviour, IPrimaryAttack
    {
        [Header("Weapon Configuration")]
        [Tooltip("Damage dealt per successful melee hit.")]
        [SerializeField] private float damage = 25f;

        [Tooltip("Minimum time in seconds between attacks.")]
        [SerializeField] private float attackCooldown = 0.5f;

        [Tooltip("Maximum reach of the melee attack in meters.")]
        [SerializeField] private float range = 2.5f;

        [Tooltip("Total horizontal arc angle of the attack cone in degrees.")]
        [SerializeField] private float arcAngle = 120f;

        [Tooltip("Layers checked for damageable entities.")]
        [SerializeField] private LayerMask targetLayers = ~0;

        private float nextAttackTime = 0f;
        private IDamageable ownerDamageable;
        private Transform ownerTransform;
        private PlayerStats playerStats;

        public float Damage => damage;
        public float BaseDamage => damage;
        public float EffectiveDamage => damage * (playerStats != null ? playerStats.DamageMultiplier : 1f);
        public float AttackCooldown => attackCooldown;
        public float EffectiveAttackCooldown => (playerStats != null && playerStats.AttackSpeedMultiplier > 0f)
            ? (attackCooldown / playerStats.AttackSpeedMultiplier)
            : attackCooldown;
        public float Range => range;
        public float ArcAngle => arcAngle;
        public LayerMask TargetLayers => targetLayers;
        public bool CanAttack => Time.timeScale > 0f && Time.time >= nextAttackTime;

        /// <summary>
        /// Fired whenever an attack is successfully performed.
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
        }

        /// <summary>
        /// Explicitly binds a PlayerStats component.
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            playerStats = stats;
        }

        /// <summary>
        /// Attempts to execute a melee attack. Respects attackCooldown and pause state.
        /// </summary>
        /// <returns>True if the attack was executed; false if blocked by cooldown or pause.</returns>
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

            nextAttackTime = Time.time + EffectiveAttackCooldown;
            ExecuteAttack();
            OnAttack?.Invoke();
            return true;
        }

        private void ExecuteAttack()
        {
            if (ownerTransform == null)
            {
                ResolveOwner();
            }

            Vector3 origin = ownerTransform != null ? ownerTransform.position : transform.position;
            Vector3 forward = ownerTransform != null ? ownerTransform.forward : transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            // Query colliders within reach
            Collider[] colliders = Physics.OverlapSphere(origin, range, targetLayers, QueryTriggerInteraction.Ignore);
            if (colliders == null || colliders.Length == 0)
            {
                return;
            }

            float halfArc = arcAngle * 0.5f;
            HashSet<IDamageable> hitDamageables = new HashSet<IDamageable>();

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider col = colliders[i];
                if (col == null) continue;

                // Prevent weapon owner from damaging itself
                if (ownerTransform != null && col.transform.IsChildOf(ownerTransform))
                {
                    continue;
                }

                // Determine closest point on collider to player origin
                Vector3 closestPoint = col.ClosestPoint(origin);
                Vector3 toTarget = closestPoint - origin;
                toTarget.y = 0f;

                // Check distance
                float distance = toTarget.magnitude;
                if (distance > range)
                {
                    continue;
                }

                // Check forward arc cone angle
                if (distance > 0.001f)
                {
                    float angle = Vector3.Angle(forward, toTarget.normalized);
                    if (angle > halfArc)
                    {
                        continue; // Outside forward arc cone or behind player
                    }
                }

                // Resolve damageable component on target (supporting collider on child objects)
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null)
                {
                    damageable = col.GetComponent<IDamageable>();
                }

                if (damageable != null && !ReferenceEquals(damageable, ownerDamageable))
                {
                    // De-duplicate targets: each entity damaged only once per swing
                    if (hitDamageables.Add(damageable))
                    {
                        damageable.TakeDamage(EffectiveDamage);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = ownerTransform != null ? ownerTransform.position : transform.position;
            Vector3 forward = ownerTransform != null ? ownerTransform.forward : transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            else forward.Normalize();

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(origin, range);

            Quaternion leftRot = Quaternion.Euler(0f, -arcAngle * 0.5f, 0f);
            Quaternion rightRot = Quaternion.Euler(0f, arcAngle * 0.5f, 0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + (leftRot * forward) * range);
            Gizmos.DrawLine(origin, origin + (rightRot * forward) * range);
        }
    }
}
