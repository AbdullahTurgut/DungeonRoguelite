using UnityEngine;

namespace DungeonRoguelite.Upgrades
{
    /// <summary>
    /// Data-driven definition for a temporary run upgrade.
    /// Stores metadata and magnitude for stat modifications without holding gameplay logic.
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "DungeonRoguelite/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Tooltip("Unique programmatic identifier for this upgrade.")]
        [SerializeField] private string id;

        [Tooltip("Player-facing title shown on selection cards.")]
        [SerializeField] private string displayName;

        [Tooltip("Player-facing description describing the upgrade's effect.")]
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Tooltip("Category of stat modified by this upgrade.")]
        [SerializeField] private UpgradeType upgradeType;

        [Tooltip("Additive bonus value (e.g. 0.20 for +20%).")]
        [SerializeField] private float magnitude;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public UpgradeType UpgradeType => upgradeType;
        public float Magnitude => magnitude;

        /// <summary>
        /// Initializes the upgrade definition at runtime or during automated asset generation.
        /// </summary>
        public void Initialize(string upgradeId, string name, string desc, UpgradeType type, float mag)
        {
            id = upgradeId;
            displayName = name;
            description = desc;
            upgradeType = type;
            magnitude = mag;
        }
    }
}
