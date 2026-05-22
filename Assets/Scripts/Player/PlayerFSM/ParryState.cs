using UnityEngine;

public class ParryState : IState
{
    private static readonly int ParryHash = Animator.StringToHash("Parrying");
    private Player player;
    private float parryTime;

    public ParryState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Animator.Play(ParryHash);
        parryTime = 0f;
        player.ParryStart?.Invoke();
        player.ToggleParry();
    }

    public void Exit()
    {
        player.ToggleParry();
    }

    public void FixedUpdate() { }

    public void Update()
    {
        parryTime += Time.deltaTime;

        if(parryTime > player.Data.ParryInterval)
            player.Fsm.ChangeState(player.IdleState);
    }
}
