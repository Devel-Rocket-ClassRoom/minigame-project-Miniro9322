using UnityEngine;


public class FirePillar : MonoBehaviour, IDamageable
{
    [SerializeField] private int damage = 25;

    public IDamageable.DamageInfo SetDamage() => new() { damage = damage, canParry = false };

    public void GetDamage(IDamageable.DamageInfo damageInfo) { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<IDamageable>()?.GetDamage(SetDamage());
    }
}
