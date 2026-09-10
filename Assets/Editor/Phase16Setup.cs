using System.Linq;
using DungeonRoguelite.Characters;
using DungeonRoguelite.UI;
using DungeonRoguelite.Weapons;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DungeonRoguelite.Editor
{
    public static class Phase16Setup
    {
        public const string HubPath = "Assets/Scenes/Hub_Armory.unity";
        public const string CatalogPath = "Assets/ScriptableObjects/Weapons/WeaponCatalog.asset";
        [MenuItem("DungeonRoguelite/Phase 16/Build Armory")]
        public static void Run()
        {
            Debug.Log("[PHASE 16 SETUP START]");
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Weapons")) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Weapons");
            string[] heroes = { "Warrior", "Archer", "Gunner" };
            string[] names = { "Közdiş Büyük Kılıç", "Fırtına Kiriş", "Bobin Patlatan Karabina" };
            string[] english = { "Emberfang Greatblade", "Stormstring Recurve", "Coilburst Carbine" };
            float[] damage = {1.12f,1.10f,1.08f}, speed = {1.10f,1.12f,1.15f};
            var weapons = new WeaponDefinition[3];
            var characters = new CharacterDefinition[3];
            for (int i = 0; i < 3; i++)
            {
                string path = $"Assets/ScriptableObjects/Weapons/{heroes[i]}_TierI.asset";
                weapons[i] = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
                if (weapons[i] == null) { weapons[i] = ScriptableObject.CreateInstance<WeaponDefinition>(); AssetDatabase.CreateAsset(weapons[i], path); }
                var data = new SerializedObject(weapons[i]);
                data.FindProperty("id").stringValue = heroes[i].ToLowerInvariant() + "_tier_1";
                data.FindProperty("characterId").stringValue = heroes[i].ToLowerInvariant();
                data.FindProperty("displayName").stringValue = names[i];
                data.FindProperty("englishName").stringValue = english[i];
                data.FindProperty("tier").intValue = 1;
                data.FindProperty("requiredDungeonId").stringValue = "dungeon_5";
                data.FindProperty("damageMultiplier").floatValue = damage[i];
                data.FindProperty("attackSpeedMultiplier").floatValue = speed[i];
                data.ApplyModifiedPropertiesWithoutUndo();
                characters[i] = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"Assets/ScriptableObjects/Characters/Character_{heroes[i]}.asset");
            }
            var catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(CatalogPath);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<WeaponCatalog>(); AssetDatabase.CreateAsset(catalog, CatalogPath); }
            var catalogData = new SerializedObject(catalog); SetArray(catalogData, "weapons", weapons); catalogData.ApplyModifiedPropertiesWithoutUndo();
            foreach (var character in characters) { var data = new SerializedObject(character); data.FindProperty("weaponCatalog").objectReferenceValue = catalog; data.ApplyModifiedPropertiesWithoutUndo(); }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("ArmoryCamera", typeof(Camera)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 12, -18); camera.transform.LookAt(new Vector3(0, 0, 2)); camera.orthographic = true; camera.orthographicSize = 8;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.04f,.05f,.08f);
            var light = new GameObject("ArmoryLight", typeof(Light)).GetComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.5f; light.transform.rotation = Quaternion.Euler(50,-25,0);
            Block("Floor", PrimitiveType.Cube, new Vector3(0,-.3f,2), new Vector3(20,.5f,14));
            Block("BackWall", PrimitiveType.Cube, new Vector3(0,2,8), new Vector3(20,4,.5f));
            var canvas = new GameObject("ArmoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = .5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Label(canvas.transform, "Title", "CEPHANELİK / ARMORY", new Vector2(0,450), new Vector2(900,60), 40);
            Label(canvas.transform, "Hint", "Karakter seç • Silah ustasından ücretsiz silah al • Haritaya dön", new Vector2(0,390), new Vector2(1400,45), 25);
            var buttons = new Button[3]; var labels = new TMP_Text[3]; var markers = new GameObject[3];
            for (int i=0;i<3;i++)
            {
                var pedestal = Block(heroes[i]+"Pedestal", PrimitiveType.Cylinder, new Vector3((i-1)*5,0,0), new Vector3(2.8f,.3f,2.8f));
                // Copy only the visual subtree: no PlayerInput, combat, stats, or actor lifecycle components.
                var source = characters[i].CharacterPrefab.transform.Find("Visual");
                if (source != null) { var display = Object.Instantiate(source.gameObject); display.name = heroes[i]+"Display"; display.transform.position = new Vector3((i-1)*5,1.5f,0); }
                markers[i] = Block(heroes[i]+"Selected", PrimitiveType.Cube, new Vector3((i-1)*5,.35f,-1.65f), new Vector3(2.8f,.12f,.18f));
                buttons[i] = Button(canvas.transform, heroes[i]+"Select", heroes[i], new Vector2((i-1)*490,-250), new Vector2(450,95));
                labels[i] = buttons[i].GetComponentInChildren<TMP_Text>(); labels[i].fontSize = 24;
            }
            Block("ArmorerPedestal", PrimitiveType.Cylinder, new Vector3(0,0,5), new Vector3(3,.4f,2));
            Block("Armorer", PrimitiveType.Capsule, new Vector3(0,1.6f,5), new Vector3(1.2f,1.2f,1.2f));
            Block("Anvil", PrimitiveType.Cube, new Vector3(1.5f,.8f,4), new Vector3(1.4f,1,.8f));
            var armorer = Button(canvas.transform,"ArmorerButton","SİLAH USTASI",new Vector2(0,220),new Vector2(360,60));
            var back = Button(canvas.transform,"ReturnMap","HARİTAYA DÖN",new Vector2(0,-450),new Vector2(360,60));
            var panel = Rect(canvas.transform,"EquipmentPanel",Vector2.zero,new Vector2(900,700)); panel.gameObject.AddComponent<Image>().color = new Color(.06f,.07f,.1f,.98f);
            var text = Label(panel,"EquipmentText","",new Vector2(0,90),new Vector2(800,420),28);
            var claim = Button(panel,"Claim","ÜCRETSİZ AL",new Vector2(-240,-160),new Vector2(220,60));
            var equip = Button(panel,"Equip","KUŞAN",new Vector2(0,-160),new Vector2(220,60));
            var baseButton = Button(panel,"Base","BAŞLANGIÇ",new Vector2(240,-160),new Vector2(220,60));
            var close = Button(panel,"Close","KAPAT",new Vector2(0,-265),new Vector2(220,60));
            var ui = canvas.AddComponent<ArmoryHubUI>(); var uiData = new SerializedObject(ui);
            SetArray(uiData,"characters",characters); SetArray(uiData,"characterButtons",buttons); SetArray(uiData,"characterLabels",labels); SetArray(uiData,"selectionMarkers",markers);
            Set(uiData,"armorerButton",armorer); Set(uiData,"returnButton",back); Set(uiData,"equipmentPanel",panel.gameObject); Set(uiData,"equipmentText",text); Set(uiData,"claimButton",claim); Set(uiData,"equipButton",equip); Set(uiData,"baseButton",baseButton); Set(uiData,"closeButton",close); uiData.ApplyModifiedPropertiesWithoutUndo();
            panel.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(scene,HubPath);
            var scenes = EditorBuildSettings.scenes.ToList(); if (!scenes.Any(s=>s.path==HubPath)) scenes.Add(new EditorBuildSettingsScene(HubPath,true)); EditorBuildSettings.scenes = scenes.ToArray();

            scene = EditorSceneManager.OpenScene(WorldMapCarouselSetup.ScenePath);
            var map = Object.FindFirstObjectByType<WorldMapController>();
            var old = map.transform.Find("ArmoryButton"); var entry = old != null ? old.GetComponent<Button>() : Button(map.transform,"ArmoryButton","CEPHANELİK",new Vector2(0,-470),new Vector2(300,54));
            var mapData = new SerializedObject(map); Set(mapData,"armoryButton",entry); mapData.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("[PHASE 16 SETUP COMPLETE]");
        }

        private static void Set(SerializedObject data,string key,Object value) => data.FindProperty(key).objectReferenceValue = value;
        private static void SetArray<T>(SerializedObject data,string key,T[] values) where T:Object { var array=data.FindProperty(key); array.arraySize=values.Length; for(int i=0;i<values.Length;i++) array.GetArrayElementAtIndex(i).objectReferenceValue=values[i]; }
        private static GameObject Block(string name,PrimitiveType type,Vector3 position,Vector3 scale) { var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.position=position; go.transform.localScale=scale; return go; }
        private static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size) { var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f); rect.anchoredPosition=position; rect.sizeDelta=size; return rect; }
        private static TMP_Text Label(Transform parent,string name,string text,Vector2 pos,Vector2 size,float font) { var label=Rect(parent,name,pos,size).gameObject.AddComponent<TextMeshProUGUI>(); label.text=text; label.fontSize=font; label.alignment=TextAlignmentOptions.Center; label.raycastTarget=false; return label; }
        private static Button Button(Transform parent,string name,string text,Vector2 pos,Vector2 size) { var rect=Rect(parent,name,pos,size); var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.22f,.3f); var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; Label(rect,"Label",text,Vector2.zero,size-Vector2.one*12,26); return button; }
    }
}
