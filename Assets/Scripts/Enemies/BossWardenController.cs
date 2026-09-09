using System;
using System.Collections;
using UnityEngine;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.Enemies
{
    public enum BossWardenState { Engage, Chase, Telegraph, Execute, Recovery, Dead }

    /// <summary>Focused first-boss controller. It deliberately reuses EnemyHealth and EnemyProjectile.</summary>
    [RequireComponent(typeof(EnemyHealth), typeof(CharacterController))]
    public sealed class BossWardenController : MonoBehaviour, IEnemyAttack
    {
        [Header("Identity")]
        [SerializeField] private string bossName = "Kül Muhafızı";
        [Header("Movement")]
        [SerializeField] private float phaseOneMoveSpeed = 2.6f;
        [SerializeField] private float phaseTwoMoveSpeed = 3.1f;
        [SerializeField] private float engagementRange = 16f;
        [SerializeField] private float meleeRange = 3.4f;
        [Header("Attacks")]
        [SerializeField] private float strikeDamage = 20f;
        [SerializeField] private float slamDamage = 32f;
        [SerializeField] private float boltDamage = 16f;
        [SerializeField] private float strikeCooldown = 1.6f;
        [SerializeField] private float slamCooldown = 5f;
        [SerializeField] private float boltCooldown = 3.5f;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform telegraph;

        private EnemyHealth health;
        private CharacterController controller;
        private Transform target;
        private PlayerHealth playerHealth;
        private float nextStrike;
        private float nextSlam;
        private float nextBolt;
        private bool phaseTwo;
        private bool acting;
        private float baseStrike;
        private float baseSlam;
        private float baseBolt;

        public string BossName => bossName;
        public BossWardenState State { get; private set; } = BossWardenState.Engage;
        public bool IsPhaseTwo => phaseTwo;
        public float StrikeDamage => strikeDamage;
        public float SlamDamage => slamDamage;
        public float BoltDamage => boltDamage;
        public event Action<bool> OnPhaseChanged;
        public event Action<string> OnTelegraphStarted;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            controller = GetComponent<CharacterController>();
            baseStrike = strikeDamage; baseSlam = slamDamage; baseBolt = boltDamage;
        }

        private void OnEnable() { health.OnDied += HandleDeath; }
        private void OnDisable() { health.OnDied -= HandleDeath; StopAllCoroutines(); SetTelegraph(false, Color.white); }
        private void Update()
        {
            if (health.IsDead || Time.timeScale <= 0f || acting) return;
            if (target == null) return;
            if (!phaseTwo && health.HealthNormalized <= .5f)
            {
                phaseTwo = true;
                OnPhaseChanged?.Invoke(true);
            }
            var delta = target.position - transform.position; delta.y = 0f;
            float distance = delta.magnitude;
            if (distance > engagementRange) { Move(delta); return; }
            if (distance <= meleeRange && Time.time >= nextSlam)
                StartCoroutine(SlamRoutine());
            else if (distance <= meleeRange && Time.time >= nextStrike)
                StartCoroutine(StrikeRoutine());
            else if (distance <= engagementRange && Time.time >= nextBolt)
                StartCoroutine(BoltRoutine());
            else Move(delta);
        }

        private void Move(Vector3 delta)
        {
            State = BossWardenState.Chase;
            if (delta.sqrMagnitude < .01f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(delta.normalized, Vector3.up), 540f * Time.deltaTime);
            controller.Move(delta.normalized * (phaseTwo ? phaseTwoMoveSpeed : phaseOneMoveSpeed) * Time.deltaTime + Vector3.down * 2f * Time.deltaTime);
        }

        private IEnumerator StrikeRoutine()
        {
            acting = true; State = BossWardenState.Telegraph; OnTelegraphStarted?.Invoke("Strike"); SetTelegraph(true, new Color(1f, .2f, .1f));
            yield return new WaitForSeconds(.45f); State = BossWardenState.Execute; SetTelegraph(false, Color.white);
            DealIfInRange(meleeRange, strikeDamage); nextStrike = Time.time + strikeCooldown;
            State = BossWardenState.Recovery; yield return new WaitForSeconds(.8f); acting = false; State = BossWardenState.Engage;
        }

        private IEnumerator SlamRoutine()
        {
            acting = true; State = BossWardenState.Telegraph; OnTelegraphStarted?.Invoke("Slam"); SetTelegraph(true, new Color(1f, .55f, .05f));
            yield return new WaitForSeconds(1.1f); State = BossWardenState.Execute; SetTelegraph(false, Color.white);
            DealIfInRange(6.5f, slamDamage); nextSlam = Time.time + (phaseTwo ? 3.5f : slamCooldown);
            State = BossWardenState.Recovery; yield return new WaitForSeconds(1f); acting = false; State = BossWardenState.Engage;
        }

        private IEnumerator BoltRoutine()
        {
            acting = true; State = BossWardenState.Telegraph; OnTelegraphStarted?.Invoke("Bolt"); SetTelegraph(true, new Color(.8f, .1f, 1f));
            yield return new WaitForSeconds(.7f); State = BossWardenState.Execute; SetTelegraph(false, Color.white); FireBolt();
            if (phaseTwo) { yield return new WaitForSeconds(.18f); if (!health.IsDead) FireBolt(); }
            nextBolt = Time.time + (phaseTwo ? 2.6f : boltCooldown);
            State = BossWardenState.Recovery; yield return new WaitForSeconds(.5f); acting = false; State = BossWardenState.Engage;
        }

        private void DealIfInRange(float range, float damage)
        {
            if (playerHealth == null || playerHealth.IsDead || target == null) return;
            Vector3 delta = target.position - transform.position; delta.y = 0f;
            if (delta.sqrMagnitude <= range * range) playerHealth.TakeDamage(damage);
        }
        private void FireBolt()
        {
            if (projectilePrefab == null || target == null) return;
            Vector3 direction = target.position - transform.position; direction.y = 0f;
            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.5f;
            var shot = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction.normalized, Vector3.up));
            shot.Initialize(transform, direction, 14f, boltDamage, 3f);
        }
        private void SetTelegraph(bool visible, Color color)
        {
            if (telegraph == null) return;
            telegraph.gameObject.SetActive(visible);
            var renderer = telegraph.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = color;
        }
        private void HandleDeath() { State = BossWardenState.Dead; acting = false; SetTelegraph(false, Color.white); }
        public void SetTarget(Transform newTarget) { target = newTarget; playerHealth = target != null ? target.GetComponentInParent<PlayerHealth>() : null; }
        public void InitializeAttack(float multiplier)
        {
            // Unity invokes Awake before WaveManager spawning in play mode. The lazy
            // fallback also keeps explicit editor/runtime instantiation deterministic.
            if (baseStrike <= 0f) { baseStrike = strikeDamage; baseSlam = slamDamage; baseBolt = boltDamage; }
            float m = multiplier > 0f ? multiplier : 1f;
            strikeDamage = Mathf.Max(1f, Mathf.Round(baseStrike * m)); slamDamage = Mathf.Max(1f, Mathf.Round(baseSlam * m)); boltDamage = Mathf.Max(1f, Mathf.Round(baseBolt * m));
        }
    }
}
