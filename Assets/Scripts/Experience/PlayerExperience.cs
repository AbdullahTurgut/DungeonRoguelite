using System;
using UnityEngine;

namespace DungeonRoguelite.Experience
{
    /// <summary>
    /// Manages player level, experience accumulation, threshold calculations, and level-up events.
    /// Strictly maintains single responsibility over player experience; contains no UI or stat modification logic.
    /// </summary>
    public class PlayerExperience : MonoBehaviour
    {
        [Header("Progression Settings")]
        [Tooltip("XP required to advance from Level 1 to Level 2. Must be at least 1.")]
        [SerializeField] private int baseRequiredXP = 100;

        [Tooltip("Multiplicative growth rate for XP required per subsequent level.")]
        [SerializeField] private float xpGrowthMultiplier = 1.5f;

        [Header("Runtime State (Read-Only)")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentXP = 0;
        [SerializeField] private int totalXPEarned = 0;
        [SerializeField] private int xpToNextLevel;

        public int Level => currentLevel;
        public int CurrentXP => currentXP;
        public int TotalXPEarned => totalXPEarned;
        public int XPToNextLevel => xpToNextLevel;
        public float ProgressNormalized => xpToNextLevel > 0 ? Mathf.Clamp01((float)currentXP / xpToNextLevel) : 0f;

        /// <summary>
        /// Fired whenever current XP or required XP changes. Passes (currentXP, xpToNextLevel).
        /// </summary>
        public event Action<int, int> OnExperienceChanged;

        /// <summary>
        /// Fired for each level gained. Passes (newLevel).
        /// </summary>
        public event Action<int> OnLevelUp;

        private void Awake()
        {
            ValidateSettings();
            currentLevel = Mathf.Max(1, currentLevel);
            currentXP = Mathf.Max(0, currentXP);
            xpToNextLevel = CalculateRequiredXP(currentLevel);
        }

        private void OnValidate()
        {
            ValidateSettings();
            if (xpToNextLevel <= 0)
            {
                xpToNextLevel = CalculateRequiredXP(Mathf.Max(1, currentLevel));
            }
        }

        private void ValidateSettings()
        {
            if (baseRequiredXP < 1)
            {
                baseRequiredXP = 1;
            }

            if (xpGrowthMultiplier < 1.0f)
            {
                xpGrowthMultiplier = 1.0f;
            }
        }

        /// <summary>
        /// Calculates the total XP required to advance from the specified level to the next level.
        /// Formula: RoundToInt(baseRequiredXP * Pow(xpGrowthMultiplier, level - 1)).
        /// </summary>
        public int CalculateRequiredXP(int level)
        {
            if (level <= 1)
            {
                return baseRequiredXP;
            }

            return Mathf.RoundToInt(baseRequiredXP * Mathf.Pow(xpGrowthMultiplier, level - 1));
        }

        /// <summary>
        /// Adds experience to the player.
        /// Ignores non-positive values, carries excess XP forward, and processes multiple level-ups if applicable.
        /// </summary>
        /// <param name="amount">Amount of experience to add.</param>
        public void GainExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            totalXPEarned += amount;
            currentXP += amount;

            while (currentXP >= xpToNextLevel)
            {
                currentXP -= xpToNextLevel;
                currentLevel++;
                xpToNextLevel = CalculateRequiredXP(currentLevel);
                OnLevelUp?.Invoke(currentLevel);
            }

            OnExperienceChanged?.Invoke(currentXP, xpToNextLevel);
        }
    }
}
