using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Player;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Deterministic PlayMode verification suite for Gate 9.2 (Complete Run Lifecycle & Defeat Flow).
    /// Verifies player death lifecycle, defeat UI, pause state, wave halting, mutual exclusion, and scene integrity.
    /// </summary>
    public class Milestone9_2_Verifier : MonoBehaviour
    {
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

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
            yield return StartCoroutine(RunVerificationRoutine((result) => success = result));

            // Restore timeScale before exiting playmode
            Time.timeScale = 1f;

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(success ? 0 : 1);
#endif
        }

        private IEnumerator RunVerificationRoutine(Action<bool> onComplete)
        {
            Debug.Log("[GATE 9.2] Beginning Milestone 9.2 Play Mode verification suite...");
            bool allPassed = true;

            // -------------------------------------------------------------
            // GROUP 1: Component Wiring & Initial State
            // -------------------------------------------------------------

            var defeatController = FindFirstObjectByType<PlayerDefeatController>();
            if (defeatController != null && defeatController.gameObject.name == "RunControllers")
            {
                Debug.Log("[CHECK 1 PASSED] PlayerDefeatController resolved on RunControllers GameObject.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] PlayerDefeatController missing or not on RunControllers: {defeatController}");
                allPassed = false;
            }

            if (defeatController != null && defeatController.WaveManager != null && defeatController.PlayerSpawner != null && defeatController.CompletionController != null)
            {
                Debug.Log("[CHECK 2 PASSED] PlayerDefeatController references (WaveManager, PlayerSpawner, CompletionController) wired.");
            }
            else
            {
                Debug.LogError("[CHECK 2 FAILED] PlayerDefeatController references unassigned.");
                allPassed = false;
            }

            var defeatUI = FindFirstObjectByType<PlayerDefeatUI>(FindObjectsInactive.Include);
            if (defeatUI != null && defeatUI.PanelRoot != null && defeatUI.TitleText != null && defeatUI.RestartButton != null && defeatUI.ReturnToMapButton != null)
            {
                Debug.Log("[CHECK 3 PASSED] PlayerDefeatUI resolved with PanelRoot, TitleText, RestartButton, and ReturnToMapButton.");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] PlayerDefeatUI missing or unassigned references: {defeatUI}");
                allPassed = false;
            }

            if (defeatController != null && !defeatController.IsDefeated && defeatUI != null && !defeatUI.PanelRoot.activeSelf)
            {
                Debug.Log("[CHECK 4 PASSED] Initial state clean: IsDefeated is false and defeat panel is hidden.");
            }
            else
            {
                Debug.LogError($"[CHECK 4 FAILED] Initial state contaminated: IsDefeated={defeatController?.IsDefeated}, PanelActive={defeatUI?.PanelRoot.activeSelf}");
                allPassed = false;
            }

            var completeUI = FindFirstObjectByType<DungeonCompleteUI>(FindObjectsInactive.Include);
            if (completeUI != null && completeUI.ReturnToMapButton != null)
            {
                Debug.Log("[CHECK 5 PASSED] DungeonCompleteUI has ReturnToMapButton assigned.");
            }
            else
            {
                Debug.LogError($"[CHECK 5 FAILED] DungeonCompleteUI missing ReturnToMapButton: {completeUI}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 2: Spawn Resolution & Player Binding
            // -------------------------------------------------------------

            var spawner = defeatController != null ? defeatController.PlayerSpawner : FindFirstObjectByType<PlayerSpawner>();
            if (spawner != null && spawner.ActiveCharacter != null)
            {
                Debug.Log($"[CHECK 6 PASSED] PlayerSpawner spawned active character: '{spawner.ActiveCharacter.name}'.");
            }
            else
            {
                Debug.LogError("[CHECK 6 FAILED] Active character not spawned by PlayerSpawner.");
                allPassed = false;
            }

            PlayerHealth playerHealth = spawner != null && spawner.ActiveCharacter != null ? spawner.ActiveCharacter.GetComponent<PlayerHealth>() : null;
            if (playerHealth != null && playerHealth.CurrentHealth > 0f && !playerHealth.IsDead)
            {
                Debug.Log($"[CHECK 7 PASSED] PlayerHealth resolved on active character: HP={playerHealth.CurrentHealth}/{playerHealth.MaxHealth}.");
            }
            else
            {
                Debug.LogError($"[CHECK 7 FAILED] PlayerHealth missing or not alive: {playerHealth}");
                allPassed = false;
            }

            if (defeatController != null && defeatController.BoundPlayerHealth == playerHealth)
            {
                Debug.Log("[CHECK 8 PASSED] PlayerDefeatController.BoundPlayerHealth is bound to spawned character PlayerHealth.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] PlayerDefeatController bound health mismatch: {defeatController?.BoundPlayerHealth} vs {playerHealth}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 3: Defeat Execution & State Transitions
            // -------------------------------------------------------------

            // Inflict lethal damage to player
            playerHealth.TakeDamage(99999f);
            yield return null;

            if (playerHealth.IsDead && playerHealth.CurrentHealth <= 0f)
            {
                Debug.Log("[CHECK 9 PASSED] PlayerHealth successfully transitioned to dead state (HP=0, IsDead=True).");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] PlayerHealth did not die: HP={playerHealth.CurrentHealth}, IsDead={playerHealth.IsDead}");
                allPassed = false;
            }

            if (defeatController != null && defeatController.IsDefeated)
            {
                Debug.Log("[CHECK 10 PASSED] PlayerDefeatController.IsDefeated transitioned to true.");
            }
            else
            {
                Debug.LogError("[CHECK 10 FAILED] PlayerDefeatController.IsDefeated is false!");
                allPassed = false;
            }

            if (Time.timeScale == 0f)
            {
                Debug.Log("[CHECK 11 PASSED] Time.timeScale authoritative pause applied (0.0).");
            }
            else
            {
                Debug.LogError($"[CHECK 11 FAILED] Time.timeScale is not 0: {Time.timeScale}");
                allPassed = false;
            }

            var waveManager = defeatController != null ? defeatController.WaveManager : FindFirstObjectByType<WaveManager>();
            if (waveManager != null && waveManager.CurrentState == WaveState.Inactive)
            {
                Debug.Log("[CHECK 12 PASSED] WaveManager halted and transitioned to Inactive state.");
            }
            else
            {
                Debug.LogError($"[CHECK 12 FAILED] WaveManager not inactive: {waveManager?.CurrentState}");
                allPassed = false;
            }

            if (defeatUI != null && defeatUI.PanelRoot.activeSelf)
            {
                Debug.Log("[CHECK 13 PASSED] PlayerDefeatUI PanelRoot activated upon defeat.");
            }
            else
            {
                Debug.LogError("[CHECK 13 FAILED] PlayerDefeatUI PanelRoot is not active!");
                allPassed = false;
            }

            if (defeatUI != null && defeatUI.TitleText != null && defeatUI.TitleText.text.Contains("YENİLDİN"))
            {
                Debug.Log($"[CHECK 14 PASSED] PlayerDefeatUI displays defeat header: '{defeatUI.TitleText.text}'.");
            }
            else
            {
                Debug.LogError($"[CHECK 14 FAILED] PlayerDefeatUI header incorrect: '{defeatUI?.TitleText?.text}'");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 4: Mutual Exclusion & Disk Cleanliness
            // -------------------------------------------------------------

            var completionController = defeatController != null ? defeatController.CompletionController : FindFirstObjectByType<DungeonCompletionController>();
            // Attempt to trigger victory while defeated
            if (completionController != null)
            {
                completionController.HandleWaveManagerCompletion();
                if (!completionController.HasCompleted && !completionController.IsCompletionFinished)
                {
                    Debug.Log("[CHECK 15 PASSED] Mutual exclusion verified: Defeat strictly suppresses DungeonCompletionController victory.");
                }
                else
                {
                    Debug.LogError("[CHECK 15 FAILED] DungeonCompletionController allowed victory while defeated!");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 15 FAILED] DungeonCompletionController not found for mutual exclusion test.");
                allPassed = false;
            }

            // CHECK 16: Scene disk integrity
            if (File.Exists(ScenePath))
            {
                string sceneYaml = File.ReadAllText(ScenePath);
                bool containsWarrior = sceneYaml.Contains("8cb84c2076ee41f8896574292030b7a1");
                bool containsArcher = sceneYaml.Contains("b7141fa06e0ac60429fc4eee5b36e7df");
                bool containsGunner = sceneYaml.Contains("8f3c7e3f946d0344d935f0f3ca5530ec");
                bool hasPermanentVerifier = sceneYaml.Contains("Milestone9_2_Verifier") || sceneYaml.Contains("Gate9_2_Verifier");

                if (containsWarrior && !containsArcher && !containsGunner && !hasPermanentVerifier)
                {
                    Debug.Log("[CHECK 16 PASSED] Dungeon_Prototype.unity on disk retains Character_Warrior default and zero permanent verifiers.");
                }
                else
                {
                    Debug.LogError($"[CHECK 16 FAILED] Dungeon_Prototype.unity on disk contaminated: warrior={containsWarrior}, archer={containsArcher}, verifier={hasPermanentVerifier}");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError($"[CHECK 16 FAILED] Scene file not found at {ScenePath}");
                allPassed = false;
            }

            // Report overall status
            if (allPassed)
            {
                Debug.Log("[GATE 9.2 TEST COMPLETE] All 16 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 9.2 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
