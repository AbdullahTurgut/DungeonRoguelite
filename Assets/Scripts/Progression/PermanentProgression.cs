using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRoguelite.Progression
{
    /// <summary>
    /// Value container for aggregated permanent stat multipliers.
    /// </summary>
    public struct PermanentStatModifiers
    {
        public float damageMultiplier;
        public float attackSpeedMultiplier;
        public float movementSpeedMultiplier;
        public float maxHealthMultiplier;

        public static PermanentStatModifiers Default => new PermanentStatModifiers
        {
            damageMultiplier = 1f,
            attackSpeedMultiplier = 1f,
            movementSpeedMultiplier = 1f,
            maxHealthMultiplier = 1f
        };
    }

    /// <summary>
    /// Static persistence and state service managing character-specific permanent progression.
    /// Survives game restarts, run retries, and run terminations.
    /// Manages skill points, unlocked skill nodes, and claimed dungeon completion rewards per character.
    /// </summary>
    public static class PermanentProgression
    {
        public const string PrefsKey = "PermanentProgression_Data";

        [Serializable]
        public class PermanentSaveData
        {
            public int version = 2;
            public List<CharacterProgressionData> characters = new List<CharacterProgressionData>();
        }

        [Serializable]
        public class CharacterProgressionData
        {
            public string characterId = string.Empty;
            public int availableSkillPoints = 0;
            public List<string> unlockedNodeIds = new List<string>();
            public List<string> rewardedDungeonIds = new List<string>();
            public bool legacyD5RewardPending;
        }

        private static readonly Dictionary<string, CharacterProgressionData> characterCache =
            new Dictionary<string, CharacterProgressionData>(StringComparer.OrdinalIgnoreCase);

        private static bool isLoaded = false;

        /// <summary>
        /// Fired whenever a skill node is successfully purchased for a character (characterId, nodeId).
        /// </summary>
        public static event Action<string, string> OnSkillPurchased;

        /// <summary>
        /// Fired whenever a dungeon first-clear reward is awarded to a character (characterId, dungeonId, pointsAwarded).
        /// </summary>
        public static event Action<string, string, int> OnDungeonRewardAwarded;

        /// <summary>
        /// Fired whenever available skill points change for a character (characterId, newAvailablePoints).
        /// </summary>
        public static event Action<string, int> OnSkillPointsChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            characterCache.Clear();
            isLoaded = false;
            OnSkillPurchased = null;
            OnDungeonRewardAwarded = null;
            OnSkillPointsChanged = null;
        }

        private static void EnsureLoaded()
        {
            if (isLoaded) return;

            characterCache.Clear();

            if (PlayerPrefs.HasKey(PrefsKey))
            {
                string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var data = JsonUtility.FromJson<PermanentSaveData>(json);
                        if (data != null && data.characters != null)
                        {
                            foreach (var charData in data.characters)
                            {
                                if (charData != null && !string.IsNullOrEmpty(charData.characterId))
                                {
                                    charData.unlockedNodeIds = NormalizeIds(charData.unlockedNodeIds);
                                    charData.rewardedDungeonIds = NormalizeIds(charData.rewardedDungeonIds);
                                    if (data.version < 2) charData.legacyD5RewardPending = true;
                                    characterCache[charData.characterId] = charData;
                                }
                            }
                            if (data.version < 2) SaveToPrefs();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[PermanentProgression] Failed to parse save data JSON: {ex.Message}");
                    }
                }
            }

            isLoaded = true;
        }

        private static void SaveToPrefs()
        {
            var data = new PermanentSaveData();
            data.characters.AddRange(characterCache.Values);
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PrefsKey, json);
            PlayerPrefs.Save();
        }

        private static List<string> NormalizeIds(List<string> ids)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (ids != null)
                foreach (var id in ids)
                    if (!string.IsNullOrWhiteSpace(id) && seen.Add(id.Trim())) result.Add(id.Trim());
            return result;
        }

        private static CharacterProgressionData GetOrAddCharacterData(string characterId)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(characterId)) return null;

            if (!characterCache.TryGetValue(characterId, out var data))
            {
                data = new CharacterProgressionData
                {
                    characterId = characterId,
                    availableSkillPoints = 0,
                    unlockedNodeIds = new List<string>(),
                    rewardedDungeonIds = new List<string>()
                };
                characterCache[characterId] = data;
            }

            if (data.unlockedNodeIds == null) data.unlockedNodeIds = new List<string>();
            if (data.rewardedDungeonIds == null) data.rewardedDungeonIds = new List<string>();

            return data;
        }

        /// <summary>
        /// Returns available skill points for the specified character archetype.
        /// </summary>
        public static int GetAvailablePoints(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return 0;
            EnsureLoaded();

            if (characterCache.TryGetValue(characterId, out var data))
            {
                return Mathf.Max(0, data.availableSkillPoints);
            }
            return 0;
        }

        /// <summary>
        /// Awards skill points to a specific character.
        /// </summary>
        public static void AddSkillPoints(string characterId, int points)
        {
            if (string.IsNullOrEmpty(characterId) || points <= 0) return;
            var data = GetOrAddCharacterData(characterId);
            if (data == null) return;

            data.availableSkillPoints += points;
            SaveToPrefs();
            OnSkillPointsChanged?.Invoke(characterId, data.availableSkillPoints);
        }

        /// <summary>
        /// Checks if a skill node is already purchased for the specified character.
        /// </summary>
        public static bool IsNodePurchased(string characterId, string nodeId)
        {
            if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(nodeId)) return false;
            EnsureLoaded();

            if (characterCache.TryGetValue(characterId, out var data) && data.unlockedNodeIds != null)
            {
                return data.unlockedNodeIds.Exists(id => string.Equals(id, nodeId, StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        /// <summary>
        /// Returns all unlocked node IDs for the specified character.
        /// </summary>
        public static IReadOnlyList<string> GetUnlockedNodeIds(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return Array.Empty<string>();
            EnsureLoaded();

            if (characterCache.TryGetValue(characterId, out var data) && data.unlockedNodeIds != null)
            {
                return data.unlockedNodeIds;
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Evaluates whether a node can be purchased by the character.
        /// Validates point cost, duplicate ownership, character match, and prerequisite unlock status.
        /// </summary>
        public static bool CanPurchaseNode(string characterId, SkillNodeDefinition node, SkillTreeDefinition tree)
        {
            if (string.IsNullOrEmpty(characterId) || node == null) return false;
            if (!string.Equals(characterId, node.CharacterId, StringComparison.OrdinalIgnoreCase)) return false;
            if (IsNodePurchased(characterId, node.Id)) return false;
            if (GetAvailablePoints(characterId) < node.Cost) return false;

            if (node.HasPrerequisite)
            {
                if (!IsNodePurchased(characterId, node.PrerequisiteNodeId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to purchase a skill node for the character.
        /// Deducts points, persists state, and notifies listeners on success.
        /// </summary>
        public static bool TryPurchaseNode(string characterId, SkillNodeDefinition node, SkillTreeDefinition tree)
        {
            if (!CanPurchaseNode(characterId, node, tree)) return false;

            var data = GetOrAddCharacterData(characterId);
            if (data == null) return false;

            data.availableSkillPoints -= node.Cost;
            if (!data.unlockedNodeIds.Contains(node.Id))
            {
                data.unlockedNodeIds.Add(node.Id);
            }

            SaveToPrefs();
            OnSkillPurchased?.Invoke(characterId, node.Id);
            OnSkillPointsChanged?.Invoke(characterId, data.availableSkillPoints);
            return true;
        }

        /// <summary>
        /// Checks if a dungeon completion reward has already been claimed by this character.
        /// </summary>
        public static bool IsDungeonRewardClaimed(string characterId, string dungeonId)
        {
            if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(dungeonId)) return false;
            EnsureLoaded();

            if (characterCache.TryGetValue(characterId, out var data) && data.rewardedDungeonIds != null)
            {
                if (data.legacyD5RewardPending && string.Equals(dungeonId.Trim(), "dungeon_5", StringComparison.OrdinalIgnoreCase)) return false;
                for (int i = 0; i < data.rewardedDungeonIds.Count; i++)
                {
                    if (string.Equals(data.rewardedDungeonIds[i], dungeonId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Maps canonical dungeon IDs to their first-clear permanent skill point values.
        /// Campaign first-clear economy. Returns 0 for unknown dungeons.
        /// </summary>
        public static int GetDungeonFirstClearPoints(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId)) return 0;
            switch (dungeonId.Trim().ToLowerInvariant())
            {
                case "dungeon_1":
                case "dungeon_2":
                case "dungeon_3":
                case "dungeon_4":
                case "dungeon_6":
                case "dungeon_7":
                    return 1;
                case "dungeon_5":
                case "dungeon_8":
                case "dungeon_9":
                    return 2;
                case "dungeon_10":
                    return 3;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Attempts to award the first-clear permanent reward for a dungeon to the specified character.
        /// Validates economy point mapping and enforces strict idempotency (duplicate clears award 0 points).
        /// </summary>
        public static bool TryAwardDungeonFirstClear(string characterId, string dungeonId, out int pointsAwarded)
        {
            pointsAwarded = 0;
            if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(dungeonId)) return false;

            int points = GetDungeonFirstClearPoints(dungeonId);
            if (points <= 0) return false;

            if (AwardDungeonClearReward(characterId, dungeonId, points))
            {
                pointsAwarded = points;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Awards first-clear permanent skill points for a dungeon to the specified character.
        /// Strictly idempotent: duplicate completions for the same dungeon/character grant 0 additional points.
        /// </summary>
        public static bool AwardDungeonClearReward(string characterId, string dungeonId, int points)
        {
            if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(dungeonId)) return false;
            if (IsDungeonRewardClaimed(characterId, dungeonId)) return false;

            var data = GetOrAddCharacterData(characterId);
            if (data == null) return false;

            string normalizedDungeonId = dungeonId.Trim().ToLowerInvariant();
            if (!data.rewardedDungeonIds.Exists(id => string.Equals(id, normalizedDungeonId, StringComparison.OrdinalIgnoreCase)))
                data.rewardedDungeonIds.Add(normalizedDungeonId);
            if (normalizedDungeonId == "dungeon_5") data.legacyD5RewardPending = false;
            if (points > 0)
            {
                data.availableSkillPoints += points;
            }

            SaveToPrefs();
            OnDungeonRewardAwarded?.Invoke(characterId, normalizedDungeonId, points);
            if (points > 0)
            {
                OnSkillPointsChanged?.Invoke(characterId, data.availableSkillPoints);
            }
            return true;
        }

        /// <summary>
        /// Calculates aggregated permanent stat multipliers from unlocked nodes for the character.
        /// </summary>
        public static PermanentStatModifiers GetPermanentModifiers(string characterId, SkillTreeDefinition tree)
        {
            if (string.IsNullOrEmpty(characterId) || tree == null) return PermanentStatModifiers.Default;
            EnsureLoaded();

            if (!characterCache.TryGetValue(characterId, out var data) || data.unlockedNodeIds == null || data.unlockedNodeIds.Count == 0)
            {
                return PermanentStatModifiers.Default;
            }

            float damageBonus = 0f;
            float attackSpeedBonus = 0f;
            float movementSpeedBonus = 0f;
            float maxHealthBonus = 0f;

            for (int i = 0; i < data.unlockedNodeIds.Count; i++)
            {
                string nodeId = data.unlockedNodeIds[i];
                if (tree.TryGetNode(nodeId, out var node))
                {
                    switch (node.EffectType)
                    {
                        case PermanentEffectType.DamageMultiplier:
                            damageBonus += node.EffectMagnitude;
                            break;
                        case PermanentEffectType.AttackSpeedMultiplier:
                            attackSpeedBonus += node.EffectMagnitude;
                            break;
                        case PermanentEffectType.MovementSpeedMultiplier:
                            movementSpeedBonus += node.EffectMagnitude;
                            break;
                        case PermanentEffectType.MaxHealthMultiplier:
                            maxHealthBonus += node.EffectMagnitude;
                            break;
                    }
                }
            }

            return new PermanentStatModifiers
            {
                damageMultiplier = Mathf.Max(0.01f, 1f + damageBonus),
                attackSpeedMultiplier = Mathf.Max(0.01f, 1f + attackSpeedBonus),
                movementSpeedMultiplier = Mathf.Max(0.01f, 1f + movementSpeedBonus),
                maxHealthMultiplier = Mathf.Max(0.01f, 1f + maxHealthBonus)
            };
        }

        /// <summary>
        /// Resets all saved progression across all characters and clears in-memory state.
        /// </summary>
        public static void ResetAllProgression()
        {
            characterCache.Clear();
            isLoaded = true;

            if (PlayerPrefs.HasKey(PrefsKey))
            {
                PlayerPrefs.DeleteKey(PrefsKey);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Resets progression for a specific character archetype.
        /// </summary>
        public static void ResetCharacterProgression(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return;
            EnsureLoaded();

            if (characterCache.Remove(characterId))
            {
                SaveToPrefs();
            }
        }
    }
}
