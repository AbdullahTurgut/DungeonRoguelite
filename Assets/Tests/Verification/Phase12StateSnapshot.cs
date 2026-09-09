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
    public sealed class Phase12StateSnapshot : IDisposable
    {
        private readonly List<Action> restore = new List<Action>();
        private readonly float timeScale = Time.timeScale;
        private readonly bool hadDungeonSave = PlayerPrefs.HasKey(DungeonProgression.PrefsKey);
        private readonly string dungeonSave = PlayerPrefs.GetString(DungeonProgression.PrefsKey, "");
        private readonly bool hadPermanentSave = PlayerPrefs.HasKey(PermanentProgression.PrefsKey);
        private readonly string permanentSave = PlayerPrefs.GetString(PermanentProgression.PrefsKey, "");
        private bool disposed;

        public Phase12StateSnapshot()
        {
            foreach (var type in new[] {
                typeof(CharacterSelectionSession),
                typeof(DungeonRunSession),
                typeof(RunProgressionSession),
                typeof(DungeonProgression),
                typeof(PermanentProgression)
            })
            {
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
                    else if (value is IDictionary dict)
                    {
                        var keys = new object[dict.Count];
                        var values = new object[dict.Count];
                        dict.Keys.CopyTo(keys, 0);
                        dict.Values.CopyTo(values, 0);
                        restore.Add(() =>
                        {
                            dict.Clear();
                            for (int i = 0; i < keys.Length; i++) dict.Add(keys[i], values[i]);
                        });
                    }
                    else
                    {
                        restore.Add(() => field.SetValue(null, value));
                    }
                }
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var action in restore) action();

            if (hadDungeonSave) PlayerPrefs.SetString(DungeonProgression.PrefsKey, dungeonSave);
            else PlayerPrefs.DeleteKey(DungeonProgression.PrefsKey);

            if (hadPermanentSave) PlayerPrefs.SetString(PermanentProgression.PrefsKey, permanentSave);
            else PlayerPrefs.DeleteKey(PermanentProgression.PrefsKey);

            PlayerPrefs.Save();
            Time.timeScale = timeScale;
            Debug.Log("[PHASE 12 STATE RESTORED]");
        }
    }
}
#endif
