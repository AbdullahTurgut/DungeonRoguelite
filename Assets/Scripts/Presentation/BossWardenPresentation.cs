using UnityEngine;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Audio;

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

        [Header("Ash Warden audio (optional approved clips)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip meleeSwingClip;
        [SerializeField] private AudioClip slamChargeClip;
        [SerializeField] private AudioClip slamImpactClip;
        [SerializeField] private AudioClip boltChargeClip;
        [SerializeField] private AudioClip boltFireClip;
        [SerializeField] private AudioClip boltImpactClip;
        [SerializeField] private AudioClip hurtClip;
        [SerializeField] private AudioClip deathClip;
        [SerializeField, Range(0f, 1f)] private float attackVolume = .5f;
        [SerializeField, Range(0f, 1f)] private float hurtVolume = .25f;
        [SerializeField, Range(0f, 1f)] private float deathVolume = .6f;
        [SerializeField, Min(.1f)] private float hurtCooldown = .4f;
        [SerializeField] private Transform chargeAnchor;

        private WardenEmberFeedback emberFeedback;
        private float nextHurtAt;
        
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
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = .5f;
            audioSource.dopplerLevel = 0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 6f;
            audioSource.maxDistance = 35f;
            emberFeedback = new WardenEmberFeedback(transform);
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
            bossController.OnAttackPresented += HandleAttack;
            bossController.OnBoltFired += HandleBolt;
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
            bossController.OnAttackPresented -= HandleAttack;
            bossController.OnBoltFired -= HandleBolt;
            if (enemyHealth != null)
            {
                enemyHealth.OnHealthChanged -= HandleHealthChanged;
                enemyHealth.OnDied -= HandleDeath;
            }
            RestoreFlash();
            emberFeedback?.Clear();
            if (audioSource != null) audioSource.Stop();
        }
        
        private void LateUpdate()
        {
            if (flashing && Time.unscaledTime >= flashEndsAt)
            {
                RestoreFlash();
            }
            emberFeedback.Tick(chargeAnchor != null ? chargeAnchor.position : transform.position + Vector3.up * 1.5f,
                !dead && bossController.isActiveAndEnabled && bossController.State == BossWardenState.Telegraph);
            if (dead || Time.timeScale <= 0) return;
            
            if (animator != null && movement != null)
            {
                Vector3 velocity = movement.velocity;
                velocity.y = 0;
                animator.SetFloat(MoveSpeed, velocity.magnitude, 0.1f, Time.deltaTime);
            }
        }
        
        private void HandleTelegraph(string attackName)
        {
            if (dead) return;
            emberFeedback.BeginCharge(attackName);
            if (attackName == "Slam") Play(slamChargeClip, attackVolume * 0.28f);
            else if (attackName == "Bolt") Play(boltChargeClip, attackVolume * 0.22f);
            if (animator == null) return;
            
            if (attackName == "Strike") animator.SetTrigger(Strike);
            else if (attackName == "Slam") animator.SetTrigger(Slam);
            else if (attackName == "Bolt") animator.SetTrigger(Bolt);
        }
        
        private void HandleHealthChanged(float current, float maximum)
        {
            bool damaged = current < previousHealth && current < maximum;
            previousHealth = current;
            if (damaged && current > 0f && !dead && Time.unscaledTime >= nextHurtAt)
            {
                nextHurtAt = Time.unscaledTime + hurtCooldown;
                Play(hurtClip, hurtVolume);
            }
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
            emberFeedback.Clear();
            emberFeedback.Burst(transform.position + Vector3.up * 1.2f, 10);
            audioSource.Stop();
            // Detached existing helper survives corpse cleanup; never gates XP or completion.
            AudioHelper.PlayClipAtPoint(deathClip, transform.position, deathVolume, 1f, 1f);
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.SetTrigger(Death);
            }
        }

        private void Play(AudioClip clip, float volume)
        {
            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip, volume);
        }

        private void HandleAttack(string attack, Vector3 origin, Vector3 direction, float radius, float arc)
        {
            if (dead) return;
            audioSource.Stop();
            emberFeedback.Impact(origin, direction, radius, arc);
            Play(attack == "Slam" ? slamImpactClip : meleeSwingClip, attack == "Slam" ? attackVolume * 0.44f : attackVolume * 0.32f);
        }

        private void HandleBolt(EnemyProjectile shot)
        {
            if (dead || shot == null) return;
            emberFeedback.EndCharge();
            Play(boltFireClip, attackVolume * 0.24f);
            WardenEmberFeedback.ApplyBolt(shot);
            if (!shot.IsResolved) shot.OnImpact += HandleBoltImpact;
        }

        private void HandleBoltImpact(Vector3 position)
        {
            // A projectile may outlive the presentation component during cleanup.
            if (this == null || !isActiveAndEnabled || dead) return;
            emberFeedback.Burst(position, 6, .7f);
            AudioHelper.PlayClipAtPoint(boltImpactClip, position, attackVolume * 0.20f, 1f, 1f);
        }
    }
}
