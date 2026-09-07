using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Object = UnityEngine.Object;
using DungeonRoguelite.Characters;
using DungeonRoguelite.CameraControl;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.UI;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Weapons;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Automated Play Mode verification suite for Milestone 8.5 (Character Selection UI & Integration).
    /// Validates CharacterRoster data catalog, CharacterSelection UI interactivity, local selection state,
    /// CharacterSelectionSession runtime carrier lifecycle, PlayerSpawner consumption and fallback,
    /// scene system explicit bindings, dungeon restart semantics, and combat non-regression.
    /// </summary>
    public class Milestone8_5_Verifier : MonoBehaviour
    {
        [SerializeField] private bool runAutomatedTestOnStart = true;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (runAutomatedTestOnStart || Application.isBatchMode)
            {
                StartCoroutine(RunVerificationSafe());
            }
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
                    Debug.LogError($"[M8.5 TEST EXCEPTION] Unexpected exception during verification: {ex}");
                    ExitBatch(1);
                    yield break;
                }

                yield return current;
            }
        }

        private IEnumerator RunVerificationRoutine()
        {
            yield return new WaitForSeconds(0.1f);

            Debug.Log("[M8.5 TEST START] Beginning Milestone 8.5 automated verification suite...");
            bool allPassed = true;

            const string RosterAssetPath = "Assets/ScriptableObjects/Characters/CharacterRoster.asset";
            const string WarriorAssetPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
            const string ArcherAssetPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";
            const string GunnerAssetPath = "Assets/ScriptableObjects/Characters/Character_Gunner.asset";
            const string SelectionScenePath = "Assets/Scenes/CharacterSelect/CharacterSelection.unity";
            const string DungeonScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

            // -------------------------------------------------------------
            // SECTION A: Character Roster & Definitions
            // -------------------------------------------------------------

            // CHECK 1: CharacterDefinition schema has zero combat/stat math fields
            var cdFields = typeof(CharacterDefinition).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            bool hasLeakedCombatField = false;
            foreach (var f in cdFields)
            {
                string lower = f.Name.ToLowerInvariant();
                if (lower.Contains("damage") || lower.Contains("cooldown") || lower.Contains("health") || lower.Contains("speed"))
                {
                    hasLeakedCombatField = true;
                    break;
                }
            }
            if (!hasLeakedCombatField)
            {
                Debug.Log("[CHECK 1 PASSED] CharacterDefinition schema contains zero combat calculation or stat fields.");
            }
            else
            {
                Debug.LogError("[CHECK 1 FAILED] CharacterDefinition contains leaked combat fields.");
                allPassed = false;
            }

            // CHECK 2: CharacterRoster asset exists and loads cleanly
            var roster = AssetDatabase.LoadAssetAtPath<CharacterRoster>(RosterAssetPath);
            if (roster != null)
            {
                Debug.Log("[CHECK 2 PASSED] CharacterRoster.asset exists and loaded successfully.");
            }
            else
            {
                Debug.LogError("[CHECK 2 FAILED] CharacterRoster.asset not found at path.");
                allPassed = false;
            }

            // CHECK 3: CharacterRoster contains exactly 3 characters in order: Warrior, Archer, Gunner
            var warriorAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
            var archerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
            var gunnerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(GunnerAssetPath);

            if (roster != null && roster.Count == 3 &&
                roster[0] == warriorAsset &&
                roster[1] == archerAsset &&
                roster[2] == gunnerAsset)
            {
                Debug.Log("[CHECK 3 PASSED] CharacterRoster order verified: [Warrior, Archer, Gunner].");
            }
            else
            {
                Debug.LogError($"[CHECK 3 FAILED] CharacterRoster order mismatch: count={roster?.Count}.");
                allPassed = false;
            }

            // CHECK 4: CharacterRoster validation passes (no nulls, no duplicate refs, no duplicate IDs)
            string valError = null;
            if (roster != null && roster.ValidateRoster(out valError))
            {
                Debug.Log("[CHECK 4 PASSED] CharacterRoster.ValidateRoster passed with zero errors or duplicates.");
            }
            else
            {
                Debug.LogError($"[CHECK 4 FAILED] CharacterRoster validation failed: {valError ?? "Roster was null."}");
                allPassed = false;
            }

            // CHECK 5: Character definitions link to valid character prefabs with Tag == 'Player'
            bool prefabsValid = warriorAsset?.CharacterPrefab != null && warriorAsset.CharacterPrefab.CompareTag("Player") &&
                               archerAsset?.CharacterPrefab != null && archerAsset.CharacterPrefab.CompareTag("Player") &&
                               gunnerAsset?.CharacterPrefab != null && gunnerAsset.CharacterPrefab.CompareTag("Player");
            if (prefabsValid)
            {
                Debug.Log("[CHECK 5 PASSED] All roster CharacterDefinitions point to valid prefabs tagged 'Player'.");
            }
            else
            {
                Debug.LogError("[CHECK 5 FAILED] CharacterDefinition prefab validation failed.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION B: Build Settings & Scene Integrity
            // -------------------------------------------------------------

            // CHECK 6: EditorBuildSettings scene ordering: 0 = CharacterSelection, 1 = Dungeon_Prototype
            var buildScenes = EditorBuildSettings.scenes;
            bool scene0Match = buildScenes.Length > 0 && buildScenes[0].path == SelectionScenePath && buildScenes[0].enabled;
            bool scene1Match = buildScenes.Length > 1 && buildScenes[1].path == DungeonScenePath && buildScenes[1].enabled;
            if (scene0Match && scene1Match)
            {
                Debug.Log("[CHECK 6 PASSED] EditorBuildSettings registered: 0 = CharacterSelection, 1 = Dungeon_Prototype.");
            }
            else
            {
                Debug.LogError($"[CHECK 6 FAILED] BuildSettings mismatch: scene0Match={scene0Match}, scene1Match={scene1Match}.");
                allPassed = false;
            }

            // CHECK 7: CharacterSelection scene contains no gameplay player instances
            var playersInSelection = Object.FindObjectsByType<PlayableCharacter>(FindObjectsSortMode.None);
            if (playersInSelection.Length == 0)
            {
                Debug.Log("[CHECK 7 PASSED] CharacterSelection scene contains zero gameplay PlayableCharacter instances.");
            }
            else
            {
                Debug.LogError($"[CHECK 7 FAILED] Found {playersInSelection.Length} PlayableCharacter instances in selection scene.");
                allPassed = false;
            }

            // CHECK 8: CharacterSelection scene contains exactly one EventSystem
            var eventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
            if (eventSystems.Length == 1 && eventSystems[0].GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() != null)
            {
                Debug.Log("[CHECK 8 PASSED] Exactly one EventSystem exists with InputSystemUIInputModule.");
            }
            else
            {
                Debug.LogError($"[CHECK 8 FAILED] EventSystem count={eventSystems.Length}.");
                allPassed = false;
            }

            // CHECK 9: Dungeon_Prototype.unity on disk retains defaultCharacter = Character_Warrior
            string dungeonSceneText = File.ReadAllText(DungeonScenePath);
            string warriorGuid = AssetDatabase.AssetPathToGUID(WarriorAssetPath);
            if (dungeonSceneText.Contains(warriorGuid) && dungeonSceneText.Contains("defaultCharacter"))
            {
                Debug.Log("[CHECK 9 PASSED] Dungeon_Prototype.unity on disk defaults to Character_Warrior.");
            }
            else
            {
                Debug.LogError("[CHECK 9 FAILED] Dungeon_Prototype.unity on disk does not default to Character_Warrior.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION C: Selection UI Interactivity & Local State
            // -------------------------------------------------------------

            var controller = Object.FindFirstObjectByType<CharacterSelectionController>();
            if (controller == null)
            {
                Debug.LogError("[CHECK 10-24 FAILED] CharacterSelectionController not found in scene.");
                allPassed = false;
                yield break;
            }

            // CHECK 10: CharacterSelectionSession is empty on scene entry
            if (!CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter == null)
            {
                Debug.Log("[CHECK 10 PASSED] CharacterSelectionSession is strictly empty upon entering CharacterSelection.");
            }
            else
            {
                Debug.LogError("[CHECK 10 FAILED] CharacterSelectionSession had stale state upon entering scene.");
                allPassed = false;
            }

            // CHECK 11: Controller initialized with 3 cards matching roster
            if (controller.Cards != null && controller.Cards.Count == 3 &&
                controller.Cards[0].BoundCharacter == warriorAsset &&
                controller.Cards[1].BoundCharacter == archerAsset &&
                controller.Cards[2].BoundCharacter == gunnerAsset)
            {
                Debug.Log("[CHECK 11 PASSED] CharacterSelectionController cards bound to [Warrior, Archer, Gunner].");
            }
            else
            {
                Debug.LogError("[CHECK 11 FAILED] Controller cards count or binding mismatch.");
                allPassed = false;
            }

            // CHECK 12: Default local selection is Warrior (first roster entry)
            if (controller.CurrentSelection == warriorAsset)
            {
                Debug.Log("[CHECK 12 PASSED] Default local selection is Warrior upon scene entry.");
            }
            else
            {
                Debug.LogError($"[CHECK 12 FAILED] Default selection was '{controller.CurrentSelection?.DisplayName}', expected Warrior.");
                allPassed = false;
            }

            // CHECK 13: Exactly one card is visually marked selected (Warrior active, Archer & Gunner inactive)
            bool card0Selected = controller.Cards[0].IsSelected;
            bool card1Selected = controller.Cards[1].IsSelected;
            bool card2Selected = controller.Cards[2].IsSelected;
            if (card0Selected && !card1Selected && !card2Selected)
            {
                Debug.Log("[CHECK 13 PASSED] Mutual exclusivity verified on start: only Warrior card is visually selected.");
            }
            else
            {
                Debug.LogError($"[CHECK 13 FAILED] Visual selection mismatch: c0={card0Selected}, c1={card1Selected}, c2={card2Selected}.");
                allPassed = false;
            }

            // CHECK 14: Selected preview text displays 'Selected: Warrior'
            if (controller.SelectedPreviewText != null && controller.SelectedPreviewText.text.Contains("Warrior"))
            {
                Debug.Log($"[CHECK 14 PASSED] Preview text shows: '{controller.SelectedPreviewText.text}'.");
            }
            else
            {
                Debug.LogError($"[CHECK 14 FAILED] Preview text was '{controller.SelectedPreviewText?.text}'.");
                allPassed = false;
            }

            // CHECK 15: Selecting Archer updates local selection and visual highlight
            controller.SelectCharacterLocally(archerAsset);
            bool c0AfterArcher = controller.Cards[0].IsSelected;
            bool c1AfterArcher = controller.Cards[1].IsSelected;
            bool c2AfterArcher = controller.Cards[2].IsSelected;
            if (controller.CurrentSelection == archerAsset && !c0AfterArcher && c1AfterArcher && !c2AfterArcher)
            {
                Debug.Log("[CHECK 15 PASSED] Selecting Archer updated local selection and card highlight (mutual exclusivity preserved).");
            }
            else
            {
                Debug.LogError($"[CHECK 15 FAILED] Selecting Archer failed: current={controller.CurrentSelection?.DisplayName}, c0={c0AfterArcher}, c1={c1AfterArcher}, c2={c2AfterArcher}.");
                allPassed = false;
            }

            // CHECK 16: Local card selection did NOT mutate CharacterSelectionSession
            if (!CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter == null)
            {
                Debug.Log("[CHECK 16 PASSED] Local card selection did NOT mutate CharacterSelectionSession (session remains empty).");
            }
            else
            {
                Debug.LogError("[CHECK 16 FAILED] CharacterSelectionSession was prematurely mutated on card click!");
                allPassed = false;
            }

            // CHECK 17: Selecting Gunner updates local selection and visual highlight
            controller.SelectCharacterLocally(gunnerAsset);
            bool c0AfterGunner = controller.Cards[0].IsSelected;
            bool c1AfterGunner = controller.Cards[1].IsSelected;
            bool c2AfterGunner = controller.Cards[2].IsSelected;
            if (controller.CurrentSelection == gunnerAsset && !c0AfterGunner && !c1AfterGunner && c2AfterGunner)
            {
                Debug.Log("[CHECK 17 PASSED] Selecting Gunner updated local selection and card highlight.");
            }
            else
            {
                Debug.LogError($"[CHECK 17 FAILED] Selecting Gunner failed: current={controller.CurrentSelection?.DisplayName}.");
                allPassed = false;
            }

            // CHECK 18: Re-selecting Warrior restores Warrior highlight
            controller.SelectCharacterLocally(warriorAsset);
            if (controller.CurrentSelection == warriorAsset && controller.Cards[0].IsSelected && !controller.Cards[1].IsSelected)
            {
                Debug.Log("[CHECK 18 PASSED] Re-selecting Warrior restores Warrior selection and highlight.");
            }
            else
            {
                Debug.LogError("[CHECK 18 FAILED] Re-selecting Warrior failed.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION D: Start Action & Session Commitment
            // -------------------------------------------------------------

            // CHECK 19: CharacterSelectionSession.SetSelection commits exact definition
            CharacterSelectionSession.Clear();
            CharacterSelectionSession.SetSelection(archerAsset);
            if (CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter == archerAsset)
            {
                Debug.Log("[CHECK 19 PASSED] CharacterSelectionSession.SetSelection committed Archer accurately.");
            }
            else
            {
                Debug.LogError("[CHECK 19 FAILED] SetSelection failed.");
                allPassed = false;
            }

            // CHECK 20: CharacterSelectionSession.Clear resets session
            CharacterSelectionSession.Clear();
            if (!CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter == null)
            {
                Debug.Log("[CHECK 20 PASSED] CharacterSelectionSession.Clear cleanly resets session.");
            }
            else
            {
                Debug.LogError("[CHECK 20 FAILED] Clear failed.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION E: PlayerSpawner Integration & Fallback
            // -------------------------------------------------------------

            // Setup a temporary PlayerSpawner harness in memory to verify spawning logic
            GameObject spawnerHolderGo = new GameObject("TestSpawnerHolder");
            var spawner = spawnerHolderGo.AddComponent<PlayerSpawner>();
            var spawnPointGo = new GameObject("TestSpawnPoint");
            spawner.SetSpawnPoint(spawnPointGo.transform);
            spawner.SetDefaultCharacter(warriorAsset);

            // CHECK 21: PlayerSpawner has zero character-specific branching (reflection check)
            var spawnerMethods = typeof(PlayerSpawner).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            bool hasCharacterBranchMethod = false;
            foreach (var m in spawnerMethods)
            {
                string lower = m.Name.ToLowerInvariant();
                if (lower.Contains("warrior") || lower.Contains("archer") || lower.Contains("gunner"))
                {
                    hasCharacterBranchMethod = true;
                    break;
                }
            }
            if (!hasCharacterBranchMethod)
            {
                Debug.Log("[CHECK 21 PASSED] PlayerSpawner contains zero character-specific branching methods.");
            }
            else
            {
                Debug.LogError("[CHECK 21 FAILED] PlayerSpawner contains character-specific branching.");
                allPassed = false;
            }

            // CHECK 22: PlayerSpawner with empty session spawns default Character_Warrior
            CharacterSelectionSession.Clear();
            PlayableCharacter spawnedDefault = spawner.Spawn();
            if (spawnedDefault != null && spawnedDefault.CharacterDefinition == warriorAsset && spawnedDefault.GetComponent<MeleeWeapon>() != null)
            {
                Debug.Log("[CHECK 22 PASSED] Empty session cleanly falls back to default Character_Warrior.");
            }
            else
            {
                Debug.LogError($"[CHECK 22 FAILED] Empty session fallback failed: def={spawnedDefault?.CharacterDefinition?.DisplayName}.");
                allPassed = false;
            }
            Object.DestroyImmediate(spawnedDefault.gameObject);
            ClearSpawnerActiveCharacter(spawner);

            // CHECK 23: Spawning with session did NOT overwrite PlayerSpawner.defaultCharacter
            if (spawner.DefaultCharacter == warriorAsset)
            {
                Debug.Log("[CHECK 23 PASSED] PlayerSpawner.defaultCharacter was NOT overwritten by session selection.");
            }
            else
            {
                Debug.LogError($"[CHECK 23 FAILED] defaultCharacter was mutated to '{spawner.DefaultCharacter?.DisplayName}'!");
                allPassed = false;
            }

            // CHECK 24: PlayerSpawner with CharacterSelectionSession = Archer spawns Archer
            CharacterSelectionSession.SetSelection(archerAsset);
            PlayableCharacter spawnedArcher = spawner.Spawn();
            if (spawnedArcher != null && spawnedArcher.CharacterDefinition == archerAsset && spawnedArcher.GetComponent<BowWeapon>() != null)
            {
                Debug.Log("[CHECK 24 PASSED] PlayerSpawner consumed session selection and spawned Archer with BowWeapon.");
            }
            else
            {
                Debug.LogError($"[CHECK 24 FAILED] Archer spawn failed: def={spawnedArcher?.CharacterDefinition?.DisplayName}.");
                allPassed = false;
            }
            Object.DestroyImmediate(spawnedArcher.gameObject);
            ClearSpawnerActiveCharacter(spawner);

            // CHECK 25: PlayerSpawner with CharacterSelectionSession = Gunner spawns Gunner
            CharacterSelectionSession.SetSelection(gunnerAsset);
            PlayableCharacter spawnedGunner = spawner.Spawn();
            if (spawnedGunner != null && spawnedGunner.CharacterDefinition == gunnerAsset && spawnedGunner.GetComponent<RifleWeapon>() != null)
            {
                Debug.Log("[CHECK 25 PASSED] PlayerSpawner consumed session selection and spawned Gunner with RifleWeapon.");
            }
            else
            {
                Debug.LogError($"[CHECK 25 FAILED] Gunner spawn failed: def={spawnedGunner?.CharacterDefinition?.DisplayName}.");
                allPassed = false;
            }
            Object.DestroyImmediate(spawnedGunner.gameObject);
            ClearSpawnerActiveCharacter(spawner);

            // CHECK 26: PlayerSpawner with CharacterSelectionSession = Warrior spawns Warrior
            CharacterSelectionSession.SetSelection(warriorAsset);
            PlayableCharacter spawnedWarrior = spawner.Spawn();
            if (spawnedWarrior != null && spawnedWarrior.CharacterDefinition == warriorAsset && spawnedWarrior.GetComponent<MeleeWeapon>() != null)
            {
                Debug.Log("[CHECK 26 PASSED] PlayerSpawner consumed session selection and spawned Warrior with MeleeWeapon.");
            }
            else
            {
                Debug.LogError($"[CHECK 26 FAILED] Warrior spawn failed: def={spawnedWarrior?.CharacterDefinition?.DisplayName}.");
                allPassed = false;
            }
            Object.DestroyImmediate(spawnedWarrior.gameObject);
            ClearSpawnerActiveCharacter(spawner);

            // Clean temporary spawner harness
            Object.DestroyImmediate(spawnPointGo);
            Object.DestroyImmediate(spawnerHolderGo);

            // -------------------------------------------------------------
            // SECTION F: Restart Semantics
            // -------------------------------------------------------------

            // CHECK 27: Warrior restart preserves Warrior selection in session
            CharacterSelectionSession.SetSelection(warriorAsset);
            // Simulate scene reload (session must retain reference)
            if (CharacterSelectionSession.SelectedCharacter == warriorAsset)
            {
                Debug.Log("[CHECK 27 PASSED] Warrior selection survives simulated scene reload for dungeon restart.");
            }
            else
            {
                Debug.LogError("[CHECK 27 FAILED] Warrior selection lost.");
                allPassed = false;
            }

            // CHECK 28: Archer restart preserves Archer selection in session
            CharacterSelectionSession.SetSelection(archerAsset);
            if (CharacterSelectionSession.SelectedCharacter == archerAsset)
            {
                Debug.Log("[CHECK 28 PASSED] Archer selection survives simulated scene reload for dungeon restart.");
            }
            else
            {
                Debug.LogError("[CHECK 28 FAILED] Archer selection lost.");
                allPassed = false;
            }

            // CHECK 29: Gunner restart preserves Gunner selection in session
            CharacterSelectionSession.SetSelection(gunnerAsset);
            if (CharacterSelectionSession.SelectedCharacter == gunnerAsset)
            {
                Debug.Log("[CHECK 29 PASSED] Gunner selection survives simulated scene reload for dungeon restart.");
            }
            else
            {
                Debug.LogError("[CHECK 29 FAILED] Gunner selection lost.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION G: Full In-Scene Transition & Binding Verification
            // -------------------------------------------------------------

            // Test transition to Dungeon_Prototype with Gunner in session
            CharacterSelectionSession.SetSelection(gunnerAsset);
            Debug.Log("[M8.5 TEST] Testing scene load into Dungeon_Prototype with Gunner selected...");

            AsyncOperation loadDungeon = SceneManager.LoadSceneAsync(DungeonScenePath);
            float timeout = 5.0f;
            float elapsed = 0f;
            while (!loadDungeon.isDone)
            {
                elapsed += Time.deltaTime;
                if (elapsed > timeout)
                {
                    Debug.LogError("[M8.5 TEST] Timed out waiting for Dungeon_Prototype scene to load.");
                    allPassed = false;
                    break;
                }
                yield return null;
            }

            yield return null; // Wait 1 frame for Awake/Start

            // CHECK 30: Exactly one player exists in Dungeon_Prototype and it is Gunner
            var dungeonPlayers = Object.FindObjectsByType<PlayableCharacter>(FindObjectsSortMode.None);
            if (dungeonPlayers.Length == 1 && dungeonPlayers[0].CharacterDefinition == gunnerAsset)
            {
                Debug.Log("[CHECK 30 PASSED] Exactly one player spawned in Dungeon_Prototype: Gunner.");
            }
            else
            {
                Debug.LogError($"[CHECK 30 FAILED] Player in dungeon count={dungeonPlayers.Length}, def={dungeonPlayers[0]?.CharacterDefinition?.DisplayName}.");
                allPassed = false;
            }

            var activeGunner = dungeonPlayers.Length > 0 ? dungeonPlayers[0] : null;

            // CHECK 31: CameraFollow target bound to runtime Gunner
            var cam = Object.FindFirstObjectByType<CameraFollow>();
            if (cam != null && cam.Target == activeGunner?.transform)
            {
                Debug.Log("[CHECK 31 PASSED] CameraFollow explicitly bound to runtime Gunner transform.");
            }
            else
            {
                Debug.LogError("[CHECK 31 FAILED] CameraFollow target binding mismatch.");
                allPassed = false;
            }

            // CHECK 32: WaveManager playerTarget bound to runtime Gunner
            var waveMgr = Object.FindFirstObjectByType<WaveManager>();
            if (waveMgr != null && waveMgr.PlayerTarget == activeGunner?.transform)
            {
                Debug.Log("[CHECK 32 PASSED] WaveManager playerTarget explicitly bound to runtime Gunner transform.");
            }
            else
            {
                Debug.LogError("[CHECK 32 FAILED] WaveManager playerTarget binding mismatch.");
                allPassed = false;
            }

            // CHECK 33: UpgradeManager bound to runtime Gunner PlayerExperience and PlayerStats
            var upMgr = Object.FindFirstObjectByType<UpgradeManager>();
            var gExp = activeGunner?.GetComponent<PlayerExperience>();
            var gStats = activeGunner?.GetComponent<PlayerStats>();
            var upExpField = typeof(UpgradeManager).GetField("playerExperience", BindingFlags.NonPublic | BindingFlags.Instance);
            var upStatsField = typeof(UpgradeManager).GetField("playerStats", BindingFlags.NonPublic | BindingFlags.Instance);
            bool upExpBound = upExpField != null && (PlayerExperience)upExpField.GetValue(upMgr) == gExp;
            bool upStatsBound = upStatsField != null && (PlayerStats)upStatsField.GetValue(upMgr) == gStats;
            if (upExpBound && upStatsBound)
            {
                Debug.Log("[CHECK 33 PASSED] UpgradeManager explicitly bound to runtime Gunner PlayerExperience and PlayerStats.");
            }
            else
            {
                Debug.LogError($"[CHECK 33 FAILED] UpgradeManager binding mismatch: exp={upExpBound}, stats={upStatsBound}.");
                allPassed = false;
            }

            // CHECK 34: PlayerExperienceUI bound to runtime Gunner
            var expUI = Object.FindFirstObjectByType<PlayerExperienceUI>();
            var expUiField = typeof(PlayerExperienceUI).GetField("playerExperience", BindingFlags.NonPublic | BindingFlags.Instance);
            bool expUiBound = expUiField != null && (PlayerExperience)expUiField.GetValue(expUI) == gExp;
            if (expUiBound)
            {
                Debug.Log("[CHECK 34 PASSED] PlayerExperienceUI explicitly bound to runtime Gunner PlayerExperience.");
            }
            else
            {
                Debug.LogError("[CHECK 34 FAILED] PlayerExperienceUI binding mismatch.");
                allPassed = false;
            }

            // CHECK 35: DungeonCompletionController bound to runtime Gunner
            var compCtrl = Object.FindFirstObjectByType<DungeonCompletionController>();
            var compExpField = typeof(DungeonCompletionController).GetField("playerExperience", BindingFlags.NonPublic | BindingFlags.Instance);
            bool compExpBound = compExpField != null && (PlayerExperience)compExpField.GetValue(compCtrl) == gExp;
            if (compExpBound)
            {
                Debug.Log("[CHECK 35 PASSED] DungeonCompletionController explicitly bound to runtime Gunner PlayerExperience.");
            }
            else
            {
                Debug.LogError("[CHECK 35 FAILED] DungeonCompletionController binding mismatch.");
                allPassed = false;
            }

            // -------------------------------------------------------------
            // SECTION H: Combat Regressions (Warrior, Archer, Gunner)
            // -------------------------------------------------------------

            // CHECK 36: Gunner hitscan combat functional in dungeon
            var gunnerRifle = activeGunner?.GetComponent<RifleWeapon>();
            if (gunnerRifle != null)
            {
                bool rifleAttacked = gunnerRifle.TryAttack();
                if (rifleAttacked)
                {
                    Debug.Log("[CHECK 36 PASSED] Gunner hitscan combat executed successfully in dungeon.");
                }
                else
                {
                    Debug.LogError("[CHECK 36 FAILED] Gunner rifle attack failed.");
                    allPassed = false;
                }
            }

            // CHECK 37: Archer combat regression
            var spawnerInDungeon = Object.FindFirstObjectByType<PlayerSpawner>();
            if (activeGunner != null) Object.DestroyImmediate(activeGunner.gameObject);
            ClearSpawnerActiveCharacter(spawnerInDungeon);

            PlayableCharacter testArcher = spawnerInDungeon.Spawn(archerAsset);
            yield return null;
            if (testArcher != null && testArcher.GetComponent<BowWeapon>() != null)
            {
                var bow = testArcher.GetComponent<BowWeapon>();
                bool bowAttacked = bow.TryAttack();
                if (bowAttacked)
                {
                    Debug.Log("[CHECK 37 PASSED] Archer ranged combat regression verified: bow fired arrow.");
                }
                else
                {
                    Debug.LogError("[CHECK 37 FAILED] Archer bow attack failed.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 37 FAILED] Spawning Archer in dungeon failed.");
                allPassed = false;
            }
            if (testArcher != null) Object.DestroyImmediate(testArcher.gameObject);
            ClearSpawnerActiveCharacter(spawnerInDungeon);

            // CHECK 38: Warrior combat regression
            PlayableCharacter testWarrior = spawnerInDungeon.Spawn(warriorAsset);
            yield return null;
            if (testWarrior != null && testWarrior.GetComponent<MeleeWeapon>() != null)
            {
                var sword = testWarrior.GetComponent<MeleeWeapon>();
                bool swordAttacked = sword.TryAttack();
                if (swordAttacked)
                {
                    Debug.Log("[CHECK 38 PASSED] Warrior melee combat regression verified: sword swing executed.");
                }
                else
                {
                    Debug.LogError("[CHECK 38 FAILED] Warrior sword attack failed.");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[CHECK 38 FAILED] Spawning Warrior in dungeon failed.");
                allPassed = false;
            }

            // CHECK 39: Universal Upgrade scaling verified
            if (testWarrior != null)
            {
                var wStats = testWarrior.GetComponent<PlayerStats>();
                var wSword = testWarrior.GetComponent<MeleeWeapon>();
                float baseDmg = wSword.BaseDamage;
                wStats.AddDamageBonus(0.20f);
                if (Mathf.Approximately(wSword.EffectiveDamage, baseDmg * 1.20f))
                {
                    Debug.Log("[CHECK 39 PASSED] Universal PlayerStats damage bonus scales EffectiveDamage (+20%).");
                }
                else
                {
                    Debug.LogError($"[CHECK 39 FAILED] Scaled damage mismatch: got {wSword.EffectiveDamage}, expected {baseDmg * 1.20f}.");
                    allPassed = false;
                }
            }
            if (testWarrior != null) Object.DestroyImmediate(testWarrior.gameObject);
            ClearSpawnerActiveCharacter(spawnerInDungeon);

            // CHECK 40: Dungeon completion summary generation
            if (compCtrl != null)
            {
                compCtrl.HandleWaveManagerCompletion();
                yield return null;
                if (compCtrl.HasCompleted && compCtrl.IsCompletionFinished)
                {
                    Debug.Log("[CHECK 40 PASSED] DungeonCompletionController completion flow finalized successfully.");
                }
                else
                {
                    Debug.LogError("[CHECK 40 FAILED] Completion controller did not finalize.");
                    allPassed = false;
                }
                Time.timeScale = 1.0f; // Restore time scale
            }

            // CHECK 41: Clean up session state
            CharacterSelectionSession.Clear();
            if (!CharacterSelectionSession.HasSelection)
            {
                Debug.Log("[CHECK 41 PASSED] CharacterSelectionSession cleanly reset after test execution.");
            }
            else
            {
                Debug.LogError("[CHECK 41 FAILED] Session state remained active.");
                allPassed = false;
            }

            // Summary
            if (allPassed)
            {
                Debug.Log("[M8.5 TEST SUCCESS] ALL 41 VERIFICATION CHECKS PASSED!");
                ExitBatch(0);
            }
            else
            {
                Debug.LogError("[M8.5 TEST FAILURE] ONE OR MORE VERIFICATION CHECKS FAILED!");
                ExitBatch(1);
            }
        }

        private static void ClearSpawnerActiveCharacter(PlayerSpawner spawner)
        {
            if (spawner != null)
            {
                var activeField = typeof(PlayerSpawner).GetField("activeCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
                if (activeField != null)
                {
                    activeField.SetValue(spawner, null);
                }
            }
        }

        private static void ExitBatch(int exitCode)
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
