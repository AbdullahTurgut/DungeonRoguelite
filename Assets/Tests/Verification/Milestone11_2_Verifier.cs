#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Weapons;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Upgrades;

namespace DungeonRoguelite.Tests
{
    public sealed class Milestone11_2_Verifier : MonoBehaviour
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private Phase11StateSnapshot snapshot;
        private int checks;
        private int errors;
        private bool finished;
        private float deadline;
        private GameObject prefab;

        private IEnumerator Start()
        {
            snapshot = new Phase11StateSnapshot();
            deadline = Time.realtimeSinceStartup + 90f;
            Application.logMessageReceived += Log;
            Time.timeScale = 1f;
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
                catch (Exception exception) { Debug.LogException(exception); success = false; break; }
                yield return current;
            }
            while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
            Finish(success);
        }
        private void Update()
        {
            if (!finished && snapshot != null && Time.realtimeSinceStartup > deadline)
            {
                Debug.LogError("[GATE 11.2 TIMEOUT]");
                Finish(false);
            }
        }
        private void Log(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
        }
        private void Finish(bool success)
        {
            if (finished) return;
            finished = true;
            try { Cleanup(); snapshot?.Dispose(); }
            catch (Exception exception) { Debug.LogException(exception); success = false; }
            Application.logMessageReceived -= Log;
            success &= errors == 0;
            Debug.Log($"[GATE 11.2 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.isPlaying = false;
        }
        private void OnDestroy()
        {
            Application.logMessageReceived -= Log;
            snapshot?.Dispose();
        }
        private T Own<T>(T obj) where T : UnityEngine.Object { owned.Add(obj); return obj; }
        private void Check(bool value, string description)
        {
            if (!value) throw new InvalidOperationException(description);
            Debug.Log($"[GATE 11.2 CHECK {++checks} PASSED] {description}");
        }
        private void Cleanup()
        {
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) DestroyImmediate(owned[i]);
            owned.Clear();
            foreach (var e in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) DestroyImmediate(e.gameObject);
            foreach (var p in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) DestroyImmediate(p.gameObject);
            foreach (var p in FindObjectsByType<ArrowProjectile>(FindObjectsSortMode.None)) DestroyImmediate(p.gameObject);
            Time.timeScale = 1f;
        }
        private IEnumerator Wait(Func<bool> predicate, float seconds = 3f)
        {
            float until = Time.realtimeSinceStartup + seconds;
            while (!predicate())
            {
                if (Time.realtimeSinceStartup > until) throw new TimeoutException("Bounded gameplay wait");
                yield return null;
            }
        }
        private WaveManager Wave(Transform target, float hp = 1f, float damage = 1f)
        {
            var definition = Own(ScriptableObject.CreateInstance<WaveDefinition>());
            definition.Initialize(new[] { new EnemySpawnEntry(prefab, 1) }, 0.02f);
            var dungeon = Own(ScriptableObject.CreateInstance<DungeonDefinition>());
            dungeon.SetConfiguration("runner_test", "Test", "", "", "", new[] { definition }, hp, damage);
            var manager = Own(new GameObject("Waves")).AddComponent<WaveManager>();
            manager.ConfigureFromDungeon(dungeon);
            manager.SetSpawnPoints(new[] { manager.transform });
            manager.BindPlayer(target);
            manager.BeginDungeon();
            return manager;
        }
        private EnemyHealth First(WaveManager manager)
        {
            foreach (var enemy in manager.ActiveEnemies) { Own(enemy.gameObject); return enemy; }
            throw new InvalidOperationException("No production spawn");
        }
        private IEnumerator Verify()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-gateInjectFailure") >= 0)
            {
                Time.timeScale = 0f;
                RunProgressionSession.StartNewRun("injected");
                DungeonProgression.RecordDungeonCompleted("injected");
                Own(new GameObject("FailureCleanupProbe"));
                Check(false, "Intentional cleanup assertion failure");
            }
            if (Array.IndexOf(args, "-gateInjectTimeout") >= 0)
            {
                Time.timeScale = 0f;
                deadline = Time.realtimeSinceStartup + 0.2f;
                yield return new WaitForSecondsRealtime(10f);
            }
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Runner.prefab");
            Check(prefab != null && prefab.GetComponent<EnemyHealth>() && prefab.GetComponent<EnemyMovement>() &&
                prefab.GetComponent<IEnemyAttack>() is EnemyAttack && prefab.GetComponent<ExperienceReward>() &&
                prefab.GetComponent<EnemyVisualFeedback>() && prefab.GetComponent<CorpseCleanup>(), "Runner shared composition");
            var attack = prefab.GetComponent<EnemyAttack>();
            var movement = prefab.GetComponent<EnemyMovement>();
            Check(prefab.GetComponent<EnemyHealth>().MaxHealth == 25 && attack.Damage == 6 && attack.AttackRange == 1.3f &&
                attack.AttackCooldown == 0.65f && movement.MoveSpeed == 5.5f && movement.StoppingDistance == 1.1f &&
                prefab.GetComponent<ExperienceReward>().XPAmount == 10, "Exact base configuration");
            var cc = prefab.GetComponent<CharacterController>();
            Check(cc.height == 1.7f && cc.radius == 0.425f && cc.center == Vector3.up * 0.85f &&
                prefab.GetComponentsInChildren<Collider>().Length == 1, "Controller geometry and no child colliders");
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Runner.mat");
            Check(prefab.transform.Find("Visual").localScale == Vector3.one * 0.85f && material != null &&
                material.GetColor("_BaseColor") == new Color(1f, 0.45f, 0.05f) &&
                Array.TrueForAll(prefab.GetComponentsInChildren<Renderer>(), r => r.sharedMaterial == material), "Orange material and 0.85 silhouette configuration");
            for (int d = 1; d <= 3; d++)
            {
                var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>($"Assets/ScriptableObjects/Dungeons/Dungeon_0{d}.asset");
                var target = Own(new GameObject("ExplicitTarget"));
                target.transform.position = Vector3.forward * 20f;
                var playerHealth = target.AddComponent<PlayerHealth>();
                var manager = Wave(target.transform, dungeon.EnemyHealthMultiplier, dungeon.EnemyDamageMultiplier);
                var enemy = First(manager);
                var a = enemy.GetComponent<EnemyAttack>();
                var m = enemy.GetComponent<EnemyMovement>();
                Check(enemy.MaxHealth == Mathf.Round(25 * dungeon.EnemyHealthMultiplier) && a.Damage == Mathf.Round(6 * dungeon.EnemyDamageMultiplier),
                    $"D{d} actual scaling: {enemy.MaxHealth} HP / {a.Damage} damage");
                Check(a.Target == target.transform && m.Target == target.transform, "Generic wave interface explicit binding");
                var before = enemy.transform.position;
                m.StepMovement(0.1f);
                Check(Mathf.Abs(enemy.transform.position.z - before.z - 0.55f) < 0.01f && m.IsMoving, "Real fast pursuit step");
                target.transform.position = enemy.transform.position + Vector3.forward;
                m.StepMovement(0.01f);
                Check(!m.IsMoving, "Stops within 1.1m");
                Time.timeScale = 0f;
                before = enemy.transform.position;
                m.StepMovement(0.1f);
                Check(enemy.transform.position == before && !a.TryAttack(), "Pause suppresses movement and melee");
                Time.timeScale = 1f;
                float hp = playerHealth.CurrentHealth;
                Check(a.TryAttack() && playerHealth.CurrentHealth == hp - a.Damage && !a.TryAttack(), "Real melee damage and immediate cooldown");
                m.enabled = false;
                a.enabled = false;
                yield return new WaitForSeconds(0.67f);
                Check(a.TryAttack(), "Melee resumes after configured cooldown");
                Cleanup();
            }
            yield return Weapons();
            yield return DeathAndCompletion();
            Check(prefab.GetComponent<EnemyHealth>().MaxHealth == 25 && attack.Damage == 6, "Prefab immutable after runtime scaling and combat");
            var zombie = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            Check(zombie.GetComponent<EnemyHealth>().MaxHealth == 50 && zombie.GetComponent<EnemyAttack>().Damage == 10 &&
                zombie.GetComponent<EnemyMovement>().MoveSpeed == 3 && zombie.GetComponent<ExperienceReward>().XPAmount == 10, "Zombie baseline unchanged");
        }
        private IEnumerator Weapons()
        {
            foreach (string hero in new[] { "Warrior", "Archer", "Gunner" })
            {
                var player = Own(Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Characters/{hero}.prefab")));
                foreach (var behaviour in player.GetComponents<MonoBehaviour>()) behaviour.enabled = false;
                var enemy = Own(Instantiate(prefab)).GetComponent<EnemyHealth>();
                enemy.transform.position = Vector3.forward * 1.8f;
                enemy.GetComponent<EnemyMovement>().enabled = false;
                enemy.GetComponent<EnemyAttack>().enabled = false;
                Physics.SyncTransforms();
                float expected = hero == "Warrior" ? 25f : hero == "Archer" ? 20f : 10f;
                Check(player.GetComponent<IPrimaryAttack>().TryAttack(), hero + " production weapon fires");
                yield return Wait(() => enemy.CurrentHealth < enemy.MaxHealth);
                Check(enemy.CurrentHealth == Mathf.Max(0, 25 - expected), hero + " actual damage against Runner");
                Cleanup();
            }
        }
        private IEnumerator DeathAndCompletion()
        {
            var target = Own(new GameObject("Player"));
            target.transform.position = Vector3.one * 100f;
            target.AddComponent<PlayerHealth>();
            var xp = target.AddComponent<PlayerExperience>();
            xp.RestoreState(1, 90, 90);
            var stats = target.AddComponent<PlayerStats>();
            var upgrades = Own(new GameObject("Upgrades")).AddComponent<UpgradeManager>();
            var upgrade = Own(ScriptableObject.CreateInstance<UpgradeDefinition>());
            upgrade.Initialize("test_damage", "Damage", "", UpgradeType.Damage, 0.2f);
            upgrades.SetAvailableUpgrades(new[] { upgrade });
            upgrades.BindPlayer(xp, stats);
            RunProgressionSession.StartNewRun("warrior");
            var manager = Wave(target.transform);
            var runStats = Own(new GameObject("Stats")).AddComponent<DungeonRunStats>();
            runStats.SetReferences(manager, xp);
            var completionObject = Own(new GameObject("Completion"));
            completionObject.SetActive(false);
            var completion = completionObject.AddComponent<DungeonCompletionController>();
            completion.SetReferences(manager, upgrades, runStats, xp);
            completionObject.SetActive(true);
            var enemy = First(manager);
            int deaths = 0;
            Action onDeath = () => deaths++;
            enemy.OnDied += onDeath;
            yield return Wait(() => !manager.IsSpawning);
            var renderer = enemy.GetComponentInChildren<Renderer>();
            var material = renderer.sharedMaterial;
            var color = material.GetColor("_BaseColor");
            enemy.TakeDamage(1);
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            Check(block.GetColor("_BaseColor") == Color.white, "Immediate hit flash");
            enemy.TakeDamage(1000);
            enemy.TakeDamage(1000);
            Check(enemy.IsDead && deaths == 1 && manager.LivingEnemyCount == 0 && enemy.gameObject.activeSelf,
                "Immediate authoritative death and living count exactly once with retained corpse");
            Check(Array.TrueForAll(enemy.GetComponentsInChildren<Collider>(), c => !c.enabled) &&
                !enemy.GetComponent<EnemyMovement>().enabled && !enemy.GetComponent<EnemyAttack>().TryAttack(), "Corpse collision and combat disabled");
            Check(enemy.GetComponent<ExperienceReward>().HasRewarded && xp.TotalXPEarned == 100 && xp.Level == 2 &&
                completion.IsWaitingForUpgrades && !completion.IsCompletionFinished, "Exactly one final XP reward precedes upgrade and completion");
            upgrades.SelectUpgrade(upgrade);
            Check(completion.IsCompletionFinished && RunProgressionSession.Level == 2 &&
                RunProgressionSession.CommittedUpgradeIds.Count == 1, "Upgrade precedes victory commit");
            yield return new WaitForSecondsRealtime(0.15f);
            renderer.GetPropertyBlock(block);
            Check(block.isEmpty && renderer.sharedMaterial == material && material.GetColor("_BaseColor") == color,
                "Paused flash restoration and shared material safety");
            enemy.OnDied -= onDeath;
            yield return new WaitForSecondsRealtime(0.9f);
            Check(enemy != null && Time.timeScale == 0, "Corpse remains before 1.5 unscaled seconds");
            yield return Wait(() => enemy == null);
            Check(manager.LivingEnemyCount == 0 && xp.TotalXPEarned == 100, "Paused corpse cleanup has no duplicate gameplay effects");
            Cleanup();
        }
    }
}
#endif
