using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

/// <summary>
/// 대시 잔상 효과 컨트롤러
/// 캐릭터의 SpriteRenderer가 있는 GameObject에 붙여서 사용
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DashAfterImage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Afterimage Settings")]
    [Tooltip("잔상 생성 간격 (ms)")]
    [SerializeField] private int spawnInterval = 50;

    [Tooltip("잔상이 처음 생성될 때의 알파값 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float startAlpha = 0.6f;

    [Tooltip("잔상이 완전히 사라지는 데 걸리는 시간 (초)")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Tooltip("잔상 정렬 순서 오프셋 (음수 = 원본 뒤로)")]
    [SerializeField] private int sortingOrderOffset = -1;

    private bool isSpawning = false;
    private CancellationTokenSource cts;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>대시 시작 — 멈추라고 할 때까지 계속 잔상 생성</summary>
    public void StartAfterImage()
    {
        if (isSpawning) return;
        isSpawning = true;
        _ = SpawnLoop();
    }

    /// <summary>대시 종료 — 잔상 생성 중단</summary>
    public void StopAfterImage()
    {
        isSpawning = false;
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    /// <summary>지정한 시간 동안만 잔상 재생 (가장 자주 쓰는 패턴)</summary>
    async UniTask PlayAfterImage(float duration)
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }
        cts = new CancellationTokenSource();
        _ = PlayForDuration(duration, cts.Token);
    }

    async UniTask SpawnLoop()
    {
        while (isSpawning)
        {
            SpawnOne();
            await UniTask.Delay(spawnInterval);
        }
    }

    async UniTask PlayForDuration(float duration, CancellationToken cts)
    {
        try
        {
            isSpawning = true;
            float t = 0f;
            while (t < duration)
            {
                cts.ThrowIfCancellationRequested();

                SpawnOne();
                await UniTask.Delay(spawnInterval);
                t += spawnInterval;
            }
            isSpawning = false;
        }
        catch (OperationCanceledException)
        {
            this.cts.Dispose();
        }
        
    }

    private void SpawnOne()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        GameObject ghost = new GameObject("Afterimage");
        ghost.transform.SetPositionAndRotation(transform.position, transform.rotation);
        ghost.transform.localScale = transform.lossyScale;

        SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
        ghostSr.sprite = spriteRenderer.sprite;
        ghostSr.flipX = spriteRenderer.flipX;
        ghostSr.flipY = spriteRenderer.flipY;
        ghostSr.sortingLayerID = spriteRenderer.sortingLayerID;
        ghostSr.sortingOrder = spriteRenderer.sortingOrder + sortingOrderOffset;

        Color c = spriteRenderer.color;
        c.a = startAlpha;
        ghostSr.color = c;

        AfterImagePiece piece = ghost.AddComponent<AfterImagePiece>();
        piece.Init(fadeDuration);
    }
}
