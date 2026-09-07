using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DungeonRoguelite.Characters;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Orchestrates the Character Selection screen.
    /// Reads the CharacterRoster catalog, binds selection cards, manages local active selection,
    /// and commits the final selection to CharacterSelectionSession upon starting the dungeon.
    /// Strictly handles selection screen flow; contains zero combat, stat, or spawning logic.
    /// </summary>
    public class CharacterSelectionController : MonoBehaviour
    {
        [Header("Data Source")]
        [Tooltip("The CharacterRoster containing playable CharacterDefinitions in order.")]
        [SerializeField] private CharacterRoster roster;

        [Header("UI References")]
        [Tooltip("Array of CharacterSelectionCards configured in the scene.")]
        [SerializeField] private CharacterSelectionCard[] cards;

        [Tooltip("Text label displaying the currently selected character's name.")]
        [SerializeField] private TextMeshProUGUI selectedPreviewText;

        [Tooltip("Button to confirm selection and start the dungeon run.")]
        [SerializeField] private Button startButton;

        [Header("Scene Transition")]
        [Tooltip("Name of the gameplay dungeon scene to load upon starting.")]
        [SerializeField] private string targetSceneName = "Dungeon_Prototype";

        private CharacterDefinition currentSelection;
        private bool isTransitioning = false;

        public CharacterRoster Roster => roster;
        public IReadOnlyList<CharacterSelectionCard> Cards => cards;
        public CharacterDefinition CurrentSelection => currentSelection;
        public TextMeshProUGUI SelectedPreviewText => selectedPreviewText;
        public Button StartButton => startButton;
        public string TargetSceneName => targetSceneName;

        private void Awake()
        {
            // Requirement: Entering CharacterSelection scene clears any previous run selection
            CharacterSelectionSession.Clear();
        }

        private void Start()
        {
            InitializeUI();
        }

        private void OnEnable()
        {
            BindCardEvents();

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStartClicked);
                startButton.onClick.AddListener(HandleStartClicked);
            }
        }

        private void OnDisable()
        {
            UnbindCardEvents();

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStartClicked);
            }
        }

        /// <summary>
        /// Populates cards from roster and establishes the default initial selection.
        /// </summary>
        public void InitializeUI()
        {
            if (roster == null)
            {
                Debug.LogError("[CharacterSelectionController] CharacterRoster is not assigned.");
                return;
            }

            if (!roster.ValidateRoster(out string error))
            {
                Debug.LogError($"[CharacterSelectionController] Invalid CharacterRoster: {error}");
                return;
            }

            // Bind roster entries to available cards
            if (cards != null)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    if (cards[i] != null)
                    {
                        if (i < roster.Count)
                        {
                            cards[i].gameObject.SetActive(true);
                            cards[i].Bind(roster[i]);
                        }
                        else
                        {
                            cards[i].gameObject.SetActive(false);
                        }
                    }
                }
            }

            // Default local selection: first valid roster entry (Warrior)
            if (roster.Count > 0)
            {
                SelectCharacterLocally(roster[0]);
            }

            if (startButton != null)
            {
                startButton.interactable = currentSelection != null;
            }
        }

        private void BindCardEvents()
        {
            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card != null)
                    {
                        card.OnCardSelected -= HandleCardSelected;
                        card.OnCardSelected += HandleCardSelected;
                    }
                }
            }
        }

        private void UnbindCardEvents()
        {
            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card != null)
                    {
                        card.OnCardSelected -= HandleCardSelected;
                    }
                }
            }
        }

        /// <summary>
        /// Handles click on a character card. Updates UI state locally WITHOUT touching CharacterSelectionSession.
        /// </summary>
        private void HandleCardSelected(CharacterDefinition def)
        {
            SelectCharacterLocally(def);
        }

        /// <summary>
        /// Updates the local selection and refreshes visual highlights across all cards.
        /// </summary>
        public void SelectCharacterLocally(CharacterDefinition def)
        {
            if (def == null) return;

            currentSelection = def;

            // Enforce mutual exclusivity: exactly one card is marked selected
            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card != null && card.gameObject.activeSelf)
                    {
                        bool isThisCard = card.BoundCharacter == currentSelection;
                        card.SetSelected(isThisCard);
                    }
                }
            }

            // Update preview label
            if (selectedPreviewText != null)
            {
                selectedPreviewText.text = $"Selected: {currentSelection.DisplayName}";
            }

            if (startButton != null && !isTransitioning)
            {
                startButton.interactable = true;
            }
        }

        /// <summary>
        /// Confirms selection, commits to CharacterSelectionSession, and loads the dungeon scene.
        /// </summary>
        public void HandleStartClicked()
        {
            if (isTransitioning) return;

            if (currentSelection == null)
            {
                Debug.LogWarning("[CharacterSelectionController] Cannot start: No character selected.");
                return;
            }

            isTransitioning = true;

            if (startButton != null)
            {
                startButton.interactable = false;
            }

            // Commit selected character into session runtime carrier
            CharacterSelectionSession.SetSelection(currentSelection);

            // Transition to gameplay scene
            SceneManager.LoadScene(targetSceneName);
        }

        /// <summary>
        /// Programmatic configuration helper used by setup and verification scripts.
        /// </summary>
        public void SetReferences(CharacterRoster newRoster, CharacterSelectionCard[] newCards, TextMeshProUGUI preview, Button startBtn, string sceneName = "Dungeon_Prototype")
        {
            UnbindCardEvents();
            roster = newRoster;
            cards = newCards;
            selectedPreviewText = preview;
            startButton = startBtn;
            targetSceneName = sceneName;
            BindCardEvents();
        }
    }
}
