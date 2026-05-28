using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public float closeRange = 3f;
    public float farRange = 8f;
    public float periodicInterval = 5f;
    private float periodicTimer;
    [SerializeField] private float stunInterval = 3f;
    [SerializeField] private GameObject warning;
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override IState ChooseNextAction()
    {
        if (periodicTimer >= periodicInterval)
        {
            periodicTimer = 0f;
            return Attack2;
        }

        if (PlayerDistance <= closeRange) return Attack1;
        if (PlayerDistance >= farRange) return Rush;
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
        if (isDeath)
            return;

        CurrHp -= damageInfo.damage;
        Animator.Play(HitHash);
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
        SceneManager.LoadScene("Boss2");
    }

    private void ToggleWarning()
    {
        if (warning.activeSelf)
        {
            warning.SetActive(false);
        }
        else
        {
            warning.SetActive(true);
        }
    }
}
