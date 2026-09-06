namespace DungeonRoguelite.Weapons
{
    /// <summary>
    /// Minimal contract for player primary attack execution.
    /// Decouples input coordination in PlayerAttack from concrete attack mechanics.
    /// </summary>
    public interface IPrimaryAttack
    {
        /// <summary>
        /// Attempts to execute an attack. Respects cooldowns and pause state.
        /// </summary>
        /// <returns>True if the attack was successfully executed; false if blocked by cooldown or pause.</returns>
        bool TryAttack();
    }
}
