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
        player.Animator.Play(DieHash);
        player.OnGameOver?.Invoke();
        player.ToggleInvincible();
    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {

    }
}
