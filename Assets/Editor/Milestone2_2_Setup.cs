using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DungeonRoguelite.Player;
using DungeonRoguelite.Weapons;
using DungeonRoguelite.Tests;

namespace DungeonRoguelite.Editor
{
    public static class Milestone2_2_Setup
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/Player.prefab";
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";

        [MenuItem("DungeonRoguelite/Setup Milestone 2.2")]
        public static void Setup()
        {
            Debug.Log("[Milestone 2.2] Setting up Sword weapon and verification harness...");
            UpdatePlayerPrefab();
            UpdatePrototypeScene();
            Debug.Log("[Milestone 2.2] Setup completed successfully.");
        }

        public static void UpdatePlayerPrefab()
        {
            var rootGo = PrefabUtility.LoadPrefabContents(PrefabPath);

            // 1. Add MeleeWeapon component
            var melee = rootGo.GetComponent<MeleeWeapon>();
            if (melee == null)
            {
                melee = rootGo.AddComponent<MeleeWeapon>();
            }

            var serializedMelee = new SerializedObject(melee);
            serializedMelee.FindProperty("damage").floatValue = 25f;
            serializedMelee.FindProperty("attackCooldown").floatValue = 0.5f;
            serializedMelee.FindProperty("range").floatValue = 2.0f;
            serializedMelee.FindProperty("arcAngle").floatValue = 120f;
            serializedMelee.ApplyModifiedProperties();

            // 2. Add PlayerAttack component
            var attack = rootGo.GetComponent<PlayerAttack>();
            if (attack == null)
            {
                attack = rootGo.AddComponent<PlayerAttack>();
            }

            var serializedAttack = new SerializedObject(attack);
            serializedAttack.FindProperty("equippedWeapon").objectReferenceValue = melee;
            serializedAttack.ApplyModifiedProperties();

            // 3. Add WeaponAnchor and placeholder Sword visual if not present
            var visual = rootGo.transform.Find("Visual");
            Transform parentForAnchor = visual != null ? visual : rootGo.transform;

            var existingAnchor = parentForAnchor.Find("WeaponAnchor");
            if (existingAnchor == null)
            {
                var weaponAnchorGo = new GameObject("WeaponAnchor");
                weaponAnchorGo.transform.SetParent(parentForAnchor, false);
                weaponAnchorGo.transform.localPosition = new Vector3(0.35f, 0f, 0.2f);
                weaponAnchorGo.transform.localRotation = Quaternion.identity;
                weaponAnchorGo.transform.localScale = Vector3.one;

                var swordVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                swordVisual.name = "SwordVisual";

                // Ensure NO collider exists on the weapon visual
                var boxCol = swordVisual.GetComponent<BoxCollider>();
                if (boxCol != null)
                {
                    Object.DestroyImmediate(boxCol);
                }

                swordVisual.transform.SetParent(weaponAnchorGo.transform, false);
                swordVisual.transform.localPosition = new Vector3(0f, 0f, 0.35f);
                swordVisual.transform.localRotation = Quaternion.identity;
                swordVisual.transform.localScale = new Vector3(0.08f, 0.08f, 0.7f);
            }

            PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
            PrefabUtility.UnloadPrefabContents(rootGo);

            Debug.Log($"[Milestone 2.2] Updated {PrefabPath} with MeleeWeapon, PlayerAttack, and SwordVisual.");
        }

        public static void UpdatePrototypeScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Clean up previous milestone verifiers
            string[] previousVerifiers = new string[]
            {
                "Milestone1_1_RuntimeVerifier",
                "Milestone1_2_RuntimeVerifier",
                "Milestone1_3_RuntimeVerifier",
                "Milestone2_1_RuntimeVerifier"
            };

            foreach (var verifierName in previousVerifiers)
            {
                var oldGo = GameObject.Find(verifierName);
                if (oldGo != null)
                {
                    Object.DestroyImmediate(oldGo);
                }
            }

            // Ensure Player in scene has MeleeWeapon and PlayerAttack
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                var melee = playerGo.GetComponent<MeleeWeapon>();
                if (melee == null)
                {
                    melee = playerGo.AddComponent<MeleeWeapon>();
                }
                var attack = playerGo.GetComponent<PlayerAttack>();
                if (attack == null)
                {
                    attack = playerGo.AddComponent<PlayerAttack>();
                }
            }

            // Setup Milestone 2.2 verifier
            var verifierGo = GameObject.Find("Milestone2_2_RuntimeVerifier");
            if (verifierGo == null)
            {
                verifierGo = new GameObject("Milestone2_2_RuntimeVerifier");
            }
            if (verifierGo.GetComponent<Milestone2_2_Verifier>() == null)
            {
                verifierGo.AddComponent<Milestone2_2_Verifier>();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Milestone 2.2] Configured {ScenePath} with Milestone2_2_Verifier.");
        }

        [MenuItem("DungeonRoguelite/Run Milestone 2.2 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 2.2] Running PlayMode verification...");
            UpdatePlayerPrefab();
            UpdatePrototypeScene();
            EditorApplication.EnterPlaymode();
        }
    }
}
