using UnityEngine;

public class AttackZone : MonoBehaviour
{
    [SerializeField] private GameObject parent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.tag);

        if (collision.gameObject.CompareTag("Player") && parent.CompareTag("Boss"))
        {
            var damage = parent.GetComponent<IDamageable>().GetDamageAmount();
            collision.gameObject.GetComponent<IDamageable>().OnDamage(damage);
        }
        else if (collision.gameObject.CompareTag("Boss") && parent.CompareTag("Player"))
        {
            var damage = parent.GetComponent<IDamageable>().GetDamageAmount();
            collision.gameObject.GetComponent<IDamageable>().OnDamage(damage);
        }
    }
}