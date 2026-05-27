using UnityEngine;

public class BossBullet : MonoBehaviour, IDamageable
{
    [SerializeField] private int damage = 15;

    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        if (rb && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            rb.rotation = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        }
    }

    public IDamageable.DamageInfo SetDamage() =>
        new IDamageable.DamageInfo { damage = damage, canParry = false };

    public void GetDamage(IDamageable.DamageInfo damageInfo) { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.GetDamage(SetDamage());
        }

        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
            Destroy(gameObject);
    }
}
