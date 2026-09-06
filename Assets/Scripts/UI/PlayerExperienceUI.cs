using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Experience;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Event-driven UI controller for the player experience bar and level text.
    /// Strictly listens to PlayerExperience events; contains no Update() polling or gameplay logic.
    /// </summary>
    public class PlayerExperienceUI : MonoBehaviour
    {
        [Header("Target Experience Component")]
        [Tooltip("The PlayerExperience component to listen to. Auto-discovers if unassigned.")]
        [SerializeField] private PlayerExperience playerExperience;

        [Header("UI Visual Elements")]
        [Tooltip("Slider displaying the normalized experience progress towards the next level.")]
        [SerializeField] private Slider xpSlider;

        [Tooltip("Text label displaying current level and XP values.")]
        [SerializeField] private TextMeshProUGUI levelText;

        public PlayerExperience PlayerExperience => playerExperience;
        public Slider XPSlider => xpSlider;
        public TextMeshProUGUI LevelText => levelText;

        private void Awake()
        {
            ResolvePlayerExperience();
        }

        private void OnEnable()
        {
            ResolvePlayerExperience();

            if (playerExperience != null)
            {
                playerExperience.OnExperienceChanged -= HandleExperienceChanged;
                playerExperience.OnExperienceChanged += HandleExperienceChanged;

                playerExperience.OnLevelUp -= HandleLevelUp;
                playerExperience.OnLevelUp += HandleLevelUp;

                // Initial refresh on enable
                RefreshUI(playerExperience.CurrentXP, playerExperience.XPToNextLevel, playerExperience.Level);
            }
        }

        private void OnDisable()
        {
            if (playerExperience != null)
            {
                playerExperience.OnExperienceChanged -= HandleExperienceChanged;
                playerExperience.OnLevelUp -= HandleLevelUp;
            }
        }

        private void ResolvePlayerExperience()
        {
            if (playerExperience == null)
            {
                playerExperience = Object.FindFirstObjectByType<PlayerExperience>();
            }
        }

        private void HandleExperienceChanged(int currentXP, int xpToNextLevel)
        {
            int level = playerExperience != null ? playerExperience.Level : 1;
            RefreshUI(currentXP, xpToNextLevel, level);
        }

        private void HandleLevelUp(int newLevel)
        {
            int currentXP = playerExperience != null ? playerExperience.CurrentXP : 0;
            int xpToNextLevel = playerExperience != null ? playerExperience.XPToNextLevel : 100;
            RefreshUI(currentXP, xpToNextLevel, newLevel);
        }

        /// <summary>
        /// Updates the visual elements based on experience state.
        /// </summary>
        public void RefreshUI(int currentXP, int xpToNextLevel, int level)
        {
            if (xpSlider != null)
            {
                xpSlider.minValue = 0f;
                xpSlider.maxValue = 1f;
                xpSlider.value = xpToNextLevel > 0 ? Mathf.Clamp01((float)currentXP / xpToNextLevel) : 0f;
            }

            if (levelText != null)
            {
                levelText.text = $"Level {level} ({currentXP} / {xpToNextLevel} XP)";
            }
        }

        /// <summary>
        /// Explicitly binds a PlayerExperience component to this UI view.
        /// </summary>
        public void SetPlayerExperience(PlayerExperience target)
        {
            if (playerExperience != null)
            {
                playerExperience.OnExperienceChanged -= HandleExperienceChanged;
                playerExperience.OnLevelUp -= HandleLevelUp;
            }

            playerExperience = target;

            if (isActiveAndEnabled && playerExperience != null)
            {
                playerExperience.OnExperienceChanged += HandleExperienceChanged;
                playerExperience.OnLevelUp += HandleLevelUp;
                RefreshUI(playerExperience.CurrentXP, playerExperience.XPToNextLevel, playerExperience.Level);
            }
        }
    }
}
