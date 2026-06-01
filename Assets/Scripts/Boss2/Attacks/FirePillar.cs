using UnityEngine;


public class FirePillar : MonoBehaviour, IDamageable
{
    [SerializeField] private float damageMultiplier = 1f;
    private int damage;

    private bool hasHit = false;

    public void Init(int baseAtk) => damage = Mathf.RoundToInt(baseAtk * damageMultiplier);

    public IDamageable.DamageInfo SetDamage() => new() { damage = damage, canParry = false };

    public void GetDamage(IDamageable.DamageInfo damageInfo) { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.GetDamage(SetDamage());
            hasHit = true;
        }
    }

    private void DestroyIt()
    {
        Destroy(gameObject);
    }
}
