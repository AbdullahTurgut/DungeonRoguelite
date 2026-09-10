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

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private EnemyHealth health;
        private MaterialPropertyBlock[] originals;
        private MaterialPropertyBlock flashBlock;
        private bool[] supported;
        private float previousHealth;
        private float flashEndsAt;
        private bool flashing;
        private LineRenderer attackCue;

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
            if (targetRenderers == null || targetRenderers.Length == 0)
                targetRenderers = GetComponentsInChildren<Renderer>(true);

            originals = new MaterialPropertyBlock[targetRenderers.Length];
            supported = new bool[targetRenderers.Length];
            flashBlock = new MaterialPropertyBlock();
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                originals[i] = new MaterialPropertyBlock();
                var renderer = targetRenderers[i];
                supported[i] = renderer != null && renderer.sharedMaterial != null &&
                    renderer.sharedMaterial.HasProperty(BaseColor);
            }
        }

        private void OnEnable()
        {
            previousHealth = health.CurrentHealth;
            health.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            HideAttackCue();
            health.OnHealthChanged -= HandleHealthChanged;
            Restore();
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
                renderer.SetPropertyBlock(flashBlock);
            }
            flashing = true;
            flashEndsAt = Time.unscaledTime + flashDuration;
        }

        private void Update()
        {
            if (flashing && Time.unscaledTime >= flashEndsAt) Restore();
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
