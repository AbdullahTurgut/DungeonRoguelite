using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.UI;

namespace DungeonRoguelite.Editor
{
    /// <summary>Scoped, repeatable authoring of the existing World Map presentation only.</summary>
    public static class WorldMapCarouselSetup
    {
        public const string ScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";

        [MenuItem("DungeonRoguelite/Phase 14/Setup World Map Carousel")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var map = UnityEngine.Object.FindFirstObjectByType<WorldMapController>();
            if (map == null) throw new InvalidOperationException("WorldMapController missing");
            Apply(map);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[WORLD MAP CAROUSEL SETUP COMPLETE]");
        }

        public static void Apply(WorldMapController map)
        {
            var root = map.transform;
            var header = Rect(root, "Header", Vector2.zero, new Vector2(1920, 1080));
            Place(map.TitleText.rectTransform, header, new Vector2(0, 420), new Vector2(800, 80));
            Place(map.ActiveHeroText.rectTransform, header, new Vector2(0, 350), new Vector2(600, 45));
            var viewport = Rect(root, "CarouselViewport", new Vector2(0, 25), new Vector2(1740, 560));
            var container = Rect(viewport, "CardsContainer", Vector2.zero, new Vector2(1740, 560));
            var left = NavigationButton(viewport, "LeftNavButton", "<", -790);
            var right = NavigationButton(viewport, "RightNavButton", ">", 790);
            map.SetCarouselReferences(container, left, right);

            // Preserve serialized card identities and all their bindings. Runtime clones inherit this template.
            for (int i = 0; i < map.Cards.Count; i++)
            {
                var card = map.Cards[i];
                Place((RectTransform)card.transform, container, new Vector2((i - 1) * 500, 20), new Vector2(440, 450));
                ConfigureText(card.TitleText, new Vector2(0, 145), new Vector2(400, 72), 32, 2, true);
                ConfigureText(card.DescriptionText, new Vector2(0, 25), new Vector2(390, 135), 22, 3, false);
                ConfigureText(card.StatusText, new Vector2(0, -110), new Vector2(320, 42), 26, 1, true);
                // Keep locked cards readable and clickable for inspection, including their focus outline.
                if (card.LockOverlay != null)
                {
                    var image = card.LockOverlay.GetComponent<Image>();
                    if (image != null) { image.color = new Color(.04f, .05f, .08f, .12f); image.raycastTarget = false; }
                    var label = card.LockOverlay.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (label != null) ConfigureText(label, new Vector2(0, -175), new Vector2(320, 42), 26, 1, true);
                }
                if (card.SelectionBorder != null)
                {
                    card.SelectionBorder.transform.SetAsLastSibling();
                    foreach (var graphic in card.SelectionBorder.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
                }
                EditorUtility.SetDirty(card);
            }
            Place((RectTransform)map.EnterDungeonButton.transform, root, new Vector2(0, -300), new Vector2(360, 64));
            Place((RectTransform)map.BackButton.transform, root, new Vector2(-790, -470), new Vector2(220, 54));
            if (map.SkillTreeButton != null)
                Place((RectTransform)map.SkillTreeButton.transform, root, new Vector2(790, -470), new Vector2(220, 54));
            if (map.SkillTreePanel != null)
            {
                var modalBlocker = map.SkillTreePanel.PanelRoot.GetComponent<Image>();
                if (modalBlocker != null)
                {
                    modalBlocker.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                    modalBlocker.raycastTarget = true;
                }

                map.SkillTreePanel.transform.SetAsLastSibling();
            }
            EditorUtility.SetDirty(map);
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var found = parent.Find(name);
            var rect = found != null ? (RectTransform)found : new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            Place(rect, parent, position, size);
            return rect;
        }

        private static void Place(RectTransform rect, Transform parent, Vector2 position, Vector2 size)
        {
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.localScale = Vector3.one;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void ConfigureText(TextMeshProUGUI text, Vector2 position, Vector2 size, float fontSize, int lines, bool bold)
        {
            Place(text.rectTransform, text.transform.parent, position, size);
            text.fontSize = fontSize;
            text.enableAutoSizing = false;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.maxVisibleLines = lines;
            text.lineSpacing = 0;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }

        private static Button NavigationButton(Transform parent, string name, string caption, float x)
        {
            var rect = Rect(parent, name, new Vector2(x, 20), new Vector2(72, 72));
            var image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.color = new Color(.18f, .24f, .35f, 1);
            var button = rect.GetComponent<Button>() ?? rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var textRect = Rect(rect, "Text", Vector2.zero, new Vector2(72, 72));
            var label = textRect.GetComponent<TextMeshProUGUI>() ?? textRect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = caption;
            ConfigureText(label, Vector2.zero, new Vector2(72, 72), 36, 1, true);
            return button;
        }
    }
}
