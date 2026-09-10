#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Progression;
using DungeonRoguelite.UI;

namespace DungeonRoguelite.Tests
{
    public sealed class WorldMapCarouselVerifier : MonoBehaviour
    {
        private Phase12StateSnapshot snapshot;
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private int checks;
        private int errors;

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            snapshot = new Phase12StateSnapshot();
            Application.logMessageReceived += OnLog;
            bool success = true;
            // Drive nested iterators here so every assertion failure reaches the same cleanup path.
            var stack = new Stack<IEnumerator>();
            stack.Push(Verify());
            while (stack.Count > 0)
            {
                object current = null;
                try
                {
                    if (!stack.Peek().MoveNext()) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
                    current = stack.Peek().Current;
                    if (current is IEnumerator child) { stack.Push(child); continue; }
                }
                catch (Exception ex) { Debug.LogException(ex); success = false; break; }
                yield return current;
                if (errors != 0) { success = false; break; }
            }
            while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
            // Tear down test-loaded scenes before restoring static events/session state.
            var cleanupScene = SceneManager.CreateScene("CarouselVerificationCleanup");
            SceneManager.SetActiveScene(cleanupScene);
            for (int i = SceneManager.sceneCount - 1; i >= 0; --i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene != cleanupScene) yield return SceneManager.UnloadSceneAsync(scene);
            }
            Cleanup();
            success &= errors == 0;
            Application.logMessageReceived -= OnLog;
            Debug.Log($"[WORLD MAP CAROUSEL COMPLETE] {(success ? "PASSED" : "FAILED")}: {checks} checks passed.");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.isPlaying = false;
        }

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception) errors++;
        }

        private void Cleanup()
        {
            foreach (var obj in owned) if (obj != null) DestroyImmediate(obj);
            owned.Clear();
            if (snapshot != null)
            {
                snapshot.Dispose(); snapshot = null;
                Debug.Log("[WORLD MAP CAROUSEL STATE RESTORED]");
            }
        }

        private void OnDestroy() { Application.logMessageReceived -= OnLog; Cleanup(); }

        private void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("[CAROUSEL CHECK FAILED] " + name);
            Debug.Log($"[CAROUSEL CHECK {++checks} PASSED] {name}");
        }

        private IEnumerator Verify()
        {
            DungeonProgression.ResetProgression();
            DungeonRunSession.Clear();
            RunProgressionSession.EndRun();
            yield return SceneManager.LoadSceneAsync("CharacterSelection");
            yield return null;
            var selection = FindFirstObjectByType<CharacterSelectionController>();
            Check(selection != null && selection.TargetSceneName == "WorldMap", "Character Select has the production World Map route");
            selection.StartButton.onClick.Invoke();
            yield return null;
            yield return null;
            var map = FindFirstObjectByType<WorldMapController>();
            Check(map != null && CharacterSelectionSession.HasSelection && RunProgressionSession.HasActiveRun,
                "Character Select button loads World Map and retains the selected hero");
            var catalog = map.DungeonCatalog;
            Check(catalog.Count == 5 && map.Cards.Count == 5 && map.Cards.Select(c => c.BoundDungeon).SequenceEqual(catalog.Dungeons),
                "Production catalog D1-D5 binds five card objects");
            Check(map.FocusedIndex == 0 && map.SelectedDungeon == catalog[0], "Fresh map focuses D1");
            CheckWindow(map, 0, 0, 3);
            var initialSession = DungeonRunSession.SelectedDungeon;
            for (int i = 0; i < 5; ++i)
            {
                var card = map.Cards[i];
                Check(card.IsUnlocked == (i == 0) && !card.IsCompleted && card.LockOverlay.activeSelf == (i != 0), $"Fresh D{i + 1} lock/completion presentation");
            }
            map.LeftNavigationButton.onClick.Invoke();
            Check(map.FocusedIndex == 0 && !map.LeftNavigationButton.interactable, "Left boundary cannot underflow");
            int[] windows = { 0, 0, 1, 2, 2 };
            for (int i = 1; i < 5; ++i)
            {
                map.RightNavigationButton.onClick.Invoke();
                CheckWindow(map, i, windows[i], 3);
                Check(!map.EnterDungeonButton.interactable, $"Locked D{i + 1} focus disables Enter");
            }
            map.RightNavigationButton.onClick.Invoke();
            Check(map.FocusedIndex == 4 && !map.RightNavigationButton.interactable, "Right boundary cannot overflow");
            Check(DungeonRunSession.SelectedDungeon == initialSession, "Browsing locked entries never commits a dungeon session");
            map.Cards[2].CardButton.onClick.Invoke();
            CheckWindow(map, 2, 1, 3);
            for (int i = 1; i >= 0; --i) { map.LeftNavigationButton.onClick.Invoke(); CheckWindow(map, i, 0, 3); }

            // Each prerequisite opens exactly its next production entry, including D4 and D5.
            for (int i = 0; i < 4; ++i)
            {
                DungeonProgression.RecordDungeonCompleted(catalog[i].Id);
                map.InitializeMap();
                Check(map.FocusedIndex == i + 1 && map.EnterDungeonButton.interactable, $"D{i + 1} completion focuses newest unlocked D{i + 2}");
                Check(map.Cards[i].IsCompleted && map.Cards[i + 1].IsUnlocked && !map.Cards[i + 1].IsCompleted,
                    $"D{i + 1}/D{i + 2} completion and unlock semantics preserved");
            }
            for (int i = 0; i < 5; ++i)
            {
                map.SelectDungeonLocally(catalog[i]);
                CheckLayout(map);
            }

            map.SelectDungeonLocally(catalog[3]);
            int focus = map.FocusedIndex;
            int window = map.WindowStart;
            var before = map.SelectedDungeon;
            map.SkillTreeButton.onClick.Invoke();
            var panel = map.SkillTreePanel;
            Check(panel.IsOpen && panel.transform.parent == map.transform && panel.transform.GetSiblingIndex() == map.transform.childCount - 1,
                "Skill Tree opens as final Canvas sibling");
            yield return null;
            Canvas.ForceUpdateCanvases();
            var pointer = new PointerEventData(EventSystem.current);
            foreach (var target in map.Cards.Where(c => c.gameObject.activeInHierarchy).Select(c => c.transform)
                .Concat(new[] { map.LeftNavigationButton.transform, map.RightNavigationButton.transform, map.EnterDungeonButton.transform }))
            {
                pointer.position = RectTransformUtility.WorldToScreenPoint(null, target.position);
                var hits = new List<RaycastResult>();
                map.GetComponent<GraphicRaycaster>().Raycast(pointer, hits);
                Check(hits.Count > 0 && (hits[0].gameObject.transform == panel.transform || hits[0].gameObject.transform.IsChildOf(panel.transform)),
                    $"Modal intercepts pointer above {target.name}");
            }
            map.LeftNavigationButton.onClick.Invoke(); map.RightNavigationButton.onClick.Invoke();
            map.Cards[2].CardButton.onClick.Invoke(); map.EnterDungeonButton.onClick.Invoke(); map.BackButton.onClick.Invoke();
            Check(map.FocusedIndex == focus && map.WindowStart == window && map.SelectedDungeon == before,
                "Modal blocks underlying navigation, selection and scene transitions");
            panel.CloseButton.onClick.Invoke();
            Check(!panel.IsOpen && map.FocusedIndex == focus && map.WindowStart == window && map.SelectedDungeon == before &&
                map.LeftNavigationButton.interactable && map.RightNavigationButton.interactable, "Close preserves complete carousel state");

            var mockCatalog = ScriptableObject.CreateInstance<DungeonCatalog>(); owned.Add(mockCatalog);
            var d6 = ScriptableObject.CreateInstance<DungeonDefinition>(); owned.Add(d6);
            d6.SetConfiguration("carousel_mock_6", "Bölüm 6: Gelecek", "Yalnızca doğrulama için geçici bölüm.", "WorldMap", "dungeon_5", Array.Empty<DungeonRoguelite.Waves.WaveDefinition>());
            for (int count = 0; count <= 6; ++count)
            {
                mockCatalog.SetDungeons(catalog.Dungeons.Concat(new[] { d6 }).Take(count).ToArray());
                map.SetReferences(mockCatalog, map.Cards.ToArray(), map.EnterDungeonButton, map.BackButton, map.ActiveHeroText, map.TitleText);
                map.InitializeMap();
                if (count == 0)
                {
                    Check(map.Cards.All(c => !c.gameObject.activeSelf) && map.SelectedDungeon == null && !map.EnterDungeonButton.interactable &&
                        !map.LeftNavigationButton.interactable && !map.RightNavigationButton.interactable, "Empty catalog safely disables selection and navigation");
                    continue;
                }
                for (int i = 0; i < count; ++i)
                {
                    map.SelectDungeonLocally(mockCatalog[i]);
                    CheckWindow(map, i, Mathf.Clamp(i - 1, 0, Mathf.Max(0, count - 3)), Mathf.Min(3, count));
                }
            }
            Check(catalog.Count == 5 && !EditorUtility.IsDirty(catalog), "Sixth mock entry never mutates production catalog");
            map.SetReferences(catalog, map.Cards.ToArray(), map.EnterDungeonButton, map.BackButton, map.ActiveHeroText, map.TitleText);
            map.InitializeMap();
            Check(map.Cards.Count == 6 && !map.Cards[5].gameObject.activeSelf, "Catalog shrink hides surplus reusable cards");

            map.SelectDungeonLocally(catalog[4]);
            map.EnterDungeonButton.onClick.Invoke();
            yield return null;
            Check(SceneManager.GetActiveScene().name == catalog[4].SceneName && DungeonRunSession.SelectedDungeon == catalog[4],
                "Production Enter button commits the focused D5 and loads Dungeon_05");
            yield return SceneManager.LoadSceneAsync("WorldMap");
            yield return null;
            map = FindFirstObjectByType<WorldMapController>();
            Check(map.FocusedIndex == 4, "Returning from dungeon focuses newest unlocked D5");
            map.BackButton.onClick.Invoke();
            yield return null; yield return null;
            selection = FindFirstObjectByType<CharacterSelectionController>();
            Check(selection != null, "Back button returns to Character Select");
            selection.StartButton.onClick.Invoke();
            yield return null; yield return null;
            map = FindFirstObjectByType<WorldMapController>();
            Check(map != null && map.FocusedIndex == 4, "Character reselection returns to newest unlocked D5");
        }

        private void CheckWindow(WorldMapController map, int focus, int start, int count)
        {
            var visible = map.Cards.Where(c => c.gameObject.activeInHierarchy).ToArray();
            Check(map.FocusedIndex == focus && map.WindowStart == start && visible.Length == count &&
                visible.Select(c => c.BoundDungeon).SequenceEqual(map.DungeonCatalog.Dungeons.Skip(start).Take(count)),
                $"Catalog {map.DungeonCatalog.Count}: focus {focus}, window {start}, {count} visible");
            Check(map.SelectedDungeon == map.DungeonCatalog[focus] && map.Cards.Count(c => c.IsSelected) == 1 && map.Cards[focus].IsSelected,
                "Exactly one visible focused card controls selection");
            for (int i = 0; i < visible.Length; i++)
            {
                var rect = (RectTransform)visible[i].transform;
                Check(rect.sizeDelta == new Vector2(440, 450) && rect.anchoredPosition == new Vector2((i - (count - 1) * .5f) * 500, 20),
                    $"Slot {i}: fixed readable size and centered spacing");
            }
        }

        private void CheckLayout(WorldMapController map)
        {
            Canvas.ForceUpdateCanvases();
            var root = (RectTransform)map.transform;
            var scaler = map.GetComponent<CanvasScaler>();
            Check(scaler.referenceResolution == new Vector2(1920,1080), "Layout retains 1920x1080 design coordinates");
            var targets = map.Cards.Where(c => c.gameObject.activeInHierarchy).Select(c => (RectTransform)c.transform).Concat(new[] {
                (RectTransform)map.LeftNavigationButton.transform, (RectTransform)map.RightNavigationButton.transform,
                (RectTransform)map.EnterDungeonButton.transform, (RectTransform)map.BackButton.transform,
                (RectTransform)map.SkillTreeButton.transform, map.TitleText.rectTransform, map.ActiveHeroText.rectTransform }).ToArray();
            var rects = targets.Select(t => DesignBounds(root,t)).ToArray();
            var screen = new Rect(-960,-540,1920,1080);
            Check(rects.All(r => screen.Contains(r.min) && screen.Contains(r.max)), "All major controls inside 1280x720 scaled screen");
            for (int i = 0; i < rects.Length; ++i)
                for (int j = i + 1; j < rects.Length; ++j)
                    Check(!rects[i].Overlaps(rects[j]), $"No overlap: {targets[i].name} / {targets[j].name}");
            foreach (var card in map.Cards.Where(c => c.gameObject.activeInHierarchy))
            {
                foreach (var text in new[] {card.TitleText, card.DescriptionText, card.StatusText})
                {
                    text.ForceMeshUpdate();
                    Check(!text.enableAutoSizing && text.overflowMode == TextOverflowModes.Ellipsis && text.fontSize >= 22,
                        $"{card.BoundDungeon.Id}/{text.name}: fixed readable font and bounded overflow");
                    Check(DesignBounds((RectTransform)card.transform, text.rectTransform).min.x >= -220 &&
                        DesignBounds((RectTransform)card.transform, text.rectTransform).max.x <= 220, "Text stays within card width");
                }
                Check(card.TitleText.maxVisibleLines == 2 && card.DescriptionText.maxVisibleLines == 3, "Title and description line limits are 2 and 3");
            }
        }

        private static Rect DesignBounds(RectTransform root, RectTransform target)
        {
            var corners = new Vector3[4]; target.GetWorldCorners(corners);
            var min = (Vector2)root.InverseTransformPoint(corners[0]);
            var max = (Vector2)root.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
    }
}
#endif
