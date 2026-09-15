using System;
using System.IO;
using System.Linq;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Presentation;
using DungeonRoguelite.UI;
using DungeonRoguelite.Weapons;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Offline icon authoring only. No preview camera or model is stored in the game scene.
public static class ArmoryWeaponPreviewAuthoring
{
    const string ScenePath = "Assets/Scenes/Hub_Armory.unity";
    const string Folder = "Assets/Art/ArmoryWeaponPreviews";

    [MenuItem("Tools/Hub Armory/Bake Weapon Preview Icons")]
    public static void Bake()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Stop Play mode and save the current scene before baking previews.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var ui = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ArmoryHubUI>(true)).Single();
        var data = new SerializedObject(ui);
        var panel = ((GameObject)data.FindProperty("equipmentPanel").objectReferenceValue).transform;
        var text = (TMP_Text)data.FindProperty("equipmentText").objectReferenceValue;
        var entries = data.FindProperty("weaponPreviews");
        entries.ClearArray();
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "ArmoryWeaponPreviews");

        var presentations = data.FindProperty("heroPresentations");
        for (int i = 0; i < presentations.arraySize; i++)
        {
            var source = new SerializedObject(presentations.GetArrayElementAtIndex(i).objectReferenceValue);
            var character = (CharacterDefinition)source.FindProperty("characterDef").objectReferenceValue;
            AddEntry(character, null, (GameObject)source.FindProperty("baseWeaponVisual").objectReferenceValue);
            var tiers = source.FindProperty("tierWeapons");
            for (int j = 0; j < tiers.arraySize; j++)
            {
                var tier = tiers.GetArrayElementAtIndex(j);
                string id = tier.FindPropertyRelative("weaponId").stringValue;
                var weapon = character.WeaponCatalog.Weapons.Single(w => w != null && w.Id == id);
                AddEntry(character, weapon, (GameObject)tier.FindPropertyRelative("visualObject").objectReferenceValue);
            }
        }

        void AddEntry(CharacterDefinition character, WeaponDefinition weapon, GameObject visual)
        {
            string id = weapon != null ? weapon.Id : character.Id + "_base";
            Sprite sprite = visual != null ? BakeSprite(visual, id) : null;
            int index = entries.arraySize++;
            var entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("character").objectReferenceValue = character;
            entry.FindPropertyRelative("weapon").objectReferenceValue = weapon;
            entry.FindPropertyRelative("sprite").objectReferenceValue = sprite;
            Debug.Log("[ARMORY PREVIEW] " + id + " " + (sprite != null ? "READY" : "fallback: missing source visual"));
        }

        var slot = Child(panel, "WeaponPreviewSlot", new Vector2(-305f, 105f), new Vector2(180f, 180f));
        var background = slot.GetComponent<Image>() ?? slot.gameObject.AddComponent<Image>();
        background.color = new Color(.035f, .045f, .06f, 1f);
        background.raycastTarget = false;
        var border = slot.GetComponent<Outline>() ?? slot.gameObject.AddComponent<Outline>();
        border.effectColor = new Color(.38f, .32f, .22f, .7f);
        border.effectDistance = new Vector2(1f, -1f);
        var iconRect = Child(slot, "WeaponPreviewImage", Vector2.zero, new Vector2(160f, 160f));
        var icon = iconRect.GetComponent<Image>() ?? iconRect.gameObject.AddComponent<Image>();
        icon.color = Color.white;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.sprite = null;
        icon.enabled = false;
        var fallbackRect = Child(slot, "PreviewFallback", Vector2.zero, new Vector2(155f, 70f));
        var fallback = fallbackRect.GetComponent<TextMeshProUGUI>() ?? fallbackRect.gameObject.AddComponent<TextMeshProUGUI>();
        fallback.font = text.font;
        fallback.text = "G\u00d6RSEL\nYOK";
        fallback.fontSize = 18f;
        fallback.alignment = TextAlignmentOptions.Center;
        fallback.color = new Color(.55f, .57f, .6f);
        fallback.raycastTarget = false;
        text.rectTransform.anchoredPosition = new Vector2(100f, 90f);
        text.rectTransform.sizeDelta = new Vector2(550f, 420f);
        data.FindProperty("weaponPreviewImage").objectReferenceValue = icon;
        data.FindProperty("weaponPreviewFallback").objectReferenceValue = fallback;
        data.ApplyModifiedPropertiesWithoutUndo();
        // Deliberately bypass the older Hub authoring tool, which also syncs weapon poses.
        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.OpenScene(ScenePath);
        Debug.Log("[ARMORY PREVIEW] COMPLETE: static icons and popup bindings saved; Hub open for manual QA.");
    }

    static RectTransform Child(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var existing = parent.Find(name);
        var rect = existing != null ? (RectTransform)existing :
            new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    static Sprite BakeSprite(GameObject source, string id)
    {
        var preview = new PreviewRenderUtility();
        var root = new GameObject("OfflineWeaponIcon");
        Texture2D texture = null;
        try
        {
            // Copy only geometry/materials: never run source components or mutate the approved model.
            foreach (var filter in source.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (filter.sharedMesh == null || renderer == null) continue;
                var part = new GameObject("IconMesh", typeof(MeshFilter), typeof(MeshRenderer));
                part.transform.SetParent(root.transform, false);
                Matrix4x4 matrix = source.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                part.transform.localPosition = matrix.GetColumn(3);
                part.transform.localRotation = matrix.rotation;
                part.transform.localScale = matrix.lossyScale;
                part.GetComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                part.GetComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;
            }
            var renderers = root.GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("No weapon geometry for " + id);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            var axes = new[] { Vector3.right, Vector3.up, Vector3.forward }
                .OrderByDescending(axis => Vector3.Dot(bounds.size, axis)).ToArray();
            Vector3 normal = Vector3.Cross(axes[1], axes[0]).normalized;
            preview.AddSingleGO(root);
            preview.camera.orthographic = true;
            preview.camera.orthographicSize = bounds.size.magnitude * .59f;
            preview.camera.nearClipPlane = .01f;
            preview.camera.farClipPlane = bounds.size.magnitude * 10f + 10f;
            preview.camera.transform.position = bounds.center + normal * (bounds.size.magnitude * 3f + 1f);
            preview.camera.transform.rotation = Quaternion.LookRotation(-normal, axes[0]) * Quaternion.Euler(0f, 0f, -30f);
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = new Color(.035f, .045f, .06f, 1f);
            preview.lights[0].intensity = 1.3f;
            preview.lights[0].transform.rotation = preview.camera.transform.rotation * Quaternion.Euler(25f, -25f, 0f);
            preview.lights[1].intensity = .8f;
            preview.lights[1].transform.rotation = preview.camera.transform.rotation * Quaternion.Euler(-20f, 35f, 0f);
            preview.ambientColor = new Color(.4f, .4f, .4f);
            preview.BeginStaticPreview(new Rect(0, 0, 256, 256));
            preview.Render(true);
            texture = preview.EndStaticPreview();
            string path = Folder + "/" + id + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 256;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        finally
        {
            if (texture != null) Object.DestroyImmediate(texture);
            preview.Cleanup();
            if (root != null) Object.DestroyImmediate(root);
        }
    }
}
