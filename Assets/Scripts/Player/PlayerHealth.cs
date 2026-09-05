using System;
using UnityEngine;

namespace DungeonRoguelite.Player
{
    /// <summary>
    /// Manages player health state, damage reception, and death notification.
    /// Strictly decoupled from movement, aiming, and combat resolution abstractions.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Configuration")]
        [Tooltip("Maximum health capacity. Must be at least 1.")]
        [SerializeField] private float maxHealth = 100f;

        [Header("Runtime State (Read-Only)")]
        [SerializeField] private float currentHealth;
        [SerializeField] private bool isDead;

        /// <summary>
        /// Maximum health capacity.
        /// </summary>
        public float MaxHealth => maxHealth;

        /// <summary>
        /// Current health value, clamped between 0 and MaxHealth.
        /// </summary>
        public float CurrentHealth => currentHealth;

        /// <summary>
        /// True if current health has reached zero and death has been triggered.
        /// </summary>
        public bool IsDead => isDead;

        /// <summary>
        /// Current health normalized between 0.0 and 1.0.
        /// </summary>
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
        /// Applies damage to the player.
        /// Ignores zero, negative values, and any damage received while dead.
        /// Clamps health between 0 and MaxHealth.
        /// </summary>
        /// <param name="amount">The damage amount to apply.</param>
        public void TakeDamage(float amount)
        {
            // Ignore invalid damage amounts or damage while already dead
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
