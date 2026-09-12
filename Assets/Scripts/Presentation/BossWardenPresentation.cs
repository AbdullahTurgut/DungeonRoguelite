using UnityEngine;
using DungeonRoguelite.Enemies;

namespace DungeonRoguelite.Presentation
{
    [RequireComponent(typeof(BossWardenController))]
    public sealed class BossWardenPresentation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController movement;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private Renderer[] targetRenderers;
        
        [SerializeField, Min(0.001f)] private float flashDuration = 0.08f;
        [SerializeField] private Color flashColor = Color.white;
        
        private BossWardenController bossController;
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int Strike = Animator.StringToHash("Strike");
        private static readonly int Slam = Animator.StringToHash("Slam");
        private static readonly int Bolt = Animator.StringToHash("Bolt");
        private static readonly int Death = Animator.StringToHash("Death");
        
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock[] originalBlocks;
        private MaterialPropertyBlock flashBlock;
        private bool[] flashSupported;
        private float previousHealth;
        private float flashEndsAt;
        private bool flashing;
        private bool dead;
        
        private void Awake()
        {
            bossController = GetComponent<BossWardenController>();
            if (targetRenderers != null && targetRenderers.Length > 0)
            {
                originalBlocks = new MaterialPropertyBlock[targetRenderers.Length];
                flashSupported = new bool[targetRenderers.Length];
                flashBlock = new MaterialPropertyBlock();
                for (int i = 0; i < targetRenderers.Length; i++)
                {
                    originalBlocks[i] = new MaterialPropertyBlock();
                    var r = targetRenderers[i];
                    flashSupported[i] = r != null && r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColor);
                }
            }
        }
        
        private void OnEnable()
        {
            if (animator != null) animator.applyRootMotion = false;
            bossController.OnTelegraphStarted += HandleTelegraph;
            if (enemyHealth != null)
            {
                previousHealth = enemyHealth.CurrentHealth;
                enemyHealth.OnHealthChanged += HandleHealthChanged;
                enemyHealth.OnDied += HandleDeath;
            }
        }
        
        private void OnDisable()
        {
            bossController.OnTelegraphStarted -= HandleTelegraph;
            if (enemyHealth != null)
            {
                enemyHealth.OnHealthChanged -= HandleHealthChanged;
                enemyHealth.OnDied -= HandleDeath;
            }
            RestoreFlash();
        }
        
        private void LateUpdate()
        {
            if (dead || Time.timeScale <= 0) return;
            
            if (flashing && Time.unscaledTime >= flashEndsAt)
            {
                RestoreFlash();
            }
            
            if (animator != null && movement != null)
            {
                Vector3 velocity = movement.velocity;
                velocity.y = 0;
                animator.SetFloat(MoveSpeed, velocity.magnitude, 0.1f, Time.deltaTime);
            }
        }
        
        private void HandleTelegraph(string attackName)
        {
            if (dead || animator == null) return;
            
            if (attackName == "Strike") animator.SetTrigger(Strike);
            else if (attackName == "Slam") animator.SetTrigger(Slam);
            else if (attackName == "Bolt") animator.SetTrigger(Bolt);
        }
        
        private void HandleHealthChanged(float current, float maximum)
        {
            bool damaged = current < previousHealth && current < maximum;
            previousHealth = current;
            if (!damaged || targetRenderers == null || targetRenderers.Length == 0) return;
            
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                var r = targetRenderers[i];
                if (r == null || !flashSupported[i]) continue;
                if (!flashing) r.GetPropertyBlock(originalBlocks[i]);
                r.GetPropertyBlock(flashBlock);
                flashBlock.SetColor(BaseColor, flashColor);
                r.SetPropertyBlock(flashBlock);
            }
            flashing = true;
            flashEndsAt = Time.unscaledTime + flashDuration;
        }
        
        private void RestoreFlash()
        {
            if (!flashing || targetRenderers == null) return;
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                var r = targetRenderers[i];
                if (r == null || !flashSupported[i]) continue;
                r.SetPropertyBlock(originalBlocks[i]);
            }
            flashing = false;
        }
        
        private void HandleDeath()
        {
            if (dead) return;
            dead = true;
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.SetTrigger(Death);
            }
        }
    }
}
