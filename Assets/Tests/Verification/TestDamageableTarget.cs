using UnityEngine;
using DungeonRoguelite.Combat;

namespace DungeonRoguelite.Tests
{
    /// <summary>
    /// Test-only dummy implementation of IDamageable used for verification suites.
    /// Exclusively lives in Assets/Tests/Verification/ and is not part of production gameplay.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TestDamageableTarget : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private int hitCount = 0;
        [SerializeField] private float lastDamageReceived = 0f;
        [SerializeField] private float totalDamageReceived = 0f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public int HitCount => hitCount;
        public float LastDamageReceived => lastDamageReceived;
        public float TotalDamageReceived => totalDamageReceived;
        public bool IsDead => currentHealth <= 0f;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;

            hitCount++;
            lastDamageReceived = amount;
            totalDamageReceived += amount;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
        }

        public void ResetTarget(float newHealth = 100f)
        {
            maxHealth = newHealth;
            currentHealth = newHealth;
            hitCount = 0;
            lastDamageReceived = 0f;
            totalDamageReceived = 0f;
        }
    }
}
