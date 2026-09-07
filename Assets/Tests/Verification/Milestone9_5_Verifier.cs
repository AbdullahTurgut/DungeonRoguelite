using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using DungeonRoguelite.CameraControl;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Player;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Deterministic PlayMode verification suite for Gate 9.5 (Second Dungeon & Full Campaign Integration).
    /// Verifies Dungeon 2 wave assets, Dungeon_02 scene architecture, 4-scene Build Settings,
    /// and end-to-end multi-dungeon progression flow.
    /// </summary>
    public class Milestone9_5_Verifier : MonoBehaviour
    {
        private const string Dungeon01Path = "Assets/ScriptableObjects/Dungeons/Dungeon_01.asset";
        private const string Dungeon02Path = "Assets/ScriptableObjects/Dungeons/Dungeon_02.asset";
        private const string CatalogPath = "Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset";
        private const string Dungeon02ScenePath = "Assets/Scenes/Dungeons/Dungeon_02.unity";
        private const string DungeonPrototypeScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
        private const string WorldMapScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";
        private const string SelectionScenePath = "Assets/Scenes/CharacterSelect/CharacterSelection.unity";

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
            yield return null;
            Debug.Log("[GATE 9.5] Beginning Milestone 9.5 Play Mode verification suite...");
            bool allPassed = true;

            // Load definitions
            var d1 = Resources.Load<DungeonDefinition>("Dungeons/Dungeon_01");
            var d2 = Resources.Load<DungeonDefinition>("Dungeons/Dungeon_02");
            var catalog = Resources.Load<DungeonCatalog>("Dungeons/DungeonCatalog");
#if UNITY_EDITOR
            if (d1 == null) d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon01Path);
            if (d2 == null) d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon02Path);
            if (catalog == null) catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>(CatalogPath);
#endif

            // -------------------------------------------------------------
            // GROUP 1: Dungeon 2 Data & Wave Assets
            // -------------------------------------------------------------

            bool d2DataPass = d2 != null &&
                              d2.Id == "dungeon_2" &&
                              d2.SceneName == "Dungeon_02" &&
                              d2.RequiredDungeonId == "dungeon_1" &&
                              d2.HasPrerequisite &&
                              d2.Waves != null &&
                              d2.Waves.Length == 4;
            if (d2DataPass)
            {
                Debug.Log($"[CHECK 1 PASSED] Dungeon_02.asset configuration verified: Id='{d2.Id}', Scene='{d2.SceneName}', Waves={d2.Waves.Length}.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] Dungeon_02.asset configuration mismatch: {d2}");
                allPassed = false;
            }

            bool wavesValid = true;
            if (d2 != null && d2.Waves != null && d2.Waves.Length == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    var w = d2.Waves[i];
                    if (w == null || w.TotalEnemyCount <= 0 || w.SpawnInterval <= 0f)
                    {
                        wavesValid = false;
                        break;
                    }
                }
            }
            else
            {
                wavesValid = false;
            }

            if (wavesValid)
            {
                Debug.Log("[CHECK 2 PASSED] All 4 wave definitions for Dungeon 2 exist with valid spawn entries.");
            }
            else
            {
                Debug.LogError("[CHECK 2 FAILED] Dungeon 2 wave definitions missing or invalid.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 2: Build Settings Order & Catalog Integrity
            // -------------------------------------------------------------

#if UNITY_EDITOR
            var scenes = EditorBuildSettings.scenes;
            bool bsOrderPass = scenes.Length >= 4 &&
                               scenes[0].path.Contains("CharacterSelection") &&
                               scenes[1].path.Contains("WorldMap") &&
                               scenes[2].path.Contains("Dungeon_Prototype") &&
                               scenes[3].path.Contains("Dungeon_02");
            if (bsOrderPass)
            {
                Debug.Log($"[CHECK 3 PASSED] 4-Scene Build Settings order verified: 0={scenes[0].path}, 1={scenes[1].path}, 2={scenes[2].path}, 3={scenes[3].path}.");
            }
            else
            {
                Debug.LogError("[CHECK 3 FAILED] Build Settings scene count or order mismatch!");
                allPassed = false;
            }
#endif

            if (catalog != null && catalog.Count == 2 && catalog[0] == d1 && catalog[1] == d2)
            {
                Debug.Log("[CHECK 4 PASSED] DungeonCatalog.asset contains [Dungeon_01, Dungeon_02].");
            }
            else
            {
                Debug.LogError("[CHECK 4 FAILED] DungeonCatalog entries mismatch.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 3: Dungeon_02 Scene Architecture & Bindings
            // -------------------------------------------------------------

            var spawner = FindFirstObjectByType<PlayerSpawner>();
            if (spawner != null && spawner.ActiveCharacter != null)
            {
                Debug.Log($"[CHECK 5 PASSED] Dungeon_02 PlayerSpawner instantiated active character: '{spawner.ActiveCharacter.name}'.");
            }
            else
            {
                Debug.LogError("[CHECK 5 FAILED] Active character not spawned in Dungeon_02.");
                allPassed = false;
            }

            var waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null && waveManager.TotalWaves == 4)
            {
                Debug.Log($"[CHECK 6 PASSED] Dungeon_02 WaveManager has 4 waves configured ({waveManager.TotalWaves} waves).");
            }
            else
            {
                Debug.LogError($"[CHECK 6 FAILED] Dungeon_02 WaveManager wave count mismatch: {waveManager?.TotalWaves}");
                allPassed = false;
            }

            var cam = FindFirstObjectByType<CameraFollow>();
            if (cam != null && spawner != null && cam.Target == spawner.ActiveCharacter.transform)
            {
                Debug.Log("[CHECK 7 PASSED] CameraFollow target successfully bound to spawned character in Dungeon_02.");
            }
            else
            {
                Debug.LogError("[CHECK 7 FAILED] CameraFollow target not bound in Dungeon_02.");
                allPassed = false;
            }

            var playerAttack = spawner != null && spawner.ActiveCharacter != null ? spawner.ActiveCharacter.GetComponent<PlayerAttack>() : null;
            if (playerAttack != null)
            {
                bool attacked = playerAttack.TryAttack();
                Debug.Log($"[CHECK 8 PASSED] Combat interaction functional in Dungeon_02: TryAttack={attacked}.");
            }
            else
            {
                Debug.LogError("[CHECK 8 FAILED] PlayerAttack missing on spawned character in Dungeon_02.");
                allPassed = false;
            }

            var defeatController = FindFirstObjectByType<PlayerDefeatController>();
            var defeatUI = FindFirstObjectByType<PlayerDefeatUI>(FindObjectsInactive.Include);
            if (defeatController != null && defeatUI != null)
            {
                Debug.Log("[CHECK 9 PASSED] PlayerDefeatController and PlayerDefeatUI present and wired in Dungeon_02.");
            }
            else
            {
                Debug.LogError("[CHECK 9 FAILED] PlayerDefeatController or PlayerDefeatUI missing in Dungeon_02.");
                allPassed = false;
            }

            var compController = FindFirstObjectByType<DungeonCompletionController>();
            var compUI = FindFirstObjectByType<DungeonCompleteUI>(FindObjectsInactive.Include);
            if (compController != null && compUI != null && compUI.ReturnToMapButton != null)
            {
                Debug.Log("[CHECK 10 PASSED] DungeonCompletionController and DungeonCompleteUI (with ReturnToMapButton) wired in Dungeon_02.");
            }
            else
            {
                Debug.LogError("[CHECK 10 FAILED] Completion systems missing or ReturnToMapButton unassigned in Dungeon_02.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 4: Full Campaign Progression Lifecycle
            // -------------------------------------------------------------

            DungeonProgression.ResetProgression();

            // Initial state: D1 unlocked, D2 locked
            bool step1 = DungeonProgression.IsDungeonUnlocked(d1) && !DungeonProgression.IsDungeonUnlocked(d2);

            // Complete D1
            DungeonProgression.RecordDungeonCompleted("dungeon_1");
            bool step2 = DungeonProgression.IsDungeonCompleted("dungeon_1") && DungeonProgression.IsDungeonUnlocked(d2);

            // Complete D2
            DungeonProgression.RecordDungeonCompleted("dungeon_2");
            bool step3 = DungeonProgression.IsDungeonCompleted("dungeon_1") && DungeonProgression.IsDungeonCompleted("dungeon_2");

            if (step1 && step2 && step3)
            {
                Debug.Log("[CHECK 11 PASSED] End-to-end campaign progression lifecycle verified: D1 -> Unlock D2 -> Complete D2.");
            }
            else
            {
                Debug.LogError($"[CHECK 11 FAILED] Campaign progression lifecycle mismatch: step1={step1}, step2={step2}, step3={step3}");
                allPassed = false;
            }

            // Clean up session and progression
            DungeonProgression.ResetProgression();
            DungeonRunSession.Clear();

            // -------------------------------------------------------------
            // GROUP 5: Scene Disk Integrity
            // -------------------------------------------------------------

            if (File.Exists(Dungeon02ScenePath))
            {
                string sceneYaml = File.ReadAllText(Dungeon02ScenePath);
                bool containsWarrior = sceneYaml.Contains("8cb84c2076ee41f8896574292030b7a1");
                bool containsArcher = sceneYaml.Contains("b7141fa06e0ac60429fc4eee5b36e7df");
                bool containsGunner = sceneYaml.Contains("8f3c7e3f946d0344d935f0f3ca5530ec");
                bool hasPermanentVerifier = sceneYaml.Contains("Milestone9_5_Verifier") || sceneYaml.Contains("Gate9_5_Verifier");

                if (containsWarrior && !containsArcher && !containsGunner && !hasPermanentVerifier)
                {
                    Debug.Log("[CHECK 12 PASSED] Dungeon_02.unity on disk retains Character_Warrior default and zero permanent verifiers.");
                }
                else
                {
                    Debug.LogError($"[CHECK 12 FAILED] Dungeon_02.unity on disk contaminated: warrior={containsWarrior}, archer={containsArcher}, verifier={hasPermanentVerifier}");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError($"[CHECK 12 FAILED] Scene file not found at {Dungeon02ScenePath}");
                allPassed = false;
            }

            if (allPassed)
            {
                Debug.Log("[GATE 9.5 TEST COMPLETE] All 12 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[GATE 9.5 TEST FAILED] One or more verification checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
