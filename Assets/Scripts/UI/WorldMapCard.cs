using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Presentation component representing a single dungeon on the World Map screen.
    /// Pure presentation layer; notifies WorldMapController via OnCardClicked when selected.
    /// </summary>
    public class WorldMapCard : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Text displaying dungeon display name.")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("Text displaying dungeon description / wave requirement.")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Tooltip("Text displaying dungeon status ('AÇIK', 'KİLİTLİ', 'TAMAMLANDI').")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Tooltip("Visual overlay shown when the dungeon is locked.")]
        [SerializeField] private GameObject lockOverlay;

        [Tooltip("Visual outline/border shown when the card is currently selected.")]
        [SerializeField] private GameObject selectionBorder;

        [Tooltip("Button component for the card.")]
        [SerializeField] private Button cardButton;

        private DungeonDefinition boundDungeon;
        private bool isUnlocked;
        private bool isCompleted;
        private bool isSelected;

        public DungeonDefinition BoundDungeon => boundDungeon;
        public bool IsUnlocked => isUnlocked;
        public bool IsCompleted => isCompleted;
        public bool IsSelected => isSelected;
        public TextMeshProUGUI TitleText => titleText;
        public TextMeshProUGUI DescriptionText => descriptionText;
        public TextMeshProUGUI StatusText => statusText;
        public GameObject LockOverlay => lockOverlay;
        public GameObject SelectionBorder => selectionBorder;
        public Button CardButton => cardButton;

        /// <summary>
        /// Fired when this card is clicked by the player.
        /// </summary>
        public event Action<WorldMapCard> OnCardClicked;

        private void Awake()
        {
            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(HandleCardButtonClicked);
                cardButton.onClick.AddListener(HandleCardButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(HandleCardButtonClicked);
            }
        }

        private void HandleCardButtonClicked()
        {
            OnCardClicked?.Invoke(this);
        }

        /// <summary>
        /// Binds the card to a DungeonDefinition and updates its presentation state.
        /// </summary>
        public void Bind(DungeonDefinition dungeon, bool unlocked, bool completed)
        {
            boundDungeon = dungeon;
            isUnlocked = unlocked;
            isCompleted = completed;

            if (titleText != null)
            {
                titleText.text = dungeon != null ? dungeon.DisplayName : string.Empty;
                titleText.color = Color.white;
            }

            if (descriptionText != null)
            {
                descriptionText.text = dungeon != null ? dungeon.Description : string.Empty;
                descriptionText.color = unlocked ? new Color(0.90f, 0.93f, 0.97f, 1f) : new Color(0.72f, 0.76f, 0.82f, 1f);
            }

            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!unlocked);
            }

            if (statusText != null)
            {
                if (completed)
                {
                    statusText.text = "TAMAMLANDI";
                    statusText.color = new Color(0.35f, 0.95f, 0.45f, 1f);
                }
                else if (unlocked)
                {
                    statusText.text = "AÇIK";
                    statusText.color = new Color(0.25f, 0.85f, 1f, 1f);
                }
                else
                {
                    statusText.text = "KİLİTLİ";
                    statusText.color = new Color(0.95f, 0.35f, 0.35f, 1f);
                }
            }
        }

        /// <summary>
        /// Updates the selection border highlight.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectionBorder != null)
            {
                selectionBorder.SetActive(selected);
            }
        }

        public void SetReferences(
            TextMeshProUGUI title,
            TextMeshProUGUI desc,
            TextMeshProUGUI status,
            GameObject lockObj,
            GameObject selectBorder,
            Button btn)
        {
            titleText = title;
            descriptionText = desc;
            statusText = status;
            lockOverlay = lockObj;
            selectionBorder = selectBorder;
            cardButton = btn;

            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(HandleCardButtonClicked);
                cardButton.onClick.AddListener(HandleCardButtonClicked);
            }
        }
    }
}
