using UnityEngine;

public class BossDeath : IState
{
    private static readonly int DeathHash = Animator.StringToHash("Death");

    private BossController boss;

    public BossDeath(BossController boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        boss.Animator.SetTrigger(DeathHash);
    }

    public void Exit()
    {

    }

    public void Update()
    {

    }
}
