using UnityEngine;

public class BossIdle : IState
{
    private static readonly int MoveHash = Animator.StringToHash("Move");
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
        boss.Animator.SetBool(MoveHash, false);
        thinkTime = 0f;
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        if(boss.CurrHp <= 0)
        {
            boss.Fsm.ChangeState(boss.Death);
        }

        if(thinkTime < thinkDuration)
        {
            thinkTime += Time.deltaTime;
            return;
        }

        boss.Animator.SetBool(MoveHash, true);
        boss.transform.position += boss.transform.localScale.x * boss.Data.moveSpeed * Time.deltaTime * Vector3.right;

        if (boss.PlayerDistance <= boss.closeRange || boss.PlayerDistance >= boss.farRange)
            boss.Fsm.ChangeState(boss.DecideState);
    }
}
