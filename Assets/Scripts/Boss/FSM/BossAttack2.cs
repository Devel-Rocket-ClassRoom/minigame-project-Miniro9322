using UnityEngine;

public class BossAttack2 : IState
{
    private static readonly int Attack2Hash = Animator.StringToHash("Attack2");

    private Boss1 boss;

    public BossAttack2(Boss1 boss)
    {
        this.boss = boss;
    }
    public void Enter()
    {
        boss.Animator.Play(Attack2Hash);
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
