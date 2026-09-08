using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Enemies;

namespace DungeonRoguelite.Editor
{
    public static class Phase11AssetBuilder
    {
        public static void Runner() => Melee("Runner", 25f, 5.5f, 1.1f, 1.3f, 6f, 0.65f, 10, 0.85f, new Color(1f, 0.45f, 0.05f));
        public static void Tank() => Melee("Tank", 150f, 1.8f, 1.8f, 2f, 22f, 1.8f, 40, 1.4f, new Color(0.18f, 0.21f, 0.25f));
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
