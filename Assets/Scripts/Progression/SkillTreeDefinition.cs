using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRoguelite.Progression
{
    /// <summary>
    /// Data-driven definition holding all skill nodes for a specific character archetype.
    /// Pure configuration asset; contains zero runtime state or save data.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTree_", menuName = "Dungeon Roguelite/Skill Tree Definition")]
    public class SkillTreeDefinition : ScriptableObject
    {
        [Tooltip("Owning character identifier (e.g. 'warrior', 'archer', 'gunner').")]
        [SerializeField] private string characterId;

        [Tooltip("All nodes comprising this character's skill tree.")]
        [SerializeField] private List<SkillNodeDefinition> nodes = new List<SkillNodeDefinition>();

        public string CharacterId => characterId;
        public IReadOnlyList<SkillNodeDefinition> Nodes => nodes;

        public void Initialize(string charId, IEnumerable<SkillNodeDefinition> initialNodes)
        {
            characterId = charId;
            nodes.Clear();
            if (initialNodes != null)
            {
                nodes.AddRange(initialNodes);
            }
        }

        public bool TryGetNode(string nodeId, out SkillNodeDefinition node)
        {
            if (!string.IsNullOrEmpty(nodeId) && nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (string.Equals(nodes[i].Id, nodeId, StringComparison.OrdinalIgnoreCase))
                    {
                        node = nodes[i];
                        return true;
                    }
                }
            }
            node = null;
            return false;
        }

        public bool ValidateTree(out string error)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                error = "CharacterId is null or empty.";
                return false;
            }
            if (nodes == null || nodes.Count == 0)
            {
                error = "Skill tree contains no nodes.";
                return false;
            }

            var idSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null)
                {
                    error = $"Node at index {i} is null.";
                    return false;
                }
                if (string.IsNullOrEmpty(n.Id))
                {
                    error = $"Node at index {i} has empty ID.";
                    return false;
                }
                if (!idSet.Add(n.Id))
                {
                    error = $"Duplicate node ID '{n.Id}'.";
                    return false;
                }
                if (!string.Equals(n.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    error = $"Node '{n.Id}' character ID '{n.CharacterId}' does not match tree character ID '{characterId}'.";
                    return false;
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.HasPrerequisite && !idSet.Contains(n.PrerequisiteNodeId))
                {
                    error = $"Node '{n.Id}' has missing prerequisite '{n.PrerequisiteNodeId}'.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
