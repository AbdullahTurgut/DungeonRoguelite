using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Player;
using DungeonRoguelite.Tests;
using DungeonRoguelite.UI;

namespace DungeonRoguelite.Editor
{
    public static class Milestone5_1_Setup
    {
        private const string PickupsDir = "Assets/Prefabs/Pickups";
        private const string PickupPrefabPath = "Assets/Prefabs/Pickups/ExperiencePickup.prefab";
        private const string MaterialsDir = "Assets/Materials/Pickups";
        private const string MaterialPath = "Assets/Materials/Pickups/M_ExperiencePickup.mat";
        private const string PlayerPrefabPath = "Assets/Prefabs/Characters/Player.prefab";
        private const string ZombiePrefabPath = "Assets/Prefabs/Enemies/Zombie.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 5.1")]
        public static void Setup()
        {
            Debug.Log("[Milestone 5.1] Setting up Experience system assets, prefabs, and prototype scene...");
            ImportTMPEssentialsIfNeeded();
            CreateOrUpdatePickupMaterial();
            CreateOrUpdatePickupPrefab();
            UpdatePlayerPrefab();
            UpdateZombiePrefab();
            UpdatePrototypeScene();
            Debug.Log("[Milestone 5.1] Setup completed successfully.");
        }

        public static void ImportTMPEssentialsIfNeeded()
        {
            if (!Directory.Exists("Assets/TextMesh Pro"))
            {
                string packagePath = Path.GetFullPath("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage");
                if (File.Exists(packagePath))
                {
                    Debug.Log("[Milestone 5.1] Importing TMP Essential Resources...");
                    AssetDatabase.ImportPackage(packagePath, false);
                    AssetDatabase.Refresh();
                }
            }
        }

        public static void CreateOrUpdatePickupMaterial()
        {
            if (!Directory.Exists(MaterialsDir))
            {
                Directory.CreateDirectory(MaterialsDir);
                AssetDatabase.Refresh();
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
                if (shader == null) shader = Shader.Find("Standard");

                mat = new Material(shader);
                mat.color = new Color(0f, 0.85f, 1f, 1f); // Cyan
                AssetDatabase.CreateAsset(mat, MaterialPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[Milestone 5.1] Created pickup material at {MaterialPath}.");
            }
        }

        public static void CreateOrUpdatePickupPrefab()
        {
            if (!Directory.Exists(PickupsDir))
            {
                Directory.CreateDirectory(PickupsDir);
                AssetDatabase.Refresh();
            }

            bool wasLoadedFromPrefab = File.Exists(PickupPrefabPath);
            GameObject rootGo = wasLoadedFromPrefab ? PrefabUtility.LoadPrefabContents(PickupPrefabPath) : new GameObject("ExperiencePickup");

            // Configure SphereCollider trigger (required by ExperiencePickup)
            var sphereCol = rootGo.GetComponent<SphereCollider>();
            if (sphereCol == null) sphereCol = rootGo.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.8f;
            sphereCol.center = Vector3.zero;

            // Configure kinematic Rigidbody
            var rb = rootGo.GetComponent<Rigidbody>();
            if (rb == null) rb = rootGo.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Configure ExperiencePickup component
            var pickup = rootGo.GetComponent<ExperiencePickup>();
            if (pickup == null) pickup = rootGo.AddComponent<ExperiencePickup>();
            var sPickup = new SerializedObject(pickup);
            sPickup.FindProperty("xpValue").intValue = 10;
            sPickup.ApplyModifiedProperties();

            // Configure visual child
            var visualTransform = rootGo.transform.Find("Visual");
            GameObject visualGo;
            if (visualTransform == null)
            {
                visualGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visualGo.name = "Visual";
                visualGo.transform.SetParent(rootGo.transform, false);
                visualGo.transform.localPosition = Vector3.zero;
                visualGo.transform.localRotation = Quaternion.Euler(45f, 45f, 45f);
                visualGo.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

                // Remove collider from visual child
                var boxCol = visualGo.GetComponent<BoxCollider>();
                if (boxCol != null) Object.DestroyImmediate(boxCol);
            }
            else
            {
                visualGo = visualTransform.gameObject;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat != null)
            {
                var renderer = visualGo.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.sharedMaterial = mat;
            }

            PrefabUtility.SaveAsPrefabAsset(rootGo, PickupPrefabPath);
            if (wasLoadedFromPrefab)
            {
                PrefabUtility.UnloadPrefabContents(rootGo);
            }
            else
            {
                Object.DestroyImmediate(rootGo);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Milestone 5.1] ExperiencePickup prefab saved to {PickupPrefabPath}.");
        }

        public static void UpdatePlayerPrefab()
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"[Milestone 5.1] Player prefab not found at {PlayerPrefabPath}");
                return;
            }

            GameObject playerRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            var playerExp = playerRoot.GetComponent<PlayerExperience>();
            if (playerExp == null)
            {
                playerExp = playerRoot.AddComponent<PlayerExperience>();
            }

            var sExp = new SerializedObject(playerExp);
            sExp.FindProperty("baseRequiredXP").intValue = 100;
            sExp.FindProperty("xpGrowthMultiplier").floatValue = 1.5f;
            sExp.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(playerRoot, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(playerRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Milestone 5.1] Player prefab updated with PlayerExperience at {PlayerPrefabPath}.");
        }

        public static void UpdateZombiePrefab()
        {
            if (!File.Exists(ZombiePrefabPath))
            {
                Debug.LogError($"[Milestone 5.1] Zombie prefab not found at {ZombiePrefabPath}");
                return;
            }

            GameObject zombieRoot = PrefabUtility.LoadPrefabContents(ZombiePrefabPath);
            var expReward = zombieRoot.GetComponent<ExperienceReward>();
            if (expReward == null)
            {
                expReward = zombieRoot.AddComponent<ExperienceReward>();
            }

            GameObject pickupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PickupPrefabPath);
            var sReward = new SerializedObject(expReward);
            sReward.FindProperty("xpAmount").intValue = 10;
            sReward.FindProperty("pickupPrefab").objectReferenceValue = pickupPrefab;
            sReward.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(zombieRoot, ZombiePrefabPath);
            PrefabUtility.UnloadPrefabContents(zombieRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Milestone 5.1] Zombie prefab updated with ExperienceReward at {ZombiePrefabPath}.");
        }

        public static void UpdatePrototypeScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Clean up previous milestone verifiers
            string[] oldVerifiers = new string[]
            {
                "Milestone1_1_RuntimeVerifier",
                "Milestone1_2_RuntimeVerifier",
                "Milestone1_3_RuntimeVerifier",
                "Milestone2_1_RuntimeVerifier",
                "Milestone2_2_RuntimeVerifier",
                "Milestone3_1_RuntimeVerifier",
                "Milestone4_1_RuntimeVerifier"
            };

            foreach (var verifierName in oldVerifiers)
            {
                var oldGo = GameObject.Find(verifierName);
                if (oldGo != null)
                {
                    Object.DestroyImmediate(oldGo);
                }
            }

            // Ensure Player has PlayerExperience
            var playerGo = GameObject.FindWithTag("Player");
            PlayerExperience playerExp = null;
            if (playerGo != null)
            {
                playerExp = playerGo.GetComponent<PlayerExperience>();
                if (playerExp == null)
                {
                    playerExp = playerGo.AddComponent<PlayerExperience>();
                }
            }
            else
            {
                Debug.LogError("[Milestone 5.1] Player GameObject with tag 'Player' not found in scene.");
            }

            // Setup EventSystem
            var eventSystemGo = GameObject.Find("EventSystem");
            if (eventSystemGo == null)
            {
                eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Setup Canvas
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Canvas");
            }
            var canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null) canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var raycaster = canvasGo.GetComponent<GraphicRaycaster>();
            if (raycaster == null) raycaster = canvasGo.AddComponent<GraphicRaycaster>();

            // Setup ExperienceHUD
            var hudTransform = canvasGo.transform.Find("ExperienceHUD");
            GameObject hudGo;
            if (hudTransform == null)
            {
                hudGo = new GameObject("ExperienceHUD");
                hudGo.transform.SetParent(canvasGo.transform, false);
            }
            else
            {
                hudGo = hudTransform.gameObject;
            }

            var hudRect = hudGo.GetComponent<RectTransform>();
            if (hudRect == null) hudRect = hudGo.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0.5f, 1f);
            hudRect.anchorMax = new Vector2(0.5f, 1f);
            hudRect.pivot = new Vector2(0.5f, 1f);
            hudRect.anchoredPosition = new Vector2(0f, -25f);
            hudRect.sizeDelta = new Vector2(600f, 60f);

            // Setup XPSlider
            var sliderTransform = hudGo.transform.Find("XPSlider");
            GameObject sliderGo;
            if (sliderTransform == null)
            {
                sliderGo = new GameObject("XPSlider");
                sliderGo.transform.SetParent(hudGo.transform, false);
            }
            else
            {
                sliderGo = sliderTransform.gameObject;
            }
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            if (sliderRect == null) sliderRect = sliderGo.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.5f);
            sliderRect.anchorMax = new Vector2(1f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(0f, 10f);
            sliderRect.sizeDelta = new Vector2(0f, 24f);

            var slider = sliderGo.GetComponent<Slider>();
            if (slider == null) slider = sliderGo.AddComponent<Slider>();

            // Background
            var bgTransform = sliderGo.transform.Find("Background");
            GameObject bgGo = bgTransform != null ? bgTransform.gameObject : new GameObject("Background");
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRect = bgGo.GetComponent<RectTransform>();
            if (bgRect == null) bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bgGo.GetComponent<Image>();
            if (bgImage == null) bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.12f, 0.14f, 0.18f, 0.9f);

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
            var fillImage = fillGo.GetComponent<Image>();
            if (fillImage == null) fillImage = fillGo.AddComponent<Image>();
            fillImage.color = new Color(0f, 0.8f, 1f, 1f);

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false;

            // Setup LevelText
            var textTransform = hudGo.transform.Find("LevelText");
            GameObject textGo = textTransform != null ? textTransform.gameObject : new GameObject("LevelText");
            textGo.transform.SetParent(hudGo.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            if (textRect == null) textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0f);
            textRect.anchoredPosition = new Vector2(0f, -5f);
            textRect.sizeDelta = new Vector2(0f, 25f);

            var levelText = textGo.GetComponent<TextMeshProUGUI>();
            if (levelText == null) levelText = textGo.AddComponent<TextMeshProUGUI>();
            levelText.text = "Level 1 (0 / 100 XP)";
            levelText.fontSize = 18f;
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.color = Color.white;

            // Setup PlayerExperienceUI
            var expUI = hudGo.GetComponent<PlayerExperienceUI>();
            if (expUI == null) expUI = hudGo.AddComponent<PlayerExperienceUI>();

            var sExpUI = new SerializedObject(expUI);
            sExpUI.FindProperty("xpSlider").objectReferenceValue = slider;
            sExpUI.FindProperty("levelText").objectReferenceValue = levelText;
            if (playerExp != null)
            {
                sExpUI.FindProperty("playerExperience").objectReferenceValue = playerExp;
            }
            sExpUI.ApplyModifiedProperties();

            // Setup Milestone 5.1 Verifier
            var verifierGo = GameObject.Find("Milestone5_1_RuntimeVerifier");
            if (verifierGo == null)
            {
                verifierGo = new GameObject("Milestone5_1_RuntimeVerifier");
            }

            if (verifierGo.GetComponent<Milestone5_1_Verifier>() == null)
            {
                verifierGo.AddComponent<Milestone5_1_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 5.1] Scene {ScenePath} configured with Experience Canvas, HUD, and Milestone5_1_Verifier.");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 5.1 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 5.1] Running PlayMode verification...");
            ImportTMPEssentialsIfNeeded();
            CreateOrUpdatePickupMaterial();
            CreateOrUpdatePickupPrefab();
            UpdatePlayerPrefab();
            UpdateZombiePrefab();
            UpdatePrototypeScene();
            EditorApplication.EnterPlaymode();
        }
    }
}
