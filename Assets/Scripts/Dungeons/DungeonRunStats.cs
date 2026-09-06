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
            StartTracking();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (waveManager != null)
            {
                waveManager.OnEnemyDefeated -= HandleEnemyDefeated;
                waveManager.OnEnemyDefeated += HandleEnemyDefeated;
            }
        }

        private void OnDisable()
        {
            if (waveManager != null)
            {
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

        private void HandleEnemyDefeated(EnemyHealth enemy)
        {
            if (isTracking)
            {
                enemiesDefeated++;
            }
        }

        /// <summary>
        /// Builds and returns an immutable summary snapshot of the current run.
        /// </summary>
        public DungeonRunSummary BuildSummary()
        {
            if (playerExperience == null)
            {
                ResolveReferences();
            }

            int finalLevel = playerExperience != null ? playerExperience.Level : 1;
            int totalXP = playerExperience != null ? playerExperience.TotalXPEarned : 0;
            float runTime = ElapsedTime;

            return new DungeonRunSummary(runTime, enemiesDefeated, finalLevel, totalXP);
        }

        public void SetReferences(WaveManager wave, PlayerExperience exp)
        {
            waveManager = wave;
            playerExperience = exp;
        }
    }
}
