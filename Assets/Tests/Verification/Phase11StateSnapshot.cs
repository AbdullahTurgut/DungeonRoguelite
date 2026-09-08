#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Progression;

namespace DungeonRoguelite.Tests
{
    // Test-only reflection preserves exact private session state, including checkpoint lists
    // and event delegates, without adding production APIs or invoking persistence events.
    public sealed class Phase11StateSnapshot : IDisposable
    {
        private readonly List<Action> restore = new List<Action>();
        private readonly float timeScale = Time.timeScale;
        private readonly bool hadSave = PlayerPrefs.HasKey(DungeonProgression.PrefsKey);
        private readonly string save = PlayerPrefs.GetString(DungeonProgression.PrefsKey, "");
        private bool disposed;

        public Phase11StateSnapshot()
        {
            foreach (var type in new[] { typeof(CharacterSelectionSession), typeof(DungeonRunSession),
                typeof(RunProgressionSession), typeof(DungeonProgression) })
            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (field.IsLiteral) continue;
                var value = field.GetValue(null);
                if (value is IList list)
                {
                    var copy = new object[list.Count];
                    list.CopyTo(copy, 0);
                    restore.Add(() => { list.Clear(); foreach (var item in copy) list.Add(item); });
                }
                else if (value is HashSet<string> set)
                {
                    var copy = new List<string>(set);
                    restore.Add(() => { set.Clear(); set.UnionWith(copy); });
                }
                else restore.Add(() => field.SetValue(null, value));
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var action in restore) action();
            if (hadSave) PlayerPrefs.SetString(DungeonProgression.PrefsKey, save);
            else PlayerPrefs.DeleteKey(DungeonProgression.PrefsKey);
            PlayerPrefs.Save();
            Time.timeScale = timeScale;
            Debug.Log("[PHASE 11 STATE RESTORED]");
        }
    }
}
#endif
