using System;
using System.Collections.Generic;
using UnityEngine;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Characters;

namespace DungeonRoguelite.Upgrades
{
    /// <summary>
    /// Coordinates temporary upgrade selection upon player level-up.
    /// Manages pause/resume time scale, pending multi-level-up queues, choice presentation, and stat application.
    /// Strictly translates UpgradeDefinition into PlayerStats modifications without coupling PlayerStats to Upgrades.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Target References")]
        [Tooltip("The PlayerExperience component to listen to. Auto-resolves if unassigned.")]
        [SerializeField] private PlayerExperience playerExperience;

        [Tooltip("The PlayerStats component to apply bonuses to. Auto-resolves if unassigned.")]
        [SerializeField] private PlayerStats playerStats;

        [Tooltip("Optional reference to the PlayerSpawner for explicit runtime binding.")]
        [SerializeField] private PlayerSpawner playerSpawner;

        [Header("Upgrade Pool")]
        [Tooltip("The pool of upgrades available for presentation (3 prototype upgrades).")]
        [SerializeField] private UpgradeDefinition[] availableUpgrades;

        private int pendingChoicesCount = 0;
        private bool isSelectionActive = false;
        private bool isResolvingSelection = false;
        private float previousTimeScale = 1f;
        private readonly List<string> collectedUpgradeIds = new List<string>();

        public int PendingChoicesCount => pendingChoicesCount;
        public bool IsSelectionActive => isSelectionActive;
        public IReadOnlyList<UpgradeDefinition> AvailableUpgrades => availableUpgrades;
        public IReadOnlyList<string> CollectedUpgradeIds => collectedUpgradeIds;
        public PlayerSpawner PlayerSpawner => playerSpawner;
        public PlayerExperience PlayerExperience => playerExperience;
        public PlayerStats PlayerStats => playerStats;

        /// <summary>
        /// Fired when an upgrade choice panel must be displayed with available choices.
        /// </summary>
        public event Action<UpgradeDefinition[]> OnUpgradeChoicesRequested;

        /// <summary>
        /// Fired when all pending upgrade choices are resolved and the panel should close.
        /// </summary>
        public event Action OnUpgradeSelectionClosed;

        private void Awake()
        {
            if (playerSpawner == null)
            {
                ResolveReferences();
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
                ResolveReferences();
                if (playerExperience != null)
                {
                    playerExperience.OnLevelUp -= HandleLevelUp;
                    playerExperience.OnLevelUp += HandleLevelUp;
                }
            }
        }

        private void OnDisable()
        {
            if (playerSpawner != null)
            {
                playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
            }

            if (playerExperience != null)
            {
                playerExperience.OnLevelUp -= HandleLevelUp;
            }

            // Safe restoration if disabled while selection was active
            if (isSelectionActive)
            {
                Time.timeScale = previousTimeScale;
                isSelectionActive = false;
                isResolvingSelection = false;
            }
        }

        private void OnDestroy()
        {
            if (isSelectionActive)
            {
                Time.timeScale = previousTimeScale;
            }
        }

        private void ResolveReferences()
        {
            if (playerExperience == null)
            {
                playerExperience = FindFirstObjectByType<PlayerExperience>();
            }

            if (playerStats == null)
            {
                playerStats = FindFirstObjectByType<PlayerStats>();
            }
        }

        /// <summary>
        /// Handles level-up notifications from PlayerExperience.
        /// Safely increments pendingChoicesCount and initiates selection if not already active.
        /// </summary>
        /// <param name="newLevel">The new level reached by the player.</param>
        private void HandleLevelUp(int newLevel)
        {
            pendingChoicesCount++;

            if (!isSelectionActive)
            {
                OpenSelection();
            }
        }

        private void OpenSelection()
        {
            isSelectionActive = true;
            isResolvingSelection = false;
            previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;

            OnUpgradeChoicesRequested?.Invoke(GetAvailableChoices());
        }

        /// <summary>
        /// Returns the array of available upgrade choices to present.
        /// </summary>
        public UpgradeDefinition[] GetAvailableChoices()
        {
            if (availableUpgrades == null || availableUpgrades.Length == 0)
            {
                return Array.Empty<UpgradeDefinition>();
            }

            return (UpgradeDefinition[])availableUpgrades.Clone();
        }

        /// <summary>
        /// Applies the selected upgrade and advances the pending choice queue.
        /// Guarded against rapid or duplicate calls.
        /// </summary>
        /// <param name="upgrade">The chosen UpgradeDefinition.</param>
        public void SelectUpgrade(UpgradeDefinition upgrade)
        {
            if (!isSelectionActive || isResolvingSelection || upgrade == null)
            {
                return;
            }

            isResolvingSelection = true;

            // Apply selected bonus to PlayerStats
            ApplyUpgradeToStats(upgrade);

            if (!string.IsNullOrEmpty(upgrade.Id))
            {
                collectedUpgradeIds.Add(upgrade.Id);
            }

            pendingChoicesCount--;

            if (pendingChoicesCount > 0)
            {
                // More choices remain: refresh choices and keep gameplay paused
                isResolvingSelection = false;
                OnUpgradeChoicesRequested?.Invoke(GetAvailableChoices());
            }
            else
            {
                // All pending choices resolved: resume gameplay and close panel
                pendingChoicesCount = 0;
                isSelectionActive = false;
                isResolvingSelection = false;
                Time.timeScale = previousTimeScale;
                OnUpgradeSelectionClosed?.Invoke();
            }
        }

        /// <summary>
        /// Translates UpgradeType into appropriate PlayerStats method calls.
        /// </summary>
        private void ApplyUpgradeToStats(UpgradeDefinition definition)
        {
            if (playerStats == null)
            {
                ResolveReferences();
            }

            if (playerStats == null)
            {
                Debug.LogWarning("[UpgradeManager] Cannot apply upgrade; PlayerStats component is missing.");
                return;
            }

            switch (definition.UpgradeType)
            {
                case UpgradeType.Damage:
                    playerStats.AddDamageBonus(definition.Magnitude);
                    break;
                case UpgradeType.AttackSpeed:
                    playerStats.AddAttackSpeedBonus(definition.Magnitude);
                    break;
                case UpgradeType.MovementSpeed:
                    playerStats.AddMovementSpeedBonus(definition.Magnitude);
                    break;
                default:
                    Debug.LogWarning($"[UpgradeManager] Unhandled upgrade type: {definition.UpgradeType}");
                    break;
            }
        }

        /// <summary>
        /// Reconstructs temporary PlayerStats modifiers from recorded semantic upgrade IDs.
        /// Operates without pausing gameplay or invoking upgrade selection UI.
        /// </summary>
        /// <param name="upgradeIds">Collection of stable upgrade string identifiers.</param>
        public void ReconstructUpgrades(IEnumerable<string> upgradeIds)
        {
            if (playerStats == null)
            {
                ResolveReferences();
            }

            if (playerStats == null || upgradeIds == null)
            {
                return;
            }

            collectedUpgradeIds.Clear();

            foreach (var id in upgradeIds)
            {
                if (string.IsNullOrEmpty(id)) continue;

                var def = FindUpgradeDefinitionById(id);
                if (def != null)
                {
                    ApplyUpgradeToStats(def);
                    collectedUpgradeIds.Add(def.Id);
                }
                else
                {
                    Debug.LogWarning($"[UpgradeManager] Cannot reconstruct upgrade: ID '{id}' not found in pool.");
                }
            }
        }

        /// <summary>
        /// Searches available upgrades for a matching unique ID.
        /// </summary>
        public UpgradeDefinition FindUpgradeDefinitionById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (availableUpgrades != null)
            {
                for (int i = 0; i < availableUpgrades.Length; i++)
                {
                    if (availableUpgrades[i] != null && string.Equals(availableUpgrades[i].Id, id, StringComparison.OrdinalIgnoreCase))
                    {
                        return availableUpgrades[i];
                    }
                }
            }

#if UNITY_EDITOR
            // Fallback in Editor in case instance array was not pre-populated in scene
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UpgradeDefinition");
            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var def = UnityEditor.AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
                if (def != null && string.Equals(def.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return def;
                }
            }
#endif

            return null;
        }

        /// <summary>
        /// Explicitly sets the upgrade pool. Useful for testing or setup scripts.
        /// </summary>
        public void SetAvailableUpgrades(UpgradeDefinition[] upgrades)
        {
            availableUpgrades = upgrades;
        }

        /// <summary>
        /// Explicitly binds the runtime PlayerExperience and PlayerStats components.
        /// Safely unsubscribes from previous player events before binding new ones.
        /// Does not reset or mutate active upgrade choices or time scale.
        /// </summary>
        public void BindPlayer(PlayerExperience experience, PlayerStats stats)
        {
            if (playerExperience != null)
            {
                playerExperience.OnLevelUp -= HandleLevelUp;
            }

            playerExperience = experience;
            playerStats = stats;

            if (isActiveAndEnabled && playerExperience != null)
            {
                playerExperience.OnLevelUp -= HandleLevelUp;
                playerExperience.OnLevelUp += HandleLevelUp;
            }
        }

        /// <summary>
        /// Explicitly sets the player component references. Useful for testing or setup scripts.
        /// </summary>
        public void SetPlayerReferences(PlayerExperience exp, PlayerStats stats)
        {
            BindPlayer(exp, stats);
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

        private void HandlePlayerSpawned(PlayableCharacter character)
        {
            if (character != null)
            {
                var exp = character.GetComponent<PlayerExperience>();
                var stats = character.GetComponent<PlayerStats>();
                BindPlayer(exp, stats);
            }
        }
    }
}
