using UnityEngine;
using DungeonRoguelite.Combat;

namespace DungeonRoguelite.Weapons
{
    /// <summary>
    /// Governs arrow movement, continuous sweep collision, damage application via IDamageable,
    /// and lifetime destruction.
    /// Operates on a single authoritative FixedUpdate collision sweep path.
    /// </summary>
    public class ArrowProjectile : MonoBehaviour
    {
        [Header("Projectile Configuration")]
        [Tooltip("SphereCast radius used for collision sweep.")]
        [SerializeField] private float sweepRadius = 0.15f;

        [Tooltip("Layers checked for projectile impacts.")]
        [SerializeField] private LayerMask collisionLayers = ~0;

        private Transform ownerRoot;
        private Vector3 travelDirection;
        private float speed = 18f;
        private float damage = 20f;
        private float lifetime = 2.0f;
        private float elapsedLifetime = 0f;
        private bool isInitialized = false;
        private bool hitResolved = false;

        public float Speed => speed;
        public float Damage => damage;
        public float Lifetime => lifetime;
        public float ElapsedLifetime => elapsedLifetime;
        public Vector3 TravelDirection => travelDirection;
        public Transform OwnerRoot => ownerRoot;
        public bool HitResolved => hitResolved;
        public float SweepRadius => sweepRadius;

        /// <summary>
        /// Initializes the arrow projectile parameters.
        /// </summary>
        /// <param name="owner">Root transform of the character who fired the projectile.</param>
        /// <param name="direction">Normalized travel direction on the horizontal plane.</param>
        /// <param name="projectileSpeed">Travel speed in m/s.</param>
        /// <param name="projectileDamage">Damage applied to IDamageable targets.</param>
        /// <param name="maxLifetime">Maximum duration before self-destruction in seconds.</param>
        public void Initialize(Transform owner, Vector3 direction, float projectileSpeed, float projectileDamage, float maxLifetime)
        {
            ownerRoot = owner;
            travelDirection = direction;
            travelDirection.y = 0f;
            if (travelDirection.sqrMagnitude < 0.001f)
            {
                travelDirection = Vector3.forward;
            }
            else
            {
                travelDirection.Normalize();
            }

            speed = projectileSpeed > 0f ? projectileSpeed : 18f;
            damage = projectileDamage;
            lifetime = maxLifetime > 0f ? maxLifetime : 2.0f;
            elapsedLifetime = 0f;
            hitResolved = false;
            isInitialized = true;
        }

        private void FixedUpdate()
        {
            if (Time.timeScale <= 0f || hitResolved)
            {
                return;
            }

            // Track lifetime using scaled fixed delta time
            elapsedLifetime += Time.fixedDeltaTime;
            if (elapsedLifetime >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (!isInitialized)
            {
                travelDirection = transform.forward;
                travelDirection.y = 0f;
                travelDirection.Normalize();
            }

            float travelDistance = speed * Time.fixedDeltaTime;
            Vector3 currentPos = transform.position;

            // Single authoritative collision sweep: SphereCast ignoring triggers
            RaycastHit[] hits = Physics.SphereCastAll(
                currentPos,
                sweepRadius,
                travelDirection,
                travelDistance,
                collisionLayers,
                QueryTriggerInteraction.Ignore
            );

            if (hits != null && hits.Length > 0)
            {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit hit = hits[i];
                    Collider hitCol = hit.collider;
                    if (hitCol == null) continue;

                    Transform hitTransform = hitCol.transform;

                    // Self check
                    if (hitTransform == transform || hitTransform.IsChildOf(transform))
                    {
                        continue;
                    }

                    // Hierarchy-safe owner check
                    if (ownerRoot != null && (hitTransform == ownerRoot || hitTransform.IsChildOf(ownerRoot)))
                    {
                        continue;
                    }

                    // Valid solid hit detected
                    ResolveHit(hitCol);
                    return;
                }
            }

            // No obstacle hit: advance forward
            transform.position = currentPos + travelDirection * travelDistance;
        }

        private void ResolveHit(Collider hitCol)
        {
            if (hitResolved)
            {
                return;
            }

            hitResolved = true;

            // Check for IDamageable on target or its parents
            IDamageable damageable = hitCol.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                damageable = hitCol.GetComponent<IDamageable>();
            }

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Destroy arrow upon any solid impact (environment or damageable target)
            Destroy(gameObject);
        }
    }
}
