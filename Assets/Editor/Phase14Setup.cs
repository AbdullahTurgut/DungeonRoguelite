using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;

namespace DungeonRoguelite.Editor
{
    /// <summary>Idempotent, gate-scoped authoring helpers for Dungeon 5. Never reset a later D5 gate.</summary>
    public static class Phase14Setup
    {
        public const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_05.unity";
        private const string SourceScene = "Assets/Scenes/Dungeons/Dungeon_04.unity";
        private const string DungeonPath = "Assets/ScriptableObjects/Dungeons/Dungeon_05.asset";
        private const string BossPath = "Assets/Prefabs/Enemies/AshWarden.prefab";
        private const string CatalogPath = "Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset";

        [MenuItem("DungeonRoguelite/Phase 14/Setup Gate 14.1 Foundation")]
        public static void SetupGate14_1()
        {
            if (!File.Exists(ScenePath)) { AssetDatabase.CopyAsset(SourceScene, ScenePath); AssetDatabase.Refresh(); }
            var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(DungeonPath);
            if (dungeon == null) { dungeon = ScriptableObject.CreateInstance<DungeonDefinition>(); AssetDatabase.CreateAsset(dungeon, DungeonPath); }
            var catalog = AssetDatabase.LoadAssetAtPath<DungeonCatalog>(CatalogPath);
            var list = new List<DungeonDefinition>(catalog.Dungeons);
            if (!list.Contains(dungeon)) list.Add(dungeon);
            dungeon.SetConfiguration("dungeon_5", "Bölüm 5: Kül Mabedi", "Kül Muhafızının hüküm sürdüğü kadim mabedi temizle.", "Dungeon_05", "dungeon_4", System.Array.Empty<WaveDefinition>(), 1.4f, 1.3f, DungeonType.Boss);
            catalog.SetDungeons(list.ToArray()); EditorUtility.SetDirty(dungeon); EditorUtility.SetDirty(catalog);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var manager = Object.FindFirstObjectByType<WaveManager>(); if (manager != null) manager.SetWaves(System.Array.Empty<WaveDefinition>());
            EnsureSpawnPoints(manager, 6); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[GATE 14.1 SETUP COMPLETE] Dungeon 5 foundation saved.");
        }

        [MenuItem("DungeonRoguelite/Phase 14/Setup Gate 14.2 Arena")]
        public static void SetupGate14_2()
        {
            if (!File.Exists(ScenePath)) SetupGate14_1();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            DestroyRoot("ColonnadeArena");
            string[] obsolete = { "Floor", "Wall_North", "Wall_South", "Wall_East", "Wall_West", "Environment_Obstacles" };
            foreach (var item in obsolete) DestroyRoot(item);
            var root = GameObject.Find("AshenSanctumArena") ?? new GameObject("AshenSanctumArena"); ClearChildren(root.transform);
            CreateBlock(root.transform, "Floor", new Vector3(0, -.3f, 0), new Vector3(44, .5f, 36), new Color(.12f, .10f, .11f));
            CreateBlock(root.transform, "NorthWall", new Vector3(0, 1.5f, 18), new Vector3(46, 3, 1), new Color(.18f, .12f, .12f));
            CreateBlock(root.transform, "SouthWall", new Vector3(0, 1.5f, -18), new Vector3(46, 3, 1), new Color(.18f, .12f, .12f));
            CreateBlock(root.transform, "WestWall", new Vector3(-22, 1.5f, 0), new Vector3(1, 3, 37), new Color(.18f, .12f, .12f));
            CreateBlock(root.transform, "EastWall", new Vector3(22, 1.5f, 0), new Vector3(1, 3, 37), new Color(.18f, .12f, .12f));
            CreateBlock(root.transform, "Obelisk_West", new Vector3(-10, 2, 0), new Vector3(2.5f, 4, 2.5f), new Color(.25f, .16f, .16f));
            CreateBlock(root.transform, "Obelisk_East", new Vector3(10, 2, 0), new Vector3(2.5f, 4, 2.5f), new Color(.25f, .16f, .16f));
            var playerSpawn = GameObject.Find("PlayerSpawnPoint"); if (playerSpawn != null) { playerSpawn.transform.position = new Vector3(0, 0, -13); playerSpawn.transform.rotation = Quaternion.identity; }
            var bossSpawn = GameObject.Find("BossSpawnPoint") ?? new GameObject("BossSpawnPoint"); bossSpawn.transform.position = new Vector3(0, 0, 11);
            var manager = Object.FindFirstObjectByType<WaveManager>(); EnsureSpawnPoints(manager, 6);
            Vector3[] points = { new Vector3(0,0,11), new Vector3(-17,0,12), new Vector3(17,0,12), new Vector3(-19,0,0), new Vector3(19,0,0), new Vector3(0,0,15) };
            var spawnRoot = GameObject.Find("SpawnPoints").transform;
            for (int i = 0; i < 6; i++) spawnRoot.Find("SpawnPoint_0" + (i + 1)).position = points[i];
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("[GATE 14.2 SETUP COMPLETE] Ashen Sanctum arena saved.");
        }

        [MenuItem("DungeonRoguelite/Phase 14/Setup Gate 14.3 Boss")]
        public static void SetupGate14_3()
        {
            if (!File.Exists(ScenePath)) SetupGate14_1();
            SetupGate14_2();
            var tank = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Tank.prefab");
            var instance = PrefabUtility.InstantiatePrefab(tank) as GameObject; instance.name = "AshWarden";
            foreach (var component in instance.GetComponents<EnemyAttack>()) Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponents<EnemyMovement>()) Object.DestroyImmediate(component);
            var warden = instance.GetComponent<BossWardenController>() ?? instance.AddComponent<BossWardenController>();
            var health = instance.GetComponent<EnemyHealth>(); var reward = instance.GetComponent<ExperienceReward>(); reward.SetXPAmount(960);
            var tankReward = tank.GetComponent<ExperienceReward>(); reward.SetPickupPrefab(tankReward.PickupPrefab);
            var healthSo = new SerializedObject(health); healthSo.FindProperty("maxHealth").floatValue = 1200f; healthSo.ApplyModifiedPropertiesWithoutUndo();
            var cc = instance.GetComponent<CharacterController>(); cc.radius = 1.2f; cc.height = 3.2f;
            var visual = instance.transform.Find("Visual"); if (visual != null) visual.localScale = Vector3.one * 2.1f;
            var telegraph = GameObject.CreatePrimitive(PrimitiveType.Cylinder); telegraph.name = "Telegraph"; telegraph.transform.SetParent(instance.transform); telegraph.transform.localPosition = new Vector3(0, .03f, 0); telegraph.transform.localScale = new Vector3(6.5f, .02f, 6.5f); Object.DestroyImmediate(telegraph.GetComponent<Collider>()); telegraph.SetActive(false);
            var muzzle = new GameObject("Muzzle"); muzzle.transform.SetParent(instance.transform); muzzle.transform.localPosition = new Vector3(0, 1.5f, 1.4f);
            var so = new SerializedObject(warden); so.FindProperty("projectilePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EnemyProjectile>("Assets/Prefabs/Enemies/EnemyProjectile.prefab"); so.FindProperty("muzzle").objectReferenceValue = muzzle.transform; so.FindProperty("telegraph").objectReferenceValue = telegraph.transform; so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(instance, BossPath); Object.DestroyImmediate(instance);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single); EnsureBossHud(scene); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[GATE 14.3 SETUP COMPLETE] Ash Warden and HUD saved.");
        }

        [MenuItem("DungeonRoguelite/Phase 14/Apply Boss HUD Presentation")]
        public static void ApplyBossHudPresentation()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EnsureBossHud(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[D5 BOSS HUD PRESENTATION COMPLETE]");
        }

        [MenuItem("DungeonRoguelite/Phase 14/Setup Gate 14.4 Encounter")]
        public static void SetupGate14_4()
        {
            // Encounter authoring depends on the authoritative arena, but each setup call
            // remains scoped and preserves waves until this gate intentionally replaces them.
            SetupGate14_2();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BossPath) == null) SetupGate14_3();
            var zombie = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab"); var runner = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Runner.prefab"); var ranged = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Ranged.prefab"); var tank = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Tank.prefab"); var boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossPath);
            var waves = new[] {
                Wave("Wave_05_01", new [] { new EnemySpawnEntry(zombie,3), new EnemySpawnEntry(runner,2), new EnemySpawnEntry(ranged,2)}, .5f),
                Wave("Wave_05_02", new [] { new EnemySpawnEntry(zombie,3), new EnemySpawnEntry(runner,3), new EnemySpawnEntry(ranged,4), new EnemySpawnEntry(tank,1)}, .55f),
                Wave("Wave_05_03", new [] { new EnemySpawnEntry(boss,1)}, .5f)
            };
            var dungeon = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(DungeonPath); dungeon.SetConfiguration("dungeon_5", "Bölüm 5: Kül Mabedi", "Kül Muhafızının hüküm sürdüğü kadim mabedi temizle.", "Dungeon_05", "dungeon_4", waves, 1.4f, 1.3f, DungeonType.Boss); EditorUtility.SetDirty(dungeon);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single); var manager = Object.FindFirstObjectByType<WaveManager>(); manager.SetWaves(waves); EditorUtility.SetDirty(manager); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[GATE 14.4 SETUP COMPLETE] Dungeon 5 encounter saved.");
        }

        private static WaveDefinition Wave(string name, EnemySpawnEntry[] entries, float interval)
        {
            string path = "Assets/ScriptableObjects/Waves/" + name + ".asset"; var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path); if (wave == null) { wave = ScriptableObject.CreateInstance<WaveDefinition>(); AssetDatabase.CreateAsset(wave, path); } wave.Initialize(entries, interval); EditorUtility.SetDirty(wave); return wave;
        }
        private static void EnsureSpawnPoints(WaveManager manager, int count)
        { var root = GameObject.Find("SpawnPoints") ?? new GameObject("SpawnPoints"); var result = new Transform[count]; for (int i=0;i<count;i++) { var child=root.transform.Find("SpawnPoint_0"+(i+1)); if(child==null){var o=new GameObject("SpawnPoint_0"+(i+1));o.transform.SetParent(root.transform);child=o.transform;} result[i]=child; } if(manager!=null) manager.SetSpawnPoints(result); }
        private static void DestroyRoot(string name) { foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects()) if(root.name==name) { Object.DestroyImmediate(root); return; } }
        private static void ClearChildren(Transform root) { while(root.childCount>0) Object.DestroyImmediate(root.GetChild(0).gameObject); }
        private static void CreateBlock(Transform parent,string name,Vector3 pos,Vector3 scale,Color color) { var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent);o.transform.position=pos;o.transform.localScale=scale;var r=o.GetComponent<Renderer>();r.sharedMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));r.sharedMaterial.color=color; }
        private static void EnsureBossHud(Scene scene)
        {
            var existing=Object.FindFirstObjectByType<BossHealthBarUI>();
            if(existing!=null)
            {
                var existingSerialized = new SerializedObject(existing);
                var existingFill = existingSerialized.FindProperty("fill").objectReferenceValue as Image;
                if (existingFill != null)
                {
                    existingFill.type = Image.Type.Simple;
                    var existingFillRect = existingFill.rectTransform;
                    existingFillRect.anchorMin = Vector2.zero;
                    existingFillRect.anchorMax = Vector2.one;
                    existingFillRect.offsetMin = Vector2.zero;
                    existingFillRect.offsetMax = Vector2.zero;
                    EditorUtility.SetDirty(existingFill);
                }
                var existingPhase = existingSerialized.FindProperty("phaseLabel").objectReferenceValue as TMP_Text;
                if (existingPhase != null)
                {
                    existingPhase.text = string.Empty;
                    Object.DestroyImmediate(existingPhase.gameObject);
                    existingSerialized.FindProperty("phaseLabel").objectReferenceValue = null;
                    existingSerialized.ApplyModifiedPropertiesWithoutUndo();
                }
                return;
            }
            var canvasGo=new GameObject("BossHealthHUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;
            var panel=new GameObject("Panel",typeof(RectTransform),typeof(Image));panel.transform.SetParent(canvasGo.transform,false);var rect=panel.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.25f,.9f);rect.anchorMax=new Vector2(.75f,.96f);rect.offsetMin=rect.offsetMax=Vector2.zero;panel.GetComponent<Image>().color=new Color(0,0,0,.7f);
            var fillGo=new GameObject("Fill",typeof(RectTransform),typeof(Image));fillGo.transform.SetParent(panel.transform,false);var fill=fillGo.GetComponent<Image>();fill.type=Image.Type.Simple;fill.color=Color.red;var fillRect=fillGo.GetComponent<RectTransform>();fillRect.anchorMin=Vector2.zero;fillRect.anchorMax=Vector2.one;fillRect.offsetMin=fillRect.offsetMax=Vector2.zero;
            var labelGo=new GameObject("Name",typeof(RectTransform),typeof(TextMeshProUGUI));labelGo.transform.SetParent(panel.transform,false);var label=labelGo.GetComponent<TextMeshProUGUI>();label.alignment=TextAlignmentOptions.Center;label.fontSize=28;var labelRect=labelGo.GetComponent<RectTransform>();labelRect.anchorMin=new Vector2(0,1);labelRect.anchorMax=new Vector2(1,1);labelRect.anchoredPosition=new Vector2(0,26);labelRect.sizeDelta=new Vector2(0,36);
            var hud=canvasGo.AddComponent<BossHealthBarUI>();var so=new SerializedObject(hud);so.FindProperty("panelRoot").objectReferenceValue=panel;so.FindProperty("fill").objectReferenceValue=fill;so.FindProperty("nameLabel").objectReferenceValue=label;so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
