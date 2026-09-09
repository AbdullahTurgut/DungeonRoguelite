using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Enemies;

namespace DungeonRoguelite.Editor
{
    public static class Phase11AssetBuilder
    {
        public static void Runner() => Melee("Runner", 25f, 5.5f, 1.1f, 1.3f, 6f, 0.65f, 10, 0.85f, new Color(1f, 0.45f, 0.05f));
        public static void Tank() => Melee("Tank", 150f, 1.8f, 1.8f, 2f, 22f, 1.8f, 40, 1.4f, new Color(0.18f, 0.21f, 0.25f));
        public static void Ranged()
        {
            const string projectilePath = "Assets/Prefabs/Enemies/EnemyProjectile.prefab";
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_EnemyProjectile.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, "Assets/Materials/M_EnemyProjectile.mat");
            }
            material.SetColor("_BaseColor", new Color(0.85f, 0.3f, 1f));
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.6f, 0.1f, 1f));
            var shot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            try
            {
                shot.name = "EnemyProjectile";
                Object.DestroyImmediate(shot.GetComponent<Collider>());
                shot.transform.localScale = Vector3.one * 0.3f;
                shot.GetComponent<Renderer>().sharedMaterial = material;
                shot.AddComponent<EnemyProjectile>();
                PrefabUtility.SaveAsPrefabAsset(shot, projectilePath);
            }
            finally { Object.DestroyImmediate(shot); }
            Melee("Ranged", 35, 2.8f, 7, 9, 8, 1.6f, 15, 1, new Color(0.22f, 0.06f, 0.4f));
            const string path = "Assets/Prefabs/Enemies/Ranged.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Object.DestroyImmediate(root.GetComponent<EnemyAttack>());
                var attack = root.AddComponent<EnemyRangedAttack>();
                var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = "VioletOrb";
                Object.DestroyImmediate(orb.GetComponent<Collider>());
                orb.transform.SetParent(root.transform, false);
                orb.transform.localPosition = new Vector3(0, 1.5f, 0.45f);
                orb.transform.localScale = Vector3.one * 0.3f;
                orb.GetComponent<Renderer>().sharedMaterial = material;
                var serialized = new SerializedObject(attack);
                serialized.FindProperty("projectilePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(projectilePath).GetComponent<EnemyProjectile>();
                serialized.FindProperty("muzzle").objectReferenceValue = orb.transform;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                serialized = new SerializedObject(root.GetComponent<EnemyVisualFeedback>());
                var renderers = root.GetComponentsInChildren<Renderer>();
                var bindings = serialized.FindProperty("targetRenderers");
                bindings.arraySize = renderers.Length;
                for (int i = 0; i < renderers.Length; i++) bindings.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
                AssetDatabase.SaveAssets();
                Debug.Log("[RANGED ASSETS COMPLETE]");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void Melee(string name, float hp, float speed, float stop, float range, float damage, float cooldown, int xp, float scale, Color color)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            var root = Object.Instantiate(source);
            try
            {
                root.name = name;
                Set(root.GetComponent<EnemyHealth>(), "maxHealth", hp);
                Set(root.GetComponent<EnemyMovement>(), "moveSpeed", speed);
                Set(root.GetComponent<EnemyMovement>(), "stoppingDistance", stop);
                Set(root.GetComponent<EnemyAttack>(), "damage", damage);
                Set(root.GetComponent<EnemyAttack>(), "attackRange", range);
                Set(root.GetComponent<EnemyAttack>(), "attackCooldown", cooldown);
                var controller = root.GetComponent<CharacterController>();
                controller.height = 2f * scale;
                controller.radius = 0.5f * scale;
                controller.center = Vector3.up * scale;
                var visual = root.transform.Find("Visual");
                visual.localPosition = Vector3.up * scale;
                visual.localScale = Vector3.one * scale;
                var reward = new SerializedObject(root.GetComponent<Experience.ExperienceReward>());
                reward.FindProperty("xpAmount").intValue = xp;
                reward.ApplyModifiedPropertiesWithoutUndo();
                string path = $"Assets/Materials/M_{name}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    AssetDatabase.CreateAsset(material, path);
                }
                material.SetColor("_BaseColor", color);
                foreach (var renderer in root.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/Enemies/{name}.prefab");
                AssetDatabase.SaveAssets();
                Debug.Log($"[{name} ASSETS COMPLETE]");
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void Set(Object target, string field, float value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
