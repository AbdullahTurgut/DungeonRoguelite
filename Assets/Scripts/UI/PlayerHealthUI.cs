using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Event-driven UI controller for the player health bar and numerical health text.
    /// Strictly listens to PlayerHealth.OnHealthChanged events; contains zero Update() polling.
    /// Supports explicit runtime binding via PlayerSpawner.OnPlayerSpawned.
    /// </summary>
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("Target References")]
        [Tooltip("The PlayerHealth component to observe. Auto-resolved via spawner if unassigned.")]
        [SerializeField] private PlayerHealth playerHealth;

        [Tooltip("Optional reference to PlayerSpawner for explicit runtime binding.")]
        [SerializeField] private PlayerSpawner playerSpawner;

        [Header("UI Components")]
        [Tooltip("Slider displaying the normalized health percentage.")]
        [SerializeField] private Slider healthSlider;

        [Tooltip("Text label displaying current and maximum health values (e.g. '100 / 100').")]
        [SerializeField] private TextMeshProUGUI healthText;

        [Tooltip("Optional label displaying 'CAN'.")]
        [SerializeField] private TextMeshProUGUI labelText;

        public PlayerHealth PlayerHealth => playerHealth;
        public PlayerSpawner PlayerSpawner => playerSpawner;
        public Slider HealthSlider => healthSlider;
        public TextMeshProUGUI HealthText => healthText;
        public TextMeshProUGUI LabelText => labelText;

        private void Awake()
        {
            if (playerSpawner == null)
            {
                ResolvePlayerHealth();
            }
        }

        private void OnEnable()
        {
            if (playerSpawner != null)
            {
                playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
                playerSpawner.OnPlayerSpawned += HandlePlayerSpawned;

                if (playerSpawner.ActiveCharacter != null)
                {
                    HandlePlayerSpawned(playerSpawner.ActiveCharacter);
                }
            }
            else
            {
                ResolvePlayerHealth();

                if (playerHealth != null)
                {
                    playerHealth.OnHealthChanged -= HandleHealthChanged;
                    playerHealth.OnHealthChanged += HandleHealthChanged;

                    RefreshUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                }
            }
        }

        private void OnDisable()
        {
            if (playerSpawner != null)
            {
                playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void ResolvePlayerHealth()
        {
            if (playerHealth == null)
            {
                playerHealth = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
            }
        }

        private void HandlePlayerSpawned(PlayableCharacter character)
        {
            if (character != null)
            {
                var health = character.GetComponent<PlayerHealth>();
                Bind(health);
            }
        }

        /// <summary>
        /// Explicitly binds a PlayerHealth component to this UI view.
        /// </summary>
        public void Bind(PlayerHealth health)
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }

            playerHealth = health;

            if (isActiveAndEnabled && playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
                RefreshUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        /// <summary>
        /// Configures the PlayerSpawner reference for runtime binding.
        /// </summary>
        public void SetPlayerSpawner(PlayerSpawner spawner)
        {
            if (playerSpawner != null)
            {
                playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
            }

            playerSpawner = spawner;

            if (isActiveAndEnabled && playerSpawner != null)
            {
                playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
                playerSpawner.OnPlayerSpawned += HandlePlayerSpawned;

                if (playerSpawner.ActiveCharacter != null)
                {
                    HandlePlayerSpawned(playerSpawner.ActiveCharacter);
                }
            }
        }

        public void SetReferences(PlayerHealth health, PlayerSpawner spawner, Slider slider, TextMeshProUGUI text, TextMeshProUGUI label = null)
        {
            playerHealth = health;
            playerSpawner = spawner;
            healthSlider = slider;
            healthText = text;
            labelText = label;
        }

        private void HandleHealthChanged(float current, float max)
        {
            RefreshUI(current, max);
        }

        /// <summary>
        /// Updates the visual elements based on current and max health.
        /// Zero allocation or polling; called strictly on health events.
        /// </summary>
        public void RefreshUI(float current, float max)
        {
            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                healthSlider.value = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            }

            if (healthText != null)
            {
                int currentInt = Mathf.CeilToInt(Mathf.Max(0f, current));
                int maxInt = Mathf.CeilToInt(max);
                healthText.text = $"{currentInt} / {maxInt}";
            }

            if (labelText != null && string.IsNullOrEmpty(labelText.text))
            {
                labelText.text = "CAN";
            }
        }
    }
}
