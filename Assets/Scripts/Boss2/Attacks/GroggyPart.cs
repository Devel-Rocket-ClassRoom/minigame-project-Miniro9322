using UnityEngine;
using UnityEngine.UI;

public class GroggyPart : MonoBehaviour, IDamageable
    {
        [Header("설정")]
        [SerializeField] private float maxHP = 100f;

        [Header("UI (선택)")]
        [SerializeField] private Slider hpBarSlider;

        [Header("시각 효과")]
        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private ParticleSystem destroyEffect;
        [SerializeField] private Color          damagedColor = Color.red;

        private Boss2Controller boss;
        private float           currentHP;
        private SpriteRenderer  sr;
        private Color           originalColor;
        private bool            isDestroyed = false;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr) originalColor = sr.color;
        }

        public void Initialize(Boss2Controller boss)
        {
            this.boss  = boss;
            currentHP  = maxHP;
            UpdateHPBar();
        }

        public IDamageable.DamageInfo SetDamage() => default;

        public void GetDamage(IDamageable.DamageInfo damageInfo)
        {
            TakeDamage(damageInfo.damage);
        }

        public void TakeDamage(int damage)
        {
            if (isDestroyed) return;

            currentHP -= damage;
            currentHP  = Mathf.Max(0f, currentHP);
            UpdateHPBar();

            if (hitEffect) hitEffect.Play();
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

            if (destroyEffect)
                Instantiate(destroyEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }

        private void UpdateHPBar()
        {
            if (hpBarSlider) hpBarSlider.value = currentHP / maxHP;
        }

        private void ResetColor()
        {
            if (sr) sr.color = originalColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("PlayerWeapon")) return;
            var weapon = other.GetComponent<IDamageable>();
            if (weapon != null) GetDamage(weapon.SetDamage());
        }
}
