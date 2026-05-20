using UnityEngine;

public class BossAttack : IState
{
    private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int RushHash = Animator.StringToHash("Rush");
    private static readonly int Attack1Hash = Animator.StringToHash("Attack1");

    private BossController boss;

    public BossAttack(BossController boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        //if(boss.PlayerDistance < attackDistance)
        //{
        //    boss.Animator.Play(Attack1Hash);
        //}
        //else if(boss.PlayerDistance > rushDistance)
        //{
        //    boss.Animator.Play(RushHash);
        //}
        //else if(attack2Cool > attack2Interval)
        //{
        //    boss.Animator.Play(Attack2Hash);
        //}
    }

    public void Exit()
    {
        boss.IsAttackEnd = false;
    }

    public void Update()
    {
        if (boss.CurrHp <= 0f)
        {
            boss.Fsm.ChangeState(new BossDeath(boss));
        }

        if (boss.IsAttackEnd)
        {
            boss.Fsm.ChangeState(new BossIdle(boss));
        }
    }
}
