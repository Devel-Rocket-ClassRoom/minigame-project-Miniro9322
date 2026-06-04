using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip attack1Sound;
    [SerializeField] private AudioClip attack2Sound;
    [SerializeField] private AudioClip attack3Sound;
    [SerializeField] private AudioClip dodgeAttackSound;
    [SerializeField] private AudioClip plungeSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip parrySound;


    private void PlayWalkSound()
    {
        SoundManager.Instance.PlaySFX(walkSound, 3f);
    }

    private void PlayAttack1Sound()
    {
        SoundManager.Instance.PlaySFX(attack1Sound);
    }

    private void PlayAttack2Sound()
    {
        SoundManager.Instance.PlaySFX(attack2Sound);
    }

    private void PlayAttack3Sound()
    {
        SoundManager.Instance.PlaySFX(attack3Sound);
    }

    private void PlayDodgeAttackSound()
    {
        SoundManager.Instance.PlaySFX(dodgeAttackSound);
    }

    private void PlayPlungeSound()
    {
        SoundManager.Instance.PlaySFX(plungeSound, 0.5f);
    }

    private void PlayHitSound()
    {
        SoundManager.Instance.PlaySFX(hitSound);
    }

    public void PlayParrySound()
    {
        SoundManager.Instance.PlaySFX(parrySound);
    }
}
