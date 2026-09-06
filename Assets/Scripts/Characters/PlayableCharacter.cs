using UnityEngine;

namespace DungeonRoguelite.Characters
{
    /// <summary>
    /// Lightweight identity anchor attached to the playable character root GameObject.
    /// Exposes its CharacterDefinition and provides a stable root type for player identification and spawning.
    /// Strictly maintains single responsibility; contains no combat math, XP logic, upgrade calculations,
    /// or monolithic manager behavior.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayableCharacter : MonoBehaviour
    {
        [Header("Character Identity")]
        [Tooltip("The CharacterDefinition describing this character archetype.")]
        [SerializeField] private CharacterDefinition characterDefinition;

        /// <summary>
        /// The CharacterDefinition associated with this playable character.
        /// </summary>
        public CharacterDefinition CharacterDefinition => characterDefinition;

        /// <summary>
        /// Binds a CharacterDefinition to this playable character.
        /// </summary>
        public void SetCharacterDefinition(CharacterDefinition definition)
        {
            characterDefinition = definition;
        }
    }
}
