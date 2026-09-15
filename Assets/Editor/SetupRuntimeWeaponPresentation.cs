using System;
using System.Linq;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Presentation;
using DungeonRoguelite.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Explicit transfer of approved Hub visual branches; never saves the Hub scene.
public static class SetupRuntimeWeaponPresentation
{
    public static void Apply()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the open scene before prefab authoring.");
        const string hubPath = "Assets/Scenes/Hub_Armory.unity";
        var scene = EditorSceneManager.OpenScene(hubPath);
        var displays = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<HubHeroPresentation>(true));
        foreach (var display in displays)
        {
            var source = new SerializedObject(display);
            var definition = (CharacterDefinition)source.FindProperty("characterDef").objectReferenceValue;
            string path = AssetDatabase.GetAssetPath(definition.CharacterPrefab);
            if (!path.StartsWith("Assets/Prefabs/Characters/")) throw new InvalidOperationException("Unexpected playable prefab: " + path);
            var prefab = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (prefab.GetComponent<EquippedWeaponPresentation>() != null)
                    throw new InvalidOperationException("Runtime mappings already exist; inspect before replacing: " + path);
                var starter = (GameObject)source.FindProperty("baseWeaponVisual").objectReferenceValue;
                var tiers = source.FindProperty("tierWeapons");
                if (starter == null || tiers.arraySize != 2) throw new InvalidOperationException("Incomplete Hub mapping: " + definition.Id);
                string parentPath = AnimationUtility.CalculateTransformPath(starter.transform.parent, display.transform);
                var parent = prefab.transform.Find(parentPath);
                if (parent == null) throw new InvalidOperationException("Missing matching hand attachment: " + parentPath);
                // Disable old hand-weapon renderers only; never deactivate an origin or remove components.
                // The shared parent is either the dedicated weapon hand slot or FirearmVisual.
                if (!parent.name.StartsWith("handslot.") && parent.name != "FirearmVisual")
                    throw new InvalidOperationException("Unexpected weapon-only parent: " + parentPath);
                foreach (var renderer in parent.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                var runtime = prefab.AddComponent<EquippedWeaponPresentation>();
                var target = new SerializedObject(runtime);
                target.FindProperty("character").objectReferenceValue = prefab.GetComponent<PlayableCharacter>();
                target.FindProperty("baseWeaponVisual").objectReferenceValue = CopyVisual(starter, parent, true);
                var targetTiers = target.FindProperty("tierWeapons");
                targetTiers.arraySize = tiers.arraySize;
                for (int i = 0; i < tiers.arraySize; i++)
                {
                    var entry = tiers.GetArrayElementAtIndex(i);
                    var visual = (GameObject)entry.FindPropertyRelative("visualObject").objectReferenceValue;
                    if (visual == null) throw new InvalidOperationException("Missing approved tier: " + definition.Id);
                    var slot = targetTiers.GetArrayElementAtIndex(i);
                    slot.FindPropertyRelative("weaponId").stringValue = entry.FindPropertyRelative("weaponId").stringValue;
                    slot.FindPropertyRelative("visualObject").objectReferenceValue = CopyVisual(visual, parent, false);
                }
                target.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            // Inspect the saved asset, not just the in-memory authoring objects.
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var bindings = new SerializedObject(saved.GetComponent<EquippedWeaponPresentation>());
            var baseVisual = (GameObject)bindings.FindProperty("baseWeaponVisual").objectReferenceValue;
            LogVisual(definition.Id + " Base", baseVisual);
            var slots = bindings.FindProperty("tierWeapons");
            for (int i = 0; i < slots.arraySize; i++)
            {
                var slot = slots.GetArrayElementAtIndex(i);
                LogVisual(slot.FindPropertyRelative("weaponId").stringValue,
                    (GameObject)slot.FindPropertyRelative("visualObject").objectReferenceValue);
            }
            foreach (var component in saved.GetComponentsInChildren<Component>(true))
                if (component == null) throw new InvalidOperationException("Missing prefab script: " + path);
        }
        EditorSceneManager.OpenScene(hubPath);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Runtime weapon prefab authoring complete; Hub_Armory open for manual QA.");
    }

    static GameObject CopyVisual(GameObject source, Transform parent, bool active)
    {
        if (source.GetComponentsInChildren<Component>(true).Any(c =>
            !(c is Transform) && !(c is MeshFilter) && !(c is MeshRenderer)))
            throw new InvalidOperationException("Approved branch contains non-mesh components: " + source.name);
        var copy = Object.Instantiate(source, parent, false);
        copy.name = "Equipped_" + source.name;
        copy.transform.localPosition = source.transform.localPosition;
        copy.transform.localRotation = source.transform.localRotation;
        copy.transform.localScale = source.transform.localScale;
        copy.SetActive(active);
        return copy;
    }

    static void LogVisual(string id, GameObject visual)
    {
        if (visual == null) throw new InvalidOperationException("Missing saved visual: " + id);
        var meshes = visual.GetComponentsInChildren<MeshFilter>(true);
        if (meshes.Length == 0 || meshes.Any(m => m.sharedMesh == null)) throw new InvalidOperationException("Missing mesh: " + id);
        Debug.Log("RUNTIME MAPPING " + id + " = " + string.Join(", ", meshes.Select(m => AssetDatabase.GetAssetPath(m.sharedMesh))));
    }
}
