using UnityEngine;

namespace DungeonRoguelite.Dungeons
{
    /// <summary>
    /// Lightweight static session carrier for passing the selected DungeonDefinition
    /// across scene transitions into the dungeon gameplay scene.
    /// Pure runtime state: contains no UI, no persistence, and no save logic.
    /// Cleared on domain reload and when entering WorldMap or CharacterSelection.
    /// Survives scene reload during Dungeon Retry / Restart.
    /// </summary>
    public static class DungeonRunSession
    {
        private static DungeonDefinition selectedDungeon;

        /// <summary>
        /// The currently selected DungeonDefinition for the upcoming or active dungeon run.
        /// </summary>
        public static DungeonDefinition SelectedDungeon => selectedDungeon;

        /// <summary>
        /// True if a non-null DungeonDefinition is currently assigned to the session.
        /// </summary>
        public static bool HasSelection => selectedDungeon != null;

        /// <summary>
        /// Sets the active dungeon selection for the session.
        /// </summary>
        /// <param name="dungeon">The DungeonDefinition selected by the player.</param>
        public static void SetSelection(DungeonDefinition dungeon)
        {
            selectedDungeon = dungeon;
        }

        /// <summary>
        /// Clears the session selection, causing WaveManager to use its configured fallback waves.
        /// </summary>
        public static void Clear()
        {
            selectedDungeon = null;
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
