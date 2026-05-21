using UnityEngine;

public class IdleState : IState
{
    private Player player;

    public IdleState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void FixedUpdate()
    {
        if (player.Rb.linearVelocity.y < 0f)
            player.Fsm.ChangeState(player.FallState);
    }

    public void Update()
    {

    }
}
