using System;
using UnityEngine;
namespace DungeonRoguelite.Weapons
{
    [CreateAssetMenu(menuName = "Dungeon Roguelite/Weapon Catalog")]
    public sealed class WeaponCatalog : ScriptableObject
    {
        [SerializeField] private WeaponDefinition[] weapons = Array.Empty<WeaponDefinition>();
        public System.Collections.Generic.IReadOnlyList<WeaponDefinition> Weapons => weapons;
        public WeaponDefinition Find(string id) => Array.Find(weapons, w => w != null && string.Equals(w.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
