using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] private GameObject warning2;
    [SerializeField] private SceneTransitionWall transitionWall;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip attack1Sound;
    [SerializeField] private AudioClip attack2Sound;
    [SerializeField] private AudioClip rushSound;
    [SerializeField] private AudioClip hitSound;
    private float stunTime = 0f;
    private bool isStuned;

    public UnityEvent BossDeath;

    protected override void Update()
    {
        if (IsGameOver || IsDead)
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

        if (periodicTimer >= periodicInterval &&
            Fsm.CurrentState != Attack2 &&
            Fsm.CurrentState != Attack1 &&
            Fsm.CurrentState != Rush &&
            Fsm.CurrentState != Death)
        {
            periodicTimer = 0f;
            Fsm.ChangeState(Attack2);
            return;
        }

        base.Update();
    }

    public override IState ChooseNextAction()
    {
        if (periodicTimer >= periodicInterval)
        {
            periodicTimer = 0f;
            return Attack2;
        }

        bool canAttack1 = PlayerDistance <= closeRange;
        bool canRush    = PlayerDistance <= farRange;

        if (!canAttack1 && !canRush) return Chase;

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
        IsGameOver = true;
    }

    public override IDamageable.DamageInfo SetDamage()
    {
        return new IDamageable.DamageInfo() { canParry = CanParry, damage = Data.atk, ignoreInvincible = IgnoreInvincible };
    }

    public override void GetDamage(IDamageable.DamageInfo damageInfo)
    {
        if (IsDead) return;

        SoundManager.Instance.PlaySFX(hitSound);
        base.GetDamage(damageInfo);
        Animator.Play(HitHash);

        if (CurrHp <= 0)
        {
            IsDead = true;
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
        IsDead = true;
    }

    private void OnDeath()
    {
        warning.SetActive(false);
        warning2.SetActive(false);

        if (GameManager.Instance.isExtra)
        {
            BossDeath?.Invoke();
            return;
        }

        if (transitionWall != null)
            transitionWall.Activate();
    }

    private void ToggleWarning() => warning.SetActive(!warning.activeSelf);
    private void ToggleWarning2() => warning2.SetActive(!warning2.activeSelf);

    public void RushAvailable()
    {
        CanRush = true;
    }

    public void DisableRush()
    {
        CanRush = false;
    }

    private void PlayWalkSound() => SoundManager.Instance.PlaySFX(walkSound, 3f);
    private void PlayAttack1Sound() => SoundManager.Instance.PlaySFX(attack1Sound, 3f);
    private void PlayAttack2Sound() => SoundManager.Instance.PlaySFX(attack2Sound);
    private void PlayRushSound() => SoundManager.Instance.PlaySFX(rushSound);
}