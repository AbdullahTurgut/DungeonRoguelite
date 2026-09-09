using System;
using UnityEngine;

namespace DungeonRoguelite.Progression
{
    /// <summary>
    /// Serializable definition of an individual node in a character's skill tree.
    /// Immutable configuration; contains zero runtime state or save data.
    /// </summary>
    [Serializable]
    public class SkillNodeDefinition
    {
        [Tooltip("Stable unique identifier for this node (e.g. 'warrior_dmg_1').")]
        [SerializeField] private string id;

        [Tooltip("Owning character identifier (e.g. 'warrior', 'archer', 'gunner').")]
        [SerializeField] private string characterId;

        [Tooltip("Branch category within the character's tree.")]
        [SerializeField] private string branchId;

        [Tooltip("Tier rank in the branch (1, 2, 3).")]
        [SerializeField] private int tier = 1;

        [Tooltip("Player-facing name for this skill.")]
        [SerializeField] private string displayName;

        [Tooltip("Player-facing description of the skill effect.")]
        [TextArea(2, 3)]
        [SerializeField] private string description;

        [Tooltip("Permanent skill point cost to purchase this node.")]
        [SerializeField] private int cost = 1;

        [Tooltip("Node ID that must be purchased before this node can be unlocked. Empty for Tier 1.")]
        [SerializeField] private string prerequisiteNodeId;

        [Tooltip("Category of permanent stat modifier applied by this node.")]
        [SerializeField] private PermanentEffectType effectType;

        [Tooltip("Additive modifier magnitude (e.g. 0.10 for +10%).")]
        [SerializeField] private float effectMagnitude;

        public string Id => id;
        public string CharacterId => characterId;
        public string BranchId => branchId;
        public int Tier => tier;
        public string DisplayName => displayName;
        public string Description => description;
        public int Cost => cost > 0 ? cost : 1;
        public string PrerequisiteNodeId => prerequisiteNodeId;
        public PermanentEffectType EffectType => effectType;
        public float EffectMagnitude => effectMagnitude;

        public bool HasPrerequisite => !string.IsNullOrEmpty(prerequisiteNodeId);

        public SkillNodeDefinition() { }

        public SkillNodeDefinition(
            string id,
            string characterId,
            string branchId,
            int tier,
            string displayName,
            string description,
            int cost,
            string prerequisiteNodeId,
            PermanentEffectType effectType,
            float effectMagnitude)
        {
            this.id = id;
            this.characterId = characterId;
            this.branchId = branchId;
            this.tier = tier;
            this.displayName = displayName;
            this.description = description;
            this.cost = cost > 0 ? cost : 1;
            this.prerequisiteNodeId = prerequisiteNodeId;
            this.effectType = effectType;
            this.effectMagnitude = effectMagnitude;
        }
    }
}
