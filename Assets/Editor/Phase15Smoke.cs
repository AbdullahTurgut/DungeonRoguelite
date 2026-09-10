using System;
using System.Linq;
using System.Reflection;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Tests;
using UnityEditor;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    public static class Phase15Smoke
    {
        public static void Run()
        {
            Debug.Log("[PHASE 15 SMOKE START]");
            bool passed = false;
            try
            {
                using (var snapshot = new Phase12StateSnapshot())
                {
                    PlayerPrefs.SetString(PermanentProgression.PrefsKey,
                        "{\"version\":1,\"characters\":[{\"characterId\":\"warrior\",\"availableSkillPoints\":7,\"unlockedNodeIds\":[\"warrior_power_1\",\"warrior_power_1\",null],\"rewardedDungeonIds\":[\"dungeon_1\",\"dungeon_1\",\"dungeon_5\"]},{\"characterId\":\"archer\",\"availableSkillPoints\":3,\"unlockedNodeIds\":null,\"rewardedDungeonIds\":null}]}");
                    Reload();
                    Require(PermanentProgression.GetAvailablePoints("warrior") == 7 && PermanentProgression.GetUnlockedNodeIds("warrior").Count == 1 && PermanentProgression.GetAvailablePoints("archer") == 3, "grandfathered state and normalized lists");
                    Require(!PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_1", out _) && PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_5", out int reward) && reward == 2, "historical reward and legacy D5 claim");
                    Reload();
                    Require(!PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_5", out _) && PermanentProgression.GetAvailablePoints("warrior") == 9, "migration and D5 are idempotent across reload");
                    foreach (string hero in new[] { "Warrior", "Archer", "Gunner" })
                    {
                        var tree = AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>($"Assets/ScriptableObjects/Progression/SkillTree_{hero}.asset");
                        Require(tree.ValidateTree(out _) && tree.Nodes.Count == 9 && tree.Nodes.Sum(n => n.Cost) == 15 && tree.Nodes.All(n => n.Cost == (n.Tier == 1 ? 1 : 2)), "tree costs " + hero);
                    }
                    var warrior = AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>(Phase12AssetBuilder.WarriorTreePath);
                    warrior.TryGetNode("warrior_power_2", out var node);
                    Require(PermanentProgression.TryPurchaseNode("warrior", node, warrior) && PermanentProgression.GetAvailablePoints("warrior") == 7 && !PermanentProgression.TryPurchaseNode("archer", node, warrior), "future cost and affinity");
                    PermanentProgression.ResetAllProgression();
                    int[] rewards = {1,1,1,1,2,1,1,2,2,3};
                    for (int i = 0; i < rewards.Length; i++)
                        Require(PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_" + (i + 1), out reward) && reward == rewards[i] && !PermanentProgression.TryAwardDungeonFirstClear("warrior", "dungeon_" + (i + 1), out _), "first-clear economy");
                    Require(PermanentProgression.GetAvailablePoints("warrior") == 15 && PermanentProgression.GetAvailablePoints("archer") == 0, "campaign total and isolation");
                }
                passed = true;
                Debug.Log("[PHASE 15 SMOKE PASSED]");
            }
            catch (Exception ex) { Debug.LogException(ex); }
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void Reload() => typeof(PermanentProgression).GetMethod("ResetStaticState", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    }
}
