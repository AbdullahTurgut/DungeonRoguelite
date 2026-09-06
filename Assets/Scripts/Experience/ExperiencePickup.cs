using UnityEngine;

namespace DungeonRoguelite.Experience
{
    /// <summary>
    /// Represents a collectible experience orb/gem dropped in the dungeon.
    /// Detects Player collection via trigger volume, awards experience, and destroys itself.
    /// Strictly handles collection state; contains no hard-coded enemy or character logic.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ExperiencePickup : MonoBehaviour
    {
        [Header("Experience Configuration")]
        [Tooltip("Amount of XP awarded to the player upon collection.")]
        [SerializeField] private int xpValue = 10;

        private bool isCollected = false;

        public int XPValue => xpValue;
        public bool IsCollected => isCollected;

        /// <summary>
        /// Sets the experience value of this pickup instance.
        /// Ignores non-positive values, ensuring at least 1 XP.
        /// </summary>
        /// <param name="value">The XP value to assign.</param>
        public void Initialize(int value)
        {
            xpValue = Mathf.Max(1, value);
        }

        /// <summary>
        /// Attempts to collect this pickup for the specified PlayerExperience.
        /// Returns true if collected; false if already collected or if playerExperience is null.
        /// Guarded against duplicate collection and deferred destruction.
        /// </summary>
        /// <param name="playerExperience">The PlayerExperience to award XP to.</param>
        /// <returns>True if XP was awarded; false if already collected or invalid target.</returns>
        public bool TryCollect(PlayerExperience playerExperience)
        {
            if (isCollected || playerExperience == null)
            {
                return false;
            }

            isCollected = true;
            playerExperience.GainExperience(xpValue);
            Destroy(gameObject);
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected)
            {
                return;
            }

            var playerExperience = other.GetComponentInParent<PlayerExperience>();
            if (playerExperience == null)
            {
                playerExperience = other.GetComponent<PlayerExperience>();
            }

            if (playerExperience != null)
            {
                TryCollect(playerExperience);
            }
        }
    }
}
