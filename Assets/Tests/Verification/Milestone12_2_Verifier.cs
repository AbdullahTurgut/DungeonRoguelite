#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    public sealed class Milestone12_2_Verifier : MonoBehaviour
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private Phase12StateSnapshot snapshot;
        private int checks;
        private int runtimeErrors;

        private IEnumerator Start()
        {
            Application.logMessageReceived += HandleLog;
            snapshot = new Phase12StateSnapshot();
            bool success = true;

            try
            {
                VerifyDungeonEconomyValues();
                VerifyAuthoritativeCompletionAwarding();
                VerifyDuplicateClearRejection();
                VerifyD2AndD3EconomyAwarding();
                VerifyCrossCharacterIsolation();
                VerifyDefeatDoesNotAwardPoints();
                VerifyRetryDoesNotAwardPoints();
                VerifyReturnToMapDoesNotAwardPoints();
                VerifyRunXPAbsolutelyDecoupled();
                VerifyPersistenceAcrossReload();
                VerifyGlobalDungeonProgressionSeparation();
                VerifyFailSafeHandling();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                success = false;
            }
            finally
            {
                Cleanup();
                snapshot?.Dispose();
                snapshot = null;
            }

            Application.logMessageReceived -= HandleLog;
            success &= runtimeErrors == 0;
            Debug.Log($"[GATE 12.2 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.isPlaying = false;
            yield break;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
            snapshot?.Dispose();
            snapshot = null;
        }

        private void HandleLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                runtimeErrors++;
            }
        }

        private T Own<T>(T obj) where T : UnityEngine.Object
        {
            if (obj != null) owned.Add(obj);
            return obj;
        }

        private void DestroyOwned(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                owned.Remove(obj);
                DestroyImmediate(obj);
            }
        }

        private void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException("[GATE 12.2 FAILED] " + description);
            Debug.Log($"[GATE 12.2 CHECK {++checks} PASSED] {description}");
        }

        private void Cleanup()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null) DestroyImmediate(owned[i]);
            }
            owned.Clear();
            PermanentProgression.ResetAllProgression();
            DungeonProgression.ResetProgression();
            CharacterSelectionSession.Clear();
            RunProgressionSession.EndRun();
        }

        private void VerifyDungeonEconomyValues()
        {
            Check(PermanentProgression.GetDungeonFirstClearPoints("dungeon_1") == 2, "Economy: dungeon_1 first-clear grants 2 points");
            Check(PermanentProgression.GetDungeonFirstClearPoints("dungeon_2") == 2, "Economy: dungeon_2 first-clear grants 2 points");
            Check(PermanentProgression.GetDungeonFirstClearPoints("dungeon_3") == 3, "Economy: dungeon_3 first-clear grants 3 points");
            Check(PermanentProgression.GetDungeonFirstClearPoints("dungeon_1") +
                  PermanentProgression.GetDungeonFirstClearPoints("dungeon_2") +
                  PermanentProgression.GetDungeonFirstClearPoints("dungeon_3") == 7,
                  "Economy: total campaign points per character = 7");

            // Case insensitivity
            Check(PermanentProgression.GetDungeonFirstClearPoints("DUNGEON_1") == 2, "Economy: case-insensitive DUNGEON_1 == 2");
            Check(PermanentProgression.GetDungeonFirstClearPoints("Dungeon_2") == 2, "Economy: case-insensitive Dungeon_2 == 2");
            Check(PermanentProgression.GetDungeonFirstClearPoints("DUNGEON_3") == 3, "Economy: case-insensitive DUNGEON_3 == 3");

            // Unknown or invalid
            Check(PermanentProgression.GetDungeonFirstClearPoints("unknown_dungeon") == 0, "Economy: unknown dungeon grants 0 points");
            Check(PermanentProgression.GetDungeonFirstClearPoints(null) == 0, "Economy: null dungeon grants 0 points");
            Check(PermanentProgression.GetDungeonFirstClearPoints("") == 0, "Economy: empty dungeon grants 0 points");
        }

        private void VerifyAuthoritativeCompletionAwarding()
        {
            PermanentProgression.ResetAllProgression();
            DungeonProgression.ResetProgression();

            var go = Own(new GameObject("Test_DungeonCompletion"));
            var completionController = go.AddComponent<DungeonCompletionController>();

            // Setup test character
            var charGo = Own(new GameObject("Test_Warrior_Player"));
            var playable = charGo.AddComponent<PlayableCharacter>();
            var charDef = Own(ScriptableObject.CreateInstance<CharacterDefinition>());
            typeof(CharacterDefinition).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(charDef, "warrior");
            playable.SetCharacterDefinition(charDef);

            var exp = charGo.AddComponent<PlayerExperience>();
            var statsGo = Own(new GameObject("Test_RunStats"));
            var runStats = statsGo.AddComponent<DungeonRunStats>();

            var dungeonDef = Own(ScriptableObject.CreateInstance<DungeonDefinition>());
            dungeonDef.SetConfiguration("dungeon_1", "D1", "Desc", "Scene", null, Array.Empty<WaveDefinition>());

            var waveGo = Own(new GameObject("Test_WaveManager"));
            var waveManager = waveGo.AddComponent<WaveManager>();
            typeof(WaveManager).GetField("activeDungeon", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(waveManager, dungeonDef);

            completionController.SetReferences(waveManager, null, runStats, exp, null, playable);

            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Pre-completion: Warrior has 0 available points");
            Check(!PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_1"), "Pre-completion: Warrior D1 reward is not claimed");
            Check(!DungeonProgression.IsDungeonCompleted("dungeon_1"), "Pre-completion: D1 is not recorded in DungeonProgression");

            // Execute completion
            completionController.HandleWaveManagerCompletion();

            Check(completionController.HasCompleted, "CompletionController marked as completed");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Post-completion: Warrior awarded exactly 2 points for D1");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_1"), "Post-completion: Warrior D1 reward is marked claimed");
            Check(DungeonProgression.IsDungeonCompleted("dungeon_1"), "Post-completion: Global DungeonProgression records D1 completed");

            DestroyOwned(go);
            DestroyOwned(charGo);
            DestroyOwned(statsGo);
            DestroyOwned(waveGo);
        }

        private void VerifyDuplicateClearRejection()
        {
            // Warrior already completed D1
            Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Baseline: Warrior currently has 2 points");

            var go = Own(new GameObject("Test_DungeonCompletion_Duplicate"));
            var completionController = go.AddComponent<DungeonCompletionController>();

            var charGo = Own(new GameObject("Test_Warrior_Player2"));
            var playable = charGo.AddComponent<PlayableCharacter>();
            var charDef = Own(ScriptableObject.CreateInstance<CharacterDefinition>());
            typeof(CharacterDefinition).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(charDef, "warrior");
            playable.SetCharacterDefinition(charDef);

            var dungeonDef = Own(ScriptableObject.CreateInstance<DungeonDefinition>());
            dungeonDef.SetConfiguration("dungeon_1", "D1", "Desc", "Scene", null, Array.Empty<WaveDefinition>());

            var waveGo = Own(new GameObject("Test_WaveManager_Duplicate"));
            var waveManager = waveGo.AddComponent<WaveManager>();
            typeof(WaveManager).GetField("activeDungeon", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(waveManager, dungeonDef);

            completionController.SetReferences(waveManager, null, null, null, null, playable);

            // Trigger second completion of D1
            completionController.HandleWaveManagerCompletion();

            Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Duplicate clear: Warrior still has exactly 2 points (0 additional awarded)");

            // Also test direct TryAwardDungeonFirstClear call
            bool awarded = PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_1", out int pts);
            Check(!awarded, "Direct duplicate call: TryAwardDungeonFirstClear returned false");
            Check(pts == 0, "Direct duplicate call: points awarded == 0");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Warrior points unchanged at 2");

            DestroyOwned(go);
            DestroyOwned(charGo);
            DestroyOwned(waveGo);
        }

        private void VerifyD2AndD3EconomyAwarding()
        {
            // Award D2 to Warrior (2 points)
            bool awardedD2 = PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_2", out int ptsD2);
            Check(awardedD2, "First clear D2 for Warrior succeeded");
            Check(ptsD2 == 2, "D2 awarded exactly 2 points");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 4, "Warrior total points after D1+D2 = 4");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_2"), "Warrior marked as claimed for D2");

            // Duplicate D2 clear rejected
            Check(!PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_2", out int ptsD2Dup), "Duplicate D2 clear rejected");
            Check(ptsD2Dup == 0, "Duplicate D2 awarded 0 points");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 4, "Warrior points remain 4");

            // Award D3 to Warrior (3 points)
            bool awardedD3 = PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_3", out int ptsD3);
            Check(awardedD3, "First clear D3 for Warrior succeeded");
            Check(ptsD3 == 3, "D3 awarded exactly 3 points");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 7, "Warrior total points after D1+D2+D3 = 7 (maximum)");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_3"), "Warrior marked as claimed for D3");

            // Duplicate D3 clear rejected
            Check(!PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_3", out int ptsD3Dup), "Duplicate D3 clear rejected");
            Check(ptsD3Dup == 0, "Duplicate D3 awarded 0 points");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 7, "Warrior points remain 7");
        }

        private void VerifyCrossCharacterIsolation()
        {
            // Warrior has 7 points and cleared D1, D2, D3
            Check(PermanentProgression.GetAvailablePoints("warrior") == 7, "Warrior has 7 points");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_1"), "Warrior claimed D1");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_2"), "Warrior claimed D2");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_3"), "Warrior claimed D3");

            // Archer has 0 points and has claimed nothing
            Check(PermanentProgression.GetAvailablePoints("archer") == 0, "Isolation: Archer has 0 points");
            Check(!PermanentProgression.IsDungeonRewardClaimed("archer", "dungeon_1"), "Isolation: Archer D1 not claimed");
            Check(!PermanentProgression.IsDungeonRewardClaimed("archer", "dungeon_2"), "Isolation: Archer D2 not claimed");
            Check(!PermanentProgression.IsDungeonRewardClaimed("archer", "dungeon_3"), "Isolation: Archer D3 not claimed");

            // Gunner has 0 points and has claimed nothing
            Check(PermanentProgression.GetAvailablePoints("gunner") == 0, "Isolation: Gunner has 0 points");
            Check(!PermanentProgression.IsDungeonRewardClaimed("gunner", "dungeon_1"), "Isolation: Gunner D1 not claimed");
            Check(!PermanentProgression.IsDungeonRewardClaimed("gunner", "dungeon_2"), "Isolation: Gunner D2 not claimed");
            Check(!PermanentProgression.IsDungeonRewardClaimed("gunner", "dungeon_3"), "Isolation: Gunner D3 not claimed");

            // Archer clears D1 for the first time
            bool archerD1 = PermanentProgression.TryAwardDungeonFirstClear("archer", "dungeon_1", out int archerPts);
            Check(archerD1, "Archer first clear of D1 succeeded");
            Check(archerPts == 2, "Archer received 2 points for D1");
            Check(PermanentProgression.GetAvailablePoints("archer") == 2, "Archer total points == 2");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 7, "Warrior points remain unchanged at 7");
            Check(PermanentProgression.GetAvailablePoints("gunner") == 0, "Gunner points remain unchanged at 0");

            // Gunner clears D3 for the first time
            bool gunnerD3 = PermanentProgression.TryAwardDungeonFirstClear("gunner", "dungeon_3", out int gunnerPts);
            Check(gunnerD3, "Gunner first clear of D3 succeeded");
            Check(gunnerPts == 3, "Gunner received 3 points for D3");
            Check(PermanentProgression.GetAvailablePoints("gunner") == 3, "Gunner total points == 3");
            Check(PermanentProgression.GetAvailablePoints("archer") == 2, "Archer points remain unchanged at 2");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 7, "Warrior points remain unchanged at 7");
        }

        private void VerifyDefeatDoesNotAwardPoints()
        {
            int warriorPtsBefore = PermanentProgression.GetAvailablePoints("warrior");
            int archerPtsBefore = PermanentProgression.GetAvailablePoints("archer");

            var go = Own(new GameObject("Test_PlayerDefeat"));
            var defeatController = go.AddComponent<PlayerDefeatController>();

            var playerGo = Own(new GameObject("Test_Defeated_Player"));
            var health = playerGo.AddComponent<PlayerHealth>();
            var playable = playerGo.AddComponent<PlayableCharacter>();
            var charDef = Own(ScriptableObject.CreateInstance<CharacterDefinition>());
            typeof(CharacterDefinition).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(charDef, "archer");
            playable.SetCharacterDefinition(charDef);

            defeatController.BindPlayer(health);

            // Trigger player death
            defeatController.HandlePlayerDied();

            Check(defeatController.IsDefeated, "DefeatController: player marked defeated");
            Check(PermanentProgression.GetAvailablePoints("warrior") == warriorPtsBefore, "Defeat: Warrior points completely unchanged");
            Check(PermanentProgression.GetAvailablePoints("archer") == archerPtsBefore, "Defeat: Archer points completely unchanged");
            Check(!PermanentProgression.IsDungeonRewardClaimed("archer", "dungeon_2"), "Defeat: Archer D2 remains unclaimed");

            DestroyOwned(go);
            DestroyOwned(playerGo);
        }

        private void VerifyRetryDoesNotAwardPoints()
        {
            int warriorPtsBefore = PermanentProgression.GetAvailablePoints("warrior");
            int archerPtsBefore = PermanentProgression.GetAvailablePoints("archer");

            // Simulate defeat retry rollback
            RunProgressionSession.RestoreCheckpointOnRetry();

            Check(PermanentProgression.GetAvailablePoints("warrior") == warriorPtsBefore, "Retry: Warrior points unchanged");
            Check(PermanentProgression.GetAvailablePoints("archer") == archerPtsBefore, "Retry: Archer points unchanged");
        }

        private void VerifyReturnToMapDoesNotAwardPoints()
        {
            int warriorPtsBefore = PermanentProgression.GetAvailablePoints("warrior");
            int archerPtsBefore = PermanentProgression.GetAvailablePoints("archer");

            // Simulate defeat return to map / end run
            RunProgressionSession.EndRun();

            Check(!RunProgressionSession.HasActiveRun, "RunProgressionSession run ended");
            Check(PermanentProgression.GetAvailablePoints("warrior") == warriorPtsBefore, "EndRun: Warrior points unchanged");
            Check(PermanentProgression.GetAvailablePoints("archer") == archerPtsBefore, "EndRun: Archer points unchanged");
        }

        private void VerifyRunXPAbsolutelyDecoupled()
        {
            int warriorPtsBefore = PermanentProgression.GetAvailablePoints("warrior");

            var expGo = Own(new GameObject("Test_RunXP"));
            var exp = expGo.AddComponent<PlayerExperience>();

            Check(exp.Level == 1, "PlayerExperience initial level == 1");
            exp.GainExperience(500);
            Check(exp.Level > 1, "PlayerExperience leveled up to " + exp.Level);
            Check(exp.TotalXPEarned == 500, "PlayerExperience TotalXPEarned == 500");

            Check(PermanentProgression.GetAvailablePoints("warrior") == warriorPtsBefore, "Run XP: Warrior permanent points completely unchanged");

            DestroyOwned(expGo);
        }

        private void VerifyPersistenceAcrossReload()
        {
            // Simulate domain reload by clearing in-memory cache and forcing reload from PlayerPrefs
            var method = typeof(PermanentProgression).GetMethod("ResetStaticState", BindingFlags.NonPublic | BindingFlags.Static);
            method?.Invoke(null, null);

            // Now read back from PlayerPrefs
            Check(PermanentProgression.GetAvailablePoints("warrior") == 7, "Persistence: Warrior has 7 points after simulated reload");
            Check(PermanentProgression.GetAvailablePoints("archer") == 2, "Persistence: Archer has 2 points after simulated reload");
            Check(PermanentProgression.GetAvailablePoints("gunner") == 3, "Persistence: Gunner has 3 points after simulated reload");

            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_1"), "Persistence: Warrior claimed D1 persisted");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_2"), "Persistence: Warrior claimed D2 persisted");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_3"), "Persistence: Warrior claimed D3 persisted");

            Check(PermanentProgression.IsDungeonRewardClaimed("archer", "dungeon_1"), "Persistence: Archer claimed D1 persisted");
            Check(!PermanentProgression.IsDungeonRewardClaimed("archer", "dungeon_2"), "Persistence: Archer D2 unclaimed persisted");
            Check(!PermanentProgression.IsDungeonRewardClaimed("archer", "dungeon_3"), "Persistence: Archer D3 unclaimed persisted");

            Check(PermanentProgression.IsDungeonRewardClaimed("gunner", "dungeon_3"), "Persistence: Gunner claimed D3 persisted");
        }

        private void VerifyGlobalDungeonProgressionSeparation()
        {
            // Global dungeon progression
            DungeonProgression.RecordDungeonCompleted("dungeon_1");
            DungeonProgression.RecordDungeonCompleted("dungeon_2");
            DungeonProgression.RecordDungeonCompleted("dungeon_3");

            Check(DungeonProgression.IsDungeonCompleted("dungeon_1"), "Global: D1 completed in DungeonProgression");
            Check(DungeonProgression.IsDungeonCompleted("dungeon_2"), "Global: D2 completed in DungeonProgression");
            Check(DungeonProgression.IsDungeonCompleted("dungeon_3"), "Global: D3 completed in DungeonProgression");

            // Resetting permanent progression does not affect global dungeon progression
            PermanentProgression.ResetAllProgression();
            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "PermanentProgression reset: Warrior points == 0");
            Check(DungeonProgression.IsDungeonCompleted("dungeon_1"), "Global: D1 still completed in DungeonProgression after permanent reset");
            Check(DungeonProgression.IsDungeonCompleted("dungeon_2"), "Global: D2 still completed in DungeonProgression after permanent reset");
            Check(DungeonProgression.IsDungeonCompleted("dungeon_3"), "Global: D3 still completed in DungeonProgression after permanent reset");

            // Resetting global dungeon progression does not affect permanent progression
            PermanentProgression.AddSkillPoints("warrior", 5);
            DungeonProgression.ResetProgression();
            Check(DungeonProgression.CompletedDungeonIds.Count == 0, "Global: DungeonProgression cleared");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 5, "Permanent: Warrior points remain 5 after global reset");
        }

        private void VerifyFailSafeHandling()
        {
            // Null / empty IDs
            Check(!PermanentProgression.TryAwardDungeonFirstClear(null, "dungeon_1", out var p1), "FailSafe: null characterId returns false");
            Check(p1 == 0, "FailSafe: null characterId points == 0");

            Check(!PermanentProgression.TryAwardDungeonFirstClear("warrior", null, out var p2), "FailSafe: null dungeonId returns false");
            Check(p2 == 0, "FailSafe: null dungeonId points == 0");

            Check(!PermanentProgression.TryAwardDungeonFirstClear("", "", out var p3), "FailSafe: empty IDs return false");
            Check(p3 == 0, "FailSafe: empty IDs points == 0");

            Check(!PermanentProgression.IsDungeonRewardClaimed(null, "dungeon_1"), "FailSafe: IsDungeonRewardClaimed null character returns false");
            Check(!PermanentProgression.IsDungeonRewardClaimed("warrior", null), "FailSafe: IsDungeonRewardClaimed null dungeon returns false");

            // Completion controller with missing character and missing spawner
            var go = Own(new GameObject("Test_DungeonCompletion_NoCharacter"));
            var controller = go.AddComponent<DungeonCompletionController>();
            // FinalizeCompletion via HandleWaveManagerCompletion with no references resolves null without throwing
            controller.HandleWaveManagerCompletion();
            Check(controller.HasCompleted, "FailSafe: CompletionController handles null character gracefully without throwing");

            DestroyOwned(go);
        }
    }
}
#endif
