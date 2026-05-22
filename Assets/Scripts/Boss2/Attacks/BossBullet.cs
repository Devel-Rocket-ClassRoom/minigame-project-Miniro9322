using UnityEngine;

public class BossBullet : MonoBehaviour, IDamageable
{
    [SerializeField] private int damage = 15;

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
