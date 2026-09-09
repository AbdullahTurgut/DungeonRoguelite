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

        [Header("Skill Tree UI")]
        [Tooltip("Button to open the permanent skill tree overlay.")]
        [SerializeField] private Button skillTreeButton;

        [Tooltip("Overlay controller for the permanent skill tree.")]
        [SerializeField] private SkillTreeUI skillTreePanel;

        private DungeonDefinition selectedDungeon;
        private bool isTransitioning = false;

        public DungeonCatalog DungeonCatalog => dungeonCatalog;
        public IReadOnlyList<WorldMapCard> Cards => cards;
        public DungeonDefinition SelectedDungeon => selectedDungeon;
        public Button EnterDungeonButton => enterDungeonButton;
        public Button BackButton => backButton;
        public TextMeshProUGUI ActiveHeroText => activeHeroText;
        public TextMeshProUGUI TitleText => titleText;
        public Button SkillTreeButton => skillTreeButton;
        public SkillTreeUI SkillTreePanel => skillTreePanel;

        private void Awake()
        {
            ResolveReferences();
            EnsureCardsMatchCatalog();
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

            if (skillTreePanel == null)
            {
                skillTreePanel = GetComponentInChildren<SkillTreeUI>(true);
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

            // Ensure card components match catalog count
            EnsureCardsMatchCatalog();

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

            // Reposition cards in a clean, readable horizontal row
            ApplyHorizontalCardLayout();

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

            if (skillTreeButton != null)
            {
                skillTreeButton.onClick.RemoveListener(HandleSkillTreeClicked);
                skillTreeButton.onClick.AddListener(HandleSkillTreeClicked);
            }

            if (skillTreePanel != null && skillTreePanel.IsOpen)
            {
                skillTreePanel.Close();
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

            if (skillTreeButton != null)
            {
                skillTreeButton.onClick.RemoveListener(HandleSkillTreeClicked);
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

        /// <summary>
        /// Opens the permanent skill tree overlay for the active character without leaving the World Map.
        /// </summary>
        public void HandleSkillTreeClicked()
        {
            if (skillTreePanel != null)
            {
                CharacterDefinition hero = CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null
                    ? CharacterSelectionSession.SelectedCharacter
                    : null;
                skillTreePanel.Open(hero);
            }
        }

        private void EnsureCardsMatchCatalog()
        {
            if (dungeonCatalog == null || dungeonCatalog.Count == 0) return;

            var cardList = new List<WorldMapCard>();
            if (cards != null)
            {
                foreach (var c in cards)
                {
                    if (c != null) cardList.Add(c);
                }
            }

            if (cardList.Count == 0)
            {
                var foundCards = GetComponentsInChildren<WorldMapCard>(true);
                if (foundCards != null)
                {
                    cardList.AddRange(foundCards);
                }
            }

            if (cardList.Count > 0 && cardList.Count < dungeonCatalog.Count)
            {
                Transform parentTransform = cardList[0].transform.parent;
                WorldMapCard template = cardList[0];

                for (int i = cardList.Count; i < dungeonCatalog.Count; i++)
                {
                    GameObject cloneGo = Instantiate(template.gameObject, parentTransform, false);
                    cloneGo.name = $"Card_Dungeon{(i + 1):D2}";
                    WorldMapCard cloneCard = cloneGo.GetComponent<WorldMapCard>();
                    if (cloneCard != null)
                    {
                        cloneCard.SetSelected(false);
                        cardList.Add(cloneCard);
                    }
                }

                cards = cardList.ToArray();
            }
        }

        private void ApplyHorizontalCardLayout()
        {
            if (cards == null || cards.Length == 0) return;

            int activeCount = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null && cards[i].gameObject.activeSelf)
                {
                    activeCount++;
                }
            }

            if (activeCount == 0) return;

            // Spacing calculations for 1920 reference resolution (scaled cleanly to 1280x720)
            // For 3 cards: width = 440, spacing = 500, positions: -500, 0, +500
            // For 2 cards: width = 460, spacing = 560, positions: -280, +280
            // For 1 card: width = 460, position: 0
            float cardWidth = activeCount >= 3 ? 440f : 460f;
            float spacing = activeCount >= 3 ? 500f : (activeCount == 2 ? 560f : 0f);
            float startX = -((activeCount - 1) * spacing) / 2f;

            int currentIndex = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null && cards[i].gameObject.activeSelf)
                {
                    var rect = cards[i].GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.sizeDelta = new Vector2(cardWidth, 450f);
                        rect.anchoredPosition = new Vector2(startX + currentIndex * spacing, 30f);
                    }
                    currentIndex++;
                }
            }
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

        public void SetReferences(
            DungeonCatalog catalog,
            WorldMapCard[] mapCards,
            Button enterBtn,
            Button backBtn,
            TextMeshProUGUI heroText,
            TextMeshProUGUI title,
            Button skillBtn,
            SkillTreeUI skillPanel)
        {
            dungeonCatalog = catalog;
            cards = mapCards;
            enterDungeonButton = enterBtn;
            backButton = backBtn;
            activeHeroText = heroText;
            titleText = title;
            skillTreeButton = skillBtn;
            skillTreePanel = skillPanel;
        }
    }
}
