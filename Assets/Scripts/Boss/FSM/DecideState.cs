using UnityEngine;

public class DecideState : IState
{
    private BossController boss;
    public DecideState(BossController boss) { this.boss = boss; }

    public void Enter()
    {
        boss.Fsm.ChangeState(boss.ChooseNextAction());
    }

    public void Update() { }
    public void Exit() { }
}