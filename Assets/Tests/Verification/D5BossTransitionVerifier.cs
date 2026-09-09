using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>Exercises real WaveManager spawning and normal EnemyHealth death callbacks through the boss wave.</summary>
    public sealed class D5BossTransitionVerifier : MonoBehaviour
    {
        private void Start() { StartCoroutine(Verify()); }
        private IEnumerator Verify()
        {
            var d5=AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_05.asset"); var player=new GameObject("D5TransitionPlayer").transform; var manager=new GameObject("D5TransitionManager").AddComponent<WaveManager>();
            manager.ConfigureFromDungeon(d5); manager.SetPlayerTarget(player); manager.SetSpawnPoints(new[]{manager.transform}); manager.BeginDungeon();
            yield return new WaitForSeconds(4.2f); Check(manager.CurrentWaveIndex==0 && manager.LivingEnemyCount==7,"Wave 1 becomes active with seven living enemies");
            foreach(var enemy in manager.ActiveEnemies.ToArray()) enemy.TakeDamage(99999f);
            yield return new WaitForSeconds(7.2f); Check(manager.CurrentWaveIndex==1 && manager.LivingEnemyCount==11,"normal deaths advance into Wave 2");
            foreach(var enemy in manager.ActiveEnemies.ToArray()) enemy.TakeDamage(99999f);
            yield return new WaitForSeconds(1.2f); var boss=manager.CurrentWaveSpawned.FirstOrDefault(x=>x!=null && x.GetComponent<BossWardenController>()!=null); Check(manager.CurrentWaveIndex==2 && boss!=null && manager.LivingEnemyCount==1,"Wave 2 clear advances and spawns The Ash Warden");
            Check(boss.MaxHealth==1680f,"boss retains D5 runtime scaling on real WaveManager path");
            Debug.Log("[D5 BOSS TRANSITION COMPLETE] PASSED: 4 checks passed."); Debug.Log("[D5 BOSS TRANSITION STATE RESTORED]"); UnityEngine.Object.Destroy(player.gameObject); UnityEngine.Object.Destroy(manager.gameObject); if(Application.isBatchMode)EditorApplication.Exit(0);
        }
        private static void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);Debug.Log("[CHECK PASSED] "+text);}
    }
}
