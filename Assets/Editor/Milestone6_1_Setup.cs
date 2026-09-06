using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Tests;
using DungeonRoguelite.UI;
using DungeonRoguelite.Upgrades;

namespace DungeonRoguelite.Editor
{
    public static class Milestone6_1_Setup
    {
        private const string UpgradesDir = "Assets/ScriptableObjects/Upgrades";
        private const string DamageUpgradePath = "Assets/ScriptableObjects/Upgrades/Upgrade_Damage.asset";
        private const string AttackSpeedUpgradePath = "Assets/ScriptableObjects/Upgrades/Upgrade_AttackSpeed.asset";
        private const string MovementSpeedUpgradePath = "Assets/ScriptableObjects/Upgrades/Upgrade_MovementSpeed.asset";
        private const string PlayerPrefabPath = "Assets/Prefabs/Characters/Warrior.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 6.1")]
        public static void Setup()
        {
            Debug.Log("[Milestone 6.1] Setting up Upgrade assets, PlayerStats on prefab, and prototype scene UI...");
            CreateOrUpdateUpgradeAssets();
            UpdatePlayerPrefab();
            UpdatePrototypeScene(includeVerifier: false);
            Debug.Log("[Milestone 6.1] Setup completed successfully. Clean scene saved without verifier.");
        }

        public static void CreateOrUpdateUpgradeAssets()
        {
            if (!Directory.Exists(UpgradesDir))
            {
                Directory.CreateDirectory(UpgradesDir);
                AssetDatabase.Refresh();
            }

            CreateOrUpdateUpgradeAsset(DamageUpgradePath, "upgrade_damage", "Damage +20%", "Increases melee attack damage by 20%.", UpgradeType.Damage, 0.20f);
            CreateOrUpdateUpgradeAsset(AttackSpeedUpgradePath, "upgrade_attack_speed", "Attack Speed +15%", "Increases attack speed by 15% (reduces attack cooldown).", UpgradeType.AttackSpeed, 0.15f);
            CreateOrUpdateUpgradeAsset(MovementSpeedUpgradePath, "upgrade_movement_speed", "Movement Speed +10%", "Increases movement speed by 10%.", UpgradeType.MovementSpeed, 0.10f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateOrUpdateUpgradeAsset(string path, string id, string displayName, string desc, UpgradeType type, float mag)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
            bool isNew = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<UpgradeDefinition>();
                isNew = true;
            }

            asset.Initialize(id, displayName, desc, type, mag);
            EditorUtility.SetDirty(asset);

            if (isNew)
            {
                AssetDatabase.CreateAsset(asset, path);
            }

            Debug.Log($"[Milestone 6.1] Configured upgrade asset: {path} ({displayName})");
        }

        public static void UpdatePlayerPrefab()
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"[Milestone 6.1] Player prefab not found at {PlayerPrefabPath}");
                return;
            }

            GameObject playerRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            var playerStats = playerRoot.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = playerRoot.AddComponent<PlayerStats>();
            }

            PrefabUtility.SaveAsPrefabAsset(playerRoot, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(playerRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Milestone 6.1] Player prefab updated with PlayerStats at {PlayerPrefabPath}.");
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
                "Milestone6_1_RuntimeVerifier"
            };

            foreach (var verifierName in verifiersToClean)
            {
                var oldGo = GameObject.Find(verifierName);
                if (oldGo != null)
                {
                    Object.DestroyImmediate(oldGo);
                }
            }

            // Ensure Player instance in scene has PlayerStats
            var playerGo = GameObject.FindWithTag("Player");
            PlayerStats playerStats = null;
            PlayerExperience playerExp = null;
            if (playerGo != null)
            {
                playerStats = playerGo.GetComponent<PlayerStats>();
                if (playerStats == null) playerStats = playerGo.AddComponent<PlayerStats>();

                playerExp = playerGo.GetComponent<PlayerExperience>();
                if (playerExp == null) playerExp = playerGo.AddComponent<PlayerExperience>();
            }
            else
            {
                Debug.LogError("[Milestone 6.1] Player GameObject with tag 'Player' not found in scene.");
            }

            // Load upgrade definitions
            var damageUpgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(DamageUpgradePath);
            var attackSpeedUpgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(AttackSpeedUpgradePath);
            var movementSpeedUpgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(MovementSpeedUpgradePath);
            var upgradePool = new UpgradeDefinition[] { damageUpgrade, attackSpeedUpgrade, movementSpeedUpgrade };

            // Setup UpgradeManager GameObject
            var upgradeManagerGo = GameObject.Find("UpgradeManager");
            if (upgradeManagerGo == null)
            {
                upgradeManagerGo = new GameObject("UpgradeManager");
            }

            var upgradeManager = upgradeManagerGo.GetComponent<UpgradeManager>();
            if (upgradeManager == null) upgradeManager = upgradeManagerGo.AddComponent<UpgradeManager>();

            var sUpgradeManager = new SerializedObject(upgradeManager);
            if (playerExp != null) sUpgradeManager.FindProperty("playerExperience").objectReferenceValue = playerExp;
            if (playerStats != null) sUpgradeManager.FindProperty("playerStats").objectReferenceValue = playerStats;

            var poolProp = sUpgradeManager.FindProperty("availableUpgrades");
            poolProp.arraySize = upgradePool.Length;
            for (int i = 0; i < upgradePool.Length; i++)
            {
                poolProp.GetArrayElementAtIndex(i).objectReferenceValue = upgradePool[i];
            }
            sUpgradeManager.ApplyModifiedProperties();

            // Setup Canvas and UpgradeSelectionPanel
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

            // UpgradeSelectionPanel (Modal Full Screen Overlay)
            var panelTransform = canvasGo.transform.Find("UpgradeSelectionPanel");
            GameObject panelGo = panelTransform != null ? panelTransform.gameObject : new GameObject("UpgradeSelectionPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);

            var panelRect = panelGo.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            var panelBg = panelGo.GetComponent<Image>();
            if (panelBg == null) panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.08f, 0.85f); // Dark semi-transparent modal backdrop

            // Title Text
            var titleTransform = panelGo.transform.Find("Title");
            GameObject titleGo = titleTransform != null ? titleTransform.gameObject : new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRect = titleGo.GetComponent<RectTransform>();
            if (titleRect == null) titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.85f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800f, 60f);

            var titleTMP = titleGo.GetComponent<TextMeshProUGUI>();
            if (titleTMP == null) titleTMP = titleGo.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "LEVEL UP!";
            titleTMP.fontSize = 42f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(1f, 0.85f, 0.2f, 1f); // Gold

            // Subtitle Text
            var subTransform = panelGo.transform.Find("Subtitle");
            GameObject subGo = subTransform != null ? subTransform.gameObject : new GameObject("Subtitle");
            subGo.transform.SetParent(panelGo.transform, false);
            var subRect = subGo.GetComponent<RectTransform>();
            if (subRect == null) subRect = subGo.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0.77f);
            subRect.anchorMax = new Vector2(0.5f, 0.77f);
            subRect.pivot = new Vector2(0.5f, 0.5f);
            subRect.anchoredPosition = Vector2.zero;
            subRect.sizeDelta = new Vector2(800f, 40f);

            var subTMP = subGo.GetComponent<TextMeshProUGUI>();
            if (subTMP == null) subTMP = subGo.AddComponent<TextMeshProUGUI>();
            subTMP.text = "Choose a Temporary Upgrade";
            subTMP.fontSize = 22f;
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            // Container for 3 choices
            var cardsContainerTransform = panelGo.transform.Find("CardsContainer");
            GameObject cardsContainerGo = cardsContainerTransform != null ? cardsContainerTransform.gameObject : new GameObject("CardsContainer");
            cardsContainerGo.transform.SetParent(panelGo.transform, false);
            var containerRect = cardsContainerGo.GetComponent<RectTransform>();
            if (containerRect == null) containerRect = cardsContainerGo.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.45f);
            containerRect.anchorMax = new Vector2(0.5f, 0.45f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(960f, 320f);

            UpgradeChoiceButton[] choiceButtons = new UpgradeChoiceButton[3];
            float[] xOffsets = new float[] { -320f, 0f, 320f };
            string[] cardNames = new string[] { "Choice_01", "Choice_02", "Choice_03" };

            for (int i = 0; i < 3; i++)
            {
                var cardTransform = cardsContainerGo.transform.Find(cardNames[i]);
                GameObject cardGo = cardTransform != null ? cardTransform.gameObject : new GameObject(cardNames[i]);
                cardGo.transform.SetParent(cardsContainerGo.transform, false);

                var cardRect = cardGo.GetComponent<RectTransform>();
                if (cardRect == null) cardRect = cardGo.AddComponent<RectTransform>();
                cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                cardRect.pivot = new Vector2(0.5f, 0.5f);
                cardRect.anchoredPosition = new Vector2(xOffsets[i], 0f);
                cardRect.sizeDelta = new Vector2(280f, 300f);

                var cardBg = cardGo.GetComponent<Image>();
                if (cardBg == null) cardBg = cardGo.AddComponent<Image>();
                cardBg.color = new Color(0.14f, 0.16f, 0.22f, 0.95f); // Sleek card background

                var cardBtn = cardGo.GetComponent<Button>();
                if (cardBtn == null) cardBtn = cardGo.AddComponent<Button>();
                cardBtn.targetGraphic = cardBg;

                // Card Title
                var cardTitleTransform = cardGo.transform.Find("CardTitle");
                GameObject cardTitleGo = cardTitleTransform != null ? cardTitleTransform.gameObject : new GameObject("CardTitle");
                cardTitleGo.transform.SetParent(cardGo.transform, false);
                var cTitleRect = cardTitleGo.GetComponent<RectTransform>();
                if (cTitleRect == null) cTitleRect = cardTitleGo.AddComponent<RectTransform>();
                cTitleRect.anchorMin = new Vector2(0f, 0.65f);
                cTitleRect.anchorMax = new Vector2(1f, 0.95f);
                cTitleRect.pivot = new Vector2(0.5f, 0.5f);
                cTitleRect.sizeDelta = Vector2.zero;
                cTitleRect.anchoredPosition = Vector2.zero;

                var cTitleTMP = cardTitleGo.GetComponent<TextMeshProUGUI>();
                if (cTitleTMP == null) cTitleTMP = cardTitleGo.AddComponent<TextMeshProUGUI>();
                cTitleTMP.fontSize = 24f;
                cTitleTMP.fontStyle = FontStyles.Bold;
                cTitleTMP.alignment = TextAlignmentOptions.Center;
                cTitleTMP.color = new Color(0.2f, 0.85f, 1f, 1f); // Cyan title

                // Card Description
                var cardDescTransform = cardGo.transform.Find("CardDescription");
                GameObject cardDescGo = cardDescTransform != null ? cardDescTransform.gameObject : new GameObject("CardDescription");
                cardDescGo.transform.SetParent(cardGo.transform, false);
                var cDescRect = cardDescGo.GetComponent<RectTransform>();
                if (cDescRect == null) cDescRect = cardDescGo.AddComponent<RectTransform>();
                cDescRect.anchorMin = new Vector2(0.05f, 0.1f);
                cDescRect.anchorMax = new Vector2(0.95f, 0.6f);
                cDescRect.pivot = new Vector2(0.5f, 0.5f);
                cDescRect.sizeDelta = Vector2.zero;
                cDescRect.anchoredPosition = Vector2.zero;

                var cDescTMP = cardDescGo.GetComponent<TextMeshProUGUI>();
                if (cDescTMP == null) cDescTMP = cardDescGo.AddComponent<TextMeshProUGUI>();
                cDescTMP.fontSize = 16f;
                cDescTMP.alignment = TextAlignmentOptions.Center;
                cDescTMP.color = Color.white;
                cDescTMP.enableWordWrapping = true;

                // Choice Button Component
                var choiceBtnComp = cardGo.GetComponent<UpgradeChoiceButton>();
                if (choiceBtnComp == null) choiceBtnComp = cardGo.AddComponent<UpgradeChoiceButton>();
                choiceBtnComp.SetReferences(cTitleTMP, cDescTMP);
                var sChoiceBtn = new SerializedObject(choiceBtnComp);
                sChoiceBtn.FindProperty("titleText").objectReferenceValue = cTitleTMP;
                sChoiceBtn.FindProperty("descriptionText").objectReferenceValue = cDescTMP;
                sChoiceBtn.ApplyModifiedProperties();
                choiceButtons[i] = choiceBtnComp;
            }

            // UpgradeSelectionUI Controller on Canvas (always active, controlling panelRoot)
            var upgradeUI = canvasGo.GetComponent<UpgradeSelectionUI>();
            if (upgradeUI == null) upgradeUI = canvasGo.AddComponent<UpgradeSelectionUI>();

            // Clean up if previously attached to panelGo
            var legacyPanelUI = panelGo.GetComponent<UpgradeSelectionUI>();
            if (legacyPanelUI != null) Object.DestroyImmediate(legacyPanelUI);

            var sUpgradeUI = new SerializedObject(upgradeUI);
            sUpgradeUI.FindProperty("upgradeManager").objectReferenceValue = upgradeManager;
            sUpgradeUI.FindProperty("panelRoot").objectReferenceValue = panelGo;

            var buttonsProp = sUpgradeUI.FindProperty("choiceButtons");
            buttonsProp.arraySize = choiceButtons.Length;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceButtons[i];
            }
            sUpgradeUI.ApplyModifiedProperties();

            // Modal panel is hidden during normal gameplay by default
            panelGo.SetActive(false);

            // Optional: attach verifier only if explicitly requested for automated batch run
            if (includeVerifier)
            {
                var verifierGo = new GameObject("Milestone6_1_RuntimeVerifier");
                verifierGo.AddComponent<Milestone6_1_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 6.1] Scene saved at {ScenePath}. Panel hidden: {panelGo.activeSelf == false}. Verifier attached: {includeVerifier}.");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 6.1 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 6.1] Launching PlayMode verification harness...");
            CreateOrUpdateUpgradeAssets();
            UpdatePlayerPrefab();

            // Set up scene with the verifier attached for the test run
            UpdatePrototypeScene(includeVerifier: true);

            EditorApplication.EnterPlaymode();
        }

        public static void CleanupVerifierFromScene()
        {
            UpdatePrototypeScene(includeVerifier: false);
            Debug.Log("[Milestone 6.1] Cleaned up verifier from prototype scene.");
        }
    }
}
