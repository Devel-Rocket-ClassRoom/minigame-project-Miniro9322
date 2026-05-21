using UnityEngine;

public class BossAttack1 : IState
{
    private static readonly int Attack1Hash = Animator.StringToHash("Attack1");

    private Boss1 boss;

    public BossAttack1(Boss1 boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        boss.Animator.Play(Attack1Hash);
        boss.CanParry = false;
    }

    public void Exit()
    {
        boss.IsAttack = false;
    }

    public void FixedUpdate()
    {
    }

    public void Update()
    {
        if (boss.CurrHp <= 0f)
        {
            boss.Fsm.ChangeState(boss.Death);
        }

        if (!boss.IsAttack)
        {
            boss.Fsm.ChangeState(boss.Idle);
        }
    }
}
