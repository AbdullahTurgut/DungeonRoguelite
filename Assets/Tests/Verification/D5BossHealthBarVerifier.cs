#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Tests;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Tests
{
    /// <summary>Exercises the production boss HUD through real WaveManager spawning and EnemyHealth damage events.</summary>
    public sealed class D5BossHealthBarVerifier : MonoBehaviour
    {
        private Phase12StateSnapshot snapshot;
        private int checks;
        private bool passed = true;
        private GameObject player;
        private GameObject managerObject;
        private GameObject hudRoot;

        private IEnumerator Start()
        {
            snapshot = new Phase12StateSnapshot();
            var routine = Verify();
            while (true)
            {
                object current;
                try
                {
                    if (!routine.MoveNext()) break;
                    current = routine.Current;
                }
                catch (Exception exception)
                {
                    passed = false;
                    Debug.LogException(exception);
                    break;
                }

                yield return current;
            }
            (routine as IDisposable)?.Dispose();
            Cleanup();

            Debug.Log($"[D5 BOSS HEALTH BAR COMPLETE] {(passed ? "PASSED" : "FAILED")}: {checks} checks passed.");
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
            else EditorApplication.isPlaying = false;
        }

        private IEnumerator Verify()
        {
            var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/ScriptableObjects/Dungeons/Dungeon_05.asset");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/AshWarden.prefab");
            Check(dungeon != null && prefab != null && prefab.GetComponent<BossWardenController>() != null,
                "D5 Ash Warden production assets resolve");

            player = new GameObject("D5BossHealthPlayer");
            player.transform.position = new Vector3(100f, 0f, 0f);
            managerObject = new GameObject("D5BossHealthWaveManager");
            var manager = managerObject.AddComponent<WaveManager>();
            manager.ConfigureFromDungeon(dungeon);
            manager.SetWaves(new[] { CreateBossOnlyWave(prefab) });
            manager.SetPlayerTarget(player.transform);
            manager.SetSpawnPoints(new[] { manager.transform });

            var hud = CreateHud(manager);
            Check(hud.IsFillConfigured && hud.IsPhasePresentationHidden, "Boss HUD creates a sprite-independent RectTransform fill without phase text");
            manager.BeginDungeon();
            yield return new WaitForSeconds(.1f);

            var firstBoss = GetOnlyBoss(manager);
            var firstHealth = firstBoss.GetComponent<EnemyHealth>();
            Check(hud.BoundHealth == firstHealth && hud.BoundBoss == firstBoss && hud.IsVisible,
                "Boss HUD binds once through WaveManager.OnEnemySpawned");
            Check(Approximately(hud.DisplayedHealthRatio, 1f), "Boss spawn displays full real health ratio");

            firstHealth.TakeDamage(168f);
            Check(Approximately(hud.DisplayedHealthRatio, firstHealth.CurrentHealth / firstHealth.MaxHealth),
                "First real EnemyHealth damage updates the displayed ratio");
            firstHealth.TakeDamage(168f);
            Check(Approximately(hud.DisplayedHealthRatio, firstHealth.CurrentHealth / firstHealth.MaxHealth),
                "Second real EnemyHealth damage updates the displayed ratio again");

            firstHealth.TakeDamage(firstHealth.CurrentHealth - firstHealth.MaxHealth * .5f);
            yield return null;
            Check(firstBoss.IsPhaseTwo && hud.BoundHealth == firstHealth && Approximately(hud.DisplayedHealthRatio, .5f),
                "Phase two at real 50 percent health preserves the live boss binding");

            firstHealth.TakeDamage(firstHealth.CurrentHealth);
            yield return null;
            Check(Approximately(hud.DisplayedHealthRatio, 0f) && !hud.IsVisible && hud.BoundHealth == null && hud.BoundBoss == null,
                "Boss death reaches zero, hides the bar, and removes stale listeners");

            manager.StopWaves();
            manager.BeginDungeon();
            yield return new WaitForSeconds(.1f);
            var retryBoss = GetOnlyBoss(manager);
            var retryHealth = retryBoss.GetComponent<EnemyHealth>();
            Check(retryHealth != firstHealth && hud.BoundHealth == retryHealth && hud.BoundBoss == retryBoss && hud.IsVisible &&
                Approximately(hud.DisplayedHealthRatio, 1f), "Fresh retry boss binds once and starts at full health");
        }

        private BossWardenController GetOnlyBoss(WaveManager manager)
        {
            foreach (var health in manager.ActiveEnemies)
            {
                var boss = health != null ? health.GetComponent<BossWardenController>() : null;
                if (boss != null) return boss;
            }

            throw new InvalidOperationException("Expected exactly one spawned Ash Warden.");
        }

        private BossHealthBarUI CreateHud(WaveManager manager)
        {
            hudRoot = new GameObject("D5BossHealthHud", typeof(Canvas));
            hudRoot.SetActive(false);
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(hudRoot.transform, false);
            var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(panel.transform, false);
            var hud = hudRoot.AddComponent<BossHealthBarUI>();
            var serialized = new SerializedObject(hud);
            serialized.FindProperty("waveManager").objectReferenceValue = manager;
            serialized.FindProperty("panelRoot").objectReferenceValue = panel;
            serialized.FindProperty("fill").objectReferenceValue = fillObject.GetComponent<Image>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            hudRoot.SetActive(true);
            return hud;
        }

        private static WaveDefinition CreateBossOnlyWave(GameObject bossPrefab)
        {
            var wave = ScriptableObject.CreateInstance<WaveDefinition>();
            wave.Initialize(new[] { new EnemySpawnEntry(bossPrefab, 1) }, .001f);
            return wave;
        }

        private void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException("[D5 BOSS HEALTH BAR CHECK FAILED] " + description);
            checks++;
            Debug.Log("[D5 BOSS HEALTH BAR CHECK PASSED] " + description);
        }

        private static bool Approximately(float left, float right) => Mathf.Abs(left - right) < .0001f;

        private void Cleanup()
        {
            if (hudRoot != null) Destroy(hudRoot);
            if (managerObject != null) Destroy(managerObject);
            if (player != null) Destroy(player);
            snapshot?.Dispose();
            snapshot = null;
            Debug.Log("[D5 BOSS HEALTH BAR STATE RESTORED]");
        }
    }
}
#endif
