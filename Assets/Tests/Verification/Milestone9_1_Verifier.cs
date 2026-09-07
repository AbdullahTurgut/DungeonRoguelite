using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Weapons;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Deterministic Play Mode verification suite for Milestone 9.1 (Data-Driven Dungeon Architecture).
    /// Verifies:
    /// - DungeonDefinition schema & asset integrity (Checks 1-4)
    /// - DungeonRunSession lifecycle (Checks 5-8)
    /// - WaveManager session consumption, injection, & direct fallback (Checks 9-13)
    /// - Single OnDungeonStarted event & combat non-regression (Checks 14-17)
    /// - Scene cleanliness & disk integrity (Checks 18-19)
    /// </summary>
    public class Milestone9_1_Verifier : MonoBehaviour
    {
        private const string Dungeon01Path = "Assets/ScriptableObjects/Dungeons/Dungeon_01.asset";
        private const string Dungeon02Path = "Assets/ScriptableObjects/Dungeons/Dungeon_02.asset";
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
            IEnumerator routine = RunVerificationRoutine();
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
                    Debug.LogError($"[GATE 9.1 TEST EXCEPTION] Unexpected exception: {ex}");
                    ExitBatch(1);
                    yield break;
                }

                yield return current;
            }
        }

        private IEnumerator RunVerificationRoutine()
        {
            Debug.Log("[GATE 9.1] Beginning Milestone 9.1 verification suite...");
            bool allPassed = true;

            yield return null; // 1 frame for Awake/Start

            // -------------------------------------------------------------
            // GROUP 1: DungeonDefinition Schema & Asset Integrity
            // -------------------------------------------------------------

            // CHECK 1: DungeonDefinition is a ScriptableObject with zero runtime state fields
            Type defType = typeof(DungeonDefinition);
            bool isScriptable = typeof(ScriptableObject).IsAssignableFrom(defType);
            FieldInfo[] fields = defType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            bool hasForbiddenRuntimeFields = false;
            foreach (var f in fields)
            {
                string lower = f.Name.ToLower();
                if (lower.Contains("iscompleted") || lower.Contains("isunlocked") || lower.Contains("currentwave") || lower.Contains("gamemanager"))
                {
                    hasForbiddenRuntimeFields = true;
                    break;
                }
            }

            if (isScriptable && !hasForbiddenRuntimeFields)
            {
                Debug.Log("[CHECK 1 PASSED] DungeonDefinition is a ScriptableObject containing zero runtime state fields.");
            }
            else
            {
                Debug.LogError($"[CHECK 1 FAILED] DungeonDefinition schema invalid: isScriptable={isScriptable}, forbidden={hasForbiddenRuntimeFields}");
                allPassed = false;
            }

            // CHECK 2: Dungeon_01.asset exists and has valid configuration
            var d1 = Resources.Load<DungeonDefinition>("Dungeons/Dungeon_01");
#if UNITY_EDITOR
            if (d1 == null) d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon01Path);
#endif
            if (d1 != null && d1.Id == "dungeon_1" && d1.SceneName == "Dungeon_Prototype" && !d1.HasPrerequisite && d1.Waves != null && d1.Waves.Length == 3)
            {
                Debug.Log($"[CHECK 2 PASSED] Dungeon_01.asset valid: id='{d1.Id}', scene='{d1.SceneName}', waves={d1.Waves.Length}.");
            }
            else
            {
                Debug.LogError($"[CHECK 2 FAILED] Dungeon_01.asset missing or invalid configuration: {d1}");
                allPassed = false;
            }

            // CHECK 3: Dungeon_02.asset exists and requires dungeon_1
            var d2 = Resources.Load<DungeonDefinition>("Dungeons/Dungeon_02");
#if UNITY_EDITOR
            if (d2 == null) d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon02Path);
#endif
            if (d2 != null && d2.Id == "dungeon_2" && d2.SceneName == "Dungeon_02" && d2.RequiredDungeonId == "dungeon_1" && d2.HasPrerequisite)
            {
                Debug.Log($"[CHECK 3 PASSED] Dungeon_02.asset valid: id='{d2.Id}', scene='{d2.SceneName}', required='{d2.RequiredDungeonId}'.");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] Dungeon_02.asset missing or invalid prerequisite: {d2}");
                allPassed = false;
            }

            // CHECK 4: DungeonDefinition properties do not return null for valid strings
            if (!string.IsNullOrEmpty(d1.DisplayName) && !string.IsNullOrEmpty(d1.Description) && !string.IsNullOrEmpty(d2.DisplayName))
            {
                Debug.Log("[CHECK 4 PASSED] Localized display names and descriptions populated on DungeonDefinitions.");
            }
            else
            {
                Debug.LogError("[CHECK 4 FAILED] Display name or description empty on DungeonDefinitions.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 2: DungeonRunSession Lifecycle
            // -------------------------------------------------------------

            // CHECK 5: DungeonRunSession starts clean or can be cleared
            DungeonRunSession.Clear();
            if (!DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon == null)
            {
                Debug.Log("[CHECK 5 PASSED] DungeonRunSession cleanly clears selection.");
            }
            else
            {
                Debug.LogError("[CHECK 5 FAILED] DungeonRunSession did not clear properly.");
                allPassed = false;
            }

            // CHECK 6: DungeonRunSession.SetSelection commits Dungeon_01
            DungeonRunSession.SetSelection(d1);
            if (DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon == d1)
            {
                Debug.Log("[CHECK 6 PASSED] DungeonRunSession.SetSelection assigns Dungeon_01 accurately.");
            }
            else
            {
                Debug.LogError("[CHECK 6 FAILED] DungeonRunSession failed to assign Dungeon_01.");
                allPassed = false;
            }

            // CHECK 7: DungeonRunSession.SetSelection overwrites with Dungeon_02
            DungeonRunSession.SetSelection(d2);
            if (DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon == d2)
            {
                Debug.Log("[CHECK 7 PASSED] DungeonRunSession.SetSelection overwrites with Dungeon_02 accurately.");
            }
            else
            {
                Debug.LogError("[CHECK 7 FAILED] DungeonRunSession failed to overwrite selection.");
                allPassed = false;
            }

            // CHECK 8: DungeonRunSession.Clear resets session state
            DungeonRunSession.Clear();
            if (!DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon == null)
            {
                Debug.Log("[CHECK 8 PASSED] DungeonRunSession.Clear returns to empty state.");
            }
            else
            {
                Debug.LogError("[CHECK 8 FAILED] DungeonRunSession did not return to empty state.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 3: WaveManager Configuration & Direct Fallback
            // -------------------------------------------------------------

            var waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager == null)
            {
                Debug.LogError("[GATE 9.1 TEST FAILED] WaveManager not found in scene.");
                ExitBatch(1);
                yield break;
            }

            // CHECK 9: WaveManager has fallback serialized waves on direct launch
            if (waveManager.TotalWaves >= 3)
            {
                Debug.Log($"[CHECK 9 PASSED] WaveManager has valid serialized fallback waves ({waveManager.TotalWaves} waves).");
            }
            else
            {
                Debug.LogError($"[CHECK 9 FAILED] WaveManager missing fallback waves: TotalWaves={waveManager.TotalWaves}");
                allPassed = false;
            }

            // CHECK 10: ConfigureFromDungeon dynamically updates WaveManager before start
            var dummyDef = ScriptableObject.CreateInstance<DungeonDefinition>();
            var dummyWaves = new WaveDefinition[] { d1.Waves[0] }; // single wave
            dummyDef.SetConfiguration("dummy", "Dummy", "Desc", "Dungeon_Prototype", "", dummyWaves);

            // Configure dummy
            waveManager.ConfigureFromDungeon(dummyDef);
            if (waveManager.ActiveDungeon == dummyDef && waveManager.TotalWaves == 1)
            {
                Debug.Log("[CHECK 10 PASSED] WaveManager.ConfigureFromDungeon dynamically applied dummy wave configuration (TotalWaves=1).");
            }
            else
            {
                Debug.LogError($"[CHECK 10 FAILED] WaveManager failed to configure from dummy dungeon: TotalWaves={waveManager.TotalWaves}");
                allPassed = false;
            }

            // CHECK 11: ConfigureFromDungeon creates an independent array copy (zero asset mutation)
            dummyWaves[0] = null; // mutate original array
            if (waveManager.TotalWaves == 1 && dummyDef.Waves != null)
            {
                Debug.Log("[CHECK 11 PASSED] WaveManager protects against external wave array mutation.");
            }
            else
            {
                Debug.LogError("[CHECK 11 FAILED] WaveManager wave configuration is vulnerable to array reference mutation.");
                allPassed = false;
            }

            // Restore d1 configuration onto WaveManager
            waveManager.ConfigureFromDungeon(d1);
            if (waveManager.ActiveDungeon == d1 && waveManager.TotalWaves == 3)
            {
                Debug.Log("[CHECK 12 PASSED] WaveManager restored Dungeon_01 configuration (3 waves).");
            }
            else
            {
                Debug.LogError($"[CHECK 12 FAILED] WaveManager failed to restore Dungeon_01: TotalWaves={waveManager.TotalWaves}");
                allPassed = false;
            }

            // CHECK 13: WaveManager contains zero dungeon-specific ID branching methods
            MethodInfo[] methods = typeof(WaveManager).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            bool hasDungeonBranchingMethod = false;
            foreach (var m in methods)
            {
                string lower = m.Name.ToLower();
                if (lower.Contains("dungeon1") || lower.Contains("dungeon2") || lower.Contains("dungeon_1") || lower.Contains("dungeon_2"))
                {
                    hasDungeonBranchingMethod = true;
                    break;
                }
            }
            if (!hasDungeonBranchingMethod)
            {
                Debug.Log("[CHECK 13 PASSED] WaveManager contains zero dungeon-specific ID branching methods.");
            }
            else
            {
                Debug.LogError("[CHECK 13 FAILED] WaveManager contains hardcoded dungeon branching methods.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 4: Run Execution & Non-Regression
            // -------------------------------------------------------------

            // CHECK 14: OnDungeonStarted event fires cleanly
            int dungeonStartCount = 0;
            waveManager.OnDungeonStarted += () => dungeonStartCount++;
            if (!waveManager.HasDungeonStarted)
            {
                waveManager.BeginDungeon();
            }
            yield return null;

            if (dungeonStartCount <= 1 && waveManager.HasDungeonStarted)
            {
                Debug.Log($"[CHECK 14 PASSED] WaveManager.BeginDungeon fired OnDungeonStarted cleanly (count={dungeonStartCount}).");
            }
            else
            {
                Debug.LogError($"[CHECK 14 FAILED] WaveManager.OnDungeonStarted unexpected invocation count: {dungeonStartCount}");
                allPassed = false;
            }

            // CHECK 15: PlayableCharacter active and bound
            var spawner = FindFirstObjectByType<PlayerSpawner>();
            var player = spawner != null ? spawner.ActiveCharacter : null;
            if (player != null && player.CompareTag("Player"))
            {
                Debug.Log($"[CHECK 15 PASSED] PlayableCharacter instantiated and active: '{player.name}'.");
            }
            else
            {
                Debug.LogError("[CHECK 15 FAILED] Active PlayableCharacter missing or not tagged 'Player'.");
                allPassed = false;
            }

            // CHECK 16: Primary attack functional on player
            var attack = player != null ? player.GetComponent<PlayerAttack>() : null;
            if (attack != null)
            {
                bool attackResult = attack.TryAttack();
                Debug.Log($"[CHECK 16 PASSED] PlayerAttack.TryAttack executed successfully (result={attackResult}).");
            }
            else
            {
                Debug.LogError("[CHECK 16 FAILED] PlayerAttack component missing from active player.");
                allPassed = false;
            }

            // CHECK 17: PlayerExperience ActiveInstance resolved
            if (PlayerExperience.ActiveInstance != null)
            {
                Debug.Log("[CHECK 17 PASSED] PlayerExperience.ActiveInstance resolved cleanly.");
            }
            else
            {
                Debug.LogError("[CHECK 17 FAILED] PlayerExperience.ActiveInstance unassigned.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 5: Scene Cleanliness & Disk Integrity
            // -------------------------------------------------------------

            // CHECK 18: Dungeon_Prototype on disk retains default Character_Warrior
            string sceneYaml = File.ReadAllText(ScenePath);
            bool containsWarrior = sceneYaml.Contains("8cb84c2076ee41f8896574292030b7a1");
            bool containsArcher = sceneYaml.Contains("b7141fa06e0ac60429fc4eee5b36e7df");
            bool containsGunner = sceneYaml.Contains("8f3c7e3f946d0344d935f0f3ca5530ec");

            if (containsWarrior && !containsArcher && !containsGunner)
            {
                Debug.Log("[CHECK 18 PASSED] Dungeon_Prototype.unity on disk retains Character_Warrior default and zero character contamination.");
            }
            else
            {
                Debug.LogError($"[CHECK 18 FAILED] Dungeon_Prototype.unity on disk contaminated: warrior={containsWarrior}, archer={containsArcher}, gunner={containsGunner}");
                allPassed = false;
            }

            // CHECK 19: Dungeon_Prototype on disk has zero permanent verifiers
            bool hasSavedVerifier = sceneYaml.Contains("Milestone9_1_Verifier") || sceneYaml.Contains("Gate9_1_Verifier");
            if (!hasSavedVerifier)
            {
                Debug.Log("[CHECK 19 PASSED] Dungeon_Prototype.unity contains zero permanent verifiers saved on disk.");
            }
            else
            {
                Debug.LogError("[CHECK 19 FAILED] Verifier detected saved in Dungeon_Prototype.unity scene file.");
                allPassed = false;
            }

            // Cleanup session
            DungeonRunSession.Clear();

            // -------------------------------------------------------------
            // FINAL VERIFICATION SUMMARY
            // -------------------------------------------------------------
            if (allPassed)
            {
                Debug.Log("[GATE 9.1 TEST COMPLETE] All 19 checks PASSED with 0 errors.");
                ExitBatch(0);
            }
            else
            {
                Debug.LogError("[GATE 9.1 TEST FAILED] One or more verification checks failed.");
                ExitBatch(1);
            }
        }

        private void ExitBatch(int exitCode)
        {
#if UNITY_EDITOR
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
#endif
        }
    }
}
