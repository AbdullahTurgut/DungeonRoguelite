using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Characters;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Represents a single character card in the Character Selection UI.
    /// Displays character display name and playstyle description, renders visual selection highlight,
    /// and fires selection intent event when clicked.
    /// Strictly handles card view logic; contains zero combat, stat, or scene-management logic.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CharacterSelectionCard : MonoBehaviour
    {
        [Header("UI Text References")]
        [Tooltip("Text label displaying the character display name.")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Tooltip("Text label displaying the character description.")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Visual Feedback")]
        [Tooltip("Visual indicator GameObject/Image toggled when this card is actively selected.")]
        [SerializeField] private GameObject selectionHighlight;

        private Button button;
        private CharacterDefinition boundCharacter;
        private bool isSelected = false;

        public CharacterDefinition BoundCharacter => boundCharacter;
        public Button Button => button;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI DescriptionText => descriptionText;
        public GameObject SelectionHighlight => selectionHighlight;
        public bool IsSelected => isSelected;

        /// <summary>
        /// Fired when this card is clicked by the player. Passes the bound CharacterDefinition.
        /// </summary>
        public event Action<CharacterDefinition> OnCardSelected;

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
        /// Binds a CharacterDefinition archetype to this card, populating name and description.
        /// </summary>
        public void Bind(CharacterDefinition characterDef)
        {
            EnsureButtonReference();
            boundCharacter = characterDef;

            if (nameText != null)
            {
                nameText.text = characterDef != null ? characterDef.DisplayName : string.Empty;
            }

            if (descriptionText != null)
            {
                descriptionText.text = characterDef != null ? characterDef.Description : string.Empty;
            }

            SetSelected(false);
            SetInteractable(true);
        }

        /// <summary>
        /// Updates the visual selection highlight state for this card.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(selected);
            }
        }

        /// <summary>
        /// Enables or disables click interactivity on the button.
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
            if (boundCharacter != null)
            {
                OnCardSelected?.Invoke(boundCharacter);
            }
        }

        /// <summary>
        /// Programmatic configuration helper used by setup and verification scripts.
        /// </summary>
        public void SetReferences(TextMeshProUGUI nameLabel, TextMeshProUGUI descLabel, GameObject highlightGo)
        {
            nameText = nameLabel;
            descriptionText = descLabel;
            selectionHighlight = highlightGo;
        }
    }
}
