using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRoguelite.Progression
{
    /// <summary>
    /// Pure runtime static session carrier managing temporary campaign run progression across dungeons.
    /// Manages two distinct progression tiers:
    /// 1. Committed Run State: earned exclusively through successfully completed dungeons (Victory).
    /// 2. Dungeon Entry Checkpoint: snapshot of the committed state when entering the current dungeon (used for Retry rollback).
    /// Contains strictly data; holds zero GameObject or MonoBehaviour references, no UI, and no persistence to disk.
    /// </summary>
    public static class RunProgressionSession
    {
        private static string ownerCharacterId;
        private static bool hasActiveRun = false;

        // --- COMMITTED RUN STATE (survives successfully completed dungeons) ---
        private static int committedLevel = 1;
        private static int committedCurrentXP = 0;
        private static int committedTotalXP = 0;
        private static readonly List<string> committedUpgradeIds = new List<string>();

        // --- DUNGEON ENTRY CHECKPOINT (snapshot at dungeon start for retry rollback) ---
        private static int checkpointLevel = 1;
        private static int checkpointCurrentXP = 0;
        private static int checkpointTotalXP = 0;
        private static readonly List<string> checkpointUpgradeIds = new List<string>();

        public static bool HasActiveRun => hasActiveRun;
        public static string OwnerCharacterId => ownerCharacterId;

        // Properties reflecting the currently active/committed build for dungeon entry
        public static int Level => committedLevel;
        public static int CurrentXP => committedCurrentXP;
        public static int TotalXP => committedTotalXP;
        public static IReadOnlyList<string> CommittedUpgradeIds => committedUpgradeIds;

        public static int CheckpointLevel => checkpointLevel;
        public static int CheckpointCurrentXP => checkpointCurrentXP;
        public static int CheckpointTotalXP => checkpointTotalXP;
        public static IReadOnlyList<string> CheckpointUpgradeIds => checkpointUpgradeIds;

        /// <summary>
        /// Validates that the active campaign run belongs to the specified character ID.
        /// CharacterSelectionSession remains authoritative; this check exists strictly for ownership verification.
        /// </summary>
        public static bool ValidateOwner(string characterId)
        {
            return hasActiveRun && !string.IsNullOrEmpty(ownerCharacterId) &&
                   string.Equals(ownerCharacterId, characterId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a fresh campaign run for the specified character archetype.
        /// Resets all progression to Level 1, 0 XP, and empty upgrades.
        /// </summary>
        public static void StartNewRun(string characterId)
        {
            ownerCharacterId = characterId;
            hasActiveRun = true;

            committedLevel = 1;
            committedCurrentXP = 0;
            committedTotalXP = 0;
            committedUpgradeIds.Clear();

            checkpointLevel = 1;
            checkpointCurrentXP = 0;
            checkpointTotalXP = 0;
            checkpointUpgradeIds.Clear();
        }

        /// <summary>
        /// Creates a dungeon entry checkpoint from the currently committed state.
        /// Call upon entering a dungeon before live gameplay begins.
        /// </summary>
        public static void CreateDungeonCheckpoint()
        {
            checkpointLevel = committedLevel;
            checkpointCurrentXP = committedCurrentXP;
            checkpointTotalXP = committedTotalXP;
            checkpointUpgradeIds.Clear();
            checkpointUpgradeIds.AddRange(committedUpgradeIds);
        }

        /// <summary>
        /// Commits finalized progression earned upon dungeon victory.
        /// This state becomes the durable foundation for the next dungeon in the campaign run.
        /// </summary>
        public static void CommitDungeonVictory(int finalLevel, int finalCurrentXP, int finalTotalXP, IEnumerable<string> finalUpgradeIds)
        {
            committedLevel = Mathf.Max(1, finalLevel);
            committedCurrentXP = Mathf.Max(0, finalCurrentXP);
            committedTotalXP = Mathf.Max(0, finalTotalXP);
            committedUpgradeIds.Clear();
            if (finalUpgradeIds != null)
            {
                committedUpgradeIds.AddRange(finalUpgradeIds);
            }

            // Keep checkpoint in sync with latest victory
            CreateDungeonCheckpoint();
        }

        /// <summary>
        /// Restores state to the dungeon entry checkpoint upon Retry after Defeat.
        /// Discards any XP, levels, or upgrades accumulated during the failed attempt.
        /// </summary>
        public static void RestoreCheckpointOnRetry()
        {
            // Committed state was never modified during failed attempt, so checkpoint matches committed state
            checkpointLevel = committedLevel;
            checkpointCurrentXP = committedCurrentXP;
            checkpointTotalXP = committedTotalXP;
            checkpointUpgradeIds.Clear();
            checkpointUpgradeIds.AddRange(committedUpgradeIds);
        }

        /// <summary>
        /// Terminates the active campaign run.
        /// Clears all temporary progression, resets owner, and marks run inactive.
        /// </summary>
        public static void EndRun()
        {
            hasActiveRun = false;
            ownerCharacterId = null;

            committedLevel = 1;
            committedCurrentXP = 0;
            committedTotalXP = 0;
            committedUpgradeIds.Clear();

            checkpointLevel = 1;
            checkpointCurrentXP = 0;
            checkpointTotalXP = 0;
            checkpointUpgradeIds.Clear();
        }

        /// <summary>
        /// Completely clears all session state. Useful for test isolation.
        /// </summary>
        public static void Clear()
        {
            EndRun();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticSession()
        {
            Clear();
        }
    }
}
