using DungeonRoguelite.Enemies;
using UnityEngine;
using UnityEngine.Rendering;

namespace DungeonRoguelite.Presentation
{
    // Rendering helper owned by the existing boss presentation, with no combat queries or timers.
    internal sealed class WardenEmberFeedback
    {
        private static readonly Color Ember = new Color(1f, .32f, .065f, .8f);
        private readonly ParticleSystem embers;
        private readonly LineRenderer shock;
        private string charge;
        private float chargeAge;
        private float nextEmber;
        private float shockEndsAt;
        private const float ShockLifetime = .18f;

        public WardenEmberFeedback(Transform owner)
        {
            var particles = new GameObject("WardenEmbers");
            particles.transform.SetParent(owner, false);
            embers = particles.AddComponent<ParticleSystem>();
            embers.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = embers.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.useUnscaledTime = true;
            main.maxParticles = 24;
            main.startLifetime = .25f;
            main.startSpeed = 0f;
            main.startSize = .07f;
            var emission = embers.emission;
            emission.enabled = false;
            var shape = embers.shape;
            shape.enabled = false;
            var size = embers.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            var fade = embers.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(.3f, .18f, .1f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = AttackVfxStyle.Material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var ring = new GameObject("WardenImpactAccent");
            ring.transform.SetParent(owner, false);
            shock = ring.AddComponent<LineRenderer>();
            AttackVfxStyle.Configure(shock, Ember, .09f);
            shock.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
            shock.numCapVertices = 0;
            shock.useWorldSpace = true;
            shock.enabled = false;
        }

        public void BeginCharge(string attack)
        {
            charge = attack;
            chargeAge = 0f;
            nextEmber = 0f;
        }

        public void EndCharge() => charge = null;

        public void Tick(Vector3 anchor, bool telegraphing)
        {
            if (!telegraphing) EndCharge();
            if (charge != null && Time.timeScale > 0f)
            {
                chargeAge += Time.deltaTime;
                if (charge != "Strike" && chargeAge >= nextEmber)
                {
                    // Small body/muzzle cue, never a second danger boundary.
                    Burst(anchor, 2, .2f + Mathf.Min(chargeAge, 1f) * .2f);
                    nextEmber = chargeAge + .12f;
                }
            }
            if (!shock.enabled) return;
            float remaining = (shockEndsAt - Time.unscaledTime) / ShockLifetime;
            if (remaining <= 0f) { shock.enabled = false; return; }
            var color = Ember;
            color.a = .55f * remaining;
            shock.startColor = shock.endColor = color;
        }

        public void Impact(Vector3 origin, Vector3 direction, float radius, float arc)
        {
            EndCharge();
            bool slam = arc >= 360f;
            float width = slam ? .09f : .12f;
            shock.widthMultiplier = width;
            // Fixed full-radius shock, with outer stroke edge at the authoritative radius.
            // No expanding ring, root scale, or inferred danger area.
            float centerRadius = Mathf.Max(0f, radius - width * .5f);
            shock.positionCount = 65;
            for (int i = 0; i <= 64; i++)
            {
                Vector3 radial = Quaternion.AngleAxis(Mathf.Lerp(-arc * .5f, arc * .5f, i / 64f), Vector3.up) * direction;
                shock.SetPosition(i, origin + Vector3.up * .08f + radial * centerRadius);
            }
            shock.startColor = shock.endColor = new Color(1f, .32f, .065f, .55f);
            shock.enabled = true;
            shockEndsAt = Time.unscaledTime + ShockLifetime;
            Burst(origin + Vector3.up * .25f + (slam ? Vector3.zero : direction * radius * .55f), slam ? 12 : 5);
        }

        public void Burst(Vector3 position, int count, float speed = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = i * 2.399963f;
                var particle = new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = new Vector3(Mathf.Cos(angle), .6f + (i % 3) * .2f, Mathf.Sin(angle)) * speed,
                    startColor = i % 3 == 0 ? new Color(.3f, .25f, .22f, .7f) : Ember,
                    startSize = i % 3 == 0 ? .1f : .055f,
                    startLifetime = .22f + (i % 3) * .035f
                };
                embers.Emit(particle, 1);
            }
        }

        public void Clear()
        {
            EndCharge();
            shock.enabled = false;
            embers.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public static void ApplyBolt(EnemyProjectile shot)
        {
            // Instance-only visuals: shared prefab, transform, sweep and projectile code untouched.
            var block = new MaterialPropertyBlock();
            foreach (var renderer in shot.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", new Color(1f, .28f, .055f));
                block.SetColor("_Color", new Color(1f, .28f, .055f));
                renderer.SetPropertyBlock(block);
                block.Clear();
            }
            var trail = shot.gameObject.AddComponent<TrailRenderer>();
            trail.sharedMaterial = AttackVfxStyle.Material;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.time = .09f;
            trail.minVertexDistance = .08f;
            trail.widthMultiplier = .12f;
            trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            trail.startColor = Ember;
            trail.endColor = new Color(.2f, .16f, .14f, 0f);
            trail.Clear();
        }
    }
}
