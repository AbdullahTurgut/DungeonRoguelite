namespace DungeonRoguelite.Enemies
{
    /// <summary>The minimal identity needed by the shared boss HUD. HP belongs to EnemyHealth.</summary>
    public interface IBossPresentation
    {
        string BossName { get; }
    }
}
