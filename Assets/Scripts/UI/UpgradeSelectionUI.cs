using System.Collections.Generic;
using UnityEngine;
using DungeonRoguelite.Upgrades;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Event-driven modal panel for presenting temporary upgrade choices on level-up.
    /// Manages card visibility, disables input immediately on choice to guard against double clicks,
    /// and informs UpgradeManager of selections.
    /// Strictly handles view presentation; contains zero gameplay or stat calculations.
    /// </summary>
    public class UpgradeSelectionUI : MonoBehaviour
    {
        [Header("Target Manager")]
        [Tooltip("The UpgradeManager controlling upgrade flow. Auto-resolves if unassigned.")]
        [SerializeField] private UpgradeManager upgradeManager;

        [Header("Panel Root")]
        [Tooltip("Root GameObject of the modal upgrade selection panel (toggled active/inactive).")]
        [SerializeField] private GameObject panelRoot;

        [Header("Choice Cards")]
        [Tooltip("Array of UpgradeChoiceButtons presented to the player.")]
        [SerializeField] private UpgradeChoiceButton[] choiceButtons;

        public UpgradeManager UpgradeManager => upgradeManager;
        public GameObject PanelRoot => panelRoot;
        public IReadOnlyList<UpgradeChoiceButton> ChoiceButtons => choiceButtons;

        private void Awake()
        {
            ResolveManager();
        }

        private void OnEnable()
        {
            ResolveManager();
            BindEvents();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void ResolveManager()
        {
            if (upgradeManager == null)
            {
                upgradeManager = FindFirstObjectByType<UpgradeManager>();
            }
        }

        private void BindEvents()
        {
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeChoicesRequested -= DisplayChoices;
                upgradeManager.OnUpgradeChoicesRequested += DisplayChoices;

                upgradeManager.OnUpgradeSelectionClosed -= HidePanel;
                upgradeManager.OnUpgradeSelectionClosed += HidePanel;
            }

            if (choiceButtons != null)
            {
                foreach (var btn in choiceButtons)
                {
                    if (btn != null)
                    {
                        btn.OnClicked -= HandleChoiceSelected;
                        btn.OnClicked += HandleChoiceSelected;
                    }
                }
            }
        }

        private void UnbindEvents()
        {
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeChoicesRequested -= DisplayChoices;
                upgradeManager.OnUpgradeSelectionClosed -= HidePanel;
            }

            if (choiceButtons != null)
            {
                foreach (var btn in choiceButtons)
                {
                    if (btn != null)
                    {
                        btn.OnClicked -= HandleChoiceSelected;
                    }
                }
            }
        }

        /// <summary>
        /// Displays the modal panel and populates the choice cards with the provided upgrades.
        /// </summary>
        public void DisplayChoices(UpgradeDefinition[] choices)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (choiceButtons != null)
            {
                for (int i = 0; i < choiceButtons.Length; i++)
                {
                    if (choiceButtons[i] != null)
                    {
                        if (choices != null && i < choices.Length)
                        {
                            choiceButtons[i].gameObject.SetActive(true);
                            choiceButtons[i].Bind(choices[i]);
                            choiceButtons[i].SetInteractable(true);
                        }
                        else
                        {
                            choiceButtons[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Hides the modal panel when upgrade selection finishes.
        /// </summary>
        public void HidePanel()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Handles click event from a choice card button.
        /// Locks all buttons immediately to prevent duplicate clicks and forwards selection to UpgradeManager.
        /// </summary>
        private void HandleChoiceSelected(UpgradeDefinition selected)
        {
            // Lock all buttons immediately to block rapid/duplicate clicks
            if (choiceButtons != null)
            {
                foreach (var btn in choiceButtons)
                {
                    if (btn != null)
                    {
                        btn.SetInteractable(false);
                    }
                }
            }

            if (upgradeManager != null)
            {
                upgradeManager.SelectUpgrade(selected);
            }
        }

        /// <summary>
        /// Programmatically binds references (used by automated setup and test scripts).
        /// </summary>
        public void Configure(UpgradeManager manager, GameObject root, UpgradeChoiceButton[] buttons)
        {
            UnbindEvents();
            upgradeManager = manager;
            panelRoot = root;
            choiceButtons = buttons;
            BindEvents();
        }
    }
}
