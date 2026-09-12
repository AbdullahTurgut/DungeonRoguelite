using UnityEngine;
using DungeonRoguelite.Enemies;

namespace DungeonRoguelite.Presentation
{
    [RequireComponent(typeof(HollowCastellanController))]
    public sealed class HollowCastellanPresentation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController movement;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private Renderer[] targetRenderers;
        
        [SerializeField, Min(0.001f)] private float flashDuration = 0.08f;
        [SerializeField] private Color flashColor = Color.white;
        
        private HollowCastellanController bossController;
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int Strike = Animator.StringToHash("Strike"); // Cleave
        private static readonly int Dash = Animator.StringToHash("Dash"); // Use Slam or similar
        private static readonly int Fan = Animator.StringToHash("Fan"); // Use Bolt or similar
        private static readonly int Death = Animator.StringToHash("Death");
        
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock[] originalBlocks;
        
        private float previousHealth;
        
        private void Awake()
        {
            bossController = GetComponent<HollowCastellanController>();
            if (movement == null) movement = GetComponent<CharacterController>();
            if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
            
            if (enemyHealth != null) previousHealth = enemyHealth.CurrentHealth;

            if (targetRenderers != null && targetRenderers.Length > 0)
            {
                originalBlocks = new MaterialPropertyBlock[targetRenderers.Length];
                for (int i = 0; i < targetRenderers.Length; i++)
                {
                    originalBlocks[i] = new MaterialPropertyBlock();
                    if (targetRenderers[i] != null) targetRenderers[i].GetPropertyBlock(originalBlocks[i]);
                }
            }
        }
        
        private void OnEnable()
        {
            if (bossController != null)
            {
                bossController.OnTelegraphStarted += HandleTelegraphStarted;
            }
            if (enemyHealth != null)
            {
                enemyHealth.OnHealthChanged += HandleHealthChanged;
                enemyHealth.OnDied += HandleDeath;
            }
        }
        
        private void OnDisable()
        {
            if (bossController != null)
            {
                bossController.OnTelegraphStarted -= HandleTelegraphStarted;
            }
            if (enemyHealth != null)
            {
                enemyHealth.OnHealthChanged -= HandleHealthChanged;
                enemyHealth.OnDied -= HandleDeath;
            }
        }
        
        private void Update()
        {
            if (animator == null || movement == null || enemyHealth == null || enemyHealth.IsDead) return;
            
            Vector3 velocity = movement.velocity;
            velocity.y = 0;
            animator.SetFloat(MoveSpeed, velocity.magnitude);
        }
        
        private void HandleTelegraphStarted(string attackType)
        {
            if (animator == null) return;
            
            switch (attackType)
            {
                case "Cleave":
                    animator.SetTrigger(Strike);
                    break;
                case "Dash":
                    animator.SetTrigger(Dash);
                    break;
                case "Fan":
                    animator.SetTrigger(Fan);
                    break;
            }
        }
        
        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (currentHealth < previousHealth)
            {
                if (targetRenderers == null || targetRenderers.Length == 0) return;
                
                for (int i = 0; i < targetRenderers.Length; i++)
                {
                    if (targetRenderers[i] == null) continue;
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    targetRenderers[i].GetPropertyBlock(block);
                    block.SetColor(BaseColor, flashColor);
                    targetRenderers[i].SetPropertyBlock(block);
                }
                
                CancelInvoke(nameof(ResetFlash));
                Invoke(nameof(ResetFlash), flashDuration);
            }
            previousHealth = currentHealth;
        }
        
        private void ResetFlash()
        {
            if (targetRenderers == null || originalBlocks == null) return;
            
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                if (targetRenderers[i] != null && i < originalBlocks.Length)
                {
                    targetRenderers[i].SetPropertyBlock(originalBlocks[i]);
                }
            }
        }
        
        private void HandleDeath()
        {
            if (animator != null)
            {
                animator.SetTrigger(Death);
                animator.SetFloat(MoveSpeed, 0f);
            }
            ResetFlash();
        }
    }
}
