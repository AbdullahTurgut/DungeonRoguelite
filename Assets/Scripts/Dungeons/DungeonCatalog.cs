using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRoguelite.Dungeons
{
    /// <summary>
    /// ScriptableObject defining the catalog of playable dungeons in campaign progression order.
    /// Immutable data container consumed by WorldMap UI and progression systems.
    /// </summary>
    [CreateAssetMenu(fileName = "DungeonCatalog", menuName = "Dungeon Roguelite/Dungeons/Dungeon Catalog")]
    public class DungeonCatalog : ScriptableObject
    {
        [Header("Dungeon Sequence")]
        [Tooltip("Ordered list of dungeons presented in the world map campaign.")]
        [SerializeField] private DungeonDefinition[] dungeons;

        public IReadOnlyList<DungeonDefinition> Dungeons => dungeons;
        public int Count => dungeons != null ? dungeons.Length : 0;

        public DungeonDefinition this[int index]
        {
            get
            {
                if (dungeons == null || index < 0 || index >= dungeons.Length)
                {
                    return null;
                }
                return dungeons[index];
            }
        }

        /// <summary>
        /// Finds a DungeonDefinition by its unique identifier.
        /// Returns null if not found.
        /// </summary>
        public DungeonDefinition GetDungeonById(string id)
        {
            if (string.IsNullOrEmpty(id) || dungeons == null)
            {
                return null;
            }

            for (int i = 0; i < dungeons.Length; i++)
            {
                if (dungeons[i] != null && dungeons[i].Id == id)
                {
                    return dungeons[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Configures the dungeons list (used during automated setup).
        /// </summary>
        public void SetDungeons(DungeonDefinition[] newDungeons)
        {
            dungeons = newDungeons;
        }
    }
}
