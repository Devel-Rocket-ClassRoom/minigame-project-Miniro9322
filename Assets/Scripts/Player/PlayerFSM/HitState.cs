using UnityEngine;

public class HitState : IState
{
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly Color32 blinkColor = new(255, 180, 180, 255);

    private Player player;
    private Color defaultColor;
    private float blinkTime;
    private float blinkDuration;
    private bool isBlinked;

    public HitState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        defaultColor = player.Sr.color;

        player.Animator.Play(HitHash);

        blinkTime = player.Data.BlinkInterval;
        blinkDuration = player.Data.BlinkDurationInterval;
        isBlinked = false;

        player.ToggleInvincible();
    }

    public void Exit()
    {
        player.Sr.color = defaultColor;
        player.ResetAttackEnd();
        player.ToggleInvincible();
    }

    public void FixedUpdate() { }

    public void Update()
    {
        if (blinkTime > 0f)
        {
            if (blinkDuration > 0f)
            {
                blinkDuration -= Time.deltaTime;
            }
            else
            {
                isBlinked = !isBlinked;
                player.Sr.color = isBlinked ? blinkColor : defaultColor;
                blinkDuration = player.Data.BlinkDurationInterval;
            }

            blinkTime -= Time.deltaTime;
        }
        else
        {
            player.Sr.color = defaultColor;

            if (!player.Grounded)
            {
                if (player.Rb.linearVelocity.y > 0f)
                    player.Fsm.ChangeState(player.JumpState);
                else
                    player.Fsm.ChangeState(player.FallState);
            }
            else
            {
                player.Fsm.ChangeState(player.IdleState);
            }
        }
    }
}