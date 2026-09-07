using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Screen orchestrator for the World Map scene.
    /// Binds dungeon campaign cards from DungeonCatalog, applies progression unlock/completion state,
    /// manages local dungeon selection, commits to DungeonRunSession, and launches into gameplay.
    /// Pure orchestrator; decoupled from combat and player controller systems.
    /// </summary>
    public class WorldMapController : MonoBehaviour
    {
        [Header("Catalog Reference")]
        [Tooltip("The catalog containing campaign dungeons in progression order.")]
        [SerializeField] private DungeonCatalog dungeonCatalog;

        [Header("UI References")]
        [Tooltip("Card components representing dungeons in the scene.")]
        [SerializeField] private WorldMapCard[] cards;

        [Tooltip("Button to enter the selected dungeon.")]
        [SerializeField] private Button enterDungeonButton;

        [Tooltip("Button to navigate back to Character Selection.")]
        [SerializeField] private Button backButton;

        [Tooltip("Text label displaying the currently active hero from CharacterSelectionSession.")]
        [SerializeField] private TextMeshProUGUI activeHeroText;

        [Tooltip("Header title text for the world map.")]
        [SerializeField] private TextMeshProUGUI titleText;

        private DungeonDefinition selectedDungeon;
        private bool isTransitioning = false;

        public DungeonCatalog DungeonCatalog => dungeonCatalog;
        public IReadOnlyList<WorldMapCard> Cards => cards;
        public DungeonDefinition SelectedDungeon => selectedDungeon;
        public Button EnterDungeonButton => enterDungeonButton;
        public Button BackButton => backButton;
        public TextMeshProUGUI ActiveHeroText => activeHeroText;
        public TextMeshProUGUI TitleText => titleText;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            InitializeMap();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        private void ResolveReferences()
        {
            if (dungeonCatalog == null)
            {
                dungeonCatalog = Resources.Load<DungeonCatalog>("Dungeons/DungeonCatalog");
#if UNITY_EDITOR
                if (dungeonCatalog == null)
                {
                    dungeonCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<DungeonCatalog>("Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset");
                }
#endif
            }
        }

        /// <summary>
        /// Populates hero preview and dungeon cards from catalog and progression state.
        /// </summary>
        public void InitializeMap()
        {
            ResolveReferences();
            UnbindEvents();

            // Display active hero from session
            if (activeHeroText != null)
            {
                string heroName = CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null
                    ? CharacterSelectionSession.SelectedCharacter.DisplayName
                    : "Savaşçı (Varsayılan)";
                activeHeroText.text = $"Kahraman: {heroName}";
            }

            // Bind catalog to cards
            if (cards != null && dungeonCatalog != null)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    if (cards[i] != null)
                    {
                        if (i < dungeonCatalog.Count)
                        {
                            var def = dungeonCatalog[i];
                            bool unlocked = DungeonProgression.IsDungeonUnlocked(def);
                            bool completed = DungeonProgression.IsDungeonCompleted(def.Id);

                            cards[i].gameObject.SetActive(true);
                            cards[i].Bind(def, unlocked, completed);
                            cards[i].OnCardClicked += HandleCardClicked;
                        }
                        else
                        {
                            cards[i].gameObject.SetActive(false);
                        }
                    }
                }
            }

            // Default local selection: prioritize previously selected dungeon if unlocked, else first unlocked
            DungeonDefinition defaultToSelect = null;
            if (DungeonRunSession.HasSelection && DungeonRunSession.SelectedDungeon != null && DungeonProgression.IsDungeonUnlocked(DungeonRunSession.SelectedDungeon))
            {
                defaultToSelect = DungeonRunSession.SelectedDungeon;
            }
            else if (dungeonCatalog != null && dungeonCatalog.Count > 0)
            {
                for (int i = 0; i < dungeonCatalog.Count; i++)
                {
                    if (dungeonCatalog[i] != null && DungeonProgression.IsDungeonUnlocked(dungeonCatalog[i]))
                    {
                        defaultToSelect = dungeonCatalog[i];
                        break;
                    }
                }
            }

            if (defaultToSelect != null)
            {
                SelectDungeonLocally(defaultToSelect);
            }

            // Wire navigation buttons
            if (enterDungeonButton != null)
            {
                enterDungeonButton.onClick.RemoveListener(HandleEnterDungeonClicked);
                enterDungeonButton.onClick.AddListener(HandleEnterDungeonClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackClicked);
                backButton.onClick.AddListener(HandleBackClicked);
            }
        }

        private void UnbindEvents()
        {
            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card != null)
                    {
                        card.OnCardClicked -= HandleCardClicked;
                    }
                }
            }

            if (enterDungeonButton != null)
            {
                enterDungeonButton.onClick.RemoveListener(HandleEnterDungeonClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackClicked);
            }
        }

        /// <summary>
        /// Updates the local visual selection without committing to session carrier.
        /// </summary>
        public void SelectDungeonLocally(DungeonDefinition dungeon)
        {
            if (dungeon == null || !DungeonProgression.IsDungeonUnlocked(dungeon))
            {
                return;
            }

            selectedDungeon = dungeon;

            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card != null)
                    {
                        card.SetSelected(card.BoundDungeon == selectedDungeon);
                    }
                }
            }

            if (enterDungeonButton != null)
            {
                enterDungeonButton.interactable = true;
            }
        }

        private void HandleCardClicked(WorldMapCard card)
        {
            if (card != null && card.IsUnlocked && card.BoundDungeon != null)
            {
                SelectDungeonLocally(card.BoundDungeon);
            }
        }

        /// <summary>
        /// Commits selected dungeon into DungeonRunSession and loads its scene.
        /// </summary>
        public void HandleEnterDungeonClicked()
        {
            if (isTransitioning)
            {
                return;
            }

            if (selectedDungeon == null || !DungeonProgression.IsDungeonUnlocked(selectedDungeon))
            {
                Debug.LogWarning("[WorldMapController] Cannot enter dungeon: None selected or dungeon is locked.");
                return;
            }

            isTransitioning = true;

            if (enterDungeonButton != null)
            {
                enterDungeonButton.interactable = false;
            }

            // Commit selection to runtime session carrier
            DungeonRunSession.SetSelection(selectedDungeon);

            SceneManager.LoadScene(selectedDungeon.SceneName);
        }

        /// <summary>
        /// Returns to Character Selection screen.
        /// </summary>
        public void HandleBackClicked()
        {
            if (isTransitioning)
            {
                return;
            }

            isTransitioning = true;
            SceneManager.LoadScene("CharacterSelection");
        }

        public void SetReferences(
            DungeonCatalog catalog,
            WorldMapCard[] mapCards,
            Button enterBtn,
            Button backBtn,
            TextMeshProUGUI heroText,
            TextMeshProUGUI title)
        {
            dungeonCatalog = catalog;
            cards = mapCards;
            enterDungeonButton = enterBtn;
            backButton = backBtn;
            activeHeroText = heroText;
            titleText = title;
        }
    }
}
