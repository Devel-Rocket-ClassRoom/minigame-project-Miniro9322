using UnityEngine;
using Cinemachine;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

[RequireComponent(typeof(CinemachineVirtualCamera))]
[RequireComponent(typeof(CinemachineConfiner2D))]
public class CinemachineCamera : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    [SerializeField] private float intensity;
    [SerializeField] private float duration;

    [SerializeField] private Player player;

    [SerializeField] private float zoomSize = 3f;
    [SerializeField] private float zoomInDuration = 0.08f;
    [SerializeField] private float holdDuration = 0.15f;
    [SerializeField] private float zoomOutDuration = 0.3f;
    [SerializeField] private AudioClip sceenBgm;
    [SerializeField] private AudioClip bossBgm;

    private float defaultSize;
    private CancellationTokenSource zoomCoroutine;
    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            noise       = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            defaultSize = virtualCamera.m_Lens.OrthographicSize;
        }
        confiner = GetComponent<CinemachineConfiner2D>();
    }

    private void Start()
    {
        SoundManager.Instance.PlayBGM(sceenBgm);

    }

    private void OnEnable()
    {
        if (player != null)
            player.SuccessParry.AddListener(TriggerZoom);
    }

    private void OnDisable()
    {
        if (player != null)
            player.SuccessParry.RemoveListener(TriggerZoom);
    }

    public void TriggerZoom()
    {
        if (virtualCamera == null) return;

        zoomCoroutine?.Cancel();
        zoomCoroutine = new();
        _ = ZoomCoroutine(zoomCoroutine);
    }

    async UniTask ZoomCoroutine(CancellationTokenSource cts)
    {
        try
        {
            float elapsed = 0f;
            float startSize = virtualCamera.m_Lens.OrthographicSize;
            while (elapsed < zoomInDuration)
            {
                cts.Token.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, zoomSize, Mathf.Clamp01(elapsed / zoomInDuration));
                confiner.InvalidateCache();
                await UniTask.WaitForEndOfFrame();
            }
            virtualCamera.m_Lens.OrthographicSize = zoomSize;
            confiner.InvalidateCache();
            await UniTask.WaitForEndOfFrame();

            float held = 0f;
            while (held < holdDuration)
            {
                cts.Token.ThrowIfCancellationRequested();
                held += Time.unscaledDeltaTime;
                await UniTask.Yield();
            }

            elapsed = 0f;
            startSize = virtualCamera.m_Lens.OrthographicSize;
            while (elapsed < zoomOutDuration)
            {
                cts.Token.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, defaultSize, Mathf.Clamp01(elapsed / zoomOutDuration));
                confiner.InvalidateCache();
                await UniTask.WaitForEndOfFrame();
            }
            virtualCamera.m_Lens.OrthographicSize = defaultSize;
            confiner.InvalidateCache();
            await UniTask.WaitForEndOfFrame();
            cts.Token.ThrowIfCancellationRequested();
            zoomCoroutine = null;
        }
        catch (OperationCanceledException)
        {
            cts.Dispose();
        }
        
    }

    public void TriggerShake()
    {
        if (noise == null) return;
        _ = ShakeCoroutine(intensity, duration);
    }

    async UniTask ShakeCoroutine(float intensity, float duration)
    {
        noise.m_AmplitudeGain = intensity;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            await UniTask.Yield();
        }

        noise.m_AmplitudeGain = 0f;
    }

    public void OnBossSpawn()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlayBGM(bossBgm);
    }
}