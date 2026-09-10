using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Tests;

namespace DungeonRoguelite.Editor
{
    public static class Phase14VerificationRunner
    {
        public static void Run()
        {
            string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-gateSuite"); string gate = index >= 0 && index + 1 < args.Length ? args[index + 1] : "14_1"; bool ok = false;
            try
            {
                if (gate == "14_1") VerifyFoundation(); else if (gate == "14_2") VerifyArena(); else if (gate == "14_3") VerifyBoss(); else if (gate == "14_4") VerifyEncounter(); else if (gate == "14_5") VerifyCampaign(); else throw new InvalidOperationException("Unknown Phase 14 gate " + gate);
                ok = true; Debug.Log("[GATE " + gate.Replace('_', '.') + " COMPLETE] All checks PASSED.");
            }
            catch (Exception ex) { Debug.LogException(ex); Debug.LogError("[GATE " + gate.Replace('_', '.') + " COMPLETE] Verification FAILED."); }
            finally { Debug.Log("[PHASE 14 STATE RESTORED]"); if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1); }
        }
        private static DungeonDefinition D5 => AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_05.asset");
        private static void VerifyFoundation()
        {
            var d5 = D5; var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>("Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset");
            Check(d5 != null && d5.Id == "dungeon_5" && d5.SceneName == "Dungeon_05" && d5.RequiredDungeonId == "dungeon_4" && d5.Type == DungeonType.Boss && Mathf.Approximately(d5.EnemyHealthMultiplier,1.4f) && Mathf.Approximately(d5.EnemyDamageMultiplier,1.3f), "D5 definition and Boss classification");
            Check(catalog.Dungeons.Select(x=>x.Id).SequenceEqual(new[]{"dungeon_1","dungeon_2","dungeon_3","dungeon_4","dungeon_5"}), "catalog appends D5 while preserving D1-D4");
            Check(EditorBuildSettings.scenes.Length == 7 && EditorBuildSettings.scenes[6].path.EndsWith("Dungeon_05.unity"), "Build Settings D5 route");
            EditorSceneManager.OpenScene(WorldMapCarouselSetup.ScenePath);
            var map = UnityEngine.Object.FindFirstObjectByType<DungeonRoguelite.UI.WorldMapController>();
            Check(map != null && map.DungeonCatalog == catalog && map.CardsContainer != null &&
                map.LeftNavigationButton != null && map.RightNavigationButton != null &&
                map.Cards.All(c => c != null && c.transform.parent == map.CardsContainer &&
                    c.GetComponent<RectTransform>().sizeDelta == new Vector2(440,450)),
                "World Map serialized carousel bindings and fixed card dimensions (runtime window verified separately)");
        }
        private static void VerifyArena()
        {
            EditorSceneManager.OpenScene(Phase14Setup.ScenePath); var root=GameObject.Find("AshenSanctumArena"); Check(root!=null,"Ashen Sanctum root"); var floor=root.transform.Find("Floor"); Check(floor!=null && floor.GetComponent<Renderer>()!=null && floor.GetComponent<Collider>()!=null && Near(floor.localScale,new Vector3(44,.5f,36)),"one authoritative 44m x 36m floor");
            Check(GameObject.Find("ColonnadeArena")==null,"no copied Colonnade root"); Check(root.transform.Find("Obelisk_West")!=null && root.transform.Find("Obelisk_East")!=null,"two intended obelisks");
            var player=GameObject.Find("PlayerSpawnPoint"); var boss=GameObject.Find("BossSpawnPoint"); Check(player!=null && Near(player.transform.position,new Vector3(0,0,-13)) && boss!=null && Near(boss.transform.position,new Vector3(0,0,11)),"player and boss spawns");
            var spawns=GameObject.Find("SpawnPoints"); Check(spawns!=null && Enumerable.Range(1,6).All(i=>spawns.transform.Find("SpawnPoint_0"+i)!=null),"six spawn points");
        }
        private static void VerifyBoss()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/AshWarden.prefab"); Check(prefab!=null && prefab.GetComponent<BossWardenController>()!=null,"boss prefab and dedicated controller"); var instance=UnityEngine.Object.Instantiate(prefab); try { var health=instance.GetComponent<EnemyHealth>(); health.InitializeHealth(1.4f); var boss=instance.GetComponent<BossWardenController>(); boss.InitializeAttack(1.3f); Check(health.MaxHealth==1680f,"boss D5 runtime health scaling"); Check(boss.StrikeDamage==26f && boss.SlamDamage==42f && boss.BoltDamage==21f,"boss D5 attack rounding"); Check(instance.GetComponent<ExperienceReward>().XPAmount==960,"boss XP reward"); } finally { UnityEngine.Object.DestroyImmediate(instance); }
            EditorSceneManager.OpenScene(Phase14Setup.ScenePath); var hud = UnityEngine.Object.FindFirstObjectByType<DungeonRoguelite.UI.BossHealthBarUI>(); Check(hud != null && hud.IsFillConfigured && hud.IsPhasePresentationHidden,"boss HUD presents a RectTransform health fill without a phase label");
        }
        private static void VerifyEncounter()
        {
            var d5=D5; Check(d5.Waves!=null && d5.Waves.Length==3,"two warmup waves and boss wave"); int enemies=0,xp=0; foreach(var wave in d5.Waves) foreach(var entry in wave.EnemyEntries) { enemies+=entry.Count; xp+=entry.Count*entry.EnemyPrefab.GetComponent<ExperienceReward>().XPAmount; }
            Check(enemies==19 && xp==1200,"exact D5 total: 19 enemies / 1200 XP");
            Check(d5.Waves[0].EnemyEntries.Select(e=>e.EnemyPrefab.name+":"+e.Count).SequenceEqual(new[]{"Zombie:3","Runner:2","Ranged:2"}),"Wave 1 composition");
            Check(d5.Waves[1].EnemyEntries.Select(e=>e.EnemyPrefab.name+":"+e.Count).SequenceEqual(new[]{"Zombie:3","Runner:3","Ranged:4","Tank:1"}),"Wave 2 composition");
            Check(d5.Waves[2].EnemyEntries.Count==1 && d5.Waves[2].EnemyEntries[0].EnemyPrefab.GetComponent<BossWardenController>()!=null,"boss final wave");
        }
        private static void VerifyCampaign()
        {
            using (var snapshot = new Phase12StateSnapshot())
            {
                var d5=D5; DungeonProgression.ResetProgression(); Check(!DungeonProgression.IsDungeonUnlocked(d5),"D5 locked before D4 completion"); DungeonProgression.RecordDungeonCompleted("dungeon_4"); Check(DungeonProgression.IsDungeonUnlocked(d5),"D4 completion unlocks D5 replay route");
                RunProgressionSession.StartNewRun("warrior"); RunProgressionSession.CommitDungeonVictory(7,7,2085,new[]{"damage"}); RunProgressionSession.CreateDungeonCheckpoint(); Check(RunProgressionSession.Level==7 && RunProgressionSession.CurrentXP==7 && RunProgressionSession.TotalXP==2085,"D5 entry checkpoint preserves D4 run state");
                var go=new GameObject("D5CampaignExperience"); var xp=go.AddComponent<PlayerExperience>(); xp.RestoreState(7,7,2085); xp.GainExperience(1200); Check(xp.Level==8 && xp.CurrentXP==68 && xp.XPToNextLevel==1709 && xp.TotalXPEarned==3285,"D5 1200 XP ends Level 8, 68 of 1709"); UnityEngine.Object.DestroyImmediate(go);
                Check(PermanentProgression.GetDungeonFirstClearPoints("dungeon_5")==0 && !PermanentProgression.TryAwardDungeonFirstClear("warrior","dungeon_5",out int award) && award==0,"D5 reward and replay remain zero points");
                RunProgressionSession.RestoreCheckpointOnRetry(); Check(RunProgressionSession.Level==7 && RunProgressionSession.CurrentXP==7 && RunProgressionSession.TotalXP==2085,"Retry restores D5 entry checkpoint"); RunProgressionSession.EndRun(); Check(!RunProgressionSession.HasActiveRun && RunProgressionSession.Level==1 && RunProgressionSession.TotalXP==0,"Return Map ends run");
            }
        }
        private static bool Near(Vector3 a,Vector3 b)=>Vector3.Distance(a,b)<.01f;
        private static void Check(bool value,string name){if(!value)throw new InvalidOperationException("[CHECK FAILED] "+name);Debug.Log("[CHECK PASSED] "+name);}
    }
}
