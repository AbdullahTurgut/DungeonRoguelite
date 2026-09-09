using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Progression;

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

        [Tooltip("Optional reference to the active PlayableCharacter.")]
        [SerializeField] private PlayableCharacter activeCharacter;

        [Tooltip("Optional reference to the PlayerDefeatController for mutual exclusion.")]
        [SerializeField] private PlayerDefeatController playerDefeatController;

        private bool hasCompleted = false;
        private bool isWaitingForUpgrades = false;
        private bool isCompletionFinished = false;
        private DungeonRunSummary finalSummary;

        public bool HasCompleted => hasCompleted;
        public bool IsWaitingForUpgrades => isWaitingForUpgrades;
        public bool IsCompletionFinished => isCompletionFinished;
        public DungeonRunSummary FinalSummary => finalSummary;
        public PlayerSpawner PlayerSpawner => playerSpawner;
        public PlayableCharacter ActiveCharacter => activeCharacter;
        public PlayerExperience PlayerExperience => playerExperience;
        public PlayerDefeatController PlayerDefeatController => playerDefeatController;

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

            if (playerDefeatController == null)
            {
                playerDefeatController = FindFirstObjectByType<PlayerDefeatController>();
            }

            if (activeCharacter == null)
            {
                activeCharacter = FindFirstObjectByType<PlayableCharacter>();
            }
        }

        /// <summary>
        /// Handles the dungeon completed notification from WaveManager.
        /// Guaranteed single-fire. Resolves any remaining XP pickups and sequences upgrade UI.
        /// Enforces mutual exclusion with PlayerDefeatController.
        /// </summary>
        public void HandleWaveManagerCompletion()
        {
            if (hasCompleted)
            {
                return;
            }

            // Mutual exclusion: if player has already been defeated, victory is suppressed
            if (playerDefeatController != null && playerDefeatController.IsDefeated)
            {
                Debug.Log("[DungeonCompletionController] Victory suppressed: Player is already defeated.");
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

            // Persist dungeon campaign progression
            string completedDungeonId = null;
            if (waveManager != null && waveManager.ActiveDungeon != null)
            {
                completedDungeonId = waveManager.ActiveDungeon.Id;
            }
            else if (DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon != null)
            {
                completedDungeonId = DungeonRunSession.SelectedDungeon.Id;
            }
            else
            {
                // Fallback for direct scene launch of Dungeon_Prototype
                completedDungeonId = "dungeon_1";
            }

            if (!string.IsNullOrEmpty(completedDungeonId))
            {
                DungeonProgression.RecordDungeonCompleted(completedDungeonId);
            }

            // Award permanent progression skill points for first-clear per character
            string completedCharacterId = null;
            if (activeCharacter != null && activeCharacter.CharacterDefinition != null && !string.IsNullOrEmpty(activeCharacter.CharacterDefinition.Id))
            {
                completedCharacterId = activeCharacter.CharacterDefinition.Id;
            }
            else if (playerSpawner != null && playerSpawner.ActiveCharacter != null && playerSpawner.ActiveCharacter.CharacterDefinition != null && !string.IsNullOrEmpty(playerSpawner.ActiveCharacter.CharacterDefinition.Id))
            {
                completedCharacterId = playerSpawner.ActiveCharacter.CharacterDefinition.Id;
            }
            else if (CharacterSelectionSession.HasSelection && !string.IsNullOrEmpty(CharacterSelectionSession.SelectedCharacterId))
            {
                completedCharacterId = CharacterSelectionSession.SelectedCharacterId;
            }
            else if (CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null && !string.IsNullOrEmpty(CharacterSelectionSession.SelectedCharacter.Id))
            {
                completedCharacterId = CharacterSelectionSession.SelectedCharacter.Id;
            }
            else if (playerSpawner != null && playerSpawner.DefaultCharacter != null && !string.IsNullOrEmpty(playerSpawner.DefaultCharacter.Id))
            {
                completedCharacterId = playerSpawner.DefaultCharacter.Id;
            }

            if (!string.IsNullOrEmpty(completedCharacterId) && !string.IsNullOrEmpty(completedDungeonId))
            {
                PermanentProgression.TryAwardDungeonFirstClear(completedCharacterId, completedDungeonId, out int awardedPoints);
                if (awardedPoints > 0)
                {
                    Debug.Log($"[DungeonCompletionController] Awarded {awardedPoints} permanent skill points to '{completedCharacterId}' for first clear of '{completedDungeonId}'.");
                }
            }
            else
            {
                Debug.LogWarning($"[DungeonCompletionController] Permanent skill points not awarded: Character='{completedCharacterId}', Dungeon='{completedDungeonId}'.");
            }

            // Ensure component references are resolved before reading final run state
            if (playerExperience == null)
            {
                playerExperience = PlayerExperience.ActiveInstance != null
                    ? PlayerExperience.ActiveInstance
                    : FindFirstObjectByType<PlayerExperience>();
            }

            if (upgradeManager == null)
            {
                upgradeManager = FindFirstObjectByType<UpgradeManager>();
            }

            // Commit final victory progression into the active campaign run session
            int finalLevel = playerExperience != null ? playerExperience.Level : 1;
            int finalCurrentXP = playerExperience != null ? playerExperience.CurrentXP : 0;
            int finalTotalXP = playerExperience != null ? playerExperience.TotalXPEarned : 0;
            var finalUpgradeIds = upgradeManager != null ? upgradeManager.CollectedUpgradeIds : null;
            DungeonRoguelite.Progression.RunProgressionSession.CommitDungeonVictory(finalLevel, finalCurrentXP, finalTotalXP, finalUpgradeIds);

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
            if (activeCharacter == null && experience != null)
            {
                activeCharacter = experience.GetComponent<PlayableCharacter>();
            }
        }

        /// <summary>
        /// Explicitly binds the runtime PlayableCharacter instance for completion resolution.
        /// </summary>
        public void BindCharacter(PlayableCharacter character)
        {
            activeCharacter = character;
            if (character != null && playerExperience == null)
            {
                var exp = character.GetComponent<PlayerExperience>();
                if (exp != null) BindPlayer(exp);
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

        private void HandlePlayerSpawned(PlayableCharacter character)
        {
            if (character != null)
            {
                activeCharacter = character;
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

        /// <summary>
        /// Restores normal time scale and transitions back to WorldMap (falling back to CharacterSelection if not in build).
        /// </summary>
        public void ReturnToWorldMap()
        {
            Time.timeScale = 1f;
            string targetScene = Application.CanStreamedLevelBeLoaded("WorldMap") ? "WorldMap" : "CharacterSelection";
            SceneManager.LoadScene(targetScene);
        }

        public void SetReferences(WaveManager wave, UpgradeManager upgrade, DungeonRunStats stats, PlayerExperience exp, PlayerDefeatController defeat = null, PlayableCharacter character = null)
        {
            waveManager = wave;
            upgradeManager = upgrade;
            runStats = stats;
            playerExperience = exp;
            if (defeat != null)
            {
                playerDefeatController = defeat;
            }
            if (character != null)
            {
                activeCharacter = character;
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Resets completion state for automated testing suites.
        /// </summary>
        public void ResetCompletionForTesting()
        {
            hasCompleted = false;
            isCompletionFinished = false;
            isWaitingForUpgrades = false;
            finalSummary = default;
            activeCharacter = null;
        }
#endif
    }
}
