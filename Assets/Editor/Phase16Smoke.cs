using System;
using System.Linq;
using System.Reflection;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Tests;
using DungeonRoguelite.UI;
using DungeonRoguelite.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    public static class Phase16Smoke
    {
        public static void BuildAndVerify()
        {
            Phase16Setup.Run();
            Debug.Log("[PHASE 16 SMOKE START]");
            using (var snapshot = new Phase12StateSnapshot())
            {
                PlayerPrefs.SetString(PermanentProgression.PrefsKey,"{\"version\":2,\"characters\":[{\"characterId\":\"warrior\",\"availableSkillPoints\":4,\"unlockedNodeIds\":[\"warrior_power_1\"],\"rewardedDungeonIds\":[\"dungeon_5\"],\"legacyD5RewardPending\":false}]}");
                Reload();
                Require(PermanentProgression.GetAvailablePoints("warrior")==4 && PermanentProgression.IsNodePurchased("warrior","warrior_power_1") && PermanentProgression.IsDungeonRewardClaimed("warrior","dungeon_5"),"v2 migration preserves economy");
                var catalog=AssetDatabase.LoadAssetAtPath<WeaponCatalog>(Phase16Setup.CatalogPath);
                DungeonProgression.ResetProgression();
                Require(!PermanentProgression.TryClaimWeapon("warrior",catalog.Weapons[0]),"D5 milestone locked");
                DungeonProgression.RecordDungeonCompleted("dungeon_5");
                foreach(var weapon in catalog.Weapons)
                {
                    Require(PermanentProgression.TryClaimWeapon(weapon.CharacterId,weapon) && PermanentProgression.TryEquipWeapon(weapon.CharacterId,weapon),"claim and equip");
                    Require(!PermanentProgression.TryEquipWeapon(weapon.CharacterId == "warrior" ? "archer" : "warrior",weapon),"affinity");
                }
                RunProgressionSession.EndRun(); Reload();
                Require(JsonUtility.FromJson<PermanentProgression.PermanentSaveData>(PlayerPrefs.GetString(PermanentProgression.PrefsKey)).version==3,"v3 persisted");
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                foreach(var weapon in catalog.Weapons)
                {
                    var character=AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"Assets/ScriptableObjects/Characters/Character_{char.ToUpperInvariant(weapon.CharacterId[0])+weapon.CharacterId.Substring(1)}.asset");
                    Require(character.WeaponCatalog==catalog && PermanentProgression.GetEquippedWeapon(character.Id,catalog)==weapon,"equipment survives EndRun and reload");
                    var root=new GameObject("SpawnSmoke"); var spawner=root.AddComponent<PlayerSpawner>(); spawner.SetSpawnPoint(root.transform); spawner.SetDefaultCharacter(character);
                    var actor=spawner.Spawn(character);
                    try
                    {
                        var stats=actor.GetComponent<PlayerStats>();
                        float skill=character.Id=="warrior" ? 1.1f : 1f;
                        Require(Mathf.Abs(stats.DamageMultiplier-skill*weapon.DamageMultiplier)<.001f && Mathf.Abs(stats.AttackSpeedMultiplier-weapon.AttackSpeedMultiplier)<.001f,"production spawn stat layering");
                        stats.AddDamageBonus(.2f); Require(Mathf.Abs(stats.DamageMultiplier-skill*weapon.DamageMultiplier*1.2f)<.001f,"temporary layer");
                        stats.ResetModifiers(); Require(Mathf.Abs(stats.DamageMultiplier-skill*weapon.DamageMultiplier)<.001f,"temporary reset preserves equipment");
                    }
                    finally { UnityEngine.Object.DestroyImmediate(actor.gameObject); UnityEngine.Object.DestroyImmediate(root); }
                }
                var scene=EditorSceneManager.OpenScene(Phase16Setup.HubPath);
                var hub=UnityEngine.Object.FindFirstObjectByType<ArmoryHubUI>();
                Require(hub!=null && !scene.GetRootGameObjects().Any(g=>g.GetComponentInChildren<PlayerAttack>()!=null),"passive Hub scene");
                var data=new SerializedObject(hub);
                foreach(string field in new[]{"armorerButton","returnButton","equipmentPanel","equipmentText","claimButton","equipButton","baseButton","closeButton"}) Require(data.FindProperty(field).objectReferenceValue!=null,"Hub reference "+field);
                Require(EditorBuildSettings.scenes.Any(s=>s.enabled && s.path==Phase16Setup.HubPath),"Hub build route");
                EditorSceneManager.OpenScene(WorldMapCarouselSetup.ScenePath);
                var map=UnityEngine.Object.FindFirstObjectByType<WorldMapController>();
                Require(new SerializedObject(map).FindProperty("armoryButton").objectReferenceValue!=null,"World Map armory entry");
            }
            Debug.Log("[PHASE 16 SMOKE PASSED]");
        }
        private static void Reload() => typeof(PermanentProgression).GetMethod("ResetStaticState",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        private static void Require(bool result,string message) { if(!result) throw new InvalidOperationException(message); }
    }
}
