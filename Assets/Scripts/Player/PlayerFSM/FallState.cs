using UnityEngine;

public class FallState : IState
{
    private static readonly int FallHash = Animator.StringToHash("Fall");
    private Player player;

    public FallState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Animator.SetBool(FallHash, true);
        player.Animator.Play(FallHash);
    }

    public void Exit()
    {
        player.Animator.SetBool(FallHash, false);
    }

    public void FixedUpdate()
    {
        if (player.Rb.linearVelocity.y < 0f)
        {
            player.Rb.linearVelocity += (player.Data.FallMultiplier - 1f)
                * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
        }

        if (player.Grounded)
            player.Fsm.ChangeState(player.IdleState);
    }

    public void Update() { }
}