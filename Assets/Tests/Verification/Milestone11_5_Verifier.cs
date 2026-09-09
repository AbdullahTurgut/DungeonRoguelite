#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    // Loads real scenes and unchanged production assets. Damage is driven by the harness;
    // weapon mechanics are covered by the focused archetype and weapon regressions.
    public sealed class Milestone11_5_Verifier : MonoBehaviour
    {
        private Phase11StateSnapshot snapshot;
        private int checks, errors;
        private bool finished;
        private float deadline;
        private readonly Dictionary<string, string> assetText = new Dictionary<string, string>();
        private void Awake() { DontDestroyOnLoad(gameObject); }
        private IEnumerator Start()
        {
            snapshot = new Phase11StateSnapshot();
            deadline = Time.realtimeSinceStartup + 180;
            Application.logMessageReceived += Log;
            var stack = new Stack<IEnumerator>();
            stack.Push(Verify());
            bool success = true;
            while (stack.Count > 0 && !finished)
            {
                object current = null;
                try
                {
                    if (!stack.Peek().MoveNext()) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
                    current = stack.Peek().Current;
                    if (current is IEnumerator nested) { stack.Push(nested); continue; }
                }
                catch (Exception e) { Debug.LogException(e); success = false; break; }
                yield return current;
            }
            while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
            Finish(success);
        }
        private void Update()
        {
            if (!finished && snapshot != null && Time.realtimeSinceStartup > deadline)
            { Debug.LogError("[GATE 11.5 TIMEOUT]"); Finish(false); }
        }
        private void Log(string message, string trace, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
        private void Finish(bool success)
        {
            if (finished) return;
            finished = true;
            try
            {
                foreach (var manager in FindObjectsByType<WaveManager>(FindObjectsSortMode.None)) manager.HaltDungeon();
                foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                    if (root != gameObject) DestroyImmediate(root);
                foreach (var obj in owned) if (obj != null) DestroyImmediate(obj);
                owned.Clear();
            }
            catch (Exception e) { Debug.LogException(e); success = false; }
            finally { snapshot?.Dispose(); }
            Application.logMessageReceived -= Log;
            success &= errors == 0;
            Debug.Log($"[GATE 11.5 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.isPlaying = false;
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; snapshot?.Dispose(); }
        private void Check(bool value, string description)
        {
            if (!value) throw new InvalidOperationException(description);
            Debug.Log($"[GATE 11.5 CHECK {++checks} PASSED] {description}");
        }
        private IEnumerator Wait(Func<bool> ready, float seconds = 5)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!ready()) { if (Time.realtimeSinceStartup > end) throw new TimeoutException("Scene/gameplay wait"); yield return null; }
        }
        private IEnumerator Load(DungeonDefinition dungeon)
        {
            Time.timeScale = 1;
            DungeonRunSession.SetSelection(dungeon);
            var loading = SceneManager.LoadSceneAsync(dungeon.SceneName);
            yield return Wait(() => loading.isDone, 15);
            yield return null;
            Check(FindFirstObjectByType<WaveManager>().ActiveDungeon == dungeon, dungeon.Id + " production scene configuration");
        }
        private void ResolveChoices(UpgradeManager upgrades)
        {
            int guard = 0;
            while (upgrades.IsSelectionActive)
            {
                Check(Time.timeScale == 0 && ++guard < 30, "Upgrade selection pauses gameplay");
                upgrades.SelectUpgrade(upgrades.AvailableUpgrades[0]);
            }
        }
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private T Own<T>(T obj) where T : UnityEngine.Object { owned.Add(obj); return obj; }
        private void VerifyContent(DungeonDefinition[] dungeons)
        {
            string[] names = { "Zombie", "Runner", "Tank", "Ranged" };
            int[][] counts = {
                new[]{10,0,0,0}, new[]{10,3,0,0}, new[]{7,7,1,0},
                new[]{5,2,1,0}, new[]{5,2,0,1}, new[]{4,4,0,1}, new[]{4,3,1,2},
                new[]{5,4,1,0}, new[]{5,4,0,2}, new[]{4,3,1,2}, new[]{5,3,1,2}
            };
            int[] waveXP = {100,130,180,110,85,95,140,130,120,140,150};
            string[] descriptions = {
                "Zombiler ve hızlı koşucular. Son dalgada ağır bir düşman. 3 dalga hayatta kal.",
                "Ağır düşmanlar ve menzilli tehditler. 4 dalga hayatta kal.",
                "Dört düşman türüne karşı savaş. 4 dalga hayatta kal."
            };
            int index = 0;
            for (int d = 0; d < 3; d++)
            {
                Check(dungeons[d].Waves.Length == (d == 0 ? 3 : 4) && dungeons[d].Description == descriptions[d], "Dungeon wave count and approved description");
                foreach (var wave in dungeons[d].Waves)
                {
                    var expected = counts[index];
                    var order = (d == 0 ? new[]{0,2,1} : new[]{2,3,0,1}).Where(i => expected[i] > 0).ToArray();
                    Check(wave.EnemyEntries.Count == order.Length && wave.EnemyEntries.Select(e => e.EnemyPrefab != null ? e.EnemyPrefab.name : "null").SequenceEqual(order.Select(i => names[i])), "Exact production entry ordering wave " + index);
                    for (int e = 0; e < order.Length; e++)
                    {
                        var entry = wave.EnemyEntries[e];
                        Check(entry.Count == expected[order[e]] && entry.EnemyPrefab == AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Enemies/{names[order[e]]}.prefab"), "Exact count and canonical prefab reference");
                    }
                    Check(wave.TotalEnemyCount == expected.Sum() && wave.EnemyEntries.Sum(e => e.Count * e.EnemyPrefab.GetComponent<ExperienceReward>().XPAmount) == waveXP[index], "Exact wave enemies and XP " + index++);
                }
                Check(dungeons[d].Waves.Sum(w => w.TotalEnemyCount) == new[]{38,35,42}[d] && dungeons[d].Waves.Sum(w => w.EnemyEntries.Sum(e => e.Count * e.EnemyPrefab.GetComponent<ExperienceReward>().XPAmount)) == new[]{410,430,540}[d], "Exact dungeon enemies and XP");
            }
            Check(index == 11 && waveXP.Sum() == 1380, "11 waves / 1380 campaign XP");
            string source = System.IO.File.ReadAllText("Assets/Scripts/Waves/WaveManager.cs");
            Check(names.All(n => !source.Contains(n)), "WaveManager has no archetype-name branching");
        }
        private IEnumerator FinalSurvivors(DungeonDefinition[] dungeons)
        {
            var prefabs = new[]{"Zombie","Runner","Tank","Ranged"}.Select(n => AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Enemies/{n}.prefab")).ToArray();
            foreach (var dungeon in dungeons)
            foreach (var lastPrefab in prefabs)
            {
                RunProgressionSession.StartNewRun("warrior");
                var player = Own(new GameObject("FinalSurvivorPlayer"));
                player.transform.position = Vector3.forward * 100;
                var xp = player.AddComponent<PlayerExperience>();
                var health = player.AddComponent<PlayerHealth>();
                var stats = player.AddComponent<PlayerStats>();
                var upgrades = Own(new GameObject("FinalSurvivorUpgrades")).AddComponent<UpgradeManager>();
                var upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>("Assets/ScriptableObjects/Upgrades/Upgrade_Damage.asset");
                upgrades.SetAvailableUpgrades(new[]{upgrade});
                upgrades.BindPlayer(xp, stats);
                var wave = Own(ScriptableObject.CreateInstance<WaveDefinition>());
                wave.Initialize(prefabs.Select(p => new EnemySpawnEntry(p,1)).ToArray(), 0.01f);
                var manager = Own(new GameObject("FinalSurvivorWaves")).AddComponent<WaveManager>();
                manager.ConfigureFromDungeon(dungeon);
                manager.SetWaves(new[]{wave});
                manager.SetSpawnPoints(new[]{manager.transform});
                manager.BindPlayer(player.transform);
                var runStats = Own(new GameObject("FinalSurvivorStats")).AddComponent<DungeonRunStats>();
                runStats.SetReferences(manager, xp);
                var root = Own(new GameObject("FinalSurvivorCompletion"));
                root.SetActive(false);
                var completion = root.AddComponent<DungeonCompletionController>();
                completion.SetReferences(manager, upgrades, runStats, xp);
                root.SetActive(true);
                int kills = 0, victories = 0;
                Action<EnemyHealth> onKill = _ => kills++;
                Action<DungeonRunSummary> onVictory = _ => victories++;
                manager.OnEnemyDefeated += onKill;
                completion.OnDungeonCompleted += onVictory;
                Time.timeScale = 1;
                manager.BeginDungeon();
                yield return Wait(() => !manager.IsSpawning);
                var enemies = manager.ActiveEnemies.OrderBy(e => e.name == lastPrefab.name + "(Clone)" ? 1 : 0).ToArray();
                Check(enemies.Length == 4, "All four archetypes coexist in final-survivor fixture");
                foreach (var enemy in enemies)
                {
                    Own(enemy.gameObject);
                    var source = prefabs.First(p => p.name + "(Clone)" == enemy.name);
                    float damage = enemy.GetComponent<EnemyAttack>() != null ? enemy.GetComponent<EnemyAttack>().Damage : enemy.GetComponent<EnemyRangedAttack>().Damage;
                    float baseDamage = source.GetComponent<EnemyAttack>() != null ? source.GetComponent<EnemyAttack>().Damage : source.GetComponent<EnemyRangedAttack>().Damage;
                    Check(enemy.MaxHealth == Mathf.Round(source.GetComponent<EnemyHealth>().MaxHealth * dungeon.EnemyHealthMultiplier) && damage == Mathf.Round(baseDamage * dungeon.EnemyDamageMultiplier), dungeon.Id + " " + source.name + " production spawn scaling");
                }
                for (int i = 0; i < 3; i++) { enemies[i].TakeDamage(10000); enemies[i].TakeDamage(10000); }
                var last = enemies[3];
                Check(manager.LivingEnemyCount == 1 && kills == 3 && !completion.HasCompleted && last.name == lastPrefab.name + "(Clone)", lastPrefab.name + " holds completion as final survivor");
                foreach (var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) pickup.TryCollect(xp);
                int reward = last.GetComponent<ExperienceReward>().XPAmount;
                xp.RestoreState(1, 100 - reward, 100 - reward);
                EnemyProjectile[] shots = Array.Empty<EnemyProjectile>();
                if (last.TryGetComponent<EnemyRangedAttack>(out var ranged))
                {
                    player.transform.position = last.transform.position + Vector3.forward * 8;
                    Check(ranged.TryAttack(), "Final ranged survivor fires through production attack API");
                    shots = FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
                }
                last.TakeDamage(10000);
                last.TakeDamage(10000);
                Check(manager.LivingEnemyCount == 0 && kills == 4 && last != null && completion.HasCompleted, "Corpse does not delay wave/dungeon completion; death counted once");
                Check(xp.TotalXPEarned == 100 && xp.Level == 2 && upgrades.IsSelectionActive && completion.IsWaitingForUpgrades && !completion.IsCompletionFinished && victories == 0 && RunProgressionSession.TotalXP == 0, "Final XP exactly once precedes upgrade and victory commit");
                Check(shots.All(s => s.IsResolved), "Shooter death immediately resolves every outstanding shot");
                ResolveChoices(upgrades);
                completion.HandleWaveManagerCompletion();
                Check(completion.IsCompletionFinished && victories == 1 && RunProgressionSession.TotalXP == 100 && RunProgressionSession.CommittedUpgradeIds.Count == 1, "Upgrade precedes exactly-once victory commit");
                yield return new WaitForSecondsRealtime(1.7f);
                Check(last == null && FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None).Length == 0 && xp.TotalXPEarned == 100 && health.CurrentHealth == health.MaxHealth, "Paused corpse cleanup; no lingering damage or duplicate rewards");
                manager.OnEnemyDefeated -= onKill;
                completion.OnDungeonCompleted -= onVictory;
                for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) DestroyImmediate(owned[i]);
                owned.Clear();
                Time.timeScale = 1;
            }
        }
        private IEnumerator Verify()
        {
            foreach (string directory in new[] { "Assets/ScriptableObjects/Waves", "Assets/ScriptableObjects/Dungeons", "Assets/Prefabs/Enemies", "Assets/Scenes/Dungeons" })
                foreach (string file in System.IO.Directory.GetFiles(directory)) assetText[file] = System.IO.File.ReadAllText(file);
            var dungeons = Enumerable.Range(1, 3).Select(i => AssetDatabase.LoadAssetAtPath<DungeonDefinition>($"Assets/ScriptableObjects/Dungeons/Dungeon_0{i}.asset")).ToArray();
            Check(dungeons.All(d => d != null), "Three production dungeon definitions");
            VerifyContent(dungeons);
            yield return FinalSurvivors(dungeons);
            Check(dungeons.SelectMany(d => d.Waves).SelectMany(w => w.EnemyEntries).Select(e => e.EnemyPrefab.name).Distinct().Count() == 4, "Campaign includes all four enemy archetypes");
            foreach (string hero in new[] { "Warrior", "Archer", "Gunner" })
            {
                var character = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"Assets/ScriptableObjects/Characters/Character_{hero}.asset");
                CharacterSelectionSession.SetSelection(character);
                RunProgressionSession.StartNewRun(character.Id);
                DungeonProgression.ResetProgression();
                int cumulativeXP = 0;
                foreach (var dungeon in dungeons)
                {
                    yield return Load(dungeon);
                    var player = FindFirstObjectByType<PlayerSpawner>().ActiveCharacter;
                    var xp = player.GetComponent<PlayerExperience>();
                    var health = player.GetComponent<PlayerHealth>();
                    var upgrades = FindFirstObjectByType<UpgradeManager>();
                    var manager = FindFirstObjectByType<WaveManager>();
                    var completion = FindFirstObjectByType<DungeonCompletionController>();
                    Check(xp.TotalXPEarned == cumulativeXP && health.CurrentHealth == health.MaxHealth && upgrades.CollectedUpgradeIds.SequenceEqual(RunProgressionSession.CommittedUpgradeIds), hero + " entry restores progression and full health");
                    if (dungeon == dungeons[2]) Check(xp.Level == 5 && xp.CurrentXP == 27 && xp.XPToNextLevel == 506, "D3 entry Level 5 pacing");
                    int victories = 0, kills = 0, waves = 0;
                    completion.OnDungeonCompleted += _ => victories++;
                    manager.OnEnemyDefeated += _ => kills++;
                    manager.OnWaveCompleted += (_, __) => waves++;
                    // Wait for full mixed groups before killing: exercises coexistence and living counts.
                    // Keep the target out of combat; this suite checks orchestration, not survivability.
                    player.GetComponent<PlayerMovement>().enabled = false;
                    player.GetComponent<CharacterController>().enabled = false;
                    player.transform.position = new Vector3(0, 0, 100);
                    for (int w = 0; w < dungeon.Waves.Length; w++)
                    {
                        Time.timeScale = 20;
                        int waveIndex = w;
                        yield return Wait(() => manager.CurrentWaveIndex == waveIndex && !manager.IsSpawning && manager.CurrentState == WaveState.WaveActive, 15);
                        var enemies = manager.ActiveEnemies.ToArray();
                        Check(manager.CurrentWaveSpawned.Select(e => e.name.Replace("(Clone)", "")).SequenceEqual(dungeon.Waves[w].EnemyEntries.SelectMany(e => Enumerable.Repeat(e.EnemyPrefab.name, e.Count))), "Production spawn order matches entries");
                        var expected = dungeon.Waves[w].EnemyEntries.SelectMany(e => Enumerable.Repeat(e.EnemyPrefab.name, e.Count)).OrderBy(n => n);
                        Check(enemies.Select(e => e.name.Replace("(Clone)", "")).OrderBy(n => n).SequenceEqual(expected), hero + " " + dungeon.Id + " wave " + (w + 1) + " exact mixed roster");
                        foreach (var enemy in enemies)
                        {
                            var source = dungeon.Waves[w].EnemyEntries.First(e => e.EnemyPrefab.name + "(Clone)" == enemy.name).EnemyPrefab;
                            float baseDamage = source.GetComponent<EnemyAttack>() != null ? source.GetComponent<EnemyAttack>().Damage : source.GetComponent<EnemyRangedAttack>().Damage;
                            float damage = enemy.GetComponent<EnemyAttack>() != null ? enemy.GetComponent<EnemyAttack>().Damage : enemy.GetComponent<EnemyRangedAttack>().Damage;
                            Check(enemy.MaxHealth == Mathf.Round(source.GetComponent<EnemyHealth>().MaxHealth * dungeon.EnemyHealthMultiplier) && damage == Mathf.Round(baseDamage * dungeon.EnemyDamageMultiplier), "Mixed instance HP/damage scaling");
                        }
                        Time.timeScale = 1;
                        for (int i = 0; i < enemies.Length; i++)
                        {
                            enemies[i].TakeDamage(100000);
                            enemies[i].TakeDamage(100000);
                            Check(manager.LivingEnemyCount == enemies.Length - i - 1, "Exactly-once death removes living count");
                        }
                        // Drive pickup collection through its public API before the next timed wave.
                        // Otherwise accelerated homing can open a choice while this harness waits for spawning.
                        foreach (var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None))
                        {
                            pickup.TryCollect(xp);
                            Check(!pickup.TryCollect(xp), "Pickup rejects duplicate reward collection");
                        }
                        ResolveChoices(upgrades);
                    }
                    yield return Wait(() => completion.IsCompletionFinished);
                    cumulativeXP += dungeon.Waves.Sum(w => w.EnemyEntries.Sum(e => e.Count * e.EnemyPrefab.GetComponent<ExperienceReward>().XPAmount));
                    Check(victories == 1 && kills == dungeon.Waves.Sum(w => w.TotalEnemyCount) && waves == dungeon.Waves.Length, hero + " all waves and victory exactly once");
                    Check(xp.TotalXPEarned == cumulativeXP && RunProgressionSession.TotalXP == cumulativeXP && completion.FinalSummary.EnemiesDefeated == kills, "Exact mixed XP, summary and victory commit");
                    int di = Array.IndexOf(dungeons, dungeon);
                    Check(xp.Level == new[] {3,5,6}[di] && xp.CurrentXP == new[] {160,27,61}[di] && xp.XPToNextLevel == new[] {225,506,759}[di], "Exact campaign level and XP threshold");
                    Check(health.CurrentHealth == health.MaxHealth, "No unexpected damage in isolated campaign path");
                    Check(upgrades.CollectedUpgradeIds.SequenceEqual(RunProgressionSession.CommittedUpgradeIds) && DungeonProgression.IsDungeonCompleted(dungeon.Id), "Upgrades committed and dungeon progression persisted");
                    Check(di == 2 || DungeonProgression.IsDungeonUnlocked(dungeons[di + 1]), "Victory unlocks next dungeon");
                    yield return new WaitForSecondsRealtime(1.7f);
                    Check(FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length == 0 && FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None).Length == 0, "Paused victory clears corpses and enemy projectiles");
                    completion.ReturnToWorldMap();
                    yield return Wait(() => SceneManager.GetActiveScene().name == "WorldMap");
                    Check(RunProgressionSession.HasActiveRun && RunProgressionSession.TotalXP == cumulativeXP, "Victory map return preserves run");
                }
                yield return Load(dungeons[2]);
                var entryXP = RunProgressionSession.TotalXP;
                var entryLevel = RunProgressionSession.Level;
                var entryUpgrades = RunProgressionSession.CommittedUpgradeIds.ToArray();
                var failedXP = FindFirstObjectByType<PlayerExperience>();
                failedXP.GainExperience(1000);
                ResolveChoices(FindFirstObjectByType<UpgradeManager>());
                var defeat = FindFirstObjectByType<PlayerDefeatController>();
                var oldPlayer = defeat.BoundPlayerHealth;
                oldPlayer.TakeDamage(100000);
                Check(defeat.IsDefeated && Time.timeScale == 0 && !FindFirstObjectByType<DungeonCompletionController>().HasCompleted, "Defeat suppresses victory");
                defeat.RestartDungeon();
                yield return Wait(() => oldPlayer == null && FindFirstObjectByType<PlayerDefeatController>() != null);
                yield return null;
                Check(FindFirstObjectByType<PlayerExperience>().TotalXPEarned == entryXP && FindFirstObjectByType<PlayerExperience>().Level == entryLevel && FindFirstObjectByType<UpgradeManager>().CollectedUpgradeIds.SequenceEqual(entryUpgrades), "Real retry reload discards failed XP and upgrades");
                defeat = FindFirstObjectByType<PlayerDefeatController>();
                Check(defeat.BoundPlayerHealth.CurrentHealth == defeat.BoundPlayerHealth.MaxHealth, "Retry restores full health");
                defeat.BoundPlayerHealth.TakeDamage(100000);
                defeat.ReturnToWorldMap();
                yield return Wait(() => SceneManager.GetActiveScene().name == "WorldMap");
                Check(!RunProgressionSession.HasActiveRun && RunProgressionSession.Level == 1 && RunProgressionSession.TotalXP == 0 && RunProgressionSession.CommittedUpgradeIds.Count == 0 && DungeonProgression.IsDungeonCompleted(dungeons[2].Id), "Defeat map return resets run and preserves permanent dungeon progress");
            }
            foreach (var pair in assetText) Check(System.IO.File.ReadAllText(pair.Key) == pair.Value, "Asset unchanged: " + pair.Key);
        }
    }
}
#endif
