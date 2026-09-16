using UnityEngine;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Audio;

namespace DungeonRoguelite.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyAudioFeedback : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] hurtClips;
        [SerializeField] private float hurtVolume = 0.22f;
        [SerializeField] private AudioClip[] deathClips;
        [SerializeField] private float deathVolume = 0.50f;

        [Tooltip("Minimum time between playing hurt sounds.")]
        [SerializeField] private float hurtCooldown = 0.2f;

        private EnemyHealth health;
        private float lastHurtTime;
        private float previousHealth;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            
            // Default 3D spatial settings for enemies
            audioSource.spatialBlend = 0.8f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 15f;
        }

        private void OnEnable()
        {
            previousHealth = health.CurrentHealth;
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDied -= HandleDied;
        }

        private void HandleHealthChanged(float current, float maximum)
        {
            bool damaged = current < previousHealth && current < maximum;
            previousHealth = current;

            if (damaged && current > 0f)
            {
                if (Time.unscaledTime >= lastHurtTime + hurtCooldown)
                {
                    lastHurtTime = Time.unscaledTime;
                    AudioHelper.PlayClip(audioSource, hurtClips, 0.9f, 1.1f, hurtVolume);
                }
            }
        }

        private void HandleDied()
        {
            AudioHelper.PlayClip(audioSource, deathClips, 0.9f, 1.1f, deathVolume);
        }
    }
}
