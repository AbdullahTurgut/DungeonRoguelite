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

        [Header("Explanatory Subtitles")]
        [Tooltip("Text explaining the restart/rollback action.")]
        [SerializeField] private TextMeshProUGUI restartSubtitleText;

        [Tooltip("Text explaining the return to map/end run action.")]
        [SerializeField] private TextMeshProUGUI returnToMapSubtitleText;

        public const string RestartExplanation = "Zindana girişteki gelişiminle yeniden başla.";
        public const string ReturnToMapExplanation = "Koşuyu sonlandır ve haritaya dön.";

        public GameObject PanelRoot => panelRoot;
        public TextMeshProUGUI TitleText => titleText;
        public Button RestartButton => restartButton;
        public Button ReturnToMapButton => returnToMapButton;
        public TextMeshProUGUI RestartSubtitleText => restartSubtitleText;
        public TextMeshProUGUI ReturnToMapSubtitleText => returnToMapSubtitleText;
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

            EnsureSubtitleTexts();

            if (restartSubtitleText != null)
            {
                restartSubtitleText.text = RestartExplanation;
            }

            if (returnToMapSubtitleText != null)
            {
                returnToMapSubtitleText.text = ReturnToMapExplanation;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void EnsureSubtitleTexts()
        {
            if (restartSubtitleText == null && restartButton != null)
            {
                var existing = restartButton.transform.Find("RestartSubtitle");
                if (existing != null)
                {
                    restartSubtitleText = existing.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    var subtitleGo = new GameObject("RestartSubtitle");
                    subtitleGo.transform.SetParent(restartButton.transform, false);
                    var rect = subtitleGo.AddComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, -0.6f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.anchoredPosition = new Vector2(0f, -12f);
                    rect.sizeDelta = new Vector2(100f, 25f);
                    var tmp = subtitleGo.AddComponent<TextMeshProUGUI>();
                    tmp.fontSize = 12f;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
                    restartSubtitleText = tmp;
                }
            }

            if (returnToMapSubtitleText == null && returnToMapButton != null)
            {
                var existing = returnToMapButton.transform.Find("ReturnSubtitle");
                if (existing != null)
                {
                    returnToMapSubtitleText = existing.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    var subtitleGo = new GameObject("ReturnSubtitle");
                    subtitleGo.transform.SetParent(returnToMapButton.transform, false);
                    var rect = subtitleGo.AddComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, -0.6f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.anchoredPosition = new Vector2(0f, -12f);
                    rect.sizeDelta = new Vector2(100f, 25f);
                    var tmp = subtitleGo.AddComponent<TextMeshProUGUI>();
                    tmp.fontSize = 12f;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
                    returnToMapSubtitleText = tmp;
                }
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
            Button returnBtn,
            TextMeshProUGUI restartSubtitle = null,
            TextMeshProUGUI returnSubtitle = null)
        {
            defeatController = controller;
            panelRoot = root;
            titleText = title;
            restartButton = restartBtn;
            returnToMapButton = returnBtn;
            restartSubtitleText = restartSubtitle;
            returnToMapSubtitleText = returnSubtitle;
        }
    }
}
