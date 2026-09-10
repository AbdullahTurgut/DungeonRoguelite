#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Weapons;
using UnityEditor;
using UnityEngine;
namespace DungeonRoguelite.Tests
{
    public sealed class WardenSpacingSmoke : MonoBehaviour
    {
        private IEnumerator Start()
        {
            using(var snapshot=new Phase12StateSnapshot())
            {
                var routine=Verify();bool ok=true;
                while(true)
                {
                    object step;
                    try{if(!routine.MoveNext())break;step=routine.Current;}
                    catch(Exception ex){Debug.LogException(ex);ok=false;break;}
                    yield return step;
                }
                Debug.Log(ok?"[WARDEN SPACING PASSED]":"[WARDEN SPACING FAILED]");
                snapshot.Dispose();EditorApplication.Exit(ok?0:1);
            }
        }
        private IEnumerator Verify()
        {
            Debug.Log("[WARDEN SPACING START]");Time.timeScale=1;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=new Vector3(0,-.5f,0);floor.transform.localScale=new Vector3(30,1,30);
            var player=new GameObject("SpacingPlayer",typeof(PlayerHealth),typeof(PlayerStats),typeof(PlayerExperience),typeof(MeleeWeapon));player.tag="Player";
            player.transform.position=new Vector3(0,0,2.8f);player.transform.rotation=Quaternion.Euler(0,180,0);
            var health=player.GetComponent<PlayerHealth>();var weapon=player.GetComponent<MeleeWeapon>();
            var boss=Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/AshWarden.prefab"));var warden=boss.GetComponent<BossWardenController>();var bossHealth=boss.GetComponent<EnemyHealth>();bossHealth.InitializeHealth(1.4f);warden.InitializeAttack(1.3f);warden.SetTarget(player.transform);
            // Isolate Strike for this small timing check; Slam/Bolt code and production values are untouched.
            foreach(string field in new[]{"nextSlam","nextBolt"})typeof(BossWardenController).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(warden,Time.time+100);
            float deadline=Time.time+8;
            while(warden.State!=BossWardenState.Telegraph && Time.time<deadline)yield return null;
            Check(warden.State==BossWardenState.Telegraph,"Strike starts normally");
            player.transform.position=new Vector3(0,0,3.5f);
            while(warden.State!=BossWardenState.Recovery && Time.time<deadline)yield return null;
            Check(warden.State==BossWardenState.Recovery && health.CurrentHealth==health.MaxHealth,"evading the .45-second telegraph avoids Strike");
            // 1.1m approach at 6m/s costs <.2s, leaving time for two .5s-cooldown attacks in .85s recovery.
            yield return new WaitForSeconds(.2f);player.transform.position=new Vector3(0,0,2.4f);Physics.SyncTransforms();float before=bossHealth.CurrentHealth;
            Check(weapon.Range==2.5f && weapon.Damage==25 && weapon.AttackCooldown==.5f && weapon.TryAttack(),"unchanged Warrior attack executes");
            yield return new WaitForSeconds(.51f);Check(weapon.TryAttack() && bossHealth.CurrentHealth==before-50 && health.CurrentHealth==health.MaxHealth,"two real melee hits fit the punish window");
            while(health.CurrentHealth==health.MaxHealth && Time.time<deadline)yield return null;
            Check(health.CurrentHealth==health.MaxHealth-warden.StrikeDamage,"remaining close still takes unchanged Strike damage");
            bossHealth.TakeDamage(bossHealth.CurrentHealth-bossHealth.MaxHealth*.5f);
            yield return new WaitForSeconds(1);
            Check(warden.IsPhaseTwo,"Phase II still activates");bossHealth.TakeDamage(bossHealth.CurrentHealth);yield return null;
            foreach(var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None))if(!pickup.IsCollected)pickup.TryCollect(player.GetComponent<PlayerExperience>());
            Check(bossHealth.IsDead && player.GetComponent<PlayerExperience>().TotalXPEarned==960,"death retains 960 boss XP");
        }
        private static void Check(bool value,string text){if(!value)throw new InvalidOperationException(text);}
    }
}
#endif
