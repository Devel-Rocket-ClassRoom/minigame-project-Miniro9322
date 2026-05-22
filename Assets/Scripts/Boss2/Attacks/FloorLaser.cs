using System.Collections;
using UnityEngine;

public class FloorLaser : MonoBehaviour
    {
        [Header("시각 효과")]
        [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f, 0.4f);
        [SerializeField] private Color activeColor  = new Color(1f, 0.1f, 0.1f, 1f);

        [Header("피해")]
        [SerializeField] private float damagePerSecond = 30f;

        private SpriteRenderer sr;
        private Collider2D     col;
        private float          damageAccum = 0f;

        private void Awake()
        {
            sr  = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
        }

        public void Initialize(float warningDuration, float activeDuration)
        {
            StartCoroutine(LaserSequence(warningDuration, activeDuration));
        }

        private IEnumerator LaserSequence(float warningDuration, float activeDuration)
        {

            if (sr)  sr.color       = warningColor;
            if (col) col.enabled    = false;

            yield return new WaitForSeconds(warningDuration);

            if (sr)  sr.color    = activeColor;
            if (col) col.enabled = true;

            yield return new WaitForSeconds(activeDuration);

            Destroy(gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            damageAccum += Time.deltaTime;
            if (damageAccum < 1f) return;

            damageAccum -= 1f;
            var info = new IDamageable.DamageInfo { damage = (int)damagePerSecond, canParry = false };
            other.GetComponent<IDamageable>()?.GetDamage(info);
        }
}
