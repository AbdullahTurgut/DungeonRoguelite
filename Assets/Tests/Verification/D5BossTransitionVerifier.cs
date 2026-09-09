using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>Exercises real WaveManager spawning and normal EnemyHealth death callbacks through the boss wave.</summary>
    public sealed class D5BossTransitionVerifier : MonoBehaviour
    {
        private void Start() { StartCoroutine(Verify()); }
        private IEnumerator Verify()
        {
            var d5=AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_05.asset"); var playerGo=new GameObject("D5TransitionPlayer"); var player=playerGo.transform; var experience=playerGo.AddComponent<PlayerExperience>(); var manager=new GameObject("D5TransitionManager").AddComponent<WaveManager>();
            int completed=0; int defeated=0; manager.OnDungeonCompleted += () => completed++; manager.OnEnemyDefeated += _ => defeated++;
            manager.ConfigureFromDungeon(d5); manager.SetPlayerTarget(player); manager.SetSpawnPoints(new[]{manager.transform}); manager.BeginDungeon();
            yield return new WaitForSeconds(4.2f); Check(manager.CurrentWaveIndex==0 && manager.LivingEnemyCount==7,"Wave 1 becomes active with seven living enemies");
            foreach(var enemy in manager.ActiveEnemies.ToArray()) enemy.TakeDamage(99999f);
            yield return new WaitForSeconds(7.2f); Check(manager.CurrentWaveIndex==1 && manager.LivingEnemyCount==11,"normal deaths advance into Wave 2");
            foreach(var enemy in manager.ActiveEnemies.ToArray()) enemy.TakeDamage(99999f);
            yield return new WaitForSeconds(1.2f); var boss=manager.CurrentWaveSpawned.FirstOrDefault(x=>x!=null && x.GetComponent<BossWardenController>()!=null); Check(manager.CurrentWaveIndex==2 && boss!=null && manager.LivingEnemyCount==1 && completed==0,"Wave 2 clear advances once to the sole boss encounter");
            Check(boss.MaxHealth==1680f,"boss retains D5 runtime scaling on real WaveManager path");
            var reward=boss.GetComponent<ExperienceReward>(); Check(reward != null && reward.XPAmount==960 && !reward.HasRewarded,"boss is unique and holds the 960 XP reward before death");
            boss.TakeDamage(99999f); yield return new WaitForSeconds(.3f);
            foreach(var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) if(!pickup.IsCollected) pickup.TryCollect(experience);
            yield return new WaitForSeconds(.8f); Check(completed==1 && manager.CurrentState==WaveState.DungeonCompleted && manager.CurrentWaveIndex==2,"boss death fires final completion exactly once with no further wave");
            Check(reward.HasRewarded && defeated==19 && experience.TotalXPEarned==1200,"all 18 warmup rewards plus boss reward resolve to 1200 XP");
            Check(manager.LivingEnemyCount==0 && FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None).Length==0,"no survivor or active boss projectile remains after completion");
            Debug.Log("[D5 BOSS TRANSITION COMPLETE] PASSED: 8 checks passed."); Debug.Log("[D5 BOSS TRANSITION STATE RESTORED]"); UnityEngine.Object.Destroy(playerGo); UnityEngine.Object.Destroy(manager.gameObject); if(Application.isBatchMode)EditorApplication.Exit(0);
        }
        private static void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);Debug.Log("[CHECK PASSED] "+text);}
    }
}
