using System;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.Weapons
{
    /// <summary>
    /// Executes instantaneous ranged firearm attacks via hitscan query along the character facing direction.
    /// Implements IPrimaryAttack for polymorphic integration with PlayerAttack.
    /// Operates on a single authoritative collision sweep path sorted by ascending distance.
    /// Strictly decoupled from XP, enemies, UI, and progression.
    /// </summary>
    public class RifleWeapon : MonoBehaviour, IPrimaryAttack
    {
        [Header("Weapon Configuration")]
        [Tooltip("Base damage dealt per rifle hit.")]
        [SerializeField] private float damage = 10f;

        [Tooltip("Minimum time in seconds between rifle shots.")]
        [SerializeField] private float attackCooldown = 0.18f;

        [Tooltip("Maximum effective reach of the firearm in meters.")]
        [SerializeField] private float range = 25f;

        [Tooltip("Radius of the hitscan sphere sweep in meters (0 for infinitely thin raycast).")]
        [SerializeField] private float castRadius = 0.1f;

        [Tooltip("Layers checked for valid targets and solid obstacles.")]
        [SerializeField] private LayerMask targetLayers = ~0;

        [Header("Spawn Configuration")]
        [Tooltip("Transform determining the firearm muzzle location and aim orientation.")]
        [SerializeField] private Transform muzzlePoint;

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
        public float Range => range;
        public float CastRadius => castRadius;
        public LayerMask TargetLayers => targetLayers;
        public Transform MuzzlePoint => muzzlePoint;
        public bool CanAttack => Time.timeScale > 0f && Time.time >= nextAttackTime;

        /// <summary>
        /// Fired whenever a rifle shot is successfully executed.
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

            if (muzzlePoint == null)
            {
                Transform foundMuzzle = transform.Find("MuzzlePoint");
                if (foundMuzzle == null && ownerTransform != null)
                {
                    var transforms = ownerTransform.GetComponentsInChildren<Transform>();
                    for (int i = 0; i < transforms.Length; i++)
                    {
                        if (transforms[i].name == "MuzzlePoint")
                        {
                            foundMuzzle = transforms[i];
                            break;
                        }
                    }
                }
                muzzlePoint = foundMuzzle;
            }
        }

        /// <summary>
        /// Binds a PlayerStats component.
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            playerStats = stats;
        }

        /// <summary>
        /// Attempts to execute a rifle attack. Respects attackCooldown and pause state.
        /// </summary>
        /// <returns>True if the shot was fired; false if blocked by cooldown or pause.</returns>
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
            Vector3 origin;
            Vector3 forward;

            if (muzzlePoint != null)
            {
                origin = muzzlePoint.position;
                forward = muzzlePoint.forward;
            }
            else
            {
                Transform refTransform = ownerTransform != null ? ownerTransform : transform;
                origin = refTransform.position + Vector3.up * 1.0f + refTransform.forward * 0.8f;
                forward = refTransform.forward;
            }

            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            // Single authoritative collision query
            RaycastHit[] hits = castRadius > 0.001f
                ? Physics.SphereCastAll(origin, castRadius, forward, range, targetLayers, QueryTriggerInteraction.Ignore)
                : Physics.RaycastAll(origin, forward, range, targetLayers, QueryTriggerInteraction.Ignore);

            if (hits == null || hits.Length == 0)
            {
                return;
            }

            // Deterministically resolve hits by ascending hit.distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i].collider;
                if (col == null) continue;

                Transform hitTransform = col.transform;

                // Ignore weapon self
                if (hitTransform == transform || hitTransform.IsChildOf(transform))
                {
                    continue;
                }

                // Ignore owner root and owner descendants
                if (ownerTransform != null && (hitTransform == ownerTransform || hitTransform.IsChildOf(ownerTransform)))
                {
                    continue;
                }

                // The FIRST remaining valid solid hit is authoritative
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null)
                {
                    damageable = col.GetComponent<IDamageable>();
                }

                if (damageable != null)
                {
                    damageable.TakeDamage(EffectiveDamage);
                }

                // Stop at the first solid impact: whether damageable or environment obstacle.
                // Nothing behind this hit may be damaged (no penetration).
                break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up * 1f;
            Vector3 forward = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            else forward.Normalize();

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
            if (castRadius > 0.001f)
            {
                Gizmos.DrawWireSphere(origin, castRadius);
                Gizmos.DrawWireSphere(origin + forward * range, castRadius);
            }
            Gizmos.DrawLine(origin, origin + forward * range);
        }
    }
}
