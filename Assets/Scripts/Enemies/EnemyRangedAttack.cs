using System.Collections.Generic;
using UnityEngine;

namespace DungeonRoguelite.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    [DisallowMultipleComponent]
    public sealed class EnemyRangedAttack : MonoBehaviour, IEnemyAttack
    {
        [SerializeField] private float damage = 8f;
        [SerializeField] private float attackRange = 9f;
        [SerializeField] private float attackCooldown = 1.6f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float projectileLifetime = 3f;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform target;
        private EnemyHealth health;
        private float nextAttackTime;
        private readonly HashSet<EnemyProjectile> projectiles = new HashSet<EnemyProjectile>();

        public float Damage => damage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public EnemyProjectile ProjectilePrefab => projectilePrefab;
        public Transform Target => target;
        public int ActiveProjectileCount => projectiles.Count;

        private void Awake() { health = GetComponent<EnemyHealth>(); ResolveTarget(); }
        private void OnEnable() { health.OnDied += HandleDied; }
        private void OnDisable() { health.OnDied -= HandleDied; CancelProjectiles(); }
        private void ResolveTarget()
        {
            if (target != null) return;
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
        public void SetTarget(Transform newTarget) { target = newTarget; ResolveTarget(); }
        public void InitializeAttack(float multiplier)
        {
            damage = Mathf.Max(1f, Mathf.Round(damage * (multiplier > 0f ? multiplier : 1f)));
        }
        private void Update() { TryAttack(); }
        public bool TryAttack()
        {
            if (!isActiveAndEnabled || health.IsDead || Time.timeScale <= 0f || Time.time < nextAttackTime) return false;
            ResolveTarget();
            if (target == null || projectilePrefab == null) return false;
            Vector3 delta = target.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > attackRange * attackRange) return false;
            Vector3 direction = delta.sqrMagnitude > 0.001f ? delta.normalized : transform.forward;
            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction, Vector3.up));
            projectiles.Add(projectile);
            projectile.OnResolved += ForgetProjectile;
            projectile.Initialize(transform, direction, projectileSpeed, damage, projectileLifetime);
            nextAttackTime = Time.time + attackCooldown;
            return true;
        }
        private void ForgetProjectile(EnemyProjectile projectile)
        {
            projectile.OnResolved -= ForgetProjectile;
            projectiles.Remove(projectile);
        }
        private void HandleDied() { CancelProjectiles(); enabled = false; }
        private void CancelProjectiles()
        {
            // Cancel raises a local callback that removes the projectile from this set.
            foreach (var projectile in new List<EnemyProjectile>(projectiles))
                if (projectile != null) projectile.Cancel();
            projectiles.Clear();
        }
    }
}
