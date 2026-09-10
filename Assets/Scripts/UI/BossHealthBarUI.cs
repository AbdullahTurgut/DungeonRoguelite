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
        private IBossPresentation boss;
        public EnemyHealth BoundHealth => boundHealth;
        public IBossPresentation BoundBoss => boss;
        public float DisplayedHealthRatio => fill != null ? Mathf.Clamp01(fill.rectTransform.anchorMax.x) : 0f;
        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;
        public bool IsPhasePresentationHidden => phaseLabel == null || !phaseLabel.gameObject.activeSelf || string.IsNullOrEmpty(phaseLabel.text);
        public bool IsFillConfigured => fill != null && fill.type == Image.Type.Simple &&
                                        fill.rectTransform.anchorMin == Vector2.zero &&
                                        Mathf.Approximately(fill.rectTransform.anchorMax.y, 1f) &&
                                        fill.rectTransform.offsetMin == Vector2.zero &&
                                        fill.rectTransform.offsetMax == Vector2.zero;

        private void Awake()
        {
            if (panelRoot == null) panelRoot = gameObject;
            EnsureFillConfiguration();
            HidePhasePresentation();
            Hide();
        }
        private void OnEnable() { if (waveManager == null) waveManager = FindFirstObjectByType<WaveManager>(); if (waveManager != null) waveManager.OnEnemySpawned += TryBind; }
        private void OnDisable() { if (waveManager != null) waveManager.OnEnemySpawned -= TryBind; Unbind(); }
        private void TryBind(EnemyHealth health)
        {
            var candidate = health != null ? health.GetComponent<IBossPresentation>() : null;
            if (candidate == null) return;
            Unbind(); boundHealth = health; boss = candidate; EnsureFillConfiguration(); boundHealth.OnHealthChanged += UpdateHealth; boundHealth.OnDied += HandleBossDied;
            if (nameLabel != null) nameLabel.text = boss.BossName;
            if (panelRoot != null) panelRoot.SetActive(true); HidePhasePresentation(); UpdateHealth(boundHealth.CurrentHealth, boundHealth.MaxHealth);
        }
        private void UpdateHealth(float current, float max)
        {
            if (fill == null) return;
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            var fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(ratio, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }
        private void HidePhasePresentation()
        {
            if (phaseLabel == null) return;
            phaseLabel.text = string.Empty;
            phaseLabel.gameObject.SetActive(false);
        }
        private void Hide() { if (panelRoot != null) panelRoot.SetActive(false); }
        private void HandleBossDied()
        {
            UpdateHealth(0f, 1f);
            Hide();
            Unbind();
        }

        private void EnsureFillConfiguration()
        {
            if (fill == null) return;
            fill.type = Image.Type.Simple;
            var fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        private void Unbind() { if (boundHealth != null) { boundHealth.OnHealthChanged -= UpdateHealth; boundHealth.OnDied -= HandleBossDied; } boundHealth = null; boss = null; }
    }
}
