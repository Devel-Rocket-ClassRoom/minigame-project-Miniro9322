using UnityEngine;

public interface IDamageable
{
    struct DamageInfo
    {
        public int damage;
        public bool canParry;
    }

    DamageInfo SetDamage();

    void GetDamage(DamageInfo damageInfo);
}
