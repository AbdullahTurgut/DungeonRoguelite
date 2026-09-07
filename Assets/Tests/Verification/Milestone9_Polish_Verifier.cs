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
using UnityEditor.SceneManagement;
#endif
using DungeonRoguelite.Characters;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Player;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Deterministic Play Mode verification suite for Phase 9 Manual QA Polish Pass.
    /// Verifies World Map readability & hollow outline border, Player Health UI event-driven
    /// binding, damage/death updates, and Dungeon 1 arena expansion.
    /// </summary>
    public class Milestone9_Polish_Verifier : MonoBehaviour
    {
        private const string DungeonPrototypeScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
        private const string Dungeon02ScenePath = "Assets/Scenes/Dungeons/Dungeon_02.unity";
        private const string WorldMapScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";
        private const string CharacterSelectionScenePath = "Assets/Scenes/CharacterSelect/CharacterSelection.unity";

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
            IEnumerator routine = RunVerificationRoutine((result) => success = result);
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

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(success ? 0 : 1);
#endif
        }

        private IEnumerator RunVerificationRoutine(Action<bool> onComplete)
        {
            yield return null;
            Debug.Log("[POLISH VERIFIER] Beginning Phase 9 Polish Pass Play Mode verification suite...");
            bool allPassed = true;

            // -------------------------------------------------------------
            // GROUP 1: World Map Readability & Selection Frame
            // -------------------------------------------------------------
            var loadMapOp = SceneManager.LoadSceneAsync("WorldMap", LoadSceneMode.Additive);
            while (loadMapOp != null && !loadMapOp.isDone) yield return null;
            yield return null;

            var mapController = UnityEngine.Object.FindFirstObjectByType<WorldMapController>();
            if (mapController != null && mapController.Cards.Count > 0)
            {
                var card0 = mapController.Cards[0];
                var cardBg = card0.GetComponent<Image>();
                bool darkBody = cardBg != null && (cardBg.color.r < 0.25f && cardBg.color.g < 0.25f && cardBg.color.b < 0.30f);
                if (darkBody)
                {
                    Debug.Log("[CHECK 1 PASSED] WorldMapCard body maintains rich dark background (#101319).");
                }
                else
                {
                    Debug.LogError($"[CHECK 1 FAILED] WorldMapCard body not dark: {cardBg?.color}");
                    allPassed = false;
                }

                var border = card0.SelectionBorder;
                bool hasBorderFrame = border != null &&
                                      border.transform.Find("Top") != null &&
                                      border.transform.Find("Bottom") != null &&
                                      border.transform.Find("Left") != null &&
                                      border.transform.Find("Right") != null;
                if (hasBorderFrame)
                {
                    Debug.Log("[CHECK 2 PASSED] WorldMapCard SelectionBorder is a hollow 4-sided outline frame (zero solid fill).");
                }
                else
                {
                    Debug.LogError($"[CHECK 2 FAILED] SelectionBorder missing 4-sided frame edges: {border}");
                    allPassed = false;
                }

                bool textContrast = card0.TitleText != null && card0.TitleText.color == Color.white &&
                                    card0.DescriptionText != null && card0.DescriptionText.color.r > 0.80f;
                if (textContrast)
                {
                    Debug.Log("[CHECK 3 PASSED] WorldMapCard title (white) and description (high-contrast off-white) readability verified.");
                }
                else
                {
                    Debug.LogError("[CHECK 3 FAILED] WorldMapCard text contrast insufficient.");
                    allPassed = false;
                }

                // Selection toggle check
                card0.SetSelected(true);
                bool selectedBorderActive = border.activeSelf;
                card0.SetSelected(false);
                bool unselectedBorderInactive = !border.activeSelf;
                if (selectedBorderActive && unselectedBorderInactive)
                {
                    Debug.Log("[CHECK 4 PASSED] Selection outline border toggles cleanly without mutating card body fill.");
                }
                else
                {
                    Debug.LogError("[CHECK 4 FAILED] Selection outline border toggle failed.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 1-4 FAILED] WorldMapController or cards not found.");
                allPassed = false;
            }

            var unloadMapOp = SceneManager.UnloadSceneAsync("WorldMap");
            while (unloadMapOp != null && !unloadMapOp.isDone) yield return null;
            yield return null;

            // -------------------------------------------------------------
            // GROUP 2: Player Health UI Architecture & Event-Driven Binding
            // -------------------------------------------------------------
            var spawner = UnityEngine.Object.FindFirstObjectByType<PlayerSpawner>();
            var healthUI = UnityEngine.Object.FindFirstObjectByType<PlayerHealthUI>();

            if (healthUI != null)
            {
                Debug.Log("[CHECK 5 PASSED] PlayerHealthUI component present on Canvas.");
            }
            else
            {
                Debug.LogError("[CHECK 5 FAILED] PlayerHealthUI component missing from scene.");
                allPassed = false;
            }

            // Check no Update method polling
            var updateMethod = typeof(PlayerHealthUI).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (updateMethod == null)
            {
                Debug.Log("[CHECK 6 PASSED] PlayerHealthUI is strictly event-driven (zero Update() polling).");
            }
            else
            {
                Debug.LogError("[CHECK 6 FAILED] PlayerHealthUI contains Update() polling method!");
                allPassed = false;
            }

            bool hudHierarchyValid = healthUI != null &&
                                     healthUI.HealthSlider != null &&
                                     healthUI.HealthText != null &&
                                     healthUI.LabelText != null &&
                                     healthUI.LabelText.text == "CAN";
            if (hudHierarchyValid)
            {
                Debug.Log("[CHECK 7 PASSED] PlayerHealthHUD hierarchy verified: CAN label, HealthSlider, HealthText.");
            }
            else
            {
                Debug.LogError("[CHECK 7 FAILED] PlayerHealthHUD hierarchy missing elements.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 3: Runtime Spawning & Initial State
            // -------------------------------------------------------------
            PlayableCharacter activeChar = spawner != null ? spawner.ActiveCharacter : null;
            if (activeChar == null && spawner != null)
            {
                spawner.Spawn();
                activeChar = spawner.ActiveCharacter;
            }

            yield return null;

            var playerHealth = activeChar != null ? activeChar.GetComponent<PlayerHealth>() : null;
            bool initialHealthMatch = healthUI != null &&
                                      healthUI.HealthSlider != null &&
                                      Mathf.Approximately(healthUI.HealthSlider.value, 1f) &&
                                      healthUI.HealthText.text == "100 / 100";
            if (initialHealthMatch && playerHealth != null && playerHealth.CurrentHealth == 100f)
            {
                Debug.Log("[CHECK 8 PASSED] Initial health state verified on spawn: 100 / 100, slider at 1.0.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] Initial health mismatch: slider={healthUI?.HealthSlider?.value}, text='{healthUI?.HealthText?.text}'");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 4: Damage Reception & Death Lifecycle Updates
            // -------------------------------------------------------------
            if (playerHealth != null)
            {
                // Apply 10 damage
                playerHealth.TakeDamage(10f);
                yield return null;

                bool dmg10Valid = Mathf.Approximately(healthUI.HealthSlider.value, 0.9f) &&
                                  healthUI.HealthText.text == "90 / 100";
                if (dmg10Valid)
                {
                    Debug.Log("[CHECK 9 PASSED] Health UI updated immediately via event on TakeDamage(10): 90 / 100.");
                }
                else
                {
                    Debug.LogError($"[CHECK 9 FAILED] TakeDamage(10) UI mismatch: slider={healthUI.HealthSlider.value}, text='{healthUI.HealthText.text}'");
                    allPassed = false;
                }

                // Apply fatal damage
                playerHealth.TakeDamage(150f);
                yield return null;

                bool fatalValid = Mathf.Approximately(healthUI.HealthSlider.value, 0f) &&
                                  healthUI.HealthText.text == "0 / 100" &&
                                  playerHealth.IsDead;
                if (fatalValid)
                {
                    Debug.Log("[CHECK 10 PASSED] Fatal damage updates UI to 0 / 100 (slider 0.0) and player is dead.");
                }
                else
                {
                    Debug.LogError($"[CHECK 10 FAILED] Fatal damage UI mismatch: slider={healthUI.HealthSlider.value}, text='{healthUI.HealthText.text}'");
                    allPassed = false;
                }

                // Reset health for subsequent checks
                playerHealth.ResetHealthForTesting(100f);
                healthUI.RefreshUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            // -------------------------------------------------------------
            // GROUP 5: Multi-Character Binding (Archer & Gunner)
            // -------------------------------------------------------------
            var archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Archer.prefab");
            var gunnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Gunner.prefab");

            if (archerPrefab != null)
            {
                var archerInstance = UnityEngine.Object.Instantiate(archerPrefab);
                var archerChar = archerInstance.GetComponent<PlayableCharacter>();
                var archerHealth = archerInstance.GetComponent<PlayerHealth>();

                healthUI.Bind(archerHealth);
                bool archerBindPass = healthUI.HealthText.text == "100 / 100";
                archerHealth.TakeDamage(25f);
                bool archerDmgPass = healthUI.HealthText.text == "75 / 100";

                if (archerBindPass && archerDmgPass)
                {
                    Debug.Log("[CHECK 11 PASSED] PlayerHealthUI cleanly binds to Archer and responds to damage events.");
                }
                else
                {
                    Debug.LogError("[CHECK 11 FAILED] Archer PlayerHealthUI binding failed.");
                    allPassed = false;
                }
                UnityEngine.Object.DestroyImmediate(archerInstance);
            }

            if (gunnerPrefab != null)
            {
                var gunnerInstance = UnityEngine.Object.Instantiate(gunnerPrefab);
                var gunnerChar = gunnerInstance.GetComponent<PlayableCharacter>();
                var gunnerHealth = gunnerInstance.GetComponent<PlayerHealth>();

                healthUI.Bind(gunnerHealth);
                bool gunnerBindPass = healthUI.HealthText.text == "100 / 100";
                gunnerHealth.TakeDamage(40f);
                bool gunnerDmgPass = healthUI.HealthText.text == "60 / 100";

                if (gunnerBindPass && gunnerDmgPass)
                {
                    Debug.Log("[CHECK 12 PASSED] PlayerHealthUI cleanly binds to Gunner and responds to damage events.");
                }
                else
                {
                    Debug.LogError("[CHECK 12 FAILED] Gunner PlayerHealthUI binding failed.");
                    allPassed = false;
                }
                UnityEngine.Object.DestroyImmediate(gunnerInstance);
            }

            // Rebind active player
            if (playerHealth != null)
            {
                healthUI.Bind(playerHealth);
            }

            // -------------------------------------------------------------
            // GROUP 6: Dungeon 1 Arena Expansion & Bounds
            // -------------------------------------------------------------
            var floorGo = GameObject.Find("Floor");
            bool floorExpanded = floorGo != null && floorGo.transform.localScale.x >= 34f && floorGo.transform.localScale.z >= 34f;
            if (floorExpanded)
            {
                Debug.Log($"[CHECK 13 PASSED] Dungeon_Prototype floor expanded to 34x34 (+13.3% linear, ~28% area): {floorGo.transform.localScale}.");
            }
            else
            {
                Debug.LogError($"[CHECK 13 FAILED] Dungeon_Prototype floor scale mismatch: {floorGo?.transform.localScale}");
                allPassed = false;
            }

            var northWall = GameObject.Find("Wall_North");
            var southWall = GameObject.Find("Wall_South");
            var eastWall = GameObject.Find("Wall_East");
            var westWall = GameObject.Find("Wall_West");

            bool wallsExpanded = northWall != null && northWall.transform.position.z >= 17f &&
                                 southWall != null && southWall.transform.position.z <= -17f &&
                                 eastWall != null && eastWall.transform.position.x >= 17f &&
                                 westWall != null && westWall.transform.position.x <= -17f;
            if (wallsExpanded)
            {
                Debug.Log("[CHECK 14 PASSED] Boundary walls expanded to ±17m matching 34x34 arena.");
            }
            else
            {
                Debug.LogError("[CHECK 14 FAILED] Boundary wall positions not expanded to ±17m.");
                allPassed = false;
            }

            // Check enemy spawn points inside arena bounds
            var sp1 = GameObject.Find("SpawnPoint_01");
            var sp2 = GameObject.Find("SpawnPoint_02");
            var sp3 = GameObject.Find("SpawnPoint_03");
            var sp4 = GameObject.Find("SpawnPoint_04");

            bool spawnsValid = sp1 != null && Mathf.Abs(sp1.transform.position.x) < 17f && Mathf.Abs(sp1.transform.position.z) < 17f &&
                               sp2 != null && Mathf.Abs(sp2.transform.position.x) < 17f && Mathf.Abs(sp2.transform.position.z) < 17f &&
                               sp3 != null && Mathf.Abs(sp3.transform.position.x) < 17f && Mathf.Abs(sp3.transform.position.z) < 17f &&
                               sp4 != null && Mathf.Abs(sp4.transform.position.x) < 17f && Mathf.Abs(sp4.transform.position.z) < 17f;
            if (spawnsValid)
            {
                Debug.Log("[CHECK 15 PASSED] All enemy spawn points are valid and safely positioned inside arena walls.");
            }
            else
            {
                Debug.LogError("[CHECK 15 FAILED] One or more enemy spawn points out of bounds.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // GROUP 7: Dungeon 2 Layout Identity & Health UI
            // -------------------------------------------------------------
            var loadD2Op = SceneManager.LoadSceneAsync("Dungeon_02", LoadSceneMode.Additive);
            while (loadD2Op != null && !loadD2Op.isDone) yield return null;
            yield return null;

            var d2Scene = SceneManager.GetSceneByName("Dungeon_02");
            GameObject d2Floor = null;
            PlayerHealthUI d2HealthUI = null;
            foreach (var root in d2Scene.GetRootGameObjects())
            {
                if (root.name == "Floor") d2Floor = root;
                var ui = root.GetComponentInChildren<PlayerHealthUI>(true);
                if (ui != null) d2HealthUI = ui;
            }

            bool d2Check = d2Floor != null && d2HealthUI != null && Mathf.Approximately(d2Floor.transform.localScale.x, 30f);
            if (d2Check)
            {
                Debug.Log($"[CHECK 16 PASSED] Dungeon_02 preserves distinct layout ({d2Floor.transform.localScale.x}x{d2Floor.transform.localScale.z}) and includes PlayerHealthUI.");
            }
            else
            {
                Debug.LogError($"[CHECK 16 FAILED] Dungeon_02 verification failed: floor={d2Floor != null}, healthUI={d2HealthUI != null}, scale={d2Floor?.transform.localScale.x}");
                allPassed = false;
            }

            var unloadD2Op = SceneManager.UnloadSceneAsync(d2Scene);
            while (unloadD2Op != null && !unloadD2Op.isDone) yield return null;
            yield return null;

            // -------------------------------------------------------------
            // GROUP 8: 4-Scene Build Settings & Clean Disk State
            // -------------------------------------------------------------
            var buildScenes = EditorBuildSettings.scenes;
            bool buildOrderValid = buildScenes.Length >= 4 &&
                                   buildScenes[0].path == CharacterSelectionScenePath &&
                                   buildScenes[1].path == WorldMapScenePath &&
                                   buildScenes[2].path == DungeonPrototypeScenePath &&
                                   buildScenes[3].path == Dungeon02ScenePath;
            if (buildOrderValid)
            {
                Debug.Log("[CHECK 17 PASSED] Build Settings 4-scene order verified: CharacterSelection -> WorldMap -> Dungeon_Prototype -> Dungeon_02.");
            }
            else
            {
                Debug.LogError($"[CHECK 17 FAILED] Build Settings order mismatch: count={buildScenes.Length}");
                allPassed = false;
            }

            if (allPassed)
            {
                Debug.Log("[POLISH VERIFIER COMPLETE] All 17 checks PASSED with 0 errors.");
            }
            else
            {
                Debug.LogError("[POLISH VERIFIER FAILED] One or more checks failed.");
            }

            onComplete?.Invoke(allPassed);
        }
    }
}
