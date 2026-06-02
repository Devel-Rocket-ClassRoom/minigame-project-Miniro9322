using UnityEngine;

public class CrouchState : IState
{
    private static readonly int CrouchHash = Animator.StringToHash("Crouch");
    private Player player;

    public CrouchState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Animator.SetBool(CrouchHash, true);
        player.SetColliderCrouch(true);
    }

    public void Exit()
    {
        player.Animator.SetBool(CrouchHash, false);
        player.SetColliderCrouch(false);
    }

    public void FixedUpdate()
    {
        // 지면에서 벗어나면 크라우칭 해제
        if (!player.Grounded)
            player.Fsm.ChangeState(player.FallState);
    }

    public void Update() { }
}
