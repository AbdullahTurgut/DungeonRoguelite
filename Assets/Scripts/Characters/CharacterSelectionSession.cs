using UnityEngine;

namespace DungeonRoguelite.Characters
{
    /// <summary>
    /// Lightweight static session carrier for passing the selected CharacterDefinition
    /// across scene transitions into the gameplay scene.
    /// Pure runtime state: contains no UI, no spawning, no persistence, and no save logic.
    /// Cleared on domain reload and when entering CharacterSelection scene.
    /// Survives scene reload during Dungeon Restart.
    /// </summary>
    public static class CharacterSelectionSession
    {
        private static CharacterDefinition selectedCharacter;

        /// <summary>
        /// The currently selected CharacterDefinition for the upcoming or active dungeon run.
        /// </summary>
        public static CharacterDefinition SelectedCharacter => selectedCharacter;

        /// <summary>
        /// True if a non-null CharacterDefinition is currently assigned to the session.
        /// </summary>
        public static bool HasSelection => selectedCharacter != null;

        /// <summary>
        /// Sets the active character selection for the session.
        /// </summary>
        /// <param name="character">The CharacterDefinition selected by the player.</param>
        public static void SetSelection(CharacterDefinition character)
        {
            selectedCharacter = character;
        }

        /// <summary>
        /// Clears the session selection, causing PlayerSpawner to use its configured default fallback.
        /// </summary>
        public static void Clear()
        {
            selectedCharacter = null;
        }

        /// <summary>
        /// Subsystem registration hook to ensure zero stale static state leaks
        /// across Play Mode sessions or domain reloads in the Unity Editor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticSession()
        {
            Clear();
        }
    }
}
