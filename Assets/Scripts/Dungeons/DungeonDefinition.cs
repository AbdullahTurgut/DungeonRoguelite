using System;
using UnityEngine;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Dungeons
{
    /// <summary>
    /// Immutable data-driven configuration defining a dungeon's identity, scene,
    /// prerequisite requirement, and sequential wave composition.
    /// Contains strictly static configuration; zero runtime state, save data, or GameObjects.
    /// </summary>
    [CreateAssetMenu(fileName = "Dungeon_New", menuName = "Dungeon Roguelite/Dungeon Definition")]
    public class DungeonDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable unique string identifier (e.g. 'dungeon_1', 'dungeon_2').")]
        [SerializeField] private string id;

        [Tooltip("Player-facing localized display name.")]
        [SerializeField] private string displayName;

        [Tooltip("Player-facing brief description of the dungeon.")]
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Scene Configuration")]
        [Tooltip("The build-safe scene name to load for this dungeon.")]
        [SerializeField] private string sceneName;

        [Header("Progression Prerequisites")]
        [Tooltip("ID of the dungeon required to be completed before this dungeon unlocks (empty if unlocked by default).")]
        [SerializeField] private string requiredDungeonId;

        [Header("Wave Configuration")]
        [Tooltip("Ordered sequence of wave definitions that compose this dungeon.")]
        [SerializeField] private WaveDefinition[] waves;

        [Header("Difficulty Scaling")]
        [Tooltip("Health multiplier applied to spawned enemies in this dungeon (e.g. 1.0 = baseline, 1.1 = +10%).")]
        [SerializeField] private float enemyHealthMultiplier = 1f;

        [Tooltip("Damage multiplier applied to spawned enemies in this dungeon (e.g. 1.0 = baseline, 1.1 = +10%).")]
        [SerializeField] private float enemyDamageMultiplier = 1f;

        [Header("Classification")]
        [Tooltip("Classification of the dungeon (Normal or Boss).")]
        [SerializeField] private DungeonType dungeonType = DungeonType.Normal;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public string SceneName => sceneName;
        public string RequiredDungeonId => requiredDungeonId;
        public WaveDefinition[] Waves => waves;
        public float EnemyHealthMultiplier => enemyHealthMultiplier > 0f ? enemyHealthMultiplier : 1f;
        public float EnemyDamageMultiplier => enemyDamageMultiplier > 0f ? enemyDamageMultiplier : 1f;
        public DungeonType Type => dungeonType;

        public bool HasPrerequisite => !string.IsNullOrEmpty(requiredDungeonId);

        public void SetConfiguration(string newId, string newDisplayName, string newDescription, string newSceneName, string newRequiredDungeonId, WaveDefinition[] newWaves, float healthMultiplier = 1f, float damageMultiplier = 1f, DungeonType type = DungeonType.Normal)
        {
            id = newId;
            displayName = newDisplayName;
            description = newDescription;
            sceneName = newSceneName;
            requiredDungeonId = newRequiredDungeonId;
            waves = newWaves;
            enemyHealthMultiplier = healthMultiplier > 0f ? healthMultiplier : 1f;
            enemyDamageMultiplier = damageMultiplier > 0f ? damageMultiplier : 1f;
            dungeonType = type;
        }
    }

    /// <summary>
    /// Classification of dungeons across chapters.
    /// </summary>
    public enum DungeonType
    {
        Normal,
        Boss
    }
}
