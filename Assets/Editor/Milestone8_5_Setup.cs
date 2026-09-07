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
using DungeonRoguelite.Weapons;
using DungeonRoguelite.Dungeons;

namespace DungeonRoguelite.Editor
{
    /// <summary>
    /// Automated setup script for Milestone 8.5 (Character Selection UI & Integration).
    /// Creates CharacterRoster.asset, constructs polished CharacterSelection.unity scene in Turkish,
    /// configures Gunner prefab firing feedback components, configures BuildSettings,
    /// and guarantees scene integrity.
    /// </summary>
    public static class Milestone8_5_Setup
    {
        public const string CharacterRosterPath = "Assets/ScriptableObjects/Characters/CharacterRoster.asset";
        public const string CharacterSelectionScenePath = "Assets/Scenes/CharacterSelect/CharacterSelection.unity";
        public const string DungeonScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        public const string WarriorAssetPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
        public const string ArcherAssetPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";
        public const string GunnerAssetPath = "Assets/ScriptableObjects/Characters/Character_Gunner.asset";
        public const string GunnerPrefabPath = "Assets/Prefabs/Characters/Gunner.prefab";

        [MenuItem("DungeonRoguelite/Setup Milestone 8.5")]
        public static void Setup()
        {
            Debug.Log("[Milestone 8.5] Starting setup: CharacterRoster, CharacterSelection scene, Gunner feedback, BuildSettings...");

            EnsureDirectories();
            CreateOrUpdateCharacterRoster();
            ConfigureGunnerPrefabFeedback();
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

            if (!AssetDatabase.IsValidFolder("Assets/Materials/Weapons"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                {
                    AssetDatabase.CreateFolder("Assets", "Materials");
                }
                AssetDatabase.CreateFolder("Assets/Materials", "Weapons");
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
                Debug.Log($"[Milestone 8.5] CharacterRoster configured with {roster.Count} characters: [Savaşçı, Okçu, Nişancı].");
            }

            EditorUtility.SetDirty(roster);
            return roster;
        }

        public static void ConfigureGunnerPrefabFeedback()
        {
            var gunnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GunnerPrefabPath);
            if (gunnerPrefab == null)
            {
                Debug.LogWarning("[Milestone 8.5] Gunner.prefab not found for feedback setup.");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(gunnerPrefab);
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var rifle = root.GetComponent<RifleWeapon>();
                if (rifle != null)
                {
                    Transform muzzle = rifle.MuzzlePoint;
                    if (muzzle == null)
                    {
                        muzzle = root.transform.Find("Visual/WeaponAnchor/RifleVisual/MuzzlePoint");
                    }

                    if (muzzle != null)
                    {
                        // Ensure LineRenderer for hitscan bullet tracer
                        var line = muzzle.GetComponent<LineRenderer>();
                        if (line == null)
                        {
                            line = muzzle.gameObject.AddComponent<LineRenderer>();
                        }
                        line.positionCount = 2;
                        line.startWidth = 0.05f;
                        line.endWidth = 0.02f;
                        line.useWorldSpace = true;
                        line.enabled = false;

                        // Ensure shared material
                        string matPath = "Assets/Materials/Weapons/M_RifleTracer.mat";
                        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                        if (mat == null)
                        {
                            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                            mat = new Material(shader);
                            mat.name = "M_RifleTracer";
                            mat.color = new Color(1f, 0.95f, 0.4f, 1f);
                            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(1f, 0.95f, 0.4f, 1f));
                            AssetDatabase.CreateAsset(mat, matPath);
                        }
                        line.sharedMaterial = mat;

                        // Ensure MuzzleFlash visual child
                        Transform flashChild = muzzle.Find("MuzzleFlash");
                        GameObject flashGo = flashChild != null ? flashChild.gameObject : null;
                        if (flashGo == null)
                        {
                            flashGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            flashGo.name = "MuzzleFlash";
                            flashGo.transform.SetParent(muzzle, false);
                            flashGo.transform.localPosition = Vector3.zero;
                            flashGo.transform.localScale = Vector3.one * 0.15f;
                            var col = flashGo.GetComponent<Collider>();
                            if (col != null) UnityEngine.Object.DestroyImmediate(col);
                        }

                        var flashRen = flashGo.GetComponent<Renderer>();
                        if (flashRen != null)
                        {
                            string flashMatPath = "Assets/Materials/Weapons/M_MuzzleFlash.mat";
                            var flashMat = AssetDatabase.LoadAssetAtPath<Material>(flashMatPath);
                            if (flashMat == null)
                            {
                                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                                flashMat = new Material(shader);
                                flashMat.name = "M_MuzzleFlash";
                                flashMat.color = new Color(1f, 0.85f, 0.2f, 1f);
                                if (flashMat.HasProperty("_BaseColor")) flashMat.SetColor("_BaseColor", new Color(1f, 0.85f, 0.2f, 1f));
                                AssetDatabase.CreateAsset(flashMat, flashMatPath);
                            }
                            flashRen.sharedMaterial = flashMat;
                        }
                        flashGo.SetActive(false);

                        // Wire into RifleWeapon serialized properties
                        var sRifle = new SerializedObject(rifle);
                        sRifle.FindProperty("tracerLine").objectReferenceValue = line;
                        sRifle.FindProperty("muzzleFlashVisual").objectReferenceValue = flashGo;
                        sRifle.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[Milestone 8.5] Configured Gunner.prefab firing feedback components (tracer LineRenderer and MuzzleFlash).");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
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
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 1f);
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

            // 5. Title Text (Turkish)
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -60f);
            titleRect.sizeDelta = new Vector2(900f, 80f);
            var titleTMP = titleGo.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "KARAKTERİNİ SEÇ";
            titleTMP.fontSize = 50f;
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
            containerRect.anchoredPosition = new Vector2(0f, 30f);
            containerRect.sizeDelta = new Vector2(1150f, 520f);

            var hLayout = containerGo.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 35f;
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
                cardRect.sizeDelta = new Vector2(340f, 480f);

                // Card Dark Background Image (Ensures high contrast, text is never washed out)
                var bgImage = cardGo.AddComponent<Image>();
                bgImage.color = new Color(0.12f, 0.14f, 0.19f, 1f);

                // Button Component
                var btn = cardGo.AddComponent<Button>();
                btn.targetGraphic = bgImage;
                var colors = btn.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
                colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                colors.selectedColor = Color.white;
                btn.colors = colors;

                // Default subtle border (unselected frame)
                CreateBorderFrame(cardGo.transform, "DefaultBorder", new Color(0.25f, 0.28f, 0.35f, 0.4f), 2f, 0f);

                // Selection Highlight (Cyan border outline + subtle top accent, KEEPING background dark)
                var highlightGo = new GameObject("SelectionHighlight");
                highlightGo.transform.SetParent(cardGo.transform, false);
                var highlightRect = highlightGo.AddComponent<RectTransform>();
                highlightRect.anchorMin = Vector2.zero;
                highlightRect.anchorMax = Vector2.one;
                highlightRect.sizeDelta = Vector2.zero;
                highlightRect.anchoredPosition = Vector2.zero;

                // 4-side crisp cyan outline
                CreateBorderFrame(highlightGo.transform, "CyanOutline", new Color(0.2f, 0.85f, 1f, 1f), 3.5f, 0f);

                // Very subtle top glow/header tint (5% alpha, preserving full text readability)
                var accentGo = new GameObject("TopAccent");
                accentGo.transform.SetParent(highlightGo.transform, false);
                var accentRect = accentGo.AddComponent<RectTransform>();
                accentRect.anchorMin = new Vector2(0f, 0.75f);
                accentRect.anchorMax = new Vector2(1f, 1f);
                accentRect.sizeDelta = Vector2.zero;
                accentRect.anchoredPosition = Vector2.zero;
                var accentImage = accentGo.AddComponent<Image>();
                accentImage.color = new Color(0.2f, 0.85f, 1f, 0.08f);

                highlightGo.SetActive(false);

                // Character Name Label (Large, bold, high-contrast)
                var nameGo = new GameObject("NameText");
                nameGo.transform.SetParent(cardGo.transform, false);
                var nameRect = nameGo.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0.06f, 0.74f);
                nameRect.anchorMax = new Vector2(0.94f, 0.94f);
                nameRect.pivot = new Vector2(0.5f, 0.5f);
                nameRect.sizeDelta = Vector2.zero;
                nameRect.anchoredPosition = Vector2.zero;
                var nameTMP = nameGo.AddComponent<TextMeshProUGUI>();
                nameTMP.fontSize = 34f;
                nameTMP.fontStyle = FontStyles.Bold;
                nameTMP.alignment = TextAlignmentOptions.Center;
                nameTMP.color = Color.white;

                // Character Description Label (Substantially larger, clean readability at 720p)
                var descGo = new GameObject("DescriptionText");
                descGo.transform.SetParent(cardGo.transform, false);
                var descRect = descGo.AddComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0.08f, 0.12f);
                descRect.anchorMax = new Vector2(0.92f, 0.70f);
                descRect.pivot = new Vector2(0.5f, 0.5f);
                descRect.sizeDelta = Vector2.zero;
                descRect.anchoredPosition = Vector2.zero;
                var descTMP = descGo.AddComponent<TextMeshProUGUI>();
                descTMP.fontSize = 22f;
                descTMP.alignment = TextAlignmentOptions.Center;
                descTMP.color = new Color(0.88f, 0.90f, 0.95f, 1f);
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

            // 8. Selected Preview Text (Turkish)
            var previewGo = new GameObject("SelectedPreviewText");
            previewGo.transform.SetParent(panelGo.transform, false);
            var previewRect = previewGo.AddComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0f);
            previewRect.anchorMax = new Vector2(0.5f, 0f);
            previewRect.pivot = new Vector2(0.5f, 0f);
            previewRect.anchoredPosition = new Vector2(0f, 150f);
            previewRect.sizeDelta = new Vector2(700f, 45f);
            var previewTMP = previewGo.AddComponent<TextMeshProUGUI>();
            previewTMP.text = "Seçilen: Savaşçı";
            previewTMP.fontSize = 30f;
            previewTMP.fontStyle = FontStyles.Bold;
            previewTMP.alignment = TextAlignmentOptions.Center;
            previewTMP.color = new Color(0.2f, 0.85f, 1f, 1f);

            // 9. Start Button (Turkish)
            var startBtnGo = new GameObject("StartButton");
            startBtnGo.transform.SetParent(panelGo.transform, false);
            var startBtnRect = startBtnGo.AddComponent<RectTransform>();
            startBtnRect.anchorMin = new Vector2(0.5f, 0f);
            startBtnRect.anchorMax = new Vector2(0.5f, 0f);
            startBtnRect.pivot = new Vector2(0.5f, 0f);
            startBtnRect.anchoredPosition = new Vector2(0f, 65f);
            startBtnRect.sizeDelta = new Vector2(320f, 65f);

            var startBtnImage = startBtnGo.AddComponent<Image>();
            startBtnImage.color = new Color(0.12f, 0.65f, 0.40f, 1f); // Vibrant green

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
            startBtnTMP.text = "ZİNDANA BAŞLA";
            startBtnTMP.fontSize = 26f;
            startBtnTMP.fontStyle = FontStyles.Bold;
            startBtnTMP.alignment = TextAlignmentOptions.Center;
            startBtnTMP.color = Color.white;

            // 10. CharacterSelectionController on Canvas
            var controller = canvasGo.AddComponent<CharacterSelectionController>();
            controller.SetReferences(roster, cardComponents, previewTMP, startBtn, "WorldMap");

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
            sController.FindProperty("targetSceneName").stringValue = "WorldMap";
            sController.ApplyModifiedPropertiesWithoutUndo();

            // Save Scene
            EditorSceneManager.SaveScene(scene, CharacterSelectionScenePath);
            Debug.Log($"[Milestone 8.5] Created polished Turkish CharacterSelection scene at: {CharacterSelectionScenePath}");
        }

        private static GameObject CreateBorderFrame(Transform parent, string name, Color color, float thickness, float offset)
        {
            var frameGo = new GameObject(name);
            frameGo.transform.SetParent(parent, false);
            var frameRect = frameGo.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.sizeDelta = new Vector2(offset * 2f, offset * 2f);
            frameRect.anchoredPosition = Vector2.zero;

            // Top edge
            CreateEdge(frameGo.transform, "Top", color, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, thickness));
            // Bottom edge
            CreateEdge(frameGo.transform, "Bottom", color, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, thickness));
            // Left edge
            CreateEdge(frameGo.transform, "Left", color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(thickness, 0f));
            // Right edge
            CreateEdge(frameGo.transform, "Right", color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(thickness, 0f));

            return frameGo;
        }

        private static void CreateEdge(Transform parent, string edgeName, Color color, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 sizeDelta)
        {
            var edgeGo = new GameObject(edgeName);
            edgeGo.transform.SetParent(parent, false);
            var rect = edgeGo.AddComponent<RectTransform>();
            rect.anchorMin = aMin;
            rect.anchorMax = aMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = Vector2.zero;

            var img = edgeGo.AddComponent<Image>();
            img.color = color;
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
