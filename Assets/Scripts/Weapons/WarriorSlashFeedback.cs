using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private Color slashColor = new Color(1f, 0.84f, 0.24f, 1f);

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
            var line = visual.AddComponent<LineRenderer>();
            ConfigureLine(line);

            const int segments = 12;
            line.positionCount = segments + 1;
            float radius = Mathf.Max(0.25f, range * radiusFraction);
            float halfArc = arcAngle * 0.5f;
            Vector3 center = origin + Vector3.up * 0.72f;

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(-halfArc, halfArc, t);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                line.SetPosition(i, center + direction * radius);
            }

            activeVisuals.Add(visual);
            StartCoroutine(DestroyVisualAfterLifetime(visual));
        }

        private void ConfigureLine(LineRenderer line)
        {
            line.useWorldSpace = true;
            line.alignment = LineAlignment.View;
            line.positionCount = 0;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.startColor = slashColor;
            line.endColor = slashColor;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            // Reuse the Warrior's existing material. No art asset, material, or shader is created for this prototype.
            Renderer sourceRenderer = GetComponentInChildren<Renderer>();
            if (sourceRenderer != null && sourceRenderer.sharedMaterial != null)
            {
                line.sharedMaterial = sourceRenderer.sharedMaterial;
            }
        }

        private IEnumerator DestroyVisualAfterLifetime(GameObject visual)
        {
            yield return new WaitForSecondsRealtime(lifetime);
            activeVisuals.Remove(visual);
            if (visual != null)
            {
                Destroy(visual);
            }
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
