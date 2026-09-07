using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Player;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Deterministic PlayMode verification suite for Gate 9.3 (Campaign Progression & Persistence).
    /// Tests DungeonProgression API, PlayerPrefs persistence, prerequisite-derived unlock logic,
    /// DungeonCompletionController integration, and defeat suppression integrity.
    /// </summary>
    public class Milestone9_3_Verifier : MonoBehaviour
    {
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
        private const string Dungeon01Path = "Assets/ScriptableObjects/Dungeons/Dungeon_01.asset";
        private const string Dungeon02Path = "Assets/ScriptableObjects/Dungeons/Dungeon_02.asset";

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

            Time.timeScale = 1f;

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(success ? 0 : 1);
#endif
        }

        private IEnumerator RunVerificationRoutine(Action<bool> onComplete)
        {
            Debug.Log("[GATE 9.3] Beginning Milestone 9.3 Play Mode verification suite...");
            bool allPassed = true;

            // Load definitions
            var d1 = Resources.Load<DungeonDefinition>("Dungeons/Dungeon_01");
            var d2 = Resources.Load<DungeonDefinition>("Dungeons/Dungeon_02");
#if UNITY_EDITOR
            if (d1 == null) d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon01Path);
            if (d2 == null) d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon02Path);
#endif

            // -------------------------------------------------------------
            // GROUP 1: Progression Reset & Unlock Logic
            // -------------------------------------------------------------

            DungeonProgression.ResetProgression();
            if (DungeonProgression.CompletedDungeonIds.Count == 0)
            {
                Debug.Log("[CHECK 1 PASSED] DungeonProgression.ResetProgression successfully cleared all completed dungeons.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] ResetProgression failed to clear: count={DungeonProgression.CompletedDungeonIds.Count}");
                allPassed = false;
            }

            if (d1 != null && DungeonProgression.IsDungeonUnlocked(d1))
            {
                Debug.Log("[CHECK 2 PASSED] Dungeon_01 has no prerequisite and is unlocked by default.");
            }
            else
            {
                Debug.LogError("[CHECK 2 FAILED] Dungeon_01 is unexpectedly locked.");
                allPassed = false;
            }

            if (d2 != null && !DungeonProgression.IsDungeonUnlocked(d2))
            {
                Debug.Log("[CHECK 3 PASSED] Dungeon_02 requires prerequisite and is locked initially.");
            }
            else
            {
                Debug.LogError("[CHECK 3 FAILED] Dungeon_02 is unexpectedly unlocked before prerequisite completion.");
                allPassed = false;
            }

            if (!DungeonProgression.IsDungeonCompleted("dungeon_1"))
            {
                Debug.Log("[CHECK 4 PASSED] IsDungeonCompleted('dungeon_1') returns false initially.");
            }
            else
            {
                Debug.LogError("[CHECK 4 FAILED] IsDungeonCompleted('dungeon_1') returned true before recording.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 2: Recording, Event, and Derived Unlock
            // -------------------------------------------------------------

            string recordedEventId = null;
            Action<string> handler = (id) => recordedEventId = id;
            DungeonProgression.OnDungeonCompletedPersisted += handler;

            DungeonProgression.RecordDungeonCompleted("dungeon_1");

            DungeonProgression.OnDungeonCompletedPersisted -= handler;

            if (recordedEventId == "dungeon_1")
            {
                Debug.Log("[CHECK 5 PASSED] DungeonProgression.RecordDungeonCompleted fired OnDungeonCompletedPersisted event.");
            }
            else
            {
                Debug.LogError($"[CHECK 5 FAILED] Event not fired or wrong ID: '{recordedEventId}'");
                allPassed = false;
            }

            if (DungeonProgression.IsDungeonCompleted("dungeon_1"))
            {
                Debug.Log("[CHECK 6 PASSED] IsDungeonCompleted('dungeon_1') is now true.");
            }
            else
            {
                Debug.LogError("[CHECK 6 FAILED] IsDungeonCompleted('dungeon_1') is still false after recording.");
                allPassed = false;
            }

            if (d2 != null && DungeonProgression.IsDungeonUnlocked(d2))
            {
                Debug.Log("[CHECK 7 PASSED] Dungeon_02 derived unlock state transitioned to unlocked.");
            }
            else
            {
                Debug.LogError("[CHECK 7 FAILED] Dungeon_02 failed to unlock after prerequisite completion.");
                allPassed = false;
            }

            // Idempotency
            DungeonProgression.RecordDungeonCompleted("dungeon_1");
            if (DungeonProgression.CompletedDungeonIds.Count == 1)
            {
                Debug.Log("[CHECK 8 PASSED] Repeated RecordDungeonCompleted calls are idempotent (count=1).");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] Idempotency failed: count={DungeonProgression.CompletedDungeonIds.Count}");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 3: CompletionController Integration
            // -------------------------------------------------------------

            DungeonProgression.ResetProgression();
            var completionController = FindFirstObjectByType<DungeonCompletionController>();
            if (completionController != null)
            {
                DungeonRunSession.SetSelection(d1);
                completionController.HandleWaveManagerCompletion();

                if (completionController.HasCompleted && DungeonProgression.IsDungeonCompleted("dungeon_1") && DungeonProgression.IsDungeonUnlocked(d2))
                {
                    Debug.Log("[CHECK 9 PASSED] DungeonCompletionController.FinalizeCompletion successfully persisted dungeon_1 completion.");
                }
                else
                {
                    Debug.LogError($"[CHECK 9 FAILED] Completion integration failed: completed={completionController.HasCompleted}, d1_done={DungeonProgression.IsDungeonCompleted("dungeon_1")}");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 9 FAILED] DungeonCompletionController not found in scene.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 4: Defeat Suppression & Scene Cleanliness
            // -------------------------------------------------------------

            // Reset progression and test defeat suppression
            DungeonProgression.ResetProgression();
            if (completionController != null)
            {
                completionController.ResetCompletionForTesting();
            }

            var defeatController = FindFirstObjectByType<PlayerDefeatController>();
            if (defeatController != null)
            {
                // Reset defeat controller for test
                defeatController.ResetDefeatForTesting();

                // Inflict lethal damage to player
                var spawner = defeatController.PlayerSpawner;
                var playerHealth = spawner != null && spawner.ActiveCharacter != null ? spawner.ActiveCharacter.GetComponent<PlayerHealth>() : null;
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(99999f);
                    yield return null;

                    // Defeat is now active
                    if (defeatController.IsDefeated)
                    {
                        // Now attempt to fire completion
                        completionController.HandleWaveManagerCompletion();

                        if (!DungeonProgression.IsDungeonCompleted("dungeon_1") && !DungeonProgression.IsDungeonUnlocked(d2))
                        {
                            Debug.Log("[CHECK 10 PASSED] Defeat strictly suppresses progression persistence (dungeon_1 remains uncompleted, d2 remains locked).");
                        }
                        else
                        {
                            Debug.LogError("[CHECK 10 FAILED] Defeat failed to prevent progression persistence!");
                            allPassed = false;
                        }
                    }
                    else
                    {
                        Debug.LogError("[CHECK 10 FAILED] PlayerDefeatController is not defeated after lethal damage.");
                        allPassed = false;
                    }
                }
                else
                {
                    Debug.LogError("[CHECK 10 FAILED] PlayerHealth not found on active character.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 10 FAILED] PlayerDefeatController not found in scene.");
                allPassed = false;
            }

            // Clean up session and progression after test
            DungeonRunSession.Clear();
            DungeonProgression.ResetProgression();

            // CHECK 11: Scene disk integrity
            if (File.Exists(ScenePath))
            {
                string sceneYaml = File.ReadAllText(ScenePath);
                bool containsWarrior = sceneYaml.Contains("8cb84c2076ee41f8896574292030b7a1");
                bool containsArcher = sceneYaml.Contains("b7141fa06e0ac60429fc4eee5b36e7df");
                bool containsGunner = sceneYaml.Contains("8f3c7e3f946d0344d935f0f3ca5530ec");
                bool hasPermanentVerifier = sceneYaml.Contains("Milestone9_3_Verifier") || sceneYaml.Contains("Gate9_3_Verifier");

                if (containsWarrior && !containsArcher && !containsGunner && !hasPermanentVerifier)
                {
                    Debug.Log("[CHECK 11 PASSED] Dungeon_Prototype.unity on disk retains Character_Warrior default and zero permanent verifiers.");
                }
                else
                {
                    Debug.LogError($"[CHECK 11 FAILED] Scene file contaminated: warrior={containsWarrior}, archer={containsArcher}, verifier={hasPermanentVerifier}");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError($"[CHECK 11 FAILED] Scene file not found at {ScenePath}");
                allPassed = false;
            }

            if (allPassed)
            {
                Debug.Log("[GATE 9.3 TEST COMPLETE] All 11 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 9.3 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
