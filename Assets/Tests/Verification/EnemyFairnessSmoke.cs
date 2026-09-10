#if UNITY_EDITOR
using System;
using System.Collections;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Player;
using UnityEditor;
using UnityEngine;
namespace DungeonRoguelite.Tests
{
    public sealed class EnemyFairnessSmoke : MonoBehaviour
    {
        private IEnumerator Start()
        {
            using(var snapshot = new Phase12StateSnapshot())
            {
                var routine=Verify(); bool ok=true;
                while(true)
                {
                    object step;
                    try { if(!routine.MoveNext()) break; step=routine.Current; }
                    catch(Exception ex) { Debug.LogException(ex); ok=false; break; }
                    yield return step;
                }
                snapshot.Dispose(); Debug.Log(ok ? "[ENEMY FAIRNESS PASSED]" : "[ENEMY FAIRNESS FAILED]");
                EditorApplication.Exit(ok?0:1);
            }
        }
        private IEnumerator Verify()
        {
            Debug.Log("[ENEMY FAIRNESS START]"); Time.timeScale=1;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position=new Vector3(0,-.5f,0); floor.transform.localScale=new Vector3(30,1,30);
            var player=new GameObject("DodgePlayer",typeof(PlayerHealth),typeof(CapsuleCollider)); player.tag="Player";
            var collider=player.GetComponent<CapsuleCollider>(); collider.center=Vector3.up; collider.height=2; collider.radius=.3f;
            var hp=player.GetComponent<PlayerHealth>(); player.transform.position=new Vector3(0,0,1.2f);
            var enemy=Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab"));
            var melee=enemy.GetComponent<EnemyAttack>(); melee.SetTarget(player.transform);
            Check(melee.TryAttack() && hp.CurrentHealth==100,"wind-up starts without instant damage");
            var origin=enemy.transform.position; var facing=enemy.transform.forward;
            var cue=enemy.transform.Find("EnemyAttackCue").GetComponent<LineRenderer>();
            Check(cue.enabled,"telegraph visible");
            // Move around the committed strike at 6 m/s, remaining inside radial attack range.
            float angle=0;
            while(angle<90)
            {
                angle=Mathf.Min(90,angle+Mathf.Rad2Deg*6f/1.2f*Time.deltaTime);
                player.transform.position=Quaternion.Euler(0,angle,0)*Vector3.forward*1.2f;
                Physics.SyncTransforms(); yield return null;
            }
            Check(Vector3.Angle(enemy.transform.forward,facing)<.1f && Vector2.Distance(new Vector2(origin.x,origin.z),new Vector2(enemy.transform.position.x,enemy.transform.position.z))<.02f,"no wind-up tracking or chasing");
            float deadline=Time.time+3;
            while(melee.IsAttacking && Time.time<deadline) yield return null;
            Check(!melee.IsAttacking && hp.CurrentHealth==100 && !cue.enabled,"committed melee sector misses and ends");
            // Stand still for the next attack: it must remain dangerous and hit only once.
            while(hp.CurrentHealth==100 && Time.time<deadline) yield return null;
            Check(hp.CurrentHealth==100-melee.Damage,"standing in strike takes unchanged damage");
            yield return new WaitForSeconds(.16f);
            Check(hp.CurrentHealth==100-melee.Damage,"one hit per bounded active window");
            Destroy(enemy); yield return null;
            player.transform.position=new Vector3(0,0,7); Physics.SyncTransforms();
            enemy=Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Ranged.prefab"));
            var ranged=enemy.GetComponent<EnemyRangedAttack>(); ranged.SetTarget(player.transform);
            float before=hp.CurrentHealth;
            Check(ranged.TryAttack() && ranged.ActiveProjectileCount==0,"ranged wind-up before projectile");
            float until=Time.time+.4f;
            while(Time.time<until) { player.transform.position+=Vector3.right*6*Time.deltaTime; Physics.SyncTransforms(); yield return null; }
            deadline=Time.time+2;
            while(ranged.ActiveProjectileCount==0 && Time.time<deadline) yield return null;
            var shot=FindFirstObjectByType<EnemyProjectile>();
            Check(shot!=null && Vector3.Angle(shot.TravelDirection,Vector3.forward)<.1f,"projectile retains telegraphed direction");
            yield return new WaitForSeconds(.8f);
            Check(hp.CurrentHealth==before,"real projectile misses lateral dodge");
            // Cancel remaining shot, then permit a new committed shot at the stationary player.
            shot.Cancel();
            deadline=Time.time+4;
            while(hp.CurrentHealth==before && Time.time<deadline) yield return null;
            Check(hp.CurrentHealth==before-ranged.Damage,"stationary target still hit by real projectile");
            enemy.GetComponent<EnemyHealth>().TakeDamage(99999);
            yield return null;
            Check(!ranged.IsAttacking && ranged.ActiveProjectileCount==0,"death clears pending attack and projectiles");
        }
        private static void Check(bool value,string message) { if(!value) throw new InvalidOperationException(message); }
    }
}
#endif
