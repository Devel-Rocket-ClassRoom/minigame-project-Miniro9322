using System.Collections.Generic;
using UnityEngine;

public class AttackZone : MonoBehaviour
{
    [SerializeField] private GameObject parent;

    private readonly HashSet<GameObject> hitTargets = new();

    private void OnEnable()
    {
        hitTargets.Clear();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    private void TryDealDamage(Collider2D collision)
    {
        if (hitTargets.Contains(collision.gameObject))
            return;

        if (collision.gameObject.CompareTag("Player") && parent.CompareTag("Boss"))
        {
            var damage = parent.GetComponent<IDamageable>().SetDamage();
            collision.gameObject.GetComponent<IDamageable>().GetDamage(damage);
            hitTargets.Add(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Boss") && parent.CompareTag("Player"))
        {
            var damage = parent.GetComponent<IDamageable>().SetDamage();
            collision.gameObject.GetComponent<IDamageable>().GetDamage(damage);
            hitTargets.Add(collision.gameObject);
        }
    }

    public void Deactivate() => gameObject.SetActive(false);
    public void Activate() => gameObject.SetActive(true);
}