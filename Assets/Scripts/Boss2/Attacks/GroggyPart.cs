using UnityEngine;
using UnityEngine.Pool;

public class GroggyPart : MonoBehaviour, IDamageable
{
    [Header("설정")]
    [SerializeField] private float maxHP = 100f;

    [Header("시각 효과")]
    [SerializeField] private AudioClip destroySound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private Color damagedColor = Color.red;

    private Boss2Controller boss;
    private float currentHP;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool isDestroyed = false;

    private IObjectPool<GroggyPart> objectPool;
    public IObjectPool<GroggyPart> ObjectPool { set => objectPool = value; }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr) originalColor = sr.color;
    }

    private void OnEnable()
    {
        CancelInvoke(nameof(ResetColor));
        if (sr)
        {
            sr.enabled = true;
            sr.color   = originalColor;
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ResetColor));
    }

    public void Initialize(Boss2Controller boss)
    {
        this.boss   = boss;
        currentHP   = maxHP;
        isDestroyed = false;
        if (sr)
        {
            sr.enabled = true;
            sr.color   = originalColor;
        }
    }

    public IDamageable.DamageInfo SetDamage() => default;

    public void GetDamage(IDamageable.DamageInfo damageInfo)
    {
        TakeDamage(damageInfo.damage);
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;
        SoundManager.Instance.PlaySFX(hitSound);
        currentHP = Mathf.Max(0f, currentHP - damage);

        if (sr)
        {
            sr.color = damagedColor;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.1f);
        }

        if (currentHP <= 0f) DestroyPart();
    }

    private void DestroyPart()
    {
        isDestroyed = true;
        boss?.OnGroggyPartDestroyed();
        SoundManager.Instance.PlaySFX(destroySound);
        objectPool.Release(this);
    }

    private void ResetColor()
    {
        if (sr) sr.color = originalColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.TryGetComponent<IDamageable>(out var weapon)) GetDamage(weapon.SetDamage());
    }
}
