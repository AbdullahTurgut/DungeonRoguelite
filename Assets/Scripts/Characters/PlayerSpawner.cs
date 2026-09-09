using System;
using UnityEngine;

namespace DungeonRoguelite.Characters
{
    /// <summary>
    /// Instantiates the active playable character archetype at runtime at an explicit spawn point.
    /// Manages the ActiveCharacter reference and notifies scene systems via OnPlayerSpawned.
    /// Strictly maintains single responsibility over spawning; contains no waves, combat, XP, UI, or camera logic.
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Default character definition to spawn if none is explicitly specified.")]
        [SerializeField] private CharacterDefinition defaultCharacter;

        [Tooltip("Transform determining the spawn position and rotation. Must be assigned.")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("If true, automatically spawns the default character during Awake.")]
        [SerializeField] private bool autoSpawn = true;

        private PlayableCharacter activeCharacter;

        public PlayableCharacter ActiveCharacter => activeCharacter;
        public CharacterDefinition DefaultCharacter => defaultCharacter;
        public Transform SpawnPoint => spawnPoint;
        public bool AutoSpawn => autoSpawn;

        /// <summary>
        /// Fired immediately after the playable character is instantiated and registered.
        /// Passes the newly spawned PlayableCharacter instance.
        /// </summary>
        public event Action<PlayableCharacter> OnPlayerSpawned;

        private void Awake()
        {
            if (autoSpawn && activeCharacter == null)
            {
                CharacterDefinition characterToSpawn = (CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null)
                    ? CharacterSelectionSession.SelectedCharacter
                    : defaultCharacter;

                Spawn(characterToSpawn);
            }
        }

        /// <summary>
        /// Instantiates a playable character from the provided, session, or default CharacterDefinition.
        /// Strictly validates configuration and guards against duplicate instantiation.
        /// </summary>
        /// <param name="definition">Optional CharacterDefinition to spawn. Falls back to session or defaultCharacter if null.</param>
        /// <returns>The spawned PlayableCharacter instance, or null if validation fails.</returns>
        public PlayableCharacter Spawn(CharacterDefinition definition = null)
        {
            if (activeCharacter != null)
            {
                Debug.LogWarning("[PlayerSpawner] Cannot spawn: ActiveCharacter already exists.");
                return activeCharacter;
            }

            CharacterDefinition defToSpawn = definition;
            if (defToSpawn == null && CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null)
            {
                defToSpawn = CharacterSelectionSession.SelectedCharacter;
            }
            if (defToSpawn == null && !string.IsNullOrEmpty(CharacterSelectionSession.SelectedCharacterId) &&
                defaultCharacter != null && string.Equals(defaultCharacter.Id, CharacterSelectionSession.SelectedCharacterId, StringComparison.OrdinalIgnoreCase))
            {
                defToSpawn = defaultCharacter;
            }
            if (defToSpawn == null && DungeonRoguelite.Progression.RunProgressionSession.HasActiveRun &&
                !string.IsNullOrEmpty(DungeonRoguelite.Progression.RunProgressionSession.OwnerCharacterId) &&
                defaultCharacter != null && string.Equals(defaultCharacter.Id, DungeonRoguelite.Progression.RunProgressionSession.OwnerCharacterId, StringComparison.OrdinalIgnoreCase))
            {
                defToSpawn = defaultCharacter;
            }
            if (defToSpawn == null)
            {
                defToSpawn = defaultCharacter;
            }

            if (defToSpawn == null)
            {
                Debug.LogError("[PlayerSpawner] Cannot spawn: No CharacterDefinition specified or configured.");
                return null;
            }

            if (defToSpawn.CharacterPrefab == null)
            {
                Debug.LogError($"[PlayerSpawner] Cannot spawn: CharacterDefinition '{defToSpawn.name}' has no characterPrefab assigned.");
                return null;
            }

            if (spawnPoint == null)
            {
                Debug.LogError("[PlayerSpawner] Cannot spawn: SpawnPoint transform is missing or unassigned.");
                return null;
            }

            GameObject instance = Instantiate(defToSpawn.CharacterPrefab, spawnPoint.position, spawnPoint.rotation);
            PlayableCharacter playable = instance.GetComponent<PlayableCharacter>();
            if (playable == null)
            {
                Debug.LogError($"[PlayerSpawner] Spawned prefab '{defToSpawn.CharacterPrefab.name}' is missing PlayableCharacter component.");
                Destroy(instance);
                return null;
            }

            playable.SetCharacterDefinition(defToSpawn);
            activeCharacter = playable;

            var exp = playable.GetComponent<DungeonRoguelite.Experience.PlayerExperience>();
            var stats = playable.GetComponent<DungeonRoguelite.Player.PlayerStats>();
            var upgradeMgr = UnityEngine.Object.FindFirstObjectByType<DungeonRoguelite.Upgrades.UpgradeManager>();

            // Explicitly bind UpgradeManager to player components FIRST so references are never null during reconstruction
            if (upgradeMgr != null && (exp != null || stats != null))
            {
                upgradeMgr.BindPlayer(exp, stats);
            }

            // Apply permanent progression modifiers if character has a skill tree definition
            var skillTree = defToSpawn.SkillTree;
#if UNITY_EDITOR
            if (skillTree == null && !string.IsNullOrEmpty(defToSpawn.Id))
            {
                string treePath = $"Assets/ScriptableObjects/Progression/SkillTree_{char.ToUpper(defToSpawn.Id[0]) + defToSpawn.Id.Substring(1)}.asset";
                skillTree = UnityEditor.AssetDatabase.LoadAssetAtPath<DungeonRoguelite.Progression.SkillTreeDefinition>(treePath);
            }
#endif
            var permanentModifiers = skillTree != null
                ? DungeonRoguelite.Progression.PermanentProgression.GetPermanentModifiers(defToSpawn.Id, skillTree)
                : DungeonRoguelite.Progression.PermanentStatModifiers.Default;

            if (stats != null)
            {
                stats.SetPermanentMultipliers(
                    permanentModifiers.damageMultiplier,
                    permanentModifiers.attackSpeedMultiplier,
                    permanentModifiers.movementSpeedMultiplier,
                    permanentModifiers.maxHealthMultiplier);
            }

            var health = playable.GetComponent<DungeonRoguelite.Player.PlayerHealth>();
            if (health != null)
            {
                health.ApplyPermanentHealthMultiplier(permanentModifiers.maxHealthMultiplier);
            }

            // Restore campaign run progression if an active run exists for this character
            if (DungeonRoguelite.Progression.RunProgressionSession.HasActiveRun &&
                DungeonRoguelite.Progression.RunProgressionSession.ValidateOwner(defToSpawn.Id))
            {
                if (exp != null)
                {
                    exp.RestoreState(
                        DungeonRoguelite.Progression.RunProgressionSession.Level,
                        DungeonRoguelite.Progression.RunProgressionSession.CurrentXP,
                        DungeonRoguelite.Progression.RunProgressionSession.TotalXP);
                }

                if (upgradeMgr != null)
                {
                    upgradeMgr.ReconstructUpgrades(DungeonRoguelite.Progression.RunProgressionSession.CommittedUpgradeIds);
                }

                // Capture fresh entry checkpoint for this dungeon attempt (used for retry rollback)
                DungeonRoguelite.Progression.RunProgressionSession.CreateDungeonCheckpoint();
            }
            else
            {
                // Fallback / fresh run: initialize clean run state for this character
                DungeonRoguelite.Progression.RunProgressionSession.StartNewRun(defToSpawn.Id);
            }

            OnPlayerSpawned?.Invoke(activeCharacter);
            return activeCharacter;
        }

        /// <summary>
        /// Configures the spawn point transform.
        /// </summary>
        public void SetSpawnPoint(Transform newSpawnPoint)
        {
            spawnPoint = newSpawnPoint;
        }

        /// <summary>
        /// Configures the default character definition.
        /// </summary>
        public void SetDefaultCharacter(CharacterDefinition character)
        {
            defaultCharacter = character;
        }
    }
}
