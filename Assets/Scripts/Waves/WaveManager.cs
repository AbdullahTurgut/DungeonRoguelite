using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.Waves
{
    /// <summary>
    /// Lifecycle states for the wave progression system.
    /// </summary>
    public enum WaveState
    {
        NotStarted,
        Spawning,
        WaveActive,
        WaveCompleted,
        DungeonCompleted,
        Inactive
    }

    /// <summary>
    /// Orchestrates data-driven sequential enemy wave spawning, living enemy tracking,
    /// and dungeon completion notifications for the dungeon.
    /// Strictly maintains single responsibility over wave progression and spawning.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Configuration")]
        [Tooltip("Ordered list of wave definitions to spawn sequentially.")]
        [SerializeField] private WaveDefinition[] waves;

        [Header("Scene References")]
        [Tooltip("Target transform that spawned enemies will pursue and attack (defaults to tag 'Player').")]
        [SerializeField] private Transform playerTarget;

        [Tooltip("Optional reference to the PlayerSpawner for explicit runtime binding.")]
        [SerializeField] private PlayerSpawner playerSpawner;

        [Tooltip("Spawn point transforms cycled in round-robin order.")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("Pacing")]
        [Tooltip("Delay in seconds between clearing a wave and beginning the next wave.")]
        [SerializeField] private float waveTransitionDelay = 0.5f;

        [Tooltip("Whether to automatically begin Wave 1 on Start.")]
        [SerializeField] private bool autoStart = true;

        // Authoritative collections & mappings
        private readonly HashSet<EnemyHealth> activeEnemies = new HashSet<EnemyHealth>();
        private readonly List<EnemyHealth> currentWaveSpawned = new List<EnemyHealth>();
        private readonly Dictionary<EnemyHealth, Action> deathCallbacks = new Dictionary<EnemyHealth, Action>();

        // Runtime state
        private WaveState currentState = WaveState.NotStarted;
        private int currentWaveIndex = 0;
        private int nextSpawnPointIndex = 0;
        private bool isSpawning = false;
        private bool hasCompletedDungeon = false;
        private bool hasDungeonStarted = false;
        private DungeonDefinition activeDungeon;

        private Coroutine activeWaveCoroutine;
        private Coroutine transitionCoroutine;

        #region Public Properties

        public WaveState CurrentState => currentState;
        public int CurrentWaveIndex => currentWaveIndex;
        public int CurrentWaveNumber => currentWaveIndex + 1;
        public int TotalWaves => waves != null ? waves.Length : 0;
        public int LivingEnemyCount => activeEnemies.Count;
        public bool IsSpawning => isSpawning;
        public Transform PlayerTarget => playerTarget;
        public PlayerSpawner PlayerSpawner => playerSpawner;
        public bool HasDungeonStarted => hasDungeonStarted;
        public DungeonDefinition ActiveDungeon => activeDungeon;
        public IReadOnlyCollection<EnemyHealth> ActiveEnemies => activeEnemies;
        public IReadOnlyList<EnemyHealth> CurrentWaveSpawned => currentWaveSpawned;

        #endregion

        #region Public Events

        /// <summary>
        /// Fired exactly once when the dungeon run officially begins.
        /// </summary>
        public event Action OnDungeonStarted;

        /// <summary>
        /// Fired when a wave begins spawning. Passes (currentWaveNumber, totalWaves).
        /// </summary>
        public event Action<int, int> OnWaveStarted;

        /// <summary>
        /// Fired when all enemies in a wave have spawned and died. Passes (completedWaveNumber, totalWaves).
        /// </summary>
        public event Action<int, int> OnWaveCompleted;

        /// <summary>
        /// Fired exactly once when an actively tracked wave enemy is defeated. Passes (enemy).
        /// </summary>
        public event Action<EnemyHealth> OnEnemyDefeated;

        /// <summary>
        /// Fired exactly once after the final wave is cleared.
        /// </summary>
        public event Action OnDungeonCompleted;

        #endregion

        private void Awake()
        {
            InitializeDungeonConfiguration();

            if (playerSpawner == null)
            {
                ResolvePlayerTarget();
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
        }

        private void Start()
        {
            if (autoStart && !hasDungeonStarted)
            {
                if (playerTarget != null)
                {
                    BeginDungeon();
                }
                else if (playerSpawner == null)
                {
                    ResolvePlayerTarget();
                    if (playerTarget != null)
                    {
                        BeginDungeon();
                    }
                    else
                    {
                        Debug.LogWarning("[WaveManager] AutoStart failed: Player target is not assigned and could not be resolved.");
                    }
                }
                else
                {
                    Debug.LogWarning("[WaveManager] AutoStart deferred: Player target is not yet bound from PlayerSpawner.");
                }
            }
        }

        private void OnDisable()
        {
            if (playerSpawner != null)
            {
                playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
            }

            CleanupSubscriptionsAndRoutines();
        }

        private void OnDestroy()
        {
            CleanupSubscriptionsAndRoutines();
        }

        private void ResolvePlayerTarget()
        {
            if (playerTarget == null)
            {
                var playerGo = GameObject.FindWithTag("Player");
                if (playerGo != null)
                {
                    playerTarget = playerGo.transform;
                }
            }
        }

        /// <summary>
        /// Sets a custom array of wave definitions.
        /// </summary>
        public void SetWaves(WaveDefinition[] newWaves)
        {
            waves = newWaves;
        }

        /// <summary>
        /// Sets the spawn points used for round-robin instantiation.
        /// </summary>
        public void SetSpawnPoints(Transform[] newSpawnPoints)
        {
            spawnPoints = newSpawnPoints;
            nextSpawnPointIndex = 0;
        }

        /// <summary>
        /// Explicitly sets the target transform for spawned enemies.
        /// </summary>
        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        /// <summary>
        /// Explicitly binds the player target transform for spawned enemies.
        /// Strictly updates reference only; does not start waves or initiate gameplay.
        /// </summary>
        public void BindPlayer(Transform target)
        {
            SetPlayerTarget(target);
        }

        /// <summary>
        /// Starts the dungeon run and begins Wave 1.
        /// Guaranteed single-fire; requires a valid player target.
        /// </summary>
        public void BeginDungeon()
        {
            if (hasDungeonStarted)
            {
                return;
            }

            if (playerTarget == null)
            {
                Debug.LogError("[WaveManager] Cannot begin dungeon: Player target is null.");
                return;
            }

            hasDungeonStarted = true;
            OnDungeonStarted?.Invoke();
            StartWaves();
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
                BindPlayer(character.transform);
            }
        }

        /// <summary>
        /// Starts wave progression from the first configured wave.
        /// </summary>
        public void StartWaves()
        {
            if (waves == null || waves.Length == 0)
            {
                Debug.LogWarning("[WaveManager] No wave definitions configured. Cannot start waves.");
                return;
            }

            ResolvePlayerTarget();
            CleanupSubscriptionsAndRoutines();

            currentWaveIndex = 0;
            nextSpawnPointIndex = 0;
            hasCompletedDungeon = false;

            StartWave(currentWaveIndex);
        }

        /// <summary>
        /// Halts current spawning, unsubscribes all active callbacks, and resets state.
        /// </summary>
        public void StopWaves()
        {
            CleanupSubscriptionsAndRoutines();
            currentState = WaveState.NotStarted;
            hasDungeonStarted = false;
        }

        private void StartWave(int waveIndex)
        {
            if (waveIndex < 0 || waveIndex >= waves.Length)
            {
                return;
            }

            WaveDefinition waveDef = waves[waveIndex];
            if (waveDef == null)
            {
                Debug.LogError($"[WaveManager] WaveDefinition at index {waveIndex} is null.");
                return;
            }

            currentWaveSpawned.Clear();
            activeWaveCoroutine = StartCoroutine(SpawnWaveRoutine(waveDef));
        }

        private IEnumerator SpawnWaveRoutine(WaveDefinition waveDef)
        {
            currentState = WaveState.Spawning;
            isSpawning = true;
            OnWaveStarted?.Invoke(CurrentWaveNumber, TotalWaves);

            float interval = Mathf.Max(0.001f, waveDef.SpawnInterval);
            var entries = waveDef.EnemyEntries;

            if (entries != null)
            {
                for (int e = 0; e < entries.Count; e++)
                {
                    var entry = entries[e];
                    if (entry.EnemyPrefab == null || entry.Count <= 0)
                    {
                        continue;
                    }

                    for (int c = 0; c < entry.Count; c++)
                    {
                        SpawnEnemy(entry.EnemyPrefab);

                        if (interval > 0f)
                        {
                            yield return new WaitForSeconds(interval);
                        }
                    }
                }
            }

            isSpawning = false;

            if (activeEnemies.Count > 0)
            {
                currentState = WaveState.WaveActive;
            }
            else
            {
                CheckWaveProgression();
            }
        }

        private void SpawnEnemy(GameObject enemyPrefab)
        {
            Transform spawnPoint = GetNextSpawnPoint();
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, spawnRot);

            // Apply active dungeon scaling if configured
            float healthMult = 1f;
            float damageMult = 1f;
            if (activeDungeon != null)
            {
                healthMult = activeDungeon.EnemyHealthMultiplier;
                damageMult = activeDungeon.EnemyDamageMultiplier;
            }
            else if (DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon != null)
            {
                healthMult = DungeonRunSession.SelectedDungeon.EnemyHealthMultiplier;
                damageMult = DungeonRunSession.SelectedDungeon.EnemyDamageMultiplier;
            }

            if (healthMult != 1f && enemyInstance.TryGetComponent<EnemyHealth>(out var enemyHealthComp))
            {
                enemyHealthComp.InitializeHealth(healthMult);
            }

            var attack = enemyInstance.GetComponent<IEnemyAttack>();
            if (damageMult != 1f && attack != null)
            {
                attack.InitializeAttack(damageMult);
            }

            // Assign target explicitly to prevent per-frame scene searching
            if (playerTarget != null)
            {
                var movement = enemyInstance.GetComponent<EnemyMovement>();
                if (movement != null)
                {
                    movement.SetTarget(playerTarget);
                }

                if (attack != null)
                {
                    attack.SetTarget(playerTarget);
                }
            }

            // Register and track authoritative living status
            var health = enemyInstance.GetComponent<EnemyHealth>();
            if (health != null)
            {
                currentWaveSpawned.Add(health);
                activeEnemies.Add(health);

                // Create and store exact delegate for clean unsubscription
                Action deathHandler = () => HandleEnemyDied(health);
                deathCallbacks[health] = deathHandler;
                health.OnDied += deathHandler;
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Spawned enemy '{enemyInstance.name}' does not have an EnemyHealth component.");
            }
        }

        private Transform GetNextSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return transform;
            }

            Transform selected = spawnPoints[nextSpawnPointIndex];
            nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;
            return selected != null ? selected : transform;
        }

        private void HandleEnemyDied(EnemyHealth enemy)
        {
            if (enemy == null)
            {
                return;
            }

            // Process death only if enemy is actively tracked in this wave
            if (activeEnemies.Remove(enemy))
            {
                if (deathCallbacks.TryGetValue(enemy, out var handler))
                {
                    enemy.OnDied -= handler;
                    deathCallbacks.Remove(enemy);
                }

                OnEnemyDefeated?.Invoke(enemy);
                CheckWaveProgression();
            }
        }

        private void CheckWaveProgression()
        {
            // Transition only when:
            // 1. All enemies for the wave have finished spawning
            // 2. Every tracked enemy is dead
            if (isSpawning || activeEnemies.Count > 0)
            {
                return;
            }

            if (currentState == WaveState.Spawning || currentState == WaveState.WaveActive)
            {
                currentState = WaveState.WaveCompleted;
                OnWaveCompleted?.Invoke(CurrentWaveNumber, TotalWaves);

                if (currentWaveIndex + 1 < TotalWaves)
                {
                    transitionCoroutine = StartCoroutine(TransitionToNextWaveRoutine());
                }
                else
                {
                    TriggerDungeonCompleted();
                }
            }
        }

        private IEnumerator TransitionToNextWaveRoutine()
        {
            if (waveTransitionDelay > 0f)
            {
                yield return new WaitForSeconds(waveTransitionDelay);
            }

            currentWaveIndex++;
            StartWave(currentWaveIndex);
        }

        private void TriggerDungeonCompleted()
        {
            if (hasCompletedDungeon)
            {
                return;
            }

            hasCompletedDungeon = true;
            currentState = WaveState.DungeonCompleted;
            OnDungeonCompleted?.Invoke();
        }

        /// <summary>
        /// Configures waves dynamically from a DungeonDefinition.
        /// Creates a runtime array copy to prevent asset mutation.
        /// Must be called before BeginDungeon().
        /// <summary>
        /// Configures wave definitions dynamically from a data-driven DungeonDefinition.
        /// Performs a shallow copy of the waves array to protect the source ScriptableObject from runtime mutation.
        /// </summary>
        /// <param name="dungeon">The DungeonDefinition to load waves from.</param>
        public void ConfigureFromDungeon(DungeonDefinition dungeon)
        {
            if (dungeon == null)
            {
                return;
            }

            activeDungeon = dungeon;
            if (dungeon.Waves != null && dungeon.Waves.Length > 0)
            {
                WaveDefinition[] runtimeWaves = new WaveDefinition[dungeon.Waves.Length];
                Array.Copy(dungeon.Waves, runtimeWaves, dungeon.Waves.Length);
                waves = runtimeWaves;
            }
        }

        private void InitializeDungeonConfiguration()
        {
            if (DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon != null)
            {
                ConfigureFromDungeon(DungeonRunSession.SelectedDungeon);
            }
        }

        /// <summary>
        /// Immediately halts wave progression, stops active spawning and transition coroutines,
        /// and sets the state to Inactive. Used upon player defeat to prevent further wave events.
        /// </summary>
        public void HaltDungeon()
        {
            CleanupSubscriptionsAndRoutines();
            currentState = WaveState.Inactive;
        }

        private void CleanupSubscriptionsAndRoutines()
        {
            if (activeWaveCoroutine != null)
            {
                StopCoroutine(activeWaveCoroutine);
                activeWaveCoroutine = null;
            }

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            foreach (var kvp in deathCallbacks)
            {
                if (kvp.Key != null && kvp.Value != null)
                {
                    kvp.Key.OnDied -= kvp.Value;
                }
            }

            deathCallbacks.Clear();
            activeEnemies.Clear();
            currentWaveSpawned.Clear();
            isSpawning = false;
        }
    }
}
