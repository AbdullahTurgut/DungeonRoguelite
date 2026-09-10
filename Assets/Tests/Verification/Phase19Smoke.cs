#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.UI;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DungeonRoguelite.Tests
{
    public sealed class Phase19Smoke : MonoBehaviour
    {
        private Phase12StateSnapshot snapshot;
        private WaveManager waves;
        private PlayerExperience experience;
        private UpgradeManager upgrades;
        private BossHealthBarUI hud;
        private HollowCastellanController boss;
        private EnemyHealth bossHealth;
        private int defeated,victories,cleaves,dashes,fans,stage;
        private float deadline;
        private bool bound,finished;
        private void Start()
        {
            DontDestroyOnLoad(gameObject);snapshot=new Phase12StateSnapshot();
            try
            {
                Debug.Log("[D10 SMOKE START]");DungeonProgression.ResetProgression();PermanentProgression.ResetAllProgression();
                for(int i=1;i<10;i++)DungeonProgression.RecordDungeonCompleted("dungeon_"+i);
                var hero=AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Warrior.asset");CharacterSelectionSession.SetSelection(hero);
                var tier1=hero.WeaponCatalog.Find("warrior_tier_1");PermanentProgression.TryClaimWeapon(hero.Id,tier1);PermanentProgression.TryEquipWeapon(hero.Id,tier1);
                Check(!PermanentProgression.CanClaimWeapon(hero.Id,hero.WeaponCatalog.Find("warrior_tier_2")),"Tier II locked before D10");
                RunProgressionSession.StartNewRun(hero.Id);RunProgressionSession.CommitDungeonVictory(10,696,8185,Array.Empty<string>());
                var dungeon=AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_10.asset");Check(DungeonProgression.IsDungeonUnlocked(dungeon),"D9 unlocks D10");DungeonRunSession.SetSelection(dungeon);
                deadline=Time.realtimeSinceStartup+120;SceneManager.LoadScene("Dungeon_10");
            }
            catch(Exception ex){Fail(ex);}
        }
        private void Update()
        {
            if(finished)return;
            try
            {
                if(Time.realtimeSinceStartup>deadline)throw new Exception("D10 lifecycle timeout");
                if(SceneManager.GetActiveScene().name!="Dungeon_10")return;
                if(!bound)
                {
                    waves=FindFirstObjectByType<WaveManager>();experience=FindFirstObjectByType<PlayerExperience>();upgrades=FindFirstObjectByType<UpgradeManager>();hud=FindFirstObjectByType<BossHealthBarUI>();
                    FindFirstObjectByType<PlayerHealth>().ResetHealthForTesting(10000); // Keep this lifecycle harness alive; no production balance change.
                    waves.OnEnemyDefeated+=_=>defeated++;FindFirstObjectByType<DungeonCompletionController>().OnDungeonCompleted+=_=>victories++;bound=true;
                }
                if(upgrades.IsSelectionActive)upgrades.SelectUpgrade(upgrades.GetAvailableChoices()[0]);
                if(victories==0 && !upgrades.IsSelectionActive)Time.timeScale=3;
                foreach(var enemy in waves.ActiveEnemies.ToArray())
                {
                    var candidate=enemy.GetComponent<HollowCastellanController>();
                    if(candidate==null){enemy.TakeDamage(999999);continue;}
                    if(boss!=null)continue;
                    boss=candidate;bossHealth=enemy;
                    Check(defeated==80 && waves.CurrentWaveIndex==3 && enemy.MaxHealth==3800 && enemy.GetComponent<ExperienceReward>().XPAmount==2000,"warm-ups then one correctly scaled boss");
                    Check(hud.BoundHealth==enemy && hud.BoundBoss==boss && hud.DisplayedHealthRatio==1,"shared boss HUD full binding");
                    Move(boss.transform,new Vector3(0,0,0));Move(experience.transform,new Vector3(0,0,3));
                    boss.OnAttackExecuted+=attack=>{if(attack=="Cleave")cleaves++;if(attack=="Dash")dashes++;if(attack=="Fan"){fans++;Check(FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None).Length==3,"three shard fan");}};
                }
                foreach(var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None))if(!pickup.IsCollected)pickup.TryCollect(experience);
                if(stage==0 && cleaves>0){Move(experience.transform,boss.transform.position+Vector3.forward*10);stage=1;}
                if(stage==1 && dashes>0)
                {
                    bossHealth.TakeDamage(1900);Check(boss.IsPhaseTwo && Mathf.Approximately(hud.DisplayedHealthRatio,.5f),"real damage triggers Phase II and HUD half");
                    Move(boss.transform,Vector3.zero);Move(experience.transform,new Vector3(0,0,8));stage=2;
                }
                if(stage==2 && fans>0){bossHealth.TakeDamage(bossHealth.CurrentHealth);Check(hud.DisplayedHealthRatio==0 && !hud.IsVisible,"boss death zeroes/hides HUD");stage=3;}
                if(victories==0)return;
                Check(stage==3 && victories==1 && defeated==81 && experience.Level==11 && experience.CurrentXP==52 && experience.XPToNextLevel==5767 && experience.TotalXPEarned==11385,"exact D10 completion and campaign XP");
                Check(PermanentProgression.GetAvailablePoints("warrior")==3 && !PermanentProgression.TryAwardDungeonFirstClear("warrior","dungeon_10",out _),"three first-clear points once");
                Check(DungeonProgression.IsFirstCampaignCompleted,"persistent campaign completion milestone");
                var hero=CharacterSelectionSession.SelectedCharacter;var tier2=hero.WeaponCatalog.Find("warrior_tier_2");
                Check(PermanentProgression.TryClaimWeapon(hero.Id,tier2) && PermanentProgression.TryEquipWeapon(hero.Id,tier2),"Tier II claim/equip unlocked by boss victory");
                RunProgressionSession.EndRun();typeof(PermanentProgression).GetMethod("ResetStaticState",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
                Check(PermanentProgression.GetEquippedWeapon(hero.Id,hero.WeaponCatalog)==tier2,"Tier II survives EndRun and reload");Finish(true);
            }
            catch(Exception ex){Fail(ex);}
        }
        private static void Move(Transform actor,Vector3 position){var cc=actor.GetComponent<CharacterController>();if(cc!=null)cc.enabled=false;actor.position=position;if(cc!=null)cc.enabled=true;}
        private static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        private void Fail(Exception ex){Debug.LogException(ex);Finish(false);}
        private void Finish(bool success){if(finished)return;finished=true;snapshot?.Dispose();Debug.Log(success?"[D10 SMOKE PASSED]":"[D10 SMOKE FAILED]");EditorApplication.Exit(success?0:1);}
    }
}
#endif
