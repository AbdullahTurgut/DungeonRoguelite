using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Tests;
using DungeonRoguelite.UI;
using DungeonRoguelite.Upgrades;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Editor
{
    public static class Milestone7_1_Setup
    {
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 7.1")]
        public static void Setup()
        {
            Debug.Log("[Milestone 7.1] Setting up DungeonRunStats, DungeonCompletionController, and DungeonCompleteUI...");
            UpdatePrototypeScene(includeVerifier: false);
            Debug.Log("[Milestone 7.1] Setup completed successfully. Clean scene saved without verifier.");
        }

        public static void UpdatePrototypeScene(bool includeVerifier = false)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Clean up any legacy or previous verifiers
            string[] verifiersToClean = new string[]
            {
                "Milestone1_1_RuntimeVerifier",
                "Milestone1_2_RuntimeVerifier",
                "Milestone1_3_RuntimeVerifier",
                "Milestone2_1_RuntimeVerifier",
                "Milestone2_2_RuntimeVerifier",
                "Milestone3_1_RuntimeVerifier",
                "Milestone4_1_RuntimeVerifier",
                "Milestone5_1_RuntimeVerifier",
                "Milestone6_1_RuntimeVerifier",
                "Milestone7_1_RuntimeVerifier"
            };

            foreach (var verifierName in verifiersToClean)
            {
                var oldGo = GameObject.Find(verifierName);
                if (oldGo != null)
                {
                    Object.DestroyImmediate(oldGo);
                }
            }

            var playerGo = GameObject.FindWithTag("Player");
            PlayerExperience playerExp = null;
            if (playerGo != null)
            {
                playerExp = playerGo.GetComponent<PlayerExperience>();
            }

            var waveManagerGo = GameObject.Find("WaveManager");
            WaveManager waveManager = null;
            if (waveManagerGo != null)
            {
                waveManager = waveManagerGo.GetComponent<WaveManager>();
            }

            var upgradeManagerGo = GameObject.Find("UpgradeManager");
            UpgradeManager upgradeManager = null;
            if (upgradeManagerGo != null)
            {
                upgradeManager = upgradeManagerGo.GetComponent<UpgradeManager>();
            }

            // Setup DungeonRunStats and DungeonCompletionController on RunControllers GameObject
            var runControllersGo = GameObject.Find("RunControllers");
            if (runControllersGo == null)
            {
                runControllersGo = new GameObject("RunControllers");
            }

            var runStats = runControllersGo.GetComponent<DungeonRunStats>();
            if (runStats == null) runStats = runControllersGo.AddComponent<DungeonRunStats>();
            runStats.SetReferences(waveManager, playerExp);

            var completionController = runControllersGo.GetComponent<DungeonCompletionController>();
            if (completionController == null) completionController = runControllersGo.AddComponent<DungeonCompletionController>();
            completionController.SetReferences(waveManager, upgradeManager, runStats, playerExp);

            // Serialize properties on RunControllers
            var sRunStats = new SerializedObject(runStats);
            sRunStats.FindProperty("waveManager").objectReferenceValue = waveManager;
            sRunStats.FindProperty("playerExperience").objectReferenceValue = playerExp;
            sRunStats.ApplyModifiedProperties();

            var sCompletion = new SerializedObject(completionController);
            sCompletion.FindProperty("waveManager").objectReferenceValue = waveManager;
            sCompletion.FindProperty("upgradeManager").objectReferenceValue = upgradeManager;
            sCompletion.FindProperty("runStats").objectReferenceValue = runStats;
            sCompletion.FindProperty("playerExperience").objectReferenceValue = playerExp;
            sCompletion.ApplyModifiedProperties();

            // Setup Canvas and DungeonCompletePanel
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Canvas");
                var c = canvasGo.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                var cs = canvasGo.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // DungeonCompletePanel (Modal Overlay)
            var panelTransform = canvasGo.transform.Find("DungeonCompletePanel");
            GameObject panelGo = panelTransform != null ? panelTransform.gameObject : new GameObject("DungeonCompletePanel");
            panelGo.transform.SetParent(canvasGo.transform, false);

            var panelRect = panelGo.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            var panelBg = panelGo.GetComponent<Image>();
            if (panelBg == null) panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.04f, 0.04f, 0.07f, 0.92f); // Dark modal backdrop

            // Title Text
            var titleTransform = panelGo.transform.Find("Title");
            GameObject titleGo = titleTransform != null ? titleTransform.gameObject : new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRect = titleGo.GetComponent<RectTransform>();
            if (titleRect == null) titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.78f);
            titleRect.anchorMax = new Vector2(0.5f, 0.78f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800f, 60f);

            var titleTMP = titleGo.GetComponent<TextMeshProUGUI>();
            if (titleTMP == null) titleTMP = titleGo.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "DUNGEON CLEARED";
            titleTMP.fontSize = 44f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(1f, 0.85f, 0.2f, 1f); // Gold

            // Stats Container
            var statsContainerTransform = panelGo.transform.Find("StatsContainer");
            GameObject statsContainerGo = statsContainerTransform != null ? statsContainerTransform.gameObject : new GameObject("StatsContainer");
            statsContainerGo.transform.SetParent(panelGo.transform, false);
            var statsRect = statsContainerGo.GetComponent<RectTransform>();
            if (statsRect == null) statsRect = statsContainerGo.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 0.50f);
            statsRect.anchorMax = new Vector2(0.5f, 0.50f);
            statsRect.pivot = new Vector2(0.5f, 0.5f);
            statsRect.anchoredPosition = Vector2.zero;
            statsRect.sizeDelta = new Vector2(600f, 220f);

            // Sub-stat texts
            TextMeshProUGUI timeTMP = CreateOrGetStatText(statsContainerGo, "TimeText", "Time: 00:00", new Vector2(0f, 75f));
            TextMeshProUGUI enemiesTMP = CreateOrGetStatText(statsContainerGo, "EnemiesText", "Enemies Defeated: 0", new Vector2(0f, 25f));
            TextMeshProUGUI levelTMP = CreateOrGetStatText(statsContainerGo, "LevelText", "Level Reached: 1", new Vector2(0f, -25f));
            TextMeshProUGUI xpTMP = CreateOrGetStatText(statsContainerGo, "XPText", "XP Earned: 0", new Vector2(0f, -75f));

            // Restart Button
            var buttonTransform = panelGo.transform.Find("RestartButton");
            GameObject btnGo = buttonTransform != null ? buttonTransform.gameObject : new GameObject("RestartButton");
            btnGo.transform.SetParent(panelGo.transform, false);
            var btnRect = btnGo.GetComponent<RectTransform>();
            if (btnRect == null) btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.22f);
            btnRect.anchorMax = new Vector2(0.5f, 0.22f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.sizeDelta = new Vector2(280f, 60f);

            var btnBg = btnGo.GetComponent<Image>();
            if (btnBg == null) btnBg = btnGo.AddComponent<Image>();
            btnBg.color = new Color(0.12f, 0.45f, 0.55f, 1f); // Teal/cyan button

            var btn = btnGo.GetComponent<Button>();
            if (btn == null) btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            // Button label
            var btnLabelTransform = btnGo.transform.Find("Text");
            GameObject btnLabelGo = btnLabelTransform != null ? btnLabelTransform.gameObject : new GameObject("Text");
            btnLabelGo.transform.SetParent(btnGo.transform, false);
            var labelRect = btnLabelGo.GetComponent<RectTransform>();
            if (labelRect == null) labelRect = btnLabelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;

            var btnTMP = btnLabelGo.GetComponent<TextMeshProUGUI>();
            if (btnTMP == null) btnTMP = btnLabelGo.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "Restart Dungeon";
            btnTMP.fontSize = 22f;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.alignment = TextAlignmentOptions.Center;
            btnTMP.color = Color.white;

            // Setup DungeonCompleteUI on Canvas
            var completeUI = canvasGo.GetComponent<DungeonCompleteUI>();
            if (completeUI == null) completeUI = canvasGo.AddComponent<DungeonCompleteUI>();
            completeUI.SetReferences(completionController, panelGo, titleTMP, timeTMP, enemiesTMP, levelTMP, xpTMP, btn);

            var sUI = new SerializedObject(completeUI);
            sUI.FindProperty("completionController").objectReferenceValue = completionController;
            sUI.FindProperty("panelRoot").objectReferenceValue = panelGo;
            sUI.FindProperty("titleText").objectReferenceValue = titleTMP;
            sUI.FindProperty("timeText").objectReferenceValue = timeTMP;
            sUI.FindProperty("enemiesText").objectReferenceValue = enemiesTMP;
            sUI.FindProperty("levelText").objectReferenceValue = levelTMP;
            sUI.FindProperty("xpText").objectReferenceValue = xpTMP;
            sUI.FindProperty("restartButton").objectReferenceValue = btn;
            sUI.ApplyModifiedProperties();

            // Result panel is hidden by default during active gameplay
            panelGo.SetActive(false);

            // Optional: attach verifier only if requested for test run
            if (includeVerifier)
            {
                var verifierGo = new GameObject("Milestone7_1_RuntimeVerifier");
                verifierGo.AddComponent<Milestone7_1_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 7.1] Scene saved at {ScenePath}. Panel hidden: {panelGo.activeSelf == false}. Verifier attached: {includeVerifier}.");
        }

        private static TextMeshProUGUI CreateOrGetStatText(GameObject container, string name, string defaultText, Vector2 pos)
        {
            var t = container.transform.Find(name);
            GameObject go = t != null ? t.gameObject : new GameObject(name);
            go.transform.SetParent(container.transform, false);

            var rect = go.GetComponent<RectTransform>();
            if (rect == null) rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(500f, 40f);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = 22f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }

        [MenuItem("DungeonRoguelite/Run Milestone 7.1 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 7.1] Launching PlayMode verification harness...");
            UpdatePrototypeScene(includeVerifier: true);
            EditorApplication.EnterPlaymode();
        }

        public static void CleanupVerifierFromScene()
        {
            UpdatePrototypeScene(includeVerifier: false);
            Debug.Log("[Milestone 7.1] Cleaned up verifier from prototype scene.");
        }
    }
}
