using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Progression;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Screen overlay controller for permanent skill tree purchases on the World Map.
    /// Presents character-specific skill tree branches, tier progression, available points, and purchase buttons.
    /// Delegates all purchase validation and state mutation to PermanentProgression.
    /// Strictly decoupled from active combat and player controllers.
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("Panel Root")]
        [Tooltip("Root GameObject of the skill tree overlay (toggled active/inactive).")]
        [SerializeField] private GameObject panelRoot;

        [Header("Header References")]
        [Tooltip("Text label displaying the currently selected character display name.")]
        [SerializeField] private TextMeshProUGUI characterNameText;

        [Tooltip("Text label displaying available permanent skill points.")]
        [SerializeField] private TextMeshProUGUI availablePointsText;

        [Tooltip("Button to close the skill tree overlay.")]
        [SerializeField] private Button closeButton;

        [Header("Node Cards")]
        [Tooltip("Array of 9 SkillTreeNodeUI cards (3 branches x 3 tiers).")]
        [SerializeField] private SkillTreeNodeUI[] nodeUIs;

        [Header("Branch Header Texts (Optional)")]
        [Tooltip("Optional labels for the 3 branches.")]
        [SerializeField] private TextMeshProUGUI[] branchTitleTexts;

        [Header("Fallback Character Reference")]
        [Tooltip("Fallback CharacterDefinition if no character is selected in session.")]
        [SerializeField] private CharacterDefinition defaultCharacter;

        private CharacterDefinition activeCharacter;
        private SkillTreeDefinition activeTree;

        public GameObject PanelRoot => panelRoot;
        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;
        public TextMeshProUGUI CharacterNameText => characterNameText;
        public TextMeshProUGUI AvailablePointsText => availablePointsText;
        public Button CloseButton => closeButton;
        public IReadOnlyList<SkillTreeNodeUI> NodeUIs => nodeUIs;
        public CharacterDefinition ActiveCharacter => activeCharacter;
        public SkillTreeDefinition ActiveTree => activeTree;

        private void Awake()
        {
            ResolveReferences();
            BindCloseButton();
        }

        private void OnEnable()
        {
            BindProgressionEvents();
        }

        private void OnDisable()
        {
            UnbindProgressionEvents();
        }

        private void OnDestroy()
        {
            UnbindCloseButton();
            UnbindProgressionEvents();
            UnbindNodeEvents();
        }

        private void ResolveReferences()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (nodeUIs == null || nodeUIs.Length == 0)
            {
                nodeUIs = GetComponentsInChildren<SkillTreeNodeUI>(true);
            }

            if (defaultCharacter == null)
            {
#if UNITY_EDITOR
                defaultCharacter = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                    "Assets/ScriptableObjects/Characters/Character_Warrior.asset");
#endif
            }

            BindNodeEvents();
        }

        private void BindCloseButton()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }
        }

        private void UnbindCloseButton()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }

        private void BindProgressionEvents()
        {
            PermanentProgression.OnSkillPurchased -= HandleSkillPurchasedExternally;
            PermanentProgression.OnSkillPurchased += HandleSkillPurchasedExternally;

            PermanentProgression.OnSkillPointsChanged -= HandlePointsChangedExternally;
            PermanentProgression.OnSkillPointsChanged += HandlePointsChangedExternally;
        }

        private void UnbindProgressionEvents()
        {
            PermanentProgression.OnSkillPurchased -= HandleSkillPurchasedExternally;
            PermanentProgression.OnSkillPointsChanged -= HandlePointsChangedExternally;
        }

        private void BindNodeEvents()
        {
            if (nodeUIs != null)
            {
                for (int i = 0; i < nodeUIs.Length; i++)
                {
                    if (nodeUIs[i] != null)
                    {
                        nodeUIs[i].OnPurchaseRequested -= HandleNodePurchaseRequested;
                        nodeUIs[i].OnPurchaseRequested += HandleNodePurchaseRequested;
                    }
                }
            }
        }

        private void UnbindNodeEvents()
        {
            if (nodeUIs != null)
            {
                for (int i = 0; i < nodeUIs.Length; i++)
                {
                    if (nodeUIs[i] != null)
                    {
                        nodeUIs[i].OnPurchaseRequested -= HandleNodePurchaseRequested;
                    }
                }
            }
        }

        /// <summary>
        /// Opens the permanent skill tree panel for the specified character archetype.
        /// If character is null, resolves from CharacterSelectionSession or defaultCharacter.
        /// </summary>
        public void Open(CharacterDefinition character = null)
        {
            ResolveReferences();

            if (character == null)
            {
                if (CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null)
                {
                    character = CharacterSelectionSession.SelectedCharacter;
                }
                else if (defaultCharacter != null)
                {
                    character = defaultCharacter;
                }
#if UNITY_EDITOR
                else
                {
                    character = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                        "Assets/ScriptableObjects/Characters/Character_Warrior.asset");
                }
#endif
            }

            activeCharacter = character;
            activeTree = character != null ? character.SkillTree : null;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                // World Map cards can be cloned after this panel at runtime.
                // Keep the modal above every map child whenever it is opened.
                panelRoot.transform.SetAsLastSibling();
            }

            RefreshUI();
        }

        /// <summary>
        /// Closes the skill tree overlay without altering session or navigation state.
        /// </summary>
        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Re-evaluates node states and updates all text and visual elements.
        /// </summary>
        public void RefreshUI()
        {
            if (activeCharacter == null)
            {
                if (characterNameText != null) characterNameText.text = "Kahraman Yok";
                if (availablePointsText != null) availablePointsText.text = "Yetenek Puanı: 0";
                return;
            }

            string charId = activeCharacter.Id;
            int points = PermanentProgression.GetAvailablePoints(charId);

            if (characterNameText != null)
            {
                characterNameText.text = $"{activeCharacter.DisplayName} - Yetenek Ağacı";
            }

            if (availablePointsText != null)
            {
                availablePointsText.text = $"Yetenek Puanı: {points}";
            }

            if (activeTree == null && activeCharacter != null)
            {
                activeTree = activeCharacter.SkillTree;
            }

            var nodes = activeTree != null ? activeTree.Nodes : null;

            if (nodeUIs != null)
            {
                for (int i = 0; i < nodeUIs.Length; i++)
                {
                    if (nodeUIs[i] == null) continue;

                    if (nodes != null && i < nodes.Count)
                    {
                        var nodeDef = nodes[i];
                        SkillNodeUIState state = EvaluateNodeState(charId, nodeDef, points);
                        nodeUIs[i].Bind(nodeDef, state);
                    }
                    else
                    {
                        nodeUIs[i].gameObject.SetActive(false);
                    }
                }
            }

            UpdateBranchTitles();
        }

        private SkillNodeUIState EvaluateNodeState(string charId, SkillNodeDefinition node, int availablePoints)
        {
            if (node == null) return SkillNodeUIState.Locked;

            if (PermanentProgression.IsNodePurchased(charId, node.Id))
            {
                return SkillNodeUIState.Purchased;
            }

            if (node.HasPrerequisite && !PermanentProgression.IsNodePurchased(charId, node.PrerequisiteNodeId))
            {
                return SkillNodeUIState.Locked;
            }

            if (availablePoints < node.Cost)
            {
                return SkillNodeUIState.InsufficientPoints;
            }

            return SkillNodeUIState.Available;
        }

        private void UpdateBranchTitles()
        {
            if (branchTitleTexts == null || branchTitleTexts.Length < 3 || activeTree == null || activeTree.Nodes == null)
            {
                return;
            }

            var seenBranches = new List<string>();
            for (int i = 0; i < activeTree.Nodes.Count; i++)
            {
                string b = activeTree.Nodes[i]?.BranchId;
                if (!string.IsNullOrEmpty(b) && !seenBranches.Contains(b))
                {
                    seenBranches.Add(b);
                }
            }

            for (int b = 0; b < branchTitleTexts.Length; b++)
            {
                if (branchTitleTexts[b] == null) continue;
                if (b < seenBranches.Count)
                {
                    branchTitleTexts[b].text = FormatBranchDisplayName(seenBranches[b]);
                }
            }
        }

        private string FormatBranchDisplayName(string branchId)
        {
            switch (branchId.ToLowerInvariant())
            {
                case "durability": return "Dayanıklılık";
                case "power": return "Güç";
                case "tempo": return "Çeviklik";
                case "precision": return "Hassasiyet";
                case "survival": return "Hayatta Kalma";
                case "firepower": return "Ateş Gücü";
                case "cadence": return "Atış Hızı";
                case "handling": return "Manevra";
                default: return branchId.ToUpperInvariant();
            }
        }

        private void HandleNodePurchaseRequested(SkillNodeDefinition node)
        {
            if (activeCharacter == null || node == null || activeTree == null) return;

            bool success = PermanentProgression.TryPurchaseNode(activeCharacter.Id, node, activeTree);
            if (success)
            {
                RefreshUI();
            }
        }

        private void HandleSkillPurchasedExternally(string charId, string nodeId)
        {
            if (IsOpen && activeCharacter != null && string.Equals(charId, activeCharacter.Id, StringComparison.OrdinalIgnoreCase))
            {
                RefreshUI();
            }
        }

        private void HandlePointsChangedExternally(string charId, int newPoints)
        {
            if (IsOpen && activeCharacter != null && string.Equals(charId, activeCharacter.Id, StringComparison.OrdinalIgnoreCase))
            {
                RefreshUI();
            }
        }

        /// <summary>
        /// Programmatic setter for automated test runners and scene setup scripts.
        /// </summary>
        public void SetReferences(
            GameObject root,
            TextMeshProUGUI heroTMP,
            TextMeshProUGUI pointsTMP,
            Button closeBtn,
            SkillTreeNodeUI[] cards,
            TextMeshProUGUI[] branchTMPs = null,
            CharacterDefinition defaultHero = null)
        {
            panelRoot = root;
            characterNameText = heroTMP;
            availablePointsText = pointsTMP;
            closeButton = closeBtn;
            nodeUIs = cards;
            branchTitleTexts = branchTMPs;
            defaultCharacter = defaultHero;

            BindCloseButton();
            BindNodeEvents();
        }
    }
}
