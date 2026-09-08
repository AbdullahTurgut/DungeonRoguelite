using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Enemies;

namespace DungeonRoguelite.Editor
{
    public static class Phase11AssetBuilder
    {
        public static void Runner()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            var root = Object.Instantiate(source);
            try
            {
                root.name = "Runner";
                Set(root.GetComponent<EnemyHealth>(), "maxHealth", 25f);
                Set(root.GetComponent<EnemyMovement>(), "moveSpeed", 5.5f);
                Set(root.GetComponent<EnemyMovement>(), "stoppingDistance", 1.1f);
                Set(root.GetComponent<EnemyAttack>(), "damage", 6f);
                Set(root.GetComponent<EnemyAttack>(), "attackRange", 1.3f);
                Set(root.GetComponent<EnemyAttack>(), "attackCooldown", 0.65f);
                var controller = root.GetComponent<CharacterController>();
                controller.height = 1.7f;
                controller.radius = 0.425f;
                controller.center = Vector3.up * 0.85f;
                var visual = root.transform.Find("Visual");
                visual.localPosition = Vector3.up * 0.85f;
                visual.localScale = Vector3.one * 0.85f;
                const string path = "Assets/Materials/M_Runner.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    AssetDatabase.CreateAsset(material, path);
                }
                material.SetColor("_BaseColor", new Color(1f, 0.45f, 0.05f));
                foreach (var renderer in root.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Enemies/Runner.prefab");
                AssetDatabase.SaveAssets();
                Debug.Log("[RUNNER ASSETS COMPLETE]");
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
