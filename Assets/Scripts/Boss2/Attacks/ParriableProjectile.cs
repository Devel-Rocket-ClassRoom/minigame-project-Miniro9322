using UnityEngine;
using UnityEngine.Pool;


public class ParriableProjectile : MonoBehaviour, IDamageable
{
    [Header("설정")]
    [SerializeField] private float playerDamageMultiplier  = 1f;
    [SerializeField] private float parriedDamageMultiplier = 2.5f;
    [SerializeField] private float reflectSpeedMult        = 1.5f;

    private int playerDamage;
    private int parriedDamage;

    [Header("시각 효과")]
    [SerializeField] private Color normalColor  = new Color(1f, 0.8f, 0f);
    [SerializeField] private Color parriedColor = Color.cyan;

    private Boss2Controller boss;
    private Rigidbody2D     rb;
    private SpriteRenderer  sr;
    private bool isParried  = false;

    private IObjectPool<ParriableProjectile> objectPool;
    public IObjectPool<ParriableProjectile> ObjectPool { set => objectPool = value; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public IDamageable.DamageInfo SetDamage() => new() { damage = playerDamage, canParry = true };
    public void GetDamage(IDamageable.DamageInfo damageInfo) { }

    private void FixedUpdate()
    {
        if (rb && rb.linearVelocity.sqrMagnitude > 0.01f)
            rb.rotation = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
    }

    public void Initialize(Boss2Controller boss, int baseAtk)
    {
        this.boss     = boss;
        playerDamage  = Mathf.RoundToInt(baseAtk * playerDamageMultiplier);
        parriedDamage = Mathf.RoundToInt(baseAtk * parriedDamageMultiplier);
        isParried     = false; // 재사용 시 상태 리셋
        if (sr) sr.color = normalColor;
    }

    public void OnParried()
    {
        if (isParried) return;
        isParried = true;

        if (sr) sr.color = parriedColor;
        if (boss == null || rb == null) return;

        // 이미 보스와 겹쳐있으면 즉시 데미지
        var col     = GetComponent<Collider2D>();
        var bossCol = boss.GetComponent<Collider2D>();
        if (col != null && bossCol != null && col.IsTouching(bossCol))
        {
            boss.TakeDamage(parriedDamage);
            ReleaseToPool();
            return;
        }

        Vector3 dir   = (boss.transform.position - transform.position).normalized;
        float   speed = rb.linearVelocity.magnitude;
        rb.linearVelocity = dir * speed * reflectSpeedMult;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isParried)
        {
            if (other.CompareTag("Boss"))
            {
                boss?.TakeDamage(parriedDamage);
                ReleaseToPool();
            }
        }
        else if (other.CompareTag("Player"))
        {
            var playerComp = other.GetComponent<Player>();
            if (playerComp != null)
            {
                playerComp.SuccessParry.AddListener(OnParried);
                playerComp.GetDamage(SetDamage());
                playerComp.SuccessParry.RemoveListener(OnParried);
            }
            if (!isParried) ReleaseToPool();
        }
    }

    /// <summary>이중 반환 방지: 이미 비활성(반환됨)이면 스킵</summary>
    private void ReleaseToPool()
    {
        if (!gameObject.activeSelf) return;
        objectPool?.Release(this);
    }
}
