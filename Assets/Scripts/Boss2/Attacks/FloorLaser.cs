using UnityEngine;
using System.Collections;

public class FloorLaser : MonoBehaviour
{
    [Header("두께")]
    [Tooltip("경고 실선 두께")]
    [SerializeField] private float warningThickness = 0.04f;
    [Tooltip("발사 시 최대 두께")]
    [SerializeField] private float activeThickness = 1.2f;
    [Tooltip("얇은 선 → 굵은 선 확장 시간 (초)")]
    [SerializeField] private float expandDuration = 0.07f;

    [Header("피해")]
    [SerializeField] private int damage = 30;

    private SpriteRenderer sr;
    private Collider2D col;
    private bool isActive = false;
    private Coroutine flickerCoroutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void StartWarning()
    {
        if (col) col.enabled = false;

        SetThickness(warningThickness);
    }

    public void Activate(float activeDuration)
    {
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        StartCoroutine(ActivateCoroutine(activeDuration));
    }

    private IEnumerator ActivateCoroutine(float activeDuration)
    {
        float elapsed = 0f;
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            SetThickness(Mathf.Lerp(warningThickness, activeThickness, elapsed / expandDuration));
            yield return null;
        }
        SetThickness(activeThickness);

        if (col) col.enabled = true;
        isActive = true;

        Destroy(gameObject, activeDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        var info = new IDamageable.DamageInfo { damage = damage, canParry = false };
        other.GetComponent<IDamageable>()?.GetDamage(info);
    }

    private void SetThickness(float thickness)
    {
        Vector3 s = transform.localScale;
        s.y = thickness;
        transform.localScale = s;
    }
}