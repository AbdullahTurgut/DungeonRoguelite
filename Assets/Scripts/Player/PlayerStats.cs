using System;
using UnityEngine;

namespace DungeonRoguelite.Player
{
    /// <summary>
    /// Owns temporary runtime player stat modifiers (multipliers) for the current dungeon run.
    /// Purely holds and modifies numeric bonuses with additive percentage stacking.
    /// Completely decoupled from UI, level-up logic, and upgrade definitions.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Runtime Multipliers")]
        [Tooltip("Multiplicative modifier applied to base weapon damage (default: 1.0).")]
        [SerializeField] private float damageMultiplier = 1.0f;

        [Tooltip("Multiplicative modifier applied to base attack speed / cooldown rate (default: 1.0).")]
        [SerializeField] private float attackSpeedMultiplier = 1.0f;

        [Tooltip("Multiplicative modifier applied to base movement speed (default: 1.0).")]
        [SerializeField] private float movementSpeedMultiplier = 1.0f;

        public float DamageMultiplier => damageMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;
        public float MovementSpeedMultiplier => movementSpeedMultiplier;

        /// <summary>
        /// Fired whenever any runtime stat modifier changes.
        /// </summary>
        public event Action OnStatsChanged;

        private void Awake()
        {
            ValidateModifiers();
        }

        private void OnValidate()
        {
            ValidateModifiers();
        }

        private void ValidateModifiers()
        {
            if (damageMultiplier < 0f) damageMultiplier = 0f;
            if (attackSpeedMultiplier < 0.01f) attackSpeedMultiplier = 0.01f;
            if (movementSpeedMultiplier < 0f) movementSpeedMultiplier = 0f;
        }

        /// <summary>
        /// Adds an additive bonus to the damage multiplier (e.g. 0.20 for +20%).
        /// </summary>
        /// <param name="percentage">Additive percentage to increase (e.g. 0.20f).</param>
        public void AddDamageBonus(float percentage)
        {
            if (percentage <= 0f) return;
            damageMultiplier += percentage;
            OnStatsChanged?.Invoke();
        }

        /// <summary>
        /// Adds an additive bonus to the attack speed multiplier (e.g. 0.15 for +15%).
        /// </summary>
        /// <param name="percentage">Additive percentage to increase (e.g. 0.15f).</param>
        public void AddAttackSpeedBonus(float percentage)
        {
            if (percentage <= 0f) return;
            attackSpeedMultiplier += percentage;
            OnStatsChanged?.Invoke();
        }

        /// <summary>
        /// Adds an additive bonus to the movement speed multiplier (e.g. 0.10 for +10%).
        /// </summary>
        /// <param name="percentage">Additive percentage to increase (e.g. 0.10f).</param>
        public void AddMovementSpeedBonus(float percentage)
        {
            if (percentage <= 0f) return;
            movementSpeedMultiplier += percentage;
            OnStatsChanged?.Invoke();
        }

        /// <summary>
        /// Resets all modifiers back to neutral defaults (1.0).
        /// </summary>
        public void ResetModifiers()
        {
            damageMultiplier = 1.0f;
            attackSpeedMultiplier = 1.0f;
            movementSpeedMultiplier = 1.0f;
            OnStatsChanged?.Invoke();
        }
    }
}
