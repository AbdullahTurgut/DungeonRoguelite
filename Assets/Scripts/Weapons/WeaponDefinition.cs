using UnityEngine;
namespace DungeonRoguelite.Weapons
{
    [CreateAssetMenu(menuName = "Dungeon Roguelite/Permanent Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string englishName;
        [SerializeField] private string characterId;
        [SerializeField] private int tier = 1;
        [SerializeField] private string requiredDungeonId;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float attackSpeedMultiplier = 1f;
        public string Id => id;
        public string DisplayName => displayName;
        public string EnglishName => englishName;
        public string CharacterId => characterId;
        public int Tier => tier;
        public string RequiredDungeonId => requiredDungeonId;
        public float DamageMultiplier => damageMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;
    }
}
