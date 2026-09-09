#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Weapons;

namespace DungeonRoguelite.Tests
{
    public sealed class Milestone12_3_Verifier : MonoBehaviour
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
                VerifyWarriorTreeAssetStructureAndValidation();
                VerifyWarriorTreePurchaseProgression();
                VerifyPermanentStatAggregationAcrossTiers();
                VerifyPlayerSpawnerFreshWarriorBaseline();
                VerifyPlayerSpawnerUpgradedWarriorSpawnBinding();
                VerifyTemporaryUpgradeLayeringOnUpgradedWarrior();
                VerifyRunSemanticsCompatibility();
                VerifyCharacterIsolationAtSpawn();
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
            Debug.Log($"[GATE 12.3 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
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
            if (!condition) throw new InvalidOperationException("[GATE 12.3 FAILED] " + description);
            Debug.Log($"[GATE 12.3 CHECK {++checks} PASSED] {description}");
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

        private (PlayerSpawner spawner, GameObject spawnerGo, GameObject spawnPt) CreateTestSpawner(CharacterDefinition def)
        {
            var spawnerGo = Own(new GameObject("TestPlayerSpawner"));
            spawnerGo.SetActive(false);
            var spawner = spawnerGo.AddComponent<PlayerSpawner>();
            typeof(PlayerSpawner).GetField("autoSpawn", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(spawner, false);
            var spawnPt = Own(new GameObject("SpawnPoint"));
            spawner.SetSpawnPoint(spawnPt.transform);
            spawner.SetDefaultCharacter(def);
            spawnerGo.SetActive(true);
            return (spawner, spawnerGo, spawnPt);
        }

        private void DestroySpawnerAndCharacter(PlayerSpawner spawner, GameObject spawnerGo, GameObject spawnPt, PlayableCharacter character)
        {
            if (character != null) DestroyOwned(character.gameObject);
            if (spawnerGo != null) DestroyOwned(spawnerGo);
            if (spawnPt != null) DestroyOwned(spawnPt);
        }

        private SkillTreeDefinition LoadWarriorTree()
        {
            return AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>("Assets/ScriptableObjects/Progression/SkillTree_Warrior.asset");
        }

        private CharacterDefinition LoadWarriorDefinition()
        {
            return AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Warrior.asset");
        }

        private CharacterDefinition LoadArcherDefinition()
        {
            return AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Archer.asset");
        }

        private CharacterDefinition LoadGunnerDefinition()
        {
            return AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Gunner.asset");
        }

        private void VerifyWarriorTreeAssetStructureAndValidation()
        {
            var tree = LoadWarriorTree();
            Check(tree != null, "SkillTree_Warrior.asset exists and loaded successfully");
            Check(string.Equals(tree.CharacterId, "warrior", StringComparison.OrdinalIgnoreCase), "Tree CharacterId is 'warrior'");
            Check(tree.Nodes != null && tree.Nodes.Count == 9, "Tree has exactly 9 nodes (3 branches x 3 tiers)");
            Check(tree.ValidateTree(out string validationError), $"Tree passes ValidateTree cleanly (error: '{validationError}')");
            Check(string.IsNullOrEmpty(validationError), "Validation error message is empty");

            // Branch 1: Durability (MaxHealthMultiplier: +10%, +10%, +15%)
            Check(tree.TryGetNode("warrior_durability_1", out var d1), "Durability Tier 1 node exists (warrior_durability_1)");
            Check(d1.BranchId == "durability" && d1.Tier == 1 && d1.Cost == 1, "warrior_durability_1: branch=durability, tier=1, cost=1");
            Check(!d1.HasPrerequisite && string.IsNullOrEmpty(d1.PrerequisiteNodeId), "warrior_durability_1 has no prerequisite");
            Check(d1.EffectType == PermanentEffectType.MaxHealthMultiplier && Mathf.Approximately(d1.EffectMagnitude, 0.10f), "warrior_durability_1: effect=MaxHealthMultiplier, magnitude=+10%");

            Check(tree.TryGetNode("warrior_durability_2", out var d2), "Durability Tier 2 node exists (warrior_durability_2)");
            Check(d2.BranchId == "durability" && d2.Tier == 2 && d2.Cost == 1, "warrior_durability_2: branch=durability, tier=2, cost=1");
            Check(d2.PrerequisiteNodeId == "warrior_durability_1", "warrior_durability_2 prereq is warrior_durability_1");
            Check(d2.EffectType == PermanentEffectType.MaxHealthMultiplier && Mathf.Approximately(d2.EffectMagnitude, 0.10f), "warrior_durability_2: effect=MaxHealthMultiplier, magnitude=+10%");

            Check(tree.TryGetNode("warrior_durability_3", out var d3), "Durability Tier 3 node exists (warrior_durability_3)");
            Check(d3.BranchId == "durability" && d3.Tier == 3 && d3.Cost == 1, "warrior_durability_3: branch=durability, tier=3, cost=1");
            Check(d3.PrerequisiteNodeId == "warrior_durability_2", "warrior_durability_3 prereq is warrior_durability_2");
            Check(d3.EffectType == PermanentEffectType.MaxHealthMultiplier && Mathf.Approximately(d3.EffectMagnitude, 0.15f), "warrior_durability_3: effect=MaxHealthMultiplier, magnitude=+15%");

            // Branch 2: Power (DamageMultiplier: +10%, +10%, +15%)
            Check(tree.TryGetNode("warrior_power_1", out var p1), "Power Tier 1 node exists (warrior_power_1)");
            Check(p1.BranchId == "power" && p1.Tier == 1 && p1.Cost == 1, "warrior_power_1: branch=power, tier=1, cost=1");
            Check(!p1.HasPrerequisite && string.IsNullOrEmpty(p1.PrerequisiteNodeId), "warrior_power_1 has no prerequisite");
            Check(p1.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(p1.EffectMagnitude, 0.10f), "warrior_power_1: effect=DamageMultiplier, magnitude=+10%");

            Check(tree.TryGetNode("warrior_power_2", out var p2), "Power Tier 2 node exists (warrior_power_2)");
            Check(p2.BranchId == "power" && p2.Tier == 2 && p2.Cost == 1, "warrior_power_2: branch=power, tier=2, cost=1");
            Check(p2.PrerequisiteNodeId == "warrior_power_1", "warrior_power_2 prereq is warrior_power_1");
            Check(p2.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(p2.EffectMagnitude, 0.10f), "warrior_power_2: effect=DamageMultiplier, magnitude=+10%");

            Check(tree.TryGetNode("warrior_power_3", out var p3), "Power Tier 3 node exists (warrior_power_3)");
            Check(p3.BranchId == "power" && p3.Tier == 3 && p3.Cost == 1, "warrior_power_3: branch=power, tier=3, cost=1");
            Check(p3.PrerequisiteNodeId == "warrior_power_2", "warrior_power_3 prereq is warrior_power_2");
            Check(p3.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(p3.EffectMagnitude, 0.15f), "warrior_power_3: effect=DamageMultiplier, magnitude=+15%");

            // Branch 3: Tempo (AttackSpeedMultiplier +5%, MovementSpeedMultiplier +5%, AttackSpeedMultiplier +10%)
            Check(tree.TryGetNode("warrior_tempo_1", out var t1), "Tempo Tier 1 node exists (warrior_tempo_1)");
            Check(t1.BranchId == "tempo" && t1.Tier == 1 && t1.Cost == 1, "warrior_tempo_1: branch=tempo, tier=1, cost=1");
            Check(!t1.HasPrerequisite && string.IsNullOrEmpty(t1.PrerequisiteNodeId), "warrior_tempo_1 has no prerequisite");
            Check(t1.EffectType == PermanentEffectType.AttackSpeedMultiplier && Mathf.Approximately(t1.EffectMagnitude, 0.05f), "warrior_tempo_1: effect=AttackSpeedMultiplier, magnitude=+5%");

            Check(tree.TryGetNode("warrior_tempo_2", out var t2), "Tempo Tier 2 node exists (warrior_tempo_2)");
            Check(t2.BranchId == "tempo" && t2.Tier == 2 && t2.Cost == 1, "warrior_tempo_2: branch=tempo, tier=2, cost=1");
            Check(t2.PrerequisiteNodeId == "warrior_tempo_1", "warrior_tempo_2 prereq is warrior_tempo_1");
            Check(t2.EffectType == PermanentEffectType.MovementSpeedMultiplier && Mathf.Approximately(t2.EffectMagnitude, 0.05f), "warrior_tempo_2: effect=MovementSpeedMultiplier, magnitude=+5%");

            Check(tree.TryGetNode("warrior_tempo_3", out var t3), "Tempo Tier 3 node exists (warrior_tempo_3)");
            Check(t3.BranchId == "tempo" && t3.Tier == 3 && t3.Cost == 1, "warrior_tempo_3: branch=tempo, tier=3, cost=1");
            Check(t3.PrerequisiteNodeId == "warrior_tempo_2", "warrior_tempo_3 prereq is warrior_tempo_2");
            Check(t3.EffectType == PermanentEffectType.AttackSpeedMultiplier && Mathf.Approximately(t3.EffectMagnitude, 0.10f), "warrior_tempo_3: effect=AttackSpeedMultiplier, magnitude=+10%");

            // Character_Warrior binding
            var warriorDef = LoadWarriorDefinition();
            Check(warriorDef != null, "Character_Warrior.asset exists and loaded");
            Check(warriorDef.SkillTree != null, "Character_Warrior.SkillTree is assigned");
            Check(warriorDef.SkillTree == tree, "Character_Warrior.SkillTree matches SkillTree_Warrior.asset");
        }

        private void VerifyWarriorTreePurchaseProgression()
        {
            PermanentProgression.ResetAllProgression();
            var tree = LoadWarriorTree();
            tree.TryGetNode("warrior_durability_1", out var d1);
            tree.TryGetNode("warrior_durability_2", out var d2);
            tree.TryGetNode("warrior_durability_3", out var d3);
            tree.TryGetNode("warrior_power_1", out var p1);
            tree.TryGetNode("warrior_power_2", out var p2);
            tree.TryGetNode("warrior_power_3", out var p3);
            tree.TryGetNode("warrior_tempo_1", out var t1);
            tree.TryGetNode("warrior_tempo_2", out var t2);

            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Warrior starts with 0 available points");
            Check(!PermanentProgression.CanPurchaseNode("warrior", d1, tree), "Cannot purchase Tier 1 node with 0 points");
            Check(!PermanentProgression.TryPurchaseNode("warrior", d1, tree), "TryPurchaseNode fails with 0 points");

            // Award 1 point
            PermanentProgression.AddSkillPoints("warrior", 1);
            Check(PermanentProgression.GetAvailablePoints("warrior") == 1, "Warrior has 1 point after awarding");
            Check(!PermanentProgression.CanPurchaseNode("warrior", d2, tree), "Cannot purchase Tier 2 node before Tier 1 prerequisite");
            Check(!PermanentProgression.CanPurchaseNode("warrior", d3, tree), "Cannot purchase Tier 3 node before Tier 2 prerequisite");
            Check(!PermanentProgression.CanPurchaseNode("archer", d1, tree), "Archer cannot purchase Warrior node");

            // Purchase Tier 1
            bool purchasedD1 = PermanentProgression.TryPurchaseNode("warrior", d1, tree);
            Check(purchasedD1, "Warrior successfully purchased warrior_durability_1");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Available points decremented to 0");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_1"), "warrior_durability_1 is recorded as purchased");
            Check(!PermanentProgression.CanPurchaseNode("warrior", d1, tree), "Cannot purchase already-purchased node");
            Check(!PermanentProgression.TryPurchaseNode("warrior", d1, tree), "Duplicate TryPurchaseNode returns false");

            // Award 6 more points (7 points total, matching full campaign clear economy: D1=2, D2=2, D3=3)
            PermanentProgression.AddSkillPoints("warrior", 6);
            Check(PermanentProgression.GetAvailablePoints("warrior") == 6, "Warrior has 6 points available");

            // Complete Durability branch: T2, T3
            Check(PermanentProgression.TryPurchaseNode("warrior", d2, tree), "Purchased warrior_durability_2 (T2)");
            Check(PermanentProgression.TryPurchaseNode("warrior", d3, tree), "Purchased warrior_durability_3 (T3)");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 4, "Points remaining: 4");

            // Complete Power branch: T1, T2, T3
            Check(PermanentProgression.TryPurchaseNode("warrior", p1, tree), "Purchased warrior_power_1 (T1)");
            Check(PermanentProgression.TryPurchaseNode("warrior", p2, tree), "Purchased warrior_power_2 (T2)");
            Check(PermanentProgression.TryPurchaseNode("warrior", p3, tree), "Purchased warrior_power_3 (T3)");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 1, "Points remaining: 1");

            // Start Tempo branch: T1
            Check(PermanentProgression.TryPurchaseNode("warrior", t1, tree), "Purchased warrior_tempo_1 (T1)");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Points remaining: 0 (7 total nodes purchased)");

            // Verify 7 nodes are purchased and remaining 2 are locked
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_1"), "Purchased: warrior_durability_1");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_2"), "Purchased: warrior_durability_2");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_3"), "Purchased: warrior_durability_3");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_power_1"), "Purchased: warrior_power_1");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_power_2"), "Purchased: warrior_power_2");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_power_3"), "Purchased: warrior_power_3");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_tempo_1"), "Purchased: warrior_tempo_1");
            Check(!PermanentProgression.IsNodePurchased("warrior", "warrior_tempo_2"), "Unpurchased: warrior_tempo_2");
            Check(!PermanentProgression.IsNodePurchased("warrior", "warrior_tempo_3"), "Unpurchased: warrior_tempo_3");
            Check(!PermanentProgression.CanPurchaseNode("warrior", t2, tree), "Cannot purchase warrior_tempo_2 with 0 points");

            // Verify reload persistence
            var isLoadedField = typeof(PermanentProgression).GetField("isLoaded", BindingFlags.NonPublic | BindingFlags.Static);
            isLoadedField.SetValue(null, false);
            var cacheField = typeof(PermanentProgression).GetField("characterCache", BindingFlags.NonPublic | BindingFlags.Static);
            var dict = cacheField.GetValue(null) as System.Collections.IDictionary;
            dict?.Clear();

            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Persisted: Warrior points are 0 after simulated reload");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_3"), "Persisted: warrior_durability_3 purchased after reload");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_power_3"), "Persisted: warrior_power_3 purchased after reload");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_tempo_1"), "Persisted: warrior_tempo_1 purchased after reload");
        }

        private void VerifyPermanentStatAggregationAcrossTiers()
        {
            var tree = LoadWarriorTree();

            // Check multipliers with the 7 campaign-purchased nodes:
            // Durability: +10% +10% +15% = +35% Max HP (1.35x)
            // Power: +10% +10% +15% = +35% Damage (1.35x)
            // Tempo: +5% AtkSpd = +5% Attack Speed (1.05x), +0% Move Speed (1.00x)
            var mods7 = PermanentProgression.GetPermanentModifiers("warrior", tree);
            Check(Mathf.Approximately(mods7.maxHealthMultiplier, 1.35f), "Aggregated (7 nodes): maxHealthMultiplier == 1.35 (+35%)");
            Check(Mathf.Approximately(mods7.damageMultiplier, 1.35f), "Aggregated (7 nodes): damageMultiplier == 1.35 (+35%)");
            Check(Mathf.Approximately(mods7.attackSpeedMultiplier, 1.05f), "Aggregated (7 nodes): attackSpeedMultiplier == 1.05 (+5%)");
            Check(Mathf.Approximately(mods7.movementSpeedMultiplier, 1.00f), "Aggregated (7 nodes): movementSpeedMultiplier == 1.00 (+0%)");

            // Award 2 additional points to complete the entire 9-node tree
            PermanentProgression.AddSkillPoints("warrior", 2);
            tree.TryGetNode("warrior_tempo_2", out var t2);
            tree.TryGetNode("warrior_tempo_3", out var t3);
            Check(PermanentProgression.TryPurchaseNode("warrior", t2, tree), "Purchased warrior_tempo_2 (+5% Move Speed)");
            Check(PermanentProgression.TryPurchaseNode("warrior", t3, tree), "Purchased warrior_tempo_3 (+10% Attack Speed)");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Warrior available points == 0 after all 9 nodes");

            // Complete 9-node tree multipliers:
            // Durability: 1.35x Max HP
            // Power: 1.35x Damage
            // Tempo: 1.15x Attack Speed (5% + 10%), 1.05x Move Speed (5%)
            var mods9 = PermanentProgression.GetPermanentModifiers("warrior", tree);
            Check(Mathf.Approximately(mods9.maxHealthMultiplier, 1.35f), "Aggregated (9 nodes): maxHealthMultiplier == 1.35 (+35%)");
            Check(Mathf.Approximately(mods9.damageMultiplier, 1.35f), "Aggregated (9 nodes): damageMultiplier == 1.35 (+35%)");
            Check(Mathf.Approximately(mods9.attackSpeedMultiplier, 1.15f), "Aggregated (9 nodes): attackSpeedMultiplier == 1.15 (+15%)");
            Check(Mathf.Approximately(mods9.movementSpeedMultiplier, 1.05f), "Aggregated (9 nodes): movementSpeedMultiplier == 1.05 (+5%)");
        }

        private void VerifyPlayerSpawnerFreshWarriorBaseline()
        {
            PermanentProgression.ResetAllProgression();
            var warriorDef = LoadWarriorDefinition();

            var (spawner, spawnerGo, spawnPt) = CreateTestSpawner(warriorDef);
            var character = spawner.Spawn(warriorDef);
            Own(character.gameObject);

            Check(character != null, "Fresh Warrior successfully spawned");

            var health = character.GetComponent<PlayerHealth>();
            Check(health != null, "Spawned Warrior has PlayerHealth");
            Check(Mathf.Approximately(health.BaseMaxHealth, 100f), "Fresh Warrior: BaseMaxHealth == 100");
            Check(Mathf.Approximately(health.MaxHealth, 100f), "Fresh Warrior: MaxHealth == 100");
            Check(Mathf.Approximately(health.CurrentHealth, 100f), "Fresh Warrior: CurrentHealth == 100");
            Check(Mathf.Approximately(health.HealthNormalized, 1.0f), "Fresh Warrior: HealthNormalized == 1.0");

            var stats = character.GetComponent<PlayerStats>();
            Check(stats != null, "Spawned Warrior has PlayerStats");
            Check(Mathf.Approximately(stats.PermanentMaxHealthMultiplier, 1.0f), "Fresh Warrior: PermanentMaxHealthMultiplier == 1.0");
            Check(Mathf.Approximately(stats.PermanentDamageMultiplier, 1.0f), "Fresh Warrior: PermanentDamageMultiplier == 1.0");
            Check(Mathf.Approximately(stats.PermanentAttackSpeedMultiplier, 1.0f), "Fresh Warrior: PermanentAttackSpeedMultiplier == 1.0");
            Check(Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.0f), "Fresh Warrior: PermanentMovementSpeedMultiplier == 1.0");
            Check(Mathf.Approximately(stats.DamageMultiplier, 1.0f), "Fresh Warrior: Effective DamageMultiplier == 1.0");
            Check(Mathf.Approximately(stats.AttackSpeedMultiplier, 1.0f), "Fresh Warrior: Effective AttackSpeedMultiplier == 1.0");
            Check(Mathf.Approximately(stats.MovementSpeedMultiplier, 1.0f), "Fresh Warrior: Effective MovementSpeedMultiplier == 1.0");

            var weapon = character.GetComponent<MeleeWeapon>();
            Check(weapon != null, "Spawned Warrior has MeleeWeapon");
            Check(Mathf.Approximately(weapon.BaseDamage, 25f), "Fresh Warrior: BaseDamage == 25");
            Check(Mathf.Approximately(weapon.EffectiveDamage, 25f), "Fresh Warrior: EffectiveDamage == 25");
            Check(Mathf.Approximately(weapon.AttackCooldown, 0.5f), "Fresh Warrior: AttackCooldown == 0.5");
            Check(Mathf.Approximately(weapon.EffectiveAttackCooldown, 0.5f), "Fresh Warrior: EffectiveAttackCooldown == 0.5");

            var movement = character.GetComponent<PlayerMovement>();
            Check(movement != null, "Spawned Warrior has PlayerMovement");
            Check(Mathf.Approximately(movement.MoveSpeed, 6.0f), "Fresh Warrior: MoveSpeed == 6.0");
            Check(Mathf.Approximately(movement.EffectiveMoveSpeed, 6.0f), "Fresh Warrior: EffectiveMoveSpeed == 6.0");

            DestroySpawnerAndCharacter(spawner, spawnerGo, spawnPt, character);
        }

        private void VerifyPlayerSpawnerUpgradedWarriorSpawnBinding()
        {
            // Set up all 9 nodes purchased on Warrior
            PermanentProgression.ResetAllProgression();
            PermanentProgression.AddSkillPoints("warrior", 9);
            var tree = LoadWarriorTree();
            foreach (var node in tree.Nodes)
            {
                PermanentProgression.TryPurchaseNode("warrior", node, tree);
            }

            var warriorDef = LoadWarriorDefinition();
            var (spawner, spawnerGo, spawnPt) = CreateTestSpawner(warriorDef);
            var character = spawner.Spawn(warriorDef);
            Own(character.gameObject);

            Check(character != null, "Upgraded Warrior successfully spawned");

            // Permanent multipliers bound to PlayerStats
            var stats = character.GetComponent<PlayerStats>();
            Check(stats != null, "Upgraded Warrior has PlayerStats");
            Check(Mathf.Approximately(stats.PermanentMaxHealthMultiplier, 1.35f), "Upgraded Warrior: PermanentMaxHealthMultiplier == 1.35");
            Check(Mathf.Approximately(stats.PermanentDamageMultiplier, 1.35f), "Upgraded Warrior: PermanentDamageMultiplier == 1.35");
            Check(Mathf.Approximately(stats.PermanentAttackSpeedMultiplier, 1.15f), "Upgraded Warrior: PermanentAttackSpeedMultiplier == 1.15");
            Check(Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.05f), "Upgraded Warrior: PermanentMovementSpeedMultiplier == 1.05");

            // Fresh health rule: MaxHealth = 100 * 1.35 = 135, CurrentHealth = 135
            var health = character.GetComponent<PlayerHealth>();
            Check(health != null, "Upgraded Warrior has PlayerHealth");
            Check(Mathf.Approximately(health.BaseMaxHealth, 100f), "Upgraded Warrior: BaseMaxHealth == 100");
            Check(Mathf.Approximately(health.MaxHealth, 135f), "Upgraded Warrior: MaxHealth scaled to 135 (100 * 1.35)");
            Check(Mathf.Approximately(health.CurrentHealth, 135f), "Upgraded Warrior: CurrentHealth initialized to full 135 (Fresh Health Rule)");
            Check(Mathf.Approximately(health.HealthNormalized, 1.0f), "Upgraded Warrior: HealthNormalized == 1.0");

            // Melee weapon stats derived from permanent multipliers
            var weapon = character.GetComponent<MeleeWeapon>();
            Check(weapon != null, "Upgraded Warrior has MeleeWeapon");
            Check(Mathf.Approximately(weapon.BaseDamage, 25f), "Upgraded Warrior: BaseDamage == 25");
            Check(Mathf.Approximately(weapon.EffectiveDamage, 33.75f), "Upgraded Warrior: EffectiveDamage == 33.75 (25 * 1.35)");
            Check(Mathf.Approximately(weapon.AttackCooldown, 0.5f), "Upgraded Warrior: AttackCooldown == 0.5");
            float expectedCooldown = 0.5f / 1.15f;
            Check(Mathf.Abs(weapon.EffectiveAttackCooldown - expectedCooldown) < 0.001f, $"Upgraded Warrior: EffectiveAttackCooldown == {weapon.EffectiveAttackCooldown:F4} (expected ~{expectedCooldown:F4})");

            // Movement speed derived from permanent multipliers
            var movement = character.GetComponent<PlayerMovement>();
            Check(movement != null, "Upgraded Warrior has PlayerMovement");
            Check(Mathf.Approximately(movement.MoveSpeed, 6.0f), "Upgraded Warrior: MoveSpeed == 6.0");
            Check(Mathf.Approximately(movement.EffectiveMoveSpeed, 6.3f), "Upgraded Warrior: EffectiveMoveSpeed == 6.3 (6.0 * 1.05)");

            DestroySpawnerAndCharacter(spawner, spawnerGo, spawnPt, character);
        }

        private void VerifyTemporaryUpgradeLayeringOnUpgradedWarrior()
        {
            var warriorDef = LoadWarriorDefinition();
            var (spawner, spawnerGo, spawnPt) = CreateTestSpawner(warriorDef);
            var character = spawner.Spawn(warriorDef);
            Own(character.gameObject);

            var stats = character.GetComponent<PlayerStats>();
            var health = character.GetComponent<PlayerHealth>();
            var weapon = character.GetComponent<MeleeWeapon>();
            var movement = character.GetComponent<PlayerMovement>();

            // Layer temporary damage upgrade (+20%)
            stats.AddDamageBonus(0.20f);
            Check(Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.20f), "Layering: TemporaryDamageMultiplier == 1.20");
            Check(Mathf.Approximately(stats.PermanentDamageMultiplier, 1.35f), "Layering: PermanentDamageMultiplier == 1.35");
            Check(Mathf.Approximately(stats.DamageMultiplier, 1.62f), "Layering: Effective DamageMultiplier == 1.62 (1.20 temp * 1.35 perm)");
            Check(Mathf.Approximately(weapon.EffectiveDamage, 40.5f), "Layering: MeleeWeapon EffectiveDamage == 40.5 (25 * 1.62)");

            // Layer temporary attack speed upgrade (+15%)
            stats.AddAttackSpeedBonus(0.15f);
            Check(Mathf.Approximately(stats.TemporaryAttackSpeedMultiplier, 1.15f), "Layering: TemporaryAttackSpeedMultiplier == 1.15");
            Check(Mathf.Approximately(stats.PermanentAttackSpeedMultiplier, 1.15f), "Layering: PermanentAttackSpeedMultiplier == 1.15");
            Check(Mathf.Approximately(stats.AttackSpeedMultiplier, 1.3225f), "Layering: Effective AttackSpeedMultiplier == 1.3225 (1.15 temp * 1.15 perm)");
            float expectedLayeredCd = 0.5f / 1.3225f;
            Check(Mathf.Abs(weapon.EffectiveAttackCooldown - expectedLayeredCd) < 0.001f, $"Layering: EffectiveAttackCooldown == {weapon.EffectiveAttackCooldown:F4} (expected ~{expectedLayeredCd:F4})");

            // Layer temporary movement speed upgrade (+10%)
            stats.AddMovementSpeedBonus(0.10f);
            Check(Mathf.Approximately(stats.TemporaryMovementSpeedMultiplier, 1.10f), "Layering: TemporaryMovementSpeedMultiplier == 1.10");
            Check(Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.05f), "Layering: PermanentMovementSpeedMultiplier == 1.05");
            Check(Mathf.Approximately(stats.MovementSpeedMultiplier, 1.155f), "Layering: Effective MovementSpeedMultiplier == 1.155 (1.10 temp * 1.05 perm)");
            float expectedLayeredSpeed = 6.0f * 1.155f;
            Check(Mathf.Abs(movement.EffectiveMoveSpeed - expectedLayeredSpeed) < 0.001f, $"Layering: EffectiveMoveSpeed == {movement.EffectiveMoveSpeed:F4} (expected ~{expectedLayeredSpeed:F4})");

            // Combat damage reception does not affect multipliers
            health.TakeDamage(45f);
            Check(Mathf.Approximately(health.CurrentHealth, 90f), "Combat: Health is 90 / 135 after 45 damage");
            Check(Mathf.Approximately(stats.DamageMultiplier, 1.62f), "Combat: DamageMultiplier unaffected by health reduction");

            // Reset temporary modifiers (simulating run conclusion or death rollback)
            stats.ResetModifiers();
            Check(Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.00f), "Reset: TemporaryDamageMultiplier reset to 1.00");
            Check(Mathf.Approximately(stats.DamageMultiplier, 1.35f), "Reset: Permanent multiplier 1.35x preserved after temporary reset");
            Check(Mathf.Approximately(weapon.EffectiveDamage, 33.75f), "Reset: Weapon EffectiveDamage returns to 33.75");

            DestroySpawnerAndCharacter(spawner, spawnerGo, spawnPt, character);
        }

        private void VerifyRunSemanticsCompatibility()
        {
            var warriorDef = LoadWarriorDefinition();

            // Run Start & Checkpoint
            RunProgressionSession.StartNewRun("warrior");
            RunProgressionSession.CreateDungeonCheckpoint();

            var (spawner1, spawnerGo1, spawnPt1) = CreateTestSpawner(warriorDef);
            var character1 = spawner1.Spawn(warriorDef);
            Own(character1.gameObject);

            var stats1 = character1.GetComponent<PlayerStats>();
            var health1 = character1.GetComponent<PlayerHealth>();
            stats1.AddDamageBonus(0.20f);
            health1.TakeDamage(75f);
            Check(Mathf.Approximately(health1.CurrentHealth, 60f), "Run: Player damaged to 60 / 135");
            Check(Mathf.Approximately(stats1.DamageMultiplier, 1.62f), "Run: Temporary bonus active (1.62x)");

            // Defeat Retry: restore checkpoint, respawn fresh player
            RunProgressionSession.RestoreCheckpointOnRetry();
            DestroySpawnerAndCharacter(spawner1, spawnerGo1, spawnPt1, character1);

            var (spawner2, spawnerGo2, spawnPt2) = CreateTestSpawner(warriorDef);
            var character2 = spawner2.Spawn(warriorDef);
            Own(character2.gameObject);

            var stats2 = character2.GetComponent<PlayerStats>();
            var health2 = character2.GetComponent<PlayerHealth>();
            Check(Mathf.Approximately(stats2.TemporaryDamageMultiplier, 1.0f), "Retry: TemporaryDamageMultiplier restored to checkpoint neutral (1.0)");
            Check(Mathf.Approximately(stats2.PermanentDamageMultiplier, 1.35f), "Retry: PermanentDamageMultiplier preserved (1.35)");
            Check(Mathf.Approximately(stats2.PermanentMaxHealthMultiplier, 1.35f), "Retry: PermanentMaxHealthMultiplier preserved (1.35)");
            Check(Mathf.Approximately(health2.MaxHealth, 135f), "Retry: MaxHealth is 135");
            Check(Mathf.Approximately(health2.CurrentHealth, 135f), "Retry: CurrentHealth restored to fresh full 135 (Fresh Health Rule)");

            // EndRun (Return to Map): run ended, permanent nodes preserved
            RunProgressionSession.EndRun();
            Check(!RunProgressionSession.HasActiveRun, "EndRun: HasActiveRun is false");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_durability_3"), "EndRun: Permanent node warrior_durability_3 remains purchased");
            Check(PermanentProgression.IsNodePurchased("warrior", "warrior_power_3"), "EndRun: Permanent node warrior_power_3 remains purchased");

            DestroySpawnerAndCharacter(spawner2, spawnerGo2, spawnPt2, character2);

            // Re-spawn after EndRun: permanent progression persists cleanly
            var (spawner3, spawnerGo3, spawnPt3) = CreateTestSpawner(warriorDef);
            var character3 = spawner3.Spawn(warriorDef);
            Own(character3.gameObject);

            var stats3 = character3.GetComponent<PlayerStats>();
            var health3 = character3.GetComponent<PlayerHealth>();
            Check(Mathf.Approximately(stats3.PermanentDamageMultiplier, 1.35f), "Post-EndRun spawn: PermanentDamageMultiplier == 1.35");
            Check(Mathf.Approximately(health3.MaxHealth, 135f), "Post-EndRun spawn: MaxHealth == 135");
            Check(Mathf.Approximately(health3.CurrentHealth, 135f), "Post-EndRun spawn: CurrentHealth == 135");

            DestroySpawnerAndCharacter(spawner3, spawnerGo3, spawnPt3, character3);
        }

        private void VerifyCharacterIsolationAtSpawn()
        {
            var tree = LoadWarriorTree();

            // Archer isolation check
            var archerDef = LoadArcherDefinition();
            Check(archerDef != null, "Character_Archer.asset loaded");
            Check(archerDef.SkillTree == null || archerDef.SkillTree.CharacterId == "archer", "Archer SkillTree is either null or valid archer tree");

            // Archer permanent modifiers must be strictly 1.0 (neutral)
            var archerModsDefault = PermanentProgression.GetPermanentModifiers("archer", null);
            Check(Mathf.Approximately(archerModsDefault.damageMultiplier, 1.0f), "Archer neutral permanent damage multiplier is 1.0");
            Check(Mathf.Approximately(archerModsDefault.maxHealthMultiplier, 1.0f), "Archer neutral permanent max health multiplier is 1.0");
            Check(Mathf.Approximately(archerModsDefault.attackSpeedMultiplier, 1.0f), "Archer neutral permanent attack speed multiplier is 1.0");
            Check(Mathf.Approximately(archerModsDefault.movementSpeedMultiplier, 1.0f), "Archer neutral permanent movement speed multiplier is 1.0");

            // Even if Warrior tree is queried for Archer, Archer has 0 unlocked nodes
            var archerModsFromWarriorTree = PermanentProgression.GetPermanentModifiers("archer", tree);
            Check(Mathf.Approximately(archerModsFromWarriorTree.damageMultiplier, 1.0f), "Archer permanent damage multiplier with warrior tree is 1.0 (isolation)");
            Check(Mathf.Approximately(archerModsFromWarriorTree.maxHealthMultiplier, 1.0f), "Archer permanent max health multiplier with warrior tree is 1.0 (isolation)");

            // Spawn Archer via PlayerSpawner: verify neutral multipliers applied
            var (spawnerA, spawnerGoA, spawnPtA) = CreateTestSpawner(archerDef);
            var characterA = spawnerA.Spawn(archerDef);
            Own(characterA.gameObject);

            Check(characterA != null, "Archer successfully spawned");
            var statsA = characterA.GetComponent<PlayerStats>();
            var healthA = characterA.GetComponent<PlayerHealth>();
            Check(statsA != null, "Spawned Archer has PlayerStats");
            Check(healthA != null, "Spawned Archer has PlayerHealth");
            Check(Mathf.Approximately(statsA.PermanentDamageMultiplier, 1.0f), "Archer spawn: PermanentDamageMultiplier == 1.0");
            Check(Mathf.Approximately(statsA.PermanentAttackSpeedMultiplier, 1.0f), "Archer spawn: PermanentAttackSpeedMultiplier == 1.0");
            Check(Mathf.Approximately(statsA.PermanentMovementSpeedMultiplier, 1.0f), "Archer spawn: PermanentMovementSpeedMultiplier == 1.0");
            Check(Mathf.Approximately(statsA.PermanentMaxHealthMultiplier, 1.0f), "Archer spawn: PermanentMaxHealthMultiplier == 1.0");
            Check(Mathf.Approximately(healthA.MaxHealth, 100f), "Archer spawn: MaxHealth == 100 (unmodified by Warrior upgrades)");
            Check(Mathf.Approximately(healthA.CurrentHealth, 100f), "Archer spawn: CurrentHealth == 100");

            DestroySpawnerAndCharacter(spawnerA, spawnerGoA, spawnPtA, characterA);

            // Gunner isolation check
            var gunnerDef = LoadGunnerDefinition();
            Check(gunnerDef != null, "Character_Gunner.asset loaded");
            Check(gunnerDef.SkillTree == null || gunnerDef.SkillTree.CharacterId == "gunner", "Gunner SkillTree is either null or valid gunner tree");

            var gunnerMods = PermanentProgression.GetPermanentModifiers("gunner", null);
            Check(Mathf.Approximately(gunnerMods.damageMultiplier, 1.0f), "Gunner neutral permanent damage multiplier is 1.0");
            Check(Mathf.Approximately(gunnerMods.maxHealthMultiplier, 1.0f), "Gunner neutral permanent max health multiplier is 1.0");

            var (spawnerG, spawnerGoG, spawnPtG) = CreateTestSpawner(gunnerDef);
            var characterG = spawnerG.Spawn(gunnerDef);
            Own(characterG.gameObject);

            Check(characterG != null, "Gunner successfully spawned");
            var statsG = characterG.GetComponent<PlayerStats>();
            var healthG = characterG.GetComponent<PlayerHealth>();
            Check(statsG != null, "Spawned Gunner has PlayerStats");
            Check(healthG != null, "Spawned Gunner has PlayerHealth");
            Check(Mathf.Approximately(statsG.PermanentDamageMultiplier, 1.0f), "Gunner spawn: PermanentDamageMultiplier == 1.0");
            Check(Mathf.Approximately(statsG.PermanentAttackSpeedMultiplier, 1.0f), "Gunner spawn: PermanentAttackSpeedMultiplier == 1.0");
            Check(Mathf.Approximately(statsG.PermanentMovementSpeedMultiplier, 1.0f), "Gunner spawn: PermanentMovementSpeedMultiplier == 1.0");
            Check(Mathf.Approximately(statsG.PermanentMaxHealthMultiplier, 1.0f), "Gunner spawn: PermanentMaxHealthMultiplier == 1.0");
            Check(Mathf.Approximately(healthG.MaxHealth, 100f), "Gunner spawn: MaxHealth == 100 (unmodified by Warrior upgrades)");
            Check(Mathf.Approximately(healthG.CurrentHealth, 100f), "Gunner spawn: CurrentHealth == 100");

            DestroySpawnerAndCharacter(spawnerG, spawnerGoG, spawnPtG, characterG);
        }

        private void VerifyFailSafeHandling()
        {
            var tree = LoadWarriorTree();

            // Null and empty character ID handling
            var nullMods = PermanentProgression.GetPermanentModifiers(null, tree);
            Check(Mathf.Approximately(nullMods.damageMultiplier, 1.0f), "FailSafe: null characterId returns default modifiers");
            var emptyMods = PermanentProgression.GetPermanentModifiers("", tree);
            Check(Mathf.Approximately(emptyMods.damageMultiplier, 1.0f), "FailSafe: empty characterId returns default modifiers");
            var nullTreeMods = PermanentProgression.GetPermanentModifiers("warrior", null);
            Check(Mathf.Approximately(nullTreeMods.damageMultiplier, 1.0f), "FailSafe: null tree returns default modifiers");

            // Zero and negative multipliers clamped safely
            var testGo = Own(new GameObject("FailSafeTest"));
            var stats = testGo.AddComponent<PlayerStats>();
            var health = testGo.AddComponent<PlayerHealth>();

            stats.SetPermanentMultipliers(-0.5f, 0f, -2f, -1f);
            Check(stats.PermanentDamageMultiplier >= 1.0f, "FailSafe: negative damage multiplier clamped to >= 1.0");
            Check(stats.PermanentAttackSpeedMultiplier >= 1.0f, "FailSafe: zero attack speed multiplier clamped to >= 1.0");
            Check(stats.PermanentMovementSpeedMultiplier >= 1.0f, "FailSafe: negative move speed multiplier clamped to >= 1.0");
            Check(stats.PermanentMaxHealthMultiplier >= 1.0f, "FailSafe: negative max health multiplier clamped to >= 1.0");

            health.ApplyPermanentHealthMultiplier(-0.5f);
            Check(health.MaxHealth >= 100f, "FailSafe: negative health multiplier clamped safely");

            DestroyOwned(testGo);
        }
    }
}
#endif
