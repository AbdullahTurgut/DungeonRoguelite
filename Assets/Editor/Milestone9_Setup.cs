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
using DungeonRoguelite.Player;

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

        public const string Wave02_01Path = "Assets/ScriptableObjects/Waves/Wave_02_01.asset";
        public const string Wave02_02Path = "Assets/ScriptableObjects/Waves/Wave_02_02.asset";
        public const string Wave02_03Path = "Assets/ScriptableObjects/Waves/Wave_02_03.asset";
        public const string Wave02_04Path = "Assets/ScriptableObjects/Waves/Wave_02_04.asset";
        public const string ZombiePrefabPath = "Assets/Prefabs/Enemies/Zombie.prefab";

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
                    wavesD1,
                    1f,
                    1f
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
                    wavesD1,
                    1f,
                    1f
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
                    wavesD1,
                    1.1f,
                    1f
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
                    wavesD1,
                    1.1f,
                    1f
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
            var cardComponents = new WorldMapCard[3];
            cardComponents[0] = CreateWorldMapCard(canvasGo, "Card_Dungeon01", new Vector2(-500f, 30f));
            cardComponents[1] = CreateWorldMapCard(canvasGo, "Card_Dungeon02", new Vector2(0f, 30f));
            cardComponents[2] = CreateWorldMapCard(canvasGo, "Card_Dungeon03", new Vector2(500f, 30f));

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
            cardsProp.arraySize = 3;
            cardsProp.GetArrayElementAtIndex(0).objectReferenceValue = cardComponents[0];
            cardsProp.GetArrayElementAtIndex(1).objectReferenceValue = cardComponents[1];
            cardsProp.GetArrayElementAtIndex(2).objectReferenceValue = cardComponents[2];
            sMap.FindProperty("enterDungeonButton").objectReferenceValue = enterBtn;
            sMap.FindProperty("backButton").objectReferenceValue = backBtn;
            sMap.FindProperty("activeHeroText").objectReferenceValue = heroTMP;
            sMap.FindProperty("titleText").objectReferenceValue = titleTMP;
            sMap.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(mapScene, WorldMapScenePath);

            // 4. Update Build Settings registration & order
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(CharacterSelectionScenePath, true),
                new EditorBuildSettingsScene(WorldMapScenePath, true),
                new EditorBuildSettingsScene(DungeonPrototypeScenePath, true)
            };
            if (File.Exists(Dungeon02ScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(Dungeon02ScenePath, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();

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
            rect.sizeDelta = new Vector2(440f, 450f);

            var img = cardGo.AddComponent<Image>();
            img.color = new Color(0.10f, 0.12f, 0.16f, 0.96f); // Rich dark card body

            var btn = cardGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None; // Prevents button tint from overriding dark card body

            // Selection border: 4-edge outline frame over dark card (no full solid fill)
            var borderGo = CreateBorderFrame(cardGo.transform, "SelectionBorder", new Color(0.2f, 0.85f, 1f, 1f), 3.5f, 2f);
            borderGo.SetActive(false);

            // Title text (large, bold, pure white)
            var titleTMP = CreateOrGetText(cardGo, "Title", "Bölüm", new Vector2(0f, 150f), new Vector2(400f, 60f), 28f, Color.white);
            titleTMP.fontStyle = FontStyles.Bold;

            // Description text (clean size 20f, high contrast #E6ECF5)
            var descTMP = CreateOrGetText(cardGo, "Description", "Açıklama", new Vector2(0f, 45f), new Vector2(390f, 120f), 20f, new Color(0.90f, 0.93f, 0.97f, 1f));
            descTMP.enableWordWrapping = true;
            descTMP.lineSpacing = 8f;

            // Status text (bold 24f)
            var statusTMP = CreateOrGetText(cardGo, "Status", "AÇIK", new Vector2(0f, -80f), new Vector2(320f, 40f), 24f, new Color(0.25f, 0.85f, 1f, 1f));
            statusTMP.fontStyle = FontStyles.Bold;

            // Lock overlay
            var lockGo = new GameObject("LockOverlay");
            lockGo.transform.SetParent(cardGo.transform, false);
            var lRect = lockGo.AddComponent<RectTransform>();
            lRect.anchorMin = Vector2.zero;
            lRect.anchorMax = Vector2.one;
            lRect.sizeDelta = Vector2.zero;
            lRect.anchoredPosition = Vector2.zero;
            var lImg = lockGo.AddComponent<Image>();
            lImg.color = new Color(0.04f, 0.05f, 0.08f, 0.82f);
            var lockTMP = CreateOrGetText(lockGo, "LockText", "KİLİTLİ", Vector2.zero, new Vector2(280f, 60f), 34f, new Color(0.95f, 0.35f, 0.35f, 1f));
            lockTMP.fontStyle = FontStyles.Bold;
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

        [MenuItem("DungeonRoguelite/Phase 9/Setup Gate 9.5 (Second Dungeon)")]
        public static void SetupGate9_5()
        {
            Debug.Log("[Milestone 9.5 Setup] Setting up Dungeon 2 waves, Dungeon_02.unity scene, and 4-scene Build Settings...");
            EnsureDirectories();

            // 1. Create or update Dungeon 2 wave assets
            var zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ZombiePrefabPath);
            var w1 = CreateOrUpdateWave(Wave02_01Path, zombiePrefab, 5, 0.8f);
            var w2 = CreateOrUpdateWave(Wave02_02Path, zombiePrefab, 8, 0.7f);
            var w3 = CreateOrUpdateWave(Wave02_03Path, zombiePrefab, 12, 0.6f);
            var w4 = CreateOrUpdateWave(Wave02_04Path, zombiePrefab, 15, 0.5f);

            var wavesD2 = new WaveDefinition[] { w1, w2, w3, w4 };

            // 2. Update Dungeon_02.asset
            var d2 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon02Path);
            if (d2 == null)
            {
                d2 = ScriptableObject.CreateInstance<DungeonDefinition>();
                d2.SetConfiguration("dungeon_2", "Bölüm 2: Karanlık Mahzen", "Daha yoğun zombi akınları. 4 dalga hayatta kal.", "Dungeon_02", "dungeon_1", wavesD2, 1.1f, 1f);
                AssetDatabase.CreateAsset(d2, Dungeon02Path);
            }
            else
            {
                d2.SetConfiguration("dungeon_2", "Bölüm 2: Karanlık Mahzen", "Daha yoğun zombi akınları. 4 dalga hayatta kal.", "Dungeon_02", "dungeon_1", wavesD2, 1.1f, 1f);
                EditorUtility.SetDirty(d2);
            }

            // 3. Ensure DungeonCatalog has [D1, D2]
            var d1 = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(Dungeon01Path);
            var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>(DungeonCatalogPath);
            if (catalog != null)
            {
                catalog.SetDungeons(new DungeonDefinition[] { d1, d2 });
                EditorUtility.SetDirty(catalog);
            }
            AssetDatabase.SaveAssets();

            // 4. Create or update Dungeon_02.unity scene
            // Clone Dungeon_Prototype.unity to ensure 100% shared gameplay rig
            if (!File.Exists(Dungeon02ScenePath))
            {
                AssetDatabase.CopyAsset(DungeonPrototypeScenePath, Dungeon02ScenePath);
                AssetDatabase.Refresh();
            }

            var d2Scene = EditorSceneManager.OpenScene(Dungeon02ScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            // Configure WaveManager in Dungeon_02
            var waveManager = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.SetWaves(wavesD2);
                var sWave = new SerializedObject(waveManager);
                var wavesProp = sWave.FindProperty("waves");
                wavesProp.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    wavesProp.GetArrayElementAtIndex(i).objectReferenceValue = wavesD2[i];
                }
                sWave.ApplyModifiedProperties();
                EditorUtility.SetDirty(waveManager);
            }

            // Darken directional light for "Karanlık Mahzen" ambient atmosphere
            var dirLightGo = GameObject.Find("Directional Light");
            if (dirLightGo != null)
            {
                var light = dirLightGo.GetComponent<Light>();
                if (light != null)
                {
                    light.color = new Color(0.65f, 0.70f, 0.85f, 1f);
                    light.intensity = 0.75f;
                    EditorUtility.SetDirty(light);
                }
            }

            EditorSceneManager.SaveScene(d2Scene, Dungeon02ScenePath);

            // 5. Update Build Settings: 4 scenes in exact order
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(CharacterSelectionScenePath, true),
                new EditorBuildSettingsScene(WorldMapScenePath, true),
                new EditorBuildSettingsScene(DungeonPrototypeScenePath, true),
                new EditorBuildSettingsScene(Dungeon02ScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            Debug.Log("[Milestone 9.5 Setup] Gate 9.5 setup completed successfully.");
        }

        private static WaveDefinition CreateOrUpdateWave(string path, GameObject enemyPrefab, int count, float interval)
        {
            var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
            var entries = new EnemySpawnEntry[] { new EnemySpawnEntry(enemyPrefab, count) };
            if (wave == null)
            {
                wave = ScriptableObject.CreateInstance<WaveDefinition>();
                wave.Initialize(entries, interval);
                AssetDatabase.CreateAsset(wave, path);
            }
            else
            {
                wave.Initialize(entries, interval);
                EditorUtility.SetDirty(wave);
            }
            return wave;
        }

        [MenuItem("DungeonRoguelite/Phase 9/Run Gate 9.5 Verification")]
        public static void RunGate9_5Verification()
        {
            Debug.Log("[GATE 9.5] Running setup before verification...");
            SetupGate9_5();

            var scene = EditorSceneManager.OpenScene(Dungeon02ScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Gate9_5_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone9_5_Verifier>();

            Debug.Log("[GATE 9.5] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }

        public static void SetupPlayerHealthUIInScene(UnityEngine.SceneManagement.Scene scene)
        {
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                Debug.LogError($"[PlayerHealthUI Setup] Canvas not found in {scene.path}!");
                return;
            }

            var spawner = UnityEngine.Object.FindFirstObjectByType<PlayerSpawner>();

            // 1. Root Container: PlayerHealthHUD
            var hudTransform = canvasGo.transform.Find("PlayerHealthHUD");
            GameObject hudGo;
            if (hudTransform == null)
            {
                hudGo = new GameObject("PlayerHealthHUD");
                hudGo.transform.SetParent(canvasGo.transform, false);
            }
            else
            {
                hudGo = hudTransform.gameObject;
            }

            var hudRect = hudGo.GetComponent<RectTransform>();
            if (hudRect == null) hudRect = hudGo.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0f, 1f);
            hudRect.anchorMax = new Vector2(0f, 1f);
            hudRect.pivot = new Vector2(0f, 1f);
            hudRect.anchoredPosition = new Vector2(30f, -30f);
            hudRect.sizeDelta = new Vector2(280f, 55f);

            var hudBg = hudGo.GetComponent<Image>();
            if (hudBg == null) hudBg = hudGo.AddComponent<Image>();
            hudBg.color = new Color(0.08f, 0.10f, 0.14f, 0.92f);

            // Subtle border frame
            var frameTransform = hudGo.transform.Find("BorderFrame");
            if (frameTransform == null)
            {
                CreateBorderFrame(hudGo.transform, "BorderFrame", new Color(0.25f, 0.30f, 0.40f, 0.60f), 1.5f, 0f);
            }

            // 2. Label: "CAN"
            var labelTransform = hudGo.transform.Find("Label");
            GameObject labelGo = labelTransform != null ? labelTransform.gameObject : new GameObject("Label");
            labelGo.transform.SetParent(hudGo.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            if (labelRect == null) labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(12f, 0f);
            labelRect.sizeDelta = new Vector2(45f, 30f);

            var labelTMP = labelGo.GetComponent<TextMeshProUGUI>();
            if (labelTMP == null) labelTMP = labelGo.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "CAN";
            labelTMP.fontSize = 18f;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.color = new Color(0.95f, 0.35f, 0.35f, 1f);
            labelTMP.alignment = TextAlignmentOptions.Center;

            // 3. Slider: HealthSlider
            var sliderTransform = hudGo.transform.Find("HealthSlider");
            GameObject sliderGo = sliderTransform != null ? sliderTransform.gameObject : new GameObject("HealthSlider");
            sliderGo.transform.SetParent(hudGo.transform, false);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            if (sliderRect == null) sliderRect = sliderGo.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.5f);
            sliderRect.anchorMax = new Vector2(1f, 0.5f);
            sliderRect.pivot = new Vector2(0f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(65f, 0f);
            sliderRect.sizeDelta = new Vector2(-75f, 24f);

            var slider = sliderGo.GetComponent<Slider>();
            if (slider == null) slider = sliderGo.AddComponent<Slider>();

            // Slider Background
            var bgTransform = sliderGo.transform.Find("Background");
            GameObject bgGo = bgTransform != null ? bgTransform.gameObject : new GameObject("Background");
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRect = bgGo.GetComponent<RectTransform>();
            if (bgRect == null) bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgGo.GetComponent<Image>();
            if (bgImg == null) bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.14f, 0.16f, 0.20f, 1f);

            // Fill Area
            var fillAreaTransform = sliderGo.transform.Find("Fill Area");
            GameObject fillAreaGo = fillAreaTransform != null ? fillAreaTransform.gameObject : new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillAreaGo.GetComponent<RectTransform>();
            if (fillAreaRect == null) fillAreaRect = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            // Fill
            var fillTransform = fillAreaGo.transform.Find("Fill");
            GameObject fillGo = fillTransform != null ? fillTransform.gameObject : new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRect = fillGo.GetComponent<RectTransform>();
            if (fillRect == null) fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fillGo.GetComponent<Image>();
            if (fillImg == null) fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.92f, 0.22f, 0.24f, 1f); // Crimson red

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.interactable = false;

            // 4. Numeric Text: HealthText (centered on slider)
            var textTransform = sliderGo.transform.Find("HealthText");
            GameObject textGo = textTransform != null ? textTransform.gameObject : new GameObject("HealthText");
            textGo.transform.SetParent(sliderGo.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            if (textRect == null) textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var healthTMP = textGo.GetComponent<TextMeshProUGUI>();
            if (healthTMP == null) healthTMP = textGo.AddComponent<TextMeshProUGUI>();
            healthTMP.text = "100 / 100";
            healthTMP.fontSize = 15f;
            healthTMP.fontStyle = FontStyles.Bold;
            healthTMP.alignment = TextAlignmentOptions.Center;
            healthTMP.color = Color.white;

            // 5. Wire PlayerHealthUI on Canvas
            var healthUI = canvasGo.GetComponent<PlayerHealthUI>();
            if (healthUI == null) healthUI = canvasGo.AddComponent<PlayerHealthUI>();

            healthUI.SetReferences(null, spawner, slider, healthTMP, labelTMP);

            var sHealthUI = new SerializedObject(healthUI);
            sHealthUI.FindProperty("playerSpawner").objectReferenceValue = spawner;
            sHealthUI.FindProperty("healthSlider").objectReferenceValue = slider;
            sHealthUI.FindProperty("healthText").objectReferenceValue = healthTMP;
            sHealthUI.FindProperty("labelText").objectReferenceValue = labelTMP;
            sHealthUI.ApplyModifiedProperties();
            EditorUtility.SetDirty(healthUI);
            EditorUtility.SetDirty(canvasGo);
        }

        public static void AdjustDungeonPrototypeArena()
        {
            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);

            // Floor: 30x30 -> 34x34 (+13.3% linear width/depth, ~28% area)
            var floorGo = GameObject.Find("Floor");
            if (floorGo != null)
            {
                floorGo.transform.localScale = new Vector3(34f, 1f, 34f);
                EditorUtility.SetDirty(floorGo);
            }

            // Boundary Walls: expand from ±15 to ±17
            var northWall = GameObject.Find("Wall_North");
            if (northWall != null)
            {
                northWall.transform.position = new Vector3(0f, 1.5f, 17f);
                northWall.transform.localScale = new Vector3(34f, 3f, 1f);
                EditorUtility.SetDirty(northWall);
            }

            var southWall = GameObject.Find("Wall_South");
            if (southWall != null)
            {
                southWall.transform.position = new Vector3(0f, 1.5f, -17f);
                southWall.transform.localScale = new Vector3(34f, 3f, 1f);
                EditorUtility.SetDirty(southWall);
            }

            var eastWall = GameObject.Find("Wall_East");
            if (eastWall != null)
            {
                eastWall.transform.position = new Vector3(17f, 1.5f, 0f);
                eastWall.transform.localScale = new Vector3(1f, 3f, 34f);
                EditorUtility.SetDirty(eastWall);
            }

            var westWall = GameObject.Find("Wall_West");
            if (westWall != null)
            {
                westWall.transform.position = new Vector3(-17f, 1.5f, 0f);
                westWall.transform.localScale = new Vector3(1f, 3f, 34f);
                EditorUtility.SetDirty(westWall);
            }

            // Pillars: push outward slightly from (±5, ±5) to (±6, ±6) to maximize central combat space
            var nePillar = GameObject.Find("Pillar_NE");
            if (nePillar != null)
            {
                nePillar.transform.position = new Vector3(6f, 1.5f, 6f);
                EditorUtility.SetDirty(nePillar);
            }

            var swPillar = GameObject.Find("Pillar_SW");
            if (swPillar != null)
            {
                swPillar.transform.position = new Vector3(-6f, 1.5f, -6f);
                EditorUtility.SetDirty(swPillar);
            }

            // SpawnPoints: push outward to (±11.5, ±11.5)
            var sp1 = GameObject.Find("SpawnPoint_01");
            if (sp1 != null) { sp1.transform.position = new Vector3(-11.5f, 0f, 11.5f); EditorUtility.SetDirty(sp1); }

            var sp2 = GameObject.Find("SpawnPoint_02");
            if (sp2 != null) { sp2.transform.position = new Vector3(11.5f, 0f, 11.5f); EditorUtility.SetDirty(sp2); }

            var sp3 = GameObject.Find("SpawnPoint_03");
            if (sp3 != null) { sp3.transform.position = new Vector3(11.5f, 0f, -11.5f); EditorUtility.SetDirty(sp3); }

            var sp4 = GameObject.Find("SpawnPoint_04");
            if (sp4 != null) { sp4.transform.position = new Vector3(-11.5f, 0f, -11.5f); EditorUtility.SetDirty(sp4); }

            // Configure PlayerHealthUI in Dungeon_Prototype
            SetupPlayerHealthUIInScene(scene);

            CleanAllVerifiers();
            EditorSceneManager.SaveScene(scene, DungeonPrototypeScenePath);
            Debug.Log("[Milestone 9 Polish] Adjusted Dungeon_Prototype arena (34x34) and configured PlayerHealthUI.");
        }

        public static void SetupDungeon02Polish()
        {
            var scene = EditorSceneManager.OpenScene(Dungeon02ScenePath, OpenSceneMode.Single);
            SetupPlayerHealthUIInScene(scene);
            CleanAllVerifiers();
            EditorSceneManager.SaveScene(scene, Dungeon02ScenePath);
            Debug.Log("[Milestone 9 Polish] Configured PlayerHealthUI in Dungeon_02.");
        }

        [MenuItem("DungeonRoguelite/Phase 9/Setup Polish Pass")]
        public static void SetupPolishPass()
        {
            Debug.Log("[Milestone 9 Polish] Setting up World Map readability, Player Health UI, and Arena adjustment...");

            // 1. World Map setup with hollow outline cards & high contrast text
            SetupGate9_4();

            // 2. Adjust Dungeon 1 arena size & add PlayerHealthUI
            AdjustDungeonPrototypeArena();

            // 3. Add PlayerHealthUI to Dungeon 2 while preserving 30x30 tighter layout
            SetupDungeon02Polish();

            // 4. Ensure Build Settings: 4 scenes in exact order
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(CharacterSelectionScenePath, true),
                new EditorBuildSettingsScene(WorldMapScenePath, true),
                new EditorBuildSettingsScene(DungeonPrototypeScenePath, true),
                new EditorBuildSettingsScene(Dungeon02ScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            Debug.Log("[Milestone 9 Polish] SetupPolishPass completed successfully.");
        }

        [MenuItem("DungeonRoguelite/Phase 9/Run Polish Verification")]
        public static void RunPolishVerification()
        {
            Debug.Log("[POLISH VERIFIER] Running Polish Pass setup before verification...");
            SetupPolishPass();

            var scene = EditorSceneManager.OpenScene(DungeonPrototypeScenePath, OpenSceneMode.Single);
            CleanAllVerifiers();

            var verifierGo = new GameObject("Polish_VerifierRunner");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone9_Polish_Verifier>();

            Debug.Log("[POLISH VERIFIER] Entering Play Mode for automated verification...");
            EditorApplication.EnterPlaymode();
        }
    }
}
