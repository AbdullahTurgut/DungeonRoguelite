using System;
using System.Linq;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Experience;
using DungeonRoguelite.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    public static class Phase17Setup
    {
        public static void Build6() => Build(6);
        public static void Build7() => Build(7);

        private static void Build(int number)
        {
            bool causeway = number == 6;
            string scenePath = $"Assets/Scenes/Dungeons/Dungeon_{number:00}.unity";
            if (!System.IO.File.Exists(scenePath) && !AssetDatabase.CopyAsset(Phase13Setup.Dungeon04ScenePath,scenePath)) throw new InvalidOperationException("Scene copy failed");
            // Open before loading asset references: scene replacement unloads unused assets.
            var scene = EditorSceneManager.OpenScene(scenePath);
            int[][] counts = causeway ? new[] { new[]{6,4,0,0},new[]{5,3,2,0},new[]{3,3,2,2},new[]{3,3,3,4},new[]{3,2,3,4} }
                : new[] {new[]{6,5,3,0},new[]{5,5,4,1},new[]{4,4,4,2},new[]{3,3,4,3},new[]{2,3,5,4}};
            string[] enemyNames = {"Zombie","Runner","Ranged","Tank"};
            var prefabs = enemyNames.Select(n=>AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Enemies/{n}.prefab")).ToArray();
            var waves = new WaveDefinition[counts.Length];
            int total = 0, xp = 0;
            for (int w=0;w<counts.Length;w++)
            {
                string path = $"Assets/ScriptableObjects/Waves/Wave_{number:00}_{w+1:00}.asset";
                var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
                if (wave == null) { wave=ScriptableObject.CreateInstance<WaveDefinition>(); AssetDatabase.CreateAsset(wave,path); }
                var entries = Enumerable.Range(0,4).Where(i=>counts[w][i]>0).Select(i=>new EnemySpawnEntry(prefabs[i],counts[w][i])).ToArray();
                wave.Initialize(entries,.5f); EditorUtility.SetDirty(wave); waves[w]=wave;
                int waveXP = entries.Sum(e=>e.Count*e.EnemyPrefab.GetComponent<ExperienceReward>().XPAmount);
                total+=wave.TotalEnemyCount; xp+=waveXP;
                Debug.Log($"[D{number} CONTENT] Wave {w+1}: {wave.TotalEnemyCount} enemies / {waveXP} XP");
            }
            if (total!=(causeway?55:70) || xp!=(causeway?900:1100)) throw new InvalidOperationException("Wave arithmetic mismatch");
            string dungeonPath = $"Assets/ScriptableObjects/Dungeons/Dungeon_{number:00}.asset";
            var dungeon=AssetDatabase.LoadAssetAtPath<DungeonDefinition>(dungeonPath);
            if (dungeon==null) { dungeon=ScriptableObject.CreateInstance<DungeonDefinition>(); AssetDatabase.CreateAsset(dungeon,dungeonPath); }
            dungeon.SetConfiguration($"dungeon_{number}",causeway?"Bölüm 6: Kırık Geçit":"Bölüm 7: Çökmüş Sarnıç",
                causeway?"The Fractured Causeway — Menzilli ateş altında geçitler arasında yön değiştir.":"The Sunken Cistern — Sarnıcın çevresinde dönerek çapraz ateşi kır.",
                $"Dungeon_{number:00}",$"dungeon_{number-1}",waves,causeway?1.5f:1.6f,causeway?1.35f:1.4f);
            EditorUtility.SetDirty(dungeon);
            var catalog=AssetDatabase.LoadAssetAtPath<DungeonCatalog>("Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset");
            var list=catalog.Dungeons.ToList(); if(!list.Contains(dungeon)) list.Add(dungeon); catalog.SetDungeons(list.ToArray()); EditorUtility.SetDirty(catalog);

            foreach (var root in scene.GetRootGameObjects())
                if(root.name=="ColonnadeArena" || root.name=="FracturedCausewayArena" || root.name=="SunkenCisternArena") UnityEngine.Object.DestroyImmediate(root);
            var arena=new GameObject(causeway?"FracturedCausewayArena":"SunkenCisternArena").transform;
            var stone=Material(number,"Stone",causeway?new Color(.30f,.27f,.22f):new Color(.17f,.24f,.26f));
            var floor=Material(number,"Floor",causeway?new Color(.15f,.14f,.12f):new Color(.08f,.15f,.17f));
            float width=causeway?40:38, depth=causeway?30:34;
            Block(arena,"Floor",new Vector3(0,-.3f,0),new Vector3(width,.5f,depth),floor);
            Block(arena,"NorthWall",new Vector3(0,1.5f,depth/2),new Vector3(width+1,3,1),stone);
            Block(arena,"SouthWall",new Vector3(0,1.5f,-depth/2),new Vector3(width+1,3,1),stone);
            Block(arena,"WestWall",new Vector3(-width/2,1.5f,0),new Vector3(1,3,depth),stone);
            Block(arena,"EastWall",new Vector3(width/2,1.5f,0),new Vector3(1,3,depth),stone);
            if(causeway)
            {
                Block(arena,"BrokenRampartWest",new Vector3(-8,1.5f,3),new Vector3(14,3,2),stone);
                Block(arena,"BrokenRampartEast",new Vector3(8,1.5f,-3),new Vector3(14,3,2),stone);
            }
            else
            {
                Block(arena,"ImpassableBasin",new Vector3(0,.65f,0),new Vector3(12,.65f,12),floor,PrimitiveType.Cylinder);
                for(int i=0;i<8;i++)
                {
                    float angle=i*Mathf.PI/4;
                    Block(arena,"BasinRim"+i,new Vector3(Mathf.Cos(angle)*6,1,Mathf.Sin(angle)*6),new Vector3(1.2f,2,1.2f),stone);
                }
            }
            var playerSpawn=GameObject.Find("PlayerSpawnPoint").transform; playerSpawn.position=new Vector3(0,0,causeway?-11:-13); playerSpawn.rotation=Quaternion.identity;
            Vector3[] positions=causeway?new[]{new Vector3(-16,0,-11),new Vector3(16,0,-11),new Vector3(-17,0,0),new Vector3(17,0,0),new Vector3(-16,0,11),new Vector3(16,0,11)}
                :new[]{new Vector3(-14,0,-13),new Vector3(0,0,-14),new Vector3(14,0,-13),new Vector3(15,0,0),new Vector3(14,0,13),new Vector3(0,0,14),new Vector3(-14,0,13),new Vector3(-15,0,0)};
            var spawnRoot=GameObject.Find("SpawnPoints").transform;
            while(spawnRoot.childCount>0) UnityEngine.Object.DestroyImmediate(spawnRoot.GetChild(0).gameObject);
            var points=positions.Select((pos,i)=>{var point=new GameObject($"SpawnPoint_{i+1:00}").transform;point.SetParent(spawnRoot);point.position=pos;return point;}).ToArray();
            var manager=UnityEngine.Object.FindFirstObjectByType<WaveManager>(); manager.SetWaves(waves); manager.SetSpawnPoints(points); EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            var scenes=EditorBuildSettings.scenes.ToList(); if(!scenes.Any(s=>s.path==scenePath)) scenes.Add(new EditorBuildSettingsScene(scenePath,true)); EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"[D{number} CONTENT COMPLETE] {total} enemies / {xp} XP; scene, catalog and build route saved.");
        }
        private static Material Material(int number,string name,Color color)
        {
            string path=$"Assets/Materials/M_D{number}_{name}.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) { material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path); }
            material.color=color;EditorUtility.SetDirty(material);return material;
        }
        private static void Block(Transform parent,string name,Vector3 position,Vector3 scale,Material material,PrimitiveType type=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;
        }
    }
}
