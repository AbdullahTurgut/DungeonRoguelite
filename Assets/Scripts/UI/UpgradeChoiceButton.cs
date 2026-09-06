using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Upgrades;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Represents a selectable upgrade card in the level-up selection UI.
    /// Binds to an UpgradeDefinition, displays title and description, and handles button interaction.
    /// Strictly handles UI view logic; contains zero stat calculations or gameplay modifications.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UpgradeChoiceButton : MonoBehaviour
    {
        [Header("UI Text References")]
        [Tooltip("Text label displaying the upgrade title / display name.")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("Text label displaying the upgrade description.")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        private Button button;
        private UpgradeDefinition boundUpgrade;

        public UpgradeDefinition BoundUpgrade => boundUpgrade;
        public Button Button => button;
        public TextMeshProUGUI TitleText => titleText;
        public TextMeshProUGUI DescriptionText => descriptionText;

        /// <summary>
        /// Fired when this choice button is clicked by the player. Passes the bound UpgradeDefinition.
        /// </summary>
        public event Action<UpgradeDefinition> OnClicked;

        private void Awake()
        {
            EnsureButtonReference();
        }

        private void EnsureButtonReference()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveListener(HandleClick);
                    button.onClick.AddListener(HandleClick);
                }
            }
        }

        /// <summary>
        /// Binds an UpgradeDefinition to this button card.
        /// </summary>
        public void Bind(UpgradeDefinition upgrade)
        {
            EnsureButtonReference();
            boundUpgrade = upgrade;

            if (titleText != null)
            {
                titleText.text = upgrade != null ? upgrade.DisplayName : string.Empty;
            }

            if (descriptionText != null)
            {
                descriptionText.text = upgrade != null ? upgrade.Description : string.Empty;
            }

            SetInteractable(true);
        }

        /// <summary>
        /// Enables or disables click interaction for this button.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            EnsureButtonReference();
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void HandleClick()
        {
            if (boundUpgrade != null)
            {
                SetInteractable(false);
                OnClicked?.Invoke(boundUpgrade);
            }
        }

        /// <summary>
        /// Configures text references programmatically (used by setup scripts).
        /// </summary>
        public void SetReferences(TextMeshProUGUI title, TextMeshProUGUI desc)
        {
            titleText = title;
            descriptionText = desc;
        }
    }
}
