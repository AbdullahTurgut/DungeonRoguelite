using UnityEngine;
using UnityEngine.Rendering;

namespace DungeonRoguelite.Presentation
{
    internal static class ArrowFlightFeedback
    {
        // Called once on the spawned arrow. No root scaling, physics, or lifetime changes.
        public static void Apply(GameObject arrow, int tier)
        {
            Color color = tier == 2 ? new Color(0.65f, 0.45f, 1f) :
                tier == 1 ? new Color(0.15f, 0.85f, 1f) : new Color(0.8f, 0.85f, 0.9f, 0.55f);
            var trail = arrow.GetComponentInChildren<TrailRenderer>();
            if (trail == null) trail = arrow.AddComponent<TrailRenderer>();
            trail.sharedMaterial = AttackVfxStyle.Material;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.time = tier == 2 ? 0.095f : tier == 1 ? 0.07f : 0.025f;
            trail.minVertexDistance = 0.08f;
            trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            trail.widthMultiplier = tier == 2 ? 0.09f : tier == 1 ? 0.065f : 0.035f;
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            trail.Clear();

            var block = new MaterialPropertyBlock();
            foreach (var renderer in arrow.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.GetPropertyBlock(block);
                Color body = tier == 2 ? new Color(0.8f, 0.95f, 1f) :
                    tier == 1 ? new Color(0.35f, 0.85f, 1f) : new Color(0.75f, 0.72f, 0.6f);
                block.SetColor("_BaseColor", body);
                block.SetColor("_Color", body);
                renderer.SetPropertyBlock(block);
                block.Clear();
            }

            if (tier != 2) return;
            // A small cyan chevron gives the premium arrow a distinct silhouette.
            var accent = new GameObject("ArrowEnergyFletching");
            accent.transform.SetParent(arrow.transform, false);
            var line = accent.AddComponent<LineRenderer>();
            AttackVfxStyle.Configure(line, new Color(0.3f, 1f, 1f), 0.035f);
            line.useWorldSpace = false;
            line.positionCount = 3;
            line.SetPosition(0, new Vector3(-0.12f, 0f, -0.22f));
            line.SetPosition(1, new Vector3(0f, 0f, 0.08f));
            line.SetPosition(2, new Vector3(0.12f, 0f, -0.22f));
        }
    }
}
