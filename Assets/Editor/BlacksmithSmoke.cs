using System;
using System.Reflection;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Tests;
using DungeonRoguelite.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    public static class BlacksmithSmoke
    {
        public static void Run()
        {
            Debug.Log("[BLACKSMITH SMOKE START]");
            using (var snapshot = new Phase12StateSnapshot())
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                // Existing v3 save: no new flag, and an already-equipped second hero must survive.
                PlayerPrefs.SetString(PermanentProgression.PrefsKey,"{\"version\":3,\"characters\":[{\"characterId\":\"gunner\",\"availableSkillPoints\":4,\"claimedWeaponIds\":[\"gunner_tier_1\"],\"equippedWeaponId\":\"gunner_tier_1\"}]}");
                Reload(); DungeonProgression.ResetProgression();
                Require(!PermanentProgression.NeedsBlacksmithIntro,"locked before D5");
                DungeonProgression.RecordDungeonCompleted("dungeon_5");
                Require(PermanentProgression.NeedsBlacksmithIntro,"legacy D5 completion eligible");
                var warrior = AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Warrior.asset");
                var catalog = warrior.WeaponCatalog;
                var weapon = catalog.Find("warrior_tier_1");
                Require(!PermanentProgression.CompleteBlacksmithIntro("archer",weapon),"wrong affinity rejected");
                var root = new GameObject("BlacksmithSmokeCanvas",typeof(Canvas));
                try
                {
                    var intro = root.AddComponent<BlacksmithIntroSequence>();
                    intro.Begin(warrior,weapon,null);
                    Require(intro.IsRunning && root.transform.Find("BlacksmithIntroduction/Dialogue/Line") != null,"intro hierarchy created");
                    // Drive the actual dialogue/reward path, bypassing only the input debounce clock.
                    for (int i=0;i<4;i++)
                    {
                        typeof(BlacksmithIntroSequence).GetField("readyAt",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(intro,0f);
                        typeof(BlacksmithIntroSequence).GetMethod("Advance",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(intro,null);
                    }
                    Require(PermanentProgression.GetEquippedWeapon("warrior",catalog)==weapon,"reward auto-equipped");
                    Reload();
                    Require(!PermanentProgression.NeedsBlacksmithIntro && !PermanentProgression.CompleteBlacksmithIntro("warrior",weapon),"seen persists and reward idempotent");
                    var next = root.AddComponent<BlacksmithIntroSequence>(); next.Begin(warrior,weapon,null);
                    Require(!next.IsRunning,"reentry does not replay");
                    var archer = catalog.Find("archer_tier_1");
                    Require(PermanentProgression.TryClaimWeapon("archer",archer) && PermanentProgression.TryEquipWeapon("archer",archer),"other hero still claims and equips");
                    RunProgressionSession.EndRun(); Reload();
                    Require(PermanentProgression.GetEquippedWeapon("archer",catalog)==archer && PermanentProgression.GetEquippedWeapon("warrior",catalog)==weapon && PermanentProgression.GetEquippedWeapon("gunner",catalog)==catalog.Find("gunner_tier_1") && PermanentProgression.GetAvailablePoints("gunner")==4,"all equipment and old points survive EndRun/reload");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            Debug.Log("[BLACKSMITH SMOKE PASSED]");
        }
        private static void Reload() => typeof(PermanentProgression).GetMethod("ResetStaticState",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,null);
        private static void Require(bool value,string message) { if (!value) throw new InvalidOperationException(message); }
    }
}
