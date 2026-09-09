using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Progression;

namespace DungeonRoguelite.Editor
{
    public static class Phase12AssetBuilder
    {
        public const string ProgressionDir = "Assets/ScriptableObjects/Progression";
        public const string WarriorTreePath = "Assets/ScriptableObjects/Progression/SkillTree_Warrior.asset";
        public const string WarriorDefPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";

        [MenuItem("DungeonRoguelite/Phase 12/Build Warrior Skill Tree Asset")]
        public static SkillTreeDefinition BuildWarriorTree()
        {
            if (!AssetDatabase.IsValidFolder(ProgressionDir))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Progression");
            }

            var tree = AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>(WarriorTreePath);
            if (tree == null)
            {
                tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
                AssetDatabase.CreateAsset(tree, WarriorTreePath);
            }

            var nodes = new List<SkillNodeDefinition>
            {
                // Branch 1 - Durability
                new SkillNodeDefinition(
                    "warrior_durability_1",
                    "warrior",
                    "durability",
                    1,
                    "Demir Bünye I",
                    "+10% Azami Can",
                    1,
                    null,
                    PermanentEffectType.MaxHealthMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "warrior_durability_2",
                    "warrior",
                    "durability",
                    2,
                    "Demir Bünye II",
                    "+10% Azami Can",
                    1,
                    "warrior_durability_1",
                    PermanentEffectType.MaxHealthMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "warrior_durability_3",
                    "warrior",
                    "durability",
                    3,
                    "Demir Bünye III",
                    "+15% Azami Can",
                    1,
                    "warrior_durability_2",
                    PermanentEffectType.MaxHealthMultiplier,
                    0.15f),

                // Branch 2 - Power
                new SkillNodeDefinition(
                    "warrior_power_1",
                    "warrior",
                    "power",
                    1,
                    "Ağır Darbe I",
                    "+10% Hasar",
                    1,
                    null,
                    PermanentEffectType.DamageMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "warrior_power_2",
                    "warrior",
                    "power",
                    2,
                    "Ağır Darbe II",
                    "+10% Hasar",
                    1,
                    "warrior_power_1",
                    PermanentEffectType.DamageMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "warrior_power_3",
                    "warrior",
                    "power",
                    3,
                    "Ağır Darbe III",
                    "+15% Hasar",
                    1,
                    "warrior_power_2",
                    PermanentEffectType.DamageMultiplier,
                    0.15f),

                // Branch 3 - Tempo
                new SkillNodeDefinition(
                    "warrior_tempo_1",
                    "warrior",
                    "tempo",
                    1,
                    "Çevik Hamle I",
                    "+5% Saldırı Hızı",
                    1,
                    null,
                    PermanentEffectType.AttackSpeedMultiplier,
                    0.05f),
                new SkillNodeDefinition(
                    "warrior_tempo_2",
                    "warrior",
                    "tempo",
                    2,
                    "Hafif Adım II",
                    "+5% Hareket Hızı",
                    1,
                    "warrior_tempo_1",
                    PermanentEffectType.MovementSpeedMultiplier,
                    0.05f),
                new SkillNodeDefinition(
                    "warrior_tempo_3",
                    "warrior",
                    "tempo",
                    3,
                    "Kasırga Biçiş III",
                    "+10% Saldırı Hızı",
                    1,
                    "warrior_tempo_2",
                    PermanentEffectType.AttackSpeedMultiplier,
                    0.10f)
            };

            tree.Initialize("warrior", nodes);
            EditorUtility.SetDirty(tree);

            // Connect to Character_Warrior.asset
            var warriorDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorDefPath);
            if (warriorDef != null)
            {
                warriorDef.SetSkillTree(tree);
                EditorUtility.SetDirty(warriorDef);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WARRIOR SKILL TREE ASSET BUILT SUCCESSFULLY]");
            return tree;
        }
    }
}
