using UnityEngine;

public class AttackState : IState
{
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int AttackCountHash = Animator.StringToHash("AttackCount");
    private Player player;
    private int attackCount = 0;

    public AttackState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.ResetAttackEnd();
        player.Animator.SetInteger(AttackCountHash, attackCount);
        player.Animator.SetTrigger(AttackHash);
    }

    public void Exit()
    {
        attackCount = 0;
        player.Animator.SetInteger(AttackCountHash, attackCount);
        player.AttackEnd();
        player.CommandQueue.Clear();
        player.CloseInputQueue();
        player.Animator.ResetTrigger(AttackHash);
        player.Effect.SetActive(false);
    }

    public void FixedUpdate() { }

    public void Update()
    {
        if (player.IsAttackEnd)
        {
            if(player.CommandQueue.Count == 0)
            {
                player.Fsm.ChangeState(player.IdleState);
                return;
            }

            if (player.CommandQueue.Dequeue() == "A")
            {
                attackCount++;
                player.Animator.SetInteger(AttackCountHash, attackCount);
                player.Animator.SetTrigger(AttackHash);
                player.CommandQueue.Clear();
                player.ResetAttackEnd();
                return;
            }
        }

        if (attackCount > player.Data.MaxAttackCount)
            player.Fsm.ChangeState(player.IdleState);
    }
}
