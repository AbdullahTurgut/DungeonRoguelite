using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Player;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Dungeons
{
    /// <summary>
    /// Orchestrates the player defeat lifecycle, WaveManager halting, pause state ownership,
    /// and defeat UI notification. Strictly decoupled from UI presentation.
    /// Enforces mutual exclusion with DungeonCompletionController (victory suppresses defeat, defeat suppresses victory).
    /// </summary>
    public class PlayerDefeatController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private PlayerSpawner playerSpawner;
        [SerializeField] private DungeonCompletionController completionController;

        private bool isDefeated = false;
        private PlayerHealth boundPlayerHealth;

        public bool IsDefeated => isDefeated;
        public WaveManager WaveManager => waveManager;
        public PlayerSpawner PlayerSpawner => playerSpawner;
        public DungeonCompletionController CompletionController => completionController;
        public PlayerHealth BoundPlayerHealth => boundPlayerHealth;

        /// <summary>
        /// Fired exactly once when player defeat is finalized.
        /// </summary>
        public event Action OnPlayerDefeated;

        private void Awake()
        {
            ResolveReferences();
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
                var ph = FindFirstObjectByType<PlayerHealth>();
                if (ph != null)
                {
                    BindPlayer(ph);
                }
            }
        }

        private void OnDisable()
        {
            if (playerSpawner != null)
            {
                playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
            }

            if (boundPlayerHealth != null)
            {
                boundPlayerHealth.OnDied -= HandlePlayerDied;
            }
        }

        private void OnDestroy()
        {
            // Failsafe: ensure timeScale is not left frozen if destroyed unexpectedly
            if (isDefeated && Time.timeScale <= 0f)
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

            if (playerSpawner == null)
            {
                playerSpawner = FindFirstObjectByType<PlayerSpawner>();
            }

            if (completionController == null)
            {
                completionController = FindFirstObjectByType<DungeonCompletionController>();
            }
        }

        /// <summary>
        /// Binds to the runtime PlayerHealth component of the spawned character.
        /// </summary>
        public void BindPlayer(PlayerHealth health)
        {
            if (boundPlayerHealth != null)
            {
                boundPlayerHealth.OnDied -= HandlePlayerDied;
            }

            boundPlayerHealth = health;

            if (boundPlayerHealth != null)
            {
                boundPlayerHealth.OnDied -= HandlePlayerDied;
                boundPlayerHealth.OnDied += HandlePlayerDied;
            }
        }

        private void HandlePlayerSpawned(PlayableCharacter character)
        {
            if (character != null)
            {
                var health = character.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    BindPlayer(health);
                }
            }
        }

        /// <summary>
        /// Handles player death notification.
        /// Halts wave progression, takes authoritative pause ownership, and fires defeat event.
        /// Enforces mutual exclusion: if victory already completed, defeat is suppressed.
        /// </summary>
        public void HandlePlayerDied()
        {
            if (isDefeated)
            {
                return;
            }

            // Mutual exclusion: if dungeon victory already occurred, ignore defeat
            if (completionController != null && (completionController.HasCompleted || completionController.IsCompletionFinished))
            {
                Debug.Log("[PlayerDefeatController] Defeat suppressed: Dungeon is already completed in victory.");
                return;
            }

            isDefeated = true;

            // Immediately halt wave processing and cancel active wave/transition coroutines
            if (waveManager != null)
            {
                waveManager.HaltDungeon();
            }

            // Authoritative pause ownership for defeat modal
            Time.timeScale = 0f;

            OnPlayerDefeated?.Invoke();
        }

        /// <summary>
        /// Restores timeScale and reloads current active dungeon scene.
        /// Preserves CharacterSelectionSession and rolls back RunProgressionSession to the entry checkpoint.
        /// </summary>
        public void RestartDungeon()
        {
            // Discard failed-attempt gains by reverting to the dungeon entry checkpoint
            DungeonRoguelite.Progression.RunProgressionSession.RestoreCheckpointOnRetry();

            Time.timeScale = 1f;
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        /// <summary>
        /// Restores timeScale, terminates the active campaign run, and transitions back to WorldMap.
        /// </summary>
        public void ReturnToWorldMap()
        {
            // Defeat return to map ends current campaign run
            DungeonRoguelite.Progression.RunProgressionSession.EndRun();

            Time.timeScale = 1f;
            string targetScene = Application.CanStreamedLevelBeLoaded("WorldMap") ? "WorldMap" : "CharacterSelection";
            SceneManager.LoadScene(targetScene);
        }

        /// <summary>
        /// Explicitly sets references for testing or editor setup.
        /// </summary>
        public void SetReferences(WaveManager wave, PlayerSpawner spawner, DungeonCompletionController completion)
        {
            waveManager = wave;
            playerSpawner = spawner;
            completionController = completion;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Test helper to reset defeat state during verification runs.
        /// </summary>
        public void ResetDefeatForTesting()
        {
            isDefeated = false;
        }
#endif
    }
}
