namespace DungeonRoguelite.Combat
{
    /// <summary>
    /// Contract for any entity, character, or object capable of receiving damage.
    /// Decouples damage-dealing sources (weapons, hazards, projectiles) from specific health implementations.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies damage to this entity.
        /// </summary>
        /// <param name="amount">The damage amount to apply.</param>
        void TakeDamage(float amount);
    }
}
