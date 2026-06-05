using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;

    [SerializeField] private float crossFadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource.ignoreListenerPause = true;
        sfxSource.ignoreListenerPause = true;

        ApplyVolume();
    }

    private void ApplyVolume()
    {
        bgmSource.volume = SaveManager.Data.bgmVolume;
        sfxSource.volume = SaveManager.Data.sfxVolume;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;
        StartCoroutine(CrossFade(clip));
    }

    public void StopBGM() => StartCoroutine(FadeOut());

    public void PlaySFX(AudioClip clip, float multiple = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, SaveManager.Data.sfxVolume * multiple);
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
        SaveManager.SetBGMVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        SaveManager.SetSFXVolume(volume);
    }

    private IEnumerator CrossFade(AudioClip newClip)
    {
        float startVolume = bgmSource.volume;

        // 페이드 아웃
        float elapsed = 0f;
        while (elapsed < crossFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / crossFadeDuration);
            yield return null;
        }

        bgmSource.clip = newClip;
        bgmSource.Play();

        // 페이드 인
        elapsed = 0f;
        while (elapsed < crossFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVolume, elapsed / crossFadeDuration);
            yield return null;
        }

        bgmSource.volume = startVolume;
    }

    private IEnumerator FadeOut()
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;
        while (elapsed < crossFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / crossFadeDuration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }

    public void PlaySFXLoop(AudioClip clip)
    {
        if (clip == null) return;
        loopSfxSource.clip = clip;
        loopSfxSource.loop = true;
        loopSfxSource.Play();
    }

    public void StopSFXLoop()
    {
        loopSfxSource.loop = false;
        loopSfxSource.Stop();
    }
}
