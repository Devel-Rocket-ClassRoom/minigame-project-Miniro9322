using UnityEngine;

public class AttackZone : MonoBehaviour
{
    [SerializeField] private GameObject parent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && parent.CompareTag("Boss"))
        {
            var damage = parent.GetComponent<IDamageable>().SetDamage();
            collision.gameObject.GetComponent<IDamageable>().GetDamage(damage);
        }
        else if (collision.gameObject.CompareTag("Boss") && parent.CompareTag("Player"))
        {
            var damage = parent.GetComponent<IDamageable>().SetDamage();
            collision.gameObject.GetComponent<IDamageable>().GetDamage(damage);
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
}