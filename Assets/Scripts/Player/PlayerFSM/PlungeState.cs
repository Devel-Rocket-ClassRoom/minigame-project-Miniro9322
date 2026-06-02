using UnityEngine;

public class PlungeState : IState
{
    private static readonly int PlungeHash = Animator.StringToHash("Plunge");
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private Player player;

    public PlungeState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Animator.Play(PlungeHash);
        player.Rb.linearVelocity = new Vector2(player.Rb.linearVelocity.x, -player.Data.PlungeSpeed);
        player.downAttackZone.gameObject.SetActive(true);
    }

    public void Exit()
    {
        player.Animator.Play(IdleHash);
        player.downAttackZone.gameObject.SetActive(false);
    }

    public void FixedUpdate()
    {
        if (player.Grounded)
            player.Fsm.ChangeState(player.IdleState);
    }

    public void Update() { }
}
