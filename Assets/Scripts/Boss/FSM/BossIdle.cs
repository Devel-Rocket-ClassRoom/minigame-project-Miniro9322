using UnityEngine;

public class BossIdle : IState
{
    private Boss1 boss;
    private float maxthinkTime = 2f;
    private float thinkTime;
    private float thinkDuration;

    public BossIdle(Boss1 boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        thinkDuration = Random.Range(1f, maxthinkTime);
    }

    public void Exit()
    {
        thinkTime = 0f;
    }

    public void FixedUpdate() { }

    public void Update()
    {
        if (boss.CurrHp <= 0)
        {
            boss.Fsm.ChangeState(boss.Death);
            return;
        }

        thinkTime += Time.deltaTime;
        if (thinkTime >= thinkDuration)
            boss.Fsm.ChangeState(boss.DecideState);
    }
}
