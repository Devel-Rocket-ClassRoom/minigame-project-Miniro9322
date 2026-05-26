using UnityEngine;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(CinemachineVirtualCamera))]
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
    private Transform originalFollow;
    private GameObject zoomFocus;
    private Coroutine zoomCoroutine;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            defaultSize = virtualCamera.m_Lens.OrthographicSize;
            originalFollow = virtualCamera.Follow;
        }

        zoomFocus = new GameObject("_ZoomFocus");
        zoomFocus.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(zoomFocus);
    }

    private void OnDestroy()
    {
        if (zoomFocus != null)
            Destroy(zoomFocus);
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
        if (virtualCamera == null || player == null) return;

        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomCoroutine(player.transform.position));
    }

    private IEnumerator ZoomCoroutine(Vector3 parryPosition)
    {
        // 패링 위치에 포커스 고정
        zoomFocus.transform.position = parryPosition;
        virtualCamera.Follow = zoomFocus.transform;
        virtualCamera.LookAt = zoomFocus.transform;

        // 줌인
        float elapsed = 0f;
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        while (elapsed < zoomInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, zoomSize, elapsed / zoomInDuration);
            yield return null;
        }
        virtualCamera.m_Lens.OrthographicSize = zoomSize;

        // 유지
        float held = 0f;
        while (held < holdDuration)
        {
            held += Time.unscaledDeltaTime;
            yield return null;
        }

        // 줌아웃 + 플레이어 추적 복원
        virtualCamera.Follow = originalFollow;
        virtualCamera.LookAt = originalFollow;

        elapsed = 0f;
        startSize = virtualCamera.m_Lens.OrthographicSize;
        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, defaultSize, elapsed / zoomOutDuration);
            yield return null;
        }
        virtualCamera.m_Lens.OrthographicSize = defaultSize;

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