using System.Collections;
using UnityEngine;

public abstract class BossController : MonoBehaviour, IDamageable
{
    public FSM Fsm { get; private set; }
    public Animator Animator { get; private set; }
    public float PlayerDistance { get; private set; }
    public int CurrHp { get; protected set; }
    public bool IsAttack { get; set; }
    public DecideState DecideState { get; protected set; }
    [SerializeField] private Transform lookAtZone;
    public Transform LookAtZone => lookAtZone;

    [SerializeField] private BossData data;
    public BossData Data => data;

    [SerializeField] private AttackZone attackZone;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("── 피격 효과 ──")]
    [SerializeField] private float hitFlashDuration = 0.08f;
    [SerializeField] private float hitStopDuration = 0.04f;

    [Header("── 사망 연출 ──")]
    [SerializeField] private float deathStopDuration = 0.3f;
    [SerializeField] private float deathSlowScale = 0.2f;
    [SerializeField] private float deathSlowDuration = 1.0f;
    [SerializeField] private ParticleSystem deathParticle;  // 없어도 작동

    private Transform player;
    private int maxHp;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        DecideState = new DecideState(this);
        attackZone.Deactivate();
        OnAwake();
    }

    private void Start()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        maxHp = data.Hp;
        CurrHp = maxHp;
        Fsm = new FSM();
        InitStates();
    }

    protected virtual void OnAwake() { }

    protected virtual void Update()
    {
        PlayerDistance = Vector3.Distance(transform.position, player.position);
        if (!IsAttack)
        {
            transform.localScale = player.position.x < transform.position.x
                ? new Vector3(-1f, 1f, 1f)
                : new Vector3(1f, 1f, 1f);
        }

        Fsm.Update();
    }

    protected virtual void FixedUpdate()
    {
        Fsm.FixedUpdate();
    }

    protected abstract void InitStates();
    public abstract IState ChooseNextAction();

    public void StartAttack()   => IsAttack = true;
    public void EndAttack()     => IsAttack = false;
    public void OnAttackZone()  => attackZone.Activate();
    public void OffAttackZone() => attackZone.Deactivate();

    public abstract IDamageable.DamageInfo SetDamage();

    public virtual void GetDamage(IDamageable.DamageInfo damageInfo)
    {
        CurrHp -= damageInfo.damage;
        StartCoroutine(HitFlashCoroutine());
        StartCoroutine(HitStopCoroutine());
    }

    protected void TriggerDeathEffect()
    {
        StartCoroutine(DeathEffectCoroutine());
    }

    private IEnumerator HitFlashCoroutine()
    {
        if (!spriteRenderer) yield break;
        Color current = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSecondsRealtime(hitFlashDuration);
        spriteRenderer.color = current;
    }

    private IEnumerator HitStopCoroutine()
    {
        Time.timeScale = 0.05f;
        float elapsed = 0f;
        while (elapsed < hitStopDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = 1f;
    }

    private IEnumerator DeathEffectCoroutine()
    {
        // 완전 정지
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(deathStopDuration);

        // 슬로우모션
        Time.timeScale = deathSlowScale;
        if (deathParticle) deathParticle.Play();

        float elapsed = 0f;
        while (elapsed < deathSlowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
    }
}
