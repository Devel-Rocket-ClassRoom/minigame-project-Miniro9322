using UnityEngine;

public interface IDamageable
{
    int GetDamageAmount();

    void OnDamage(int damage);
}
