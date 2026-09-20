using System;
using UnityEngine;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.Enemies
{
    /// <summary>A straight enemy shot with one authoritative swept collision query.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [SerializeField, Min(0.001f)] private float sweepRadius = 0.15f;
        private Transform owner;
        private EnemyHealth shooter;
        private Vector3 direction;
        private float speed;
        private float damage;
        private float expiresAt;
        private bool initialized;
        private bool resolved;

        public bool IsResolved => resolved;
        public Vector3 TravelDirection => direction;
        public float SweepRadius => sweepRadius;
        public event Action<EnemyProjectile> OnResolved;
        /// <summary>Optional presentation notification for accepted collisions only, after gameplay resolution.</summary>
        public event Action<Vector3> OnImpact;

        public void Initialize(Transform ownerRoot, Vector3 travelDirection, float projectileSpeed, float projectileDamage, float lifetime)
        {
            owner = ownerRoot;
            shooter = owner != null ? owner.GetComponentInParent<EnemyHealth>() : null;
            travelDirection.y = 0f;
            direction = travelDirection.sqrMagnitude > 0.001f ? travelDirection.normalized : Vector3.forward;
            speed = Mathf.Max(0f, projectileSpeed);
            damage = Mathf.Max(0f, projectileDamage);
            expiresAt = Time.unscaledTime + Mathf.Clamp(lifetime, 0.001f, 3f);
            initialized = true;
            if (shooter == null || shooter.IsDead) Cancel();
        }

        private void Update()
        {
            if (!initialized || resolved) return;
            if (shooter == null || shooter.IsDead || Time.unscaledTime >= expiresAt) { Cancel(); return; }
            if (Time.timeScale <= 0f) return;
            float distance = speed * Time.deltaTime;
            if (distance <= 0f) return;
            var hits = Physics.SphereCastAll(transform.position, sweepRadius, direction, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                var collider = hit.collider;
                if (collider == null) continue;
                var hitTransform = collider.transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform)) continue;
                if (owner != null && (hitTransform == owner || hitTransform.IsChildOf(owner))) continue;
                if (collider.GetComponentInParent<EnemyHealth>() != null) continue;

                // Distance zero is a valid initial-overlap impact. Resolve before callbacks
                // and deferred destruction, preventing multi-collider or reentrant damage.
                var player = collider.GetComponentInParent<PlayerHealth>();
                var impact = OnImpact;
                // SphereCast overlap hits have no reliable contact point; use the current
                // projectile position instead. Do not add another physics query.
                Vector3 impactPosition = hit.distance > 0f ? hit.point : transform.position;
                Cancel();
                if (player != null && !player.IsDead && shooter != null && !shooter.IsDead)
                    player.TakeDamage(damage);
                impact?.Invoke(impactPosition);
                return;
            }
            transform.position += direction * distance;
        }

        public void Cancel()
        {
            if (resolved) return;
            resolved = true;
            OnImpact = null;
            OnResolved?.Invoke(this);
            OnResolved = null;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            OnImpact = null;
            if (!resolved) { resolved = true; OnResolved?.Invoke(this); }
            OnResolved = null;
        }
    }
}
