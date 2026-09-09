using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Progression;

namespace DungeonRoguelite.UI
{
    public enum SkillNodeUIState
    {
        Purchased,
        Available,
        InsufficientPoints,
        Locked
    }

    /// <summary>
    /// UI representation of a single node card in the permanent skill tree.
    /// Displays node name, description, cost, and visual status.
    /// Pure presentation view; purchase authority and logic remain strictly in PermanentProgression.
    /// </summary>
    public class SkillTreeNodeUI : MonoBehaviour
    {
        [Header("UI Text References")]
        [Tooltip("Text label displaying the skill node name.")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Tooltip("Text label displaying the skill effect description.")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Tooltip("Text label displaying the point cost.")]
        [SerializeField] private TextMeshProUGUI costText;

        [Tooltip("Badge or button text displaying current status (e.g. SATIN AL, SATIN ALINDI, KİLİTLİ).")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Interactive Elements")]
        [Tooltip("The purchase button component.")]
        [SerializeField] private Button purchaseButton;

        [Tooltip("Background or card image for status tinting.")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("Border frame image for state highlighting.")]
        [SerializeField] private Image borderImage;

        private SkillNodeDefinition boundNode;
        private SkillNodeUIState currentState = SkillNodeUIState.Locked;

        public SkillNodeDefinition BoundNode => boundNode;
        public SkillNodeUIState CurrentState => currentState;
        public Button PurchaseButton => purchaseButton;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI DescriptionText => descriptionText;
        public TextMeshProUGUI CostText => costText;
        public TextMeshProUGUI StatusText => statusText;
        public Image BackgroundImage => backgroundImage;
        public Image BorderImage => borderImage;

        /// <summary>
        /// Event fired when the player clicks this node's purchase button.
        /// Passes the bound SkillNodeDefinition to the listener.
        /// </summary>
        public event Action<SkillNodeDefinition> OnPurchaseRequested;

        private void Awake()
        {
            EnsureReferences();
        }

        private void EnsureReferences()
        {
            if (purchaseButton == null)
            {
                purchaseButton = GetComponentInChildren<Button>(true);
            }

            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(HandleButtonClicked);
                purchaseButton.onClick.AddListener(HandleButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(HandleButtonClicked);
            }
        }

        /// <summary>
        /// Binds the skill node definition and visual state to the card.
        /// </summary>
        public void Bind(SkillNodeDefinition node, SkillNodeUIState state)
        {
            EnsureReferences();
            boundNode = node;
            currentState = state;

            if (node == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (nameText != null)
            {
                nameText.text = node.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = node.Description;
            }

            if (costText != null)
            {
                costText.text = state == SkillNodeUIState.Purchased ? "Mevcut" : $"{node.Cost} Puan";
            }

            if (statusText != null)
            {
                switch (state)
                {
                    case SkillNodeUIState.Purchased:
                        statusText.text = "SATIN ALINDI";
                        break;
                    case SkillNodeUIState.Available:
                        statusText.text = "SATIN AL";
                        break;
                    case SkillNodeUIState.InsufficientPoints:
                        statusText.text = "YETERSİZ PUAN";
                        break;
                    case SkillNodeUIState.Locked:
                    default:
                        statusText.text = "KİLİTLİ";
                        break;
                }
            }

            // Only AVAILABLE nodes can be clicked for purchase
            bool canInteract = (state == SkillNodeUIState.Available);
            if (purchaseButton != null)
            {
                purchaseButton.interactable = canInteract;
            }

            ApplyVisualStyling(state);
        }

        private void ApplyVisualStyling(SkillNodeUIState state)
        {
            Color bgColor;
            Color borderColor;
            Color statusColor;

            switch (state)
            {
                case SkillNodeUIState.Purchased:
                    bgColor = new Color(0.08f, 0.20f, 0.12f, 0.95f);      // Dark emerald
                    borderColor = new Color(0.25f, 0.78f, 0.40f, 1f);     // Vibrant green
                    statusColor = new Color(0.35f, 0.88f, 0.50f, 1f);
                    break;
                case SkillNodeUIState.Available:
                    bgColor = new Color(0.10f, 0.18f, 0.28f, 0.95f);      // Rich slate blue
                    borderColor = new Color(0.20f, 0.75f, 1.00f, 1f);     // Bright cyan
                    statusColor = new Color(0.30f, 0.85f, 1.00f, 1f);
                    break;
                case SkillNodeUIState.InsufficientPoints:
                    bgColor = new Color(0.22f, 0.15f, 0.08f, 0.95f);      // Dark amber
                    borderColor = new Color(0.85f, 0.55f, 0.15f, 1f);     // Amber orange
                    statusColor = new Color(0.95f, 0.65f, 0.25f, 1f);
                    break;
                case SkillNodeUIState.Locked:
                default:
                    bgColor = new Color(0.09f, 0.09f, 0.11f, 0.95f);      // Dark charcoal
                    borderColor = new Color(0.28f, 0.28f, 0.32f, 1f);     // Muted slate
                    statusColor = new Color(0.55f, 0.55f, 0.60f, 1f);
                    break;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = bgColor;
            }

            if (borderImage != null)
            {
                borderImage.color = borderColor;
            }

            if (statusText != null)
            {
                statusText.color = statusColor;
            }
        }

        private void HandleButtonClicked()
        {
            if (currentState == SkillNodeUIState.Available && boundNode != null)
            {
                OnPurchaseRequested?.Invoke(boundNode);
            }
        }

        /// <summary>
        /// Programmatic setter for test runners and scene setup scripts.
        /// </summary>
        public void SetReferences(
            TextMeshProUGUI nameTMP,
            TextMeshProUGUI descTMP,
            TextMeshProUGUI costTMP,
            TextMeshProUGUI statusTMP,
            Button btn,
            Image bgImg,
            Image borderImg)
        {
            nameText = nameTMP;
            descriptionText = descTMP;
            costText = costTMP;
            statusText = statusTMP;
            purchaseButton = btn;
            backgroundImage = bgImg;
            borderImage = borderImg;
            EnsureReferences();
        }
    }
}
