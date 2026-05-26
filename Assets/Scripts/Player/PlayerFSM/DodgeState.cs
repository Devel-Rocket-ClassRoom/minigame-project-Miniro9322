using UnityEngine;

public class DodgeState : IState
{
    private static readonly int DodgeHash = Animator.StringToHash("Dodge");
    private Player player;
    private float dodgeTime;
    private Vector3 dodgeEnd;
    private Vector3 dodgeStart;

    public DodgeState(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Animator.Play(DodgeHash);

        dodgeStart = player.transform.position;
        dodgeEnd = new Vector3(
            dodgeStart.x + player.Data.DodgeAmount * player.transform.localScale.x,
            dodgeStart.y);

        player.Rb.linearVelocity = Vector2.zero;
        player.Rb.gravityScale = 0f;

        dodgeTime = 0f;

        player.ToggleInvincible();
        player.AfterImage.StartAfterImage();
    }

    public void Exit()
    {
        player.Rb.MovePosition(dodgeEnd);
        player.Rb.linearVelocity = Vector2.zero;
        player.Rb.gravityScale = player.OriginalGravityScale;
        dodgeTime = 0f;
        player.AfterImage.StopAfterImage();
        player.ToggleInvincible();
    }

    public void FixedUpdate()
    {
        if (dodgeTime > player.Data.DodgeDuration)
        {
            player.Fsm.ChangeState(player.IdleState);
            return;
        }

        dodgeTime += Time.fixedDeltaTime;

        player.Rb.MovePosition(Vector3.Lerp(dodgeStart, dodgeEnd, dodgeTime / player.Data.DodgeDuration));
    }

    public void Update() { }
}