using UnityEngine;

namespace DungeonRoguelite.Enemies
{
    /// <summary>Brief, instance-local hit feedback; never changes gameplay state or materials.</summary>
    [RequireComponent(typeof(EnemyHealth))]
    [DisallowMultipleComponent]
    public sealed class EnemyVisualFeedback : MonoBehaviour
    {
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField, Min(0.001f)] private float flashDuration = 0.08f;
        [SerializeField] private Color flashColor = Color.white;

        [Header("Normal Enemy Impact and Death")]
        [SerializeField] private bool normalEnemyFeedback;
        [Tooltip("Visual-only child. Never assign the actor or a physics hierarchy.")]
        [SerializeField] private Transform deathVisualRoot;
        [SerializeField, Range(.15f, .4f)] private float deathDuration = .26f;
        [SerializeField, Range(.5f, 1.5f)] private float impactScale = 1f;
        [SerializeField, Range(2, 8)] private int hitParticleCount = 4;
        [SerializeField, Range(3, 12)] private int deathParticleCount = 7;
        [SerializeField] private Material impactMaterial;
        [SerializeField] private GameObject[] hideOnDeathStart;
        [SerializeField] private GameObject[] hideOnDeathEnd;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private EnemyHealth health;
        private MaterialPropertyBlock[] originals;
        private MaterialPropertyBlock flashBlock;
        private bool[] supported;
        private float previousHealth;
        private float flashEndsAt;
        private bool flashing;
        private LineRenderer attackCue;
        private bool normalFeedbackAllowed;
        private ParticleSystem impactParticles;
        private bool dying;
        private float deathStartedAt;
        private Vector3 originalVisualPosition;
        private Vector3 originalVisualScale;

        // Fixed world-space telegraph, separate from the body's damage flash.
        public void ShowAttackCue(Vector3 origin, Vector3 direction, float range, float arc, bool active)
        {
            if (attackCue == null)
            {
                var root = new GameObject("EnemyAttackCue"); root.transform.SetParent(transform,false);
                attackCue = root.AddComponent<LineRenderer>();
                attackCue.useWorldSpace = true;
                attackCue.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                attackCue.receiveShadows = false;
                foreach (var renderer in targetRenderers)
                    if (renderer != null && renderer.sharedMaterial != null) { attackCue.sharedMaterial = renderer.sharedMaterial; break; }
            }
            attackCue.enabled = true;
            attackCue.widthMultiplier = active ? 0.12f : 0.07f;
            var color = active ? new Color(1f,0.15f,0.05f) : new Color(1f,0.7f,0.1f);
            var block = new MaterialPropertyBlock(); block.SetColor(BaseColor,color); attackCue.SetPropertyBlock(block);
            origin += Vector3.up * 0.12f;
            if (arc <= 0f)
            {
                attackCue.positionCount = 2;
                attackCue.SetPosition(0,origin); attackCue.SetPosition(1,origin+direction*range);
            }
            else
            {
                const int segments = 16;
                attackCue.positionCount = segments+3;
                attackCue.SetPosition(0,origin);
                for (int i=0;i<=segments;i++)
                    attackCue.SetPosition(i+1,origin+Quaternion.AngleAxis(Mathf.Lerp(-arc/2,arc/2,i/(float)segments),Vector3.up)*direction*range);
                attackCue.SetPosition(segments+2,origin);
            }
        }

        public void HideAttackCue() { if (attackCue != null) attackCue.enabled = false; }

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            // Explicit prefab opt-in plus the existing boss identity contract.
            normalFeedbackAllowed = normalEnemyFeedback &&
                GetComponentInParent<IBossPresentation>() == null &&
                GetComponentInChildren<IBossPresentation>(true) == null;
            if (deathVisualRoot != null && (deathVisualRoot == transform ||
                !deathVisualRoot.IsChildOf(transform) || deathVisualRoot.GetComponentInChildren<Collider>(true) != null))
                deathVisualRoot = null;
            if (deathVisualRoot != null)
            {
                originalVisualPosition = deathVisualRoot.localPosition;
                originalVisualScale = deathVisualRoot.localScale;
            }
            if (targetRenderers == null || targetRenderers.Length == 0)
                targetRenderers = GetComponentsInChildren<Renderer>(true);

            originals = new MaterialPropertyBlock[targetRenderers.Length];
            supported = new bool[targetRenderers.Length];
            flashBlock = new MaterialPropertyBlock();
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                originals[i] = new MaterialPropertyBlock();
                var renderer = targetRenderers[i];
                if (renderer != null)
                {
                    supported[i] = renderer.sharedMaterial != null &&
                        renderer.sharedMaterial.HasProperty(BaseColor);
                }
            }
        }

        private void OnEnable()
        {
            previousHealth = health.CurrentHealth;
            health.OnHealthChanged += HandleHealthChanged;
            if (normalFeedbackAllowed) health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            HideAttackCue();
            health.OnHealthChanged -= HandleHealthChanged;
            if (normalFeedbackAllowed) health.OnDied -= HandleDied;
            Restore();
            if (impactParticles != null) impactParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (dying)
            {
                if (deathVisualRoot != null)
                {
                    deathVisualRoot.localPosition = originalVisualPosition;
                    deathVisualRoot.localScale = originalVisualScale;
                }
                if (hideOnDeathStart != null)
                {
                    foreach (var obj in hideOnDeathStart)
                        if (obj != null) obj.SetActive(true);
                }
                if (hideOnDeathEnd != null)
                {
                    foreach (var obj in hideOnDeathEnd)
                        if (obj != null) obj.SetActive(true);
                }
            }
            dying = false;
        }

        private void HandleHealthChanged(float current, float maximum)
        {
            // Initialization/restoration sets health to maximum and must not look like a hit.
            bool damaged = current < previousHealth && current < maximum;
            previousHealth = current;
            if (!damaged) return;

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                var renderer = targetRenderers[i];
                if (!supported[i] || renderer == null) continue;
                // Capture once per flash, so repeated hits do not save the flash as the baseline.
                if (!flashing) renderer.GetPropertyBlock(originals[i]);
                renderer.GetPropertyBlock(flashBlock);
                flashBlock.SetColor(BaseColor, flashColor);
                // Palette-textured models already have a white tint: briefly bypass the palette
                // so the existing flash is visible, then restore the original property block.
                if (normalFeedbackAllowed) flashBlock.SetTexture(BaseMap, Texture2D.whiteTexture);
                renderer.SetPropertyBlock(flashBlock);
            }
            flashing = true;
            flashEndsAt = Time.unscaledTime + flashDuration;
            // Lethal hits receive only the death burst, never two overlapping bursts.
            if (normalFeedbackAllowed && current > 0f) EmitImpact(false);
        }

        private void Update()
        {
            if (flashing && Time.unscaledTime >= flashEndsAt) Restore();
        }

        private void HandleDied()
        {
            if (dying) return;
            dying = true;
            deathStartedAt = Time.unscaledTime;
            EmitImpact(true);
            if (hideOnDeathStart != null)
            {
                foreach (var obj in hideOnDeathStart)
                    if (obj != null) obj.SetActive(false);
            }
            // Existing health listeners still stop attacks, award XP and remove the wave member now.
            // CorpseCleanup keeps its existing ownership and destruction delay.
        }

        private void LateUpdate()
        {
            if (!dying || deathVisualRoot == null) return;
            float t = Mathf.Clamp01((Time.unscaledTime - deathStartedAt) / Mathf.Max(.15f, deathDuration));
            float collapse = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.15f, 1f, t));
            deathVisualRoot.localScale = originalVisualScale * (1f - collapse);
            deathVisualRoot.localPosition = originalVisualPosition + Vector3.down * (.16f * collapse);
            if (t >= 1f && hideOnDeathEnd != null)
            {
                foreach (var obj in hideOnDeathEnd)
                    if (obj != null) obj.SetActive(false);
            }
        }

        private Vector3 ImpactPosition()
        {
            // Health events provide no contact point. Use visible body bounds without another physics query.
            Bounds bounds = default;
            bool found = false;
            foreach (var renderer in targetRenderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                if (!found) { bounds = renderer.bounds; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return found ? bounds.center + Vector3.up * .1f : transform.position + Vector3.up;
        }

        private void EmitImpact(bool death)
        {
            if (impactMaterial == null) return;
            if (impactParticles == null)
            {
                var root = new GameObject("NormalEnemyImpact");
                root.transform.SetParent(transform, false);
                impactParticles = root.AddComponent<ParticleSystem>();
                impactParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = impactParticles.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = .25f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(.10f, .18f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(.7f * impactScale, 1.5f * impactScale);
                main.startSize = new ParticleSystem.MinMaxCurve(.035f * impactScale, .07f * impactScale);
                main.startColor = new Color(1f, .9f, .7f, .9f);
                main.maxParticles = 12;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.useUnscaledTime = true;
                var emission = impactParticles.emission;
                emission.enabled = false;
                var shape = impactParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = .06f * impactScale;
                var size = impactParticles.sizeOverLifetime;
                size.enabled = true;
                size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
                var color = impactParticles.colorOverLifetime;
                color.enabled = true;
                var gradient = new Gradient();
                gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
                color.color = gradient;
                var renderer = impactParticles.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = impactMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            impactParticles.transform.position = ImpactPosition();
            // Reuse one capped emitter per enemy; repeated damage cannot accumulate unbounded effects.
            impactParticles.Clear();
            impactParticles.Play();
            impactParticles.Emit(death ? deathParticleCount : hitParticleCount);
        }

        private void Restore()
        {
            if (!flashing) return;
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                if (supported[i] && targetRenderers[i] != null)
                    targetRenderers[i].SetPropertyBlock(originals[i].isEmpty ? null : originals[i]);
            }
            flashing = false;
        }
    }
}
