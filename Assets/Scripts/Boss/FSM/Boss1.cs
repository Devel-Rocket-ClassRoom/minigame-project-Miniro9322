using UnityEngine;

public class Boss1 : BossController
{
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int ParryHash = Animator.StringToHash("Parry");

    public IState Idle { get; private set; }
    public IState Chase { get; private set; }
    public IState Attack1 { get; private set; }
    public IState Attack2 { get; private set; }
    public IState Rush { get; private set; }
    public IState Death { get; private set; }
    public bool CanParry { get; set; }
    public bool CanRush { get; private set; } = false;
    public bool IgnoreInvincible { get; set; } = false;


    [SerializeField] private float closeRange = 3f;
    [SerializeField] private float farRange = 8f;
    [SerializeField] private float periodicInterval = 5f;
    public float CloseRange => closeRange;
    public float FarRange => farRange;

    [Header("── 패턴 가중치 ──")]
    [SerializeField] private float weightAttack1 = 60f;
    [SerializeField] private float weightRush = 40f;

    private float periodicTimer;
    [SerializeField] private float stunInterval = 3f;
    [SerializeField] private GameObject warning;
    [SerializeField] private SceneTransitionWall transitionWall;
    private float stunTime = 0f;
    private bool isGameOver;
    private bool isStuned;
    private bool isDeath;

    protected override void Update()
    {
        if (isGameOver || isDeath)
        {
            return;
        }

        if (isStuned)
        {
            if(stunTime < stunInterval)
            {
                stunTime += Time.deltaTime;
                return;
            }

            stunTime = 0f;
            isStuned = false;
        }

        periodicTimer += Time.deltaTime;
        base.Update();
    }

    public override IState ChooseNextAction()
    {
        // Attack2는 쿨타임 방식으로 우선 발동
        if (periodicTimer >= periodicInterval)
        {
            periodicTimer = 0f;
            return Attack2;
        }

        bool canAttack1 = PlayerDistance <= closeRange;
        bool canRush    = PlayerDistance <= farRange;

        // 둘 다 사거리 밖이면 추격
        if (!canAttack1 && !canRush) return Chase;

        // 사거리 내 후보만 추려서 가중치 선택
        float total = 0f;
        if (canAttack1) total += weightAttack1;
        if (canRush)    total += weightRush;

        float roll = Random.Range(0f, total);

        if (canAttack1 && roll < weightAttack1) return Attack1;
        return Rush;
    }

    protected override void InitStates()
    {
        Idle = new BossIdle(this);
        Chase = new BossChase(this);
        Attack1 = new BossAttack1(this);
        Attack2 = new BossAttack2(this);
        Rush = new BossRushAttack(this);
        Death = new BossDeath(this);
        Fsm.ChangeState(Idle);
    }

    public void OnGameOver()
    {
        isGameOver = true;
    }

    public override IDamageable.DamageInfo SetDamage()
    {
        return new IDamageable.DamageInfo() { canParry = CanParry, damage = Data.atk, ignoreInvincible = IgnoreInvincible };
    }

    public override void GetDamage(IDamageable.DamageInfo damageInfo)
    {
        if (isDeath) return;

        base.GetDamage(damageInfo);
        Animator.Play(HitHash);

        if (CurrHp <= 0)
        {
            isDeath = true;
            TriggerDeathEffect();
            Fsm.ChangeState(Death);
        }
    }

    public void OnParry()
    {
        Fsm.ChangeState(Idle);
        isStuned = true;
        Animator.SetTrigger(ParryHash);
        IsAttack = false;
    }

    public void SetDeath()
    {
        isDeath = true;
    }

    private void OnDeath()
    {
        if (transitionWall != null)
            transitionWall.Activate();
    }

    private void ToggleWarning() => warning.SetActive(!warning.activeSelf);

    public void RushAvailable()
    {
        CanRush = !CanRush;
    }
}
