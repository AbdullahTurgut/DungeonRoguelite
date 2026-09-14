using System;
using DungeonRoguelite.Combat;
using UnityEngine;

namespace DungeonRoguelite.Enemies
{
    /// <summary>Local static-geometry steering only; the caller owns movement and attack locks.</summary>
    [Serializable]
    public sealed class EnemyObstacleAvoidance
    {
        [SerializeField] private LayerMask obstacleLayers = Physics.DefaultRaycastLayers;
        [SerializeField, Min(.2f)] private float probeDistance = 1.3f;
        [SerializeField, Min(.1f)] private float sideHoldTime = .8f;
        [SerializeField, Min(.05f)] private float clearDelay = .2f;
        [SerializeField, Min(30f)] private float steeringDegreesPerSecond = 240f;

        [NonSerialized] private RaycastHit[] hits;
        [NonSerialized] private int side;
        [NonSerialized] private float holdRemaining;
        [NonSerialized] private float clearTime;
        [NonSerialized] private Vector3 direction;
        [NonSerialized] private Vector3 bypassDirection;

        public void Reset()
        {
            side = 0;
            holdRemaining = clearTime = 0f;
            direction = bypassDirection = Vector3.zero;
        }

        public Vector3 Steer(CharacterController controller, Vector3 pursuit, float targetDistance, float speed, float dt)
        {
            var scale = controller.transform.lossyScale;
            float radius = Mathf.Max(.05f, controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)) * .95f);
            Vector3 origin = controller.transform.TransformPoint(controller.center);
            float lookAhead = Mathf.Min(targetDistance, Mathf.Max(probeDistance, speed * .3f));
            float forwardClearance = Clearance(controller, origin, radius, pursuit, lookAhead, out var normal);
            bool blocked = forwardClearance < lookAhead;
            holdRemaining = Mathf.Max(0f, holdRemaining - dt);
            Vector3 desired = pursuit;

            if (blocked)
            {
                clearTime = 0f;
                // Follow the obstacle surface, with a little outward clearance and forward progress.
                Vector3 tangent = Vector3.Cross(Vector3.up, normal).normalized;
                Vector3 left = (tangent + normal * .3f + pursuit * .2f).normalized;
                Vector3 right = (-tangent + normal * .3f + pursuit * .2f).normalized;
                float leftClear = Clearance(controller, origin, radius, left, lookAhead, out _);
                float rightClear = Clearance(controller, origin, radius, right, lookAhead, out _);
                float chosenClear = side > 0 ? leftClear : rightClear;
                float otherClear = side > 0 ? rightClear : leftClear;
                // A side can change only after the hold expires AND the current way is obstructed.
                if (side == 0 || (holdRemaining <= 0f && chosenClear < .2f && otherClear > chosenClear + .25f))
                {
                    if (Mathf.Abs(leftClear - rightClear) < .05f)
                        side = (controller.GetInstanceID() & 1) == 0 ? 1 : -1;
                    else
                        side = leftClear > rightClear ? 1 : -1;
                    holdRemaining = sideHoldTime;
                }
                bypassDirection = side > 0 ? left : right;
                desired = bypassDirection;
                if (leftClear < .15f && rightClear < .15f)
                    return Vector3.zero; // Wait/re-evaluate a closed corner, without alternating sides.
            }
            else if (side != 0)
            {
                clearTime += dt;
                if (clearTime >= clearDelay)
                    side = 0;
                else
                    desired = bypassDirection;
            }

            if (direction.sqrMagnitude < .001f) direction = pursuit;
            direction = Vector3.RotateTowards(direction, desired, steeringDegreesPerSecond * Mathf.Deg2Rad * dt, 0f).normalized;
            // Never smooth a turn through the obstacle. The CharacterController remains the final collision authority.
            float step = Mathf.Max(.01f, speed * dt);
            float clearance = Clearance(controller, origin, radius, direction, step + .05f, out _);
            float fraction = Mathf.Clamp01((clearance - .03f) / step);
            return direction * fraction;
        }

        private float Clearance(CharacterController owner, Vector3 origin, float radius, Vector3 heading, float distance, out Vector3 normal)
        {
            if (hits == null) hits = new RaycastHit[24];
            int count = Physics.SphereCastNonAlloc(origin, radius, heading, hits, distance, obstacleLayers, QueryTriggerInteraction.Ignore);
            var results = hits;
            // A dense crowd must not hide the actual wall just because the non-alloc buffer filled.
            if (count == hits.Length)
            {
                results = Physics.SphereCastAll(origin, radius, heading, distance, obstacleLayers, QueryTriggerInteraction.Ignore);
                count = results.Length;
            }
            float nearest = distance;
            normal = -heading;
            for (int i = 0; i < count; i++)
            {
                var hit = results[i];
                var collider = hit.collider;
                if (collider == null || hit.distance >= nearest || Mathf.Abs(hit.normal.y) > .65f ||
                    collider.attachedRigidbody != null || collider.transform.IsChildOf(owner.transform) ||
                    collider.GetComponentInParent<CharacterController>() != null ||
                    collider.GetComponentInParent<IDamageable>() != null ||
                    collider.GetComponentInParent<EnemyProjectile>() != null ||
                    collider.GetComponentInParent<DungeonRoguelite.Weapons.ArrowProjectile>() != null)
                    continue;
                nearest = hit.distance;
                normal = Vector3.ProjectOnPlane(hit.normal, Vector3.up).normalized;
            }
            return nearest;
        }
    }
}
