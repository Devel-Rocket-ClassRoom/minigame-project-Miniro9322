using UnityEngine;
using Cinemachine;
using System.Collections;
[RequireComponent(typeof(CinemachineVirtualCamera))]

[RequireComponent(typeof(CinemachineConfiner))]
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

    private float defaultSize;
    private Coroutine zoomCoroutine;
    private CinemachineConfiner confiner;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            noise       = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            defaultSize = virtualCamera.m_Lens.OrthographicSize;
        }
        confiner = GetComponent<CinemachineConfiner>();
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

        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomCoroutine(Vector3.zero));
    }

    private IEnumerator ZoomCoroutine(Vector3 parryPosition)
    {
        float elapsed   = 0f;
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        while (elapsed < zoomInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, zoomSize, Mathf.Clamp01(elapsed / zoomInDuration));
            confiner.InvalidatePathCache();
            yield return null;
        }
        virtualCamera.m_Lens.OrthographicSize = zoomSize;
        confiner.InvalidatePathCache();

        float held = 0f;
        while (held < holdDuration)
        {
            held += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed   = 0f;
        startSize = virtualCamera.m_Lens.OrthographicSize;
        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, defaultSize, Mathf.Clamp01(elapsed / zoomOutDuration));
            confiner.InvalidatePathCache();
            yield return new WaitForEndOfFrame();
        }
        virtualCamera.m_Lens.OrthographicSize = defaultSize;
        confiner.InvalidatePathCache();
        yield return new WaitForEndOfFrame();
        zoomCoroutine = null;
    }

    public void TriggerShake()
    {
        if (noise == null) return;
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        noise.m_AmplitudeGain = intensity;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        noise.m_AmplitudeGain = 0f;
    }
}