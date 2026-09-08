using UnityEngine;

namespace DungeonRoguelite.Enemies
{
    /// <summary>Removes an already-dead enemy root after a purely visual, unscaled delay.</summary>
    [RequireComponent(typeof(EnemyHealth))]
    [DisallowMultipleComponent]
    public sealed class CorpseCleanup : MonoBehaviour
    {
        [SerializeField, Min(0.001f)] private float cleanupDelay = 1.5f;
        private EnemyHealth health;
        private bool scheduled;
        private float destroyAt;

        private void Awake() => health = GetComponent<EnemyHealth>();

        private void OnEnable()
        {
            health.OnDied += HandleDied;
            if (health.IsDead) HandleDied();
        }

        private void OnDisable() => health.OnDied -= HandleDied;

        private void HandleDied()
        {
            if (scheduled) return;
            scheduled = true;
            destroyAt = Time.unscaledTime + cleanupDelay;
        }

        private void Update()
        {
            if (scheduled && Time.unscaledTime >= destroyAt) Destroy(gameObject);
        }
    }
}
