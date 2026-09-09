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
    public sealed class Milestone12_4_Verifier : MonoBehaviour
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
                VerifyAllSkillTreeAssetStructuresAndValidation();
                VerifyArcherAndGunnerPurchaseProgressionAndIsolation();
                VerifyCampaignEconomyAndSevenPointCap();
                VerifyArcherSpawnBindingAndRealBowCombat();
                VerifyGunnerSpawnBindingAndRealRifleCombat();
                VerifyCombatUpgradesLayeringOnArcherAndGunner();
                VerifyRunSemanticsCompatibilityOnAllArchetypes();
                VerifyDeterministicCharacterSwitching();
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
            Debug.Log($"[GATE 12.4 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
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
            if (!condition) throw new InvalidOperationException("[GATE 12.4 FAILED] " + description);
            Debug.Log($"[GATE 12.4 CHECK {++checks} PASSED] {description}");
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

        private SkillTreeDefinition LoadTree(string path)
        {
            return AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>(path);
        }

        private CharacterDefinition LoadDefinition(string path)
        {
            return AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path);
        }

        private void VerifyAllSkillTreeAssetStructuresAndValidation()
        {
            // --- WARRIOR TREE INTEGRITY (Unchanged) ---
            var warriorTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Warrior.asset");
            Check(warriorTree != null, "Warrior tree exists");
            Check(warriorTree.CharacterId == "warrior", "Warrior tree characterId is warrior");
            Check(warriorTree.Nodes.Count == 9, "Warrior tree has 9 nodes");
            Check(warriorTree.ValidateTree(out string wErr), $"Warrior tree validates cleanly: {wErr}");
            var warriorDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Warrior.asset");
            Check(warriorDef != null && warriorDef.SkillTree == warriorTree, "Character_Warrior is bound to SkillTree_Warrior");

            // --- ARCHER TREE INTEGRITY ---
            var archerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Archer.asset");
            Check(archerTree != null, "Archer tree exists and loaded");
            Check(archerTree.CharacterId == "archer", "Archer tree characterId is 'archer'");
            Check(archerTree.Nodes.Count == 9, "Archer tree has exactly 9 nodes (3 branches x 3 tiers)");
            Check(archerTree.ValidateTree(out string aErr), $"Archer tree passes ValidateTree cleanly: {aErr}");
            Check(string.IsNullOrEmpty(aErr), "Archer validation error is empty");

            // Branch 1 - Precision (DamageMultiplier: +10%, +10%, +15% = 1.35x)
            Check(archerTree.TryGetNode("archer_precision_1", out var ap1), "archer_precision_1 exists");
            Check(ap1.BranchId == "precision" && ap1.Tier == 1 && ap1.Cost == 1, "archer_precision_1: branch=precision, tier=1, cost=1");
            Check(!ap1.HasPrerequisite && ap1.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(ap1.EffectMagnitude, 0.10f), "archer_precision_1: +10% Damage, no prereq");

            Check(archerTree.TryGetNode("archer_precision_2", out var ap2), "archer_precision_2 exists");
            Check(ap2.BranchId == "precision" && ap2.Tier == 2 && ap2.Cost == 1, "archer_precision_2: branch=precision, tier=2, cost=1");
            Check(ap2.PrerequisiteNodeId == "archer_precision_1" && ap2.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(ap2.EffectMagnitude, 0.10f), "archer_precision_2: +10% Damage, prereq ap1");

            Check(archerTree.TryGetNode("archer_precision_3", out var ap3), "archer_precision_3 exists");
            Check(ap3.BranchId == "precision" && ap3.Tier == 3 && ap3.Cost == 1, "archer_precision_3: branch=precision, tier=3, cost=1");
            Check(ap3.PrerequisiteNodeId == "archer_precision_2" && ap3.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(ap3.EffectMagnitude, 0.15f), "archer_precision_3: +15% Damage, prereq ap2");

            // Branch 2 - Tempo (AttackSpeedMultiplier: +5%, +5%, +10% = 1.20x)
            Check(archerTree.TryGetNode("archer_tempo_1", out var at1), "archer_tempo_1 exists");
            Check(at1.BranchId == "tempo" && at1.Tier == 1 && at1.Cost == 1, "archer_tempo_1: branch=tempo, tier=1, cost=1");
            Check(!at1.HasPrerequisite && at1.EffectType == PermanentEffectType.AttackSpeedMultiplier && Mathf.Approximately(at1.EffectMagnitude, 0.05f), "archer_tempo_1: +5% AtkSpd, no prereq");

            Check(archerTree.TryGetNode("archer_tempo_2", out var at2), "archer_tempo_2 exists");
            Check(at2.BranchId == "tempo" && at2.Tier == 2 && at2.Cost == 1, "archer_tempo_2: branch=tempo, tier=2, cost=1");
            Check(at2.PrerequisiteNodeId == "archer_tempo_1" && at2.EffectType == PermanentEffectType.AttackSpeedMultiplier && Mathf.Approximately(at2.EffectMagnitude, 0.05f), "archer_tempo_2: +5% AtkSpd, prereq at1");

            Check(archerTree.TryGetNode("archer_tempo_3", out var at3), "archer_tempo_3 exists");
            Check(at3.BranchId == "tempo" && at3.Tier == 3 && at3.Cost == 1, "archer_tempo_3: branch=tempo, tier=3, cost=1");
            Check(at3.PrerequisiteNodeId == "archer_tempo_2" && at3.EffectType == PermanentEffectType.AttackSpeedMultiplier && Mathf.Approximately(at3.EffectMagnitude, 0.10f), "archer_tempo_3: +10% AtkSpd, prereq at2");

            // Branch 3 - Mobility / Survival (MovementSpeedMultiplier: +5%, MaxHealthMultiplier: +10%, MovementSpeedMultiplier: +10%)
            Check(archerTree.TryGetNode("archer_survival_1", out var as1), "archer_survival_1 exists");
            Check(as1.BranchId == "survival" && as1.Tier == 1 && as1.Cost == 1, "archer_survival_1: branch=survival, tier=1, cost=1");
            Check(!as1.HasPrerequisite && as1.EffectType == PermanentEffectType.MovementSpeedMultiplier && Mathf.Approximately(as1.EffectMagnitude, 0.05f), "archer_survival_1: +5% MoveSpd, no prereq");

            Check(archerTree.TryGetNode("archer_survival_2", out var as2), "archer_survival_2 exists");
            Check(as2.BranchId == "survival" && as2.Tier == 2 && as2.Cost == 1, "archer_survival_2: branch=survival, tier=2, cost=1");
            Check(as2.PrerequisiteNodeId == "archer_survival_1" && as2.EffectType == PermanentEffectType.MaxHealthMultiplier && Mathf.Approximately(as2.EffectMagnitude, 0.10f), "archer_survival_2: +10% Max HP, prereq as1");

            Check(archerTree.TryGetNode("archer_survival_3", out var as3), "archer_survival_3 exists");
            Check(as3.BranchId == "survival" && as3.Tier == 3 && as3.Cost == 1, "archer_survival_3: branch=survival, tier=3, cost=1");
            Check(as3.PrerequisiteNodeId == "archer_survival_2" && as3.EffectType == PermanentEffectType.MovementSpeedMultiplier && Mathf.Approximately(as3.EffectMagnitude, 0.10f), "archer_survival_3: +10% MoveSpd, prereq as2");

            // Character_Archer binding
            var archerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Archer.asset");
            Check(archerDef != null, "Character_Archer.asset loaded");
            Check(archerDef.SkillTree != null, "Character_Archer.SkillTree is assigned");
            Check(archerDef.SkillTree == archerTree, "Character_Archer.SkillTree matches SkillTree_Archer.asset");

            // --- GUNNER TREE INTEGRITY ---
            var gunnerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Gunner.asset");
            Check(gunnerTree != null, "Gunner tree exists and loaded");
            Check(gunnerTree.CharacterId == "gunner", "Gunner tree characterId is 'gunner'");
            Check(gunnerTree.Nodes.Count == 9, "Gunner tree has exactly 9 nodes (3 branches x 3 tiers)");
            Check(gunnerTree.ValidateTree(out string gErr), $"Gunner tree passes ValidateTree cleanly: {gErr}");
            Check(string.IsNullOrEmpty(gErr), "Gunner validation error is empty");

            // Branch 1 - Firepower (DamageMultiplier: +10%, +10%, +15% = 1.35x)
            Check(gunnerTree.TryGetNode("gunner_firepower_1", out var gf1), "gunner_firepower_1 exists");
            Check(gf1.BranchId == "firepower" && gf1.Tier == 1 && gf1.Cost == 1, "gunner_firepower_1: branch=firepower, tier=1, cost=1");
            Check(!gf1.HasPrerequisite && gf1.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(gf1.EffectMagnitude, 0.10f), "gunner_firepower_1: +10% Damage, no prereq");

            Check(gunnerTree.TryGetNode("gunner_firepower_2", out var gf2), "gunner_firepower_2 exists");
            Check(gf2.BranchId == "firepower" && gf2.Tier == 2 && gf2.Cost == 1, "gunner_firepower_2: branch=firepower, tier=2, cost=1");
            Check(gf2.PrerequisiteNodeId == "gunner_firepower_1" && gf2.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(gf2.EffectMagnitude, 0.10f), "gunner_firepower_2: +10% Damage, prereq gf1");

            Check(gunnerTree.TryGetNode("gunner_firepower_3", out var gf3), "gunner_firepower_3 exists");
            Check(gf3.BranchId == "firepower" && gf3.Tier == 3 && gf3.Cost == 1, "gunner_firepower_3: branch=firepower, tier=3, cost=1");
            Check(gf3.PrerequisiteNodeId == "gunner_firepower_2" && gf3.EffectType == PermanentEffectType.DamageMultiplier && Mathf.Approximately(gf3.EffectMagnitude, 0.15f), "gunner_firepower_3: +15% Damage, prereq gf2");

            // Branch 2 - Cadence (AttackSpeedMultiplier: +5%, +5%, +10% = 1.20x)
            Check(gunnerTree.TryGetNode("gunner_cadence_1", out var gc1), "gunner_cadence_1 exists");
            Check(gc1.BranchId == "cadence" && gc1.Tier == 1 && gc1.Cost == 1, "gunner_cadence_1: branch=cadence, tier=1, cost=1");
            Check(!gc1.HasPrerequisite && gc1.EffectType == PermanentEffectType.AttackSpeedMultiplier && Mathf.Approximately(gc1.EffectMagnitude, 0.05f), "gunner_cadence_1: +5% AtkSpd, no prereq");

            Check(gunnerTree.TryGetNode("gunner_cadence_2", out var gc2), "gunner_cadence_2 exists");
            Check(gc2.BranchId == "cadence" && gc2.Tier == 2 && gc2.Cost == 1, "gunner_cadence_2: branch=cadence, tier=2, cost=1");
            Check(gc2.PrerequisiteNodeId == "gunner_cadence_1" && gc2.EffectType == PermanentEffectType.AttackSpeedMultiplier && Mathf.Approximately(gc2.EffectMagnitude, 0.05f), "gunner_cadence_2: +5% AtkSpd, prereq gc1");

            Check(gunnerTree.TryGetNode("gunner_cadence_3", out var gc3), "gunner_cadence_3 exists");
            Check(gc3.BranchId == "cadence" && gc3.Tier == 3 && gc3.Cost == 1, "gunner_cadence_3: branch=cadence, tier=3, cost=1");
            Check(gc3.PrerequisiteNodeId == "gunner_cadence_2" && gc3.EffectType == PermanentEffectType.AttackSpeedMultiplier && Mathf.Approximately(gc3.EffectMagnitude, 0.10f), "gunner_cadence_3: +10% AtkSpd, prereq gc2");

            // Branch 3 - Handling (MovementSpeedMultiplier: +5%, MaxHealthMultiplier: +10%, MovementSpeedMultiplier: +5%)
            Check(gunnerTree.TryGetNode("gunner_handling_1", out var gh1), "gunner_handling_1 exists");
            Check(gh1.BranchId == "handling" && gh1.Tier == 1 && gh1.Cost == 1, "gunner_handling_1: branch=handling, tier=1, cost=1");
            Check(!gh1.HasPrerequisite && gh1.EffectType == PermanentEffectType.MovementSpeedMultiplier && Mathf.Approximately(gh1.EffectMagnitude, 0.05f), "gunner_handling_1: +5% MoveSpd, no prereq");

            Check(gunnerTree.TryGetNode("gunner_handling_2", out var gh2), "gunner_handling_2 exists");
            Check(gh2.BranchId == "handling" && gh2.Tier == 2 && gh2.Cost == 1, "gunner_handling_2: branch=handling, tier=2, cost=1");
            Check(gh2.PrerequisiteNodeId == "gunner_handling_1" && gh2.EffectType == PermanentEffectType.MaxHealthMultiplier && Mathf.Approximately(gh2.EffectMagnitude, 0.10f), "gunner_handling_2: +10% Max HP, prereq gh1");

            Check(gunnerTree.TryGetNode("gunner_handling_3", out var gh3), "gunner_handling_3 exists");
            Check(gh3.BranchId == "handling" && gh3.Tier == 3 && gh3.Cost == 1, "gunner_handling_3: branch=handling, tier=3, cost=1");
            Check(gh3.PrerequisiteNodeId == "gunner_handling_2" && gh3.EffectType == PermanentEffectType.MovementSpeedMultiplier && Mathf.Approximately(gh3.EffectMagnitude, 0.05f), "gunner_handling_3: +5% MoveSpd, prereq gh2");

            // Character_Gunner binding
            var gunnerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Gunner.asset");
            Check(gunnerDef != null, "Character_Gunner.asset loaded");
            Check(gunnerDef.SkillTree != null, "Character_Gunner.SkillTree is assigned");
            Check(gunnerDef.SkillTree == gunnerTree, "Character_Gunner.SkillTree matches SkillTree_Gunner.asset");
        }

        private void VerifyArcherAndGunnerPurchaseProgressionAndIsolation()
        {
            PermanentProgression.ResetAllProgression();
            var warriorTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Warrior.asset");
            var archerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Archer.asset");
            var gunnerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Gunner.asset");

            archerTree.TryGetNode("archer_precision_1", out var ap1);
            archerTree.TryGetNode("archer_precision_2", out var ap2);
            gunnerTree.TryGetNode("gunner_firepower_1", out var gf1);
            gunnerTree.TryGetNode("gunner_firepower_2", out var gf2);
            warriorTree.TryGetNode("warrior_durability_1", out var wd1);

            // Cross-character purchase rejection
            PermanentProgression.AddSkillPoints("archer", 3);
            PermanentProgression.AddSkillPoints("gunner", 3);
            PermanentProgression.AddSkillPoints("warrior", 3);

            Check(!PermanentProgression.CanPurchaseNode("archer", wd1, warriorTree), "Archer cannot purchase Warrior node");
            Check(!PermanentProgression.TryPurchaseNode("archer", wd1, warriorTree), "TryPurchaseNode fails for Archer on Warrior node");

            Check(!PermanentProgression.CanPurchaseNode("gunner", wd1, warriorTree), "Gunner cannot purchase Warrior node");
            Check(!PermanentProgression.TryPurchaseNode("gunner", wd1, warriorTree), "TryPurchaseNode fails for Gunner on Warrior node");

            Check(!PermanentProgression.CanPurchaseNode("warrior", ap1, archerTree), "Warrior cannot purchase Archer node");
            Check(!PermanentProgression.TryPurchaseNode("warrior", ap1, archerTree), "TryPurchaseNode fails for Warrior on Archer node");

            Check(!PermanentProgression.CanPurchaseNode("warrior", gf1, gunnerTree), "Warrior cannot purchase Gunner node");
            Check(!PermanentProgression.TryPurchaseNode("warrior", gf1, gunnerTree), "TryPurchaseNode fails for Warrior on Gunner node");

            Check(!PermanentProgression.CanPurchaseNode("archer", gf1, gunnerTree), "Archer cannot purchase Gunner node");
            Check(!PermanentProgression.TryPurchaseNode("archer", gf1, gunnerTree), "TryPurchaseNode fails for Archer on Gunner node");

            Check(!PermanentProgression.CanPurchaseNode("gunner", ap1, archerTree), "Gunner cannot purchase Archer node");
            Check(!PermanentProgression.TryPurchaseNode("gunner", ap1, archerTree), "TryPurchaseNode fails for Gunner on Archer node");

            // Prerequisite gating
            Check(!PermanentProgression.CanPurchaseNode("archer", ap2, archerTree), "Archer cannot purchase Tier 2 before Tier 1");
            Check(!PermanentProgression.CanPurchaseNode("gunner", gf2, gunnerTree), "Gunner cannot purchase Tier 2 before Tier 1");

            // Valid purchases
            Check(PermanentProgression.TryPurchaseNode("archer", ap1, archerTree), "Archer purchases archer_precision_1");
            Check(PermanentProgression.GetAvailablePoints("archer") == 2, "Archer points: 2");
            Check(PermanentProgression.GetAvailablePoints("gunner") == 3, "Gunner points unchanged (3)");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 3, "Warrior points unchanged (3)");

            Check(PermanentProgression.TryPurchaseNode("gunner", gf1, gunnerTree), "Gunner purchases gunner_firepower_1");
            Check(PermanentProgression.GetAvailablePoints("gunner") == 2, "Gunner points: 2");
            Check(PermanentProgression.GetAvailablePoints("archer") == 2, "Archer points unchanged (2)");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 3, "Warrior points unchanged (3)");

            // Duplicate purchase rejection
            Check(!PermanentProgression.CanPurchaseNode("archer", ap1, archerTree), "Archer cannot repurchase archer_precision_1");
            Check(!PermanentProgression.TryPurchaseNode("archer", ap1, archerTree), "Duplicate TryPurchaseNode returns false for Archer");

            Check(!PermanentProgression.CanPurchaseNode("gunner", gf1, gunnerTree), "Gunner cannot repurchase gunner_firepower_1");
            Check(!PermanentProgression.TryPurchaseNode("gunner", gf1, gunnerTree), "Duplicate TryPurchaseNode returns false for Gunner");

            // Reload persistence verification
            var isLoadedField = typeof(PermanentProgression).GetField("isLoaded", BindingFlags.NonPublic | BindingFlags.Static);
            isLoadedField.SetValue(null, false);
            var cacheField = typeof(PermanentProgression).GetField("characterCache", BindingFlags.NonPublic | BindingFlags.Static);
            var dict = cacheField.GetValue(null) as System.Collections.IDictionary;
            dict?.Clear();

            Check(PermanentProgression.IsNodePurchased("archer", "archer_precision_1"), "Archer purchase persisted across reload");
            Check(PermanentProgression.IsNodePurchased("gunner", "gunner_firepower_1"), "Gunner purchase persisted across reload");
            Check(!PermanentProgression.IsNodePurchased("warrior", "archer_precision_1"), "Warrior does not own Archer node");
            Check(!PermanentProgression.IsNodePurchased("warrior", "gunner_firepower_1"), "Warrior does not own Gunner node");
        }

        private void VerifyCampaignEconomyAndSevenPointCap()
        {
            PermanentProgression.ResetAllProgression();
            var archerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Archer.asset");
            var gunnerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Gunner.asset");

            // Award campaign first-clears to Archer (D1: 2, D2: 2, D3: 3 = 7 points total)
            Check(PermanentProgression.AwardDungeonClearReward("archer", "dungeon_1", PermanentProgression.GetDungeonFirstClearPoints("dungeon_1")), "Archer clears D1 (+2 pts)");
            Check(PermanentProgression.AwardDungeonClearReward("archer", "dungeon_2", PermanentProgression.GetDungeonFirstClearPoints("dungeon_2")), "Archer clears D2 (+2 pts)");
            Check(PermanentProgression.AwardDungeonClearReward("archer", "dungeon_3", PermanentProgression.GetDungeonFirstClearPoints("dungeon_3")), "Archer clears D3 (+3 pts)");
            Check(PermanentProgression.GetAvailablePoints("archer") == 7, "Archer has exactly 7 points from campaign clears");

            // Replay protection: 0 extra points
            Check(!PermanentProgression.AwardDungeonClearReward("archer", "dungeon_1", 2), "Replaying D1 awards 0 points for Archer");
            Check(!PermanentProgression.AwardDungeonClearReward("archer", "dungeon_2", 2), "Replaying D2 awards 0 points for Archer");
            Check(!PermanentProgression.AwardDungeonClearReward("archer", "dungeon_3", 3), "Replaying D3 awards 0 points for Archer");
            Check(PermanentProgression.GetAvailablePoints("archer") == 7, "Archer points remain 7");

            // Gunner and Warrior still have 0 points (isolation)
            Check(PermanentProgression.GetAvailablePoints("gunner") == 0, "Gunner has 0 points (unaffected by Archer clears)");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Warrior has 0 points (unaffected by Archer clears)");

            // Gunner clears campaign
            Check(PermanentProgression.AwardDungeonClearReward("gunner", "dungeon_1", 2), "Gunner clears D1 (+2 pts)");
            Check(PermanentProgression.AwardDungeonClearReward("gunner", "dungeon_2", 2), "Gunner clears D2 (+2 pts)");
            Check(PermanentProgression.AwardDungeonClearReward("gunner", "dungeon_3", 3), "Gunner clears D3 (+3 pts)");
            Check(PermanentProgression.GetAvailablePoints("gunner") == 7, "Gunner has exactly 7 points from campaign clears");

            // Archer purchases 7 nodes:
            // Precision full branch (3 nodes) + Tempo full branch (3 nodes) + Survival Tier 1 (1 node) = 7 nodes total
            string[] archer7 = {
                "archer_precision_1", "archer_precision_2", "archer_precision_3",
                "archer_tempo_1", "archer_tempo_2", "archer_tempo_3",
                "archer_survival_1"
            };
            foreach (var nId in archer7)
            {
                archerTree.TryGetNode(nId, out var node);
                Check(PermanentProgression.TryPurchaseNode("archer", node, archerTree), $"Archer purchased {nId}");
            }
            Check(PermanentProgression.GetAvailablePoints("archer") == 0, "Archer has 0 points left after 7 purchases");
            archerTree.TryGetNode("archer_survival_2", out var lockedAs2);
            Check(!PermanentProgression.CanPurchaseNode("archer", lockedAs2, archerTree), "Archer cannot purchase 8th node (capped by 7 campaign points)");

            // Gunner purchases 7 nodes:
            // Firepower full branch (3 nodes) + Cadence full branch (3 nodes) + Handling Tier 1 (1 node) = 7 nodes total
            string[] gunner7 = {
                "gunner_firepower_1", "gunner_firepower_2", "gunner_firepower_3",
                "gunner_cadence_1", "gunner_cadence_2", "gunner_cadence_3",
                "gunner_handling_1"
            };
            foreach (var nId in gunner7)
            {
                gunnerTree.TryGetNode(nId, out var node);
                Check(PermanentProgression.TryPurchaseNode("gunner", node, gunnerTree), $"Gunner purchased {nId}");
            }
            Check(PermanentProgression.GetAvailablePoints("gunner") == 0, "Gunner has 0 points left after 7 purchases");
            gunnerTree.TryGetNode("gunner_handling_2", out var lockedGh2);
            Check(!PermanentProgression.CanPurchaseNode("gunner", lockedGh2, gunnerTree), "Gunner cannot purchase 8th node (capped by 7 campaign points)");
        }

        private void VerifyArcherSpawnBindingAndRealBowCombat()
        {
            // Fully unlock all 9 Archer nodes for complete tree verification
            var archerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Archer.asset");
            PermanentProgression.ResetCharacterProgression("archer");
            PermanentProgression.AddSkillPoints("archer", 9);
            foreach (var node in archerTree.Nodes)
            {
                PermanentProgression.TryPurchaseNode("archer", node, archerTree);
            }

            var archerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Archer.asset");
            var (spawner, spawnerGo, spawnPt) = CreateTestSpawner(archerDef);
            var character = spawner.Spawn(archerDef);
            Own(character.gameObject);

            Check(character != null, "Upgraded Archer successfully spawned");

            // Verify PlayerStats permanent multipliers
            var stats = character.GetComponent<PlayerStats>();
            Check(stats != null, "Archer has PlayerStats");
            Check(Mathf.Approximately(stats.PermanentDamageMultiplier, 1.35f), "Archer: PermanentDamageMultiplier == 1.35 (+35%)");
            Check(Mathf.Approximately(stats.PermanentAttackSpeedMultiplier, 1.20f), "Archer: PermanentAttackSpeedMultiplier == 1.20 (+20%)");
            Check(Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.15f), "Archer: PermanentMovementSpeedMultiplier == 1.15 (+15%)");
            Check(Mathf.Approximately(stats.PermanentMaxHealthMultiplier, 1.10f), "Archer: PermanentMaxHealthMultiplier == 1.10 (+10%)");

            // Verify PlayerHealth scaling & fresh health rule (base HP 100 * 1.10 = 110)
            var health = character.GetComponent<PlayerHealth>();
            Check(health != null, "Archer has PlayerHealth");
            Check(Mathf.Approximately(health.BaseMaxHealth, 100f), "Archer: BaseMaxHealth == 100");
            Check(Mathf.Approximately(health.MaxHealth, 110f), "Archer: MaxHealth scaled to 110 (100 * 1.10)");
            Check(Mathf.Approximately(health.CurrentHealth, 110f), "Archer: CurrentHealth initialized to fresh full 110 (Fresh Health Rule)");
            Check(Mathf.Approximately(health.HealthNormalized, 1.0f), "Archer: HealthNormalized == 1.0");

            // Verify BowWeapon combat stats (base damage 20 * 1.35 = 27.0, base cd 0.6 / 1.20 = 0.50s)
            var bow = character.GetComponent<BowWeapon>();
            Check(bow != null, "Archer has BowWeapon");
            Check(Mathf.Approximately(bow.BaseDamage, 20f), "Archer: Bow BaseDamage == 20");
            Check(Mathf.Approximately(bow.EffectiveDamage, 27.0f), "Archer: Bow EffectiveDamage == 27.0 (20 * 1.35)");
            Check(Mathf.Approximately(bow.AttackCooldown, 0.6f), "Archer: Bow Base AttackCooldown == 0.6");
            Check(Mathf.Approximately(bow.EffectiveAttackCooldown, 0.50f), "Archer: Bow EffectiveAttackCooldown == 0.50s (0.6 / 1.20)");

            // Verify PlayerMovement speed (base speed 6.5 * 1.15 = 7.475 m/s)
            var movement = character.GetComponent<PlayerMovement>();
            Check(movement != null, "Archer has PlayerMovement");
            Check(Mathf.Approximately(movement.MoveSpeed, 6.5f), "Archer: Base MoveSpeed == 6.5");
            Check(Mathf.Approximately(movement.EffectiveMoveSpeed, 7.475f), "Archer: EffectiveMoveSpeed == 7.475 m/s (6.5 * 1.15)");

            DestroySpawnerAndCharacter(spawner, spawnerGo, spawnPt, character);
        }

        private void VerifyGunnerSpawnBindingAndRealRifleCombat()
        {
            // Fully unlock all 9 Gunner nodes for complete tree verification
            var gunnerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Gunner.asset");
            PermanentProgression.ResetCharacterProgression("gunner");
            PermanentProgression.AddSkillPoints("gunner", 9);
            foreach (var node in gunnerTree.Nodes)
            {
                PermanentProgression.TryPurchaseNode("gunner", node, gunnerTree);
            }

            var gunnerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Gunner.asset");
            var (spawner, spawnerGo, spawnPt) = CreateTestSpawner(gunnerDef);
            var character = spawner.Spawn(gunnerDef);
            Own(character.gameObject);

            Check(character != null, "Upgraded Gunner successfully spawned");

            // Verify PlayerStats permanent multipliers
            var stats = character.GetComponent<PlayerStats>();
            Check(stats != null, "Gunner has PlayerStats");
            Check(Mathf.Approximately(stats.PermanentDamageMultiplier, 1.35f), "Gunner: PermanentDamageMultiplier == 1.35 (+35%)");
            Check(Mathf.Approximately(stats.PermanentAttackSpeedMultiplier, 1.20f), "Gunner: PermanentAttackSpeedMultiplier == 1.20 (+20%)");
            Check(Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.10f), "Gunner: PermanentMovementSpeedMultiplier == 1.10 (+10%)");
            Check(Mathf.Approximately(stats.PermanentMaxHealthMultiplier, 1.10f), "Gunner: PermanentMaxHealthMultiplier == 1.10 (+10%)");

            // Verify PlayerHealth scaling & fresh health rule (base HP 100 * 1.10 = 110)
            var health = character.GetComponent<PlayerHealth>();
            Check(health != null, "Gunner has PlayerHealth");
            Check(Mathf.Approximately(health.BaseMaxHealth, 100f), "Gunner: BaseMaxHealth == 100");
            Check(Mathf.Approximately(health.MaxHealth, 110f), "Gunner: MaxHealth scaled to 110 (100 * 1.10)");
            Check(Mathf.Approximately(health.CurrentHealth, 110f), "Gunner: CurrentHealth initialized to fresh full 110 (Fresh Health Rule)");
            Check(Mathf.Approximately(health.HealthNormalized, 1.0f), "Gunner: HealthNormalized == 1.0");

            // Verify RifleWeapon combat stats (base damage 10 * 1.35 = 13.5, base cd 0.18 / 1.20 = 0.15s)
            var rifle = character.GetComponent<RifleWeapon>();
            Check(rifle != null, "Gunner has RifleWeapon");
            Check(Mathf.Approximately(rifle.BaseDamage, 10f), "Gunner: Rifle BaseDamage == 10");
            Check(Mathf.Approximately(rifle.EffectiveDamage, 13.5f), "Gunner: Rifle EffectiveDamage == 13.5 (10 * 1.35)");
            Check(Mathf.Approximately(rifle.AttackCooldown, 0.18f), "Gunner: Rifle Base AttackCooldown == 0.18");
            Check(Mathf.Approximately(rifle.EffectiveAttackCooldown, 0.15f), "Gunner: Rifle EffectiveAttackCooldown == 0.15s (0.18 / 1.20)");

            // Verify PlayerMovement speed (base speed 6.0 * 1.10 = 6.60 m/s)
            var movement = character.GetComponent<PlayerMovement>();
            Check(movement != null, "Gunner has PlayerMovement");
            Check(Mathf.Approximately(movement.MoveSpeed, 6.0f), "Gunner: Base MoveSpeed == 6.0");
            Check(Mathf.Approximately(movement.EffectiveMoveSpeed, 6.60f), "Gunner: EffectiveMoveSpeed == 6.60 m/s (6.0 * 1.10)");

            DestroySpawnerAndCharacter(spawner, spawnerGo, spawnPt, character);
        }

        private void VerifyCombatUpgradesLayeringOnArcherAndGunner()
        {
            // --- ARCHER LAYERING ---
            var archerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Archer.asset");
            var (spawnerA, spawnerGoA, spawnPtA) = CreateTestSpawner(archerDef);
            var characterA = spawnerA.Spawn(archerDef);
            Own(characterA.gameObject);

            var statsA = characterA.GetComponent<PlayerStats>();
            var healthA = characterA.GetComponent<PlayerHealth>();
            var bow = characterA.GetComponent<BowWeapon>();
            var moveA = characterA.GetComponent<PlayerMovement>();

            // Layer +20% damage upgrade
            statsA.AddDamageBonus(0.20f);
            Check(Mathf.Approximately(statsA.DamageMultiplier, 1.62f), "Archer Layering: DamageMultiplier == 1.62 (1.20 temp * 1.35 perm)");
            Check(Mathf.Approximately(bow.EffectiveDamage, 32.4f), "Archer Layering: Bow EffectiveDamage == 32.4 (20 * 1.62)");

            // Layer +15% attack speed upgrade
            statsA.AddAttackSpeedBonus(0.15f);
            Check(Mathf.Approximately(statsA.AttackSpeedMultiplier, 1.38f), "Archer Layering: AttackSpeedMultiplier == 1.38 (1.15 temp * 1.20 perm)");
            float expectedBowCd = 0.6f / 1.38f;
            Check(Mathf.Abs(bow.EffectiveAttackCooldown - expectedBowCd) < 0.001f, $"Archer Layering: Bow EffectiveAttackCooldown == {bow.EffectiveAttackCooldown:F4} (expected ~{expectedBowCd:F4})");

            // Layer +10% movement speed upgrade
            statsA.AddMovementSpeedBonus(0.10f);
            Check(Mathf.Approximately(statsA.MovementSpeedMultiplier, 1.265f), "Archer Layering: MovementSpeedMultiplier == 1.265 (1.10 temp * 1.15 perm)");
            float expectedBowMoveSpeed = 6.5f * 1.265f;
            Check(Mathf.Abs(moveA.EffectiveMoveSpeed - expectedBowMoveSpeed) < 0.001f, $"Archer Layering: EffectiveMoveSpeed == {moveA.EffectiveMoveSpeed:F4} (expected ~{expectedBowMoveSpeed:F4})");

            // Damage player: does not corrupt multipliers
            healthA.TakeDamage(35f);
            Check(Mathf.Approximately(healthA.CurrentHealth, 75f), "Archer Layering: Health is 75 / 110 after 35 damage");
            Check(Mathf.Approximately(statsA.DamageMultiplier, 1.62f), "Archer Layering: Multiplier unaffected by damage");

            // Reset modifiers
            statsA.ResetModifiers();
            Check(Mathf.Approximately(statsA.DamageMultiplier, 1.35f), "Archer Layering: DamageMultiplier returns to 1.35 perm on reset");
            Check(Mathf.Approximately(bow.EffectiveDamage, 27.0f), "Archer Layering: Bow EffectiveDamage returns to 27.0");

            DestroySpawnerAndCharacter(spawnerA, spawnerGoA, spawnPtA, characterA);

            // --- GUNNER LAYERING ---
            var gunnerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Gunner.asset");
            var (spawnerG, spawnerGoG, spawnPtG) = CreateTestSpawner(gunnerDef);
            var characterG = spawnerG.Spawn(gunnerDef);
            Own(characterG.gameObject);

            var statsG = characterG.GetComponent<PlayerStats>();
            var healthG = characterG.GetComponent<PlayerHealth>();
            var rifle = characterG.GetComponent<RifleWeapon>();
            var moveG = characterG.GetComponent<PlayerMovement>();

            // Layer +20% damage upgrade
            statsG.AddDamageBonus(0.20f);
            Check(Mathf.Approximately(statsG.DamageMultiplier, 1.62f), "Gunner Layering: DamageMultiplier == 1.62 (1.20 temp * 1.35 perm)");
            Check(Mathf.Approximately(rifle.EffectiveDamage, 16.2f), "Gunner Layering: Rifle EffectiveDamage == 16.2 (10 * 1.62)");

            // Layer +15% attack speed upgrade
            statsG.AddAttackSpeedBonus(0.15f);
            Check(Mathf.Approximately(statsG.AttackSpeedMultiplier, 1.38f), "Gunner Layering: AttackSpeedMultiplier == 1.38 (1.15 temp * 1.20 perm)");
            float expectedRifleCd = 0.18f / 1.38f;
            Check(Mathf.Abs(rifle.EffectiveAttackCooldown - expectedRifleCd) < 0.001f, $"Gunner Layering: Rifle EffectiveAttackCooldown == {rifle.EffectiveAttackCooldown:F4} (expected ~{expectedRifleCd:F4})");

            // Layer +10% movement speed upgrade
            statsG.AddMovementSpeedBonus(0.10f);
            Check(Mathf.Approximately(statsG.MovementSpeedMultiplier, 1.21f), "Gunner Layering: MovementSpeedMultiplier == 1.21 (1.10 temp * 1.10 perm)");
            float expectedRifleMoveSpeed = 6.0f * 1.21f;
            Check(Mathf.Abs(moveG.EffectiveMoveSpeed - expectedRifleMoveSpeed) < 0.001f, $"Gunner Layering: EffectiveMoveSpeed == {moveG.EffectiveMoveSpeed:F4} (expected ~{expectedRifleMoveSpeed:F4})");

            // Damage player: does not corrupt multipliers
            healthG.TakeDamage(40f);
            Check(Mathf.Approximately(healthG.CurrentHealth, 70f), "Gunner Layering: Health is 70 / 110 after 40 damage");
            Check(Mathf.Approximately(statsG.DamageMultiplier, 1.62f), "Gunner Layering: Multiplier unaffected by damage");

            // Reset modifiers
            statsG.ResetModifiers();
            Check(Mathf.Approximately(statsG.DamageMultiplier, 1.35f), "Gunner Layering: DamageMultiplier returns to 1.35 perm on reset");
            Check(Mathf.Approximately(rifle.EffectiveDamage, 13.5f), "Gunner Layering: Rifle EffectiveDamage returns to 13.5");

            DestroySpawnerAndCharacter(spawnerG, spawnerGoG, spawnPtG, characterG);
        }

        private void VerifyRunSemanticsCompatibilityOnAllArchetypes()
        {
            var archerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Archer.asset");

            // Start run with Archer
            RunProgressionSession.StartNewRun("archer");
            RunProgressionSession.CreateDungeonCheckpoint();

            var (spawnerA, spawnerGoA, spawnPtA) = CreateTestSpawner(archerDef);
            var characterA = spawnerA.Spawn(archerDef);
            Own(characterA.gameObject);

            var statsA = characterA.GetComponent<PlayerStats>();
            var healthA = characterA.GetComponent<PlayerHealth>();
            statsA.AddDamageBonus(0.30f);
            healthA.TakeDamage(50f);
            Check(Mathf.Approximately(healthA.CurrentHealth, 60f), "Archer Run: Damaged to 60 / 110");
            Check(Mathf.Approximately(statsA.DamageMultiplier, 1.755f), "Archer Run: Layered 1.30 * 1.35 = 1.755");

            // Defeat Retry: restore checkpoint, respawn
            RunProgressionSession.RestoreCheckpointOnRetry();
            DestroySpawnerAndCharacter(spawnerA, spawnerGoA, spawnPtA, characterA);

            var (spawnerA2, spawnerGoA2, spawnPtA2) = CreateTestSpawner(archerDef);
            var characterA2 = spawnerA2.Spawn(archerDef);
            Own(characterA2.gameObject);

            var statsA2 = characterA2.GetComponent<PlayerStats>();
            var healthA2 = characterA2.GetComponent<PlayerHealth>();
            Check(Mathf.Approximately(statsA2.TemporaryDamageMultiplier, 1.0f), "Archer Retry: Temporary bonus reverted");
            Check(Mathf.Approximately(statsA2.PermanentDamageMultiplier, 1.35f), "Archer Retry: Permanent damage preserved (1.35)");
            Check(Mathf.Approximately(healthA2.MaxHealth, 110f), "Archer Retry: MaxHealth == 110");
            Check(Mathf.Approximately(healthA2.CurrentHealth, 110f), "Archer Retry: CurrentHealth restored to fresh full 110");

            // EndRun: permanent progression intact
            RunProgressionSession.EndRun();
            Check(!RunProgressionSession.HasActiveRun, "Archer EndRun: Run ended");
            Check(PermanentProgression.IsNodePurchased("archer", "archer_precision_3"), "Archer EndRun: Permanent node archer_precision_3 remains purchased");

            DestroySpawnerAndCharacter(spawnerA2, spawnerGoA2, spawnPtA2, characterA2);
        }

        private void VerifyDeterministicCharacterSwitching()
        {
            // Fully unlock all 3 character trees
            var warriorTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Warrior.asset");
            var archerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Archer.asset");
            var gunnerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Gunner.asset");

            PermanentProgression.ResetAllProgression();

            PermanentProgression.AddSkillPoints("warrior", 9);
            foreach (var n in warriorTree.Nodes) PermanentProgression.TryPurchaseNode("warrior", n, warriorTree);

            PermanentProgression.AddSkillPoints("archer", 9);
            foreach (var n in archerTree.Nodes) PermanentProgression.TryPurchaseNode("archer", n, archerTree);

            PermanentProgression.AddSkillPoints("gunner", 9);
            foreach (var n in gunnerTree.Nodes) PermanentProgression.TryPurchaseNode("gunner", n, gunnerTree);

            var warriorDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Warrior.asset");
            var archerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Archer.asset");
            var gunnerDef = LoadDefinition("Assets/ScriptableObjects/Characters/Character_Gunner.asset");

            // 1. Spawn WARRIOR
            var (sW1, sGoW1, ptW1) = CreateTestSpawner(warriorDef);
            var cW1 = sW1.Spawn(warriorDef);
            Own(cW1.gameObject);
            var statsW1 = cW1.GetComponent<PlayerStats>();
            var healthW1 = cW1.GetComponent<PlayerHealth>();
            var weaponW1 = cW1.GetComponent<MeleeWeapon>();
            Check(Mathf.Approximately(healthW1.MaxHealth, 135f), "Switch [Warrior 1]: MaxHealth == 135");
            Check(Mathf.Approximately(weaponW1.EffectiveDamage, 33.75f), "Switch [Warrior 1]: EffectiveDamage == 33.75");
            Check(Mathf.Approximately(statsW1.PermanentAttackSpeedMultiplier, 1.15f), "Switch [Warrior 1]: AttackSpeedMult == 1.15");
            Check(Mathf.Approximately(statsW1.PermanentMovementSpeedMultiplier, 1.05f), "Switch [Warrior 1]: MovementSpeedMult == 1.05");
            DestroySpawnerAndCharacter(sW1, sGoW1, ptW1, cW1);

            // 2. Spawn ARCHER
            var (sA, sGoA, ptA) = CreateTestSpawner(archerDef);
            var cA = sA.Spawn(archerDef);
            Own(cA.gameObject);
            var statsA = cA.GetComponent<PlayerStats>();
            var healthA = cA.GetComponent<PlayerHealth>();
            var weaponA = cA.GetComponent<BowWeapon>();
            Check(Mathf.Approximately(healthA.MaxHealth, 110f), "Switch [Archer]: MaxHealth == 110");
            Check(Mathf.Approximately(weaponA.EffectiveDamage, 27.0f), "Switch [Archer]: EffectiveDamage == 27.0");
            Check(Mathf.Approximately(statsA.PermanentAttackSpeedMultiplier, 1.20f), "Switch [Archer]: AttackSpeedMult == 1.20");
            Check(Mathf.Approximately(statsA.PermanentMovementSpeedMultiplier, 1.15f), "Switch [Archer]: MovementSpeedMult == 1.15");
            DestroySpawnerAndCharacter(sA, sGoA, ptA, cA);

            // 3. Spawn GUNNER
            var (sG, sGoG, ptG) = CreateTestSpawner(gunnerDef);
            var cG = sG.Spawn(gunnerDef);
            Own(cG.gameObject);
            var statsG = cG.GetComponent<PlayerStats>();
            var healthG = cG.GetComponent<PlayerHealth>();
            var weaponG = cG.GetComponent<RifleWeapon>();
            Check(Mathf.Approximately(healthG.MaxHealth, 110f), "Switch [Gunner]: MaxHealth == 110");
            Check(Mathf.Approximately(weaponG.EffectiveDamage, 13.5f), "Switch [Gunner]: EffectiveDamage == 13.5");
            Check(Mathf.Approximately(statsG.PermanentAttackSpeedMultiplier, 1.20f), "Switch [Gunner]: AttackSpeedMult == 1.20");
            Check(Mathf.Approximately(statsG.PermanentMovementSpeedMultiplier, 1.10f), "Switch [Gunner]: MovementSpeedMult == 1.10");
            DestroySpawnerAndCharacter(sG, sGoG, ptG, cG);

            // 4. Return to WARRIOR: verify completely intact
            var (sW2, sGoW2, ptW2) = CreateTestSpawner(warriorDef);
            var cW2 = sW2.Spawn(warriorDef);
            Own(cW2.gameObject);
            var statsW2 = cW2.GetComponent<PlayerStats>();
            var healthW2 = cW2.GetComponent<PlayerHealth>();
            var weaponW2 = cW2.GetComponent<MeleeWeapon>();
            Check(Mathf.Approximately(healthW2.MaxHealth, 135f), "Switch [Warrior 2]: MaxHealth == 135 restored deterministically");
            Check(Mathf.Approximately(weaponW2.EffectiveDamage, 33.75f), "Switch [Warrior 2]: EffectiveDamage == 33.75 restored deterministically");
            Check(Mathf.Approximately(statsW2.PermanentAttackSpeedMultiplier, 1.15f), "Switch [Warrior 2]: AttackSpeedMult == 1.15 restored deterministically");
            Check(Mathf.Approximately(statsW2.PermanentMovementSpeedMultiplier, 1.05f), "Switch [Warrior 2]: MovementSpeedMult == 1.05 restored deterministically");
            DestroySpawnerAndCharacter(sW2, sGoW2, ptW2, cW2);
        }

        private void VerifyFailSafeHandling()
        {
            var archerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Archer.asset");
            var gunnerTree = LoadTree("Assets/ScriptableObjects/Progression/SkillTree_Gunner.asset");

            // Unknown character querying Archer/Gunner tree returns Default
            var unknownMods = PermanentProgression.GetPermanentModifiers("paladin", archerTree);
            Check(Mathf.Approximately(unknownMods.damageMultiplier, 1.0f), "FailSafe: unknown character returns 1.0 damage");
            Check(Mathf.Approximately(unknownMods.maxHealthMultiplier, 1.0f), "FailSafe: unknown character returns 1.0 max health");

            // Null tree returns Default
            var nullTreeMods = PermanentProgression.GetPermanentModifiers("archer", null);
            Check(Mathf.Approximately(nullTreeMods.damageMultiplier, 1.0f), "FailSafe: null tree returns 1.0 damage");

            // Null node in CanPurchaseNode returns false
            Check(!PermanentProgression.CanPurchaseNode("archer", null, archerTree), "FailSafe: null node cannot be purchased");
            Check(!PermanentProgression.TryPurchaseNode("archer", null, archerTree), "FailSafe: TryPurchaseNode returns false on null node");

            // Empty character ID in CanPurchaseNode returns false
            archerTree.TryGetNode("archer_precision_1", out var node);
            Check(!PermanentProgression.CanPurchaseNode("", node, archerTree), "FailSafe: empty character cannot purchase node");
            Check(!PermanentProgression.TryPurchaseNode(null, node, archerTree), "FailSafe: null character cannot purchase node");
        }
    }
}
#endif
