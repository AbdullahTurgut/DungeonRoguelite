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
    public static class Phase18Setup
    {
        public static void Build8() => Build(8);
        public static void Build9() => Build(9);

        private static void Build(int number)
        {
            bool courtyard = number == 8;
            string scenePath = $"Assets/Scenes/Dungeons/Dungeon_{number:00}.unity";
            if (!System.IO.File.Exists(scenePath) && !AssetDatabase.CopyAsset(Phase13Setup.Dungeon04ScenePath,scenePath)) throw new InvalidOperationException("Scene copy failed");
            // Open before loading asset references: scene replacement unloads unused assets.
            var scene = EditorSceneManager.OpenScene(scenePath);
            int[][] counts = courtyard ? new[] { new[]{8,5,0,0},new[]{7,6,2,0},new[]{6,5,2,3},new[]{5,5,3,5},new[]{4,4,3,7} }
                : new[] {new[]{7,5,3,0},new[]{6,5,4,2},new[]{4,4,4,3},new[]{3,4,3,4},new[]{3,4,3,5},new[]{2,3,3,6}};
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
            if (total!=(courtyard?80:90) || xp!=(courtyard?1300:1600)) throw new InvalidOperationException("Wave arithmetic mismatch");
            string dungeonPath = $"Assets/ScriptableObjects/Dungeons/Dungeon_{number:00}.asset";
            var dungeon=AssetDatabase.LoadAssetAtPath<DungeonDefinition>(dungeonPath);
            if (dungeon==null) { dungeon=ScriptableObject.CreateInstance<DungeonDefinition>(); AssetDatabase.CreateAsset(dungeon,dungeonPath); }
            dungeon.SetConfiguration($"dungeon_{number}",courtyard?"Bölüm 8: Közlü Avlu":"Bölüm 9: Gölge Hisarı",
                courtyard?"The Ember Courtyard — Dikilitaşları siper al, ağır düşmanları ayır.":"The Gloam Bastion — İki koridor arasında yön değiştir, yan saldırıları engelle.",
                $"Dungeon_{number:00}",$"dungeon_{number-1}",waves,courtyard?1.7f:1.8f,courtyard?1.45f:1.5f);
            EditorUtility.SetDirty(dungeon);
            var catalog=AssetDatabase.LoadAssetAtPath<DungeonCatalog>("Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset");
            var list=catalog.Dungeons.ToList(); if(!list.Contains(dungeon)) list.Add(dungeon); catalog.SetDungeons(list.ToArray()); EditorUtility.SetDirty(catalog);

            foreach (var root in scene.GetRootGameObjects())
                if(root.name=="ColonnadeArena" || root.name=="EmberCourtyardArena" || root.name=="GloamBastionArena") UnityEngine.Object.DestroyImmediate(root);
            var arena=new GameObject(courtyard?"EmberCourtyardArena":"GloamBastionArena").transform;
            var stone=Material(number,"Stone",courtyard?new Color(.38f,.22f,.16f):new Color(.22f,.20f,.30f));
            var floor=Material(number,"Floor",courtyard?new Color(.19f,.11f,.08f):new Color(.10f,.09f,.15f));
            float width=courtyard?42:44, depth=courtyard?32:34;
            Block(arena,"Floor",new Vector3(0,-.3f,0),new Vector3(width,.5f,depth),floor);
            Block(arena,"NorthWall",new Vector3(0,1.5f,depth/2),new Vector3(width+1,3,1),stone);
            Block(arena,"SouthWall",new Vector3(0,1.5f,-depth/2),new Vector3(width+1,3,1),stone);
            Block(arena,"WestWall",new Vector3(-width/2,1.5f,0),new Vector3(1,3,depth),stone);
            Block(arena,"EastWall",new Vector3(width/2,1.5f,0),new Vector3(1,3,depth),stone);
            if(courtyard)
            {
                Vector3[] obelisks = { new Vector3(-7,2,-5), new Vector3(7,2,-5), new Vector3(-7,2,5), new Vector3(7,2,5) };
                for(int i=0;i<obelisks.Length;i++) Block(arena,"Obelisk"+i,obelisks[i],new Vector3(3,4,3),stone);
            }
            else
            {
                Block(arena,"NorthSpine",new Vector3(0,1.5f,7),new Vector3(3,3,8),stone);
                Block(arena,"SouthSpine",new Vector3(0,1.5f,-7),new Vector3(3,3,8),stone);
            }
            var playerSpawn=GameObject.Find("PlayerSpawnPoint").transform; playerSpawn.position=new Vector3(0,0,courtyard?-12:-13); playerSpawn.rotation=Quaternion.identity;
            Vector3[] positions = {new Vector3(-17,0,-12),new Vector3(0,0,-14),new Vector3(17,0,-12),new Vector3(18,0,0),new Vector3(17,0,12),new Vector3(0,0,14),new Vector3(-17,0,12),new Vector3(-18,0,0)};
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
