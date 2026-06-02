using System.Collections;
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
        player.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        player.Animator.Play(DieHash);
        player.ToggleInvincible();
        player.StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // 히트스탑
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.3f);

        // 슬로우모션으로 사망 애니메이션 재생
        Time.timeScale = 0.3f;

        yield return null;
        var stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);
        // unscaled 기준으로 애니메이션 길이만큼 대기
        yield return new WaitForSecondsRealtime(stateInfo.length);

        Time.timeScale = 1f;
        player.Animator.updateMode = AnimatorUpdateMode.Normal;

        player.OnGameOver?.Invoke();
    }

    public void Exit() { }
    public void FixedUpdate() { }
    public void Update() { }
}
