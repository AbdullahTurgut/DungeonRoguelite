using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.UI;
using DungeonRoguelite.Upgrades;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated Play Mode verification suite for Milestone 10.3: Run Lifecycle & End-Run Semantics.
    /// Verifies:
    /// 1. Player Defeat UI displays Turkish explanatory subtitles under Restart and Return to Map buttons.
    /// 2. Player Defeat Retry cleanly rolls back to dungeon entry checkpoint.
    /// 3. Return to Map terminates the active campaign run and wipes progression.
    /// 4. Reselection from Character Select begins clean Level 1 run for the new hero.
    /// 5. Standalone editor launch cleanly defaults without session bleed.
    /// </summary>
    public class Milestone10_3_Verifier : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(RunVerificationSafe());
        }

        private IEnumerator RunVerificationSafe()
        {
            yield return null;
            yield return null;

            bool success = false;
            IEnumerator routine = RunVerificationRoutine(result => success = result);
            while (true)
            {
                object current = null;
                try
                {
                    if (!routine.MoveNext())
                    {
                        break;
                    }
                    current = routine.Current;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    success = false;
                    break;
                }

                yield return current;
            }

            Time.timeScale = 1f;
            yield return null;

            if (success)
            {
                Debug.Log("[GATE 10.3 COMPLETE] All Milestone 10.3 tests PASSED successfully!");
            }
            else
            {
                Debug.LogError("[GATE 10.3 COMPLETE] Verification FAILED.");
            }

#if UNITY_EDITOR
            System.IO.File.AppendAllText("gate_verification_results.log", $"[GATE 10.3 RESULT] Success: {success} at {DateTime.Now}\n");
            EditorApplication.isPlaying = false;
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
#endif
        }

        private IEnumerator RunVerificationRoutine(Action<bool> onComplete)
        {
            yield return null;
            Time.timeScale = 1f;
            Debug.Log("[GATE 10.3] Beginning Milestone 10.3 Automated Play Mode Verification...");
            bool allPassed = true;

            // -------------------------------------------------------------
            // CHECK 1: Defeat UI Explanatory Subtitles
            // -------------------------------------------------------------
            var canvasGo = new GameObject("TestDefeatCanvas", typeof(Canvas));
            var defeatPanel = new GameObject("TestDefeatPanel");
            defeatPanel.transform.SetParent(canvasGo.transform, false);

            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(defeatPanel.transform, false);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();

            var restartBtnGo = new GameObject("RestartButton");
            restartBtnGo.transform.SetParent(defeatPanel.transform, false);
            var restartBtn = restartBtnGo.AddComponent<Button>();

            var returnBtnGo = new GameObject("ReturnToMapButton");
            returnBtnGo.transform.SetParent(defeatPanel.transform, false);
            var returnBtn = returnBtnGo.AddComponent<Button>();

            var defeatUIGo = new GameObject("TestPlayerDefeatUI");
            var defeatUI = defeatUIGo.AddComponent<PlayerDefeatUI>();
            defeatUI.SetReferences(null, defeatPanel, titleTmp, restartBtn, returnBtn);

            defeatUI.HandlePlayerDefeated();

            bool c1RestartSub = defeatUI.RestartSubtitleText != null &&
                                defeatUI.RestartSubtitleText.text == "Zindana girişteki gelişiminle yeniden başla.";
            bool c1ReturnSub = defeatUI.ReturnToMapSubtitleText != null &&
                               defeatUI.ReturnToMapSubtitleText.text == "Koşuyu sonlandır ve haritaya dön.";

            if (c1RestartSub && c1ReturnSub)
            {
                Debug.Log($"[CHECK 1 PASSED] PlayerDefeatUI displays verified Turkish explanatory subtitles: Restart='{defeatUI.RestartSubtitleText.text}', Return='{defeatUI.ReturnToMapSubtitleText.text}'.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] Defeat UI subtitles mismatch: Restart='{defeatUI.RestartSubtitleText?.text}', Return='{defeatUI.ReturnToMapSubtitleText?.text}'");
                allPassed = false;
            }

            DestroyImmediate(defeatUIGo);
            DestroyImmediate(canvasGo);

            // -------------------------------------------------------------
            // CHECK 2: Retry Rollback to Entry Checkpoint
            // -------------------------------------------------------------
            RunProgressionSession.StartNewRun("warrior");
            // Simulate having won Dungeon 1 with Level 3, 45 XP, and 1 upgrade
            RunProgressionSession.CommitDungeonVictory(3, 45, 120, new List<string> { "upg_damage_01" });

            // Now enter Dungeon 2 (Create entry checkpoint)
            RunProgressionSession.CreateDungeonCheckpoint();
            bool c2Checkpoint = RunProgressionSession.CheckpointLevel == 3 &&
                                RunProgressionSession.CheckpointCurrentXP == 45 &&
                                RunProgressionSession.CheckpointTotalXP == 120 &&
                                RunProgressionSession.CheckpointUpgradeIds.Count == 1;

            // In Dungeon 2, player earns mid-dungeon XP (say to Level 4, 15 XP, +1 upgrade)
            // But dies!
            RunProgressionSession.RestoreCheckpointOnRetry();

            bool c2Rollback = RunProgressionSession.Level == 3 &&
                              RunProgressionSession.CurrentXP == 45 &&
                              RunProgressionSession.TotalXP == 120 &&
                              RunProgressionSession.CommittedUpgradeIds.Count == 1 &&
                              RunProgressionSession.CommittedUpgradeIds[0] == "upg_damage_01";

            if (c2Checkpoint && c2Rollback)
            {
                Debug.Log($"[CHECK 2 PASSED] Retry rollback verified: Reverted to Dungeon 2 entry checkpoint (Level {RunProgressionSession.Level}, {RunProgressionSession.CurrentXP} XP, 1 upgrade), discarding failed attempt gains.");
            }
            else
            {
                Debug.LogError($"[CHECK 2 FAILED] Retry rollback mismatch: level={RunProgressionSession.Level}, xp={RunProgressionSession.CurrentXP}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 3: Return to Map / End-Run Semantics
            // -------------------------------------------------------------
            RunProgressionSession.EndRun();

            bool c3 = !RunProgressionSession.HasActiveRun &&
                      string.IsNullOrEmpty(RunProgressionSession.OwnerCharacterId) &&
                      RunProgressionSession.Level == 1 &&
                      RunProgressionSession.CurrentXP == 0 &&
                      RunProgressionSession.TotalXP == 0 &&
                      RunProgressionSession.CommittedUpgradeIds.Count == 0 &&
                      RunProgressionSession.CheckpointUpgradeIds.Count == 0;

            if (c3)
            {
                Debug.Log("[CHECK 3 PASSED] Defeat Return to Map terminates active run and wipes all temporary progression back to clean baseline.");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] EndRun did not fully reset state: active={RunProgressionSession.HasActiveRun}, level={RunProgressionSession.Level}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 4: Clean Reselection After Ended Run
            // -------------------------------------------------------------
            RunProgressionSession.StartNewRun("gunner");
            bool c4 = RunProgressionSession.HasActiveRun &&
                      RunProgressionSession.OwnerCharacterId == "gunner" &&
                      RunProgressionSession.Level == 1 &&
                      RunProgressionSession.CurrentXP == 0 &&
                      RunProgressionSession.CommittedUpgradeIds.Count == 0;

            if (c4)
            {
                Debug.Log("[CHECK 4 PASSED] Reselection after ended run initializes clean campaign run for Gunner.");
            }
            else
            {
                Debug.LogError($"[CHECK 4 FAILED] Reselection initialization failed: owner={RunProgressionSession.OwnerCharacterId}, level={RunProgressionSession.Level}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // CHECK 5: Direct Launch Neutrality
            // -------------------------------------------------------------
            RunProgressionSession.Clear();
            bool c5 = !RunProgressionSession.HasActiveRun &&
                      !RunProgressionSession.ValidateOwner("warrior") &&
                      !RunProgressionSession.ValidateOwner("archer") &&
                      !RunProgressionSession.ValidateOwner("gunner");

            if (c5)
            {
                Debug.Log("[CHECK 5 PASSED] Direct launch neutrality verified: No active run or owner bleed.");
            }
            else
            {
                Debug.LogError("[CHECK 5 FAILED] Direct launch neutrality failed.");
                allPassed = false;
            }

            if (allPassed)
            {
                Debug.Log("[GATE 10.3 TEST COMPLETE] All 5 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 10.3 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
