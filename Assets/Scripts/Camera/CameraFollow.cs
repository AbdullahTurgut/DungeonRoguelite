using UnityEngine;

namespace DungeonRoguelite.CameraControl
{
    /// <summary>
    /// Smoothly follows a target Transform from a fixed angled top-down perspective using Vector3.SmoothDamp in LateUpdate.
    /// Does not influence or alter player movement or aiming calculations.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Positioning")]
        [Tooltip("The target Transform to follow (e.g. the Player).")]
        [SerializeField] private Transform target;

        [Tooltip("Positional offset relative to the target.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 16f, -11f);

        [Tooltip("Fixed top-down pitch and orientation angles.")]
        [SerializeField] private Vector3 fixedRotationEuler = new Vector3(55f, 0f, 0f);

        [Header("Smooth Damping")]
        [Tooltip("Approximate time it takes to reach the target position.")]
        [SerializeField] private float smoothTime = 0.15f;

        private Vector3 currentVelocity;

        public Transform Target => target;
        public Vector3 Offset => offset;
        public float SmoothTime => smoothTime;
        public Vector3 CurrentVelocity => currentVelocity;

        private void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            // Immediately set orientation and snap position to prevent start frame jump
            transform.rotation = Quaternion.Euler(fixedRotationEuler);
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

            // Lock camera rotation strictly to the configured angled top-down orientation
            transform.rotation = Quaternion.Euler(fixedRotationEuler);
        }

        /// <summary>
        /// Instantly snaps the camera to the target's current position plus offset without smoothing.
        /// </summary>
        public void SnapToTarget()
        {
            if (target != null)
            {
                transform.position = target.position + offset;
                currentVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Sets a new target to follow.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
