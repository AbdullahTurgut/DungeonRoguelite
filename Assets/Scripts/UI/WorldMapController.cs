using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
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

        [Header("Carousel")]
        [SerializeField] private RectTransform cardsContainer;
        [SerializeField] private Button leftNavigationButton;
        [SerializeField] private Button rightNavigationButton;
        public const int VisibleCapacity = 3;
        private int focusedIndex = -1;

        public int FocusedIndex => focusedIndex;
        public int WindowStart => Mathf.Clamp(focusedIndex - 1, 0, Mathf.Max(0, (dungeonCatalog != null ? dungeonCatalog.Count : 0) - VisibleCapacity));
        public RectTransform CardsContainer => cardsContainer;
        public Button LeftNavigationButton => leftNavigationButton;
        public Button RightNavigationButton => rightNavigationButton;
        private bool IsInputBlocked => isTransitioning || (skillTreePanel != null && skillTreePanel.IsOpen);

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

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || IsInputBlocked)
            {
                return;
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame)
            {
                HandleKeyboardNavigation(Key.LeftArrow);
            }
            else if (keyboard.rightArrowKey.wasPressedThisFrame)
            {
                HandleKeyboardNavigation(Key.RightArrow);
            }
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
                            bool completed = def != null && DungeonProgression.IsDungeonCompleted(def.Id);

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

            // Initialization focuses the newest unlocked entry. Browsing never commits a run selection.
            focusedIndex = dungeonCatalog != null && dungeonCatalog.Count > 0 ? 0 : -1;
            if (dungeonCatalog != null)
            {
                for (int i = dungeonCatalog.Count - 1; i >= 0; i--)
                {
                    if (dungeonCatalog[i] != null && DungeonProgression.IsDungeonUnlocked(dungeonCatalog[i]))
                    {
                        focusedIndex = i;
                        break;
                    }
                }
            }

            RefreshCarousel();
            if (leftNavigationButton != null) leftNavigationButton.onClick.AddListener(NavigateLeft);
            if (rightNavigationButton != null) rightNavigationButton.onClick.AddListener(NavigateRight);

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
            if (leftNavigationButton != null) leftNavigationButton.onClick.RemoveListener(NavigateLeft);
            if (rightNavigationButton != null) rightNavigationButton.onClick.RemoveListener(NavigateRight);
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
            if (dungeon == null || dungeonCatalog == null || IsInputBlocked)
            {
                return;
            }

            for (int i = 0; i < dungeonCatalog.Count; i++)
            {
                if (dungeonCatalog[i] == dungeon)
                {
                    focusedIndex = i;
                    RefreshCarousel();
                    return;
                }
            }
        }

        public void NavigateLeft() => MoveFocus(-1);
        public void NavigateRight() => MoveFocus(1);

        /// <summary>
        /// Routes supported keyboard input through the same navigation methods as the carousel buttons.
        /// </summary>
        public void HandleKeyboardNavigation(Key key)
        {
            if (!isActiveAndEnabled || IsInputBlocked)
            {
                return;
            }

            if (key == Key.LeftArrow)
            {
                NavigateLeft();
            }
            else if (key == Key.RightArrow)
            {
                NavigateRight();
            }
        }

        private void MoveFocus(int direction)
        {
            if (IsInputBlocked || dungeonCatalog == null || dungeonCatalog.Count == 0) return;
            focusedIndex = Mathf.Clamp(focusedIndex + direction, 0, dungeonCatalog.Count - 1);
            RefreshCarousel();
        }

        private void HandleCardClicked(WorldMapCard card)
        {
            if (card != null && card.gameObject.activeInHierarchy && card.BoundDungeon != null)
            {
                SelectDungeonLocally(card.BoundDungeon);
            }
        }

        /// <summary>
        /// Commits selected dungeon into DungeonRunSession and loads its scene.
        /// </summary>
        public void HandleEnterDungeonClicked()
        {
            if (IsInputBlocked)
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
            if (IsInputBlocked)
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
            if (IsInputBlocked) return;
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
                Transform parentTransform = cardsContainer != null ? cardsContainer : cardList[0].transform.parent;
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

            }
            cards = cardList.ToArray();
            if (cardsContainer != null)
                foreach (var card in cards)
                    if (card.transform.parent != cardsContainer) card.transform.SetParent(cardsContainer, false);
        }

        private void RefreshCarousel()
        {
            int count = dungeonCatalog != null ? dungeonCatalog.Count : 0;
            int visibleCount = Mathf.Min(VisibleCapacity, count);
            selectedDungeon = focusedIndex >= 0 && focusedIndex < count ? dungeonCatalog[focusedIndex] : null;
            if (enterDungeonButton != null)
                enterDungeonButton.interactable = !isTransitioning && DungeonProgression.IsDungeonUnlocked(selectedDungeon);
            if (leftNavigationButton != null) leftNavigationButton.interactable = !isTransitioning && focusedIndex > 0;
            if (rightNavigationButton != null) rightNavigationButton.interactable = !isTransitioning && focusedIndex >= 0 && focusedIndex < count - 1;
            if (cards == null) return;
            for (int i = 0; i < cards.Length; ++i)
            {
                if (cards[i] == null) continue;
                bool visible = i >= WindowStart && i < WindowStart + visibleCount && cards[i].BoundDungeon != null;
                cards[i].gameObject.SetActive(visible);
                cards[i].SetSelected(i == focusedIndex && selectedDungeon != null);
                if (visible)
                {
                    var rect = cards[i].GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.sizeDelta = new Vector2(440f, 450f);
                        rect.anchoredPosition = new Vector2((i - WindowStart - (visibleCount - 1) * .5f) * 500f, 20f);
                    }
                }
            }
        }

        public void SetCarouselReferences(RectTransform container, Button left, Button right)
        {
            if (leftNavigationButton != null) leftNavigationButton.onClick.RemoveListener(NavigateLeft);
            if (rightNavigationButton != null) rightNavigationButton.onClick.RemoveListener(NavigateRight);
            cardsContainer = container;
            leftNavigationButton = left;
            rightNavigationButton = right;
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
