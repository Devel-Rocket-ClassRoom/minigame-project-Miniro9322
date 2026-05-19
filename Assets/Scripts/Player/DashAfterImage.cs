using System.Collections;
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
    [Tooltip("잔상 생성 간격 (초)")]
    [SerializeField] private float spawnInterval = 0.05f;

    [Tooltip("잔상이 처음 생성될 때의 알파값 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float startAlpha = 0.6f;

    [Tooltip("잔상이 완전히 사라지는 데 걸리는 시간 (초)")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Tooltip("잔상 정렬 순서 오프셋 (음수 = 원본 뒤로)")]
    [SerializeField] private int sortingOrderOffset = -1;

    private bool isSpawning = false;
    private Coroutine routine;

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
        routine = StartCoroutine(SpawnLoop());
    }

    /// <summary>대시 종료 — 잔상 생성 중단</summary>
    public void StopAfterImage()
    {
        isSpawning = false;
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    /// <summary>지정한 시간 동안만 잔상 재생 (가장 자주 쓰는 패턴)</summary>
    public void PlayAfterImage(float duration)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PlayForDuration(duration));
    }

    private IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator PlayForDuration(float duration)
    {
        isSpawning = true;
        float t = 0f;
        while (t < duration)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
            t += spawnInterval;
        }
        isSpawning = false;
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

        // 원본 색 유지, 알파만 낮춤
        Color c = spriteRenderer.color;
        c.a = startAlpha;
        ghostSr.color = c;

        AfterImagePiece piece = ghost.AddComponent<AfterImagePiece>();
        piece.Init(fadeDuration);
    }
}
