using System;
using System.Collections;
using UnityEngine;
using DungeonRoguelite.Player;

namespace DungeonRoguelite.Enemies
{
    public enum BossWardenState { Engage, Chase, Telegraph, Execute, Recovery, Dead }

    /// <summary>Focused first-boss controller. It deliberately reuses EnemyHealth and EnemyProjectile.</summary>
    [RequireComponent(typeof(EnemyHealth), typeof(CharacterController))]
    public sealed class BossWardenController : MonoBehaviour, IEnemyAttack, IBossPresentation
    {
        [Header("Identity")]
        [SerializeField] private string bossName = "Kül Muhafızı";
        [Header("Movement")]
        [SerializeField] private float phaseOneMoveSpeed = 2.6f;
        [SerializeField] private float phaseTwoMoveSpeed = 3.1f;
        [SerializeField] private float engagementRange = 16f;
        [SerializeField] private float meleeRange = 2.9f;
        [SerializeField] private float slamTriggerRange = 3.4f;
        [Header("Attacks")]
        [SerializeField] private float strikeDamage = 20f;
        [SerializeField] private float slamDamage = 32f;
        [SerializeField] private float boltDamage = 16f;
        [SerializeField] private float strikeCooldown = 2.1f;
        [SerializeField] private float strikeRecovery = .85f;
        [SerializeField, Range(1f,180f)] private float strikeArc = 140f;
        [SerializeField, Min(.01f)] private float strikeActiveDuration = .15f;
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
        private const float SlamRadius = 6.5f;
        private LineRenderer attackOutline;

        public string BossName => bossName;
        public BossWardenState State { get; private set; } = BossWardenState.Engage;
        public bool IsPhaseTwo => phaseTwo;
        public float StrikeDamage => strikeDamage;
        public float SlamDamage => slamDamage;
        public float BoltDamage => boltDamage;
        public event Action<bool> OnPhaseChanged;
        public event Action<string> OnTelegraphStarted;
        // Presentation notifications carry authoritative geometry; listeners never resolve attacks.
        public event Action<string, Vector3, Vector3, float, float> OnAttackPresented;
        public event Action<EnemyProjectile> OnBoltFired;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            controller = GetComponent<CharacterController>();
            baseStrike = strikeDamage; baseSlam = slamDamage; baseBolt = boltDamage;
        }

        private void OnEnable() { health.OnDied += HandleDeath; }
        private void OnDisable() { health.OnDied -= HandleDeath; StopAllCoroutines(); acting = false; HideTelegraph(); }
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
            if (distance <= slamTriggerRange && Time.time >= nextSlam)
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
            if (delta.sqrMagnitude <= meleeRange * meleeRange) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(delta.normalized, Vector3.up), 540f * Time.deltaTime);
            controller.Move(delta.normalized * (phaseTwo ? phaseTwoMoveSpeed : phaseOneMoveSpeed) * Time.deltaTime + Vector3.down * 2f * Time.deltaTime);
        }

        private IEnumerator StrikeRoutine()
        {
            acting = true;
            Vector3 origin = transform.position, direction = CommitDirection();
            State = BossWardenState.Telegraph;
            ShowTelegraph(origin, direction, meleeRange, strikeArc, new Color(1f,.7f,.1f));
            OnTelegraphStarted?.Invoke("Strike");
            yield return new WaitForSeconds(.45f);
            State = BossWardenState.Execute;
            ShowTelegraph(origin, direction, meleeRange, strikeArc, new Color(1f,.3f,.05f));
            nextStrike = Time.time + strikeCooldown;
            OnAttackPresented?.Invoke("Strike", origin, direction, meleeRange, strikeArc);
            float endsAt = Time.time + strikeActiveDuration;
            bool hit = false;
            while (Time.time < endsAt)
            {
                if (Time.timeScale > 0f && !hit && playerHealth != null && !playerHealth.IsDead && target != null)
                {
                    Vector3 delta = target.position - origin; delta.y = 0f;
                    if (delta.sqrMagnitude <= meleeRange * meleeRange && Vector3.Angle(direction,delta) <= strikeArc * .5f)
                    {
                        hit = true; playerHealth.TakeDamage(strikeDamage);
                    }
                }
                yield return null;
            }
            HideTelegraph();
            State = BossWardenState.Recovery; yield return new WaitForSeconds(strikeRecovery); acting = false; State = BossWardenState.Engage;
        }

        private IEnumerator SlamRoutine()
        {
            acting = true; Vector3 origin = transform.position;
            State = BossWardenState.Telegraph;
            ShowTelegraph(origin, transform.forward, SlamRadius, 360f, new Color(1f,.1f,.08f));
            OnTelegraphStarted?.Invoke("Slam");
            yield return new WaitForSeconds(1.1f); State = BossWardenState.Execute; HideTelegraph();
            DealIfInRange(origin, SlamRadius, slamDamage); nextSlam = Time.time + (phaseTwo ? 3.5f : slamCooldown);
            OnAttackPresented?.Invoke("Slam", origin, transform.forward, SlamRadius, 360f);
            State = BossWardenState.Recovery; yield return new WaitForSeconds(1f); acting = false; State = BossWardenState.Engage;
        }

        private IEnumerator BoltRoutine()
        {
            acting = true; Vector3 direction = CommitDirection();
            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.5f;
            State = BossWardenState.Telegraph;
            ShowTelegraph(new Vector3(origin.x,transform.position.y,origin.z), direction, engagementRange, 0f, new Color(.8f,.1f,1f));
            OnTelegraphStarted?.Invoke("Bolt");
            yield return new WaitForSeconds(.7f); State = BossWardenState.Execute; HideTelegraph(); FireBolt(origin,direction);
            if (phaseTwo) { yield return new WaitForSeconds(.18f); if (!health.IsDead) FireBolt(origin,direction); }
            nextBolt = Time.time + (phaseTwo ? 2.6f : boltCooldown);
            State = BossWardenState.Recovery; yield return new WaitForSeconds(.5f); acting = false; State = BossWardenState.Engage;
        }

        private Vector3 CommitDirection()
        {
            Vector3 direction = target != null ? target.position - transform.position : transform.forward;
            direction.y = 0f;
            direction = direction.sqrMagnitude > .001f ? direction.normalized : transform.forward;
            transform.rotation = Quaternion.LookRotation(direction,Vector3.up);
            return direction;
        }
        private void DealIfInRange(Vector3 origin, float range, float damage)
        {
            if (playerHealth == null || playerHealth.IsDead || target == null) return;
            Vector3 delta = target.position - origin; delta.y = 0f;
            if (delta.sqrMagnitude <= range * range) playerHealth.TakeDamage(damage);
        }
        private void FireBolt(Vector3 origin, Vector3 direction)
        {
            if (projectilePrefab == null || health.IsDead) return;
            var shot = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction.normalized, Vector3.up));
            shot.Initialize(transform, direction, 14f, boltDamage, 3f);
            OnBoltFired?.Invoke(shot);
        }
        private void ShowTelegraph(Vector3 origin, Vector3 direction, float range, float arc, Color color)
        {
            // World-space geometry avoids the boss prefab's scale changing the indicated radius.
            if (telegraph != null) telegraph.gameObject.SetActive(false);
            if (attackOutline == null)
            {
                var root = new GameObject("WardenAttackOutline"); root.transform.SetParent(transform,false);
                attackOutline = root.AddComponent<LineRenderer>(); attackOutline.useWorldSpace = true;
                attackOutline.widthMultiplier = .12f;
                attackOutline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                attackOutline.receiveShadows = false;
                var source = telegraph != null ? telegraph.GetComponent<Renderer>() : GetComponentInChildren<Renderer>();
                if (source != null) attackOutline.sharedMaterial = source.sharedMaterial;
            }
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor",color); block.SetColor("_Color",color);
            attackOutline.SetPropertyBlock(block); attackOutline.enabled = true;
            origin += Vector3.up * .1f;
            if (arc <= 0f)
            {
                attackOutline.positionCount=2; attackOutline.SetPosition(0,origin); attackOutline.SetPosition(1,origin+direction*range);
                return;
            }
            const int segments = 64;
            bool circle = arc >= 360f;
            attackOutline.positionCount = circle ? segments+1 : segments+3;
            if (!circle) attackOutline.SetPosition(0,origin);
            for (int i=0;i<=segments;i++)
                attackOutline.SetPosition(i+(circle?0:1),origin+Quaternion.AngleAxis(Mathf.Lerp(-arc/2,arc/2,i/(float)segments),Vector3.up)*direction*range);
            if (!circle) attackOutline.SetPosition(segments+2,origin);
        }
        private void HideTelegraph()
        {
            if (telegraph != null) telegraph.gameObject.SetActive(false);
            if (attackOutline != null) attackOutline.enabled = false;
        }
        private void HandleDeath() { StopAllCoroutines(); State = BossWardenState.Dead; acting = false; HideTelegraph(); }
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
