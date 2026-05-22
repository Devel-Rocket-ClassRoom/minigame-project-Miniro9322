using UnityEngine;


public class ParriableProjectile : MonoBehaviour, IDamageable
{
    [Header("설정")]
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float playerDamage = 20f;
    [SerializeField] private float parriedDamage = 50f;
    [SerializeField] private float reflectSpeedMult = 1.5f;

    [Header("시각 효과")]
    [SerializeField] private Color normalColor = new Color(1f, 0.8f, 0f);
    [SerializeField] private Color parriedColor = Color.cyan;

    private Boss2Controller boss;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isParried = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public IDamageable.DamageInfo SetDamage() => new() { damage = (int)playerDamage, canParry = true };

    public void GetDamage(IDamageable.DamageInfo damageInfo) { }

    public void Initialize(Boss2Controller boss)
    {
        this.boss = boss;
        if (sr) sr.color = normalColor;
        Destroy(gameObject, lifetime);
    }

    public void OnParried()
    {
        if (isParried) return;
        isParried = true;

        if (sr) sr.color = parriedColor;

        if (boss != null && rb != null)
        {
            Vector3 dir = (boss.transform.position - transform.position).normalized;
            float speed = rb.linearVelocity.magnitude;
            rb.linearVelocity = dir * speed * reflectSpeedMult;
        }

        Debug.Log("[ParriableProjectile] 패링! 보스에게 반사");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isParried)
        {
            if (other.CompareTag("Boss"))
            {
                boss?.TakeDamage((int)parriedDamage);
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (player != null)
            {
                player.SuccessParry.AddListener(OnParried);
                player.GetDamage(SetDamage());
                player.SuccessParry.RemoveListener(OnParried);
            }
            if (!isParried) Destroy(gameObject);
        }

        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
            Destroy(gameObject);
    }
}
