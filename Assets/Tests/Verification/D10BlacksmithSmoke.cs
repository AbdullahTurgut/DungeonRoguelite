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
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DungeonRoguelite.Tests
{
    public sealed class D10BlacksmithSmoke : MonoBehaviour
    {
        private Phase12StateSnapshot snapshot;
        private float deadline;
        private int stage;
        private bool finished;
        private CharacterDefinition hero;
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            snapshot = new Phase12StateSnapshot();
            try
            {
                Debug.Log("[D10 BLACKSMITH START]");
                DungeonProgression.ResetProgression(); PermanentProgression.ResetAllProgression();
                for (int i=1;i<10;i++) DungeonProgression.RecordDungeonCompleted("dungeon_"+i);
                hero = AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/ScriptableObjects/Characters/Character_Archer.asset");
                CharacterSelectionSession.SetSelection(hero);
                // Missing Tier I presentation/reward is deliberately retained for recovery coverage.
                RunProgressionSession.StartNewRun(hero.Id);
                RunProgressionSession.CommitDungeonVictory(10,696,8185,Array.Empty<string>());
                DungeonRunSession.SetSelection(AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_10.asset"));
                Check(!PermanentProgression.NeedsSecondBlacksmithIntro,"D10 gate closed");
                deadline = Time.realtimeSinceStartup + 120;
                SceneManager.LoadScene("Dungeon_10");
            }
            catch (Exception ex) { Fail(ex); }
        }
        private void Update()
        {
            if (finished) return;
            try
            {
                if (Time.realtimeSinceStartup > deadline) throw new Exception("D10 Blacksmith timeout");
                if (stage == 0 && SceneManager.GetActiveScene().name == "Dungeon_10")
                {
                    var completion = FindFirstObjectByType<DungeonCompletionController>();
                    var xp = FindFirstObjectByType<PlayerExperience>();
                    var upgrades = FindFirstObjectByType<UpgradeManager>();
                    var waves = FindFirstObjectByType<WaveManager>();
                    if (completion == null || xp == null || waves == null) return;
                    FindFirstObjectByType<PlayerHealth>().ResetHealthForTesting(10000);
                    if (upgrades.IsSelectionActive) upgrades.SelectUpgrade(upgrades.GetAvailableChoices()[0]);
                    foreach (var enemy in waves.ActiveEnemies.ToArray()) enemy.TakeDamage(999999);
                    foreach (var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) if (!pickup.IsCollected) pickup.TryCollect(xp);
                    if (!completion.IsCompletionFinished) { Check(!completion.OffersSecondBlacksmithIntro,"no premature interception"); return; }
                    Check(xp.Level == 11 && xp.CurrentXP == 52 && xp.TotalXPEarned == 11385 && !upgrades.IsSelectionActive,"XP/upgrades resolved before transition");
                    Check(completion.OffersSecondBlacksmithIntro && PermanentProgression.GetAvailablePoints(hero.Id)==3,"completed D10 offers reward");
                    var button = FindFirstObjectByType<DungeonCompleteUI>().ReturnToMapButton;
                    Check(!button.GetComponentInChildren<TMP_Text>().text.Contains("DEMİRCİ"),"return-map label preserved");
                    stage=1; button.onClick.Invoke();
                }
                else if (stage == 1 && SceneManager.GetActiveScene().name == "Hub_Armory")
                {
                    var intro = FindFirstObjectByType<BlacksmithIntroSequence>();
                    if (intro == null || !intro.IsRunning) return;
                    for (int i=0;i<4;i++) Advance(intro);
                    var catalog=hero.WeaponCatalog; var reward=catalog.Find("archer_tier_2");
                    Check(PermanentProgression.GetEquippedWeapon(hero.Id,catalog)==reward && PermanentProgression.IsWeaponClaimed(hero.Id,"archer_tier_1"),"Tier II equipped and missed Tier I recovered");
                    Check(intro.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains(reward.DisplayName) && t.text.Contains("Tier II")),"named Tier II reveal");
                    Advance(intro); Check(!intro.IsRunning,"normal Armory resumes");
                    RunProgressionSession.EndRun(); Reload();
                    Check(!PermanentProgression.NeedsSecondBlacksmithIntro && !PermanentProgression.NeedsBlacksmithIntro && !PermanentProgression.CompleteBlacksmithIntro(hero.Id,reward),"persistent one-time reward");
                    var save=JsonUtility.FromJson<PermanentProgression.PermanentSaveData>(PlayerPrefs.GetString(PermanentProgression.PrefsKey));
                    Check(save.characters.Single(c=>c.characterId==hero.Id).claimedWeaponIds.Count==2,"no duplicate ownership");
                    foreach (var id in new[]{"warrior","gunner"})
                    {
                        var weapon=catalog.Find(id+"_tier_2");
                        Check(PermanentProgression.TryClaimWeapon(id,weapon) && PermanentProgression.TryEquipWeapon(id,weapon),"other hero affinity claim/equip");
                    }
                    stage=2; SceneManager.LoadScene("Hub_Armory");
                }
                else if (stage == 2 && SceneManager.GetActiveScene().name == "Hub_Armory")
                {
                    if (FindFirstObjectByType<ArmoryHubUI>() == null) return;
                    // Wait a frame for the reloaded Hub's Start method.
                    stage=3;
                }
                else if (stage == 3)
                {
                    Check(FindFirstObjectByType<BlacksmithIntroSequence>()==null && PermanentProgression.GetEquippedWeapon(hero.Id,hero.WeaponCatalog)?.Tier==2,"Hub reload does not replay or erase equipment");
                    Finish(true);
                }
            }
            catch (Exception ex) { Fail(ex); }
        }
        private static void Advance(BlacksmithIntroSequence intro)
        {
            typeof(BlacksmithIntroSequence).GetField("readyAt",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(intro,0f);
            typeof(BlacksmithIntroSequence).GetMethod("Advance",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(intro,null);
        }
        private static void Reload() => typeof(PermanentProgression).GetMethod("ResetStaticState",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,null);
        private static void Check(bool value,string message) { if(!value) throw new InvalidOperationException(message); }
        private void Fail(Exception ex) { Debug.LogException(ex); Finish(false); }
        private void Finish(bool success)
        {
            finished=true; snapshot?.Dispose();
            Debug.Log(success ? "[D10 BLACKSMITH PASSED]" : "[D10 BLACKSMITH FAILED]");
            EditorApplication.Exit(success?0:1);
        }
    }
}
#endif
