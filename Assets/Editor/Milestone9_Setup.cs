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
    }
}
