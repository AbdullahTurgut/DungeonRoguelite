#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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
    /// <summary>Exercises production spawning, damage, rewards and completion without private API calls.</summary>
    public sealed class Milestone11_1_Verifier : MonoBehaviour
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private int checks;
        private int runtimeErrors;
        private GameObject zombie;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int Marker = Shader.PropertyToID("_Gate11Marker");

        private IEnumerator Start()
        {
            Application.logMessageReceived += HandleLog;
            bool hadSave = PlayerPrefs.HasKey(DungeonProgression.PrefsKey);
            string saved = PlayerPrefs.GetString(DungeonProgression.PrefsKey, "");
            var completedIds = new List<string>(DungeonProgression.CompletedDungeonIds);
            float originalTimeScale = Time.timeScale;
            bool success = true;
            var routines = new Stack<IEnumerator>();
            routines.Push(Verify());
            // Flatten nested routines so exceptions in every test produce a failing batch exit.
            while (routines.Count > 0)
            {
                object current = null;
                try
                {
                    if (!routines.Peek().MoveNext()) { routines.Pop(); continue; }
                    current = routines.Peek().Current;
                    if (current is IEnumerator nested) { routines.Push(nested); continue; }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    success = false;
                    break;
                }
                yield return current;
            }
            Cleanup();
            RunProgressionSession.Clear();
            DungeonRunSession.Clear();
            CharacterSelectionSession.Clear();
            DungeonProgression.ResetProgression();
            foreach (string id in completedIds) DungeonProgression.RecordDungeonCompleted(id);
            if (hadSave) PlayerPrefs.SetString(DungeonProgression.PrefsKey, saved);
            else PlayerPrefs.DeleteKey(DungeonProgression.PrefsKey);
            PlayerPrefs.Save();
            Time.timeScale = originalTimeScale;
            Application.logMessageReceived -= HandleLog;
            success &= runtimeErrors == 0;
            Debug.Log($"[GATE 11.1 COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.isPlaying = false;
        }

        private void HandleLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) runtimeErrors++;
        }

        private T Own<T>(T value) where T : UnityEngine.Object { owned.Add(value); return value; }
        private void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException("[GATE 11.1 FAILED] " + description);
            Debug.Log($"[GATE 11.1 CHECK {++checks} PASSED] {description}");
        }
        private void Cleanup()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) DestroyImmediate(owned[i]);
            owned.Clear();
            foreach (var pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None))
                DestroyImmediate(pickup.gameObject);
            Time.timeScale = 1f;
        }
        private GameObject Player()
        {
            var player = Own(new GameObject("ExplicitPlayer"));
            player.transform.position = new Vector3(100f, 0f, 100f);
            player.AddComponent<PlayerHealth>();
            return player;
        }
        private WaveManager Wave(GameObject prefab, Transform player, float hp = 1f, float damage = 1f, int count = 1, int waveCount = 1)
        {
            var definition = Own(ScriptableObject.CreateInstance<WaveDefinition>());
            definition.Initialize(new[] { new EnemySpawnEntry(prefab, count) }, 0.02f);
            var definitions = new WaveDefinition[waveCount];
            for (int i = 0; i < waveCount; i++) definitions[i] = definition;
            var dungeon = Own(ScriptableObject.CreateInstance<DungeonDefinition>());
            dungeon.SetConfiguration("gate11_test", "Test", "", "", "", definitions, hp, damage);
            var manager = Own(new GameObject("TestWaves")).AddComponent<WaveManager>();
            manager.ConfigureFromDungeon(dungeon);
            manager.SetSpawnPoints(new[] { manager.transform });
            manager.BindPlayer(player);
            return manager;
        }
        private IEnumerator WaitUntilBounded(Func<bool> predicate, string description, float seconds = 3f)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate())
            {
                if (Time.realtimeSinceStartup > deadline) throw new TimeoutException(description);
                yield return null;
            }
        }
        private EnemyHealth First(WaveManager manager)
        {
            foreach (var enemy in manager.ActiveEnemies) { Own(enemy.gameObject); return enemy; }
            throw new InvalidOperationException("Expected a spawned enemy.");
        }
        private bool ColorIs(Renderer renderer, Color color)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            return block.GetColor(BaseColor) == color;
        }

        private IEnumerator Verify()
        {
            Time.timeScale = 1f;
            RunProgressionSession.Clear();
            DungeonRunSession.Clear();
            CharacterSelectionSession.Clear();
            zombie = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            Check(zombie != null && zombie.GetComponent<IEnemyAttack>() != null &&
                typeof(IEnemyAttack).GetMethods().Length == 2, "Minimal interface and Zombie implementation");
            Check(zombie.GetComponent<EnemyVisualFeedback>() != null && zombie.GetComponent<CorpseCleanup>() != null,
                "Zombie includes feedback and cleanup");
            var movement = zombie.GetComponent<EnemyMovement>();
            var attack = zombie.GetComponent<EnemyAttack>();
            Check(zombie.GetComponent<EnemyHealth>().MaxHealth == 50f && attack.Damage == 10f &&
                attack.AttackRange == 1.5f && attack.AttackCooldown == 1f && movement.MoveSpeed == 3f &&
                movement.StoppingDistance == 1.3f && zombie.GetComponent<ExperienceReward>().XPAmount == 10,
                "Zombie base combat, movement and reward values preserved");
            yield return VerifyScaling();
            yield return VerifyFeedback();
            yield return VerifyDeath();
            yield return VerifyCompletion();
            Check(zombie.GetComponent<EnemyHealth>().MaxHealth == 50f && zombie.GetComponent<EnemyAttack>().Damage == 10f,
                "Runtime tests did not mutate prefab stats");
        }

        private IEnumerator VerifyScaling()
        {
            for (int i = 1; i <= 3; i++)
            {
                var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>($"Assets/ScriptableObjects/Dungeons/Dungeon_0{i}.asset");
                Check(dungeon != null && Mathf.Approximately(dungeon.EnemyHealthMultiplier, 1f + (i - 1) * 0.1f) &&
                    Mathf.Approximately(dungeon.EnemyDamageMultiplier, i == 3 ? 1.1f : 1f), $"D{i} difficulty asset unchanged");
                var player = Player();
                var decoy = Own(new GameObject("TaggedDecoy"));
                decoy.tag = "Player";
                var manager = Wave(zombie, player.transform, dungeon.EnemyHealthMultiplier, dungeon.EnemyDamageMultiplier);
                manager.BeginDungeon();
                var enemy = First(manager);
                var attack = enemy.GetComponent<EnemyAttack>();
                Check(enemy.MaxHealth == 50f + (i - 1) * 5f && attack.Damage == (i == 3 ? 11f : 10f),
                    $"D{i} production wave spawn scales once");
                Check(attack.Target == player.transform && enemy.GetComponent<EnemyMovement>().Target == player.transform,
                    $"D{i} explicit target wins over tagged decoy");
                player.transform.position = enemy.transform.position + Vector3.forward;
                float before = player.GetComponent<PlayerHealth>().CurrentHealth;
                Check(attack.TryAttack() && player.GetComponent<PlayerHealth>().CurrentHealth == before - attack.Damage &&
                    !attack.TryAttack(), $"D{i} real attack applies scaled damage and cooldown");
                Cleanup();
            }
            var alternate = Own(new GameObject("AlternativeAttackTemplate"));
            alternate.transform.position = Vector3.one * 500f;
            alternate.AddComponent<EnemyHealth>();
            alternate.AddComponent<Gate11AttackProbe>();
            var target = Player();
            var waves = Wave(alternate, target.transform, 1.2f, 1.1f);
            waves.BeginDungeon();
            var spawned = First(waves);
            var probe = spawned.GetComponent<Gate11AttackProbe>();
            Check(spawned.GetComponent<EnemyAttack>() == null && probe.Initializations == 1 &&
                probe.Multiplier == 1.1f && probe.Target == target.transform, "Alternative implementation receives interface scaling and binding");
            Cleanup();
            yield return null;
        }

        private IEnumerator VerifyFeedback()
        {
            var instance = Own(Instantiate(zombie));
            var health = instance.GetComponent<EnemyHealth>();
            var renderers = instance.GetComponentsInChildren<Renderer>();
            var materials = new Material[renderers.Length];
            var baseline = new Color(0.2f, 0.3f, 0.4f, 1f);
            var block = new MaterialPropertyBlock();
            block.SetColor(BaseColor, baseline);
            block.SetFloat(Marker, 17f);
            for (int i = 0; i < renderers.Length; i++)
            {
                materials[i] = renderers[i].sharedMaterial;
                renderers[i].SetPropertyBlock(block);
            }
            int materialCount = Resources.FindObjectsOfTypeAll<Material>().Length;
            Color sharedColor = materials[0].GetColor(BaseColor);
            health.InitializeHealth(0.5f);
            Check(Array.TrueForAll(renderers, r => ColorIs(r, baseline)), "Downward health initialization does not flash");
            health.TakeDamage(0f);
            health.TakeDamage(-1f);
            Check(Array.TrueForAll(renderers, r => ColorIs(r, baseline)), "Ignored damage does not flash");
            health.TakeDamage(1f);
            Check(Array.TrueForAll(renderers, r => ColorIs(r, Color.white)), "Damage immediately flashes both renderers");
            yield return new WaitForSecondsRealtime(0.04f);
            health.TakeDamage(1f);
            yield return new WaitForSecondsRealtime(0.05f);
            Check(Array.TrueForAll(renderers, r => ColorIs(r, Color.white)), "Repeated hit restarts flash deadline");
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.12f);
            Check(Array.TrueForAll(renderers, r => ColorIs(r, baseline)), "Flash restores while paused");
            renderers[0].GetPropertyBlock(block);
            Check(block.GetFloat(Marker) == 17f, "Unrelated property block values preserved");
            health.TakeDamage(1f);
            instance.GetComponent<EnemyVisualFeedback>().enabled = false;
            Check(Array.TrueForAll(renderers, r => ColorIs(r, baseline)), "Disabling feedback restores overrides");
            instance.GetComponent<EnemyVisualFeedback>().enabled = true;
            for (int i = 0; i < 10; i++) health.TakeDamage(0.1f);
            yield return new WaitForSecondsRealtime(0.12f);
            Check(Resources.FindObjectsOfTypeAll<Material>().Length == materialCount && materials[0].GetColor(BaseColor) == sharedColor,
                "Repeated feedback creates no material instances or shared color changes");
            for (int i = 0; i < renderers.Length; i++)
                Check(renderers[i].sharedMaterial == materials[i], "Shared material identity preserved");
            Cleanup();
        }

        private IEnumerator VerifyDeath()
        {
            var player = Player();
            var manager = Wave(zombie, player.transform, count: 1, waveCount: 2);
            int kills = 0;
            manager.OnEnemyDefeated += _ => kills++;
            manager.BeginDungeon();
            var enemy = First(manager);
            int deaths = 0;
            enemy.OnDied += () => deaths++;
            enemy.TakeDamage(1000f);
            Check(Array.TrueForAll(enemy.GetComponentsInChildren<Renderer>(), r => ColorIs(r, Color.white)),
                "First hit on an unscaled spawn produces lethal hit feedback");
            Check(enemy.IsDead && manager.LivingEnemyCount == 0 && deaths == 1 && kills == 1,
                "Death and living-count removal are synchronous");
            Check(manager.IsSpawning && manager.CurrentWaveIndex == 0, "Death during spawning does not skip spawn completion");
            Check(enemy.gameObject.activeSelf && !enemy.GetComponent<CharacterController>().enabled &&
                !enemy.GetComponent<EnemyMovement>().enabled && !enemy.GetComponent<EnemyAttack>().TryAttack(),
                "Corpse stays visible but stops collision, movement and attacks immediately");
            Check(FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == 1 &&
                enemy.GetComponent<ExperienceReward>().HasRewarded, "Death immediately creates exactly one XP pickup");
            enemy.TakeDamage(1000f);
            Check(deaths == 1 && kills == 1 && FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == 1,
                "Repeated lethal damage cannot duplicate death, kill or XP");
            yield return new WaitForSecondsRealtime(0.12f);
            enemy.TakeDamage(1000f);
            var corpseBlock = new MaterialPropertyBlock();
            enemy.GetComponentInChildren<Renderer>().GetPropertyBlock(corpseBlock);
            Check(corpseBlock.isEmpty, "Post-death damage cannot restart a finished flash");
            yield return WaitUntilBounded(() => manager.CurrentWaveIndex == 1, "Second wave transition");
            var second = First(manager);
            Check(enemy != null && manager.LivingEnemyCount == 1, "Next wave starts before previous corpse removal");
            Time.timeScale = 0f;
            yield return WaitUntilBounded(() => enemy == null, "Paused corpse cleanup");
            Check(second != null && !second.IsDead, "Cleanup removes only the dead root");
            Cleanup();
        }

        private IEnumerator VerifyCompletion()
        {
            var player = Player();
            var experience = player.AddComponent<PlayerExperience>();
            var stats = player.AddComponent<PlayerStats>();
            experience.RestoreState(1, 90, 90);
            RunProgressionSession.StartNewRun("warrior");
            var manager = Wave(zombie, player.transform);
            var upgrades = Own(new GameObject("Upgrades")).AddComponent<UpgradeManager>();
            var upgrade = Own(ScriptableObject.CreateInstance<UpgradeDefinition>());
            upgrade.Initialize("gate11_damage", "Damage", "", UpgradeType.Damage, 0.2f);
            upgrades.SetAvailableUpgrades(new[] { upgrade });
            upgrades.BindPlayer(experience, stats);
            var runStats = Own(new GameObject("Stats")).AddComponent<DungeonRunStats>();
            runStats.SetReferences(manager, experience);
            var completionGo = Own(new GameObject("Completion"));
            completionGo.SetActive(false);
            var completion = completionGo.AddComponent<DungeonCompletionController>();
            completion.SetReferences(manager, upgrades, runStats, experience);
            completionGo.SetActive(true);
            int completed = 0;
            manager.OnDungeonCompleted += () => completed++;
            manager.BeginDungeon();
            var enemy = First(manager);
            yield return WaitUntilBounded(() => !manager.IsSpawning, "Final wave spawning finished");
            float deathTime = Time.realtimeSinceStartup;
            enemy.TakeDamage(1000f);
            Check(completed == 1 && manager.LivingEnemyCount == 0 && enemy != null,
                "Final wave completes immediately while corpse exists");
            Check(experience.TotalXPEarned == 100 && experience.Level == 2 && completion.IsWaitingForUpgrades &&
                !completion.IsCompletionFinished && Time.timeScale == 0f, "Final-kill XP resolves before pending upgrade selection");
            upgrades.SelectUpgrade(upgrade);
            Check(completion.IsCompletionFinished && completion.FinalSummary.EnemiesDefeated == 1 &&
                completion.FinalSummary.TotalXPEarned == 100 && RunProgressionSession.Level == 2 &&
                RunProgressionSession.CommittedUpgradeIds.Count == 1, "Upgrade, summary and victory commit finish before cleanup");
            yield return new WaitForSecondsRealtime(1f);
            Check(enemy != null, "Corpse is retained before 1.5 seconds");
            yield return WaitUntilBounded(() => enemy == null, "Final corpse removed during victory pause");
            float corpseLifetime = Time.realtimeSinceStartup - deathTime;
            Check(corpseLifetime >= 1.5f && corpseLifetime < 1.75f && Time.timeScale == 0f && completed == 1,
                "Cleanup uses unscaled delay and does not repeat completion");
            Check(manager.CurrentWaveSpawned.Count == 1 && manager.CurrentWaveSpawned[0] == null,
                "Historical spawn list can contain a destroyed corpse without affecting living count");
            Cleanup();
            var early = Own(Instantiate(zombie));
            early.GetComponent<EnemyHealth>().TakeDamage(1000f);
            DestroyImmediate(early);
            yield return new WaitForSecondsRealtime(1.6f);
            Check(early == null, "Early teardown safely cancels pending visual work");
        }
    }

    // Verifier-only implementation proves WaveManager does not require the concrete melee component.
    public sealed class Gate11AttackProbe : MonoBehaviour, IEnemyAttack
    {
        public int Initializations { get; private set; }
        public float Multiplier { get; private set; }
        public Transform Target { get; private set; }
        public void InitializeAttack(float multiplier) { Initializations++; Multiplier = multiplier; }
        public void SetTarget(Transform target) { Target = target; }
    }
}
#endif
