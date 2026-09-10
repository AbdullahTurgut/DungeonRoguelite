using System;
using System.Collections;
using DungeonRoguelite.Player;
using UnityEngine;

namespace DungeonRoguelite.Enemies
{
    /// <summary>Second boss: locked-direction cleave and collision-bounded dash, then a shard fan.</summary>
    [RequireComponent(typeof(EnemyHealth),typeof(CharacterController))]
    public sealed class HollowCastellanController : MonoBehaviour, IEnemyAttack, IBossPresentation
    {
        [SerializeField] private string bossName = "Boş Kalenin Muhafızı";
        [SerializeField] private float phaseOneSpeed = 3f;
        [SerializeField] private float phaseTwoSpeed = 3.4f;
        [SerializeField] private float cleaveDamage = 24f;
        [SerializeField] private float dashDamage = 32f;
        [SerializeField] private float shardDamage = 12f;
        [SerializeField] private float cleaveRange = 4.2f;
        [SerializeField] private float dashRange = 16f;
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private LineRenderer telegraph;
        private EnemyHealth health;
        private CharacterController body;
        private Transform target;
        private PlayerHealth playerHealth;
        private float baseCleave,baseDash,baseShard,nextAttack,nextDash,nextFan;
        private bool acting;
        public string BossName => bossName;
        public bool IsPhaseTwo { get; private set; }
        public event Action<string> OnAttackExecuted;

        private void Awake()
        {
            health=GetComponent<EnemyHealth>();body=GetComponent<CharacterController>();
            baseCleave=cleaveDamage;baseDash=dashDamage;baseShard=shardDamage;
        }
        private void OnEnable() { health.OnDied+=Die; health.OnHealthChanged+=HealthChanged; }
        private void OnDisable() { health.OnDied-=Die;health.OnHealthChanged-=HealthChanged;StopAllCoroutines();acting=false;HideTelegraph(); }
        private void HealthChanged(float current,float max) { if(max>0 && current<=max*.5f)IsPhaseTwo=true; }
        private void Update()
        {
            if(health.IsDead || target==null || acting || Time.timeScale<=0)return;
            var delta=target.position-transform.position;delta.y=0;
            float distance=delta.magnitude;
            if(Time.time>=nextAttack)
            {
                if(IsPhaseTwo && Time.time>=nextFan) { StartCoroutine(Attack("Fan"));return; }
                if(distance<=cleaveRange) { StartCoroutine(Attack("Cleave"));return; }
                if(distance<=dashRange && Time.time>=nextDash) { StartCoroutine(Attack("Dash"));return; }
            }
            if(delta.sqrMagnitude>.01f)
            {
                transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(delta),360*Time.deltaTime);
                body.Move(delta.normalized*(IsPhaseTwo?phaseTwoSpeed:phaseOneSpeed)*Time.deltaTime+Vector3.down*2*Time.deltaTime);
            }
        }
        private IEnumerator Attack(string attack)
        {
            acting=true;
            Vector3 origin=transform.position;
            Vector3 direction=target.position-origin;direction.y=0;
            if(direction.sqrMagnitude<.001f)direction=transform.forward;
            direction.Normalize();transform.rotation=Quaternion.LookRotation(direction);
            // The target direction locks when the warning appears, giving the player time to evade.
            ShowTelegraph(attack,origin,direction);
            yield return new WaitForSeconds(attack=="Dash"?.85f:attack=="Cleave"?.7f:.9f);
            if(health.IsDead)yield break;
            HideTelegraph();
            if(attack=="Cleave")
            {
                var delta=target!=null?target.position-transform.position:Vector3.one*1000;delta.y=0;
                if(delta.magnitude<=cleaveRange && Vector3.Angle(direction,delta)<=60 && !BlockedToPlayer())Damage(cleaveDamage);
            }
            else if(attack=="Dash")
            {
                float travel=0;bool hit=false;
                while(travel<dashRange && !health.IsDead)
                {
                    Vector3 before=transform.position;
                    float step=Mathf.Min(dashSpeed*Time.deltaTime,dashRange-travel);
                    var flags=body.Move(direction*step+Vector3.down*2*Time.deltaTime);
                    Vector3 after=transform.position;
                    if(!hit && target!=null && DistanceToSegment(target.position,before,after)<=body.radius+.5f && !BlockedToPlayer()) { Damage(dashDamage);hit=true; }
                    travel+=step;
                    if((flags&CollisionFlags.Sides)!=0)break;
                    yield return null;
                }
                nextDash=Time.time+(IsPhaseTwo?2.2f:4f);
            }
            else
            {
                for(int i=-1;i<=1;i++)
                {
                    Vector3 ray=Quaternion.AngleAxis(i*18,Vector3.up)*direction;
                    var shot=Instantiate(projectilePrefab,muzzle.position,Quaternion.LookRotation(ray));
                    shot.Initialize(transform,ray,15,shardDamage,3);
                }
                nextFan=Time.time+5f;
            }
            OnAttackExecuted?.Invoke(attack);
            yield return new WaitForSeconds(IsPhaseTwo?.5f:.8f);
            nextAttack=Time.time+(IsPhaseTwo?.35f:.6f);acting=false;
        }
        private bool BlockedToPlayer()
        {
            if(target==null)return true;
            Vector3 from=transform.position+Vector3.up;Vector3 delta=target.position+Vector3.up-from;
            foreach(var hit in Physics.RaycastAll(from,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
                if(!hit.transform.IsChildOf(transform) && hit.transform.GetComponentInParent<PlayerHealth>()==null)return true;
            return false;
        }
        private static float DistanceToSegment(Vector3 point,Vector3 a,Vector3 b)
        {
            point.y=a.y=b.y=0;var ab=b-a;
            return Vector3.Distance(point,a+ab*(ab.sqrMagnitude>0?Mathf.Clamp01(Vector3.Dot(point-a,ab)/ab.sqrMagnitude):0));
        }
        private void Damage(float amount) { if(playerHealth!=null && !playerHealth.IsDead)playerHealth.TakeDamage(amount); }
        private void ShowTelegraph(string attack,Vector3 origin,Vector3 direction)
        {
            if(telegraph==null)return;
            telegraph.gameObject.SetActive(true);telegraph.useWorldSpace=true;
            telegraph.startColor=telegraph.endColor=attack=="Fan"?new Color(.6f,.3f,1):attack=="Dash"?Color.cyan:new Color(1,.5f,.1f);
            if(attack=="Cleave")
            {
                telegraph.positionCount=25;
                for(int i=0;i<25;i++)telegraph.SetPosition(i,origin+Vector3.up*.08f+(Quaternion.AngleAxis(-60+5*i,Vector3.up)*direction)*cleaveRange);
            }
            else
            {
                telegraph.positionCount=attack=="Dash"?2:6;
                if(attack=="Dash") { telegraph.SetPosition(0,origin+Vector3.up*.08f);telegraph.SetPosition(1,origin+Vector3.up*.08f+direction*dashRange); }
                else for(int i=0;i<3;i++) { telegraph.SetPosition(i*2,origin+Vector3.up*.08f);telegraph.SetPosition(i*2+1,origin+Vector3.up*.08f+(Quaternion.AngleAxis((i-1)*18,Vector3.up)*direction)*9); }
            }
        }
        private void HideTelegraph() { if(telegraph!=null)telegraph.gameObject.SetActive(false); }
        private void Die() { StopAllCoroutines();acting=false;HideTelegraph(); }
        public void SetTarget(Transform value) { target=value;playerHealth=value!=null?value.GetComponentInParent<PlayerHealth>():null; }
        public void InitializeAttack(float multiplier)
        {
            if(baseCleave<=0) { baseCleave=cleaveDamage;baseDash=dashDamage;baseShard=shardDamage; }
            float m=multiplier>0?multiplier:1;
            cleaveDamage=Mathf.Round(baseCleave*m);dashDamage=Mathf.Round(baseDash*m);shardDamage=Mathf.Round(baseShard*m);
        }
    }
}
