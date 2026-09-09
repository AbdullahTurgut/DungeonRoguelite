#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;

namespace DungeonRoguelite.Tests
{
    public sealed class Milestone12_1_Verifier : MonoBehaviour
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
                VerifyFreshProfileAndDefaults();
                VerifySkillPointAwardingAndEvents();
                VerifySkillTreeDefinitionValidation();
                VerifyPurchaseRulesAndPrerequisites();
                VerifyCharacterIsolation();
                VerifySaveLoadPersistence();
                VerifyCorruptJsonRecovery();
                VerifyDungeonRewardClaimingAndIdempotency();
                VerifyPermanentStatModifiersAggregation();
                VerifyPlayerStatsLayering();
                VerifyPlayerHealthMultiplierAndFreshHealth();
                VerifyResetAndCleanup();
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
            Debug.Log($"[GATE 12.1 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
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

        private void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException("[GATE 12.1 FAILED] " + description);
            Debug.Log($"[GATE 12.1 CHECK {++checks} PASSED] {description}");
        }

        private void Cleanup()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null) DestroyImmediate(owned[i]);
            }
            owned.Clear();
            PermanentProgression.ResetAllProgression();
        }

        private void VerifyFreshProfileAndDefaults()
        {
            PermanentProgression.ResetAllProgression();

            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "Fresh profile: Warrior available points == 0");
            Check(PermanentProgression.GetAvailablePoints("archer") == 0, "Fresh profile: Archer available points == 0");
            Check(PermanentProgression.GetAvailablePoints("gunner") == 0, "Fresh profile: Gunner available points == 0");

            Check(PermanentProgression.GetUnlockedNodeIds("warrior").Count == 0, "Fresh profile: Warrior unlocked nodes count == 0");
            Check(PermanentProgression.GetUnlockedNodeIds("archer").Count == 0, "Fresh profile: Archer unlocked nodes count == 0");

            Check(!PermanentProgression.IsNodePurchased("warrior", "any_node"), "Fresh profile: unpurchased node returns false");
            Check(!PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_1"), "Fresh profile: dungeon reward not claimed");
        }

        private void VerifySkillPointAwardingAndEvents()
        {
            string changedChar = null;
            int changedPoints = -1;
            Action<string, int> handler = (cId, pts) => { changedChar = cId; changedPoints = pts; };

            PermanentProgression.OnSkillPointsChanged += handler;
            try
            {
                PermanentProgression.AddSkillPoints("warrior", 5);
                Check(PermanentProgression.GetAvailablePoints("warrior") == 5, "AddSkillPoints: Warrior points increased to 5");
                Check(changedChar == "warrior" && changedPoints == 5, "AddSkillPoints: OnSkillPointsChanged event fired correctly");

                // Negative or zero points should be rejected
                PermanentProgression.AddSkillPoints("warrior", 0);
                PermanentProgression.AddSkillPoints("warrior", -2);
                Check(PermanentProgression.GetAvailablePoints("warrior") == 5, "AddSkillPoints: non-positive points ignored");
            }
            finally
            {
                PermanentProgression.OnSkillPointsChanged -= handler;
            }
        }

        private void VerifySkillTreeDefinitionValidation()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Own(tree);

            var nodes = new List<SkillNodeDefinition>
            {
                new SkillNodeDefinition("w_dmg_1", "warrior", "dmg", 1, "Dmg I", "+10% Dmg", 1, null, PermanentEffectType.DamageMultiplier, 0.10f),
                new SkillNodeDefinition("w_dmg_2", "warrior", "dmg", 2, "Dmg II", "+10% Dmg", 1, "w_dmg_1", PermanentEffectType.DamageMultiplier, 0.10f),
                new SkillNodeDefinition("w_hp_1", "warrior", "hp", 1, "HP I", "+15% HP", 1, null, PermanentEffectType.MaxHealthMultiplier, 0.15f)
            };
            tree.Initialize("warrior", nodes);

            Check(tree.ValidateTree(out string err1), "Valid skill tree passes validation");
            Check(tree.TryGetNode("w_dmg_1", out var foundNode) && foundNode.Id == "w_dmg_1", "TryGetNode finds valid node");
            Check(!tree.TryGetNode("non_existent", out _), "TryGetNode returns false for missing node");

            // Duplicate ID test
            var dupTree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Own(dupTree);
            dupTree.Initialize("warrior", new[] {
                new SkillNodeDefinition("w_dmg_1", "warrior", "dmg", 1, "A", "", 1, null, PermanentEffectType.DamageMultiplier, 0.10f),
                new SkillNodeDefinition("w_dmg_1", "warrior", "dmg", 2, "B", "", 1, null, PermanentEffectType.DamageMultiplier, 0.10f)
            });
            Check(!dupTree.ValidateTree(out _), "Duplicate node ID fails tree validation");

            // Missing prerequisite test
            var missingPrereqTree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Own(missingPrereqTree);
            missingPrereqTree.Initialize("warrior", new[] {
                new SkillNodeDefinition("w_dmg_2", "warrior", "dmg", 2, "B", "", 1, "missing_node", PermanentEffectType.DamageMultiplier, 0.10f)
            });
            Check(!missingPrereqTree.ValidateTree(out _), "Missing prerequisite fails tree validation");

            // Character mismatch test
            var mismatchTree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Own(mismatchTree);
            mismatchTree.Initialize("warrior", new[] {
                new SkillNodeDefinition("a_dmg_1", "archer", "dmg", 1, "A", "", 1, null, PermanentEffectType.DamageMultiplier, 0.10f)
            });
            Check(!mismatchTree.ValidateTree(out _), "Character mismatch fails tree validation");
        }

        private void VerifyPurchaseRulesAndPrerequisites()
        {
            PermanentProgression.ResetAllProgression();
            PermanentProgression.AddSkillPoints("warrior", 3);

            var tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Own(tree);
            var node1 = new SkillNodeDefinition("w_dmg_1", "warrior", "dmg", 1, "Dmg I", "", 1, null, PermanentEffectType.DamageMultiplier, 0.10f);
            var node2 = new SkillNodeDefinition("w_dmg_2", "warrior", "dmg", 2, "Dmg II", "", 1, "w_dmg_1", PermanentEffectType.DamageMultiplier, 0.10f);
            var nodeCost2 = new SkillNodeDefinition("w_big_1", "warrior", "special", 1, "Big I", "", 2, null, PermanentEffectType.AttackSpeedMultiplier, 0.15f);
            var archerNode = new SkillNodeDefinition("a_dmg_1", "archer", "dmg", 1, "Archer Dmg", "", 1, null, PermanentEffectType.DamageMultiplier, 0.10f);

            tree.Initialize("warrior", new[] { node1, node2, nodeCost2 });

            // Prerequisite locked
            Check(!PermanentProgression.CanPurchaseNode("warrior", node2, tree), "Tier 2 node cannot be purchased before Tier 1 prerequisite");
            Check(!PermanentProgression.TryPurchaseNode("warrior", node2, tree), "TryPurchaseNode fails when prerequisite missing");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 3, "Points unchanged after failed purchase");

            // Character mismatch
            Check(!PermanentProgression.CanPurchaseNode("warrior", archerNode, tree), "Warrior cannot purchase Archer node");

            // Valid Tier 1 purchase
            string purchasedId = null;
            Action<string, string> purchaseHandler = (cId, nId) => { if (cId == "warrior") purchasedId = nId; };
            PermanentProgression.OnSkillPurchased += purchaseHandler;
            try
            {
                Check(PermanentProgression.CanPurchaseNode("warrior", node1, tree), "Tier 1 node can be purchased");
                Check(PermanentProgression.TryPurchaseNode("warrior", node1, tree), "Tier 1 node purchase succeeds");
                Check(PermanentProgression.IsNodePurchased("warrior", "w_dmg_1"), "Node marked as purchased");
                Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Points deducted from 3 to 2");
                Check(purchasedId == "w_dmg_1", "OnSkillPurchased event fired with correct node ID");

                // Duplicate purchase rejection
                Check(!PermanentProgression.CanPurchaseNode("warrior", node1, tree), "Purchased node cannot be purchased again");
                Check(!PermanentProgression.TryPurchaseNode("warrior", node1, tree), "TryPurchaseNode fails for already purchased node");
                Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Points unchanged after duplicate purchase attempt");

                // Prerequisite now satisfied
                Check(PermanentProgression.CanPurchaseNode("warrior", node2, tree), "Tier 2 node can be purchased after prerequisite met");
                Check(PermanentProgression.TryPurchaseNode("warrior", node2, tree), "Tier 2 node purchase succeeds");
                Check(PermanentProgression.IsNodePurchased("warrior", "w_dmg_2"), "Tier 2 node marked as purchased");
                Check(PermanentProgression.GetAvailablePoints("warrior") == 1, "Points deducted from 2 to 1");

                // Insufficient points
                Check(!PermanentProgression.CanPurchaseNode("warrior", nodeCost2, tree), "Cannot purchase node with cost 2 when only 1 point available");
                Check(!PermanentProgression.TryPurchaseNode("warrior", nodeCost2, tree), "TryPurchaseNode fails due to insufficient points");
                Check(PermanentProgression.GetAvailablePoints("warrior") == 1, "Points remain 1 after insufficient point rejection");
            }
            finally
            {
                PermanentProgression.OnSkillPurchased -= purchaseHandler;
            }
        }

        private void VerifyCharacterIsolation()
        {
            PermanentProgression.ResetAllProgression();
            PermanentProgression.AddSkillPoints("warrior", 2);

            var tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Own(tree);
            var node1 = new SkillNodeDefinition("w_dmg_1", "warrior", "dmg", 1, "Dmg I", "", 1, null, PermanentEffectType.DamageMultiplier, 0.10f);
            tree.Initialize("warrior", new[] { node1 });

            PermanentProgression.TryPurchaseNode("warrior", node1, tree);

            Check(PermanentProgression.GetAvailablePoints("warrior") == 1, "Warrior has 1 point remaining");
            Check(PermanentProgression.IsNodePurchased("warrior", "w_dmg_1"), "Warrior has w_dmg_1 purchased");

            Check(PermanentProgression.GetAvailablePoints("archer") == 0, "Archer has 0 points (isolation)");
            Check(!PermanentProgression.IsNodePurchased("archer", "w_dmg_1"), "Archer does not have Warrior skill (isolation)");

            Check(PermanentProgression.GetAvailablePoints("gunner") == 0, "Gunner has 0 points (isolation)");
            Check(!PermanentProgression.IsNodePurchased("gunner", "w_dmg_1"), "Gunner does not have Warrior skill (isolation)");
        }

        private void VerifySaveLoadPersistence()
        {
            PermanentProgression.ResetAllProgression();
            PermanentProgression.AddSkillPoints("warrior", 4);

            var tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Own(tree);
            var node = new SkillNodeDefinition("w_hp_1", "warrior", "hp", 1, "HP I", "", 1, null, PermanentEffectType.MaxHealthMultiplier, 0.10f);
            tree.Initialize("warrior", new[] { node });

            PermanentProgression.TryPurchaseNode("warrior", node, tree);
            PermanentProgression.AwardDungeonClearReward("warrior", "dungeon_1", 2);

            // Verify raw saved JSON in PlayerPrefs
            string rawJson = PlayerPrefs.GetString(PermanentProgression.PrefsKey, "");
            Check(!string.IsNullOrEmpty(rawJson), "SaveData successfully serialized to PlayerPrefs");

            // Reset in-memory cache to simulate domain reload / application restart
            var isLoadedField = typeof(PermanentProgression).GetField("isLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            isLoadedField.SetValue(null, false);

            // Re-query: triggers EnsureLoaded from PlayerPrefs
            Check(PermanentProgression.GetAvailablePoints("warrior") == 5, "Persisted points loaded correctly (4 - 1 + 2 = 5)");
            Check(PermanentProgression.IsNodePurchased("warrior", "w_hp_1"), "Persisted unlocked node loaded correctly");
            Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_1"), "Persisted rewarded dungeon loaded correctly");
        }

        private void VerifyCorruptJsonRecovery()
        {
            PlayerPrefs.SetString(PermanentProgression.PrefsKey, "{ INVALID_JSON_DATA_CORRUPT: true, ");
            PlayerPrefs.Save();

            var isLoadedField = typeof(PermanentProgression).GetField("isLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            isLoadedField.SetValue(null, false);

            // Ensure corrupt JSON does not throw and recovers safely
            int points = PermanentProgression.GetAvailablePoints("warrior");
            Check(points == 0, "Corrupt JSON safely falls back to default 0 points without crashing");
        }

        private void VerifyDungeonRewardClaimingAndIdempotency()
        {
            PermanentProgression.ResetAllProgression();

            string awardedChar = null;
            string awardedDungeon = null;
            int awardedPoints = -1;
            Action<string, string, int> handler = (cId, dId, pts) => { awardedChar = cId; awardedDungeon = dId; awardedPoints = pts; };

            PermanentProgression.OnDungeonRewardAwarded += handler;
            try
            {
                Check(!PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_1"), "Reward not yet claimed");

                // First clear: awards 2 points
                bool awarded1 = PermanentProgression.AwardDungeonClearReward("warrior", "dungeon_1", 2);
                Check(awarded1, "AwardDungeonClearReward returns true on first clear");
                Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Warrior received 2 points");
                Check(PermanentProgression.IsDungeonRewardClaimed("warrior", "dungeon_1"), "Reward recorded as claimed");
                Check(awardedChar == "warrior" && awardedDungeon == "dungeon_1" && awardedPoints == 2, "OnDungeonRewardAwarded event fired");

                // Duplicate clear: must award 0 extra points
                awardedPoints = -1;
                bool awardedDup = PermanentProgression.AwardDungeonClearReward("warrior", "dungeon_1", 2);
                Check(!awardedDup, "Duplicate AwardDungeonClearReward returns false (anti-farming protection)");
                Check(PermanentProgression.GetAvailablePoints("warrior") == 2, "Warrior points unchanged after duplicate clear");
                Check(awardedPoints == -1, "Event not fired on duplicate reward attempt");

                // Character isolation: Archer has not claimed D1 reward
                Check(!PermanentProgression.IsDungeonRewardClaimed("archer", "dungeon_1"), "Archer D1 reward is unclaimed (isolation)");
                bool awardedArcher = PermanentProgression.AwardDungeonClearReward("archer", "dungeon_1", 2);
                Check(awardedArcher, "Archer can claim their own first-clear reward for D1");
                Check(PermanentProgression.GetAvailablePoints("archer") == 2, "Archer received 2 points");
            }
            finally
            {
                PermanentProgression.OnDungeonRewardAwarded -= handler;
            }
        }

        private void VerifyPermanentStatModifiersAggregation()
        {
            PermanentProgression.ResetAllProgression();
            PermanentProgression.AddSkillPoints("warrior", 10);

            var tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Own(tree);

            var nodes = new List<SkillNodeDefinition>
            {
                new SkillNodeDefinition("w_dmg_1", "warrior", "dmg", 1, "Dmg I", "", 1, null, PermanentEffectType.DamageMultiplier, 0.10f),
                new SkillNodeDefinition("w_dmg_2", "warrior", "dmg", 2, "Dmg II", "", 1, "w_dmg_1", PermanentEffectType.DamageMultiplier, 0.10f),
                new SkillNodeDefinition("w_hp_1", "warrior", "hp", 1, "HP I", "", 1, null, PermanentEffectType.MaxHealthMultiplier, 0.15f),
                new SkillNodeDefinition("w_spd_1", "warrior", "spd", 1, "Spd I", "", 1, null, PermanentEffectType.MovementSpeedMultiplier, 0.06f)
            };
            tree.Initialize("warrior", nodes);

            // Default modifiers (no nodes unlocked)
            var defaultMods = PermanentProgression.GetPermanentModifiers("warrior", tree);
            Check(Mathf.Approximately(defaultMods.damageMultiplier, 1.0f), "Default permanent damage multiplier is 1.0");
            Check(Mathf.Approximately(defaultMods.attackSpeedMultiplier, 1.0f), "Default permanent attack speed multiplier is 1.0");
            Check(Mathf.Approximately(defaultMods.movementSpeedMultiplier, 1.0f), "Default permanent movement speed multiplier is 1.0");
            Check(Mathf.Approximately(defaultMods.maxHealthMultiplier, 1.0f), "Default permanent max health multiplier is 1.0");

            // Purchase nodes: Dmg I (+10%), Dmg II (+10%), HP I (+15%), Spd I (+6%)
            PermanentProgression.TryPurchaseNode("warrior", nodes[0], tree);
            PermanentProgression.TryPurchaseNode("warrior", nodes[1], tree);
            PermanentProgression.TryPurchaseNode("warrior", nodes[2], tree);
            PermanentProgression.TryPurchaseNode("warrior", nodes[3], tree);

            var mods = PermanentProgression.GetPermanentModifiers("warrior", tree);
            Check(Mathf.Approximately(mods.damageMultiplier, 1.20f), "Aggregated permanent damage multiplier is 1.20 (+20%)");
            Check(Mathf.Approximately(mods.maxHealthMultiplier, 1.15f), "Aggregated permanent max health multiplier is 1.15 (+15%)");
            Check(Mathf.Approximately(mods.movementSpeedMultiplier, 1.06f), "Aggregated permanent movement speed multiplier is 1.06 (+6%)");
            Check(Mathf.Approximately(mods.attackSpeedMultiplier, 1.00f), "Unpurchased permanent attack speed multiplier remains 1.00");
        }

        private void VerifyPlayerStatsLayering()
        {
            var go = new GameObject("TestPlayerStats");
            Own(go);
            var stats = go.AddComponent<PlayerStats>();

            // Configure permanent multipliers foundation
            stats.SetPermanentMultipliers(dmg: 1.20f, atkSpd: 1.08f, spd: 1.06f, maxHp: 1.15f);

            Check(Mathf.Approximately(stats.PermanentDamageMultiplier, 1.20f), "PlayerStats: PermanentDamageMultiplier == 1.20");
            Check(Mathf.Approximately(stats.PermanentAttackSpeedMultiplier, 1.08f), "PlayerStats: PermanentAttackSpeedMultiplier == 1.08");
            Check(Mathf.Approximately(stats.PermanentMovementSpeedMultiplier, 1.06f), "PlayerStats: PermanentMovementSpeedMultiplier == 1.06");
            Check(Mathf.Approximately(stats.PermanentMaxHealthMultiplier, 1.15f), "PlayerStats: PermanentMaxHealthMultiplier == 1.15");

            // Effective multipliers with no temporary upgrades
            Check(Mathf.Approximately(stats.DamageMultiplier, 1.20f), "PlayerStats: Effective DamageMultiplier == 1.20 (1.0 * 1.20)");
            Check(Mathf.Approximately(stats.AttackSpeedMultiplier, 1.08f), "PlayerStats: Effective AttackSpeedMultiplier == 1.08");
            Check(Mathf.Approximately(stats.MovementSpeedMultiplier, 1.06f), "PlayerStats: Effective MovementSpeedMultiplier == 1.06");

            // Apply temporary upgrade (+20% damage)
            stats.AddDamageBonus(0.20f);
            Check(Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.20f), "PlayerStats: TemporaryDamageMultiplier == 1.20");
            Check(Mathf.Approximately(stats.DamageMultiplier, 1.44f), "PlayerStats: Effective DamageMultiplier == 1.44 (1.20 temp * 1.20 perm)");

            // Reset temporary modifiers (e.g. EndRun or Retry rollback)
            stats.ResetModifiers();
            Check(Mathf.Approximately(stats.TemporaryDamageMultiplier, 1.00f), "PlayerStats: TemporaryDamageMultiplier reset to 1.00");
            Check(Mathf.Approximately(stats.DamageMultiplier, 1.20f), "PlayerStats: PermanentDamageMultiplier preserved after temporary reset");
        }

        private void VerifyPlayerHealthMultiplierAndFreshHealth()
        {
            var go = new GameObject("TestPlayerHealth");
            Own(go);
            var health = go.AddComponent<PlayerHealth>();

            Check(Mathf.Approximately(health.BaseMaxHealth, 100f), "PlayerHealth: BaseMaxHealth is 100");
            Check(Mathf.Approximately(health.MaxHealth, 100f), "PlayerHealth: initial MaxHealth is 100");
            Check(Mathf.Approximately(health.CurrentHealth, 100f), "PlayerHealth: initial CurrentHealth is 100");

            float changedCur = -1f, changedMax = -1f;
            health.OnHealthChanged += (cur, max) => { changedCur = cur; changedMax = max; };

            // Apply permanent multiplier (+15% max health)
            health.ApplyPermanentHealthMultiplier(1.15f);

            Check(Mathf.Approximately(health.MaxHealth, 115f), "PlayerHealth: MaxHealth scaled to 115f (100 * 1.15)");
            Check(Mathf.Approximately(health.CurrentHealth, 115f), "PlayerHealth: CurrentHealth initialized to fresh full 115f");
            Check(Mathf.Approximately(changedCur, 115f) && Mathf.Approximately(changedMax, 115f), "PlayerHealth: OnHealthChanged fired with (115, 115)");

            // Take damage
            health.TakeDamage(35f);
            Check(Mathf.Approximately(health.CurrentHealth, 80f), "PlayerHealth: CurrentHealth is 80f after taking 35 damage");

            // Fresh spawn simulation: re-applying permanent multiplier restores fresh full health
            health.ApplyPermanentHealthMultiplier(1.15f);
            Check(Mathf.Approximately(health.CurrentHealth, 115f), "PlayerHealth: Fresh spawn re-application restores 115 / 115 full health");
        }

        private void VerifyResetAndCleanup()
        {
            PermanentProgression.AddSkillPoints("warrior", 5);
            PermanentProgression.AddSkillPoints("archer", 5);

            PermanentProgression.ResetCharacterProgression("warrior");
            Check(PermanentProgression.GetAvailablePoints("warrior") == 0, "ResetCharacterProgression clears Warrior");
            Check(PermanentProgression.GetAvailablePoints("archer") == 5, "ResetCharacterProgression preserves Archer");

            PermanentProgression.ResetAllProgression();
            Check(PermanentProgression.GetAvailablePoints("archer") == 0, "ResetAllProgression clears all characters");
        }
    }
}
#endif
