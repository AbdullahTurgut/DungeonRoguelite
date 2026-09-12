using UnityEngine;
using DungeonRoguelite.Player;
using DungeonRoguelite.Weapons;
using DungeonRoguelite.Enemies;

namespace DungeonRoguelite.Presentation
{
    // Presentation only. The gameplay root, attack events and health remain authoritative.
    public sealed class MeleeHumanoidPresentation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController movement;
        [SerializeField] private MeleeWeapon weapon;
        [SerializeField] private BowWeapon bowWeapon;
        [SerializeField] private RifleWeapon rifleWeapon;
        [SerializeField] private EnemyAttack enemyAttack;
        [SerializeField] private EnemyRangedAttack rangedAttack;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField, Min(0.001f)] private float flashDuration = 0.08f;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float attackClipLength = 1.1f;
        [SerializeField, Range(0, 1)] private float releaseTime = 0.25f;
        [SerializeField] private float enemyWindup = 0.4f;
        [SerializeField] private float enemyFollowThrough = 0.3f;
        private float returnAt;
        private bool attacking, windingUp, dead;
        private bool EnemyIsAttacking => enemyAttack != null ? enemyAttack.IsAttacking : rangedAttack != null && rangedAttack.IsAttacking;
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveZ = Animator.StringToHash("MoveZ");
        private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");
        private static readonly int Locomotion = Animator.StringToHash("Locomotion");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock[] originalBlocks;
        private MaterialPropertyBlock flashBlock;
        private bool[] flashSupported;
        private float previousPlayerHealth;
        private float flashEndsAt;
        private bool flashing;

        private void Awake()
        {
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
            animator.applyRootMotion = false;
            if (weapon != null) weapon.OnAttack += Swing;
            if (bowWeapon != null) bowWeapon.OnAttack += Swing;
            if (rifleWeapon != null) rifleWeapon.OnAttack += Swing;
            if (enemyAttack != null) enemyAttack.OnAttack += Swing;
            if (rangedAttack != null) rangedAttack.OnAttack += Swing;
            if (playerHealth != null)
            {
                previousPlayerHealth = playerHealth.CurrentHealth;
                playerHealth.OnHealthChanged += HandlePlayerHealthChanged;
                playerHealth.OnDied += Die;
            }
            if (enemyHealth != null) enemyHealth.OnDied += Die;
        }

        private void OnDisable()
        {
            if (weapon != null) weapon.OnAttack -= Swing;
            if (bowWeapon != null) bowWeapon.OnAttack -= Swing;
            if (rifleWeapon != null) rifleWeapon.OnAttack -= Swing;
            if (enemyAttack != null) enemyAttack.OnAttack -= Swing;
            if (rangedAttack != null) rangedAttack.OnAttack -= Swing;
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandlePlayerHealthChanged;
                playerHealth.OnDied -= Die;
            }
            if (enemyHealth != null) enemyHealth.OnDied -= Die;
            RestoreFlash();
        }

        private void LateUpdate()
        {
            if (dead || Time.timeScale <= 0) return;
            if (EnemyIsAttacking && !attacking && !windingUp)
            {
                windingUp = true;
                animator.SetFloat(AttackSpeed, releaseTime * attackClipLength / Mathf.Max(.01f, enemyWindup));
                animator.Play(Attack, 0, 0);
            }
            if (windingUp && !EnemyIsAttacking)
            {
                windingUp = false;
                animator.CrossFadeInFixedTime(Locomotion, .06f);
            }
            if (attacking && Time.time >= returnAt)
            {
                // Keep recovery pose until the production enemy attack releases movement.
                if (EnemyIsAttacking) return;
                attacking = false;
                animator.CrossFadeInFixedTime(Locomotion, .06f);
            }
            if (flashing && Time.unscaledTime >= flashEndsAt)
            {
                RestoreFlash();
            }
            Vector3 local = transform.InverseTransformDirection(movement.velocity);
            local.y = 0;
            local = local.sqrMagnitude > .01f ? local.normalized : Vector3.zero;
            animator.SetFloat(MoveX, local.x, .08f, Time.deltaTime);
            animator.SetFloat(MoveZ, local.z, .08f, Time.deltaTime);
        }

        private void Swing()
        {
            if (dead) return;
            windingUp = false;
            attacking = true;
            float cooldown = weapon != null ? weapon.EffectiveAttackCooldown
                : bowWeapon != null ? bowWeapon.EffectiveAttackCooldown
                : rifleWeapon != null ? rifleWeapon.EffectiveAttackCooldown
                : 0.5f;
            float duration = (weapon != null || bowWeapon != null || rifleWeapon != null)
                ? Mathf.Clamp(cooldown * .85f, .05f, .45f)
                : enemyFollowThrough;
            returnAt = Time.time + duration;
            // Start at release because player damage is instantaneous; never delay gameplay for the clip.
            animator.SetFloat(AttackSpeed, (1 - releaseTime) * attackClipLength / Mathf.Max(.01f, duration));
            animator.Play(Attack, 0, releaseTime);
        }

        private void HandlePlayerHealthChanged(float current, float maximum)
        {
            bool damaged = current < previousPlayerHealth && current < maximum;
            previousPlayerHealth = current;
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

        private void Die()
        {
            if (dead) return;
            dead = true;
            // Defeat/upgrade overlays pause gameplay; corpse presentation can still finish.
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.Play(Death, 0, 0);
        }
    }
}
