using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Characters;

namespace DungeonRoguelite.Dungeons
{
    /// <summary>
    /// Orchestrates dungeon completion lifecycle, final XP pickup resolution,
    /// upgrade selection sequencing, pause state ownership, and run summary dispatch.
    /// Dedicated strictly to completion orchestration; contains no UI or stat calculations.
    /// Supports explicit runtime binding via PlayerSpawner.OnPlayerSpawned.
    /// </summary>
    public class DungeonCompletionController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private DungeonRunStats runStats;
        [SerializeField] private PlayerExperience playerExperience;

        [Tooltip("Optional reference to the PlayerSpawner for explicit runtime binding.")]
        [SerializeField] private PlayerSpawner playerSpawner;

        private bool hasCompleted = false;
        private bool isWaitingForUpgrades = false;
        private bool isCompletionFinished = false;
        private DungeonRunSummary finalSummary;

        public bool HasCompleted => hasCompleted;
        public bool IsWaitingForUpgrades => isWaitingForUpgrades;
        public bool IsCompletionFinished => isCompletionFinished;
        public DungeonRunSummary FinalSummary => finalSummary;
        public PlayerSpawner PlayerSpawner => playerSpawner;
        public PlayerExperience PlayerExperience => playerExperience;

        /// <summary>
        /// Fired when all XP pickups and pending upgrades are resolved,
        /// passing the finalized DungeonRunSummary.
        /// </summary>
        public event Action<DungeonRunSummary> OnDungeonCompleted;

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
            }

            if (waveManager != null)
            {
                waveManager.OnDungeonCompleted -= HandleWaveManagerCompletion;
                waveManager.OnDungeonCompleted += HandleWaveManagerCompletion;
            }

            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeSelectionClosed -= HandleUpgradesClosed;
                upgradeManager.OnUpgradeSelectionClosed += HandleUpgradesClosed;
            }
        }

        private void OnDisable()
        {
            if (playerSpawner != null)
            {
                playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
            }

            if (waveManager != null)
            {
                waveManager.OnDungeonCompleted -= HandleWaveManagerCompletion;
            }

            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeSelectionClosed -= HandleUpgradesClosed;
            }
        }

        private void OnDestroy()
        {
            // Failsafe: ensure timeScale is not left at 0 if destroyed unexpectedly
            if (isCompletionFinished && Time.timeScale <= 0f)
            {
                Time.timeScale = 1f;
            }
        }

        private void ResolveReferences()
        {
            if (waveManager == null)
            {
                waveManager = FindFirstObjectByType<WaveManager>();
            }

            if (upgradeManager == null)
            {
                upgradeManager = FindFirstObjectByType<UpgradeManager>();
            }

            if (runStats == null)
            {
                runStats = FindFirstObjectByType<DungeonRunStats>();
            }

            if (playerExperience == null)
            {
                playerExperience = FindFirstObjectByType<PlayerExperience>();
            }
        }

        /// <summary>
        /// Handles the dungeon completed notification from WaveManager.
        /// Guaranteed single-fire. Resolves any remaining XP pickups and sequences upgrade UI.
        /// </summary>
        public void HandleWaveManagerCompletion()
        {
            if (hasCompleted)
            {
                return;
            }

            hasCompleted = true;

            // Freeze run timer immediately at wave completion
            if (runStats != null)
            {
                runStats.StopTracking();
            }

            // Deterministic Final-Kill XP resolution:
            // Programmatically collect any remaining ExperiencePickup instances in the scene
            ResolveRemainingExperiencePickups();

            // Sequence Upgrade Selection vs Dungeon Complete (Option B)
            if (upgradeManager != null && upgradeManager.IsSelectionActive)
            {
                isWaitingForUpgrades = true;
                // UpgradeManager owns Time.timeScale = 0f; wait for OnUpgradeSelectionClosed
            }
            else
            {
                FinalizeCompletion();
            }
        }

        /// <summary>
        /// Programmatically collects any active ExperiencePickup instances in the scene for the player.
        /// Performs a one-time lookup at dungeon completion (no per-frame polling).
        /// </summary>
        public void ResolveRemainingExperiencePickups()
        {
            if (playerExperience == null)
            {
                ResolveReferences();
            }

            if (playerExperience == null)
            {
                return;
            }

            var pickups = FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None);
            if (pickups != null && pickups.Length > 0)
            {
                for (int i = 0; i < pickups.Length; i++)
                {
                    if (pickups[i] != null && !pickups[i].IsCollected)
                    {
                        pickups[i].TryCollect(playerExperience);
                    }
                }
            }
        }

        private void HandleUpgradesClosed()
        {
            if (isWaitingForUpgrades)
            {
                isWaitingForUpgrades = false;
                FinalizeCompletion();
            }
        }

        /// <summary>
        /// Takes authoritative ownership of pause state, snapshots run summary,
        /// and emits the completion event.
        /// </summary>
        private void FinalizeCompletion()
        {
            // Authoritative pause ownership for completion screen
            Time.timeScale = 0f;
            isCompletionFinished = true;

            if (runStats != null)
            {
                finalSummary = runStats.BuildSummary(playerExperience);
            }
            else
            {
                int level = playerExperience != null ? playerExperience.Level : 1;
                int xp = playerExperience != null ? playerExperience.TotalXPEarned : 0;
                finalSummary = new DungeonRunSummary(0f, 0, level, xp);
            }

            OnDungeonCompleted?.Invoke(finalSummary);
        }

        /// <summary>
        /// Explicitly binds the runtime PlayerExperience component for completion resolution.
        /// </summary>
        public void BindPlayer(PlayerExperience experience)
        {
            playerExperience = experience;
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
                BindPlayer(exp);
            }
        }

        /// <summary>
        /// Restores normal time scale and reloads the active dungeon scene.
        /// </summary>
        public void RestartDungeon()
        {
            Time.timeScale = 1f;
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        public void SetReferences(WaveManager wave, UpgradeManager upgrade, DungeonRunStats stats, PlayerExperience exp)
        {
            waveManager = wave;
            upgradeManager = upgrade;
            runStats = stats;
            playerExperience = exp;
        }
    }
}
