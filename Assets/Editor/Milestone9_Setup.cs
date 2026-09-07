using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Editor
{
    /// <summary>
    /// Automated setup and gate runner for Phase 9 (Dungeon Progression & Campaign Flow).
    /// Orchestrates setup and deterministic verification execution across Milestones 9.1 to 9.5.
    /// </summary>
    public static class Milestone9_Setup
    {
        public const string DungeonFolder = "Assets/ScriptableObjects/Dungeons";
        public const string Dungeon01Path = "Assets/ScriptableObjects/Dungeons/Dungeon_01.asset";
        public const string Dungeon02Path = "Assets/ScriptableObjects/Dungeons/Dungeon_02.asset";
        public const string DungeonCatalogPath = "Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset";

        public const string DungeonPrototypeScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
        public const string CharacterSelectionScenePath = "Assets/Scenes/CharacterSelect/CharacterSelection.unity";
        public const string WorldMapScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";
        public const string Dungeon02ScenePath = "Assets/Scenes/Dungeons/Dungeon_02.unity";

        public const string Wave01Path = "Assets/ScriptableObjects/Waves/Wave_01.asset";
        public const string Wave02Path = "Assets/ScriptableObjects/Waves/Wave_02.asset";
        public const string Wave03Path = "Assets/ScriptableObjects/Waves/Wave_03.asset";

        [MenuItem("DungeonRoguelite/Phase 9/Setup Gate 9.1 Assets")]
        public static void SetupGate9_1()
        {
            Debug.Log("[Milestone 9.1 Setup] Creating/verifying Dungeon definitions...");
            EnsureDirectories();
            CreateOrUpdateDungeonDefinitions();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Milestone 9.1 Setup] Setup completed successfully.");
        }

        public static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }

            if (!AssetDatabase.IsValidFolder(DungeonFolder))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Dungeons");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scenes/WorldMap"))
            {
                AssetDatabase.CreateFolder("Assets/Scenes", "WorldMap");
            }
        }

        public static void CreateOrUpdateDungeonDefinitions()
        {
            var wave1 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave01Path);
            var wave2 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave02Path);
            var wave3 = AssetDatabase.LoadAssetAtPath<WaveDefinition>(Wave03Path);

            var wavesD1 = new WaveDefinition[] { wave1, wave2, wave3 };

            // Dungeon 1
            var d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon01Path);
            if (d1 == null)
            {
                d1 = ScriptableObject.CreateInstance<DungeonDefinition>();
                d1.SetConfiguration(
                    "dungeon_1",
                    "Bölüm 1: Unutulmuş Mezarlık",
                    "Zombilerin kol gezdiği kadim mezarlık. 3 dalga hayatta kal.",
                    "Dungeon_Prototype",
                    "",
                    wavesD1
                );
                AssetDatabase.CreateAsset(d1, Dungeon01Path);
                Debug.Log($"[Milestone 9.1 Setup] Created {Dungeon01Path}");
            }
            else
            {
                d1.SetConfiguration(
                    "dungeon_1",
                    "Bölüm 1: Unutulmuş Mezarlık",
                    "Zombilerin kol gezdiği kadim mezarlık. 3 dalga hayatta kal.",
                    "Dungeon_Prototype",
                    "",
                    wavesD1
                );
                EditorUtility.SetDirty(d1);
            }

            // Dungeon 2
            var d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon02Path);
            if (d2 == null)
            {
                d2 = ScriptableObject.CreateInstance<DungeonDefinition>();
                d2.SetConfiguration(
                    "dungeon_2",
                    "Bölüm 2: Karanlık Mahzen",
                    "Daha dar ve yoğun düşman akınları. 4 dalga hayatta kal.",
                    "Dungeon_02",
                    "dungeon_1",
                    wavesD1
                );
                AssetDatabase.CreateAsset(d2, Dungeon02Path);
                Debug.Log($"[Milestone 9.1 Setup] Created {Dungeon02Path}");
            }
            else
            {
                d2.SetConfiguration(
                    "dungeon_2",
                    "Bölüm 2: Karanlık Mahzen",
                    "Daha dar ve yoğun düşman akınları. 4 dalga hayatta kal.",
                    "Dungeon_02",
                    "dungeon_1",
                    wavesD1
                );
                EditorUtility.SetDirty(d2);
            }
        }

        [MenuItem("DungeonRoguelite/Phase 9/Run Gate 9.1 Verification")]
        public static void RunGate9_1Verification()
        {
            Debug.Log("[GATE 9.1] Running setup before verification...");
            SetupGate9_1();

            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate9_1_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone9_1_Verifier>();

            Debug.Log("[GATE 9.1] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Phase 9/Setup Gate 9.2 (Defeat Flow)")]
        public static void SetupGate9_2()
        {
            Debug.Log("[Milestone 9.2 Setup] Configuring PlayerDefeatController and Defeat UI in Dungeon_Prototype...");
            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);

            // 1. Clean existing verifiers
            CleanAllVerifiers();

            // 2. Resolve scene objects
            var waveManager = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
            var playerSpawner = UnityEngine.Object.FindFirstObjectByType<PlayerSpawner>();
            var completionController = UnityEngine.Object.FindFirstObjectByType<DungeonCompletionController>();

            var runControllersGo = GameObject.Find("RunControllers");
            if (runControllersGo == null)
            {
                runControllersGo = new GameObject("RunControllers");
            }

            // 3. Setup PlayerDefeatController on RunControllers
            var defeatController = runControllersGo.GetComponent<PlayerDefeatController>();
            if (defeatController == null)
            {
                defeatController = runControllersGo.AddComponent<PlayerDefeatController>();
            }
            defeatController.SetReferences(waveManager, playerSpawner, completionController);

            var sDefeat = new SerializedObject(defeatController);
            sDefeat.FindProperty("waveManager").objectReferenceValue = waveManager;
            sDefeat.FindProperty("playerSpawner").objectReferenceValue = playerSpawner;
            sDefeat.FindProperty("completionController").objectReferenceValue = completionController;
            sDefeat.ApplyModifiedProperties();
            EditorUtility.SetDirty(defeatController);

            // 4. Update DungeonCompletionController with defeatController
            if (completionController != null)
            {
                var sComp = new SerializedObject(completionController);
                sComp.FindProperty("playerDefeatController").objectReferenceValue = defeatController;
                sComp.ApplyModifiedProperties();
                EditorUtility.SetDirty(completionController);
            }

            // 5. Canvas resolution
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                Debug.LogError("[Milestone 9.2 Setup] Canvas not found in scene!");
                return;
            }

            // 6. Setup DefeatPanel under Canvas
            var defeatPanelTransform = canvasGo.transform.Find("DefeatPanel");
            GameObject defeatPanelGo;
            if (defeatPanelTransform == null)
            {
                defeatPanelGo = new GameObject("DefeatPanel");
                defeatPanelGo.transform.SetParent(canvasGo.transform, false);

                var rect = defeatPanelGo.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;

                var img = defeatPanelGo.AddComponent<Image>();
                img.color = new Color(0.04f, 0.04f, 0.04f, 0.90f);
            }
            else
            {
                defeatPanelGo = defeatPanelTransform.gameObject;
            }

            // Title Text
            var titleTMP = CreateOrGetText(defeatPanelGo, "Title", "YENİLDİN", new Vector2(0f, 100f), new Vector2(400f, 70f), 48f, new Color(0.95f, 0.25f, 0.25f, 1f));

            // Restart Button
            var restartBtn = CreateButton(defeatPanelGo, "RestartButton", "YENİDEN DENE", new Vector2(0f, 0f), new Vector2(280f, 50f));

            // Return to Map Button
            var returnToMapBtn = CreateButton(defeatPanelGo, "MapButton", "HARİTAYA DÖN", new Vector2(0f, -65f), new Vector2(280f, 50f));

            // Setup PlayerDefeatUI on Canvas
            var defeatUI = canvasGo.GetComponent<PlayerDefeatUI>();
            if (defeatUI == null)
            {
                defeatUI = canvasGo.AddComponent<PlayerDefeatUI>();
            }
            defeatUI.SetReferences(defeatController, defeatPanelGo, titleTMP, restartBtn, returnToMapBtn);

            var sDefeatUI = new SerializedObject(defeatUI);
            sDefeatUI.FindProperty("defeatController").objectReferenceValue = defeatController;
            sDefeatUI.FindProperty("panelRoot").objectReferenceValue = defeatPanelGo;
            sDefeatUI.FindProperty("titleText").objectReferenceValue = titleTMP;
            sDefeatUI.FindProperty("restartButton").objectReferenceValue = restartBtn;
            sDefeatUI.FindProperty("returnToMapButton").objectReferenceValue = returnToMapBtn;
            sDefeatUI.ApplyModifiedProperties();
            EditorUtility.SetDirty(defeatUI);

            // Defeat panel hidden by default
            defeatPanelGo.SetActive(false);

            // 7. Update DungeonCompletePanel with MapButton
            var completePanelTransform = canvasGo.transform.Find("DungeonCompletePanel");
            if (completePanelTransform != null)
            {
                var completePanelGo = completePanelTransform.gameObject;
                var compRect = completePanelGo.GetComponent<RectTransform>();
                if (compRect != null)
                {
                    compRect.sizeDelta = new Vector2(600f, 530f);
                }

                var existingRestartTransform = completePanelGo.transform.Find("RestartButton");
                if (existingRestartTransform != null)
                {
                    var rRect = existingRestartTransform.GetComponent<RectTransform>();
                    if (rRect != null)
                    {
                        rRect.anchoredPosition = new Vector2(0f, -130f);
                    }
                }

                var mapBtnComplete = CreateButton(completePanelGo, "MapButton", "HARİTAYA DÖN", new Vector2(0f, -195f), new Vector2(250f, 50f));

                var completeUI = canvasGo.GetComponent<DungeonCompleteUI>();
                if (completeUI != null)
                {
                    var sCompleteUI = new SerializedObject(completeUI);
                    sCompleteUI.FindProperty("returnToMapButton").objectReferenceValue = mapBtnComplete;
                    sCompleteUI.ApplyModifiedProperties();
                    EditorUtility.SetDirty(completeUI);
                }
            }

            EditorSceneManager.SaveScene(scene, DungeonPrototypeScenePath);
            Debug.Log("[Milestone 9.2 Setup] Gate 9.2 setup complete. Clean scene saved.");
        }

        [MenuItem("DungeonRoguelite/Phase 9/Run Gate 9.2 Verification")]
        public static void RunGate9_2Verification()
        {
            Debug.Log("[GATE 9.2] Running setup before verification...");
            SetupGate9_2();

            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate9_2_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone9_2_Verifier>();

            Debug.Log("[GATE 9.2] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        public static void CleanAllVerifiers()
        {
            var oldVerifiers = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in oldVerifiers)
            {
                if (mb != null && mb.GetType().Name.Contains("Verifier"))
                {
                    UnityEngine.Object.DestroyImmediate(mb.gameObject);
                }
            }
        }

        private static TextMeshProUGUI CreateOrGetText(GameObject container, string name, string text, Vector2 pos, Vector2 size, float fontSize, Color color)
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
            rect.sizeDelta = size;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            return tmp;
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size)
        {
            var existing = parent.transform.Find(name);
            GameObject btnGo = existing != null ? existing.gameObject : new GameObject(name);
            btnGo.transform.SetParent(parent.transform, false);

            var rect = btnGo.GetComponent<RectTransform>();
            if (rect == null) rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = btnGo.GetComponent<Image>();
            if (img == null) img = btnGo.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            var btn = btnGo.GetComponent<Button>();
            if (btn == null) btn = btnGo.AddComponent<Button>();

            var labelTransform = btnGo.transform.Find("Text");
            GameObject labelGo = labelTransform != null ? labelTransform.gameObject : new GameObject("Text");
            labelGo.transform.SetParent(btnGo.transform, false);

            var labelRect = labelGo.GetComponent<RectTransform>();
            if (labelRect == null) labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;

            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }

        [MenuItem("DungeonRoguelite/Phase 9/Run Gate 9.3 Verification")]
        public static void RunGate9_3Verification()
        {
            Debug.Log("[GATE 9.3] Preparing Gate 9.3 verification...");
            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate9_3_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone9_3_Verifier>();

            Debug.Log("[GATE 9.3] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Phase 9/Setup Gate 9.4 (World Map)")]
        public static void SetupGate9_4()
        {
            Debug.Log("[Milestone 9.4 Setup] Configuring DungeonCatalog, WorldMap scene, and Build Settings...");
            EnsureDirectories();

            // 1. Create or update DungeonCatalog.asset
            var d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon01Path);
            var d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon02Path);
            var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>(DungeonCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<DungeonCatalog>();
                catalog.SetDungeons(new DungeonDefinition[] { d1, d2 });
                AssetDatabase.CreateAsset(catalog, DungeonCatalogPath);
                Debug.Log($"[Milestone 9.4 Setup] Created {DungeonCatalogPath}");
            }
            else
            {
                catalog.SetDungeons(new DungeonDefinition[] { d1, d2 });
                EditorUtility.SetDirty(catalog);
            }
            AssetDatabase.SaveAssets();

            // 2. Update CharacterSelection.unity targetSceneName to WorldMap
            if (File.Exists(CharacterSelectionScenePath))
            {
                var csScene = EditorSceneManager.OpenScene(CharacterSelectionScenePath, OpenSceneMode.Single);
                CleanAllVerifiers();
                var csController = UnityEngine.Object.FindFirstObjectByType<CharacterSelectionController>();
                if (csController != null)
                {
                    var so = new SerializedObject(csController);
                    so.FindProperty("targetSceneName").stringValue = "WorldMap";
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(csController);
                }
                EditorSceneManager.SaveScene(csScene, CharacterSelectionScenePath);
            }

            // 3. Create WorldMap.unity scene
            var mapScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CleanAllVerifiers();

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.10f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            // EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Header Title
            var titleTMP = CreateOrGetText(canvasGo, "TitleText", "DÜNYA HARİTASI", new Vector2(0f, 420f), new Vector2(800f, 80f), 48f, Color.white);

            // Active Hero Label
            var heroTMP = CreateOrGetText(canvasGo, "ActiveHeroText", "Kahraman: Savaşçı", new Vector2(0f, 350f), new Vector2(600f, 45f), 24f, new Color(0.95f, 0.85f, 0.35f, 1f));

            // Dungeon Cards
            var cardComponents = new WorldMapCard[2];
            cardComponents[0] = CreateWorldMapCard(canvasGo, "Card_Dungeon01", new Vector2(-280f, 30f));
            cardComponents[1] = CreateWorldMapCard(canvasGo, "Card_Dungeon02", new Vector2(280f, 30f));

            // Enter Button
            var enterBtn = CreateButton(canvasGo, "EnterButton", "ZİNDANA GİR", new Vector2(0f, -280f), new Vector2(340f, 65f));

            // Back Button
            var backBtn = CreateButton(canvasGo, "BackButton", "GERİ", new Vector2(-700f, -420f), new Vector2(200f, 50f));

            // WorldMapController
            var catalogAsset = AssetDatabase.LoadAssetAtPath<DungeonCatalog>(DungeonCatalogPath);
            var mapController = canvasGo.AddComponent<WorldMapController>();
            mapController.SetReferences(catalogAsset, cardComponents, enterBtn, backBtn, heroTMP, titleTMP);

            var sMap = new SerializedObject(mapController);
            sMap.FindProperty("dungeonCatalog").objectReferenceValue = catalogAsset;
            var cardsProp = sMap.FindProperty("cards");
            cardsProp.arraySize = 2;
            cardsProp.GetArrayElementAtIndex(0).objectReferenceValue = cardComponents[0];
            cardsProp.GetArrayElementAtIndex(1).objectReferenceValue = cardComponents[1];
            sMap.FindProperty("enterDungeonButton").objectReferenceValue = enterBtn;
            sMap.FindProperty("backButton").objectReferenceValue = backBtn;
            sMap.FindProperty("activeHeroText").objectReferenceValue = heroTMP;
            sMap.FindProperty("titleText").objectReferenceValue = titleTMP;
            sMap.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(mapScene, WorldMapScenePath);

            // 4. Update Build Settings registration & order
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(CharacterSelectionScenePath, true),
                new EditorBuildSettingsScene(WorldMapScenePath, true),
                new EditorBuildSettingsScene(DungeonPrototypeScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            Debug.Log("[Milestone 9.4 Setup] Gate 9.4 setup completed successfully.");
        }

        [MenuItem("DungeonRoguelite/Phase 9/Run Gate 9.4 Verification")]
        public static void RunGate9_4Verification()
        {
            Debug.Log("[GATE 9.4] Running setup before verification...");
            SetupGate9_4();

            var scene = EditorSceneManager.OpenScene(WorldMapScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate9_4_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone9_4_Verifier>();

            Debug.Log("[GATE 9.4] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        private static WorldMapCard CreateWorldMapCard(GameObject parent, string name, Vector2 pos)
        {
            var cardGo = new GameObject(name);
            cardGo.transform.SetParent(parent.transform, false);

            var rect = cardGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(460f, 450f);

            var img = cardGo.AddComponent<Image>();
            img.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            var btn = cardGo.AddComponent<Button>();

            // Selection border
            var borderGo = new GameObject("SelectionBorder");
            borderGo.transform.SetParent(cardGo.transform, false);
            var bRect = borderGo.AddComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.sizeDelta = new Vector2(8f, 8f);
            bRect.anchoredPosition = Vector2.zero;
            var bImg = borderGo.AddComponent<Image>();
            bImg.color = new Color(0.95f, 0.85f, 0.3f, 1f);
            borderGo.transform.SetAsFirstSibling();
            borderGo.SetActive(false);

            // Title text
            var titleTMP = CreateOrGetText(cardGo, "Title", "Bölüm", new Vector2(0f, 150f), new Vector2(420f, 60f), 28f, Color.white);

            // Description text
            var descTMP = CreateOrGetText(cardGo, "Description", "Açıklama", new Vector2(0f, 50f), new Vector2(400f, 100f), 20f, new Color(0.8f, 0.85f, 0.9f, 1f));
            descTMP.enableWordWrapping = true;

            // Status text
            var statusTMP = CreateOrGetText(cardGo, "Status", "AÇIK", new Vector2(0f, -70f), new Vector2(300f, 40f), 24f, new Color(0.4f, 0.8f, 1f, 1f));

            // Lock overlay
            var lockGo = new GameObject("LockOverlay");
            lockGo.transform.SetParent(cardGo.transform, false);
            var lRect = lockGo.AddComponent<RectTransform>();
            lRect.anchorMin = Vector2.zero;
            lRect.anchorMax = Vector2.one;
            lRect.sizeDelta = Vector2.zero;
            lRect.anchoredPosition = Vector2.zero;
            var lImg = lockGo.AddComponent<Image>();
            lImg.color = new Color(0.04f, 0.04f, 0.06f, 0.75f);
            var lockTMP = CreateOrGetText(lockGo, "LockText", "KİLİTLİ", Vector2.zero, new Vector2(250f, 50f), 32f, new Color(0.85f, 0.3f, 0.3f, 1f));
            lockGo.SetActive(false);

            var card = cardGo.AddComponent<WorldMapCard>();
            card.SetReferences(titleTMP, descTMP, statusTMP, lockGo, borderGo, btn);

            var sCard = new SerializedObject(card);
            sCard.FindProperty("titleText").objectReferenceValue = titleTMP;
            sCard.FindProperty("descriptionText").objectReferenceValue = descTMP;
            sCard.FindProperty("statusText").objectReferenceValue = statusTMP;
            sCard.FindProperty("lockOverlay").objectReferenceValue = lockGo;
            sCard.FindProperty("selectionBorder").objectReferenceValue = borderGo;
            sCard.FindProperty("cardButton").objectReferenceValue = btn;
            sCard.ApplyModifiedProperties();

            return card;
        }
    }
}
