using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Displays the Player Defeat modal screen when notified by PlayerDefeatController.
    /// Pure presentation layer; contains zero combat or wave progression logic.
    /// </summary>
    public class PlayerDefeatUI : MonoBehaviour
    {
        [Header("Controller Reference")]
        [Tooltip("The PlayerDefeatController to listen to. Auto-resolves if unassigned.")]
        [SerializeField] private PlayerDefeatController defeatController;

        [Header("UI References")]
        [Tooltip("The root GameObject of the defeat modal overlay.")]
        [SerializeField] private GameObject panelRoot;

        [Tooltip("Text displaying the defeat title ('YENİLDİN').")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("Button to restart the current dungeon.")]
        [SerializeField] private Button restartButton;

        [Tooltip("Button to return to the world map.")]
        [SerializeField] private Button returnToMapButton;

        public GameObject PanelRoot => panelRoot;
        public TextMeshProUGUI TitleText => titleText;
        public Button RestartButton => restartButton;
        public Button ReturnToMapButton => returnToMapButton;
        public PlayerDefeatController DefeatController => defeatController;

        private void Awake()
        {
            ResolveReferences();

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (returnToMapButton != null)
            {
                returnToMapButton.onClick.RemoveListener(OnReturnToMapButtonClicked);
                returnToMapButton.onClick.AddListener(OnReturnToMapButtonClicked);
            }
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (defeatController != null)
            {
                defeatController.OnPlayerDefeated -= HandlePlayerDefeated;
                defeatController.OnPlayerDefeated += HandlePlayerDefeated;
            }
        }

        private void OnDisable()
        {
            if (defeatController != null)
            {
                defeatController.OnPlayerDefeated -= HandlePlayerDefeated;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }

            if (returnToMapButton != null)
            {
                returnToMapButton.onClick.RemoveListener(OnReturnToMapButtonClicked);
            }
        }

        private void ResolveReferences()
        {
            if (defeatController == null)
            {
                defeatController = FindFirstObjectByType<PlayerDefeatController>();
            }
        }

        /// <summary>
        /// Handles the defeat notification from PlayerDefeatController and reveals the modal panel.
        /// </summary>
        public void HandlePlayerDefeated()
        {
            if (titleText != null)
            {
                titleText.text = "YENİLDİN";
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void OnRestartButtonClicked()
        {
            if (defeatController == null)
            {
                ResolveReferences();
            }

            if (defeatController != null)
            {
                defeatController.RestartDungeon();
            }
        }

        private void OnReturnToMapButtonClicked()
        {
            if (defeatController == null)
            {
                ResolveReferences();
            }

            if (defeatController != null)
            {
                defeatController.ReturnToWorldMap();
            }
        }

        public void SetReferences(
            PlayerDefeatController controller,
            GameObject root,
            TextMeshProUGUI title,
            Button restartBtn,
            Button returnBtn)
        {
            defeatController = controller;
            panelRoot = root;
            titleText = title;
            restartButton = restartBtn;
            returnToMapButton = returnBtn;
        }
    }
}
