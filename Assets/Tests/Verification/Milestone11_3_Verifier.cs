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
    public sealed class Milestone11_3_Verifier : MonoBehaviour
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
            deadline = Time.realtimeSinceStartup + 180f;
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
                Debug.LogError("[GATE 11.3 TIMEOUT]");
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
            Debug.Log($"[GATE 11.3 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
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
            Debug.Log($"[GATE 11.3 CHECK {++checks} PASSED] {description}");
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
        private WaveManager Wave(Transform target, float hp = 1f, float damage = 1f, bool twoWaves = false)
        {
            var definition = Own(ScriptableObject.CreateInstance<WaveDefinition>());
            definition.Initialize(new[] { new EnemySpawnEntry(prefab, 1) }, 0.02f);
            var dungeon = Own(ScriptableObject.CreateInstance<DungeonDefinition>());
            dungeon.SetConfiguration("tank_test", "Test", "", "", "", twoWaves ? new[] { definition, definition } : new[] { definition }, hp, damage);
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
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Tank.prefab");
            Check(prefab != null && prefab.GetComponent<EnemyHealth>() && prefab.GetComponent<EnemyMovement>() &&
                prefab.GetComponent<IEnemyAttack>() is EnemyAttack && prefab.GetComponent<ExperienceReward>() &&
                prefab.GetComponent<EnemyVisualFeedback>() && prefab.GetComponent<CorpseCleanup>(), "Tank shared composition");
            var attack = prefab.GetComponent<EnemyAttack>();
            var movement = prefab.GetComponent<EnemyMovement>();
            Check(prefab.GetComponent<EnemyHealth>().MaxHealth == 150 && attack.Damage == 22 && attack.AttackRange == 2f &&
                attack.AttackCooldown == 1.8f && movement.MoveSpeed == 1.8f && movement.StoppingDistance == 1.8f &&
                prefab.GetComponent<ExperienceReward>().XPAmount == 40, "Exact base configuration");
            var cc = prefab.GetComponent<CharacterController>();
            Check(cc.height == 2.8f && cc.radius == 0.7f && cc.center == Vector3.up * 1.4f &&
                prefab.GetComponentsInChildren<Collider>().Length == 1, "Controller geometry and no child colliders");
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Tank.mat");
            Check(prefab.transform.Find("Visual").localScale == Vector3.one * 1.4f && material != null &&
                material.GetColor("_BaseColor") == new Color(0.18f, 0.21f, 0.25f) &&
                Array.TrueForAll(prefab.GetComponentsInChildren<Renderer>(), r => r.sharedMaterial == material), "Charcoal material and 1.4 silhouette configuration");
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
                Check(enemy.MaxHealth == Mathf.Round(150 * dungeon.EnemyHealthMultiplier) && a.Damage == Mathf.Round(22 * dungeon.EnemyDamageMultiplier),
                    $"D{d} actual scaling: {enemy.MaxHealth} HP / {a.Damage} damage");
                Check(a.Target == target.transform && m.Target == target.transform, "Generic wave interface explicit binding");
                target.transform.position = enemy.transform.position + Vector3.forward * 2.01f;
                Check(!a.TryAttack(), "Outside 2m attack range is rejected");
                target.transform.position = Vector3.forward * 20f;
                var before = enemy.transform.position;
                m.StepMovement(0.1f);
                Check(Mathf.Abs(enemy.transform.position.z - before.z - 0.18f) < 0.01f && m.IsMoving, "Real slow pursuit step");
                target.transform.position = enemy.transform.position + Vector3.forward;
                m.StepMovement(0.01f);
                Check(!m.IsMoving, "Stops within 1.8m");
                Time.timeScale = 0f;
                before = enemy.transform.position;
                m.StepMovement(0.1f);
                Check(enemy.transform.position == before && !a.TryAttack(), "Pause suppresses movement and melee");
                Time.timeScale = 1f;
                float hp = playerHealth.CurrentHealth;
                Check(a.TryAttack() && playerHealth.CurrentHealth == hp - a.Damage && !a.TryAttack(), "Real melee damage and immediate cooldown");
                m.enabled = false;
                a.enabled = false;
                yield return new WaitForSeconds(1.82f);
                Check(a.TryAttack(), "Melee resumes after configured cooldown");
                Cleanup();
            }
            yield return Weapons();
            yield return WaveTransition();
            yield return DeathAndCompletion();
            Check(prefab.GetComponent<EnemyHealth>().MaxHealth == 150 && attack.Damage == 22, "Prefab immutable after runtime scaling and combat");
            var zombie = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            Check(zombie.GetComponent<EnemyHealth>().MaxHealth == 50 && zombie.GetComponent<EnemyAttack>().Damage == 10 &&
                zombie.GetComponent<EnemyMovement>().MoveSpeed == 3 && zombie.GetComponent<ExperienceReward>().XPAmount == 10, "Zombie baseline unchanged");
        }
        private IEnumerator WaveTransition()
        {
            var target = Own(new GameObject("DistantTarget"));
            target.transform.position = Vector3.one * 100f;
            var manager = Wave(target.transform, twoWaves: true);
            var first = First(manager);
            yield return Wait(() => !manager.IsSpawning);
            first.TakeDamage(1000f);
            Check(manager.LivingEnemyCount == 0 && first != null, "Nonfinal Tank death removes living enemy before corpse destruction");
            yield return Wait(() => manager.CurrentWaveIndex == 1);
            var second = First(manager);
            Check(first != null && second != null && manager.LivingEnemyCount == 1, "Second wave starts while first Tank corpse remains");
            var pickups = FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None);
            Check(pickups.Length == 1 && first.GetComponent<ExperienceReward>().XPAmount == 40,
                "Nonfinal Tank creates exactly one 40 XP reward");
            Cleanup();
        }
        private IEnumerator Weapons()
        {
            for (int dungeonIndex = 1; dungeonIndex <= 3; dungeonIndex++)
            foreach (string hero in new[] { "Warrior", "Archer", "Gunner" })
            for (int upgraded = 0; upgraded <= 1; upgraded++)
            {
                var floor = Own(GameObject.CreatePrimitive(PrimitiveType.Cube));
                floor.transform.position = Vector3.down * 0.5f;
                floor.transform.localScale = new Vector3(40, 1, 40);
                var player = Own(Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Characters/{hero}.prefab")));
                foreach (var behaviour in player.GetComponents<MonoBehaviour>()) behaviour.enabled = false;
                var stats = player.GetComponent<PlayerStats>();
                var xp = player.GetComponent<PlayerExperience>();
                xp.RestoreState(1, 0, 0);
                if (upgraded == 1)
                {
                    var upgrades = Own(new GameObject("WeaponUpgrades")).AddComponent<UpgradeManager>();
                    var definition = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>("Assets/ScriptableObjects/Upgrades/Upgrade_Damage.asset");
                    Check(definition != null, "Production damage upgrade asset exists");
                    upgrades.SetAvailableUpgrades(new[] { definition });
                    upgrades.BindPlayer(xp, stats);
                    xp.GainExperience(100);
                    upgrades.SelectUpgrade(definition);
                    Check(Mathf.Approximately(stats.DamageMultiplier, 1.2f), "Actual UpgradeManager applies production 20% damage upgrade");
                }
                var enemy = Own(Instantiate(prefab)).GetComponent<EnemyHealth>();
                var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>($"Assets/ScriptableObjects/Dungeons/Dungeon_0{dungeonIndex}.asset");
                enemy.InitializeHealth(dungeon.EnemyHealthMultiplier);
                enemy.transform.position = Vector3.forward * 1.8f;
                enemy.GetComponent<EnemyMovement>().SetTarget(enemy.transform);
                enemy.GetComponent<EnemyAttack>().enabled = false;
                var child = new GameObject("ExtraDamageCollider");
                child.transform.SetParent(enemy.transform, false);
                child.transform.localPosition = Vector3.up;
                child.AddComponent<BoxCollider>().size = Vector3.one * 0.3f;
                Physics.SyncTransforms();
                float expected = hero == "Warrior" ? 25f : hero == "Archer" ? 20f : 10f;
                expected *= upgraded == 1 ? 1.2f : 1f;
                int hits = 0;
                int expectedHits = Mathf.CeilToInt(enemy.MaxHealth / expected);
                while (!enemy.IsDead && hits <= expectedHits)
                {
                    float before = enemy.CurrentHealth;
                    // Extra collider tests deduplication on nonlethal hits; production Tank
                    // has only its controller, whose death shutdown is asserted below.
                    if (before <= expected && child != null) DestroyImmediate(child);
                    yield return Wait(() => player.GetComponent<IPrimaryAttack>().TryAttack());
                    yield return Wait(() => enemy.CurrentHealth < before);
                    Check(Mathf.Approximately(enemy.CurrentHealth, Mathf.Max(0, before - expected)),
                        $"D{dungeonIndex} {hero} upgrade={upgraded}: hit {++hits}, full damage {expected}, no duplicate collider damage or mitigation");
                }
                Check(enemy.IsDead && hits == expectedHits, $"D{dungeonIndex} {hero} upgrade={upgraded} kills in {hits} real hits");
                Check(Array.TrueForAll(enemy.GetComponentsInChildren<Collider>(), c => !c.enabled), "Production Tank corpse has no collision");
                Cleanup();
            }
        }
        private IEnumerator DeathAndCompletion()
        {
            var target = Own(new GameObject("Player"));
            target.transform.position = Vector3.one * 100f;
            target.AddComponent<PlayerHealth>();
            var xp = target.AddComponent<PlayerExperience>();
            xp.RestoreState(1, 60, 60);
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
