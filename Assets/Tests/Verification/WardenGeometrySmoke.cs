#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Progression;
using UnityEditor;
using UnityEngine;
namespace DungeonRoguelite.Tests
{
    public sealed class WardenGeometrySmoke : MonoBehaviour
    {
        private IEnumerator Start()
        {
            using(var snapshot=new Phase12StateSnapshot())
            {
                var routine=Verify(); bool ok=true;
                while(true)
                {
                    object step;
                    try { if(!routine.MoveNext()) break; step=routine.Current; }
                    catch(Exception ex) { Debug.LogException(ex); ok=false; break; }
                    yield return step;
                }
                snapshot.Dispose(); Debug.Log(ok?"[WARDEN GEOMETRY PASSED]":"[WARDEN GEOMETRY FAILED]");
                EditorApplication.Exit(ok?0:1);
            }
        }
        private IEnumerator Verify()
        {
            Debug.Log("[WARDEN GEOMETRY START]"); Time.timeScale=1;
            PermanentProgression.ResetAllProgression(); DungeonProgression.ResetProgression();
            CharacterSelectionSession.SetSelection(AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Warrior.asset"));
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube); floor.transform.position=new Vector3(0,-.5f,0); floor.transform.localScale=new Vector3(40,1,40);
            var player=new GameObject("GeometryPlayer",typeof(PlayerHealth),typeof(PlayerExperience),typeof(CapsuleCollider));
            player.tag="Player"; var collider=player.GetComponent<CapsuleCollider>(); collider.center=Vector3.up; collider.height=2; collider.radius=.3f;
            var hp=player.GetComponent<PlayerHealth>(); hp.ResetHealthForTesting(1000);
            var boss=Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/AshWarden.prefab"));
            var warden=boss.GetComponent<BossWardenController>(); var health=boss.GetComponent<EnemyHealth>();
            health.InitializeHealth(1.4f); warden.InitializeAttack(1.3f); warden.SetTarget(player.transform);
            player.transform.position=boss.transform.position+Vector3.forward*2.4f;
            Schedule(warden,"nextStrike"); yield return null; yield return null;
            Check(warden.State==BossWardenState.Telegraph,"natural Strike start");
            Vector3 origin=boss.transform.position, direction=boss.transform.forward;
            player.transform.position=origin-direction*2.4f; Physics.SyncTransforms();
            yield return new WaitForSeconds(.65f);
            Check(hp.CurrentHealth==1000 && Vector3.Angle(direction,boss.transform.forward)<.1f && warden.State==BossWardenState.Recovery,"rear dodge safe; committed facing and bounded swing");
            yield return new WaitForSeconds(.9f);
            Schedule(warden,"nextStrike"); yield return null; yield return null;
            Check(warden.State==BossWardenState.Telegraph,"second frontal Strike starts");
            yield return new WaitForSeconds(.65f);
            Check(hp.CurrentHealth==1000-warden.StrikeDamage,"stationary front takes one unchanged hit");
            yield return new WaitForSeconds(.9f);
            player.transform.position=boss.transform.position+Vector3.forward*2.4f;
            Schedule(warden,"nextSlam"); yield return null; yield return null;
            float before=hp.CurrentHealth; origin=boss.transform.position;
            var outline=boss.transform.Find("WardenAttackOutline").GetComponent<LineRenderer>();
            Check(warden.State==BossWardenState.Telegraph && outline.positionCount==65,"Slam circle presented");
            for(int i=0;i<outline.positionCount;i++) { var delta=outline.GetPosition(i)-origin; delta.y=0; Check(Mathf.Abs(delta.magnitude-6.5f)<.001f,"world-space circle matches actual radius"); }
            player.transform.position=origin+Vector3.forward*6.7f; Physics.SyncTransforms();
            yield return new WaitForSeconds(1.2f);
            Check(hp.CurrentHealth==before && warden.State==BossWardenState.Recovery,"outside Slam radius misses");
            yield return new WaitForSeconds(1.05f);
            player.transform.position=boss.transform.position+Vector3.forward*2.4f;
            Schedule(warden,"nextSlam"); yield return null; yield return null;
            yield return new WaitForSeconds(.85f); Check(hp.CurrentHealth==before,"no damage during red wind-up");
            yield return new WaitForSeconds(.4f); Check(hp.CurrentHealth==before-warden.SlamDamage,"inside circle hit at execution only");
            yield return new WaitForSeconds(1.05f);
            health.TakeDamage(health.CurrentHealth-health.MaxHealth*.5f);
            player.transform.position=boss.transform.position+Vector3.forward*10;
            Schedule(warden,"nextBolt"); yield return null; yield return null;
            Check(warden.IsPhaseTwo && warden.State==BossWardenState.Telegraph,"Phase II and Bolt start normally");
            direction=boss.transform.forward; before=hp.CurrentHealth;
            var block=new MaterialPropertyBlock(); outline.GetPropertyBlock(block); Color color=block.GetColor("_BaseColor");
            Check(color.b>color.g && color.r>color.g && outline.positionCount==2,"purple aimed telegraph");
            player.transform.position+=Vector3.right*4; Physics.SyncTransforms();
            yield return new WaitForSeconds(.95f);
            var shots=FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
            Check(shots.Length==2,"Phase II retains double Bolt");
            foreach(var shot in shots) Check(Vector3.Angle(direction,shot.TravelDirection)<.1f,"both bolts retain committed aim");
            yield return new WaitForSeconds(.5f); Check(hp.CurrentHealth==before,"purple projectiles dodgeable");
            // Kill during the next wind-up: pending melee damage must not execute after death.
            player.transform.position=boss.transform.position+Vector3.forward*2.4f;
            Schedule(warden,"nextStrike"); yield return null; yield return null;
            health.TakeDamage(health.CurrentHealth); yield return new WaitForSeconds(.7f);
            Check(health.IsDead && warden.State==BossWardenState.Dead && !outline.enabled && hp.CurrentHealth==before,"death cancels attack and clears cue");
            var xp=player.GetComponent<PlayerExperience>();
            foreach(var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) if(!pickup.IsCollected) pickup.TryCollect(xp);
            Check(xp.TotalXPEarned==960,"boss still awards 960 XP");
            DungeonRunSession.SetSelection(AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_05.asset"));
            Check(DungeonRunSession.SelectedDungeon!=null,"D5 definition exists");
            var completion=new GameObject("Completion").AddComponent<DungeonCompletionController>(); completion.BindPlayer(xp);
            completion.HandleWaveManagerCompletion();
            Check(completion.IsCompletionFinished && DungeonProgression.IsDungeonCompleted("dungeon_5"),"completion boundary still commits D5");
            Check(PermanentProgression.GetAvailablePoints("warrior")==2,"unchanged D5 first-clear reward");
        }
        private static void Schedule(BossWardenController boss,string attack)
        {
            // Isolate the attack under test by adjusting only cooldown timers in this harness.
            foreach(string field in new[]{"nextStrike","nextSlam","nextBolt"})
                typeof(BossWardenController).GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(boss,field==attack?0f:Time.time+100);
        }
        private static void Check(bool value,string message) { if(!value) throw new InvalidOperationException(message); }
    }
}
#endif
