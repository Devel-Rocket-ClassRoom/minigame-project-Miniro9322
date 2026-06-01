using UnityEngine;

public class Boss1 : BossController
{
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int ParryHash = Animator.StringToHash("Parry");

    public IState Idle { get; private set; }
    public IState Attack1 { get; private set; }
    public IState Attack2 { get; private set; }
    public IState Rush { get; private set; }
    public IState Death { get; private set; }
    public bool CanParry { get; set; }
    public bool CanRush { get; private set; } = false;


    [SerializeField] private float closeRange = 3f;
    [SerializeField] private float farRange = 8f;
    [SerializeField] private float periodicInterval = 5f;
    public float CloseRange => closeRange;
    public float FarRange => farRange;

    [Header("── 패턴 가중치 ──")]
    [SerializeField] private float weightAttack1 = 50f;
    [SerializeField] private float weightRush = 30f;
    [SerializeField] private float weightIdle = 20f;

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
        if (periodicTimer >= periodicInterval)
        {
            periodicTimer = 0f;
            return Attack2;
        }

        float total = weightAttack1 + weightRush + weightIdle;
        float roll = Random.Range(0f, total);

        if (roll < weightAttack1) return Attack1;
        if (roll < weightAttack1 + weightRush) return Rush;
        return Idle;
    }

    protected override void InitStates()
    {
        Idle = new BossIdle(this);
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
        return new IDamageable.DamageInfo() { canParry = this.CanParry, damage = Data.atk };
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
