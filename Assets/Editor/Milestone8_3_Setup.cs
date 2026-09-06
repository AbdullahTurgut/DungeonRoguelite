using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.InputSystem;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Player;
using DungeonRoguelite.Weapons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Combat;

namespace DungeonRoguelite.Editor
{
    public static class Milestone8_3_Setup
    {
        public const string ArcherAssetPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";
        public const string ArcherPrefabPath = "Assets/Prefabs/Characters/Archer.prefab";
        public const string ArrowPrefabPath = "Assets/Prefabs/Weapons/Arrow.prefab";
        public const string WarriorPrefabPath = "Assets/Prefabs/Characters/Warrior.prefab";
        public const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("DungeonRoguelite/Setup Milestone 8.3")]
        public static void Setup()
        {
            Debug.Log("[Milestone 8.3] Setting up Archer assets, BowWeapon, Arrow.prefab, and Archer.prefab...");

            EnsureDirectories();
            CreateOrUpdateArrowPrefab();
            CreateOrUpdateArcherAsset();
            CreateOrUpdateArcherPrefab();
            LinkArcherPrefabToAsset();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Milestone 8.3] Setup completed successfully.");
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

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Weapons"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }
                AssetDatabase.CreateFolder("Assets/Prefabs", "Weapons");
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

        public static void CreateOrUpdateArrowPrefab()
        {
            GameObject arrowGo = new GameObject("Arrow");
            arrowGo.tag = "Untagged";

            // ArrowProjectile component
            var projectile = arrowGo.AddComponent<ArrowProjectile>();

            // Small trigger collider
            var col = arrowGo.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.radius = 0.12f;
            col.height = 0.8f;
            col.direction = 2; // Z-axis
            col.center = Vector3.zero;

            // Kinematic Rigidbody
            var rb = arrowGo.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Child visual
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(arrowGo.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.08f, 0.08f, 0.7f);

            // Remove box collider from visual primitive
            var visualCol = visual.GetComponent<Collider>();
            if (visualCol != null)
            {
                Object.DestroyImmediate(visualCol);
            }

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(arrowGo, ArrowPrefabPath);
            Object.DestroyImmediate(arrowGo);

            Debug.Log($"[Milestone 8.3] Created Arrow prefab at: {ArrowPrefabPath}");
        }

        public static void CreateOrUpdateArcherAsset()
        {
            var archerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
            if (archerAsset == null)
            {
                archerAsset = ScriptableObject.CreateInstance<CharacterDefinition>();
                AssetDatabase.CreateAsset(archerAsset, ArcherAssetPath);
            }

            var so = new SerializedObject(archerAsset);
            so.FindProperty("id").stringValue = "archer";
            so.FindProperty("displayName").stringValue = "Archer";
            so.FindProperty("description").stringValue = "A nimble marksman equipped with a bow for long-range precision attacks and agile kiting.";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(archerAsset);

            Debug.Log($"[Milestone 8.3] Configured Character_Archer asset at: {ArcherAssetPath}");
        }

        public static void CreateOrUpdateArcherPrefab()
        {
            // Instantiate clean Warrior as base structure to clone shared player setup
            var warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefabPath);
            GameObject archerGo;

            if (warriorPrefab != null)
            {
                archerGo = Object.Instantiate(warriorPrefab);
                archerGo.name = "Archer";
            }
            else
            {
                archerGo = new GameObject("Archer");
            }

            try
            {
                archerGo.tag = "Player";

                // Ensure CharacterController
                var cc = archerGo.GetComponent<CharacterController>();
                if (cc == null) cc = archerGo.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.5f;
                cc.center = new Vector3(0f, 1f, 0f);

                // Configure PlayerMovement
                var movement = archerGo.GetComponent<PlayerMovement>();
                if (movement == null) movement = archerGo.AddComponent<PlayerMovement>();
                var soMovement = new SerializedObject(movement);
                soMovement.FindProperty("moveSpeed").floatValue = 6.5f;
                soMovement.FindProperty("gravity").floatValue = 20f;
                soMovement.ApplyModifiedPropertiesWithoutUndo();

                // Configure PlayerAim
                var aim = archerGo.GetComponent<PlayerAim>();
                if (aim == null) aim = archerGo.AddComponent<PlayerAim>();

                // Configure PlayerHealth
                var health = archerGo.GetComponent<PlayerHealth>();
                if (health == null) health = archerGo.AddComponent<PlayerHealth>();
                var soHealth = new SerializedObject(health);
                soHealth.FindProperty("maxHealth").floatValue = 100f;
                soHealth.ApplyModifiedPropertiesWithoutUndo();

                // Configure PlayerStats
                var stats = archerGo.GetComponent<PlayerStats>();
                if (stats == null) stats = archerGo.AddComponent<PlayerStats>();

                // Configure PlayerExperience
                var exp = archerGo.GetComponent<PlayerExperience>();
                if (exp == null) exp = archerGo.AddComponent<PlayerExperience>();

                // Remove MeleeWeapon
                var melee = archerGo.GetComponent<MeleeWeapon>();
                if (melee != null)
                {
                    Object.DestroyImmediate(melee);
                }

                // Add and configure BowWeapon
                var bow = archerGo.GetComponent<BowWeapon>();
                if (bow == null) bow = archerGo.AddComponent<BowWeapon>();
                var soBow = new SerializedObject(bow);
                soBow.FindProperty("damage").floatValue = 20f;
                soBow.FindProperty("attackCooldown").floatValue = 0.6f;
                soBow.FindProperty("projectileSpeed").floatValue = 18f;
                soBow.FindProperty("projectileLifetime").floatValue = 2.0f;

                var arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath);
                if (arrowPrefab != null)
                {
                    soBow.FindProperty("arrowPrefab").objectReferenceValue = arrowPrefab;
                }

                // Setup visual hierarchy & ProjectileSpawnPoint
                Transform weaponAnchor = archerGo.transform.Find("Visual/WeaponAnchor");
                if (weaponAnchor == null)
                {
                    Transform visual = archerGo.transform.Find("Visual");
                    if (visual == null)
                    {
                        var visualGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        visualGo.name = "Visual";
                        visualGo.transform.SetParent(archerGo.transform, false);
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

                // Remove SwordVisual if present
                Transform swordVisual = weaponAnchor.Find("SwordVisual");
                if (swordVisual != null)
                {
                    Object.DestroyImmediate(swordVisual.gameObject);
                }

                // Create or update BowVisual
                Transform bowVisual = weaponAnchor.Find("BowVisual");
                if (bowVisual == null)
                {
                    var bowGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bowGo.name = "BowVisual";
                    bowGo.transform.SetParent(weaponAnchor, false);
                    bowGo.transform.localPosition = new Vector3(0f, 0f, 0.35f);
                    bowGo.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
                    var col = bowGo.GetComponent<Collider>();
                    if (col != null) Object.DestroyImmediate(col);
                    bowVisual = bowGo.transform;
                }

                // Create or update ProjectileSpawnPoint
                Transform spawnPoint = weaponAnchor.Find("ProjectileSpawnPoint");
                if (spawnPoint == null)
                {
                    var spawnGo = new GameObject("ProjectileSpawnPoint");
                    spawnGo.transform.SetParent(weaponAnchor, false);
                    spawnGo.transform.localPosition = new Vector3(0f, 0f, 0.8f);
                    spawnPoint = spawnGo.transform;
                }
                soBow.FindProperty("projectileSpawnPoint").objectReferenceValue = spawnPoint;
                soBow.ApplyModifiedPropertiesWithoutUndo();

                // Configure PlayerAttack
                var attack = archerGo.GetComponent<PlayerAttack>();
                if (attack == null) attack = archerGo.AddComponent<PlayerAttack>();
                var soAttack = new SerializedObject(attack);
                soAttack.FindProperty("primaryWeapon").objectReferenceValue = bow;
                soAttack.ApplyModifiedPropertiesWithoutUndo();

                // Configure PlayableCharacter
                var playable = archerGo.GetComponent<PlayableCharacter>();
                if (playable == null) playable = archerGo.AddComponent<PlayableCharacter>();
                var archerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
                if (archerAsset != null)
                {
                    var soPlayable = new SerializedObject(playable);
                    soPlayable.FindProperty("characterDefinition").objectReferenceValue = archerAsset;
                    soPlayable.ApplyModifiedPropertiesWithoutUndo();
                }

                // Save prefab asset
                PrefabUtility.SaveAsPrefabAsset(archerGo, ArcherPrefabPath);
                Debug.Log($"[Milestone 8.3] Created Archer prefab at: {ArcherPrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(archerGo);
            }
        }

        public static void LinkArcherPrefabToAsset()
        {
            var archerAsset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
            var archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherPrefabPath);

            if (archerAsset != null && archerPrefab != null)
            {
                var so = new SerializedObject(archerAsset);
                so.FindProperty("characterPrefab").objectReferenceValue = archerPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(archerAsset);
                Debug.Log("[Milestone 8.3] Linked Archer.prefab to Character_Archer.asset.");
            }
        }

        [MenuItem("DungeonRoguelite/Run Milestone 8.3 PlayMode Verification")]
        public static void RunPlayModeVerification()
        {
            Debug.Log("[Milestone 8.3] Launching PlayMode verification harness...");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Dungeons/Dungeon_Prototype.unity", OpenSceneMode.Single);
            CleanupVerifierFromScene();

            var verifierGo = new GameObject("Milestone8_3_RuntimeVerifier");
            verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone8_3_Verifier>();
            Debug.Log("[Milestone 8.3] Attached transient Milestone8_3_RuntimeVerifier in memory.");

            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Run Targeted Check 33")]
        public static void RunTargetedCheck33()
        {
            Debug.Log("[Milestone 8.3] Launching targeted Check 33 harness...");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Dungeons/Dungeon_Prototype.unity", OpenSceneMode.Single);
            CleanupVerifierFromScene();

            var verifierGo = new GameObject("Milestone8_3_RuntimeVerifier");
            var verifier = verifierGo.AddComponent<DungeonRoguelite.Tests.Milestone8_3_Verifier>();
            verifier.TargetedCheck33Only = true;
            Debug.Log("[Milestone 8.3] Attached transient Milestone8_3_RuntimeVerifier with TargetedCheck33Only = true.");

            EditorApplication.EnterPlaymode();
        }

        public static void CleanupVerifierFromScene()
        {
            var oldVerifier = GameObject.Find("Milestone8_3_RuntimeVerifier");
            if (oldVerifier != null)
            {
                Object.DestroyImmediate(oldVerifier);
            }

            var allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var c in allComponents)
            {
                if (c != null && c.GetType().Name.Contains("Verifier"))
                {
                    Debug.Log($"[Milestone 8.3] Removing verifier component from scene: {c.GetType().Name}");
                    Object.DestroyImmediate(c.gameObject);
                }
            }
        }
    }
}
