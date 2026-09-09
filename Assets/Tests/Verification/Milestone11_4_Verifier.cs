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
    public sealed class Milestone11_4_Verifier : MonoBehaviour
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
                Debug.LogError("[GATE 11.4 TIMEOUT]");
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
            Debug.Log($"[GATE 11.4 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
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
            Debug.Log($"[GATE 11.4 CHECK {++checks} PASSED] {description}");
        }
        private void Cleanup()
        {
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) DestroyImmediate(owned[i]);
            owned.Clear();
            foreach (var e in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) DestroyImmediate(e.gameObject);
            foreach (var p in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) DestroyImmediate(p.gameObject);
            foreach (var p in FindObjectsByType<ArrowProjectile>(FindObjectsSortMode.None)) DestroyImmediate(p.gameObject);
            foreach (var p in FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None)) DestroyImmediate(p.gameObject);
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
            dungeon.SetConfiguration("ranged_test", "Test", "", "", "", new[] { definition }, hp, damage);
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
        private EnemyProjectile shotPrefab;
        private IEnumerator Verify()
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Ranged.prefab");
            shotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EnemyProjectile.prefab").GetComponent<EnemyProjectile>();
            var a = prefab.GetComponent<EnemyRangedAttack>();
            Check(prefab.GetComponent<EnemyHealth>().MaxHealth == 35 && prefab.GetComponent<EnemyAttack>() == null &&
                ReferenceEquals(prefab.GetComponent<IEnemyAttack>(), a) && prefab.GetComponent<EnemyMovement>() &&
                prefab.GetComponent<ExperienceReward>().XPAmount == 15 && prefab.GetComponent<EnemyVisualFeedback>() &&
                prefab.GetComponent<CorpseCleanup>() && prefab.GetComponentsInChildren<Collider>().Length == 1, "Ranged shared composition and 35 HP / 15 XP");
            Check(a.Damage == 8 && a.AttackRange == 9 && a.AttackCooldown == 1.6f && a.ProjectileSpeed == 10 &&
                a.ProjectileLifetime == 3 && a.ProjectilePrefab == shotPrefab, "Exact ranged attack and projectile configuration");
            Check(prefab.GetComponent<EnemyMovement>().MoveSpeed == 2.8f && prefab.GetComponent<EnemyMovement>().StoppingDistance == 7,
                "Movement speed 2.8, stopping distance 7");
            Check(shotPrefab.SweepRadius == 0.15f && shotPrefab.GetComponent<Rigidbody>() == null &&
                shotPrefab.GetComponentsInChildren<Collider>().Length == 0, "One swept projectile authority, no rigidbody/collider callbacks");
            Check(prefab.transform.Find("VioletOrb") != null && prefab.transform.Find("VioletOrb").localScale == Vector3.one * 0.3f &&
                prefab.transform.Find("Visual").GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor") == new Color(0.22f, 0.06f, 0.4f) &&
                shotPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor") == new Color(0.85f, 0.3f, 1f), "Deep violet body and bright violet marker/projectile configuration");
            yield return AttackAndScaling();
            foreach (string scenario in new[] { "player", "multiple-player", "owner", "owner-child", "Zombie", "Runner", "Tank", "Ranged",
                "wall", "thin-wall", "pillar-edge", "close", "zero-distance", "trigger", "non-player", "self" })
                yield return Collision(scenario);
            yield return Lifetime(false);
            yield return Lifetime(true);
            yield return ShooterDeath();
            yield return Completion();
            yield return PausedImpactAndWeapons();
            Check(prefab.GetComponent<EnemyHealth>().MaxHealth == 35 && a.Damage == 8, "Prefab stats immutable after tests");
        }
        private IEnumerator PausedImpactAndWeapons()
        {
            var owner = Shooter(Vector3.back);
            var playerHealth = PlayerAt(Vector3.forward * 0.5f);
            var shot = Shot(owner, Vector3.up, Vector3.forward);
            Time.timeScale = 0;
            Physics.SyncTransforms();
            yield return new WaitForSecondsRealtime(0.2f);
            Check(shot != null && shot.transform.position == Vector3.up && playerHealth.CurrentHealth == playerHealth.MaxHealth,
                "Paused close impact produces no travel or damage");
            yield return Wait(() => shot == null, 3.2f);
            Check(playerHealth.CurrentHealth == playerHealth.MaxHealth, "Paused shot expires without damaging overlapping player");
            Cleanup();
            playerHealth = PlayerAt(Vector3.forward * 20);
            playerHealth.tag = "Player";
            var fallback = Own(Instantiate(prefab));
            Check(fallback.GetComponent<EnemyRangedAttack>().Target == playerHealth.transform, "Existing Player tag fallback resolves when no explicit target supplied");
            Cleanup();
            foreach (string hero in new[] { "Warrior", "Archer", "Gunner" })
            {
                var player = Own(Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Characters/{hero}.prefab")));
                foreach (var behaviour in player.GetComponents<MonoBehaviour>()) behaviour.enabled = false;
                var enemy = Own(Instantiate(prefab)).GetComponent<EnemyHealth>();
                enemy.transform.position = Vector3.forward * 1.8f;
                enemy.GetComponent<EnemyMovement>().enabled = false;
                enemy.GetComponent<EnemyRangedAttack>().enabled = false;
                Physics.SyncTransforms();
                float expected = hero == "Warrior" ? 25 : hero == "Archer" ? 20 : 10;
                Check(player.GetComponent<IPrimaryAttack>().TryAttack(), hero + " production weapon fires at Ranged");
                yield return Wait(() => enemy.CurrentHealth < enemy.MaxHealth);
                Check(enemy.CurrentHealth == 35 - expected, hero + " applies actual weapon damage to Ranged");
                Cleanup();
            }
        }
        private PlayerHealth PlayerAt(Vector3 position)
        {
            var go = Own(new GameObject("ExplicitPlayer"));
            go.transform.position = position;
            var collider = go.AddComponent<CapsuleCollider>();
            collider.center = Vector3.up;
            collider.height = 2;
            collider.radius = 0.4f;
            return go.AddComponent<PlayerHealth>();
        }
        private EnemyProjectile Shot(EnemyHealth owner, Vector3 origin, Vector3 direction)
        {
            var p = Own(Instantiate(shotPrefab, origin, Quaternion.identity));
            p.Initialize(owner.transform, direction, 10, 8, 3);
            return p;
        }
        private EnemyHealth Shooter(Vector3 position)
        {
            var go = Own(new GameObject("Shooter"));
            go.transform.position = position;
            return go.AddComponent<EnemyHealth>();
        }
        private IEnumerator AttackAndScaling()
        {
            for (int d = 1; d <= 3; d++)
            {
                var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>($"Assets/ScriptableObjects/Dungeons/Dungeon_0{d}.asset");
                var player = PlayerAt(Vector3.forward * 20);
                var decoy = Own(new GameObject("TaggedDecoy"));
                decoy.tag = "Player";
                var manager = Wave(player.transform, dungeon.EnemyHealthMultiplier, dungeon.EnemyDamageMultiplier);
                var enemy = First(manager);
                var attack = enemy.GetComponent<EnemyRangedAttack>();
                var movement = enemy.GetComponent<EnemyMovement>();
                Check(enemy.MaxHealth == Mathf.Round(35 * dungeon.EnemyHealthMultiplier) && attack.Damage == Mathf.Round(8 * dungeon.EnemyDamageMultiplier),
                    $"D{d} actual scaling {enemy.MaxHealth} HP / {attack.Damage} damage");
                Check(attack.Target == player.transform && movement.Target == player.transform, "Explicit interface binding wins over tagged decoy");
                player.transform.position = Vector3.forward * 9.01f;
                Check(!attack.TryAttack(), "Attack outside 9m rejected");
                player.transform.position = Vector3.forward * 8;
                var before = enemy.transform.position;
                movement.StepMovement(0.1f);
                Check(Mathf.Abs(enemy.transform.position.z - before.z - 0.28f) < 0.01f, "Real ranged pursuit speed");
                player.transform.position = enemy.transform.position + Vector3.forward * 7;
                movement.StepMovement(0.01f);
                Check(!movement.IsMoving, "Pursuit stops at 7m");
                movement.enabled = false;
                Time.timeScale = 0;
                Check(!attack.TryAttack() && attack.ActiveProjectileCount == 0, "Paused attack suppressed");
                Time.timeScale = 1;
                // Range and aim ignore Y; keep projectile lane clear for a snapshot check.
                player.transform.position = enemy.transform.position + new Vector3(0, 50, 8);
                Check(attack.TryAttack() && attack.ActiveProjectileCount == 1 && !attack.TryAttack(), "XZ range permits shot and cooldown prevents duplicate");
                var projectile = FindFirstObjectByType<EnemyProjectile>();
                Check(projectile.TravelDirection == Vector3.forward, "Shot snapshots horizontal direction");
                player.transform.position += Vector3.right * 10;
                yield return new WaitForSecondsRealtime(0.1f);
                Check(projectile != null && projectile.TravelDirection == Vector3.forward, "Moving target cannot steer projectile");
                Time.timeScale = 0;
                before = projectile.transform.position;
                yield return new WaitForSecondsRealtime(0.1f);
                Check(projectile.transform.position == before, "Pause freezes projectile gameplay travel");
                Time.timeScale = 1;
                yield return new WaitForSeconds(1.55f);
                player.transform.position = enemy.transform.position + Vector3.forward * 8;
                Check(attack.TryAttack(), "Ranged cooldown releases after 1.6 scaled seconds");
                Cleanup();
            }
        }
        private IEnumerator Collision(string scenario)
        {
            var owner = Shooter(new Vector3(0, 0, -2));
            var player = PlayerAt(Vector3.forward * (scenario == "close" ? 0.6f : scenario == "zero-distance" ? 0 : 5));
            bool blocked = scenario == "wall" || scenario == "thin-wall" || scenario == "pillar-edge" || scenario == "non-player";
            if (scenario == "multiple-player")
            {
                var child = new GameObject("PlayerChildCollider");
                child.transform.SetParent(player.transform, false);
                child.transform.localPosition = Vector3.up;
                child.AddComponent<SphereCollider>().radius = 0.6f;
            }
            TestDamageableTarget dummy = null;
            EnemyHealth ally = null;
            if (scenario == "Zombie" || scenario == "Runner" || scenario == "Tank" || scenario == "Ranged")
            {
                var allyObject = Own(Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Enemies/{scenario}.prefab")));
                allyObject.transform.position = Vector3.forward * 2;
                foreach (var behaviour in allyObject.GetComponents<MonoBehaviour>()) behaviour.enabled = false;
                ally = allyObject.GetComponent<EnemyHealth>();
            }
            if (scenario == "owner") { owner.transform.position = Vector3.forward * 2; owner.gameObject.AddComponent<CapsuleCollider>().center = Vector3.up; }
            if (scenario == "owner-child")
            {
                var child = new GameObject("OwnerChild");
                child.transform.SetParent(owner.transform, false);
                child.transform.position = new Vector3(0, 1, 2);
                child.AddComponent<BoxCollider>();
            }
            if (blocked || scenario == "trigger")
            {
                var obstacle = Own(GameObject.CreatePrimitive(scenario == "pillar-edge" ? PrimitiveType.Cylinder : PrimitiveType.Cube));
                obstacle.transform.position = new Vector3(scenario == "pillar-edge" ? 0.6f : 0, 1, 2);
                obstacle.transform.localScale = new Vector3(1, 2, scenario == "thin-wall" ? 0.01f : 1);
                obstacle.GetComponent<Collider>().isTrigger = scenario == "trigger";
                if (scenario == "non-player") dummy = obstacle.AddComponent<TestDamageableTarget>();
            }
            var shot = Shot(owner, Vector3.up, Vector3.forward);
            if (scenario == "self") shot.gameObject.AddComponent<SphereCollider>().radius = 1;
            Physics.SyncTransforms();
            yield return Wait(() => shot == null);
            Check(player.CurrentHealth == (blocked ? player.MaxHealth : player.MaxHealth - 8), scenario + ": first accepted impact obeys collision policy");
            if (ally != null) Check(ally.CurrentHealth == ally.MaxHealth, scenario + " ally takes no damage");
            if (dummy != null) Check(dummy.HitCount == 0, "Non-player IDamageable is blocked without damage");
            yield return new WaitForSecondsRealtime(0.08f);
            Check(player.CurrentHealth == (blocked ? player.MaxHealth : player.MaxHealth - 8), scenario + ": no duplicate player damage");
            Cleanup();
        }
        private IEnumerator Lifetime(bool paused)
        {
            var owner = Shooter(Vector3.zero);
            var player = PlayerAt(Vector3.forward * 1);
            player.gameObject.SetActive(false);
            var shot = Shot(owner, Vector3.up, Vector3.forward);
            float start = Time.realtimeSinceStartup;
            if (paused) Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(2.8f);
            Check(shot != null, "Projectile remains before 3s lifetime, paused=" + paused);
            yield return Wait(() => shot == null, 0.5f);
            Check(Time.realtimeSinceStartup - start < 3.3f, "Hard 3s unscaled projectile lifetime, paused=" + paused);
            Cleanup();
        }
        private IEnumerator ShooterDeath()
        {
            var player = PlayerAt(Vector3.forward * 8);
            var manager = Wave(player.transform);
            var enemy = First(manager);
            enemy.GetComponent<EnemyMovement>().enabled = false;
            var attack = enemy.GetComponent<EnemyRangedAttack>();
            Check(attack.TryAttack(), "Shooter creates outstanding shot");
            player.transform.position = Vector3.forward * 30;
            yield return new WaitForSeconds(1.65f);
            player.transform.position = Vector3.forward * 8;
            Check(attack.TryAttack() && attack.ActiveProjectileCount == 2, "Shooter locally owns multiple outstanding shots");
            var shots = FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
            enemy.TakeDamage(1000);
            Check(attack.ActiveProjectileCount == 0 && Array.TrueForAll(shots, s => s.IsResolved) && !attack.TryAttack(),
                "Death synchronously cancels ALL owned shots before deferred Destroy");
            yield return new WaitForSecondsRealtime(0.8f);
            Check(player.CurrentHealth == player.MaxHealth && FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None).Length == 0,
                "Dead shooter cannot later damage player");
            Cleanup();
        }
        private IEnumerator Completion()
        {
            var player = PlayerAt(Vector3.forward * 8);
            var xp = player.gameObject.AddComponent<PlayerExperience>();
            xp.RestoreState(1, 85, 85);
            var stats = player.gameObject.AddComponent<PlayerStats>();
            var upgrades = Own(new GameObject("Upgrades")).AddComponent<UpgradeManager>();
            var upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>("Assets/ScriptableObjects/Upgrades/Upgrade_Damage.asset");
            upgrades.SetAvailableUpgrades(new[] { upgrade });
            upgrades.BindPlayer(xp, stats);
            RunProgressionSession.StartNewRun("warrior");
            var manager = Wave(player.transform);
            var runStats = Own(new GameObject("Stats")).AddComponent<DungeonRunStats>();
            runStats.SetReferences(manager, xp);
            var go = Own(new GameObject("Completion"));
            go.SetActive(false);
            var completion = go.AddComponent<DungeonCompletionController>();
            completion.SetReferences(manager, upgrades, runStats, xp);
            go.SetActive(true);
            var enemy = First(manager);
            yield return Wait(() => !manager.IsSpawning);
            var shots = FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
            Check(shots.Length > 0, "Final shooter has a dangerous shot before death");
            var materials = new List<Material>();
            var colors = new List<Color>();
            foreach (var renderer in enemy.GetComponentsInChildren<Renderer>()) { materials.Add(renderer.sharedMaterial); colors.Add(renderer.sharedMaterial.GetColor("_BaseColor")); }
            enemy.TakeDamage(1000);
            enemy.TakeDamage(1000);
            Check(enemy.IsDead && manager.LivingEnemyCount == 0 && Array.TrueForAll(shots, s => s.IsResolved), "Final death removes living count and projectile danger immediately");
            Check(!enemy.GetComponent<CharacterController>().enabled && !enemy.GetComponent<EnemyMovement>().enabled &&
                !enemy.GetComponent<EnemyRangedAttack>().enabled, "Final corpse has no movement, attack or collision");
            Check(xp.TotalXPEarned == 100 && xp.Level == 2 && completion.IsWaitingForUpgrades && !completion.IsCompletionFinished,
                "Exactly-once final XP precedes level-up and pending completion");
            var block = new MaterialPropertyBlock();
            foreach (var renderer in enemy.GetComponentsInChildren<Renderer>())
            {
                renderer.GetPropertyBlock(block);
                Check(block.GetColor("_BaseColor") == Color.white, "Body/marker lethal flash");
            }
            upgrades.SelectUpgrade(upgrade);
            Check(completion.IsCompletionFinished && RunProgressionSession.Level == 2 && RunProgressionSession.CommittedUpgradeIds.Count == 1,
                "Upgrade selection precedes victory commit");
            yield return new WaitForSecondsRealtime(0.15f);
            int i = 0;
            foreach (var renderer in enemy.GetComponentsInChildren<Renderer>())
            {
                renderer.GetPropertyBlock(block);
                Check(block.isEmpty && renderer.sharedMaterial == materials[i] && materials[i].GetColor("_BaseColor") == colors[i], "Paused flash restores without shared material mutation");
                i++;
            }
            yield return new WaitForSecondsRealtime(0.9f);
            Check(enemy != null, "Corpse retained before 1.5s");
            yield return Wait(() => enemy == null);
            Check(FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None).Length == 0 && player.CurrentHealth == player.MaxHealth &&
                xp.TotalXPEarned == 100, "Final corpse cleanup during pause leaves no dangerous shots or duplicate rewards");
            Cleanup();
        }
    }
}
#endif
