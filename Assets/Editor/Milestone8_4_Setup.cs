using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Player;
using DungeonRoguelite.Weapons;
using DungeonRoguelite.Combat;
using DungeonRoguelite.Experience;

namespace DungeonRoguelite.Editor
{
    public static class Milestone8_4_Setup
    {
        public const string GunnerAssetPath = "Assets/ScriptableObjects/Characters/Character_Gunner.asset";
        public const string GunnerPrefabPath = "Assets/Prefabs/Characters/Gunner.prefab";
        public const string WarriorPrefabPath = "Assets/Prefabs/Characters/Warrior.prefab";

        [MenuItem("DungeonRoguelite/Setup Milestone 8.4")]
        public static void Setup()
        {
            Debug.Log("[Milestone 8.4] Setting up Gunner assets, RifleWeapon, and Gunner.prefab...");

            EnsureDirectories();
            CreateOrUpdateGunnerAsset();
            CreateOrUpdateGunnerPrefab();
            LinkGunnerPrefabToAsset();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Milestone 8.4] Setup completed successfully.");
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

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Characters"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }
                AssetDatabase.CreateFolder("Assets/Prefabs", "Characters");
            }
        }

        public static void CreateOrUpdateGunnerAsset()
        {
            var gunnerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(GunnerAssetPath);
            if (gunnerAsset == null)
            {
                gunnerAsset = ScriptableObject.CreateInstance<CharacterDefinition>();
                AssetDatabase.CreateAsset(gunnerAsset, GunnerAssetPath);
            }

            var so = new SerializedObject(gunnerAsset);
            so.FindProperty("id").stringValue = "gunner";
            so.FindProperty("displayName").stringValue = "Gunner";
            so.FindProperty("description").stringValue = "A rapid-fire marksman utilizing a high-precision rifle for fast, instant-impact ranged engagements.";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gunnerAsset);

            Debug.Log($"[Milestone 8.4] Configured Character_Gunner asset at: {GunnerAssetPath}");
        }

        public static void CreateOrUpdateGunnerPrefab()
        {
            var warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefabPath);
            GameObject gunnerGo;

            if (warriorPrefab != null)
            {
                gunnerGo = Object.Instantiate(warriorPrefab);
                gunnerGo.name = "Gunner";
            }
            else
            {
                gunnerGo = new GameObject("Gunner");
            }

            try
            {
                gunnerGo.tag = "Player";

                // Ensure CharacterController
                var cc = gunnerGo.GetComponent<CharacterController>();
                if (cc == null) cc = gunnerGo.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.5f;
                cc.center = new Vector3(0f, 1f, 0f);

                // Configure PlayerMovement
                var movement = gunnerGo.GetComponent<PlayerMovement>();
                if (movement == null) movement = gunnerGo.AddComponent<PlayerMovement>();
                var soMovement = new SerializedObject(movement);
                soMovement.FindProperty("moveSpeed").floatValue = 6.0f;
                soMovement.FindProperty("gravity").floatValue = 20f;
                soMovement.ApplyModifiedPropertiesWithoutUndo();

                // Configure PlayerAim
                var aim = gunnerGo.GetComponent<PlayerAim>();
                if (aim == null) aim = gunnerGo.AddComponent<PlayerAim>();

                // Configure PlayerHealth
                var health = gunnerGo.GetComponent<PlayerHealth>();
                if (health == null) health = gunnerGo.AddComponent<PlayerHealth>();
                var soHealth = new SerializedObject(health);
                soHealth.FindProperty("maxHealth").floatValue = 100f;
                soHealth.ApplyModifiedPropertiesWithoutUndo();

                // Configure PlayerStats
                var stats = gunnerGo.GetComponent<PlayerStats>();
                if (stats == null) stats = gunnerGo.AddComponent<PlayerStats>();

                // Configure PlayerExperience
                var exp = gunnerGo.GetComponent<PlayerExperience>();
                if (exp == null) exp = gunnerGo.AddComponent<PlayerExperience>();

                // Remove MeleeWeapon or BowWeapon
                var melee = gunnerGo.GetComponent<MeleeWeapon>();
                if (melee != null) Object.DestroyImmediate(melee);

                var bow = gunnerGo.GetComponent<BowWeapon>();
                if (bow != null) Object.DestroyImmediate(bow);

                // Add and configure RifleWeapon
                var rifle = gunnerGo.GetComponent<RifleWeapon>();
                if (rifle == null) rifle = gunnerGo.AddComponent<RifleWeapon>();
                var soRifle = new SerializedObject(rifle);
                soRifle.FindProperty("damage").floatValue = 10f;
                soRifle.FindProperty("attackCooldown").floatValue = 0.18f;
                soRifle.FindProperty("range").floatValue = 25f;
                soRifle.FindProperty("castRadius").floatValue = 0.1f;
                soRifle.FindProperty("targetLayers").intValue = ~0;

                // Setup visual hierarchy & MuzzlePoint
                Transform weaponAnchor = gunnerGo.transform.Find("Visual/WeaponAnchor");
                if (weaponAnchor == null)
                {
                    Transform visual = gunnerGo.transform.Find("Visual");
                    if (visual == null)
                    {
                        var visualGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        visualGo.name = "Visual";
                        visualGo.transform.SetParent(gunnerGo.transform, false);
                        visualGo.transform.localPosition = new Vector3(0f, 1f, 0f);
                        var col = visualGo.GetComponent<Collider>();
                        if (col != null) Object.DestroyImmediate(col);
                        visual = visualGo.transform;
                    }

                    var anchorGo = new GameObject("WeaponAnchor");
                    anchorGo.transform.SetParent(visual, false);
                    anchorGo.transform.localPosition = new Vector3(0.35f, 0f, 0.2f);
                    weaponAnchor = anchorGo.transform;
                }

                // Remove SwordVisual or BowVisual if present
                Transform swordVisual = weaponAnchor.Find("SwordVisual");
                if (swordVisual != null) Object.DestroyImmediate(swordVisual.gameObject);

                Transform bowVisual = weaponAnchor.Find("BowVisual");
                if (bowVisual != null) Object.DestroyImmediate(bowVisual.gameObject);

                Transform projSpawnPoint = weaponAnchor.Find("ProjectileSpawnPoint");
                if (projSpawnPoint != null) Object.DestroyImmediate(projSpawnPoint.gameObject);

                // Create or update RifleVisual
                Transform rifleVisual = weaponAnchor.Find("RifleVisual");
                if (rifleVisual == null)
                {
                    var rifleGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rifleGo.name = "RifleVisual";
                    rifleGo.transform.SetParent(weaponAnchor, false);
                    rifleGo.transform.localPosition = new Vector3(0f, 0f, 0.35f);
                    rifleGo.transform.localScale = new Vector3(0.1f, 0.12f, 0.8f);
                    var col = rifleGo.GetComponent<Collider>();
                    if (col != null) Object.DestroyImmediate(col);
                    rifleVisual = rifleGo.transform;
                }

                // Create or update MuzzlePoint
                Transform muzzlePoint = rifleVisual.Find("MuzzlePoint");
                if (muzzlePoint == null)
                {
                    var muzzleGo = new GameObject("MuzzlePoint");
                    muzzleGo.transform.SetParent(rifleVisual, false);
                    muzzleGo.transform.localPosition = new Vector3(0f, 0f, 0.45f);
                    muzzlePoint = muzzleGo.transform;
                }

                soRifle.FindProperty("muzzlePoint").objectReferenceValue = muzzlePoint;
                soRifle.ApplyModifiedPropertiesWithoutUndo();

                // Configure PlayerAttack
                var attack = gunnerGo.GetComponent<PlayerAttack>();
                if (attack == null) attack = gunnerGo.AddComponent<PlayerAttack>();
                var soAttack = new SerializedObject(attack);
                soAttack.FindProperty("primaryWeapon").objectReferenceValue = rifle;
                soAttack.ApplyModifiedPropertiesWithoutUndo();

                // Configure PlayableCharacter
                var playable = gunnerGo.GetComponent<PlayableCharacter>();
                if (playable == null) playable = gunnerGo.AddComponent<PlayableCharacter>();
                var gunnerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(GunnerAssetPath);
                if (gunnerAsset != null)
                {
                    var soPlayable = new SerializedObject(playable);
                    soPlayable.FindProperty("characterDefinition").objectReferenceValue = gunnerAsset;
                    soPlayable.ApplyModifiedPropertiesWithoutUndo();
                }

                // Save prefab asset
                PrefabUtility.SaveAsPrefabAsset(gunnerGo, GunnerPrefabPath);
                Debug.Log($"[Milestone 8.4] Created Gunner prefab at: {GunnerPrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(gunnerGo);
            }
        }

        public static void LinkGunnerPrefabToAsset()
        {
            var gunnerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(GunnerAssetPath);
            var gunnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GunnerPrefabPath);

            if (gunnerAsset != null && gunnerPrefab != null)
            {
                var so = new SerializedObject(gunnerAsset);
                so.FindProperty("characterPrefab").objectReferenceValue = gunnerPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gunnerAsset);
                Debug.Log("[Milestone 8.4] Linked Gunner.prefab to Character_Gunner.asset.");
            }
        }

        [MenuItem("DungeonRoguelite/Run Milestone 8.4 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 8.4] Launching PlayMode verification harness...");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Dungeons/Dungeon_Prototype.unity", OpenSceneMode.Single);
            CleanupVerifierFromScene();

            var verifierGo = new GameObject("Milestone8_4_RuntimeVerifier");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone8_4_Verifier>();
            Debug.Log("[Milestone 8.4] Attached transient Milestone8_4_RuntimeVerifier in memory.");

            EditorApplication.EnterPlaymode();
        }

        public static void CleanupVerifierFromScene()
        {
            var oldVerifier = GameObject.Find("Milestone8_4_RuntimeVerifier");
            if (oldVerifier != null)
            {
                Object.DestroyImmediate(oldVerifier);
            }

            var allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var c in allComponents)
            {
                if (c != null && c.GetType().Name.Contains("Verifier"))
                {
                    Debug.Log($"[Milestone 8.4] Removing verifier component from scene: {c.GetType().Name}");
                    Object.DestroyImmediate(c.gameObject);
                }
            }
        }
    }
}
