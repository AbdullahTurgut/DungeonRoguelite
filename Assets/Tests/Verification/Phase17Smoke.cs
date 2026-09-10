#if UNITY_EDITOR
using System;
using System.Linq;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonRoguelite.Tests
{
    public sealed class Phase17Smoke : MonoBehaviour
    {
        public int DungeonNumber;
        private Phase12StateSnapshot snapshot;
        private WaveManager waves;
        private DungeonCompletionController completion;
        private PlayerExperience experience;
        private UpgradeManager upgrades;
        private float deadline;
        private int defeated, victories;
        private bool bound, finished;
        private int EntryLevel => DungeonNumber < 8 ? 8 : 9;
        private int EntryXP => DungeonNumber == 6 ? 68 : DungeonNumber == 7 ? 968 : DungeonNumber == 8 ? 359 : 1659;
        private int EntryTotal => DungeonNumber == 6 ? 3285 : DungeonNumber == 7 ? 4185 : DungeonNumber == 8 ? 5285 : 6585;
        private int EarnedXP => DungeonNumber == 6 ? 900 : DungeonNumber == 7 ? 1100 : DungeonNumber == 8 ? 1300 : 1600;
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            snapshot=new Phase12StateSnapshot();
            try
            {
                Debug.Log($"[D{DungeonNumber} SMOKE START]");
                DungeonProgression.ResetProgression(); PermanentProgression.ResetAllProgression();
                var dungeon=AssetDatabase.LoadAssetAtPath<DungeonDefinition>($"Assets/ScriptableObjects/Dungeons/Dungeon_{DungeonNumber:00}.asset");
                Require(dungeon.RequiredDungeonId=="dungeon_"+(DungeonNumber-1) && !DungeonProgression.IsDungeonUnlocked(dungeon),"locked until preceding dungeon completion");
                for(int i=1;i<DungeonNumber;i++)DungeonProgression.RecordDungeonCompleted("dungeon_"+i);
                var hero=AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Warrior.asset");
                CharacterSelectionSession.SetSelection(hero);
                var weapon=hero.WeaponCatalog.Find("warrior_tier_1");PermanentProgression.TryClaimWeapon(hero.Id,weapon);PermanentProgression.TryEquipWeapon(hero.Id,weapon);
                RunProgressionSession.StartNewRun(hero.Id);
                RunProgressionSession.CommitDungeonVictory(EntryLevel,EntryXP,EntryTotal,Array.Empty<string>());
                Require(DungeonProgression.IsDungeonUnlocked(dungeon),"prerequisite unlock");
                DungeonRunSession.SetSelection(dungeon);
                deadline=Time.realtimeSinceStartup+100;
                SceneManager.LoadScene(dungeon.SceneName);
            }
            catch(Exception ex) { Fail(ex); }
        }
        private void Update()
        {
            if(finished)return;
            try
            {
                if(Time.realtimeSinceStartup>deadline)throw new Exception("Lifecycle timeout");
                if(SceneManager.GetActiveScene().name!=$"Dungeon_{DungeonNumber:00}")return;
                if(!bound)
                {
                    waves=FindFirstObjectByType<WaveManager>();completion=FindFirstObjectByType<DungeonCompletionController>();experience=FindFirstObjectByType<PlayerExperience>();upgrades=FindFirstObjectByType<UpgradeManager>();
                    Require(waves!=null && completion!=null && experience!=null && upgrades!=null,"scene runtime bindings");
                    waves.OnEnemyDefeated += _=>defeated++;
                    completion.OnDungeonCompleted += _=>victories++;
                    bound=true;
                }
                if(upgrades.IsSelectionActive)upgrades.SelectUpgrade(upgrades.GetAvailableChoices()[0]);
                if(!upgrades.IsSelectionActive && !completion.IsCompletionFinished)Time.timeScale=4;
                foreach(var enemy in waves.ActiveEnemies.ToArray()) if(enemy!=null)enemy.TakeDamage(999999);
                foreach(var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None))if(!pickup.IsCollected)pickup.TryCollect(experience);
                if(victories==0)return;
                Require(defeated==(DungeonNumber==6?55:DungeonNumber==7?70:DungeonNumber==8?80:90) && victories==1 && waves.CurrentWaveIndex==(DungeonNumber==9?5:4),"all real waves completed once");
                Require(experience.TotalXPEarned==EntryTotal+EarnedXP && experience.Level==(DungeonNumber==6?8:DungeonNumber==9?10:9) && experience.CurrentXP==(DungeonNumber==6?968:DungeonNumber==7?359:DungeonNumber==8?1659:696),"exact campaign XP and level");
                Require(experience.XPToNextLevel==(DungeonNumber==6?1709:DungeonNumber==9?3844:2563),"production next-level threshold");
                Require(DungeonProgression.IsDungeonCompleted("dungeon_"+DungeonNumber) && PermanentProgression.GetAvailablePoints("warrior")== (DungeonNumber<8?1:2),"completion and first-clear points persisted");
                Require(!PermanentProgression.TryAwardDungeonFirstClear("warrior","dungeon_"+DungeonNumber,out _),"replay cannot farm points");
                Require(RunProgressionSession.TotalXP==experience.TotalXPEarned,"victory commits run state");
                var hero=CharacterSelectionSession.SelectedCharacter;
                Require(PermanentProgression.GetEquippedWeapon(hero.Id,hero.WeaponCatalog)?.Id=="warrior_tier_1","Tier I survives dungeon completion");
                Finish(true);
            }
            catch(Exception ex) { Fail(ex); }
        }
        private void Fail(Exception ex) { Debug.LogException(ex);Finish(false); }
        private void Finish(bool success)
        {
            finished=true;snapshot?.Dispose();snapshot=null;
            Debug.Log($"[D{DungeonNumber} SMOKE {(success?"PASSED":"FAILED")}]");
            EditorApplication.Exit(success?0:1);
        }
        private static void Require(bool value,string message) { if(!value)throw new InvalidOperationException(message); }
    }
}
#endif
