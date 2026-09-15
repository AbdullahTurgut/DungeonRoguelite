using System;
using System.Collections;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Player;
using DungeonRoguelite.Presentation;

namespace DungeonRoguelite.Weapons
{
    /// <summary>
    /// Executes instantaneous ranged firearm attacks via hitscan query along the character facing direction.
    /// Implements IPrimaryAttack for polymorphic integration with PlayerAttack.
    /// Operates on a single authoritative collision sweep path sorted by ascending distance.
    /// Provides lightweight muzzle flash and hitscan tracer feedback for successful shots.
    /// Strictly decoupled from XP, enemies, UI, and progression. Zero ammo/reload mechanics.
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

        [Header("Visual Feedback")]
        [Tooltip("Duration in seconds that the bullet tracer remains visible.")]
        [SerializeField] private float tracerDuration = 0.05f;

        [Tooltip("Duration in seconds that the muzzle flash remains visible.")]
        [SerializeField] private float flashDuration = 0.04f;

        [Tooltip("Optional LineRenderer used for drawing the bullet tracer.")]
        [SerializeField] private LineRenderer tracerLine;

        [Tooltip("Optional GameObject toggled for muzzle flash visual.")]
        [SerializeField] private GameObject muzzleFlashVisual;

        private float nextAttackTime = 0f;
        private Transform ownerTransform;
        private IDamageable ownerDamageable;
        private PlayerStats playerStats;
        private Coroutine feedbackCoroutine;
        private Renderer flashRenderer;
        private GameObject styledFlash;
        private Vector3 baseFlashScale;
        private MaterialPropertyBlock flashBlock;

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
        public float TracerDuration => tracerDuration;
        public float FlashDuration => flashDuration;
        public LineRenderer TracerLine => tracerLine;
        public GameObject MuzzleFlashVisual => muzzleFlashVisual;
        public bool CanAttack => Time.timeScale > 0f && Time.time >= nextAttackTime;

        /// <summary>
        /// Fired whenever a rifle shot is successfully executed.
        /// </summary>
        public event Action OnAttack;

        /// <summary>
        /// Fired whenever a rifle shot is successfully executed, passing the shot origin and resolved hit/end point.
        /// </summary>
        public event Action<Vector3, Vector3> OnShotFired;

        private void Awake()
        {
            ResolveOwner();
            EnsureFeedbackComponents();
        }

        private void OnDisable()
        {
            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
                feedbackCoroutine = null;
            }

            if (muzzleFlashVisual != null)
            {
                muzzleFlashVisual.SetActive(false);
            }

            if (tracerLine != null)
            {
                tracerLine.enabled = false;
            }
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
        /// Ensures fallback tracer LineRenderer and muzzle flash objects exist if not pre-configured on prefab.
        /// </summary>
        public void EnsureFeedbackComponents()
        {
            Transform anchor = muzzlePoint != null ? muzzlePoint : transform;

            if (tracerLine == null)
            {
                tracerLine = anchor.GetComponentInChildren<LineRenderer>();
            }

            if (muzzleFlashVisual == null)
            {
                Transform flashChild = anchor.Find("MuzzleFlash");
                if (flashChild != null)
                {
                    muzzleFlashVisual = flashChild.gameObject;
                }
            }
        }

        /// <summary>
        /// Sets visual feedback components explicitly.
        /// </summary>
        public void SetFeedbackReferences(LineRenderer line, GameObject flash)
        {
            tracerLine = line;
            muzzleFlashVisual = flash;
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

            Vector3 endPoint = origin + forward * range;

            // Single authoritative collision query
            RaycastHit[] hits = castRadius > 0.001f
                ? Physics.SphereCastAll(origin, castRadius, forward, range, targetLayers, QueryTriggerInteraction.Ignore)
                : Physics.RaycastAll(origin, forward, range, targetLayers, QueryTriggerInteraction.Ignore);

            if (hits != null && hits.Length > 0)
            {
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

                    // Solid hit determined: calculate authoritative endpoint
                    Vector3 hitPt = hits[i].point;
                    if (hitPt == Vector3.zero)
                    {
                        hitPt = origin + forward * hits[i].distance;
                    }
                    endPoint = hitPt;

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

            // Fire shot event with resolved endpoint
            OnShotFired?.Invoke(origin, endPoint);

            // Trigger lightweight visual feedback
            TriggerVisualFeedback(origin, endPoint);
        }

        private void TriggerVisualFeedback(Vector3 origin, Vector3 endPoint)
        {
            EnsureFeedbackComponents();

            if (tracerLine != null)
            {
                tracerLine.positionCount = 2;
                tracerLine.SetPosition(0, origin);
                tracerLine.SetPosition(1, endPoint);

                int tier = AttackVfxStyle.EquippedTier(this);
                // Absolute world widths avoid multiplying the authored curve into a beam.
                AttackVfxStyle.Configure(tracerLine, ShotColor(tier),
                    tier == 2 ? 0.065f : tier == 1 ? 0.05f : 0.035f);
                tracerLine.useWorldSpace = true;

                tracerLine.enabled = true;
            }

            if (muzzleFlashVisual != null)
            {
                int tier = AttackVfxStyle.EquippedTier(this);
                if (styledFlash != muzzleFlashVisual)
                {
                    styledFlash = muzzleFlashVisual;
                    baseFlashScale = styledFlash.transform.localScale;
                    flashRenderer = styledFlash.GetComponent<Renderer>();
                }
                // Scale only the dedicated flash mesh, never the combat anchor.
                styledFlash.transform.localScale = Vector3.Scale(baseFlashScale,
                    tier == 2 ? new Vector3(1.25f, 1.25f, 2f) :
                    tier == 1 ? new Vector3(1.1f, 1.1f, 1.4f) : Vector3.one);
                if (flashRenderer != null)
                {
                    if (flashBlock == null) flashBlock = new MaterialPropertyBlock();
                    flashRenderer.sharedMaterial = AttackVfxStyle.Material;
                    flashRenderer.GetPropertyBlock(flashBlock);
                    flashBlock.SetColor("_Color", ShotColor(tier));
                    flashRenderer.SetPropertyBlock(flashBlock);
                    flashRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                muzzleFlashVisual.SetActive(true);
            }

            if (gameObject.activeInHierarchy)
            {
                if (feedbackCoroutine != null)
                {
                    StopCoroutine(feedbackCoroutine);
                }
                feedbackCoroutine = StartCoroutine(HideFeedbackRoutine());
            }
        }

        private static Color ShotColor(int tier) => tier == 2 ? new Color(0.85f, 0.5f, 1f) :
            tier == 1 ? new Color(0.2f, 0.85f, 1f) : new Color(1f, 0.92f, 0.72f, 0.85f);

        private IEnumerator HideFeedbackRoutine()
        {
            float elapsed = 0f;
            float maxDuration = Mathf.Max(tracerDuration, flashDuration);

            while (elapsed < maxDuration)
            {
                // Presentation expires even when a level-up pauses gameplay.
                elapsed += Time.unscaledDeltaTime;

                if (muzzleFlashVisual != null && elapsed >= flashDuration)
                {
                    muzzleFlashVisual.SetActive(false);
                }

                if (tracerLine != null && elapsed >= tracerDuration)
                {
                    tracerLine.enabled = false;
                }

                yield return null;
            }

            if (muzzleFlashVisual != null) muzzleFlashVisual.SetActive(false);
            if (tracerLine != null) tracerLine.enabled = false;
            feedbackCoroutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up * 1f;
            Vector3 forward = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            else forward.Normalize();

            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.5f);
            if (castRadius > 0.001f)
            {
                Gizmos.DrawWireSphere(origin, castRadius);
                Gizmos.DrawWireSphere(origin + forward * range, castRadius);
            }
            Gizmos.DrawLine(origin, origin + forward * range);
        }
    }
}
