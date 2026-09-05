using System;
using UnityEngine;
using DungeonRoguelite.Combat;

namespace DungeonRoguelite.Enemies
{
    /// <summary>
    /// Manages enemy health state, damage reception, and death notification.
    /// Implements IDamageable for polymorphic combat interaction.
    /// Strictly owns health state and events only; does not directly manipulate movement, attack, or physics components.
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Configuration")]
        [Tooltip("Maximum health capacity. Must be at least 1.")]
        [SerializeField] private float maxHealth = 50f;

        [Header("Runtime State (Read-Only)")]
        [SerializeField] private float currentHealth;
        [SerializeField] private bool isDead;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsDead => isDead;
        public float HealthNormalized => maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        /// <summary>
        /// Fired whenever current health changes. Passes (currentHealth, maxHealth).
        /// </summary>
        public event Action<float, float> OnHealthChanged;

        /// <summary>
        /// Fired exactly once when health reaches zero.
        /// </summary>
        public event Action OnDied;

        private void Awake()
        {
            ValidateMaxHealth();
            currentHealth = maxHealth;
            isDead = false;
        }

        private void OnValidate()
        {
            ValidateMaxHealth();
        }

        private void ValidateMaxHealth()
        {
            if (maxHealth < 1f)
            {
                maxHealth = 1f;
            }
        }

        /// <summary>
        /// Applies damage to the enemy.
        /// Ignores non-positive values and damage received while already dead.
        /// Clamps health between 0 and MaxHealth.
        /// </summary>
        /// <param name="amount">The damage amount to apply.</param>
        public void TakeDamage(float amount)
        {
            if (amount <= 0f || isDead)
            {
                return;
            }

            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f && !isDead)
            {
                isDead = true;
                OnDied?.Invoke();
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Test-oriented reset method used exclusively by automated verification suites.
        /// </summary>
        public void ResetHealthForTesting(float newMaxHealth = -1f)
        {
            if (newMaxHealth >= 1f)
            {
                maxHealth = newMaxHealth;
            }
            ValidateMaxHealth();
            currentHealth = maxHealth;
            isDead = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
#endif
    }
}
