using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRoguelite.Waves
{
    /// <summary>
    /// Configuration for a single enemy spawn entry within a wave.
    /// Supports polymorphic enemy prefabs for future enemy types without modifying wave logic.
    /// </summary>
    [Serializable]
    public struct EnemySpawnEntry
    {
        [Tooltip("Enemy prefab with an EnemyHealth component to instantiate.")]
        [SerializeField] private GameObject enemyPrefab;

        [Tooltip("Quantity of this enemy type to spawn in this wave.")]
        [SerializeField] private int count;

        public GameObject EnemyPrefab => enemyPrefab;
        public int Count => count;

        public EnemySpawnEntry(GameObject enemyPrefab, int count)
        {
            this.enemyPrefab = enemyPrefab;
            this.count = count;
        }
    }

    /// <summary>
    /// ScriptableObject defining an individual wave composition and pacing.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveDefinition", menuName = "DungeonRoguelite/Waves/Wave Definition")]
    public class WaveDefinition : ScriptableObject
    {
        [Tooltip("List of enemy entries to spawn in this wave.")]
        [SerializeField] private EnemySpawnEntry[] enemyEntries;

        [Tooltip("Interval in seconds between individual enemy spawns.")]
        [SerializeField] private float spawnInterval = 0.5f;

        public IReadOnlyList<EnemySpawnEntry> EnemyEntries => enemyEntries;
        public float SpawnInterval => spawnInterval;

        /// <summary>
        /// Total number of enemies to spawn across all entries in this wave.
        /// </summary>
        public int TotalEnemyCount
        {
            get
            {
                if (enemyEntries == null) return 0;
                int sum = 0;
                for (int i = 0; i < enemyEntries.Length; i++)
                {
                    if (enemyEntries[i].EnemyPrefab != null && enemyEntries[i].Count > 0)
                    {
                        sum += enemyEntries[i].Count;
                    }
                }
                return sum;
            }
        }

        /// <summary>
        /// Initializes the wave definition at runtime or during asset creation.
        /// </summary>
        public void Initialize(EnemySpawnEntry[] entries, float interval)
        {
            enemyEntries = entries;
            spawnInterval = interval;
        }
    }
}
