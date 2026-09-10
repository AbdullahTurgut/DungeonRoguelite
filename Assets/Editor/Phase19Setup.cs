using System;
using System.Linq;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Enemies;
using DungeonRoguelite.Experience;
using DungeonRoguelite.UI;
using DungeonRoguelite.Waves;
using DungeonRoguelite.Weapons;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonRoguelite.Editor
{
    public static class Phase19Setup
    {
        public const string ScenePath="Assets/Scenes/Dungeons/Dungeon_10.unity";
        public static void Build()
        {
            if(!System.IO.File.Exists(ScenePath) && !AssetDatabase.CopyAsset("Assets/Scenes/Dungeons/Dungeon_05.unity",ScenePath))throw new Exception("D10 scene copy failed");
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var stone=Material("Stone",new Color(.23f,.25f,.32f));var floor=Material("Floor",new Color(.075f,.08f,.12f));
            foreach(var old in scene.GetRootGameObjects())if(old.name=="AshenSanctumArena" || old.name=="StarlessThroneArena")UnityEngine.Object.DestroyImmediate(old);
            var arena=new GameObject("StarlessThroneArena").transform;
            Block(arena,"Floor",new Vector3(0,-.3f,0),new Vector3(44,.5f,36),floor);
            Block(arena,"NorthWall",new Vector3(0,1.5f,18),new Vector3(45,3,1),stone);
            Block(arena,"SouthWall",new Vector3(0,1.5f,-18),new Vector3(45,3,1),stone);
            Block(arena,"WestWall",new Vector3(-22,1.5f,0),new Vector3(1,3,36),stone);
            Block(arena,"EastWall",new Vector3(22,1.5f,0),new Vector3(1,3,36),stone);
            foreach(var pos in new[]{new Vector3(-9,2,-6),new Vector3(9,2,-6),new Vector3(-9,2,6),new Vector3(9,2,6)})Block(arena,"ThronePillar",pos,new Vector3(3.5f,4,3.5f),stone);
            var player=GameObject.Find("PlayerSpawnPoint").transform;player.position=new Vector3(0,0,-13);player.rotation=Quaternion.identity;
            var spawnRoot=GameObject.Find("SpawnPoints").transform;
            while(spawnRoot.childCount>0)UnityEngine.Object.DestroyImmediate(spawnRoot.GetChild(0).gameObject);
            Vector3[] positions={new Vector3(0,0,11),new Vector3(-17,0,13),new Vector3(17,0,13),new Vector3(-18,0,0),new Vector3(18,0,0),new Vector3(-17,0,-12),new Vector3(17,0,-12),new Vector3(0,0,15)};
            var points=positions.Select((p,i)=>{var t=new GameObject("SpawnPoint_"+i).transform;t.SetParent(spawnRoot);t.position=p;return t;}).ToArray();
            var boss=BuildBoss(stone);
            string[] names={"Zombie","Runner","Ranged","Tank"};var enemies=names.Select(n=>AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Enemies/{n}.prefab")).ToArray();
            int[][] counts={new[]{10,6,4,0},new[]{10,7,6,3},new[]{10,7,10,7}};
            var waves=new WaveDefinition[4];int total=0,xp=0;
            for(int i=0;i<3;i++)
            {
                var entries=Enumerable.Range(0,4).Where(j=>counts[i][j]>0).Select(j=>new EnemySpawnEntry(enemies[j],counts[i][j])).ToArray();
                waves[i]=Wave(i+1,entries);int waveXP=entries.Sum(e=>e.Count*e.EnemyPrefab.GetComponent<ExperienceReward>().XPAmount);total+=waves[i].TotalEnemyCount;xp+=waveXP;
                Debug.Log($"[D10 CONTENT] Warm-up {i+1}: {waves[i].TotalEnemyCount} enemies / {waveXP} XP");
            }
            waves[3]=Wave(4,new[]{new EnemySpawnEntry(boss,1)});total++;xp+=boss.GetComponent<ExperienceReward>().XPAmount;
            if(total!=81 || xp!=3200)throw new Exception("D10 arithmetic mismatch");
            const string path="Assets/ScriptableObjects/Dungeons/Dungeon_10.asset";
            var dungeon=AssetDatabase.LoadAssetAtPath<DungeonDefinition>(path);
            if(dungeon==null){dungeon=ScriptableObject.CreateInstance<DungeonDefinition>();AssetDatabase.CreateAsset(dungeon,path);}
            dungeon.SetConfiguration("dungeon_10","Bölüm 10: Yıldızsız Taht","The Starless Throne — Boş Kalenin Muhafızı ile yüzleş.","Dungeon_10","dungeon_9",waves,1.9f,1.55f,DungeonType.Boss);EditorUtility.SetDirty(dungeon);
            var catalog=AssetDatabase.LoadAssetAtPath<DungeonCatalog>("Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset");var list=catalog.Dungeons.ToList();if(!list.Contains(dungeon))list.Add(dungeon);catalog.SetDungeons(list.ToArray());EditorUtility.SetDirty(catalog);
            var manager=UnityEngine.Object.FindFirstObjectByType<WaveManager>();manager.SetWaves(waves);manager.SetSpawnPoints(points);EditorUtility.SetDirty(manager);
            var hud=UnityEngine.Object.FindFirstObjectByType<BossHealthBarUI>();var hudData=new SerializedObject(hud);hudData.FindProperty("waveManager").objectReferenceValue=manager;hudData.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            BuildArmory();Debug.Log("[D10 CONTENT COMPLETE] 81 enemies / 3200 XP; Tier II assets and Hub acknowledgement saved.");
        }
        private static GameObject BuildBoss(Material material)
        {
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Tank.prefab"));
            instance.name="HollowCastellan";
            foreach(var component in instance.GetComponents<EnemyAttack>())UnityEngine.Object.DestroyImmediate(component);
            foreach(var component in instance.GetComponents<EnemyMovement>())UnityEngine.Object.DestroyImmediate(component);
            var visual=instance.transform.Find("Visual");if(visual!=null)UnityEngine.Object.DestroyImmediate(visual.gameObject);
            var statue=new GameObject("Visual").transform;statue.SetParent(instance.transform,false);
            Block(statue,"ArmoredTorso",new Vector3(0,1.6f,0),new Vector3(1.7f,2.5f,1.1f),material,false);
            Block(statue,"Helm",new Vector3(0,3.15f,0),new Vector3(1.1f,.9f,1),material,false);
            Block(statue,"Shoulders",new Vector3(0,2.6f,0),new Vector3(2.8f,.6f,1.3f),material,false);
            Block(statue,"Greatsword",new Vector3(1.5f,1.9f,.4f),new Vector3(.25f,3.6f,.45f),material,false);
            var body=instance.GetComponent<CharacterController>();body.radius=1;body.height=3.7f;body.center=new Vector3(0,1.85f,0);
            var health=new SerializedObject(instance.GetComponent<EnemyHealth>());health.FindProperty("maxHealth").floatValue=2000;health.ApplyModifiedPropertiesWithoutUndo();
            instance.GetComponent<ExperienceReward>().SetXPAmount(2000);
            var controller=instance.AddComponent<HollowCastellanController>();var data=new SerializedObject(controller);
            var muzzle=new GameObject("Muzzle").transform;muzzle.SetParent(instance.transform,false);muzzle.localPosition=new Vector3(0,1.5f,1.2f);
            var telegraph=new GameObject("Telegraph",typeof(LineRenderer)).GetComponent<LineRenderer>();telegraph.transform.SetParent(instance.transform,false);telegraph.startWidth=telegraph.endWidth=.18f;telegraph.positionCount=0;telegraph.sharedMaterial=Material("Telegraph",Color.white,"Universal Render Pipeline/Particles/Unlit");telegraph.gameObject.SetActive(false);
            data.FindProperty("projectilePrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnemyProjectile>("Assets/Prefabs/Enemies/EnemyProjectile.prefab");data.FindProperty("muzzle").objectReferenceValue=muzzle;data.FindProperty("telegraph").objectReferenceValue=telegraph;data.ApplyModifiedPropertiesWithoutUndo();
            var prefab=PrefabUtility.SaveAsPrefabAsset(instance,"Assets/Prefabs/Enemies/HollowCastellan.prefab");UnityEngine.Object.DestroyImmediate(instance);return prefab;
        }
        private static void BuildArmory()
        {
            var scene=EditorSceneManager.OpenScene(Phase16Setup.HubPath);
            var catalog=AssetDatabase.LoadAssetAtPath<WeaponCatalog>(Phase16Setup.CatalogPath);var all=catalog.Weapons.ToList();
            string[] heroes={"Warrior","Archer","Gunner"};string[] names={"Yıldız Kıran Büyük Kılıç","Geceyarısı Kiriş","Kale Yıkan Karabina"};string[] english={"Starbreaker Greatblade","Midnight Recurve","Bastion Carbine"};
            float[] damage={1.18f,1.16f,1.14f},speed={1.15f,1.18f,1.21f};
            for(int i=0;i<3;i++)
            {
                string path=$"Assets/ScriptableObjects/Weapons/{heroes[i]}_TierII.asset";var weapon=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
                if(weapon==null){weapon=ScriptableObject.CreateInstance<WeaponDefinition>();AssetDatabase.CreateAsset(weapon,path);}
                var data=new SerializedObject(weapon);data.FindProperty("id").stringValue=heroes[i].ToLowerInvariant()+"_tier_2";data.FindProperty("characterId").stringValue=heroes[i].ToLowerInvariant();data.FindProperty("displayName").stringValue=names[i];data.FindProperty("englishName").stringValue=english[i];data.FindProperty("tier").intValue=2;data.FindProperty("requiredDungeonId").stringValue="dungeon_10";data.FindProperty("damageMultiplier").floatValue=damage[i];data.FindProperty("attackSpeedMultiplier").floatValue=speed[i];data.ApplyModifiedPropertiesWithoutUndo();if(!all.Contains(weapon))all.Add(weapon);
            }
            var cat=new SerializedObject(catalog);var array=cat.FindProperty("weapons");array.arraySize=all.Count;for(int i=0;i<all.Count;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=all[i];cat.ApplyModifiedPropertiesWithoutUndo();
            var hub=UnityEngine.Object.FindFirstObjectByType<ArmoryHubUI>();var hubData=new SerializedObject(hub);
            var panel=(GameObject)hubData.FindProperty("equipmentPanel").objectReferenceValue;
            var old=panel.transform.Find("CycleWeapon");Button button;
            if(old!=null)button=old.GetComponent<Button>();
            else
            {
                var go=new GameObject("CycleWeapon",typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(panel.transform,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(260,-265);rect.sizeDelta=new Vector2(240,60);go.GetComponent<Image>().color=new Color(.15f,.22f,.3f);button=go.GetComponent<Button>();button.targetGraphic=go.GetComponent<Image>();
                var label=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(go.transform,false);var lr=label.GetComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;var text=label.GetComponent<TextMeshProUGUI>();text.text="DİĞER SİLAH";text.fontSize=23;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            }
            hubData.FindProperty("cycleWeaponButton").objectReferenceValue=button;hubData.FindProperty("campaignAcknowledgement").objectReferenceValue=hub.transform.Find("Hint").GetComponent<TMP_Text>();hubData.ApplyModifiedPropertiesWithoutUndo();
            // Make room for the currently equipped name without changing the compact panel footprint.
            ((TMP_Text)hubData.FindProperty("equipmentText").objectReferenceValue).fontSize=24;
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        private static WaveDefinition Wave(int number,EnemySpawnEntry[] entries)
        {
            string path=$"Assets/ScriptableObjects/Waves/Wave_10_{number:00}.asset";var wave=AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);if(wave==null){wave=ScriptableObject.CreateInstance<WaveDefinition>();AssetDatabase.CreateAsset(wave,path);}wave.Initialize(entries,.5f);EditorUtility.SetDirty(wave);return wave;
        }
        private static Material Material(string name,Color color,string shader="Universal Render Pipeline/Lit")
        {
            string path=$"Assets/Materials/M_D10_{name}.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(mat,path);}mat.color=color;EditorUtility.SetDirty(mat);return mat;
        }
        private static void Block(Transform parent,string name,Vector3 pos,Vector3 scale,Material material,bool collider=true)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        }
    }
}
