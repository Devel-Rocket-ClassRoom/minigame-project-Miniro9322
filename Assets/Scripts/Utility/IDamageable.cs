using UnityEngine;

public interface IDamageable
{
    struct DamageInfo
    {
        public int damage;
        public bool canParry;
        public Vector2 knockbackDir; // 공격자 → 피격자 방향 (zero 이면 HitState에서 자동 계산)
    }

    DamageInfo SetDamage();

    void GetDamage(DamageInfo damageInfo);
}
