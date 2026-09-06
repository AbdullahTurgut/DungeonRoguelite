using UnityEngine;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Dungeons
{
    /// <summary>
    /// Tracks active run statistics (elapsed gameplay time, enemies defeated)
    /// and packages run summary data for dungeon completion.
    /// Sourced directly from authoritative components (WaveManager, PlayerExperience).
    /// </summary>
    public class DungeonRunStats : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private PlayerExperience playerExperience;

        private float startTime;
        private float finalCompletionTime = -1f;
        private int enemiesDefeated = 0;
        private bool isTracking = false;

        public float ElapsedTime => finalCompletionTime >= 0f ? finalCompletionTime : (isTracking ? Mathf.Max(0f, Time.time - startTime) : 0f);
        public int EnemiesDefeated => enemiesDefeated;
        public bool IsTracking => isTracking;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            // Tracking is initiated by waveManager.OnDungeonStarted, not on scene start
            if (waveManager == null)
            {
                ResolveReferences();
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (waveManager != null)
            {
                waveManager.OnDungeonStarted -= HandleDungeonStarted;
                waveManager.OnDungeonStarted += HandleDungeonStarted;

                waveManager.OnEnemyDefeated -= HandleEnemyDefeated;
                waveManager.OnEnemyDefeated += HandleEnemyDefeated;

                if (waveManager.HasDungeonStarted && !isTracking && finalCompletionTime < 0f)
                {
                    StartTracking();
                }
            }
        }

        private void OnDisable()
        {
            if (waveManager != null)
            {
                waveManager.OnDungeonStarted -= HandleDungeonStarted;
                waveManager.OnEnemyDefeated -= HandleEnemyDefeated;
            }
        }

        private void ResolveReferences()
        {
            if (waveManager == null)
            {
                waveManager = FindFirstObjectByType<WaveManager>();
            }

            if (playerExperience == null)
            {
                playerExperience = FindFirstObjectByType<PlayerExperience>();
            }
        }

        /// <summary>
        /// Starts or resets tracking for a fresh run.
        /// </summary>
        public void StartTracking()
        {
            startTime = Time.time;
            finalCompletionTime = -1f;
            enemiesDefeated = 0;
            isTracking = true;
        }

        /// <summary>
        /// Freezes elapsed time at run completion.
        /// </summary>
        public void StopTracking()
        {
            if (isTracking)
            {
                finalCompletionTime = Mathf.Max(0f, Time.time - startTime);
                isTracking = false;
            }
        }

        private void HandleDungeonStarted()
        {
            StartTracking();
        }

        private void HandleEnemyDefeated(EnemyHealth enemy)
        {
            if (isTracking)
            {
                enemiesDefeated++;
            }
        }

        /// <summary>
        /// Builds and returns an immutable summary snapshot of the current run.
        /// Accepts an optional explicit PlayerExperience instance or falls back to internal reference.
        /// </summary>
        public DungeonRunSummary BuildSummary(PlayerExperience overrideExperience = null)
        {
            PlayerExperience sourceExp = overrideExperience != null ? overrideExperience : playerExperience;
            if (sourceExp == null)
            {
                ResolveReferences();
                sourceExp = playerExperience;
            }

            int finalLevel = sourceExp != null ? sourceExp.Level : 1;
            int totalXP = sourceExp != null ? sourceExp.TotalXPEarned : 0;
            float runTime = ElapsedTime;

            return new DungeonRunSummary(runTime, enemiesDefeated, finalLevel, totalXP);
        }

        public void SetPlayerExperience(PlayerExperience exp)
        {
            playerExperience = exp;
        }

        public void SetWaveManager(WaveManager wave)
        {
            if (waveManager != null)
            {
                waveManager.OnDungeonStarted -= HandleDungeonStarted;
                waveManager.OnEnemyDefeated -= HandleEnemyDefeated;
            }

            waveManager = wave;

            if (isActiveAndEnabled && waveManager != null)
            {
                waveManager.OnDungeonStarted -= HandleDungeonStarted;
                waveManager.OnDungeonStarted += HandleDungeonStarted;

                waveManager.OnEnemyDefeated -= HandleEnemyDefeated;
                waveManager.OnEnemyDefeated += HandleEnemyDefeated;

                if (waveManager.HasDungeonStarted && !isTracking && finalCompletionTime < 0f)
                {
                    StartTracking();
                }
            }
        }

        public void SetReferences(WaveManager wave, PlayerExperience exp)
        {
            SetWaveManager(wave);
            SetPlayerExperience(exp);
        }
    }
}
