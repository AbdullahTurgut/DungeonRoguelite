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

            CharacterDefinition defToSpawn = definition != null
                ? definition
                : (CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null
                    ? CharacterSelectionSession.SelectedCharacter
                    : defaultCharacter);
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
