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
