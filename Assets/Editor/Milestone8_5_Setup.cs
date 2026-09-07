using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DungeonRoguelite.Characters;
using DungeonRoguelite.UI;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.Editor
{
    /// <summary>
    /// Automated setup script for Milestone 8.5 (Character Selection UI & Integration).
    /// Creates CharacterRoster.asset, constructs CharacterSelection.unity scene,
    /// configures EditorBuildSettings scene ordering, and guarantees scene integrity.
    /// </summary>
    public static class Milestone8_5_Setup
    {
        public const string CharacterRosterPath = "Assets/ScriptableObjects/Characters/CharacterRoster.asset";
        public const string CharacterSelectionScenePath = "Assets/Scenes/CharacterSelect/CharacterSelection.unity";
        public const string DungeonScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        public const string WarriorAssetPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
        public const string ArcherAssetPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";
        public const string GunnerAssetPath = "Assets/ScriptableObjects/Characters/Character_Gunner.asset";

        [MenuItem("DungeonRoguelite/Setup Milestone 8.5")]
        public static void Setup()
        {
            Debug.Log("[Milestone 8.5] Starting setup: CharacterRoster, CharacterSelection scene, BuildSettings...");

            EnsureDirectories();
            CreateOrUpdateCharacterRoster();
            CreateOrUpdateCharacterSelectionScene();
            ConfigureBuildSettings();
            VerifyDungeonPrototypeIntegrity();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Milestone 8.5] Setup completed successfully.");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Characters"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                {
                    AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
                }
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Characters");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scenes/CharacterSelect"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                }
                AssetDatabase.CreateFolder("Assets/Scenes", "CharacterSelect");
            }
        }

        public static CharacterRoster CreateOrUpdateCharacterRoster()
        {
            var warrior = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
            var archer = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
            var gunner = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(GunnerAssetPath);

            if (warrior == null || archer == null || gunner == null)
            {
                Debug.LogError($"[Milestone 8.5] Missing character definitions! warrior={warrior != null}, archer={archer != null}, gunner={gunner != null}");
                return null;
            }

            var roster = AssetDatabase.LoadAssetAtPath<CharacterRoster>(CharacterRosterPath);
            if (roster == null)
            {
                roster = ScriptableObject.CreateInstance<CharacterRoster>();
                AssetDatabase.CreateAsset(roster, CharacterRosterPath);
            }

            roster.SetCharacters(new[] { warrior, archer, gunner });

            if (!roster.ValidateRoster(out string error))
            {
                Debug.LogError($"[Milestone 8.5] CharacterRoster validation failed: {error}");
            }
            else
            {
                Debug.Log($"[Milestone 8.5] CharacterRoster configured with {roster.Count} characters: Warrior, Archer, Gunner.");
            }

            EditorUtility.SetDirty(roster);
            return roster;
        }

        public static void CreateOrUpdateCharacterSelectionScene()
        {
            var roster = CreateOrUpdateCharacterRoster();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.12f, 0.16f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            // 2. EventSystem with InputSystemUIInputModule
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 3. Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 4. Panel Root
            var panelGo = new GameObject("SelectionPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            // 5. Title Text
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -60f);
            titleRect.sizeDelta = new Vector2(800f, 80f);
            var titleTMP = titleGo.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "SELECT CHARACTER";
            titleTMP.fontSize = 46f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;

            // 6. Cards Container
            var containerGo = new GameObject("CardContainer");
            containerGo.transform.SetParent(panelGo.transform, false);
            var containerRect = containerGo.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = new Vector2(0f, 20f);
            containerRect.sizeDelta = new Vector2(1100f, 520f);

            var hLayout = containerGo.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 40f;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;

            // 7. Create 3 Character Cards
            var cardComponents = new CharacterSelectionCard[3];
            string[] cardNames = { "Card_Warrior", "Card_Archer", "Card_Gunner" };

            for (int i = 0; i < 3; i++)
            {
                var cardGo = new GameObject(cardNames[i]);
                cardGo.transform.SetParent(containerGo.transform, false);
                var cardRect = cardGo.AddComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(320f, 460f);

                // Card Background Image
                var bgImage = cardGo.AddComponent<Image>();
                bgImage.color = new Color(0.16f, 0.19f, 0.25f, 1f);

                // Button Component
                var btn = cardGo.AddComponent<Button>();
                btn.targetGraphic = bgImage;
                var colors = btn.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
                colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                colors.selectedColor = Color.white;
                btn.colors = colors;

                // Selection Highlight (border outline)
                var highlightGo = new GameObject("SelectionHighlight");
                highlightGo.transform.SetParent(cardGo.transform, false);
                var highlightRect = highlightGo.AddComponent<RectTransform>();
                highlightRect.anchorMin = Vector2.zero;
                highlightRect.anchorMax = Vector2.one;
                highlightRect.sizeDelta = new Vector2(10f, 10f); // 5px border outside
                highlightRect.anchoredPosition = Vector2.zero;
                var highlightImage = highlightGo.AddComponent<Image>();
                highlightImage.color = new Color(0.2f, 0.85f, 1f, 0.9f); // Cyan border
                highlightGo.transform.SetAsFirstSibling(); // Draw behind/around card
                highlightGo.SetActive(false);

                // Character Name Label
                var nameGo = new GameObject("NameText");
                nameGo.transform.SetParent(cardGo.transform, false);
                var nameRect = nameGo.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.70f);
                nameRect.anchorMax = new Vector2(1f, 0.95f);
                nameRect.pivot = new Vector2(0.5f, 0.5f);
                nameRect.sizeDelta = Vector2.zero;
                nameRect.anchoredPosition = Vector2.zero;
                var nameTMP = nameGo.AddComponent<TextMeshProUGUI>();
                nameTMP.fontSize = 28f;
                nameTMP.fontStyle = FontStyles.Bold;
                nameTMP.alignment = TextAlignmentOptions.Center;
                nameTMP.color = new Color(0.2f, 0.85f, 1f, 1f);

                // Character Description Label
                var descGo = new GameObject("DescriptionText");
                descGo.transform.SetParent(cardGo.transform, false);
                var descRect = descGo.AddComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0.08f, 0.10f);
                descRect.anchorMax = new Vector2(0.92f, 0.65f);
                descRect.pivot = new Vector2(0.5f, 0.5f);
                descRect.sizeDelta = Vector2.zero;
                descRect.anchoredPosition = Vector2.zero;
                var descTMP = descGo.AddComponent<TextMeshProUGUI>();
                descTMP.fontSize = 17f;
                descTMP.alignment = TextAlignmentOptions.Center;
                descTMP.color = new Color(0.9f, 0.9f, 0.95f, 1f);
                descTMP.enableWordWrapping = true;

                // Bind Card Component
                var cardComp = cardGo.AddComponent<CharacterSelectionCard>();
                cardComp.SetReferences(nameTMP, descTMP, highlightGo);

                var sCard = new SerializedObject(cardComp);
                sCard.FindProperty("nameText").objectReferenceValue = nameTMP;
                sCard.FindProperty("descriptionText").objectReferenceValue = descTMP;
                sCard.FindProperty("selectionHighlight").objectReferenceValue = highlightGo;
                sCard.ApplyModifiedPropertiesWithoutUndo();

                cardComponents[i] = cardComp;
            }

            // 8. Selected Preview Text
            var previewGo = new GameObject("SelectedPreviewText");
            previewGo.transform.SetParent(panelGo.transform, false);
            var previewRect = previewGo.AddComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0f);
            previewRect.anchorMax = new Vector2(0.5f, 0f);
            previewRect.pivot = new Vector2(0.5f, 0f);
            previewRect.anchoredPosition = new Vector2(0f, 140f);
            previewRect.sizeDelta = new Vector2(600f, 40f);
            var previewTMP = previewGo.AddComponent<TextMeshProUGUI>();
            previewTMP.text = "Selected: Warrior";
            previewTMP.fontSize = 24f;
            previewTMP.fontStyle = FontStyles.Normal;
            previewTMP.alignment = TextAlignmentOptions.Center;
            previewTMP.color = Color.white;

            // 9. Start Button
            var startBtnGo = new GameObject("StartButton");
            startBtnGo.transform.SetParent(panelGo.transform, false);
            var startBtnRect = startBtnGo.AddComponent<RectTransform>();
            startBtnRect.anchorMin = new Vector2(0.5f, 0f);
            startBtnRect.anchorMax = new Vector2(0.5f, 0f);
            startBtnRect.pivot = new Vector2(0.5f, 0f);
            startBtnRect.anchoredPosition = new Vector2(0f, 60f);
            startBtnRect.sizeDelta = new Vector2(280f, 65f);

            var startBtnImage = startBtnGo.AddComponent<Image>();
            startBtnImage.color = new Color(0.12f, 0.65f, 0.40f, 1f); // Green button

            var startBtn = startBtnGo.AddComponent<Button>();
            startBtn.targetGraphic = startBtnImage;

            var startBtnLabelGo = new GameObject("Text (TMP)");
            startBtnLabelGo.transform.SetParent(startBtnGo.transform, false);
            var startBtnLabelRect = startBtnLabelGo.AddComponent<RectTransform>();
            startBtnLabelRect.anchorMin = Vector2.zero;
            startBtnLabelRect.anchorMax = Vector2.one;
            startBtnLabelRect.sizeDelta = Vector2.zero;
            startBtnLabelRect.anchoredPosition = Vector2.zero;
            var startBtnTMP = startBtnLabelGo.AddComponent<TextMeshProUGUI>();
            startBtnTMP.text = "START DUNGEON";
            startBtnTMP.fontSize = 24f;
            startBtnTMP.fontStyle = FontStyles.Bold;
            startBtnTMP.alignment = TextAlignmentOptions.Center;
            startBtnTMP.color = Color.white;

            // 10. CharacterSelectionController on Canvas
            var controller = canvasGo.AddComponent<CharacterSelectionController>();
            controller.SetReferences(roster, cardComponents, previewTMP, startBtn, "Dungeon_Prototype");

            var sController = new SerializedObject(controller);
            sController.FindProperty("roster").objectReferenceValue = roster;
            var cardsProp = sController.FindProperty("cards");
            cardsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = cardComponents[i];
            }
            sController.FindProperty("selectedPreviewText").objectReferenceValue = previewTMP;
            sController.FindProperty("startButton").objectReferenceValue = startBtn;
            sController.FindProperty("targetSceneName").stringValue = "Dungeon_Prototype";
            sController.ApplyModifiedPropertiesWithoutUndo();

            // Save Scene
            EditorSceneManager.SaveScene(scene, CharacterSelectionScenePath);
            Debug.Log($"[Milestone 8.5] Created CharacterSelection scene at: {CharacterSelectionScenePath}");
        }

        public static void ConfigureBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();

            // Scene 0: CharacterSelection.unity
            scenes.Add(new EditorBuildSettingsScene(CharacterSelectionScenePath, true));

            // Scene 1: Dungeon_Prototype.unity
            scenes.Add(new EditorBuildSettingsScene(DungeonScenePath, true));

            // Retain SampleScene as disabled if it exists
            const string sampleScenePath = "Assets/Scenes/SampleScene.unity";
            if (File.Exists(sampleScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(sampleScenePath, false));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[Milestone 8.5] Configured EditorBuildSettings: Scene 0 = CharacterSelection, Scene 1 = Dungeon_Prototype.");
        }

        public static void VerifyDungeonPrototypeIntegrity()
        {
            var scene = EditorSceneManager.OpenScene(DungeonScenePath, OpenSceneMode.Single);
            var spawner = UnityEngine.Object.FindFirstObjectByType<PlayerSpawner>();
            var warriorAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);

            if (spawner != null && warriorAsset != null)
            {
                if (spawner.DefaultCharacter != warriorAsset)
                {
                    Debug.LogWarning("[Milestone 8.5] Restoring spawner.defaultCharacter to Character_Warrior in Dungeon_Prototype.");
                    spawner.SetDefaultCharacter(warriorAsset);
                    EditorUtility.SetDirty(spawner);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        [MenuItem("DungeonRoguelite/Run Milestone 8.5 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 8.5] Running setup before PlayMode verification...");
            Setup();

            // Open CharacterSelection scene
            var scene = EditorSceneManager.OpenScene(CharacterSelectionScenePath, OpenSceneMode.Single);

            // Clean any existing verifiers
            var existingVerifiers = UnityEngine.Object.FindObjectsByType<DungeonRoguelite.Tests.Milestone8_5_Verifier>(FindObjectsSortMode.None);
            foreach (var v in existingVerifiers)
            {
                UnityEngine.Object.DestroyImmediate(v.gameObject);
            }

            // Attach verifier runner to scene
            var verifierGo = new GameObject("Milestone8_5_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone8_5_Verifier>();

            Debug.Log("[Milestone 8.5] Launching Play Mode verification...");
            EditorApplication.EnterPlaymode();
        }
    }
}
