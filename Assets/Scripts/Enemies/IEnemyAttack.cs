using UnityEngine;

namespace DungeonRoguelite.Enemies
{
    /// <summary>Spawn-time configuration shared by enemy attack implementations.</summary>
    public interface IEnemyAttack
    {
        void InitializeAttack(float damageMultiplier);
        void SetTarget(Transform target);
    }
}
