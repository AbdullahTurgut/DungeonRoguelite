using UnityEngine;

namespace DungeonRoguelite.Dungeons
{
    /// <summary>
    /// Immutable value structure representing summary statistics for a completed dungeon run.
    /// Pure data container; contains zero gameplay or UI logic.
    /// </summary>
    [System.Serializable]
    public struct DungeonRunSummary
    {
        public float CompletionTime;
        public int EnemiesDefeated;
        public int FinalLevel;
        public int TotalXPEarned;

        public DungeonRunSummary(float completionTime, int enemiesDefeated, int finalLevel, int totalXPEarned)
        {
            CompletionTime = Mathf.Max(0f, completionTime);
            EnemiesDefeated = Mathf.Max(0, enemiesDefeated);
            FinalLevel = Mathf.Max(1, finalLevel);
            TotalXPEarned = Mathf.Max(0, totalXPEarned);
        }

        /// <summary>
        /// Formats CompletionTime as MM:SS (e.g. 05:42).
        /// </summary>
        public string FormattedTime
        {
            get
            {
                int minutes = Mathf.FloorToInt(CompletionTime / 60f);
                int seconds = Mathf.FloorToInt(CompletionTime % 60f);
                return $"{minutes:00}:{seconds:00}";
            }
        }
    }
}
