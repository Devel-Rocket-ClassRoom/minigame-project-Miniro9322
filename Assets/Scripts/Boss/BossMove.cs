using UnityEngine;

public class BossMove : IState
{
    private static readonly int MoveHash = Animator.StringToHash("Move");
    
    private BossController boss;

    public BossMove(BossController boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        boss.Animator.SetBool(MoveHash, true);
    }

    public void Exit()
    {
        boss.Animator.SetBool(MoveHash, false);
    }

    public void Update()
    {
        if (boss.CurrHp <= 0f)
        {
            boss.Fsm.ChangeState(new BossDeath(boss));
        }
    }
}
