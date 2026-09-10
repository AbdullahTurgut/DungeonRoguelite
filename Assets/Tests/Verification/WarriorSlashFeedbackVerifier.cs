#if UNITY_EDITOR
using System;
using System.Collections;
using DungeonRoguelite.Weapons;
using UnityEditor;
using UnityEngine;

namespace DungeonRoguelite.Tests
{
    /// <summary>Exercises the Warrior slash as an event-driven visual attached to the real MeleeWeapon execution path.</summary>
    public sealed class WarriorSlashFeedbackVerifier : MonoBehaviour
    {
        private int checks;
        private bool passed = true;
        private GameObject warrior;
        private float originalTimeScale;

        private IEnumerator Start()
        {
            originalTimeScale = Time.timeScale;
            var routine = Verify();
            while (true)
            {
                object current;
                try
                {
                    if (!routine.MoveNext()) break;
                    current = routine.Current;
                }
                catch (Exception exception)
                {
                    passed = false;
                    Debug.LogException(exception);
                    break;
                }

                yield return current;
            }

            (routine as IDisposable)?.Dispose();
            Cleanup();
            Debug.Log($"[WARRIOR SLASH FEEDBACK COMPLETE] {(passed ? "PASSED" : "FAILED")}: {checks} checks passed.");
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
            else EditorApplication.isPlaying = false;
        }

        private IEnumerator Verify()
        {
            var warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Warrior.prefab");
            var archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Archer.prefab");
            var gunnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Gunner.prefab");
            Check(warriorPrefab != null && warriorPrefab.GetComponent<WarriorSlashFeedback>() != null,
                "Warrior prefab owns the dedicated slash-feedback component");
            Check(archerPrefab != null && archerPrefab.GetComponentInChildren<WarriorSlashFeedback>(true) == null &&
                gunnerPrefab != null && gunnerPrefab.GetComponentInChildren<WarriorSlashFeedback>(true) == null,
                "Archer and Gunner prefabs remain unchanged");

            warrior = new GameObject("WarriorSlashFeedbackTest");
            warrior.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            var weapon = warrior.AddComponent<MeleeWeapon>();
            var feedback = warrior.AddComponent<WarriorSlashFeedback>();
            yield return null;

            Check(Mathf.Approximately(weapon.Damage, 25f) && Mathf.Approximately(weapon.AttackCooldown, .5f) &&
                Mathf.Approximately(weapon.Range, 2.5f) && Mathf.Approximately(weapon.ArcAngle, 120f),
                "Warrior combat values retain 25 damage, 0.5 cooldown, 2.5 range, and 120 degree arc");

            Check(weapon.TryAttack() && feedback.ActiveSlashCount == 1,
                "One successful real melee attack creates exactly one slash visual");
            LineRenderer firstSlash = FindSlashVisual();
            Vector3 visualMidpoint = firstSlash != null ? firstSlash.GetPosition(firstSlash.positionCount / 2) - warrior.transform.position : Vector3.zero;
            visualMidpoint.y = 0f;
            Check(firstSlash != null && visualMidpoint.sqrMagnitude > .001f &&
                Vector3.Dot(visualMidpoint.normalized, warrior.transform.forward) > .999f,
                "Slash arc midpoint is oriented in the real attack direction");

            yield return new WaitForSecondsRealtime(feedback.Lifetime + .03f);
            yield return null;
            Check(feedback.ActiveSlashCount == 0 && FindSlashVisual() == null,
                "Slash visual is removed within its bounded 0.12 second lifetime");

            yield return new WaitForSecondsRealtime(.37f);
            Check(weapon.TryAttack() && feedback.ActiveSlashCount == 1,
                "Repeated successful attacks create a fresh slash without retaining stale visuals");
            yield return new WaitForSecondsRealtime(feedback.Lifetime + .03f);
            yield return null;
            Check(feedback.ActiveSlashCount == 0 && FindSlashVisual() == null &&
                Mathf.Approximately(weapon.Damage, 25f) && Mathf.Approximately(weapon.AttackCooldown, .5f) &&
                Mathf.Approximately(weapon.Range, 2.5f) && Mathf.Approximately(weapon.ArcAngle, 120f),
                "Feedback cleanup leaves combat values unchanged");
        }

        private static LineRenderer FindSlashVisual()
        {
            var lines = FindObjectsByType<LineRenderer>(FindObjectsSortMode.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != null && lines[i].gameObject.name == "WarriorSlashVisual") return lines[i];
            }

            return null;
        }

        private void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException("[WARRIOR SLASH FEEDBACK CHECK FAILED] " + description);
            checks++;
            Debug.Log("[WARRIOR SLASH FEEDBACK CHECK PASSED] " + description);
        }

        private void Cleanup()
        {
            Time.timeScale = originalTimeScale;
            if (warrior != null) Destroy(warrior);
            Debug.Log("[WARRIOR SLASH FEEDBACK STATE RESTORED]");
        }
    }
}
#endif
