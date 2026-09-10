using UnityEngine;

namespace DungeonRoguelite.Characters
{
    /// <summary>
    /// Data-driven definition for a playable character archetype.
    /// Defines character identity, display metadata, and prefab linkage.
    /// Strictly decoupled from weapon-specific combat calculations and runtime stat stacking.
    /// </summary>
    [CreateAssetMenu(fileName = "Character_New", menuName = "Dungeon Roguelite/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique character key (e.g., 'warrior').")]
        [SerializeField] private string id = "warrior";

        [Tooltip("Display name shown in character selection and UI.")]
        [SerializeField] private string displayName = "Warrior";

        [Tooltip("Brief overview of character playstyle and strengths.")]
        [SerializeField, TextArea(2, 4)] private string description = "A sturdy melee fighter with high survivability and sweeping sword attacks.";

        [Header("Prefab Reference")]
        [Tooltip("The playable character prefab instantiated for this archetype.")]
        [SerializeField] private GameObject characterPrefab;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public GameObject CharacterPrefab => characterPrefab;

        [Header("Progression")]
        [Tooltip("Permanent skill tree definition for this character archetype.")]
        [SerializeField] private DungeonRoguelite.Progression.SkillTreeDefinition skillTree;

        public DungeonRoguelite.Progression.SkillTreeDefinition SkillTree => skillTree;
        [SerializeField] private DungeonRoguelite.Weapons.WeaponCatalog weaponCatalog;
        public DungeonRoguelite.Weapons.WeaponCatalog WeaponCatalog => weaponCatalog;

        /// <summary>
        /// Test or editor helper to configure the character prefab reference.
        /// </summary>
        public void SetCharacterPrefab(GameObject prefab)
        {
            characterPrefab = prefab;
        }

        /// <summary>
        /// Configures the permanent skill tree definition reference.
        /// </summary>
        public void SetSkillTree(DungeonRoguelite.Progression.SkillTreeDefinition tree)
        {
            skillTree = tree;
        }
    }
}
