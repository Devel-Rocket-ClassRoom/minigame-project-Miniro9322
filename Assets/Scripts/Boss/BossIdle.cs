using UnityEngine;

public class BossIdle : IState
{
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    
    private BossController boss;

    private float attack2CoolTime = 0f;

    public BossIdle(BossController boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        if (boss.CurrHp <= 0f)
        {
            boss.Fsm.ChangeState(new BossDeath(boss));
        }
    }
}
