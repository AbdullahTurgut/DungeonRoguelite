using System;
using System.IO;
using System.Linq;
using DungeonRoguelite.Presentation;
using DungeonRoguelite.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Targeted WIP recovery. Never recreates displays or writes gameplay prefabs/save data.
public static class SetupHubArmory
{
    const string ScenePath = "Assets/Scenes/Hub_Armory.unity";
    const string Kay = "Assets/Art/ThirdParty/KayKit_Adventurers_2.0_FREE/";
    const string Kenney = "Assets/Art/ThirdParty/kenney_blaster-kit_2.1/Models/FBX format/";
    const string Cardinal = "Assets/Art/ThirdParty/CardinalZebra_FantasyWeapons/";

    [MenuItem("Tools/Hub Armory/Sync Archer Bow Poses From Prefab")]
    public static void SyncArcherBowPoses()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before syncing Archer poses.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        SaveHubScene(scene);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Archer Base/Tier I/Tier II poses copied from the current playable prefab; Hub_Armory open.");
    }

    // All Hub authoring saves preserve the manually approved playable Archer poses.
    static void SaveHubScene(UnityEngine.SceneManagement.Scene scene)
    {
        if (scene.path != ScenePath) throw new InvalidOperationException("Expected Hub_Armory scene.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Archer.prefab");
        var source = new SerializedObject(prefab.GetComponent<EquippedWeaponPresentation>());
        var target = new SerializedObject(Find("Archer_VisualDisplay").GetComponent<HubHeroPresentation>());
        var pairs = new System.Collections.Generic.List<(Transform source, Transform target)>();
        void Pair(GameObject from, GameObject to)
        {
            if (from == null || to == null) throw new InvalidOperationException("Missing Archer visual mapping; pose sync cannot proceed.");
            string fromParent = AnimationUtility.CalculateTransformPath(from.transform.parent, prefab.transform.Find("VisualRoot"));
            string toParent = AnimationUtility.CalculateTransformPath(to.transform.parent, Find("Archer_VisualDisplay").transform.Find("VisualRoot"));
            if (fromParent != toParent) throw new InvalidOperationException("Archer attachment coordinate frames differ.");
            void Collect(Transform a, Transform b)
            {
                var sourceMesh = a.GetComponent<MeshFilter>();
                var targetMesh = b.GetComponent<MeshFilter>();
                if (a.childCount != b.childCount ||
                    (sourceMesh != null ? sourceMesh.sharedMesh : null) != (targetMesh != null ? targetMesh.sharedMesh : null))
                    throw new InvalidOperationException("Archer visual hierarchy/mesh differs: " + b.name);
                pairs.Add((a, b));
                for (int i = 0; i < a.childCount; i++) Collect(a.GetChild(i), b.GetChild(i));
            }
            Collect(from.transform, to.transform);
        }
        Pair((GameObject)source.FindProperty("baseWeaponVisual").objectReferenceValue,
            (GameObject)target.FindProperty("baseWeaponVisual").objectReferenceValue);
        var sourceEntries = source.FindProperty("tierWeapons");
        var targetEntries = target.FindProperty("tierWeapons");
        foreach (string id in new[]{"archer_tier_1", "archer_tier_2"})
        {
            GameObject Resolve(SerializedProperty entries)
            {
                for (int i = 0; i < entries.arraySize; i++)
                {
                    var entry = entries.GetArrayElementAtIndex(i);
                    if (entry.FindPropertyRelative("weaponId").stringValue == id)
                        return (GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue;
                }
                return null;
            }
            Pair(Resolve(sourceEntries), Resolve(targetEntries));
        }
        foreach (var pair in pairs)
        {
            pair.target.localPosition = pair.source.localPosition;
            pair.target.localRotation = pair.source.localRotation;
            pair.target.localScale = pair.source.localScale;
        }
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    [MenuItem("Tools/Hub Armory/Final Background Polish")]
    public static void PolishArmoryBackground()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before background authoring.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        const string folder = "Assets/Materials/Armory";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Materials", "Armory");
        Material Finish(string name, Color color, float metallic)
        {
            string path = folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Metallic", metallic);
                material.SetFloat("_Smoothness", .18f);
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }
        var floor = Finish("M_ArmorySlate", new Color(.17f, .205f, .23f), .05f);
        var wall = Finish("M_ArmoryWall", new Color(.235f, .195f, .165f), 0f);
        var trim = Finish("M_ArmoryMetalTrim", new Color(.37f, .285f, .18f), .3f);
        Find("Floor").GetComponent<MeshRenderer>().sharedMaterial = floor;
        var backWall = Find("BackWall");
        backWall.GetComponent<MeshRenderer>().sharedMaterial = wall;
        var frame = GameObject.Find("ArmoryWallTrim");
        if (frame == null)
        {
            frame = new GameObject("ArmoryWallTrim");
            var mesh = backWall.GetComponent<MeshFilter>().sharedMesh;
            void Strip(string name, Vector3 position, Vector3 scale)
            {
                var strip = new GameObject(name);
                strip.transform.SetParent(frame.transform, false);
                strip.transform.localPosition = position;
                strip.transform.localScale = scale;
                strip.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = strip.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = trim;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            // Shallow, static wall detailing behind the actors; no colliders or extra lighting.
            Strip("LeftWallBand", new Vector3(-7f, 2f, 7.70f), new Vector3(.16f, 3.6f, .08f));
            Strip("RightWallBand", new Vector3(7f, 2f, 7.70f), new Vector3(.16f, 3.6f, .08f));
            Strip("LowerWallBand", new Vector3(0f, .35f, 7.70f), new Vector3(19.5f, .12f, .08f));
        }
        var plate = Find("ArmorerButton");
        var rect = plate.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, 260f);
        rect.sizeDelta = new Vector2(230f, 44f);
        plate.GetComponent<UnityEngine.UI.Image>().color = new Color(.13f, .16f, .18f, .96f);
        var label = plate.GetComponentInChildren<TMPro.TMP_Text>();
        label.fontSize = 22f;
        label.rectTransform.sizeDelta = new Vector2(225f, 42f);
        SaveHubScene(scene); // No character, weapon or gameplay authoring.
        foreach (var presentation in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<HubHeroPresentation>(true)))
        {
            var animator = presentation.GetComponentInChildren<Animator>(true);
            var controller = (AnimatorController)animator.runtimeAnimatorController;
            var motion = controller.layers[0].stateMachine.defaultState.motion;
            var idle = motion is BlendTree tree ? tree.children[0].motion as AnimationClip : motion as AnimationClip;
            idle?.SampleAnimation(animator.gameObject, 0f);
        }
        EditorSceneManager.OpenScene(ScenePath); // Discard preview poses.
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Armory background/header polish complete; Hub_Armory open for manual QA.");
    }

    [MenuItem("Tools/Hub Armory/Configure Current Character Composition")]
    public static void ConfigureCustomerComposition()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before composition authoring.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var ui = new SerializedObject(Find("ArmoryCanvas").GetComponent<ArmoryHubUI>());
        var presentations = ui.FindProperty("heroPresentations");
        var roots = ui.FindProperty("heroDisplayRoots");
        var pedestals = ui.FindProperty("heroPedestals");
        roots.arraySize = pedestals.arraySize = presentations.arraySize;
        var buttons = ui.FindProperty("characterButtons");
        var markers = ui.FindProperty("selectionMarkers");
        for (int i = 0; i < presentations.arraySize; i++)
        {
            var presentation = (HubHeroPresentation)presentations.GetArrayElementAtIndex(i).objectReferenceValue;
            var root = presentation.transform.parent;
            var pedestal = Find(root.name.Replace("Display", "Pedestal"));
            roots.GetArrayElementAtIndex(i).objectReferenceValue = root.gameObject;
            pedestals.GetArrayElementAtIndex(i).objectReferenceValue = pedestal;
            // Move entire display areas; approved weapon/character local transforms stay untouched.
            root.position = new Vector3(2.5f, root.position.y, 5.5f);
            pedestal.transform.position = new Vector3(2.5f, pedestal.transform.position.y, 5.5f);
            root.gameObject.SetActive(i == 0); // Editor preview only; Start resolves the actual campaign ID.
            presentation.gameObject.SetActive(true);
            pedestal.SetActive(i == 0);
            var button = (UnityEngine.UI.Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue;
            button.interactable = false;
            button.gameObject.SetActive(false);
            ((GameObject)markers.GetArrayElementAtIndex(i).objectReferenceValue).SetActive(false);
        }
        ui.ApplyModifiedPropertiesWithoutUndo();
        foreach (var text in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMPro.TMP_Text>(true)))
            if (text.text.Contains("Karakter seç"))
                text.text = "Silah ustasıyla konuş • Silahını kuşan • Haritaya dön";
        Find("ArmorerButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 330f);
        SaveHubScene(scene);
        EditorSceneManager.OpenScene(ScenePath);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Current-character Armory composition authored. CharacterSelectionSession/run owner selects the customer at runtime; no selection/save writes.");
    }

    [MenuItem("Tools/Hub Armory/Polish Archer Tier II Transform")]
    public static void PolishArcherTierTwoTransform()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before bow transform authoring.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var presentation = Find("Archer_VisualDisplay").GetComponent<HubHeroPresentation>();
        var data = new SerializedObject(presentation);
        var starter = (GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue;
        var entries = data.FindProperty("tierWeapons");
        GameObject visual = null;
        for (int i = 0; i < entries.arraySize; i++)
        {
            var entry = entries.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("weaponId").stringValue == "archer_tier_2")
                visual = (GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue;
        }
        if (starter == null || visual == null) throw new InvalidOperationException("Missing Archer bow binding.");
        var mesh = visual.GetComponentInChildren<MeshFilter>(true);
        var model = mesh.transform;
        // Recompute from Base, so repeating the menu never compounds the adjustment.
        FitCardinalMesh(model, mesh.sharedMesh, starter.transform, true);
        var frame = WeaponFrame(mesh.sharedMesh.vertices, true);
        Vector3 grip = model.localPosition + model.localRotation * Vector3.Scale(frame.grip, model.localScale);
        var animator = presentation.GetComponentInChildren<Animator>();
        var poses = animator.GetComponentsInChildren<Transform>(true)
            .Select(t => (t, position: t.localPosition, rotation: t.localRotation, scale: t.localScale)).ToArray();
        var controller = (AnimatorController)animator.runtimeAnimatorController;
        var motion = controller.layers[0].stateMachine.defaultState.motion;
        var idle = motion is BlendTree tree ? tree.children[0].motion as AnimationClip : motion as AnimationClip;
        idle?.SampleAnimation(animator.gameObject, 0f);
        // Evaluate the camera correction in the actual idle hand pose, then restore that pose.
        Vector3 normal = model.rotation * (frame.rotation * Vector3.forward);
        Vector3 facing = -Camera.main.transform.forward;
        if (Vector3.Dot(normal, facing) < 0f) normal = -normal;
        Quaternion faceTurn = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(normal, facing), .35f);
        Quaternion diagonal = Quaternion.AngleAxis(-18f, Camera.main.transform.forward);
        Quaternion rotation = Quaternion.Inverse(model.parent.rotation) * diagonal * faceTurn * model.rotation;
        Vector3 scale = model.localScale * 1.18f;
        foreach (var pose in poses)
        {
            pose.t.localPosition = pose.position;
            pose.t.localRotation = pose.rotation;
            pose.t.localScale = pose.scale;
        }
        model.localRotation = rotation;
        model.localScale = scale;
        model.localPosition = grip - rotation * Vector3.Scale(frame.grip, scale);
        SaveHubScene(scene); // Only the Tier II mesh-child transform.
        idle?.SampleAnimation(animator.gameObject, 0f);
        starter.SetActive(false);
        for (int i = 0; i < entries.arraySize; i++)
        {
            var bow = (GameObject)entries.GetArrayElementAtIndex(i).FindPropertyRelative("visualObject").objectReferenceValue;
            if (bow != null) bow.SetActive(bow == visual);
        }
        EditorSceneManager.OpenScene(ScenePath);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Archer Tier II transform polish complete; Hub_Armory open for manual QA.");
    }

    [MenuItem("Tools/Hub Armory/Integrate Archer Tier II Bow")]
    public static void IntegrateArcherTierTwoBow()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before Archer bow authoring.");
        const string pack = "Assets/Art/ThirdParty/KayKitFantasyWeaponsBits/";
        const string assets = pack + "KayKit_FantasyWeaponsBits_1.0_FREE/Assets/fbx(unity)/";
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var presentation = Find("Archer_VisualDisplay").GetComponent<HubHeroPresentation>();
        var data = new SerializedObject(presentation);
        var starter = (GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue;
        var entries = data.FindProperty("tierWeapons");
        SerializedProperty slot = null;
        GameObject tierOne = null;
        for (int i = 0; i < entries.arraySize; i++)
        {
            var entry = entries.GetArrayElementAtIndex(i);
            string id = entry.FindPropertyRelative("weaponId").stringValue;
            if (id == "archer_tier_2") slot = entry;
            if (id == "archer_tier_1") tierOne = (GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue;
        }
        if (starter == null || tierOne == null || slot == null)
            throw new InvalidOperationException("Missing approved Archer attachment or Tier II slot.");
        var visual = (GameObject)slot.FindPropertyRelative("visualObject").objectReferenceValue;
        if (visual == null)
        {
            var mesh = AssetDatabase.LoadAllAssetsAtPath(assets + "bow_B.fbx").OfType<Mesh>().Single();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assets + "weapons_bits_texture.png");
            if (texture == null) throw new InvalidOperationException("Missing Fantasy Weapons Bits palette.");
            var importer = (TextureImporter)AssetImporter.GetAtPath(assets + "weapons_bits_texture.png");
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var material = AssetDatabase.LoadAssetAtPath<Material>(pack + "M_FantasyWeaponsBits.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetTexture("_BaseMap", texture);
                material.SetFloat("_Smoothness", .3f);
                AssetDatabase.CreateAsset(material, pack + "M_FantasyWeaponsBits.mat");
            }
            visual = new GameObject("Archer_FantasyBits_TierII");
            visual.transform.SetParent(starter.transform.parent, false);
            visual.transform.localPosition = starter.transform.localPosition;
            visual.transform.localRotation = starter.transform.localRotation;
            visual.transform.localScale = starter.transform.localScale;
            var model = new GameObject(mesh.name);
            model.transform.SetParent(visual.transform, false);
            model.AddComponent<MeshFilter>().sharedMesh = mesh;
            model.AddComponent<MeshRenderer>().sharedMaterial = material;
            FitCardinalMesh(model.transform, mesh, starter.transform, true);
            visual.SetActive(false);
            slot.FindPropertyRelative("visualObject").objectReferenceValue = visual;
            data.ApplyModifiedPropertiesWithoutUndo();
            SaveHubScene(scene); // Only Archer Tier II; never SaveAssets on unrelated WIP.
        }
        Debug.Log("ARCHER BINDING Base=" + BindingDescription(starter));
        Debug.Log("ARCHER BINDING archer_tier_1=" + BindingDescription(tierOne));
        Debug.Log("ARCHER BINDING archer_tier_2=" + BindingDescription(visual));
        // Inspect camera presentation without writing equipment or saving animation poses.
        foreach (var hero in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<HubHeroPresentation>(true)))
        {
            var animator = hero.GetComponentInChildren<Animator>();
            var controller = (AnimatorController)animator.runtimeAnimatorController;
            var motion = controller.layers[0].stateMachine.defaultState.motion;
            var idle = motion is BlendTree tree ? tree.children[0].motion as AnimationClip : motion as AnimationClip;
            idle?.SampleAnimation(animator.gameObject, 0f);
        }
        var bows = new[]{starter, tierOne, visual};
        for (int i = 0; i < bows.Length; i++)
        {
            foreach (var bow in bows) bow.SetActive(false);
            bows[i].SetActive(true);
        }
        EditorSceneManager.OpenScene(ScenePath); // Discard all preview activation/pose changes.
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Archer Tier II authoring complete; Hub_Armory open for manual equip/Base QA.");
    }

    [MenuItem("Tools/Hub Armory/Integrate Available Tier II Weapons")]
    public static void IntegrateAvailableTierTwoWeapons()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before Tier II authoring.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        foreach (string hero in new[]{"Warrior", "Gunner"})
        {
            var presentation = Find(hero + "_VisualDisplay").GetComponent<HubHeroPresentation>();
            var data = new SerializedObject(presentation);
            var starter = (GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue;
            var entries = data.FindProperty("tierWeapons");
            SerializedProperty slot = null;
            for (int i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("weaponId").stringValue == hero.ToLowerInvariant() + "_tier_2")
                    slot = entries.GetArrayElementAtIndex(i);
            if (starter == null || slot == null) throw new InvalidOperationException("Missing binding: " + hero);
            if (slot.FindPropertyRelative("visualObject").objectReferenceValue != null) continue;
            GameObject visual;
            if (hero == "Warrior")
            {
                var mesh = AssetDatabase.LoadAllAssetsAtPath(Cardinal + "Low Poly Fantasy Weapon Pack/Greataxe.fbx").OfType<Mesh>().Single();
                var material = AssetDatabase.LoadAssetAtPath<Material>(Cardinal + "M_CardinalWeapons.mat");
                if (material == null) throw new InvalidOperationException("Missing Cardinal presentation material.");
                visual = new GameObject("Warrior_Cardinal_TierII");
                visual.transform.SetParent(starter.transform.parent, false);
                visual.transform.localPosition = starter.transform.localPosition;
                visual.transform.localRotation = starter.transform.localRotation;
                visual.transform.localScale = starter.transform.localScale;
                var model = new GameObject(mesh.name);
                model.transform.SetParent(visual.transform, false);
                model.AddComponent<MeshFilter>().sharedMesh = mesh;
                model.AddComponent<MeshRenderer>().sharedMaterial = material;
                FitCardinalMesh(model.transform, mesh, starter.transform, false);
            }
            else
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Kenney + "blaster-q.fbx");
                if (prefab == null) throw new InvalidOperationException("Missing Kenney Tier II model.");
                visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, starter.transform.parent);
                visual.name = "Carbine_TierII";
                visual.transform.localPosition = starter.transform.localPosition;
                visual.transform.localRotation = starter.transform.localRotation;
                visual.transform.localScale = starter.transform.localScale;
            }
            visual.SetActive(false);
            slot.FindPropertyRelative("visualObject").objectReferenceValue = visual;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        SaveHubScene(scene); // Scene only; never save unrelated material assets.
        scene = EditorSceneManager.OpenScene(ScenePath);
        var presentations = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<HubHeroPresentation>(true)).ToArray();
        foreach (var presentation in presentations)
        {
            var data = new SerializedObject(presentation);
            Debug.Log("TIER AUDIT " + presentation.name + " Base=" + BindingDescription((GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue));
            var entries = data.FindProperty("tierWeapons");
            for (int i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                Debug.Log("TIER AUDIT " + entry.FindPropertyRelative("weaponId").stringValue + "=" + BindingDescription((GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue));
            }
            var animator = presentation.GetComponentInChildren<Animator>();
            var controller = (AnimatorController)animator.runtimeAnimatorController;
            var motion = controller.layers[0].stateMachine.defaultState.motion;
            var idle = motion is BlendTree tree ? tree.children[0].motion as AnimationClip : motion as AnimationClip;
            idle?.SampleAnimation(animator.gameObject, 0f);
        }
        // Camera review only; do not equip weapons or write player saves.
        for (int tier = 0; tier <= 2; tier++)
        {
            foreach (var presentation in presentations)
            {
                var data = new SerializedObject(presentation);
                var starter = (GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue;
                var entries = data.FindProperty("tierWeapons");
                GameObject selected = starter;
                for (int i = 0; i < entries.arraySize; i++)
                {
                    var entry = entries.GetArrayElementAtIndex(i);
                    var visual = (GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue;
                    if (visual == null) continue;
                    visual.SetActive(false);
                    if (entry.FindPropertyRelative("weaponId").stringValue.EndsWith("_tier_" + tier)) selected = visual;
                }
                starter.SetActive(false);
                selected.SetActive(true);
            }
        }
        EditorSceneManager.OpenScene(ScenePath); // Discard preview poses and activation.
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Tier II authoring complete. Archer Tier II remains an asset blocker. Hub_Armory open for manual QA.");
    }

    [InitializeOnLoadMethod]
    static void ScheduleAttachmentCorrection()
    {
        EditorApplication.delayCall += ApplyRequestedAttachmentCorrection;
    }

    static void ApplyRequestedAttachmentCorrection()
    {
        const string bowSyncRequest = "Logs/Phase25/sync-bows.request";
        if (File.Exists(bowSyncRequest))
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ApplyRequestedAttachmentCorrection;
                return;
            }
            File.Delete(bowSyncRequest);
            SyncArcherBowPoses();
            return;
        }
        const string request = "Logs/Phase25/align-tierI.request";
        if (!File.Exists(request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ApplyRequestedAttachmentCorrection;
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += ApplyRequestedAttachmentCorrection;
            return;
        }
        File.Delete(request);
        AlignCardinalAttachments();
    }

    [MenuItem("Tools/Hub Armory/Align Cardinal Attachments Only")]
    public static void AlignCardinalAttachments()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before attachment authoring; no unsaved scene changes were discarded.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var pairs = new System.Collections.Generic.List<(GameObject starter, GameObject tier, Animator animator)>();
        foreach (string hero in new[]{"Warrior", "Archer"})
        {
            var presentation = Find(hero + "_VisualDisplay").GetComponent<HubHeroPresentation>();
            var data = new SerializedObject(presentation);
            var starter = (GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue;
            var entries = data.FindProperty("tierWeapons");
            GameObject tier = null;
            for (int i=0; i<entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("weaponId").stringValue == hero.ToLowerInvariant() + "_tier_1")
                    tier = (GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue;
            }
            if (starter == null || tier == null) throw new InvalidOperationException("Missing approved mapping for " + hero);
            var model = tier.GetComponentInChildren<MeshFilter>(true);
            FitCardinalMesh(model.transform, model.sharedMesh, starter.transform, hero == "Archer");
            Debug.Log(hero + " Tier I mesh-child alignment: rotation=" + model.transform.localEulerAngles +
                " position=" + model.transform.localPosition + " scale=" + model.transform.localScale);
            pairs.Add((starter, tier, presentation.GetComponentInChildren<Animator>()));
        }
        SaveHubScene(scene); // Only the two mesh-child transforms changed.
        // Sample existing idle poses for camera inspection, without saving poses/equipment state.
        foreach (var pair in pairs)
        {
            var controller = (AnimatorController)pair.animator.runtimeAnimatorController;
            var motion = controller.layers[0].stateMachine.defaultState.motion;
            var idle = motion is BlendTree tree ? tree.children[0].motion as AnimationClip : motion as AnimationClip;
            idle?.SampleAnimation(pair.animator.gameObject, 0f);
            pair.starter.SetActive(true);
            pair.tier.SetActive(false);
        }
        foreach (var pair in pairs) { pair.starter.SetActive(false); pair.tier.SetActive(true); }
        EditorSceneManager.OpenScene(ScenePath);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Tier I attachment-only correction complete; Hub_Armory open for manual QA.");
    }

    // Reusable, targeted integration: only missing Warrior/Archer Tier I hand visuals.
    [MenuItem("Tools/Hub Armory/Integrate Cardinal Tier I Weapons")]
    public static void IntegrateCardinalWeapons()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        string assets = Cardinal + "Low Poly Fantasy Weapon Pack/";
        var textureImporter = (TextureImporter)AssetImporter.GetAtPath(assets + "Texture.png");
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.mipmapEnabled = false;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.SaveAndReimport();
        string materialPath = Cardinal + "M_CardinalWeapons.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(assets + "Texture.png"));
            material.SetFloat("_Smoothness", .3f);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        foreach (string hero in new[]{"Warrior", "Archer"})
        {
            var presentation = Find(hero + "_VisualDisplay").GetComponent<HubHeroPresentation>();
            var data = new SerializedObject(presentation);
            var starter = (GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue;
            string id = hero.ToLowerInvariant() + "_tier_1";
            var entries = data.FindProperty("tierWeapons");
            SerializedProperty slot = null;
            for (int i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("weaponId").stringValue == id)
                    slot = entries.GetArrayElementAtIndex(i);
            if (starter == null || slot == null) throw new InvalidOperationException("Missing binding slot: " + id);
            var visual = (GameObject)slot.FindPropertyRelative("visualObject").objectReferenceValue;
            if (visual == null)
            {
                string modelPath = assets + (hero == "Warrior" ? "Greatsword.fbx" : "Longbow.fbx");
                var mesh = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Mesh>().Single();
                visual = new GameObject(hero + "_Cardinal_TierI");
                visual.transform.SetParent(starter.transform.parent, false);
                visual.transform.localPosition = starter.transform.localPosition;
                visual.transform.localRotation = starter.transform.localRotation;
                visual.transform.localScale = starter.transform.localScale;
                var model = new GameObject(mesh.name);
                model.transform.SetParent(visual.transform, false);
                model.AddComponent<MeshFilter>().sharedMesh = mesh;
                model.AddComponent<MeshRenderer>().sharedMaterial = material;
                FitCardinalMesh(model.transform, mesh, starter.transform, hero == "Archer");
                visual.SetActive(false);
                slot.FindPropertyRelative("visualObject").objectReferenceValue = visual;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            Debug.Log("Cardinal mapping " + id + " -> " + BindingDescription(visual));
        }
        // Save this scene only. Do not SaveAssets: unrelated material drift must remain untouched.
        SaveHubScene(scene);
        scene = EditorSceneManager.OpenScene(ScenePath);
        var presentations = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<HubHeroPresentation>(true)).ToArray();
        foreach (var presentation in presentations)
        {
            var data = new SerializedObject(presentation);
            Debug.Log(presentation.name + " Base=" + BindingDescription((GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue));
            var entries = data.FindProperty("tierWeapons");
            for (int i=0; i<entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                Debug.Log(entry.FindPropertyRelative("weaponId").stringValue + "=" + BindingDescription((GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue));
            }
            var animator = presentation.GetComponentInChildren<Animator>();
            var controller = (AnimatorController)animator.runtimeAnimatorController;
            var motion = controller.layers[0].stateMachine.defaultState.motion;
            var idle = motion is BlendTree tree ? tree.children[0].motion as AnimationClip : motion as AnimationClip;
            if (idle != null) idle.SampleAnimation(animator.gameObject, 0f);
            // Preview the authored Tier I slots without changing the player's equipped IDs.
            var starter = (GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue;
            for (int i=0; i<entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                var visual = (GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue;
                if (visual == null) continue;
                bool tierOne = entry.FindPropertyRelative("weaponId").stringValue.EndsWith("_tier_1");
                visual.SetActive(tierOne);
                if (tierOne) starter.SetActive(false);
            }
        }
        EditorSceneManager.OpenScene(ScenePath); // Discard preview poses/activation; leave production scene for QA.
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Cardinal Tier I integration complete; Hub_Armory open for manual equip/Base QA. Tier II remains unavailable.");
    }

    static void FitCardinalMesh(Transform model, Mesh mesh, Transform starter, bool bow)
    {
        // Match BOTH the long axis and face plane to the Base mesh in its attachment frame.
        // Copying the starter Transform alone is insufficient: its FBX mesh has its own local axes.
        var source = starter.GetComponentInChildren<MeshFilter>(true);
        var referenceVertices = source.sharedMesh.vertices.Select(v => starter.InverseTransformPoint(source.transform.TransformPoint(v))).ToArray();
        var reference = WeaponFrame(referenceVertices, bow);
        var imported = WeaponFrame(mesh.vertices, bow);
        Quaternion rotation = reference.rotation * Quaternion.Inverse(imported.rotation);
        float factor = reference.length * (bow ? 1.04f : 1.08f) / imported.length;
        model.localRotation = rotation;
        model.localScale = Vector3.one * factor;
        model.localPosition = reference.grip - rotation * imported.grip * factor;
    }

    static (Quaternion rotation, Vector3 grip, float length) WeaponFrame(Vector3[] vertices, bool bow)
    {
        var bounds = new Bounds(vertices[0], Vector3.zero);
        foreach (var v in vertices) bounds.Encapsulate(v);
        var axes = new[]{Vector3.right, Vector3.up, Vector3.forward}.OrderByDescending(a => Vector3.Dot(bounds.size, a)).ToArray();
        Vector3 along = axes[0];
        Vector3 width = axes[1];
        float length = Vector3.Dot(bounds.size, along);
        // Swords extend away from their grip-origin; bows are symmetric along the limbs.
        if (!bow && Vector3.Dot(bounds.center, along) < 0f) along = -along;
        float center = Vector3.Dot(bounds.center, along);
        Vector3 grip = bounds.center;
        if (bow)
        {
            var middle = vertices.Where(v => Mathf.Abs(Vector3.Dot(v, along) - center) < length * .055f).ToArray();
            float gripWidth = (middle.Min(v => Vector3.Dot(v, width)) + middle.Max(v => Vector3.Dot(v, width))) * .5f;
            grip += width * (gripWidth - Vector3.Dot(grip, width));
            if (Vector3.Dot(bounds.center - grip, width) < 0f) width = -width;
        }
        else grip += along * (-length * .38f); // Mid-grip, above the pommel at the short end.
        return (Quaternion.LookRotation(Vector3.Cross(width, along), along), grip, length);
    }
    // Binding-only maintenance. Never invokes the earlier visual/composition authoring pass.
    [MenuItem("Tools/Hub Armory/Repair Weapon Bindings Only")]
    public static void RepairWeaponBindingsOnly()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var ui = new SerializedObject(Find("ArmoryCanvas").GetComponent<ArmoryHubUI>());
        var characters = ui.FindProperty("characters");
        var bindings = ui.FindProperty("heroPresentations");
        var presentations = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<HubHeroPresentation>(true)).ToArray();
        bindings.arraySize = characters.arraySize;
        for (int i = 0; i < characters.arraySize; i++)
        {
            var character = (DungeonRoguelite.Characters.CharacterDefinition)characters.GetArrayElementAtIndex(i).objectReferenceValue;
            var presentation = presentations.Single(p =>
                new SerializedObject(p).FindProperty("characterDef").objectReferenceValue == character);
            bindings.GetArrayElementAtIndex(i).objectReferenceValue = presentation;
            var data = new SerializedObject(presentation);
            var starter = (GameObject)data.FindProperty("baseWeaponVisual").objectReferenceValue;
            if (starter == null || !starter.transform.IsChildOf(presentation.transform))
                throw new InvalidOperationException(character.Id + " has an invalid Base binding.");
            var entries = data.FindProperty("tierWeapons");
            var existing = new System.Collections.Generic.Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            for (int j = 0; j < entries.arraySize; j++)
            {
                var entry = entries.GetArrayElementAtIndex(j);
                existing.Add(entry.FindPropertyRelative("weaponId").stringValue,
                    (GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue);
            }
            var weapons = character.WeaponCatalog.Weapons.Where(w => w != null &&
                string.Equals(w.CharacterId, character.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
            entries.arraySize = weapons.Length;
            Debug.Log("BINDING " + character.Id + " Base=" + BindingDescription(starter));
            for (int j = 0; j < weapons.Length; j++)
            {
                existing.TryGetValue(weapons[j].Id, out var visual);
                if (visual != null && (!visual.transform.IsChildOf(presentation.transform) ||
                    visual == starter || visual.transform.IsChildOf(starter.transform)))
                    throw new InvalidOperationException("Invalid tier binding: " + weapons[j].Id);
                var entry = entries.GetArrayElementAtIndex(j);
                entry.FindPropertyRelative("weaponId").stringValue = weapons[j].Id;
                // Explicit missing entries identify the asset blocker; never fake a swap with a starter copy.
                entry.FindPropertyRelative("visualObject").objectReferenceValue = visual;
                Debug.Log("BINDING " + weapons[j].Id + "=" + BindingDescription(visual));
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        ui.ApplyModifiedPropertiesWithoutUndo();
        SaveHubScene(scene);
        // Reopen serialized bindings before inspection. Reads equipment only; no equip/claim/save calls.
        scene = EditorSceneManager.OpenScene(ScenePath);
        foreach (var presentation in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<HubHeroPresentation>(true)))
        {
            var data = new SerializedObject(presentation);
            var character = (DungeonRoguelite.Characters.CharacterDefinition)data.FindProperty("characterDef").objectReferenceValue;
            var equipped = DungeonRoguelite.Progression.PermanentProgression.GetEquippedWeapon(character.Id, character.WeaponCatalog);
            presentation.RefreshVisuals();
            Debug.Log("CURRENT " + character.Id + " equipped=" + (equipped != null ? equipped.Id : "Base") +
                " activeMeshes=" + string.Join(",", presentation.GetComponentsInChildren<MeshFilter>().Select(m => m.name + ":" + AssetDatabase.GetAssetPath(m.sharedMesh))));
        }
        OpenForQA(); // Discard inspection activation changes and leave the production scene open.
    }
    static string BindingDescription(GameObject visual) => visual == null ? "MISSING (Base fallback)" :
        visual.name + " [" + string.Join(",", visual.GetComponentsInChildren<MeshFilter>(true).Select(m => AssetDatabase.GetAssetPath(m.sharedMesh))) + "]";
    public static void RunBatch()
    {
        EditorSceneManager.OpenScene(ScenePath);
        ApplyViaMenu();
    }
    public static void RecoverAndOpenForQA()
    {
        RunBatch();
        OpenForQA();
    }
    public static void OpenForQA()
    {
        EditorSceneManager.OpenScene(ScenePath);
        var roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var component in roots.SelectMany(r => r.GetComponentsInChildren<Component>(true)))
        {
            if (component == null) { Debug.LogError("Hub missing script"); continue; }
            if (!(component is MonoBehaviour)) continue;
            var data = new SerializedObject(component);
            var property = data.GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                    Debug.LogError("Hub missing reference: " + component.name + "/" + property.propertyPath);
        }
        Selection.activeGameObject = Find("WarriorDisplay");
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Phase 25 Hub open for manual QA. Warrior/Archer Tier I asset blockers remain. No save writes.");
    }
    [MenuItem("Tools/Hub Armory/Apply Phase 25 Recovery")]
    public static void ApplyViaMenu()
    {
        if (EditorSceneManager.GetActiveScene().path != ScenePath)
            throw new InvalidOperationException("Open Hub_Armory first.");
        var presentations = new HubHeroPresentation[3];
        int index = 0;
        foreach (string hero in new[]{"Warrior", "Archer", "Gunner"})
        {
            var display = Find(hero + "_VisualDisplay");
            var visualRoot = display.transform.Find("VisualRoot");
            foreach (var child in display.GetComponentsInChildren<Transform>(true).ToArray())
                if (child != null && (child.name.EndsWith("_Tier1") || child.name.EndsWith("_Tier2")))
                    Object.DestroyImmediate(child.gameObject);
            // Keep combat anchor transforms; remove obsolete prototype rendering only.
            foreach (var renderer in display.GetComponentsInChildren<Renderer>(true))
                if (!renderer.transform.IsChildOf(visualRoot)) Object.DestroyImmediate(renderer);
            foreach (var mesh in display.GetComponentsInChildren<MeshFilter>(true))
                if (!mesh.transform.IsChildOf(visualRoot)) Object.DestroyImmediate(mesh);
            GameObject baseWeapon;
            GameObject tierWeapon = null;
            if (hero == "Gunner")
            {
                var firearm = Descendant(visualRoot, "FirearmVisual");
                foreach (Transform child in firearm.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
                baseWeapon = Instantiate(Kenney + "blaster-g.fbx", firearm, "Carbine_Base");
                tierWeapon = Instantiate(Kenney + "blaster-p.fbx", firearm, "Carbine_TierI");
                foreach (var weapon in new[]{baseWeapon, tierWeapon})
                {
                    weapon.transform.localScale = Vector3.one * 1.15f;
                    weapon.transform.localPosition = Vector3.zero;
                    weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                }
                tierWeapon.SetActive(false);
            }
            else
            {
                baseWeapon = Descendant(visualRoot, hero == "Warrior" ? "sword_2handed" : "BowVisual").gameObject;
                Debug.LogWarning(hero + " VISUAL BLOCKER: local pack has no distinct appropriate Tier I silhouette; starter retained.");
            }
            baseWeapon.SetActive(true);
            var presentation = display.GetComponent<HubHeroPresentation>();
            var so = new SerializedObject(presentation);
            so.FindProperty("baseWeaponVisual").objectReferenceValue = baseWeapon;
            var entries = so.FindProperty("tierWeapons");
            entries.arraySize = tierWeapon != null ? 1 : 0;
            if (tierWeapon != null)
            {
                entries.GetArrayElementAtIndex(0).FindPropertyRelative("weaponId").stringValue = "gunner_tier_1";
                entries.GetArrayElementAtIndex(0).FindPropertyRelative("visualObject").objectReferenceValue = tierWeapon;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            presentations[index++] = presentation;
        }
        var uiData = new SerializedObject(Find("ArmoryCanvas").GetComponent<ArmoryHubUI>());
        var bindings = uiData.FindProperty("heroPresentations");
        bindings.arraySize = presentations.Length;
        for (int i=0; i<presentations.Length; i++) bindings.GetArrayElementAtIndex(i).objectReferenceValue = presentations[i];
        uiData.ApplyModifiedPropertiesWithoutUndo();
        // Put the interaction alongside the NPC, clear of its body and the hero row.
        Find("ArmorerButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, 220f);
        SetupArmorer();
        SaveHubScene(EditorSceneManager.GetActiveScene());
        foreach (var presentation in presentations)
        {
            var animator = presentation.GetComponentInChildren<Animator>();
            var controller = (AnimatorController)animator.runtimeAnimatorController;
            var motion = controller.layers[0].stateMachine.defaultState.motion;
            var idle = motion is BlendTree tree ? tree.children[0].motion as AnimationClip : motion as AnimationClip;
            if (idle != null) idle.SampleAnimation(animator.gameObject, 0f);
        }
        Directory.CreateDirectory("Logs/Phase25");
        var gunnerData = new SerializedObject(presentations[2]);
        var starter = (GameObject)gunnerData.FindProperty("baseWeaponVisual").objectReferenceValue;
        var tier = (GameObject)gunnerData.FindProperty("tierWeapons").GetArrayElementAtIndex(0).FindPropertyRelative("visualObject").objectReferenceValue;
        starter.SetActive(false);
        tier.SetActive(true);
        starter.SetActive(true);
        tier.SetActive(false);
        foreach (var p in presentations)
        {
            Debug.Log("PASSIVE " + p.name + ": " + string.Join(",", p.GetComponentsInChildren<MonoBehaviour>(true).Select(m => m ? m.GetType().Name : "MISSING")));
            foreach (var mesh in p.GetComponentsInChildren<MeshFilter>(true))
                Debug.Log("MESH " + mesh.name + " = " + AssetDatabase.GetAssetPath(mesh.sharedMesh));
        }
        File.WriteAllLines("Logs/Phase25/dependencies.txt", AssetDatabase.GetDependencies(ScenePath));
    }
    static void SetupArmorer()
    {
        var root = Find("ArmorerPedestal").transform;
        var model = Descendant(root, "ArmorerVisual");
        root.position = new Vector3(-2.5f, 0f, 5.5f);
        model.localScale = Vector3.one * 1.65f;
        model.localPosition = new Vector3(0f, -.05f, 0f);
        Descendant(model, "Barbarian_BearHat").gameObject.SetActive(false);
        var clips = AssetDatabase.LoadAllAssetsAtPath(Kay + "Animations/fbx/Rig_Medium/Rig_Medium_General.fbx").OfType<AnimationClip>().ToArray();
        Debug.Log("Armorer available poses: " + string.Join(",", clips.Select(c => c.name)));
        var idle = clips.FirstOrDefault(c => c.name == "Idle_A") ?? clips.FirstOrDefault(c => c.name.Contains("Idle") && !c.name.StartsWith("__"));
        if (idle != null) idle.SampleAnimation(model.gameObject, 0f);
        var workbench = Descendant(root, "Workbench");
        if (workbench != null) Object.DestroyImmediate(workbench.gameObject);
        var storage = Descendant(root, "ArmorerStorage");
        if (storage == null) storage = Instantiate(Kenney + "crate-small.fbx", root, "ArmorerStorage").transform;
        storage.localScale = Vector3.one * 2f;
        storage.localPosition = new Vector3(-1.5f, -.05f, 0f);
        var oldAnvil = Find("Anvil");
        if (oldAnvil != null) oldAnvil.SetActive(false);
        var hand = Descendant(model, "handslot.r");
        // General idle clips hide equipment sockets by scaling them to zero.
        hand.localScale = Vector3.one;
        var sword = Descendant(hand, "ArmorerInspectionSword");
        if (sword == null) sword = Instantiate(Kay + "Assets/fbx/sword_1handed.fbx", hand, "ArmorerInspectionSword").transform;
        // Preserve the FBX unit conversion on the root (this pack imports at 100x).
        var swordAsset = AssetDatabase.LoadAssetAtPath<GameObject>(Kay + "Assets/fbx/sword_1handed.fbx");
        sword.localScale = swordAsset.transform.localScale * .55f;
        sword.rotation = Quaternion.LookRotation(new Vector3(-.55f, 1f, 0f), Vector3.forward);
        Debug.Log("Armorer sword: active=" + sword.gameObject.activeInHierarchy + " scale=" + sword.lossyScale);
        foreach (var renderer in sword.GetComponentsInChildren<Renderer>())
            Debug.Log("Armorer sword renderer: enabled=" + renderer.enabled + " bounds=" + renderer.bounds);
    }
    static GameObject Instantiate(string path, Transform parent, string name)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) throw new InvalidOperationException("Missing model: " + path);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        return instance;
    }
    static Transform Descendant(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
    static GameObject Find(string name) => EditorSceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name)?.gameObject;
}
