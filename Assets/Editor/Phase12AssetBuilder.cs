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

        public const string ArcherTreePath = "Assets/ScriptableObjects/Progression/SkillTree_Archer.asset";
        public const string ArcherDefPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";

        public const string GunnerTreePath = "Assets/ScriptableObjects/Progression/SkillTree_Gunner.asset";
        public const string GunnerDefPath = "Assets/ScriptableObjects/Characters/Character_Gunner.asset";

        [MenuItem("DungeonRoguelite/Phase 12/Build All Skill Tree Assets")]
        public static void BuildAllSkillTrees()
        {
            BuildWarriorTree();
            BuildArcherTree();
            BuildGunnerTree();
            Debug.Log("[ALL SKILL TREES BUILT SUCCESSFULLY]");
        }

        [MenuItem("DungeonRoguelite/Phase 12/Build Warrior Skill Tree Asset")]
        public static SkillTreeDefinition BuildWarriorTree()
        {
            EnsureProgressionDir();

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

        [MenuItem("DungeonRoguelite/Phase 12/Build Archer Skill Tree Asset")]
        public static SkillTreeDefinition BuildArcherTree()
        {
            EnsureProgressionDir();

            var tree = AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>(ArcherTreePath);
            if (tree == null)
            {
                tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
                AssetDatabase.CreateAsset(tree, ArcherTreePath);
            }

            var nodes = new List<SkillNodeDefinition>
            {
                // Branch 1 - Precision / Power (DamageMultiplier: 1.35x)
                new SkillNodeDefinition(
                    "archer_precision_1",
                    "archer",
                    "precision",
                    1,
                    "Keskin Nişan I",
                    "+10% Hasar",
                    1,
                    null,
                    PermanentEffectType.DamageMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "archer_precision_2",
                    "archer",
                    "precision",
                    2,
                    "Keskin Nişan II",
                    "+10% Hasar",
                    1,
                    "archer_precision_1",
                    PermanentEffectType.DamageMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "archer_precision_3",
                    "archer",
                    "precision",
                    3,
                    "Keskin Nişan III",
                    "+15% Hasar",
                    1,
                    "archer_precision_2",
                    PermanentEffectType.DamageMultiplier,
                    0.15f),

                // Branch 2 - Tempo (AttackSpeedMultiplier: 1.20x)
                new SkillNodeDefinition(
                    "archer_tempo_1",
                    "archer",
                    "tempo",
                    1,
                    "Hızlı Çekiş I",
                    "+5% Saldırı Hızı",
                    1,
                    null,
                    PermanentEffectType.AttackSpeedMultiplier,
                    0.05f),
                new SkillNodeDefinition(
                    "archer_tempo_2",
                    "archer",
                    "tempo",
                    2,
                    "Hızlı Çekiş II",
                    "+5% Saldırı Hızı",
                    1,
                    "archer_tempo_1",
                    PermanentEffectType.AttackSpeedMultiplier,
                    0.05f),
                new SkillNodeDefinition(
                    "archer_tempo_3",
                    "archer",
                    "tempo",
                    3,
                    "Hızlı Çekiş III",
                    "+10% Saldırı Hızı",
                    1,
                    "archer_tempo_2",
                    PermanentEffectType.AttackSpeedMultiplier,
                    0.10f),

                // Branch 3 - Mobility / Survival (MovementSpeedMultiplier: 1.15x, MaxHealthMultiplier: 1.10x)
                new SkillNodeDefinition(
                    "archer_survival_1",
                    "archer",
                    "survival",
                    1,
                    "Rüzgar Adımı I",
                    "+5% Hareket Hızı",
                    1,
                    null,
                    PermanentEffectType.MovementSpeedMultiplier,
                    0.05f),
                new SkillNodeDefinition(
                    "archer_survival_2",
                    "archer",
                    "survival",
                    2,
                    "Avcı Çevikliği II",
                    "+10% Azami Can",
                    1,
                    "archer_survival_1",
                    PermanentEffectType.MaxHealthMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "archer_survival_3",
                    "archer",
                    "survival",
                    3,
                    "Rüzgar Adımı III",
                    "+10% Hareket Hızı",
                    1,
                    "archer_survival_2",
                    PermanentEffectType.MovementSpeedMultiplier,
                    0.10f)
            };

            tree.Initialize("archer", nodes);
            EditorUtility.SetDirty(tree);

            // Connect to Character_Archer.asset
            var archerDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherDefPath);
            if (archerDef != null)
            {
                archerDef.SetSkillTree(tree);
                EditorUtility.SetDirty(archerDef);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ARCHER SKILL TREE ASSET BUILT SUCCESSFULLY]");
            return tree;
        }

        [MenuItem("DungeonRoguelite/Phase 12/Build Gunner Skill Tree Asset")]
        public static SkillTreeDefinition BuildGunnerTree()
        {
            EnsureProgressionDir();

            var tree = AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>(GunnerTreePath);
            if (tree == null)
            {
                tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
                AssetDatabase.CreateAsset(tree, GunnerTreePath);
            }

            var nodes = new List<SkillNodeDefinition>
            {
                // Branch 1 - Firepower (DamageMultiplier: 1.35x)
                new SkillNodeDefinition(
                    "gunner_firepower_1",
                    "gunner",
                    "firepower",
                    1,
                    "Ateş Gücü I",
                    "+10% Hasar",
                    1,
                    null,
                    PermanentEffectType.DamageMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "gunner_firepower_2",
                    "gunner",
                    "firepower",
                    2,
                    "Ateş Gücü II",
                    "+10% Hasar",
                    1,
                    "gunner_firepower_1",
                    PermanentEffectType.DamageMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "gunner_firepower_3",
                    "gunner",
                    "firepower",
                    3,
                    "Ateş Gücü III",
                    "+15% Hasar",
                    1,
                    "gunner_firepower_2",
                    PermanentEffectType.DamageMultiplier,
                    0.15f),

                // Branch 2 - Cadence (AttackSpeedMultiplier: 1.20x)
                new SkillNodeDefinition(
                    "gunner_cadence_1",
                    "gunner",
                    "cadence",
                    1,
                    "Seri Tetik I",
                    "+5% Saldırı Hızı",
                    1,
                    null,
                    PermanentEffectType.AttackSpeedMultiplier,
                    0.05f),
                new SkillNodeDefinition(
                    "gunner_cadence_2",
                    "gunner",
                    "cadence",
                    2,
                    "Seri Tetik II",
                    "+5% Saldırı Hızı",
                    1,
                    "gunner_cadence_1",
                    PermanentEffectType.AttackSpeedMultiplier,
                    0.05f),
                new SkillNodeDefinition(
                    "gunner_cadence_3",
                    "gunner",
                    "cadence",
                    3,
                    "Seri Tetik III",
                    "+10% Saldırı Hızı",
                    1,
                    "gunner_cadence_2",
                    PermanentEffectType.AttackSpeedMultiplier,
                    0.10f),

                // Branch 3 - Handling (MovementSpeedMultiplier: 1.10x, MaxHealthMultiplier: 1.10x)
                new SkillNodeDefinition(
                    "gunner_handling_1",
                    "gunner",
                    "handling",
                    1,
                    "Hafif Teçhizat I",
                    "+5% Hareket Hızı",
                    1,
                    null,
                    PermanentEffectType.MovementSpeedMultiplier,
                    0.05f),
                new SkillNodeDefinition(
                    "gunner_handling_2",
                    "gunner",
                    "handling",
                    2,
                    "Taktik Zırh II",
                    "+10% Azami Can",
                    1,
                    "gunner_handling_1",
                    PermanentEffectType.MaxHealthMultiplier,
                    0.10f),
                new SkillNodeDefinition(
                    "gunner_handling_3",
                    "gunner",
                    "handling",
                    3,
                    "Hafif Teçhizat III",
                    "+5% Hareket Hızı",
                    1,
                    "gunner_handling_2",
                    PermanentEffectType.MovementSpeedMultiplier,
                    0.05f)
            };

            tree.Initialize("gunner", nodes);
            EditorUtility.SetDirty(tree);

            // Connect to Character_Gunner.asset
            var gunnerDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(GunnerDefPath);
            if (gunnerDef != null)
            {
                gunnerDef.SetSkillTree(tree);
                EditorUtility.SetDirty(gunnerDef);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GUNNER SKILL TREE ASSET BUILT SUCCESSFULLY]");
            return tree;
        }

        private static void EnsureProgressionDir()
        {
            if (!AssetDatabase.IsValidFolder(ProgressionDir))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Progression");
            }
        }
    }
}
