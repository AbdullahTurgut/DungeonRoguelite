using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.UI
{
    /// <summary>Scene HUD adapter for the active boss; it owns presentation only.</summary>
    public sealed class BossHealthBarUI : MonoBehaviour
    {
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image fill;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text phaseLabel;
        private EnemyHealth boundHealth;
        private BossWardenController boss;
        private void Awake() { if (panelRoot == null) panelRoot = gameObject; Hide(); }
        private void OnEnable() { if (waveManager == null) waveManager = FindFirstObjectByType<WaveManager>(); if (waveManager != null) waveManager.OnEnemySpawned += TryBind; }
        private void OnDisable() { if (waveManager != null) waveManager.OnEnemySpawned -= TryBind; Unbind(); }
        private void TryBind(EnemyHealth health)
        {
            var candidate = health != null ? health.GetComponent<BossWardenController>() : null;
            if (candidate == null) return;
            Unbind(); boundHealth = health; boss = candidate; boundHealth.OnHealthChanged += UpdateHealth; boundHealth.OnDied += Hide; boss.OnPhaseChanged += UpdatePhase;
            if (nameLabel != null) nameLabel.text = boss.BossName;
            if (panelRoot != null) panelRoot.SetActive(true); UpdateHealth(boundHealth.CurrentHealth, boundHealth.MaxHealth); UpdatePhase(boss.IsPhaseTwo);
        }
        private void UpdateHealth(float current, float max) { if (fill != null) fill.fillAmount = max > 0f ? current / max : 0f; }
        private void UpdatePhase(bool phaseTwo) { if (phaseLabel != null) phaseLabel.text = phaseTwo ? "PHASE II" : string.Empty; }
        private void Hide() { if (panelRoot != null) panelRoot.SetActive(false); }
        private void Unbind() { if (boundHealth != null) { boundHealth.OnHealthChanged -= UpdateHealth; boundHealth.OnDied -= Hide; } if (boss != null) boss.OnPhaseChanged -= UpdatePhase; boundHealth = null; boss = null; }
    }
}
