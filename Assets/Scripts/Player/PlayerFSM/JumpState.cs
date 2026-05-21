using UnityEngine;

public class JumpState : IState
{
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private Player player;

    public JumpState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Animator.SetBool(JumpHash, true);
        player.Animator.Play(JumpHash);

        player.Rb.linearVelocity = new Vector2(player.Rb.linearVelocity.x, player.Data.JumpPower);
    }

    public void Exit()
    {
        player.Animator.SetBool(JumpHash, false);
    }

    public void FixedUpdate()
    {
        if (!player.JumpHeld && player.Rb.linearVelocity.y > 0f)
        {
            player.Rb.linearVelocity += (player.Data.LowJumpMultiplier - 1f)
                * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
        }

        if (player.Rb.linearVelocity.y <= 0f)
            player.Fsm.ChangeState(player.FallState);
    }

    public void Update() { }
}