using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRoguelite.Dungeons
{
    /// <summary>
    /// Static persistence service for dungeon campaign progression.
    /// Stores completed dungeon IDs via PlayerPrefs JSON serialization.
    /// Derives dungeon unlock state purely from DungeonDefinition prerequisite dependencies.
    /// Zero hardcoded dungeon progression logic.
    /// </summary>
    public static class DungeonProgression
    {
        public const string PrefsKey = "DungeonProgression_CompletedDungeons";

        [Serializable]
        private class ProgressionData
        {
            public List<string> completedIds = new List<string>();
        }

        private static HashSet<string> completedDungeonIds = new HashSet<string>();
        private static bool isLoaded = false;

        /// <summary>
        /// Fired whenever a newly completed dungeon ID is persisted.
        /// </summary>
        public static event Action<string> OnDungeonCompletedPersisted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            completedDungeonIds.Clear();
            isLoaded = false;
            OnDungeonCompletedPersisted = null;
        }

        private static void EnsureLoaded()
        {
            if (isLoaded)
            {
                return;
            }

            completedDungeonIds.Clear();
            if (PlayerPrefs.HasKey(PrefsKey))
            {
                string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var data = JsonUtility.FromJson<ProgressionData>(json);
                        if (data != null && data.completedIds != null)
                        {
                            foreach (var id in data.completedIds)
                            {
                                if (!string.IsNullOrEmpty(id))
                                {
                                    completedDungeonIds.Add(id);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[DungeonProgression] Failed to parse saved progression JSON: {ex.Message}");
                    }
                }
            }

            isLoaded = true;
        }

        /// <summary>
        /// Returns true if the specified dungeon ID has been recorded as completed.
        /// </summary>
        public static bool IsDungeonCompleted(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId))
            {
                return false;
            }

            EnsureLoaded();
            return completedDungeonIds.Contains(dungeonId);
        }

        /// <summary>
        /// Derives whether a DungeonDefinition is currently unlocked based on its prerequisite.
        /// If HasPrerequisite is false, returns true. Otherwise, checks if RequiredDungeonId is completed.
        /// </summary>
        public static bool IsDungeonUnlocked(DungeonDefinition dungeon)
        {
            if (dungeon == null)
            {
                return false;
            }

            if (!dungeon.HasPrerequisite)
            {
                return true;
            }

            return IsDungeonCompleted(dungeon.RequiredDungeonId);
        }

        /// <summary>
        /// Records a dungeon ID as completed, saves to PlayerPrefs, and fires notification.
        /// Idempotent: repeated calls for the same ID do not produce duplicate entries or redundant saves.
        /// </summary>
        public static void RecordDungeonCompleted(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId))
            {
                return;
            }

            EnsureLoaded();

            if (completedDungeonIds.Add(dungeonId))
            {
                SaveToPrefs();
                OnDungeonCompletedPersisted?.Invoke(dungeonId);
            }
        }

        private static void SaveToPrefs()
        {
            var data = new ProgressionData();
            data.completedIds.AddRange(completedDungeonIds);
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PrefsKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Resets all saved progression and clears runtime cache.
        /// </summary>
        public static void ResetProgression()
        {
            completedDungeonIds.Clear();
            isLoaded = true;

            if (PlayerPrefs.HasKey(PrefsKey))
            {
                PlayerPrefs.DeleteKey(PrefsKey);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Read-only collection of all completed dungeon IDs currently in memory.
        /// </summary>
        public static IReadOnlyCollection<string> CompletedDungeonIds
        {
            get
            {
                EnsureLoaded();
                return completedDungeonIds;
            }
        }
    }
}
