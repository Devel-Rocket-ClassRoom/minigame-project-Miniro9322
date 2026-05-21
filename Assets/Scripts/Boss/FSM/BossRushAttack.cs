using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BossRushAttack : IState
{
    private static readonly int RushHash = Animator.StringToHash("Rush");

    private Boss1 boss;
    private Vector3 startPoint;
    private float rushAmount = 10f;
    private float rushTime = 0f;
    private float rushDuration = 5f;
    private Vector3 rushVector;
    private float dir;

    public BossRushAttack(Boss1 boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        Debug.Log("돌진");
        boss.Animator.Play(RushHash);
        boss.CanParry = true;
        startPoint = boss.transform.position;
        dir = boss.transform.localScale.x;
    }

    public void Exit()
    {
        boss.IsAttack = false;
        rushTime = 0f;
    }

    public void Update()
    {
        if (boss.CurrHp <= 0f)
        {
            boss.Fsm.ChangeState(boss.Death);
        }

        rushTime += Time.fixedDeltaTime;
        rushVector = new Vector3(startPoint.x + rushAmount * dir, startPoint.y);
        boss.transform.position = Vector3.Lerp(startPoint, rushVector, rushTime / rushDuration);

        if (!boss.IsAttack)
        {
            boss.Fsm.ChangeState(boss.Idle);
        }
    }
}
