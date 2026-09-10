#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.UI;
using DungeonRoguelite.Weapons;

namespace DungeonRoguelite.Tests
{
    public sealed class Milestone12_5_Verifier : MonoBehaviour
    {
        private const string WorldMapScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";
        private const string WarriorDefPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
        private const string ArcherDefPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";
        private const string GunnerDefPath = "Assets/ScriptableObjects/Characters/Character_Gunner.asset";

        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private Phase12StateSnapshot snapshot;
        private int checks;
        private int runtimeErrors;

        private IEnumerator Start()
        {
            Application.logMessageReceived += HandleLog;
            snapshot = new Phase12StateSnapshot();
            bool success = true;

            IEnumerator routine = RunVerificationFlow();
            while (true)
            {
                object current;
                try
                {
                    if (!routine.MoveNext()) break;
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

            Cleanup();
            snapshot?.Dispose();
            snapshot = null;

            Application.logMessageReceived -= HandleLog;
            success &= runtimeErrors == 0;
            Debug.Log($"[GATE 12.5 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.isPlaying = false;
        }

        private IEnumerator RunVerificationFlow()
        {
            VerifyBuildSettingsAndSceneDiskIntegrity();
            yield return StartCoroutine(VerifyWorldMapSceneAndUIHierarchy());
            VerifyDeterministicNodeStatesAndPurchasingOnWarrior();
            VerifyExternalProgressionEventSync();
            VerifyCharacterSwitchingIsolation();
            VerifySpawnTimeGameplayIntegrationAndStatLayering();
            VerifyCampaignEconomyAndIdempotencyIntegration();
            VerifyFailSafeHandling();
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
            if (!condition) throw new InvalidOperationException("[GATE 12.5 FAILED] " + description);
            Debug.Log($"[GATE 12.5 CHECK {++checks} PASSED] {description}");
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

        private void DestroySpawnerAndCharacter(PlayerSpawner spawner, GameObject spawnerGo, GameObject spawnPt, PlayableCharacter character)
        {
            if (character != null) DestroyOwned(character.gameObject);
            if (spawnerGo != null) DestroyOwned(spawnerGo);
            if (spawnPt != null) DestroyOwned(spawnPt);
        }

        private void VerifyBuildSettingsAndSceneDiskIntegrity()
        {
            // Build Settings registration and order
            var scenes = EditorBuildSettings.scenes;
            Check(scenes != null && scenes.Length >= 6, "Build Settings contains at least 6 scenes");
            Check(scenes[0].path.EndsWith("CharacterSelection.unity"), "Scene 0 is CharacterSelection");
            Check(scenes[1].path.EndsWith("WorldMap.unity"), "Scene 1 is WorldMap");
            Check(scenes[2].path.EndsWith("Dungeon_Prototype.unity"), "Scene 2 is Dungeon_Prototype");
            Check(scenes[3].path.EndsWith("Dungeon_02.unity"), "Scene 3 is Dungeon_02");
            Check(scenes[4].path.EndsWith("Dungeon_03.unity"), "Scene 4 is Dungeon_03");
            Check(scenes[5].path.EndsWith("Dungeon_04.unity"), "Scene 5 is Dungeon_04");

            // WorldMap.unity disk integrity
            Check(File.Exists(WorldMapScenePath), "WorldMap.unity scene file exists on disk");
            string yaml = File.ReadAllText(WorldMapScenePath);
            Check(yaml.Contains("SkillTreeButton"), "WorldMap.unity contains SkillTreeButton serialized on disk");
            Check(yaml.Contains("SkillTreePanel"), "WorldMap.unity contains SkillTreePanel serialized on disk");
            Check(yaml.Contains("YETENEKLER"), "WorldMap.unity contains YETENEKLER label on disk");
            Check(yaml.Contains("KAPAT"), "WorldMap.unity contains KAPAT close button on disk");
        }

        private IEnumerator VerifyWorldMapSceneAndUIHierarchy()
        {
            // Load WorldMap scene additively
            var loadOp = SceneManager.LoadSceneAsync("WorldMap", LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;

            var mapScene = SceneManager.GetSceneByName("WorldMap");
            Check(mapScene.IsValid() && mapScene.isLoaded, "WorldMap scene loaded cleanly in test runner");

            var mapController = FindFirstObjectByType<WorldMapController>();
            Check(mapController != null, "WorldMapController found in WorldMap scene");
            Check(mapController.DungeonCatalog != null && mapController.DungeonCatalog.Count == 5,
                "WorldMapController has 5 dungeons in catalog");
            Check(mapController.Cards != null && mapController.Cards.Count == 5,
                "WorldMapController has 5 bound cards");

            // SkillTreeButton
            var skillTreeBtn = mapController.SkillTreeButton;
            Check(skillTreeBtn != null, "SkillTreeButton is wired on WorldMapController");
            Check(skillTreeBtn.gameObject.activeInHierarchy, "SkillTreeButton is active in hierarchy");
            Check(skillTreeBtn.interactable, "SkillTreeButton is interactable");

            var btnText = skillTreeBtn.GetComponentInChildren<TextMeshProUGUI>(true);
            Check(btnText != null && btnText.text.Equals("YETENEKLER", StringComparison.OrdinalIgnoreCase),
                "SkillTreeButton label is 'YETENEKLER'");

            // SkillTreeUI / SkillTreePanel
            var skillTreePanel = mapController.SkillTreePanel;
            Check(skillTreePanel != null, "SkillTreePanel is wired on WorldMapController");
            Check(skillTreePanel.PanelRoot != null, "SkillTreePanel has a valid PanelRoot");
            Check(!skillTreePanel.IsOpen, "SkillTreePanel starts closed (inactive) by default");
            Check(!skillTreePanel.PanelRoot.activeSelf, "SkillTreePanel root GameObject is inactive by default");

            // Inspect internal elements of SkillTreeUI
            Check(skillTreePanel.CharacterNameText != null, "SkillTreeUI has CharacterNameText reference");
            Check(skillTreePanel.AvailablePointsText != null, "SkillTreeUI has AvailablePointsText reference");
            Check(skillTreePanel.CloseButton != null, "SkillTreeUI has CloseButton reference");

            var closeBtnText = skillTreePanel.CloseButton.GetComponentInChildren<TextMeshProUGUI>(true);
            Check(closeBtnText != null && closeBtnText.text.Equals("KAPAT", StringComparison.OrdinalIgnoreCase),
                "CloseButton label is 'KAPAT'");

            Check(skillTreePanel.NodeUIs != null && skillTreePanel.NodeUIs.Count == 9,
                "SkillTreeUI has exactly 9 SkillTreeNodeUI cards wired (3 branches x 3 tiers)");

            // Test Open / Close via button and controller
            mapController.NavigateRight();
            int focus = mapController.FocusedIndex;
            int window = mapController.WindowStart;
            var selected = mapController.SelectedDungeon;
            var visible = mapController.Cards.Select(c => c.gameObject.activeSelf).ToArray();
            bool left = mapController.LeftNavigationButton.interactable;
            bool right = mapController.RightNavigationButton.interactable;
            mapController.HandleSkillTreeClicked();
            Check(skillTreePanel.IsOpen, "HandleSkillTreeClicked opens the SkillTreePanel overlay");
            Check(skillTreePanel.PanelRoot.activeSelf, "SkillTreePanel root is active when opened");
            Check(skillTreePanel.transform.parent == mapController.transform &&
                skillTreePanel.transform.GetSiblingIndex() == mapController.transform.childCount - 1 &&
                mapController.Cards.All(c => c.transform.IsChildOf(mapController.CardsContainer)),
                "Skill Tree is the final Canvas sibling above all carousel cards and navigation");
            mapController.LeftNavigationButton.onClick.Invoke();
            mapController.RightNavigationButton.onClick.Invoke();
            Check(mapController.FocusedIndex == focus, "Modal blocks carousel navigation");

            // Close via CloseButton
            skillTreePanel.CloseButton.onClick.Invoke();
            Check(!skillTreePanel.IsOpen, "CloseButton onClick closes the SkillTreePanel overlay");
            Check(!skillTreePanel.PanelRoot.activeSelf, "SkillTreePanel root is inactive when closed");
            Check(mapController.FocusedIndex == focus && mapController.WindowStart == window &&
                mapController.SelectedDungeon == selected && visible.SequenceEqual(mapController.Cards.Select(c => c.gameObject.activeSelf)) &&
                mapController.LeftNavigationButton.interactable == left && mapController.RightNavigationButton.interactable == right,
                "Closing Skill Tree restores the exact carousel focus, window and arrow state");

            // Verify session invariants
            Check(!RunProgressionSession.HasActiveRun, "Opening/closing SkillTreePanel did not alter RunProgressionSession");

            // Unload WorldMap scene
            var unloadOp = SceneManager.UnloadSceneAsync(mapScene);
            while (!unloadOp.isDone) yield return null;
        }

        private void VerifyDeterministicNodeStatesAndPurchasingOnWarrior()
        {
            PermanentProgression.ResetAllProgression();
            var warriorDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorDefPath);
            Check(warriorDef != null && warriorDef.SkillTree != null, "WarriorDefinition and SkillTree resolved");

            var (panelGo, skillTreeUI) = CreateTestSkillTreeUI(warriorDef);

            // Step 1: Open with Warrior at 0 points
            CharacterSelectionSession.SetSelection(warriorDef);
            skillTreeUI.Open(warriorDef);

            Check(skillTreeUI.IsOpen, "SkillTreeUI opened successfully");
            Check(skillTreeUI.CharacterNameText.text.Contains(warriorDef.DisplayName),
                "CharacterNameText displays warrior DisplayName");
            Check(skillTreeUI.AvailablePointsText.text.Contains("0"),
                "AvailablePointsText displays 0 points");

            var nodes = skillTreeUI.NodeUIs;
            // With 0 points:
            // Tier 1 nodes (0, 3, 6) should be InsufficientPoints
            // Tier 2 & 3 nodes should be Locked
            Check(nodes[0].CurrentState == SkillNodeUIState.InsufficientPoints, "Node 0 (Durability 1) is InsufficientPoints at 0 pts");
            Check(!nodes[0].PurchaseButton.interactable, "Node 0 PurchaseButton is non-interactable");
            Check(nodes[0].StatusText.text.Contains("YETERSİZ PUAN"), "Node 0 StatusText displays 'YETERSİZ PUAN'");

            Check(nodes[1].CurrentState == SkillNodeUIState.Locked, "Node 1 (Durability 2) is Locked (missing prereq)");
            Check(!nodes[1].PurchaseButton.interactable, "Node 1 PurchaseButton is non-interactable");
            Check(nodes[1].StatusText.text.Contains("KİLİTLİ"), "Node 1 StatusText displays 'KİLİTLİ'");

            Check(nodes[2].CurrentState == SkillNodeUIState.Locked, "Node 2 (Durability 3) is Locked");
            Check(nodes[3].CurrentState == SkillNodeUIState.InsufficientPoints, "Node 3 (Power 1) is InsufficientPoints");
            Check(nodes[4].CurrentState == SkillNodeUIState.Locked, "Node 4 (Power 2) is Locked");
            Check(nodes[6].CurrentState == SkillNodeUIState.InsufficientPoints, "Node 6 (Tempo 1) is InsufficientPoints");
            Check(nodes[7].CurrentState == SkillNodeUIState.Locked, "Node 7 (Tempo 2) is Locked");

            // Step 2: Attempt clicking non-interactable nodes (guards against invalid actions)
            nodes[0].PurchaseButton.onClick.Invoke();
            Check(!PermanentProgression.IsNodePurchased("warrior", "warrior_durability_1"),
                "Clicking InsufficientPoints node did NOT purchase node");

            nodes[1].PurchaseButton.onClick.Invoke();
            Check(!PermanentProgression.IsNodePurchased("warrior", "warrior_durability_2"),
                "Clicking Locked node did NOT purchase node");

            // Step 3: Award 2 skill points to Warrior
            PermanentProgression.AddSkillPoints("warrior", 2);
            skillTreeUI.RefreshUI();

            Check(skillTreeUI.AvailablePointsText.text.Contains("2"), "Available points refreshed to 2");
            Check(nodes[0].CurrentState == SkillNodeUIState.Available, "Node 0 (Durability 1) transitioned to Available");
            Check(nodes[0].PurchaseButton.interactable, "Node 0 PurchaseButton is now interactable");
            Check(nodes[0].StatusText.text.Contains("SATIN AL"), "Node 0 StatusText displays 'SATIN AL'");

            Check(nodes[3].CurrentState == SkillNodeUIState.Available, "Node 3 (Power 1) transitioned to Available");
            Check(nodes[6].CurrentState == SkillNodeUIState.Available, "Node 6 (Tempo 1) transitioned to Available");
            Check(nodes[1].CurrentState == SkillNodeUIState.Locked, "Node 1 remains Locked until prereq bought");

            // Step 4: Purchase Node 0 (Durability 1, cost 1)
            nodes[0].PurchaseButton.onClick.Invoke();

            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_1"),
                "Node 0 (Durability 1) successfully purchased in PermanentProgression");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 1,
                "Warrior available points decremented to 1");
            Check(skillTreeUI.AvailablePointsText.text.Contains("1"),
                "UI points text updated to 1");

            Check(nodes[0].CurrentState == SkillNodeUIState.Purchased, "Node 0 transitioned to Purchased");
            Check(!nodes[0].PurchaseButton.interactable, "Node 0 PurchaseButton is now disabled (Purchased)");
            Check(nodes[0].StatusText.text.Contains("SATIN ALINDI"), "Node 0 StatusText displays 'SATIN ALINDI'");

            // Node 1 (Durability 2, cost 1) prereq is met and we have 1 point -> Available!
            Check(nodes[1].CurrentState == SkillNodeUIState.Available,
                "Node 1 (Durability 2) unlocked and transitioned to Available");
            Check(nodes[1].PurchaseButton.interactable, "Node 1 PurchaseButton is interactable");

            // Node 2 remains Locked
            Check(nodes[2].CurrentState == SkillNodeUIState.Locked, "Node 2 (Durability 3) remains Locked");

            // Step 5: Purchase Node 1 (Durability 2, cost 1)
            nodes[1].PurchaseButton.onClick.Invoke();

            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_2"),
                "Node 1 (Durability 2) successfully purchased");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 0,
                "Warrior available points decremented to 0");
            Check(nodes[1].CurrentState == SkillNodeUIState.Purchased, "Node 1 transitioned to Purchased");

            // Node 2 (Durability 3) prereq met, but 0 points -> InsufficientPoints
            Check(nodes[2].CurrentState == SkillNodeUIState.InsufficientPoints,
                "Node 2 (Durability 3) prereq met but points=0 -> InsufficientPoints");
            Check(!nodes[2].PurchaseButton.interactable, "Node 2 PurchaseButton is disabled");

            // Other Tier 1 nodes (Power 1, Tempo 1) return to InsufficientPoints
            Check(nodes[3].CurrentState == SkillNodeUIState.InsufficientPoints,
                "Node 3 (Power 1) transitioned back to InsufficientPoints");
            Check(nodes[6].CurrentState == SkillNodeUIState.InsufficientPoints,
                "Node 6 (Tempo 1) transitioned back to InsufficientPoints");

            DestroyOwned(panelGo);
        }

        private void VerifyExternalProgressionEventSync()
        {
            var warriorDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorDefPath);
            var (panelGo, skillTreeUI) = CreateTestSkillTreeUI(warriorDef);

            skillTreeUI.Open(warriorDef);
            Check(skillTreeUI.NodeUIs[2].CurrentState == SkillNodeUIState.InsufficientPoints,
                "EventSync baseline: Node 2 is InsufficientPoints");

            // External point addition triggers OnSkillPointsChanged
            PermanentProgression.AddSkillPoints("warrior", 1);
            Check(skillTreeUI.AvailablePointsText.text.Contains("1"),
                "EventSync: AvailablePointsText updated reactively via OnSkillPointsChanged");
            Check(skillTreeUI.NodeUIs[2].CurrentState == SkillNodeUIState.Available,
                "EventSync: Node 2 reactively transitioned to Available on external point change");

            // External purchase triggers OnSkillPurchased
            PermanentProgression.TryPurchaseNode("warrior", warriorDef.SkillTree.Nodes[2], warriorDef.SkillTree);
            Check(skillTreeUI.NodeUIs[2].CurrentState == SkillNodeUIState.Purchased,
                "EventSync: Node 2 reactively transitioned to Purchased on external OnSkillPurchased");
            Check(skillTreeUI.AvailablePointsText.text.Contains("0"),
                "EventSync: Available points updated to 0");

            DestroyOwned(panelGo);
        }

        private void VerifyCharacterSwitchingIsolation()
        {
            var warriorDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorDefPath);
            var archerDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherDefPath);
            var gunnerDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(GunnerDefPath);

            var (panelGo, skillTreeUI) = CreateTestSkillTreeUI(warriorDef);

            // Warrior has Durability 1, 2, 3 purchased from previous checks
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_1"), "Warrior Durability 1 is purchased");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_2"), "Warrior Durability 2 is purchased");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_3"), "Warrior Durability 3 is purchased");

            // Switch to Archer
            CharacterSelectionSession.SetSelection(archerDef);
            skillTreeUI.Open(archerDef);

            Check(skillTreeUI.CharacterNameText.text.Contains(archerDef.DisplayName), "SkillTreeUI rebound to Archer");
            Check(skillTreeUI.AvailablePointsText.text.Contains("0"), "Archer has 0 points initially");
            Check(PermanentProgression.GetUnlockedNodeIds("archer").Count == 0,
                "Archer has 0 unlocked nodes (strict isolation from Warrior)");

            // Archer nodes bound properly
            Check(skillTreeUI.NodeUIs[0].BoundNode.Id == "archer_precision_1", "Archer Node 0 is archer_precision_1");
            Check(skillTreeUI.NodeUIs[3].BoundNode.Id == "archer_tempo_1", "Archer Node 3 is archer_tempo_1");
            Check(skillTreeUI.NodeUIs[6].BoundNode.Id == "archer_survival_1", "Archer Node 6 is archer_survival_1");

            // Award Archer 2 points & purchase Precision 1
            PermanentProgression.AddSkillPoints("archer", 2);
            skillTreeUI.RefreshUI();
            skillTreeUI.NodeUIs[0].PurchaseButton.onClick.Invoke();

            Check(PermanentProgression.IsNodePurchased("archer", "archer_precision_1"), "Archer Precision 1 purchased");
            Check(PermanentProgression.GetAvailablePoints("archer") == 1, "Archer has 1 point remaining");
            Check(skillTreeUI.NodeUIs[0].CurrentState == SkillNodeUIState.Purchased, "Archer Node 0 is Purchased");
            Check(skillTreeUI.NodeUIs[1].CurrentState == SkillNodeUIState.Available, "Archer Node 1 is Available");

            // Switch to Gunner
            CharacterSelectionSession.SetSelection(gunnerDef);
            skillTreeUI.Open(gunnerDef);

            Check(skillTreeUI.CharacterNameText.text.Contains(gunnerDef.DisplayName), "SkillTreeUI rebound to Gunner");
            Check(skillTreeUI.AvailablePointsText.text.Contains("0"), "Gunner has 0 points initially");
            Check(PermanentProgression.GetUnlockedNodeIds("gunner").Count == 0,
                "Gunner has 0 unlocked nodes (strict isolation from Warrior & Archer)");
            Check(skillTreeUI.NodeUIs[0].BoundNode.Id == "gunner_firepower_1", "Gunner Node 0 is gunner_firepower_1");

            // Award Gunner 1 point & purchase Firepower 1
            PermanentProgression.AddSkillPoints("gunner", 1);
            skillTreeUI.RefreshUI();
            skillTreeUI.NodeUIs[0].PurchaseButton.onClick.Invoke();

            Check(PermanentProgression.IsNodePurchased("gunner", "gunner_firepower_1"), "Gunner Firepower 1 purchased");
            Check(PermanentProgression.GetAvailablePoints("gunner") == 0, "Gunner has 0 points remaining");

            // Switch back to Warrior
            CharacterSelectionSession.SetSelection(warriorDef);
            skillTreeUI.Open(warriorDef);

            Check(skillTreeUI.CharacterNameText.text.Contains(warriorDef.DisplayName), "SkillTreeUI rebound back to Warrior");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Warrior points preserved at 0");
            Check(skillTreeUI.NodeUIs[0].CurrentState == SkillNodeUIState.Purchased, "Warrior Node 0 still Purchased");
            Check(skillTreeUI.NodeUIs[1].CurrentState == SkillNodeUIState.Purchased, "Warrior Node 1 still Purchased");
            Check(skillTreeUI.NodeUIs[2].CurrentState == SkillNodeUIState.Purchased, "Warrior Node 2 still Purchased");

            // Verify cross-character counts
            Check(PermanentProgression.GetUnlockedNodeIds("warrior").Count == 3, "Warrior has exactly 3 unlocked nodes");
            Check(PermanentProgression.GetUnlockedNodeIds("archer").Count == 1, "Archer has exactly 1 unlocked node");
            Check(PermanentProgression.GetUnlockedNodeIds("gunner").Count == 1, "Gunner has exactly 1 unlocked node");

            DestroyOwned(panelGo);
        }

        private void VerifySpawnTimeGameplayIntegrationAndStatLayering()
        {
            var warriorDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorDefPath);
            Check(warriorDef != null, "Warrior definition exists for spawn test");

            // In our previous tests, Warrior unlocked Durability 1 (+10%), Durability 2 (+10%), Durability 3 (+15%) -> Total +35% MaxHealth
            var mods = PermanentProgression.GetPermanentModifiers("warrior", warriorDef.SkillTree);
            Check(Mathf.Approximately(mods.maxHealthMultiplier, 1.35f),
                "Warrior permanent maxHealthMultiplier is exactly 1.35");

            // Test player spawning binding
            var spawnerGo = Own(new GameObject("TestPlayerSpawner"));
            spawnerGo.SetActive(false);
            var spawner = spawnerGo.AddComponent<PlayerSpawner>();
            typeof(PlayerSpawner).GetField("autoSpawn", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(spawner, false);

            var spawnPt = Own(new GameObject("SpawnPoint"));
            spawner.SetSpawnPoint(spawnPt.transform);
            spawner.SetDefaultCharacter(warriorDef);
            spawnerGo.SetActive(true);

            CharacterSelectionSession.SetSelection(warriorDef);
            var character = spawner.Spawn(warriorDef);
            Own(character.gameObject);

            var stats = character.GetComponent<PlayerStats>();
            var health = character.GetComponent<PlayerHealth>();
            var weapon = character.GetComponent<MeleeWeapon>();

            Check(stats != null && health != null && weapon != null,
                "Spawned character has PlayerStats, PlayerHealth, MeleeWeapon");
            Check(Mathf.Approximately(stats.PermanentMaxHealthMultiplier, 1.35f),
                "PlayerStats.PermanentMaxHealthMultiplier is 1.35");
            Check(Mathf.Approximately(health.MaxHealth, 135f),
                "PlayerHealth.MaxHealth is 135 (100 base * 1.35)");
            Check(Mathf.Approximately(health.CurrentHealth, 135f),
                "PlayerHealth.CurrentHealth starts at full (135/135 fresh health contract)");

            // Layer temporary upgrade on stats: with Durability branch only, permanent damage is 1.00
            Check(Mathf.Approximately(stats.PermanentDamageMultiplier, 1.00f),
                "PermanentDamageMultiplier is 1.00 before Power branch");
            stats.AddDamageBonus(0.20f);
            Check(Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.20f),
                "TemporaryDamageMultiplier is 1.20");
            Check(Mathf.Approximately(stats.DamageMultiplier, 1.20f),
                "Layered DamageMultiplier is 1.20 (1.20 temp * 1.00 perm)");
            Check(Mathf.Approximately(weapon.EffectiveDamage, 30.0f),
                "Layered EffectiveDamage is 30.0 (25 base * 1.20)");

            DestroySpawnerAndCharacter(spawner, spawnerGo, spawnPt, character);
        }

        private void VerifyCampaignEconomyAndIdempotencyIntegration()
        {
            PermanentProgression.ResetAllProgression();

            // First-clear D1 with Warrior: 2 points
            bool d1_first = PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_1", out int ptsD1);
            Check(d1_first && ptsD1 == 2, "Warrior D1 first clear awards 2 points");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Warrior has 2 points");

            // Replay D1 with Warrior: 0 points (idempotent)
            bool d1_repeat = PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_1", out int ptsD1Repeat);
            Check(!d1_repeat && ptsD1Repeat == 0, "Warrior D1 replay awards 0 points (strict idempotency)");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Warrior points remain 2");

            // Archer clears D1: awards 2 points to Archer
            bool archer_d1 = PermanentProgression.TryAwardDungeonFirstClear("archer", "dungeon_1", out int ptsArcher);
            Check(archer_d1 && ptsArcher == 2, "Archer D1 first clear awards 2 points (isolated from Warrior)");
            Check(PermanentProgression.GetAvailablePoints("archer") == 2, "Archer has 2 points");

            // Warrior clears D2 (2 pts) and D3 (3 pts) -> 7 points total
            PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_2", out int ptsD2);
            PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_3", out int ptsD3);
            Check(ptsD2 == 2 && ptsD3 == 3, "Warrior D2 awards 2 pts, D3 awards 3 pts");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 7, "Warrior reaches 7-point campaign cap");
        }

        private void VerifyFailSafeHandling()
        {
            var warriorDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorDefPath);
            var (panelGo, skillTreeUI) = CreateTestSkillTreeUI(warriorDef);

            // Null session character falls back to defaultCharacter (Warrior)
            CharacterSelectionSession.Clear();
            skillTreeUI.Open(null);
            Check(skillTreeUI.IsOpen, "Open(null) opens successfully via defaultCharacter fallback");
            Check(skillTreeUI.ActiveCharacter != null && skillTreeUI.ActiveCharacter.Id == "warrior",
                "Fallback character resolved to Warrior");

            // Closing when already closed is safe
            skillTreeUI.Close();
            skillTreeUI.Close();
            Check(!skillTreeUI.IsOpen, "Double close handles safely");

            // Reopening restores state
            skillTreeUI.Open(warriorDef);
            Check(skillTreeUI.IsOpen, "Reopen succeeds");

            DestroyOwned(panelGo);
        }

        private (GameObject panelGo, SkillTreeUI skillTreeUI) CreateTestSkillTreeUI(CharacterDefinition heroDef)
        {
            var panelGo = Own(new GameObject("TestSkillTreePanel"));
            var skillTreeUI = panelGo.AddComponent<SkillTreeUI>();

            var heroTMP = Own(new GameObject("HeroNameText")).AddComponent<TextMeshProUGUI>();
            heroTMP.transform.SetParent(panelGo.transform, false);

            var pointsTMP = Own(new GameObject("PointsText")).AddComponent<TextMeshProUGUI>();
            pointsTMP.transform.SetParent(panelGo.transform, false);

            var closeBtn = Own(new GameObject("CloseButton")).AddComponent<Button>();
            closeBtn.transform.SetParent(panelGo.transform, false);

            var nodeCards = new SkillTreeNodeUI[9];
            for (int i = 0; i < 9; i++)
            {
                var cardGo = Own(new GameObject($"NodeCard_{i}"));
                cardGo.transform.SetParent(panelGo.transform, false);

                var bgImg = cardGo.AddComponent<Image>();
                var btn = cardGo.AddComponent<Button>();
                var borderImg = Own(new GameObject("Border")).AddComponent<Image>();
                borderImg.transform.SetParent(cardGo.transform, false);

                var nameTMP = Own(new GameObject("Name")).AddComponent<TextMeshProUGUI>();
                nameTMP.transform.SetParent(cardGo.transform, false);

                var descTMP = Own(new GameObject("Desc")).AddComponent<TextMeshProUGUI>();
                descTMP.transform.SetParent(cardGo.transform, false);

                var costTMP = Own(new GameObject("Cost")).AddComponent<TextMeshProUGUI>();
                costTMP.transform.SetParent(cardGo.transform, false);

                var statusTMP = Own(new GameObject("Status")).AddComponent<TextMeshProUGUI>();
                statusTMP.transform.SetParent(cardGo.transform, false);

                var nodeUI = cardGo.AddComponent<SkillTreeNodeUI>();
                nodeUI.SetReferences(nameTMP, descTMP, costTMP, statusTMP, btn, bgImg, borderImg);
                nodeCards[i] = nodeUI;
            }

            var branchTMPs = new TextMeshProUGUI[3];
            for (int b = 0; b < 3; b++)
            {
                branchTMPs[b] = Own(new GameObject($"Branch_{b}")).AddComponent<TextMeshProUGUI>();
                branchTMPs[b].transform.SetParent(panelGo.transform, false);
            }

            skillTreeUI.SetReferences(panelGo, heroTMP, pointsTMP, closeBtn, nodeCards, branchTMPs, heroDef);
            return (panelGo, skillTreeUI);
        }
    }
}
#endif
