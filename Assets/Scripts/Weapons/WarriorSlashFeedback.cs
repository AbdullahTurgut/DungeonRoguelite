using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonRoguelite.Presentation;

namespace DungeonRoguelite.Weapons
{
    /// <summary>
    /// Creates a short-lived, replaceable visual arc after the Warrior's authoritative melee swing.
    /// This component has no combat, targeting, or timing responsibilities.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeleeWeapon))]
    public sealed class WarriorSlashFeedback : MonoBehaviour
    {
        private const string SlashVisualName = "WarriorSlashVisual";

        [Header("Slash Presentation")]
        [SerializeField, Range(0.10f, 0.15f)] private float lifetime = 0.12f;
        [SerializeField, Range(0.5f, 1f)] private float radiusFraction = 0.72f;
        [SerializeField, Range(0.02f, 0.2f)] private float lineWidth = 0.11f;

        private readonly List<GameObject> activeVisuals = new List<GameObject>();
        private MeleeWeapon meleeWeapon;
        private bool subscribed;

        public float Lifetime => lifetime;
        public int ActiveSlashCount
        {
            get
            {
                activeVisuals.RemoveAll(visual => visual == null);
                return activeVisuals.Count;
            }
        }

        private void Awake()
        {
            ResolveWeapon();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopAllCoroutines();
            ClearVisuals();
        }

        private void ResolveWeapon()
        {
            if (meleeWeapon == null)
            {
                meleeWeapon = GetComponent<MeleeWeapon>();
            }
        }

        private void Subscribe()
        {
            ResolveWeapon();
            if (meleeWeapon == null || subscribed)
            {
                return;
            }

            meleeWeapon.OnAttack += CreateSlashFromAttack;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || meleeWeapon == null)
            {
                return;
            }

            meleeWeapon.OnAttack -= CreateSlashFromAttack;
            subscribed = false;
        }

        private void CreateSlashFromAttack()
        {
            Transform owner = meleeWeapon != null ? meleeWeapon.transform.root : transform.root;
            Vector3 forward = owner.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            float range = meleeWeapon != null ? meleeWeapon.Range : 2.5f;
            float arcAngle = meleeWeapon != null ? meleeWeapon.ArcAngle : 120f;
            CreateSlash(owner.position, forward, range, arcAngle);
        }

        private void CreateSlash(Vector3 origin, Vector3 forward, float range, float arcAngle)
        {
            var visual = new GameObject(SlashVisualName);
            int tier = AttackVfxStyle.EquippedTier(this);
            Color color = tier == 2 ? new Color(1f, 0.8f, 0.3f) :
                tier == 1 ? new Color(1f, 0.32f, 0.06f) : new Color(0.88f, 0.94f, 1f, 0.85f);
            float radius = Mathf.Max(0.25f, range * radiusFraction);
            Vector3 center = origin + Vector3.up * 0.72f;
            var line = visual.AddComponent<LineRenderer>();
            BuildArc(line, center, forward, radius, arcAngle, color,
                lineWidth * (tier == 2 ? 1.65f : tier == 1 ? 1.3f : 0.75f));
            LineRenderer inner = null;
            if (tier == 2)
            {
                var core = new GameObject("WhiteHotCrescent");
                core.transform.SetParent(visual.transform, false);
                inner = core.AddComponent<LineRenderer>();
                BuildArc(inner, center, forward, radius - 0.18f, arcAngle * 0.82f,
                    new Color(1f, 0.97f, 0.8f), lineWidth * 0.55f);
            }
            activeVisuals.Add(visual);
            StartCoroutine(FadeVisual(visual, line, inner));
        }

        private static void BuildArc(LineRenderer line, Vector3 center, Vector3 forward,
            float radius, float angle, Color color, float width)
        {
            AttackVfxStyle.Configure(line, color, width);
            line.useWorldSpace = true;
            line.widthCurve = new AnimationCurve(new Keyframe(0f, 0.05f),
                new Keyframe(0.35f, 1f), new Keyframe(0.75f, 0.65f), new Keyframe(1f, 0.02f));
            const int segments = 24;
            line.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(
                    Mathf.Lerp(-angle * 0.5f, angle * 0.5f, i / (float)segments), Vector3.up) * forward;
                line.SetPosition(i, center + direction * radius);
            }
        }

        private IEnumerator FadeVisual(GameObject visual, LineRenderer line, LineRenderer inner)
        {
            Color color = line.startColor;
            float elapsed = 0f;
            while (elapsed < lifetime && visual != null)
            {
                float alpha = 1f - elapsed / lifetime;
                line.startColor = new Color(color.r, color.g, color.b, color.a * alpha);
                if (inner != null) inner.startColor = new Color(1f, 0.97f, 0.8f, alpha);
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            activeVisuals.Remove(visual);
            if (visual != null) Destroy(visual);
        }

        private void ClearVisuals()
        {
            for (int i = 0; i < activeVisuals.Count; i++)
            {
                if (activeVisuals[i] != null)
                {
                    Destroy(activeVisuals[i]);
                }
            }

            activeVisuals.Clear();
        }
    }
}
