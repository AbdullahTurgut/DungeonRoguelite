using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Player;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Enemies;

namespace DungeonRoguelite.Tests
{
    public sealed class D5CompletionOrderingVerifier : MonoBehaviour
    {
        private void Start()=>StartCoroutine(Verify());
        private IEnumerator Verify()
        {
            using(var snapshot=new Phase12StateSnapshot())
            {
                var callbackTrace=new List<string>(); var bossDied=false; var victoryCount=0; var d5=AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_05.asset");
                var player=new GameObject("D5OrderPlayer"); var xp=player.AddComponent<PlayerExperience>(); player.AddComponent<PlayerStats>(); xp.RestoreState(7,7,2085); RunProgressionSession.StartNewRun("warrior"); RunProgressionSession.CommitDungeonVictory(7,7,2085,Array.Empty<string>()); RunProgressionSession.CreateDungeonCheckpoint();
                var manager=new GameObject("D5OrderWaveManager").AddComponent<WaveManager>(); manager.ConfigureFromDungeon(d5); manager.SetPlayerTarget(player.transform); manager.SetSpawnPoints(new[]{manager.transform});
                var upgrades=new GameObject("D5OrderUpgrades").AddComponent<UpgradeManager>(); var uo=new SerializedObject(upgrades); var asset=AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(AssetDatabase.FindAssets("t:UpgradeDefinition").Select(AssetDatabase.GUIDToAssetPath).First()); uo.FindProperty("availableUpgrades").arraySize=1;uo.FindProperty("availableUpgrades").GetArrayElementAtIndex(0).objectReferenceValue=asset;uo.ApplyModifiedPropertiesWithoutUndo();
                var completion=new GameObject("D5OrderCompletion").AddComponent<DungeonCompletionController>(); completion.BindPlayer(xp);
                xp.OnLevelUp += level => { if(level==8) callbackTrace.Add("level-up"); }; upgrades.OnUpgradeChoicesRequested += _=>callbackTrace.Add("upgrade-open"); upgrades.OnUpgradeSelectionClosed += ()=>callbackTrace.Add("upgrade-close"); completion.OnDungeonCompleted += _=>{victoryCount++;callbackTrace.Add("victory");};
                manager.BeginDungeon();
                yield return new WaitForSeconds(4.2f); foreach(var enemy in manager.ActiveEnemies.ToArray())enemy.TakeDamage(99999f);
                yield return new WaitForSeconds(7.2f); foreach(var enemy in manager.ActiveEnemies.ToArray())enemy.TakeDamage(99999f);
                yield return new WaitForSeconds(1.2f); var boss=manager.ActiveEnemies.First(); boss.OnDied += ()=>{bossDied=true;callbackTrace.Add("boss-health-death");};
                Trace("before-boss-death",xp,upgrades,completion,bossDied,victoryCount,callbackTrace);
                Check(!completion.IsCompletionFinished&&victoryCount==0,"before boss death: completion unfinished and victory count is zero");
                boss.TakeDamage(99999f);
                yield return null;
                Trace("after-boss-death-xp-resolution",xp,upgrades,completion,bossDied,victoryCount,callbackTrace);
                Check(bossDied&&xp.Level==8&&xp.CurrentXP==68&&xp.XPToNextLevel==1709&&xp.TotalXPEarned==3285&&upgrades.IsSelectionActive&&!completion.IsCompletionFinished&&RunProgressionSession.Level!=8&&victoryCount==0,"after boss death: reward resolved to Level 8 final XP, selection active, completion uncommitted, and victory count zero");
                Trace("selection-active",xp,upgrades,completion,bossDied,victoryCount,callbackTrace);
                Check(upgrades.IsSelectionActive&&!completion.IsCompletionFinished&&victoryCount==0,"while upgrade selection is active: completion remains blocked and victory count is zero");
                upgrades.SelectUpgrade(upgrades.GetAvailableChoices()[0]);
                yield return null;
                Trace("after-selection-close",xp,upgrades,completion,bossDied,victoryCount,callbackTrace);
                Check(!upgrades.IsSelectionActive&&completion.IsCompletionFinished&&RunProgressionSession.Level==8&&RunProgressionSession.CurrentXP==68&&victoryCount==1,"after selection closes: completion committed Level 8 CurrentXP 68 and victory fired exactly once");
                yield return null;
                Trace("post-completion-stability",xp,upgrades,completion,bossDied,victoryCount,callbackTrace);
                Check(victoryCount==1&&completion.IsCompletionFinished&&!upgrades.IsSelectionActive,"after bounded wait: victory remains exactly once, completion remains finished, and no selection reopens");
                Debug.Log("[D5 COMPLETION ORDER COMPLETE] PASSED: 5 checks passed."); Debug.Log("[D5 COMPLETION ORDER STATE RESTORED]"); UnityEngine.Object.Destroy(player);UnityEngine.Object.Destroy(manager.gameObject);UnityEngine.Object.Destroy(upgrades.gameObject);UnityEngine.Object.Destroy(completion.gameObject);
            }
            if(Application.isBatchMode)EditorApplication.Exit(0);
        }
        private static void Trace(string boundary,PlayerExperience xp,UpgradeManager upgrades,DungeonCompletionController completion,bool bossDied,int victoryCount,List<string> callbackTrace){Debug.Log($"[D5 COMPLETION ORDER TRACE] boundary={boundary}; bossDied={bossDied}; level={xp.Level}; currentXP={xp.CurrentXP}; xpToNext={xp.XPToNextLevel}; totalXP={xp.TotalXPEarned}; selectionActive={upgrades.IsSelectionActive}; completionFinished={completion.IsCompletionFinished}; runLevel={RunProgressionSession.Level}; runCurrentXP={RunProgressionSession.CurrentXP}; victoryCount={victoryCount}; callbackTrace={string.Join(" -> ",callbackTrace)}");}
        private static void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);Debug.Log("[CHECK PASSED] "+text);}
    }
}
