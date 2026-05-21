using UnityEngine;

public class DeathState : IState
{
    private static readonly int DieHash = Animator.StringToHash("Die");
    private Player player;

    public DeathState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Animator.SetTrigger(DieHash);
    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {

    }
}
