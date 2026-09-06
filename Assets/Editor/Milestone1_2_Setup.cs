using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DungeonRoguelite.Player;
using DungeonRoguelite.CameraControl;

namespace DungeonRoguelite.Editor
{
    public static class Milestone1_2_Setup
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/Warrior.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 1.2")]
        public static void Setup()
        {
            Debug.Log("[Milestone 1.2] Beginning setup for Camera and Aim...");

            UpdatePlayerPrefab();
            UpdatePrototypeScene();

            Debug.Log("[Milestone 1.2] Setup completed successfully.");
        }

        public static void UpdatePlayerPrefab()
        {
            var rootGo = PrefabUtility.LoadPrefabContents(PrefabPath);

            // 1. Add PlayerAim component if not present
            var aim = rootGo.GetComponent<PlayerAim>();
            if (aim == null)
            {
                aim = rootGo.AddComponent<PlayerAim>();
            }

            // 2. Add FacingIndicator to Visual child if not present
            var visual = rootGo.transform.Find("Visual");
            if (visual != null)
            {
                var existingIndicator = visual.Find("FacingIndicator");
                if (existingIndicator == null)
                {
                    var indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    indicator.name = "FacingIndicator";

                    // Remove collider to ensure strictly visual non-colliding
                    var boxCol = indicator.GetComponent<BoxCollider>();
                    if (boxCol != null)
                    {
                        Object.DestroyImmediate(boxCol);
                    }

                    indicator.transform.SetParent(visual, false);
                    indicator.transform.localPosition = new Vector3(0f, 0.5f, 0.45f);
                    indicator.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);
                    indicator.transform.localRotation = Quaternion.identity;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
            PrefabUtility.UnloadPrefabContents(rootGo);

            Debug.Log($"[Milestone 1.2] Updated {PrefabPath} with PlayerAim and FacingIndicator.");
        }

        public static void UpdatePrototypeScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            var playerGo = GameObject.FindWithTag("Player");
            var cameraGo = GameObject.FindWithTag("MainCamera");

            if (cameraGo != null && playerGo != null)
            {
                // Attach and configure CameraFollow
                var camFollow = cameraGo.GetComponent<CameraFollow>();
                if (camFollow == null)
                {
                    camFollow = cameraGo.AddComponent<CameraFollow>();
                }

                var serializedCam = new SerializedObject(camFollow);
                serializedCam.FindProperty("target").objectReferenceValue = playerGo.transform;
                serializedCam.FindProperty("offset").vector3Value = new Vector3(0f, 16f, -11f);
                serializedCam.FindProperty("fixedRotationEuler").vector3Value = new Vector3(55f, 0f, 0f);
                serializedCam.FindProperty("smoothTime").floatValue = 0.15f;
                serializedCam.ApplyModifiedProperties();

                cameraGo.transform.position = playerGo.transform.position + new Vector3(0f, 16f, -11f);
                cameraGo.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

                // Configure PlayerAim camera reference
                var aim = playerGo.GetComponent<PlayerAim>();
                if (aim != null)
                {
                    var serializedAim = new SerializedObject(aim);
                    serializedAim.FindProperty("aimCamera").objectReferenceValue = cameraGo.GetComponent<Camera>();
                    serializedAim.ApplyModifiedProperties();
                }
            }

            // Replace old verifier with Milestone1_2_Verifier
            var oldVerifier = Object.FindFirstObjectByType<Milestone1_1_Verifier>();
            if (oldVerifier != null)
            {
                Object.DestroyImmediate(oldVerifier.gameObject);
            }

            var verifierGo = GameObject.Find("Milestone1_2_RuntimeVerifier");
            if (verifierGo == null)
            {
                verifierGo = new GameObject("Milestone1_2_RuntimeVerifier");
            }
            if (verifierGo.GetComponent<Milestone1_2_Verifier>() == null)
            {
                verifierGo.AddComponent<Milestone1_2_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 1.2] Updated {ScenePath} with CameraFollow and Milestone1_2_Verifier.");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 1.2 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 1.2] Starting PlayMode verification...");
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.EnterPlaymode();
        }
    }
}
