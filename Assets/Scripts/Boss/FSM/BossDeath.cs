using UnityEngine;

public class BossDeath : IState
{
    private static readonly int DeathHash = Animator.StringToHash("Die");

    private Boss1 boss;

    public BossDeath(Boss1 boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        boss.Animator.SetTrigger(DeathHash);
        boss.SetDeath();
    }

    public void Exit()
    {

    }

    public void Update()
    {

    }
}
