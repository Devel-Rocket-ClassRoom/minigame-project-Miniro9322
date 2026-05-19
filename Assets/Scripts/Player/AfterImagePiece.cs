using UnityEngine;

/// <summary>
/// 개별 잔상 1개의 페이드아웃 처리
/// DashAfterImage에서 동적으로 추가되므로 직접 붙일 필요 없음
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AfterImagePiece : MonoBehaviour
{
    private SpriteRenderer sr;
    private float fadeDuration;
    private float startAlpha;
    private float timer;

    public void Init(float fadeDuration)
    {
        sr = GetComponent<SpriteRenderer>();
        this.fadeDuration = Mathf.Max(0.01f, fadeDuration);
        startAlpha = sr.color.a;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / fadeDuration;

        Color c = sr.color;
        c.a = Mathf.Lerp(startAlpha, 0f, t);
        sr.color = c;

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
