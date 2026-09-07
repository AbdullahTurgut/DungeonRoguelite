using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRoguelite.Characters
{
    /// <summary>
    /// Data catalog ScriptableObject defining the ordered list of playable character archetypes.
    /// Used by CharacterSelection UI to populate character cards dynamically.
    /// Strictly maintains data catalog responsibility; contains zero stats, zero progression,
    /// zero save data, zero selection state, and zero combat logic.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterRoster", menuName = "Dungeon Roguelite/Character Roster")]
    public class CharacterRoster : ScriptableObject
    {
        [Tooltip("Ordered list of playable CharacterDefinitions available in the roster.")]
        [SerializeField] private List<CharacterDefinition> characters = new List<CharacterDefinition>();

        /// <summary>
        /// Read-only view of the configured characters in their display order.
        /// </summary>
        public IReadOnlyList<CharacterDefinition> Characters => characters;

        /// <summary>
        /// Total number of characters configured in the roster.
        /// </summary>
        public int Count => characters != null ? characters.Count : 0;

        /// <summary>
        /// Indexer accessing a CharacterDefinition by its roster position.
        /// </summary>
        public CharacterDefinition this[int index] => characters != null ? characters[index] : null;

        /// <summary>
        /// Validates that the roster contains non-null entries, no duplicate object references,
        /// and no duplicate character IDs.
        /// </summary>
        /// <param name="errorMessage">Output error message if validation fails.</param>
        /// <returns>True if the roster is valid, false otherwise.</returns>
        public bool ValidateRoster(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (characters == null || characters.Count == 0)
            {
                errorMessage = "Roster contains zero characters.";
                return false;
            }

            var seenRefs = new HashSet<CharacterDefinition>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterDefinition def = characters[i];
                if (def == null)
                {
                    errorMessage = $"Roster contains a null CharacterDefinition at index {i}.";
                    return false;
                }

                if (!seenRefs.Add(def))
                {
                    errorMessage = $"Roster contains duplicate reference to '{def.name}' at index {i}.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(def.Id))
                {
                    errorMessage = $"CharacterDefinition '{def.name}' at index {i} has an empty or null Id.";
                    return false;
                }

                if (!seenIds.Add(def.Id))
                {
                    errorMessage = $"Roster contains duplicate character Id '{def.Id}' on '{def.name}' at index {i}.";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Programmatically configures the characters list (used by setup and verification scripts).
        /// </summary>
        public void SetCharacters(IEnumerable<CharacterDefinition> characterList)
        {
            characters = characterList != null ? new List<CharacterDefinition>(characterList) : new List<CharacterDefinition>();
        }
    }
}
