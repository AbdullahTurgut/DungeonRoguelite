using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.UI
{
    /// <summary>
    /// Displays the Dungeon Complete result screen when notified by DungeonCompletionController.
    /// Pure presentation layer; contains no combat, wave, or stat calculation logic.
    /// </summary>
    public class DungeonCompleteUI : MonoBehaviour
    {
        [Header("Controller Reference")]
        [Tooltip("The DungeonCompletionController to listen to. Auto-resolves if unassigned.")]
        [SerializeField] private DungeonCompletionController completionController;

        [Header("UI References")]
        [Tooltip("The root GameObject of the result panel overlay.")]
        [SerializeField] private GameObject panelRoot;

        [Tooltip("Text displaying the title/header.")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("Text displaying formatted elapsed run time.")]
        [SerializeField] private TextMeshProUGUI timeText;

        [Tooltip("Text displaying total enemies defeated.")]
        [SerializeField] private TextMeshProUGUI enemiesText;

        [Tooltip("Text displaying final player level reached.")]
        [SerializeField] private TextMeshProUGUI levelText;

        [Tooltip("Text displaying total cumulative XP earned.")]
        [SerializeField] private TextMeshProUGUI xpText;

        [Tooltip("Button to restart the dungeon.")]
        [SerializeField] private Button restartButton;

        [Tooltip("Button to return to the world map.")]
        [SerializeField] private Button returnToMapButton;

        public GameObject PanelRoot => panelRoot;
        public TextMeshProUGUI TitleText => titleText;
        public TextMeshProUGUI TimeText => timeText;
        public TextMeshProUGUI EnemiesText => enemiesText;
        public TextMeshProUGUI LevelText => levelText;
        public TextMeshProUGUI XPText => xpText;
        public Button RestartButton => restartButton;
        public Button ReturnToMapButton => returnToMapButton;

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

            if (completionController != null)
            {
                completionController.OnDungeonCompleted -= HandleDungeonCompleted;
                completionController.OnDungeonCompleted += HandleDungeonCompleted;
            }
        }

        private void OnDisable()
        {
            if (completionController != null)
            {
                completionController.OnDungeonCompleted -= HandleDungeonCompleted;
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
            if (completionController == null)
            {
                completionController = FindFirstObjectByType<DungeonCompletionController>();
            }
        }

        /// <summary>
        /// Populates the UI labels with the finalized summary and displays the modal panel.
        /// </summary>
        public void HandleDungeonCompleted(DungeonRunSummary summary)
        {
            if (returnToMapButton != null && completionController != null && completionController.OffersBlacksmithIntro)
            {
                var label = returnToMapButton.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = "DEMİRCİYE GİT";
            }
            if (timeText != null)
            {
                timeText.text = $"Time: {summary.FormattedTime}";
            }

            if (enemiesText != null)
            {
                enemiesText.text = $"Enemies Defeated: {summary.EnemiesDefeated}";
            }

            if (levelText != null)
            {
                levelText.text = $"Level Reached: {summary.FinalLevel}";
            }

            if (xpText != null)
            {
                xpText.text = $"XP Earned: {summary.TotalXPEarned}";
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void OnRestartButtonClicked()
        {
            if (completionController == null)
            {
                ResolveReferences();
            }

            if (completionController != null)
            {
                completionController.RestartDungeon();
            }
        }

        private void OnReturnToMapButtonClicked()
        {
            if (completionController == null)
            {
                ResolveReferences();
            }

            if (completionController != null)
            {
                completionController.ReturnToWorldMap();
            }
        }

        public void SetReferences(
            DungeonCompletionController controller,
            GameObject root,
            TextMeshProUGUI title,
            TextMeshProUGUI time,
            TextMeshProUGUI enemies,
            TextMeshProUGUI level,
            TextMeshProUGUI xp,
            Button restartBtn,
            Button returnBtn = null)
        {
            completionController = controller;
            panelRoot = root;
            titleText = title;
            timeText = time;
            enemiesText = enemies;
            levelText = level;
            xpText = xp;
            restartButton = restartBtn;
            returnToMapButton = returnBtn;
        }
    }
}
